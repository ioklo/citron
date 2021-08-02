using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        partial struct StructAnalyzer
        {
            GlobalContext globalContext;
            ITypeContainer typeContainer;
            S.StructDecl structDecl;
            StructTypeValue structTypeValue;

            // Entry
            public static void Analyze(GlobalContext globalContext, ITypeContainer typeContainer, S.StructDecl structDecl, StructTypeValue structTypeValue)
            {
                var structAnalyzer = new StructAnalyzer(globalContext, typeContainer, structDecl, structTypeValue);
                structAnalyzer.AnalyzeStructDecl();
            }           
            
            void AnalyzeStructDecl()
            {
                R.AccessModifier accessModifier;
                switch (structDecl.AccessModifier)
                {
                    case null: accessModifier = R.AccessModifier.Private; break;
                    case S.AccessModifier.Public: accessModifier = R.AccessModifier.Public; break;
                    case S.AccessModifier.Private: 
                        globalContext.AddFatalError(A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault, structDecl);
                        throw new UnreachableCodeException();
                    case S.AccessModifier.Protected: accessModifier = R.AccessModifier.Protected; break;
                    default: throw new UnreachableCodeException();
                }

                // MakeBaseTypes
                static ImmutableArray<R.Path> MakeBaseRTypes(ref S.StructDecl structDecl, GlobalContext globalContext)
                {
                    var builder = ImmutableArray.CreateBuilder<R.Path>(structDecl.BaseTypes.Length);
                    foreach (var baseType in structDecl.BaseTypes)
                    {
                        var typeValue = globalContext.GetTypeValueByTypeExp(baseType);
                        builder.Add(typeValue.GetRPath());
                    }
                    return builder.MoveToImmutable();
                }
                var baseTypes = MakeBaseRTypes(ref structDecl, globalContext);

                // TODO: typeParams
                var memberBuilder = ImmutableArray.CreateBuilder<R.StructDecl.MemberDecl>();
                foreach (var elem in structDecl.Elems)
                {
                    AnalyzeStructDeclElement(memberBuilder, elem);                    
                }               

                BuildAutomaticConstructor(memberBuilder);

                var memberDecls = memberBuilder.ToImmutable();
                var rstructDecl = new R.StructDecl(accessModifier, structDecl.Name, structDecl.TypeParams, baseTypes, memberDecls);

                typeContainer.AddStruct(rstructDecl);
            }

            R.AccessModifier AnalyzeAccessModifier(S.AccessModifier? accessModifier, S.ISyntaxNode nodeForErrorReport)
            {
                switch (accessModifier)
                {
                    case null:
                        return R.AccessModifier.Public;

                    case S.AccessModifier.Private:
                        return R.AccessModifier.Private;

                    case S.AccessModifier.Public:
                        globalContext.AddFatalError(A2401_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault, nodeForErrorReport);
                        break;

                    case S.AccessModifier.Protected:
                        globalContext.AddFatalError(A2402_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed, nodeForErrorReport);
                        break;
                }

                throw new UnreachableCodeException();
            }

            void AnalyzeVarDeclElement(ImmutableArray<R.StructDecl.MemberDecl>.Builder memberBuilder, S.VarStructDeclElement varElem)
            {
                var varTypeValue = globalContext.GetTypeValueByTypeExp(varElem.VarType);
                var rtype = varTypeValue.GetRPath();

                R.AccessModifier accessModifier = AnalyzeAccessModifier(varElem.AccessModifier, varElem);
                memberBuilder.Add(new R.StructDecl.MemberDecl.Var(accessModifier, rtype, varElem.VarNames));
            }

            void AnalyzeConstructorDeclElement(ImmutableArray<R.StructDecl.MemberDecl>.Builder memberBuilder, S.ConstructorStructDeclElement elem)
            {
                R.AccessModifier accessModifier = AnalyzeAccessModifier(elem.AccessModifier, elem);

                // name matches struct
                if (elem.Name != structDecl.Name)
                    globalContext.AddFatalError(A2402_StructDecl_CannotDeclConstructorDifferentWithTypeName, elem);
                
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, 0, elem.Parameters);

                var constructorPath = new R.Path.Nested(structTypeValue.GetRPath_Nested(), R.Name.Constructor.Instance, rparamHash, default);
                var constructorContext = new StructConstructorContext(constructorPath, structTypeValue);
                var localContext = new LocalContext();

                // 새로 만든 컨텍스트에 파라미터 순서대로 추가
                foreach (var param in elem.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                var analyzer = new StmtAndExpAnalyzer(globalContext, constructorContext, localContext);

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(elem.Body);

                var decls = constructorContext.GetDecls();

                memberBuilder.Add(new R.StructDecl.MemberDecl.Constructor(accessModifier, decls, rparamInfos, bodyResult.Stmt));
            }

            void AnalyzeFuncDeclElement(ImmutableArray<R.StructDecl.MemberDecl>.Builder memberBuilder, S.FuncStructDeclElement elem)
            {
                if (elem.FuncDecl.IsSequence)
                    AnalyzeSequenceFuncDeclElement(memberBuilder, elem);
                else
                    AnalyzeNormalFuncDeclElement(memberBuilder, elem);
            }

            void AnalyzeSequenceFuncDeclElement(ImmutableArray<R.StructDecl.MemberDecl>.Builder memberBuilder, S.FuncStructDeclElement elem)
            {
                var funcDecl = elem.FuncDecl;

                // NOTICE: AnalyzeGlobalSequenceFuncDecl와 비슷한 코드
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: global이므로 상위에 type parameter가 없다

                var structPath = structTypeValue.GetRPath_Nested();
                var funcPath = new R.Path.Nested(structPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(structTypeValue, retTypeValue, elem.FuncDecl.IsStatic, true, funcPath);
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
                var seqFuncDecl = new R.StructDecl.MemberDecl.SeqFunc(decls, funcDecl.Name, !elem.FuncDecl.IsStatic, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                memberBuilder.Add(seqFuncDecl);
            }

            void AnalyzeNormalFuncDeclElement(ImmutableArray<R.StructDecl.MemberDecl>.Builder memberBuilder, S.FuncStructDeclElement elem)
            {
                // NOTICE: AnalyzeGlobalNormalFuncDecl와 비슷한 코드

                var funcDecl = elem.FuncDecl;
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(structTypeValue.GetTotalTypeParamCount(), funcDecl.TypeParams);

                var structPath = structTypeValue.GetRPath_Nested();
                var funcPath = new R.Path.Nested(structPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(structTypeValue, retTypeValue, elem.FuncDecl.IsStatic, false, funcPath);
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

                var rfunc = new R.StructDecl.MemberDecl.Func(decls, rname, !elem.FuncDecl.IsStatic, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                memberBuilder.Add(rfunc);
            }

            void AnalyzeStructDeclElement(ImmutableArray<R.StructDecl.MemberDecl>.Builder builder, S.StructDeclElement elem)
            {
                switch (elem)
                {
                    case S.VarStructDeclElement varElem:
                        AnalyzeVarDeclElement(builder, varElem);
                        break;

                    case S.ConstructorStructDeclElement constructorElem:
                        AnalyzeConstructorDeclElement(builder, constructorElem);
                        break;

                    case S.FuncStructDeclElement funcElem:
                        AnalyzeFuncDeclElement(builder, funcElem);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            void BuildAutomaticConstructor(ImmutableArray<R.StructDecl.MemberDecl>.Builder memberBuilder)
            {
                var autoConstructor = structTypeValue.GetAutoConstructor();
                if (autoConstructor == null) return;

                ImmutableArray<R.Decl> decls = default;

                var structPath = structTypeValue.GetRPath_Nested();
                var parameters = autoConstructor.GetParameters();
                var paramBuilder = ImmutableArray.CreateBuilder<R.Param>(parameters.Length);
                var stmtBuilder = ImmutableArray.CreateBuilder<R.Stmt>(parameters.Length);
                foreach(var param in parameters)
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
                    stmtBuilder.Add(new R.ExpStmt(new R.AssignExp(new R.StructMemberLoc(R.ThisLoc.Instance, structMemberPath), new R.LoadExp(new R.LocalVarLoc(rname.Value)))));
                }
                var body = new R.BlockStmt(stmtBuilder.MoveToImmutable());

                memberBuilder.Add(new R.StructDecl.MemberDecl.Constructor(R.AccessModifier.Public, decls, paramBuilder.MoveToImmutable(), body));
            }
        }
    }
}
