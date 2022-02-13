using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Collections;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using Gum.Infra;
using Citron.Analysis;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        partial struct VarDeclElemAnalyzer
        {
            GlobalContext globalContext;
            StmtAndExpAnalyzer stmtAndExpAnalyzer;

            public VarDeclElemAnalyzer(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.stmtAndExpAnalyzer = new StmtAndExpAnalyzer(globalContext, callableContext, localContext);
            }
            
            [AutoConstructor]
            public partial struct VarDeclElementCoreResult
            {
                public R.VarDeclElement Elem { get; }
                public ITypeSymbol TypeSymbol { get; }
            }

            VarDeclElementCoreResult MakeRefVarDeclElementAndTypeCheck(string varName, S.Exp exp, S.ISyntaxNode nodeForErrorReport, ITypeSymbol declType)
            {
                var result = MakeRefVarDeclElement(varName, exp, nodeForErrorReport);

                // 타입 체크, Exact Match
                if (!result.TypeSymbol.Equals(declType))
                    globalContext.AddFatalError(A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType, nodeForErrorReport);

                return result;
            }

            VarDeclElementCoreResult MakeRefVarDeclElement(string varName, S.Exp exp, S.ISyntaxNode nodeForErrorReport)
            {
                var initExpResult = stmtAndExpAnalyzer.AnalyzeExp(exp, ResolveHint.None);

                // location만 허용
                switch (initExpResult)
                {
                    case ExpResult.Loc locResult:
                        {
                            var relem = new R.VarDeclElement.Ref(varName, locResult.Result);
                            return new VarDeclElementCoreResult(relem, locResult.TypeSymbol);
                        }

                    default:
                        globalContext.AddFatalError(A0108_VarDecl_RefNeedLocation, nodeForErrorReport);
                        throw new UnreachableCodeException();
                }
            }

            public VarDeclElementCoreResult AnalyzeVarDeclElement(bool bLocal, S.VarDeclElement elem, bool bRefDeclType, ITypeSymbol declType)
            {
                if (elem.Initializer == null)
                {
                    // local이라면 initializer가 꼭 있어야 합니다
                    if (bLocal)
                        globalContext.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, elem);

                    // ref로 시작했으면 initializer가 꼭 있어야 합니다
                    if (bRefDeclType)
                        globalContext.AddFatalError(A0106_VarDecl_RefDeclNeedInitializer, elem);

                    // var x; 체크
                    if (declType is VarTypeValue)
                        globalContext.AddFatalError(A0101_VarDecl_CantInferVarType, elem);

                    var rtype = declType.MakeRPath();
                    return new VarDeclElementCoreResult(new R.VarDeclElement.NormalDefault(rtype, elem.VarName), declType);
                }
                else
                {
                    // var 처리
                    if (declType is VarTypeValue)
                    {
                        // ref var는 에러
                        if (bRefDeclType)
                            globalContext.AddFatalError(A0107_VarDecl_DontAllowVarWithRef, elem);

                        // var x = ref exp
                        if (elem.Initializer.Value.IsRef)
                        {
                            return MakeRefVarDeclElement(elem.VarName, elem.Initializer.Value.Exp, elem);
                        }
                        else
                        {
                            var initExpResult = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.Initializer.Value.Exp, ResolveHint.None);
                            var rtype = initExpResult.TypeSymbol.MakeRPath();
                            return new VarDeclElementCoreResult(new R.VarDeclElement.Normal(rtype, elem.VarName, initExpResult.Result), initExpResult.TypeSymbol);
                        }
                    }
                    else
                    {
                        if (elem.Initializer.Value.IsRef)
                        {
                            // int x = ref ...
                            if (!bRefDeclType)
                                globalContext.AddFatalError(A0109_VarDecl_ShouldBeRefDeclWithRefInitializer, elem);

                            return MakeRefVarDeclElementAndTypeCheck(elem.VarName, elem.Initializer.Value.Exp, elem, declType);
                        }
                        else
                        {
                            // box<int> bi = box 3;
                            // ref int i = bi; // 이것도 가능 
                            if (bRefDeclType)
                            {
                                if (!elem.Initializer.Value.IsRef)
                                {
                                    // TODO: Box expression에서 ref 가져오기
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    return MakeRefVarDeclElementAndTypeCheck(elem.VarName, elem.Initializer.Value.Exp, elem, declType);
                                }
                            }
                            else
                            {
                                // int i = ref ..., 에러
                                if (elem.Initializer.Value.IsRef)
                                    globalContext.AddFatalError(A0110_VarDecl_RefInitializerUsedOnNonRefVarDecl, elem);                                

                                var initExpResult = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.Initializer.Value.Exp, ResolveHint.Make(declType));
                                var castExpResult = stmtAndExpAnalyzer.CastExp_Exp(initExpResult, declType, elem.Initializer.Value.Exp);

                                var rtype = declType.MakeRPath();
                                return new VarDeclElementCoreResult(new R.VarDeclElement.Normal(rtype, elem.VarName, castExpResult.Result), declType);
                            }
                        }
                    }
                }
            }
        }
    }
}
