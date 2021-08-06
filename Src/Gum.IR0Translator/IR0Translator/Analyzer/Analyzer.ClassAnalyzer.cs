using System;
using System.Linq;

using S = Gum.Syntax;
using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Infra;
using Gum.Collections;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [AutoConstructor]
        partial struct ClassAnalyzer
        {
            GlobalContext globalContext;
            ITypeContainer typeContainer;
            S.ClassDecl classDecl;
            ClassTypeValue classTypeValue;

            // Entry
            public static void Analyze(GlobalContext globalContext, ITypeContainer typeContainer, S.ClassDecl classDecl, ClassTypeValue classTypeValue)
            {
                var classAnalyzer = new ClassAnalyzer(globalContext, typeContainer, classDecl, classTypeValue);
                classAnalyzer.AnalyzeClassDecl();
            }

            void AnalyzeClassDecl()
            {
                R.AccessModifier accessModifier;
                switch (classDecl.AccessModifier)
                {
                    case null: accessModifier = R.AccessModifier.Private; break;
                    case S.AccessModifier.Public: accessModifier = R.AccessModifier.Public; break;
                    case S.AccessModifier.Private:
                        globalContext.AddFatalError(A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault, classDecl);
                        throw new UnreachableCodeException();
                    case S.AccessModifier.Protected: accessModifier = R.AccessModifier.Protected; break;
                    default: throw new UnreachableCodeException();
                }

                // MakeBaseTypes
                static ImmutableArray<R.Path> MakeBaseRTypes(ref S.ClassDecl classDecl, GlobalContext globalContext)
                {
                    var builder = ImmutableArray.CreateBuilder<R.Path>(classDecl.BaseTypes.Length);
                    foreach (var baseType in classDecl.BaseTypes)
                    {
                        var typeValue = globalContext.GetTypeValueByTypeExp(baseType);
                        builder.Add(typeValue.GetRPath());
                    }
                    return builder.MoveToImmutable();
                }
                var baseTypes = MakeBaseRTypes(ref classDecl, globalContext);

                // TODO: typeParams
                var memberBuilder = ImmutableArray.CreateBuilder<R.ClassMemberDecl>();
                foreach (var elem in classDecl.MemberDecls)
                {
                    AnalyzeMemberDecl(memberBuilder, elem);
                }

                BuildAutomaticConstructor(memberBuilder);

                var memberDecls = memberBuilder.ToImmutable();
                var rclassDecl = new R.ClassDecl(accessModifier, classDecl.Name, classDecl.TypeParams, baseTypes, memberDecls);

                typeContainer.AddClass(rclassDecl);
            }

            R.AccessModifier AnalyzeAccessModifier(S.AccessModifier? accessModifier, S.ISyntaxNode nodeForErrorReport)
            {
                switch (accessModifier)
                {
                    case null:
                        return R.AccessModifier.Private;

                    case S.AccessModifier.Public:
                        return R.AccessModifier.Public;

                    case S.AccessModifier.Private:
                        globalContext.AddFatalError(A2501_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault, nodeForErrorReport);
                        break;

                    case S.AccessModifier.Protected:
                        return R.AccessModifier.Protected;
                }

                throw new UnreachableCodeException();
            }

            void AnalyzeMemberVarDecl(ImmutableArray<R.ClassMemberDecl>.Builder memberBuilder, S.ClassMemberVarDecl varDecl)
            {
                var varTypeValue = globalContext.GetTypeValueByTypeExp(varDecl.VarType);
                var rtype = varTypeValue.GetRPath();

                R.AccessModifier accessModifier = AnalyzeAccessModifier(varDecl.AccessModifier, varDecl);
                memberBuilder.Add(new R.ClassMemberVarDecl(accessModifier, rtype, varDecl.VarNames));
            }

            void AnalyzeConstructorDecl(ImmutableArray<R.ClassMemberDecl>.Builder memberBuilder, S.ClassConstructorDecl constructorDecl)
            {
                R.AccessModifier accessModifier = AnalyzeAccessModifier(constructorDecl.AccessModifier, constructorDecl);

                // name matches struct
                if (constructorDecl.Name != classDecl.Name)
                    globalContext.AddFatalError(A2502_ClassDecl_CannotDeclConstructorDifferentWithTypeName, constructorDecl);

                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, 0, constructorDecl.Parameters);

                var constructorPath = new R.Path.Nested(classTypeValue.GetRPath_Nested(), R.Name.Constructor.Instance, rparamHash, default);
                var constructorContext = new ClassConstructorContext(constructorPath, classTypeValue);
                var localContext = new LocalContext();

                // 새로 만든 컨텍스트에 파라미터 순서대로 추가
                foreach (var param in constructorDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                var analyzer = new StmtAndExpAnalyzer(globalContext, constructorContext, localContext);

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(constructorDecl.Body);

                var decls = constructorContext.GetDecls();

                memberBuilder.Add(new R.ClassConstructorDecl(accessModifier, decls, rparamInfos, bodyResult.Stmt));
            }

            void AnalyzeMemberFuncDecl(ImmutableArray<R.ClassMemberDecl>.Builder memberBuilder, S.ClassMemberFuncDecl funcDecl)
            {
                if (funcDecl.IsSequence)
                    AnalyzeSequenceFuncDeclElement(memberBuilder, funcDecl);
                else
                    AnalyzeNormalFuncDeclElement(memberBuilder, funcDecl);
            }

            void AnalyzeSequenceFuncDeclElement(ImmutableArray<R.ClassMemberDecl>.Builder memberBuilder, S.ClassMemberFuncDecl funcDecl)
            {
                // NOTICE: AnalyzeGlobalSequenceFuncDecl와 비슷한 코드
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: global이므로 상위에 type parameter가 없다

                var classPath = classTypeValue.GetRPath_Nested();
                var funcPath = new R.Path.Nested(classPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(classTypeValue, retTypeValue, funcDecl.IsStatic, true, funcPath);
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                if (0 < funcDecl.TypeParams.Length)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

                Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                var retRType = retTypeValue.GetRPath();
                var parameters = funcDecl.Parameters.Select(param => param.Name).ToImmutableArray();

                var decls = funcContext.GetDecls();
                var seqFuncDecl = new R.ClassMemberSeqFuncDecl(decls, funcDecl.Name, !funcDecl.IsStatic, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                memberBuilder.Add(seqFuncDecl);
            }

            void AnalyzeNormalFuncDeclElement(ImmutableArray<R.ClassMemberDecl>.Builder memberBuilder, S.ClassMemberFuncDecl funcDecl)
            {
                // NOTICE: AnalyzeGlobalNormalFuncDecl와 비슷한 코드                
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(classTypeValue.GetTotalTypeParamCount(), funcDecl.TypeParams);

                var structPath = classTypeValue.GetRPath_Nested();
                var funcPath = new R.Path.Nested(structPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(classTypeValue, retTypeValue, funcDecl.IsStatic, false, funcPath);
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

                var decls = funcContext.GetDecls();

                var rfunc = new R.ClassMemberFuncDecl(decls, rname, !funcDecl.IsStatic, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                memberBuilder.Add(rfunc);
            }

            void AnalyzeMemberDecl(ImmutableArray<R.ClassMemberDecl>.Builder builder, S.ClassMemberDecl memberDecl)
            {
                switch (memberDecl)
                {
                    case S.ClassMemberVarDecl varDecl:
                        AnalyzeMemberVarDecl(builder, varDecl);
                        break;

                    case S.ClassConstructorDecl constructorDecl:
                        AnalyzeConstructorDecl(builder, constructorDecl);
                        break;

                    case S.ClassMemberFuncDecl funcDecl:
                        AnalyzeMemberFuncDecl(builder, funcDecl);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            void BuildAutomaticConstructor(ImmutableArray<R.ClassMemberDecl>.Builder memberBuilder)
            {
                var autoConstructor = classTypeValue.GetAutoConstructor();
                if (autoConstructor == null) return;

                ImmutableArray<R.Decl> decls = default;

                var structPath = classTypeValue.GetRPath_Nested();
                var parameters = autoConstructor.GetParameters();
                var paramBuilder = ImmutableArray.CreateBuilder<R.Param>(parameters.Length);
                var stmtBuilder = ImmutableArray.CreateBuilder<R.Stmt>(parameters.Length);
                foreach (var param in parameters)
                {
                    var paramKind = param.Kind switch
                    {
                        M.ParamKind.Normal => R.ParamKind.Normal,
                        M.ParamKind.Ref => R.ParamKind.Ref,
                        M.ParamKind.Params => R.ParamKind.Params,
                        _ => throw new UnreachableCodeException()
                    };

                    var paramTypeValue = globalContext.GetTypeValueByMType(param.Type);
                    var rname = RItemFactory.MakeName(param.Name) as R.Name.Normal;

                    Debug.Assert(rname != null);

                    paramBuilder.Add(new R.Param(paramKind, paramTypeValue.GetRPath(), rname.Value));

                    var structMemberPath = new R.Path.Nested(structPath, rname, R.ParamHash.None, default);
                    stmtBuilder.Add(new R.ExpStmt(new R.AssignExp(new R.ClassMemberLoc(R.ThisLoc.Instance, structMemberPath), new R.LoadExp(new R.LocalVarLoc(rname.Value)))));
                }
                var body = new R.BlockStmt(stmtBuilder.MoveToImmutable());

                memberBuilder.Add(new R.ClassConstructorDecl(R.AccessModifier.Public, decls, paramBuilder.MoveToImmutable(), body));
            }
        }
    }
}
