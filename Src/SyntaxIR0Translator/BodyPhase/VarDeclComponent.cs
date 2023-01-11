using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Pretune;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Infra;
using Citron.Analysis;
using Citron.Symbol;

namespace Citron.Analysis
{   
    struct VarDeclComponent
    {
        struct Result
        {
            public ITypeSymbol Type;            
            public R.VarDeclElement Elem;
            public Result(ITypeSymbol type, R.VarDeclElement elem) { Type = type; Elem = elem; }
        }

        GlobalContext globalContext;
        bool bCheckLocalInitializer;

        public VarDeclComponent(GlobalContext globalContext, bool bCheckLocalInitializer)
        {
            this.globalContext = globalContext;
            this.bCheckLocalInitializer = bCheckLocalInitializer;
        }

        public abstract void OnElemCreated(ITypeSymbol type, string name, S.VarDeclElement selem, R.VarDeclElement elem);
        public abstract void OnCompleted();

        public void AnalyzeVarDecl(S.VarDecl varDecl)
        {
            var declType = globalContext.GetSymbolByTypeExp(varDecl.Type);

            foreach (var elem in varDecl.Elems)
            {
                var result = AnalyzeVarDeclElement(elem, varDecl.IsRef, declType);
                OnElemCreated(result.Type, elem.VarName, elem, result.Elem);
            }

            OnCompleted();
        }

        Result MakeRefVarDeclElementAndTypeCheck(string varName, S.Exp exp, S.ISyntaxNode nodeForErrorReport, IType declType)
        {
            var result = MakeRefVarDeclElement(varName, exp, nodeForErrorReport);

            // 타입 체크, Exact Match
            if (!result.Type.Equals(declType))
                globalContext.AddFatalError(A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType, nodeForErrorReport);

            return result;
        }

        Result MakeRefVarDeclElement(string varName, S.Exp exp, S.ISyntaxNode nodeForErrorReport)
        {
            var initExpResult = stmtAndExpAnalyzer.AnalyzeExp(exp, ResolveHint.None);

            // location만 허용
            switch (initExpResult)
            {
                case ExpResult.Loc locResult:
                    {
                        return new Result(locResult.TypeSymbol, new R.VarDeclElement.Ref(varName, locResult.Result));
                    }

                default:
                    globalContext.AddFatalError(A0108_VarDecl_RefNeedLocation, nodeForErrorReport);
                    throw new UnreachableCodeException();
            }
        }

        public Result AnalyzeVarDeclElement(S.VarDeclElement elem, bool bRefDeclType, IType declType)
        {
            if (elem.Initializer == null)
            {
                // TODO: local이라면 initializer가 꼭 있어야 합니다 => wrong, default constructor가 있으면 된다
                if (bCheckLocalInitializer)
                    globalContext.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, elem);

                // ref로 시작했으면 initializer가 꼭 있어야 합니다
                if (bRefDeclType)
                    globalContext.AddFatalError(A0106_VarDecl_RefDeclNeedInitializer, elem);

                // var x;7 체크
                if (declType is VarType)
                    globalContext.AddFatalError(A0101_VarDecl_CantInferVarType, elem);

                return new Result(declType, new R.VarDeclElement.NormalDefault(declType, elem.VarName));
            }
            else
            {
                // var 처리
                if (declType is VarType)
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
                        var initExp = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.Initializer.Value.Exp, ResolveHint.None);
                        var varDeclElem = new R.VarDeclElement.Normal(initExp.GetTypeSymbol(), elem.VarName, initExp);

                        return new Result(initExp.GetTypeSymbol(), varDeclElem);
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

                            var initExp = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.Initializer.Value.Exp, ResolveHint.Make(declType));
                            var castExp = stmtAndExpAnalyzer.CastExp_Exp(initExp, declType, elem.Initializer.Value.Exp);

                            return new Result(declType, new R.VarDeclElement.Normal(declType, elem.VarName, castExp));
                        }
                    }
                }
            }
        }
    }
}
