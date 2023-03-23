using System.Diagnostics;
using System;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

partial struct StmtVisitor : S.IStmtVisitor
{
    ScopeContext context;

    public StmtVisitor(ScopeContext context)
    {
        this.context = context;
    }

    public void VisitVarDecl(S.VarDeclStmt stmt)
    {
        // int a;
        // var x = 
        var varDecl = stmt.VarDecl;
        var declType = context.MakeType(varDecl.Type);

        var visitor = new VarDeclElemVisitor(declType, context);

        foreach (var elemSyntax in varDecl.Elems)
            visitor.VisitElem(elemSyntax);
    }

    // CommandStmt에 있는 expStringElement를 분석한다
    public void VisitCommand(S.CommandStmt cmdStmt)
    {
        var builder = ImmutableArray.CreateBuilder<R.StringExp>();        
        foreach (var cmd in cmdStmt.Commands)
        {
            var exp = ExpVisitor.TranslateAsExp(cmd, context, hintType: null);
            builder.Add((R.StringExp)exp);
        }

        context.AddStmt(new R.CommandStmt(builder.ToImmutable()));
    }
    
    public void VisitIfTest(S.IfTestStmt ifTestStmt)
    {
        // if (e is Type varName) body 
        // if (var varName = e as Type) body // 변수 선언은 var로 시작해야 하지 않을까
        // IfTestStmt -> IfTestClassStmt, IfTestEnumElemStmt

        var targetResult = ExpVisitor.TranslateAsLoc(ifTestStmt.Exp, context, hintType: null, bWrapExpAsLoc: true);
        if (targetResult == null)
            throw new NotImplementedException();

        var (targetLoc, targetType) = targetResult.Value;
        var testType = context.MakeType(ifTestStmt.TestType);

        if (testType is EnumElemType enumElemType)
        {
            var enumType = ((ITypeSymbol)enumElemType.Symbol.GetOuter()).MakeType();

            // exact match
            if (!targetType.Equals(enumType))
                context.AddFatalError(A2301_IfTestStmt_CantDowncast, ifTestStmt.Exp);

            var bodyContext = context.MakeNestedScopeContext();
            if (ifTestStmt.VarName != null)
                bodyContext.AddLocalVarInfo(enumElemType, new Name.Normal(ifTestStmt.VarName));

            var bodyStmtVisitor = new StmtVisitor(bodyContext);
            bodyStmtVisitor.VisitEmbeddable(ifTestStmt.Body);
            var bodyStmts = bodyContext.MakeStmts();

            ImmutableArray<R.Stmt> elseStmts = default;
            if (ifTestStmt.ElseBody != null)
            {
                var elseContext = context.MakeNestedScopeContext();
                var elseStmtVisitor = new StmtVisitor(elseContext);
                elseStmtVisitor.VisitEmbeddable(ifTestStmt.ElseBody);
                elseStmts = elseContext.MakeStmts();
            }

            var stmt = new R.IfTestEnumElemStmt(targetLoc, enumElemType.Symbol, ifTestStmt.VarName, bodyStmts, elseStmts);
            context.AddStmt(stmt);
        }
        else
        {
            throw new NotImplementedException();
        }

        // TODO: if (exp is Type v) 구문 추가
        // var condResult = AnalyzeExp(ifStmt.Cond, ResolveHint.None);

        //if (idResult is locResult && locResult.Result is R.LocalVarLoc)
        //{
        //    var testTypeValue = context.GetTypeValueByTypeExp(ifStmt.TestType!);
        //    throw new NotImplementedException();

        //    //if (testTypeValue is EnumElemTypeValue enumElem)
        //    //{
        //    //    return AnalyzeIfTestEnumStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, enumElem, out outStmt);                
        //    //}
        //    //else if (testTypeValue is NormalTypeValue normal)
        //    //{
        //    //    return AnalyzeIfTestClassStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, testTypeValue, out outStmt);
        //    //}
        //    //else
        //    //{
        //    //    context.AddError(A1003_IfStmt_TestTypeShouldBeEnumOrClass, testTypeExp, "if (exp is Test) 구문은 Test부분이 타입이거나 enum값이어야 합니다");
        //    //    return false;
        //    //}
        //}
        //else
        //{
        //    context.AddFatalError(A1001_IfStmt_TestTargetShouldBeLocalVariable, ifStmt.Cond);
        //    throw new UnreachableCodeException();
        //}
    }

