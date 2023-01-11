using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Collections;
using Citron.Infra;
using System;
using System.Xml.Linq;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis
{
    struct LocalVarDeclComponent
    {
        ScopeContext context;

        public LocalVarDeclComponent(ScopeContext context)
        {
            this.context = context;
        }

        public void VisitVarDecl(S.VarDecl varDecl)
        {
            var declType = context.MakeType(varDecl.Type);

            foreach (var elem in varDecl.Elems)
            {
                VisitVarDeclElement(elem, varDecl.IsRef, declType);

                // varDecl.IsRef는 syntax에서 체크한 것이므로, syntax에서 ref가 아니더라도 ref일 수 있으므로 result.Elem으로 검사를 해야한다.
                localContext.AddLocalVarInfo(elem is R.VarDeclElement.Ref, type, new Name.Normal(name));
                elemsBuilder.Add(elem);
            }

            OnCompleted();
        }

        public void VisitVarDeclElement(S.VarDeclElement elem, bool bRefDeclType, IType declType)
        {
            if (context.DoesLocalVarNameExistInScope(elem.VarName))
                context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, selem);

            if (elem.Initializer == null)
            {
                // TODO: local이라면 initializer가 꼭 있어야 합니다 => wrong, default constructor가 있으면 된다
                context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, elem);

                // ref로 시작했으면 initializer가 꼭 있어야 합니다
                if (bRefDeclType)
                    context.AddFatalError(A0106_VarDecl_RefDeclNeedInitializer, elem);

                // var x; 체크
                if (declType is VarType)
                    context.AddFatalError(A0101_VarDecl_CantInferVarType, elem);

                // default constructor 호출
                // context.AddStmt(new R.LocalVarDeclStmt(declType, elem.VarName, ))
                throw new NotImplementedException();
            }
            else
            {
                // var 처리
                if (declType is VarType)
                {
                    if (bRefDeclType)
                        context.AddFatalError(A0107_VarDecl_DontAllowVarWithRef, elem); // ref var x = ref exp; 에러

                    // var x = ref exp
                    if (elem.Initializer.Value.IsRef)
                    {
                        return MakeRefVarDeclElement(elem.VarName, elem.Initializer.Value.Exp, elem);
                    }
                    else
                    {
                        // syntax exp가 하나 이상으로 나타날 수가 있는가

                        var initExp = ExpVisitor.Visit(elem.Initializer.Value.Exp, ResolveHint.None, context);

                        var varDeclStmt = new R.LocalVarDeclStmt(initExp.Make(),

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
                            context.AddFatalError(A0109_VarDecl_ShouldBeRefDeclWithRefInitializer, elem);

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
                                context.AddFatalError(A0110_VarDecl_RefInitializerUsedOnNonRefVarDecl, elem);

                            var initExp = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.Initializer.Value.Exp, ResolveHint.Make(declType));
                            var castExp = stmtAndExpAnalyzer.CastExp_Exp(initExp, declType, elem.Initializer.Value.Exp);

                            return new Result(declType, new R.VarDeclElement.Normal(declType, elem.VarName, castExp));
                        }
                    }
                }
            }
        }

        public override void OnElemCreated(ITypeSymbol type, string name, S.VarDeclElement selem, R.VarDeclElement elem)
        {
            
        }

        public override void OnCompleted()
        {
            // do nothing
        }

        public R.LocalVarDecl Make()
        {
            context.AddStmt(new R.LocalVarDecl(elemsBuilder.ToImmutable()));
        }

        Result MakeRefVarDeclElementAndTypeCheck(string varName, S.Exp exp, S.ISyntaxNode nodeForErrorReport, IType declType)
        {
            var result = MakeRefVarDeclElement(varName, exp, nodeForErrorReport);

            // 타입 체크, Exact Match
            if (!result.Type.Equals(declType))
                context.AddFatalError(A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType, nodeForErrorReport);

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
                    context.AddFatalError(A0108_VarDecl_RefNeedLocation, nodeForErrorReport);
                    throw new UnreachableCodeException();
            }
        }
    }
}