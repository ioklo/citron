using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Gum.Syntax;
using R = Gum.IR0;
using Gum.Infra;
using Gum.Collections;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [AutoConstructor]
        partial struct StructAnalyzer
        {
            GlobalContext globalContext;            
            S.StructDecl structDecl;
            StructTypeValue structTypeValue;
            
            // Entry
            public R.StructDecl AnalyzeStructDecl()
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
                var builder = ImmutableArray.CreateBuilder<R.StructDecl.MemberDecl>(structDecl.Elems.Length);
                foreach (var elem in structDecl.Elems)
                {
                    var memberDecl = AnalyzeStructDeclElement(elem);
                    builder.Add(memberDecl);
                }

                var memberDecls = builder.MoveToImmutable();
                return new R.StructDecl(accessModifier, structDecl.Name, structDecl.TypeParams, baseTypes, memberDecls);
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
                        globalContext.AddFatalError(AnalyzeErrorCode.A2401_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault, nodeForErrorReport);
                        break;

                    case S.AccessModifier.Protected:
                        globalContext.AddFatalError(AnalyzeErrorCode.A2402_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed, nodeForErrorReport);
                        break;
                }

                throw new UnreachableCodeException();
            }

            R.StructDecl.MemberDecl.Var AnalyzeVarDeclElement(S.VarStructDeclElement varElem)
            {
                var varTypeValue = globalContext.GetTypeValueByTypeExp(varElem.VarType);
                var rtype = varTypeValue.GetRPath();

                R.AccessModifier accessModifier = AnalyzeAccessModifier(varElem.AccessModifier, varElem);
                return new R.StructDecl.MemberDecl.Var(accessModifier, rtype, varElem.VarNames);
            }

            R.StructDecl.MemberDecl.Constructor AnalyzeConstructorDeclElement(S.ConstructorStructDeclElement elem)
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

                return new R.StructDecl.MemberDecl.Constructor(accessModifier, decls, rparamInfos, bodyResult.Stmt);
            }
            
            R.StructDecl.MemberDecl AnalyzeStructDeclElement(S.StructDeclElement elem)
            {
                switch (elem)
                {
                    case S.VarStructDeclElement varElem:
                        return AnalyzeVarDeclElement(varElem);

                    case S.ConstructorStructDeclElement constructorElem:
                        return AnalyzeConstructorDeclElement(constructorElem);

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