    public void VisitIf(S.IfStmt ifStmt)
    {
        // 순회
        var condExp = ExpVisitor.TranslateAsExp(ifStmt.Cond, context, context.GetBoolType());
        condExp = BodyMisc.TryCastExp_Exp(condExp, context.GetBoolType());

        if (condExp == null)
            context.AddFatalError(A1001_IfStmt_ConditionShouldBeBool, ifStmt.Cond);

        var bodyContext = context.MakeNestedScopeContext();
        var bodyVisitor = new StmtVisitor(bodyContext);
        bodyVisitor.VisitEmbeddable(ifStmt.Body);
        var bodyStmts = bodyContext.MakeStmts();

        ImmutableArray<R.Stmt> elseStmts = default;
        if (ifStmt.ElseBody != null)
        {
            var elseContext = context.MakeNestedScopeContext();
            var elseVisitor = new StmtVisitor(elseContext);
            elseVisitor.VisitEmbeddable(ifStmt.ElseBody);
            elseStmts = elseContext.MakeStmts();
        }

        var stmt = new R.IfStmt(condExp, bodyStmts, elseStmts);
        context.AddStmt(stmt);
    }

    public void VisitForStmtInitializer(S.ForStmtInitializer forInit)
    {
        switch (forInit)
        {
            case S.VarDeclForStmtInitializer varDeclInit:
                {
                    var declType = context.MakeType(varDeclInit.VarDecl.Type);
                    var visitor = new VarDeclElemVisitor(declType, context);

                    foreach (var elem in varDeclInit.VarDecl.Elems)
                        visitor.VisitElem(elem);
                    break;
                }

            case S.ExpForStmtInitializer expInit:
                {
                    var exp = TranslateAsTopLevelExp(expInit.Exp, hintType: null, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall);
                    context.AddStmt(new R.ExpStmt(exp));
                    break;
                }

            default:
                throw new NotImplementedException();
        }
    }

    public void VisitFor(S.ForStmt forStmt)
    {
        // for(
        //     int i = 0; <- forStmtContext 
        //     i < 20; <- condition
        //     i++)
        // {
        // 
        // }

        var forStmtContext = context.MakeNestedScopeContext();
        var initVisitor = new StmtVisitor(forStmtContext);

        ImmutableArray<R.Stmt> initStmts = default;
        if (forStmt.Initializer != null)
        {
            initVisitor.VisitForStmtInitializer(forStmt.Initializer);
            initStmts = forStmtContext.MakeStmts(); // vardecl이 될 수도, 다른 ExpStmt일수도 있다
        }

        R.Exp? condExp = null;
        if (forStmt.CondExp != null)
        {
            var boolType = forStmtContext.GetBoolType();
            condExp = ExpVisitor.TranslateAsCastExp(forStmt.CondExp, forStmtContext, hintType: boolType, targetType: boolType);

            if (condExp == null)
                context.AddFatalError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp);
        }

        R.Exp? continueExp = null;
        if (forStmt.ContinueExp != null)
            continueExp = initVisitor.TranslateAsTopLevelExp(forStmt.ContinueExp, hintType: null, A1103_ForStmt_ContinueExpShouldBeAssignOrCall);

        var bodyContext = context.MakeLoopNestedScopeContext();
        var bodyVisitor = new StmtVisitor(bodyContext);

        bodyVisitor.VisitEmbeddable(forStmt.Body);
        var bodyStmts = bodyContext.MakeStmts();

        context.AddStmt(new R.ForStmt(initStmts, condExp, continueExp, bodyStmts));
    }

    public void VisitContinue(S.ContinueStmt continueStmt)
    {
        if (!context.IsInLoop())
            context.AddFatalError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);

        context.AddStmt(new R.ContinueStmt());
    }

    public void VisitBreak(S.BreakStmt breakStmt)
    {
        if (!context.IsInLoop())
            context.AddFatalError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt);

        context.AddStmt(new R.BreakStmt());
    }

    public void VisitReturn(S.ReturnStmt returnStmt)
    {
        // seq 함수는 여기서 모두 처리 
        if (context.IsSeqFunc())
        {
            if (returnStmt.Info != null)
                context.AddFatalError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt);

            context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.None()));
            return;
        }

        // 리턴 값이 없을 경우
        if (returnStmt.Info == null)
        {
            // 리턴값이 이미 정해져 있는 경우
            if (context.IsSetReturn())
            {
                var funcReturn = context.GetReturn();

                // 생성자거나, void 함수가 아니라면 에러
                if (funcReturn != null && !funcReturn.Value.Type.Equals(context.GetVoidType()))
                {
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                }
            }
            else
            {
                // 처음으로 보이는 return; 으로 이 함수는 void 리턴을 확정 한다.
                context.SetReturn(context.GetVoidType());
            }

            context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.None()));
        }
        // DELETE ME: ref가 exp로 편입되면서, 따로 처리할 필요가 없어졌다
        //else if (returnStmt.Info.Value.IsRef) // return ref i; 또는  () => ref i;, 
        //{
        //    if (!context.IsSetReturn())
        //    {
        //        // 아직 리턴타입이 안정해졌다면, 힌트타입 없이 분석
        //        var returnResult = ExpVisitor.TranslateAsLoc(returnStmt.Info.Value.Value, context, hintType: null, bWrapExpAsLoc: false);
        //        if (returnResult == null)
        //            context.AddFatalError(A1203_ReturnStmt_RefTargetIsNotLocation, returnStmt.Info.Value.Value);

        //        var (returnLoc, returnType) = returnResult.Value;
        //        context.SetReturn(true, returnType);
        //        context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.Ref(returnLoc)));
        //    }
        //    else
        //    {   
        //        var funcReturn = context.GetReturn();

        //        if (funcReturn == null || !funcReturn.Value.IsRef)
        //            context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);

        //        // ref return은 힌트를 쓰지 않는다
        //        var returnResult = ExpVisitor.TranslateAsLoc(returnStmt.Info.Value.Value, context, hintType: null, bWrapExpAsLoc: false);
        //        if (returnResult == null)
        //            context.AddFatalError(A1203_ReturnStmt_RefTargetIsNotLocation, returnStmt.Info.Value.Value);

        //        var (returnLoc, returnType) = returnResult.Value;

        //        // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
        //        // ExactMatch여야 한다
        //        if (!returnType.Equals(funcReturn.Value))
        //            context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);

        //        context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.Ref(returnLoc)));
        //    }
        //}
        else // 일반적인 리턴 값일 경우
        {
            // 아직 리턴값이 정해지지 않은 경우
            if (!context.IsSetReturn())
            {
                // 힌트타입 없이 분석
                var retValueExp = ExpVisitor.TranslateAsExp(returnStmt.Info.Value.Value, context, hintType: null);

                // 리턴값이 안 적혀 있었으므로 적는다
                context.SetReturn(retValueExp.GetExpType());
                context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.Expression(retValueExp)));
            }
            else
            {
                var funcReturn = context.GetReturn();

                // 생성자였다면
                if (funcReturn == null)
                    throw new NotImplementedException(); // 에러 처리

                // 리턴타입을 힌트로 사용한다
                // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                var retValueExp = ExpVisitor.TranslateAsCastExp(returnStmt.Info.Value.Value, context, hintType: funcReturn.Value.Type, targetType: funcReturn.Value.Type);
                if (retValueExp == null)
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                
                context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.Expression(retValueExp)));
            }
        }
    }

    // { }
    public void VisitBlock(S.BlockStmt blockStmt)
    {
        AnalyzerFatalException? fatalException = null;
        ScopeContext blockContext = context.MakeNestedScopeContext();
        var blockVisitor = new StmtVisitor(blockContext);

        foreach (var stmt in blockStmt.Stmts)
        {
            try
            {
                stmt.Accept(ref blockVisitor);
            }
            catch (AnalyzerFatalException e) // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            {
                fatalException = e;
            }
        }

        if (fatalException != null)
            throw fatalException;

        var stmts = blockContext.MakeStmts();
        context.AddStmt(new R.BlockStmt(stmts));
    }

    public void VisitBlank(S.BlankStmt stmt)
    {
        context.AddStmt(new R.BlankStmt());
    }

    public void VisitExp(S.ExpStmt expStmt)
    {
        var exp = TranslateAsTopLevelExp(expStmt.Exp, hintType: null, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);
        context.AddStmt(new R.ExpStmt(exp));
    }

    public void VisitTask(S.TaskStmt taskStmt)
    {
        var visitor = new LambdaVisitor(context.GetVoidType(), paramSyntaxes: default, taskStmt.Body, context, taskStmt);
        var (lambda, args) = visitor.Visit();
        
        context.AddStmt(new R.TaskStmt(lambda, args));
    }

    public void VisitAwait(S.AwaitStmt awaitStmt)
    {
        var newContext = context.MakeNestedScopeContext();
        var newVisitor = new StmtVisitor(newContext);
        newVisitor.VisitBody(awaitStmt.Body);

        context.AddStmt(new R.AwaitStmt(newContext.MakeStmts()));
    }

    public void VisitAsync(S.AsyncStmt asyncStmt)
    {
        var visitor = new LambdaVisitor(context.GetVoidType(), default, asyncStmt.Body, context, asyncStmt);
        var(lambda, args) = visitor.Visit();
        context.AddStmt(new R.AsyncStmt(lambda, args));
    }

    public void VisitForeach(S.ForeachStmt foreachStmt)
    {
        // SYNTAX: foreach(var elemVarName in iterator) body
        // IR0: foreach(type elemVarName in iteratorLoc)
        var iterResult = ExpVisitor.TranslateAsLoc(foreachStmt.Iterator, context, hintType: null, bWrapExpAsLoc: true);
        if (iterResult == null)
            throw new NotImplementedException(); // iterator가 location으로 나오지 않습니다

        var (iterLoc, iterType) = iterResult.Value;

        // 먼저, iteratorResult가 anonymous_seq타입인지 확인한다
        if (context.IsSeqType(iterType, out var seqItemType)) // seq<>
        {
            var itemType = context.MakeType(foreachStmt.Type);

            if (itemType is VarType) // var type처리
            {
                itemType = seqItemType;
            }
            else
            {
                // 완전히 같은지 체크
                if (itemType.Equals(seqItemType))
                {
                    // relemType = elemType.MakeRPath();
                }
                else
                {
                    // 다르다면 seq_cast 시도
                    // foreach(type elemName in seq_cast(type, iteratorLoc))
                    throw new NotImplementedException();
                }
            }

            // 루프 컨텍스트를 하나 열고
            var bodyContext = context.MakeLoopNestedScopeContext();

            // TODO: ref foreach
            if (foreachStmt.IsRef)
                throw new NotImplementedException();

            // 루프 컨텍스트에 로컬을 하나 추가하고
            bodyContext.AddLocalVarInfo(itemType, new Name.Normal(foreachStmt.VarName));

            var bodyVisitor = new StmtVisitor(bodyContext);
            // 본문 분석
            bodyVisitor.VisitEmbeddable(foreachStmt.Body);

            var body = context.MakeStmts();
            context.AddStmt(new R.ForeachStmt(itemType, foreachStmt.VarName, iterLoc, body));
        }
        else
        {
            // 축약형 처리
            // anonymous_seq를 리턴하는 식으로 변경해준다
            // foreach(var i in [1, 2, 3, 4]) => foreach(var i in [1, 2, 3, 4].GetEnumerator())
            if (context.IsListType(iterType, out var listItemType))
            {
                var itemType = context.MakeType(foreachStmt.Type);

                if (itemType is VarType) // var type처리
                {
                    itemType = listItemType;
                }
                else
                {
                    // 완전히 같은지 체크
                    if (itemType.Equals(listItemType))
                    {
                        // relemType = elemType.MakeRPath();
                    }
                    else
                    {
                        // 다르다면 seq_cast 시도
                        // foreach(type elemName in seq_cast(type, iteratorLoc))
                        throw new NotImplementedException();
                    }
                }

                var listIterator = new R.TempLoc(new R.ListIteratorExp(iterLoc, context.GetListIterType(listItemType)));

                // 루프 컨텍스트를 하나 열고
                var bodyContext = context.MakeLoopNestedScopeContext();

                // TODO: ref foreach
                if (foreachStmt.IsRef)
                    throw new NotImplementedException();

                // 루프 컨텍스트에 로컬을 하나 추가하고
                bodyContext.AddLocalVarInfo(itemType, new Name.Normal(foreachStmt.VarName));

                // 본문 분석
                var bodyVisitor = new StmtVisitor(bodyContext);
                bodyVisitor.VisitEmbeddable(foreachStmt.Body);
                var stmts = bodyContext.MakeStmts();

                context.AddStmt(new R.ForeachStmt(itemType, foreachStmt.VarName, listIterator, stmts));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    // TODO: ref 처리?
    public void VisitYield(S.YieldStmt yieldStmt)
    {
        if (!context.IsSeqFunc())
            context.AddFatalError(A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt);

        // yield에서는 retType이 명시되는 경우만 있을 것이다
        var callableReturn = context.GetReturn();
        Debug.Assert(callableReturn != null);

        // NOTICE: 리턴 타입을 힌트로 넣었다
        var retValueExp = ExpVisitor.TranslateAsCastExp(yieldStmt.Value, context, hintType: callableReturn.Value.Type, targetType: callableReturn.Value.Type);
        if (retValueExp == null) // 캐스트 실패
            throw new NotImplementedException();

        context.AddStmt(new R.YieldStmt(retValueExp));
    }

    public void VisitDirective(S.DirectiveStmt directiveStmt)
    {
        switch (directiveStmt.Name)
        {
            case "static_notnull":
                if (directiveStmt.Args.Length != 1)
                    context.AddFatalError(A2801_StaticNotNullDirective_ShouldHaveOneArgument, directiveStmt);

                var argResult = ExpVisitor.TranslateAsLoc(directiveStmt.Args[0], context, hintType: null, bWrapExpAsLoc: false);
                if (argResult == null)
                    context.AddFatalError(A2802_StaticNotNullDirective_ArgumentMustBeLocation, directiveStmt);

                var (argLoc, _) = argResult.Value;

                context.AddStmt(new R.DirectiveStmt.StaticNotNull(argLoc));
                break;

            default:
                throw new NotImplementedException(); // 인식할 수 없는 directive입니다
        }
    }

    public void VisitBody(ImmutableArray<S.Stmt> body)
    {
        foreach (var stmt in body)
        {
            stmt.Accept(ref this);
        }
    }

    public void VisitEmbeddable(S.EmbeddableStmt embedStmt)
    {
        // if (...) 'stmt'
        // if (...) '{ stmt... }' 를 받는다
        switch (embedStmt)
        {
            case S.EmbeddableStmt.Single single:
                // TODO: VarDecl은 등장하면 에러를 내도록 한다
                // 지금은 그냥 패스
                single.Stmt.Accept(ref this);
                return;

            case S.EmbeddableStmt.Multiple multiple:
                foreach (var stmt in multiple.Stmts)
                    stmt.Accept(ref this);
                return;

            default:
                throw new NotImplementedException();
        }
    }

    

    bool IsTopLevelExp(R.Exp exp)
    {
        switch (exp)
        {
            case R.CallInternalUnaryAssignOperatorExp:
            case R.AssignExp:
            case R.CallGlobalFuncExp:
            case R.CallClassMemberFuncExp:
            case R.CallStructMemberFuncExp:
            case R.CallValueExp:
                return true;

            default:
                return false;
        }
    }

    public R.Exp TranslateAsTopLevelExp(S.Exp expSyntax, IType? hintType, SyntaxAnalysisErrorCode code)
    {
        var exp = ExpVisitor.TranslateAsExp(expSyntax, context, hintType);

        if (!IsTopLevelExp(exp))
            context.AddFatalError(code, expSyntax);

        return exp;
    }
}