using System.Diagnostics;
using System;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

partial struct StmtVisitor
{
    ScopeContext context;

    public StmtVisitor(ScopeContext context)
    {
        this.context = context;
    }

    // CommandStmt에 있는 expStringElement를 분석한다
    void VisitCommandStmt(S.CommandStmt cmdStmt)
    {
        var builder = ImmutableArray.CreateBuilder<R.StringExp>();        
        foreach (var cmd in cmdStmt.Commands)
        {
            var exp = ExpVisitor.TranslateAsExp(cmd, context, hintType: null);
            builder.Add((R.StringExp)exp);
        }

        context.AddStmt(new R.CommandStmt(builder.ToImmutable()));
    }
    
    // int a;
    // var x = 
    void VisitVarDeclStmt(S.VarDeclStmt stmt)
    {
        var varDecl = stmt.VarDecl;
        var declType = context.MakeType(varDecl.Type);

        var visitor = new VarDeclElemVisitor(varDecl.IsRef, declType, context);

        foreach (var elemSyntax in varDecl.Elems)
            visitor.VisitElem(elemSyntax);
    }

    //bool AnalyzeIfTestEnumStmt(
    //    IdentifierInfo varIdInfo,
    //    Exp target, 
    //    S.Stmt thenBody,
    //    S.Stmt? elseBody,
    //    TypeValue targetType, EnumElemTypeValue enumElem, [NotNullWhen(true)] out Stmt? outStmt)
    //{
    //    bool bResult = true;
    //    Stmt? ir0ThenBody = null;
    //    Stmt? ir0ElseBody = null;

    //    context.ExecInLocalScope(() =>
    //    {
    //        context.AddOverrideVarInfo(varIdInfo.StorageInfo, targetType);

    //        if (!AnalyzeStmt(thenBody, out ir0ThenBody))
    //            bResult = false;

    //        if (elseBody != null)
    //            if (!AnalyzeStmt(elseBody, out ir0ElseBody))
    //                bResult = false;
    //    });

    //    if (bResult != false)
    //    {
    //        Debug.Assert(ir0ThenBody != null);

    //        var targetTypeId = context.GetTypeId(targetType);
    //        outStmt = new IfTestEnumStmt(new ExpInfo(target, targetTypeId), enumElem.Name, ir0ThenBody, ir0ElseBody);
    //        return true;
    //    }
    //    else
    //    {
    //        outStmt = null;
    //        return false;
    //    }
    //}

    //bool AnalyzeIfTestClassStmt(
    //    IdentifierInfo.Var varIdInfo,
    //    Exp target,
    //    S.Stmt thenBody,
    //    S.Stmt? elseBody,
    //    TypeValue targetType,
    //    TypeValue testType,
    //    Context context, 
    //    [NotNullWhen(true)] out Stmt? outStmt)
    //{
    //    bool bResult = true;
    //    Stmt? ir0ThenBody = null;
    //    Stmt? ir0ElseBody = null;

    //    context.ExecInLocalScope(() =>
    //    {
    //        context.AddOverrideVarInfo(varIdInfo.StorageInfo, testType);

    //        if (!AnalyzeStmt(thenBody, out ir0ThenBody))
    //            bResult = false;

    //        if (elseBody != null)
    //            if (!AnalyzeStmt(elseBody, out ir0ElseBody))
    //                bResult = false;
    //    });

    //    if (bResult)
    //    {
    //        Debug.Assert(ir0ThenBody != null);
    //        var targetTypeId = context.GetTypeId(targetType);
    //        var testTypeId = context.GetTypeId(testType);
    //        outStmt = new IfTestClassStmt(new ExpInfo(target, targetTypeId), testTypeId, ir0ThenBody, ir0ElseBody);
    //        return true;
    //    }
    //    else
    //    {
    //        outStmt = null;
    //        return false;
    //    }
    //}
    // 
    void VisitIfTestStmt(S.IfTestStmt ifTestStmt)
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
                bodyContext.AddLocalVarInfo(true, enumElemType, new Name.Normal(ifTestStmt.VarName));

            var bodyStmtVisitor = new StmtVisitor(bodyContext);
            bodyStmtVisitor.VisitEmbeddableStmt(ifTestStmt.Body);
            var bodyStmt = bodyContext.MakeSingleStmt();

            R.Stmt? elseStmt = null;
            if (ifTestStmt.ElseBody != null)
            {
                var elseContext = context.MakeNestedScopeContext();
                var elseStmtVisitor = new StmtVisitor(elseContext);
                elseStmtVisitor.VisitEmbeddableStmt(ifTestStmt.ElseBody);
                elseStmt = elseContext.MakeSingleStmt();
            }

            var stmt = new R.IfTestEnumElemStmt(targetLoc, enumElemType.Symbol, ifTestStmt.VarName, bodyStmt, elseStmt);
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

    // if (...) 'stmt'
    // if (...) '{ stmt... }' 를 받는다
    public void VisitEmbeddableStmt(S.EmbeddableStmt embedStmt)
    {   
        switch(embedStmt)
        {
            case S.EmbeddableStmt.Single single:
                // TODO: VarDecl은 등장하면 에러를 내도록 한다
                // 지금은 그냥 패스
                VisitStmt(single.Stmt);
                return;

            case S.EmbeddableStmt.Multiple multiple:
                foreach (var stmt in multiple.Stmts)
                    VisitStmt(stmt);
                return;

            default:
                throw new NotImplementedException();
        }
    }

    public void VisitBody(ImmutableArray<S.Stmt> body)
    {   
        foreach (var stmt in body)
        {
            VisitStmt(stmt);
        }
    }

    void VisitIfStmt(S.IfStmt ifStmt)
    {
        // 순회
        var condExp = ExpVisitor.TranslateAsExp(ifStmt.Cond, context, context.GetBoolType());
        condExp = BodyMisc.TryCastExp_Exp(condExp, context.GetBoolType());

        if (condExp == null)
            context.AddFatalError(A1001_IfStmt_ConditionShouldBeBool, ifStmt.Cond);

        var bodyContext = context.MakeNestedScopeContext();
        var bodyVisitor = new StmtVisitor(bodyContext);
        bodyVisitor.VisitEmbeddableStmt(ifStmt.Body);
        var bodyStmt = bodyContext.MakeSingleStmt();

        R.Stmt? elseStmt = null;
        if (ifStmt.ElseBody != null)
        {
            var elseContext = context.MakeNestedScopeContext();
            var elseVisitor = new StmtVisitor(elseContext);
            elseVisitor.VisitEmbeddableStmt(ifStmt.ElseBody);
            elseStmt = elseContext.MakeSingleStmt();
        }

        var stmt = new R.IfStmt(condExp, bodyStmt, elseStmt);
        context.AddStmt(stmt);
    }

    void VisitForStmtInitializer(S.ForStmtInitializer forInit)
    {
        switch (forInit)
        {
            case S.VarDeclForStmtInitializer varDeclInit:
                {
                    var declType = context.MakeType(varDeclInit.VarDecl.Type);
                    var visitor = new VarDeclElemVisitor(varDeclInit.VarDecl.IsRef, declType, context);

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

    void VisitForStmt(S.ForStmt forStmt)
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

        bodyVisitor.VisitEmbeddableStmt(forStmt.Body);
        var bodyStmt = bodyContext.MakeSingleStmt();

        context.AddStmt(new R.ForStmt(initStmts, condExp, continueExp, bodyStmt));
    }

    void VisitContinueStmt(S.ContinueStmt continueStmt)
    {
        if (!context.IsInLoop())
            context.AddFatalError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);

        context.AddStmt(R.ContinueStmt.Instance);
    }

    void VisitBreakStmt(S.BreakStmt breakStmt)
    {
        if (!context.IsInLoop())
            context.AddFatalError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt);

        context.AddStmt(R.BreakStmt.Instance);
    }

    void VisitReturnStmt(S.ReturnStmt returnStmt)
    {
        // seq 함수는 여기서 모두 처리 
        if (context.IsSeqFunc())
        {
            if (returnStmt.Info != null)
                context.AddFatalError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt);

            context.AddStmt(new R.ReturnStmt(R.ReturnInfo.None.Instance));
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
                if (funcReturn != null && !context.IsVoidType(funcReturn.Value.Type))
                {
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                }
            }
            else
            {
                // 처음으로 보이는 return; 으로 이 함수는 void 리턴을 확정 한다.
                context.SetReturn(false, context.GetVoidType());
            }

            context.AddStmt(new R.ReturnStmt(R.ReturnInfo.None.Instance));
        }
        else if (returnStmt.Info.Value.IsRef) // return ref i; 또는  () => ref i;
        {
            if (!context.IsSetReturn())
            {
                // 아직 리턴타입이 안정해졌다면, 힌트타입 없이 분석
                var returnResult = ExpVisitor.TranslateAsLoc(returnStmt.Info.Value.Value, context, hintType: null, bWrapExpAsLoc: false);
                if (returnResult == null)
                    context.AddFatalError(A1203_ReturnStmt_RefTargetIsNotLocation, returnStmt.Info.Value.Value);

                var (returnLoc, returnType) = returnResult.Value;
                context.SetReturn(true, returnType);
                context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.Ref(returnLoc)));
            }
            else
            {   
                var funcReturn = context.GetReturn();

                if (funcReturn == null || !funcReturn.Value.IsRef)
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);

                // ref return은 힌트를 쓰지 않는다
                var returnResult = ExpVisitor.TranslateAsLoc(returnStmt.Info.Value.Value, context, hintType: null, bWrapExpAsLoc: false);
                if (returnResult == null)
                    context.AddFatalError(A1203_ReturnStmt_RefTargetIsNotLocation, returnStmt.Info.Value.Value);

                var (returnLoc, returnType) = returnResult.Value;

                // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                // ExactMatch여야 한다
                if (!returnType.Equals(funcReturn.Value))
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);

                context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.Ref(returnLoc)));
            }
        }
        else // 일반적인 리턴 값일 경우
        {
            // 아직 리턴값이 정해지지 않은 경우
            if (!context.IsSetReturn())
            {
                // 힌트타입 없이 분석
                var retValueExp = ExpVisitor.TranslateAsExp(returnStmt.Info.Value.Value, context, hintType: null);

                // 리턴값이 안 적혀 있었으므로 적는다
                context.SetReturn(false, retValueExp.GetExpType());
                context.AddStmt(new R.ReturnStmt(new R.ReturnInfo.Expression(retValueExp)));
            }
            else
            {
                var funcReturn = context.GetReturn();

                // 생성자였다면
                if (funcReturn == null)
                    throw new NotImplementedException(); // 에러 처리

                // TODO: ref 고려
                if (funcReturn.Value.IsRef)
                    throw new NotImplementedException();

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
    void VisitBlockStmt(S.BlockStmt blockStmt)
    {
        AnalyzerFatalException? fatalException = null;
        ScopeContext blockContext = context.MakeNestedScopeContext();
        var blockVisitor = new StmtVisitor(blockContext);

        foreach (var stmt in blockStmt.Stmts)
        {
            try
            {
                blockVisitor.VisitStmt(stmt);
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

    void VisitBlankStmt()
    {
        context.AddStmt(new R.BlankStmt());
    }

    void VisitExpStmt(S.ExpStmt expStmt)
    {
        var exp = TranslateAsTopLevelExp(expStmt.Exp, hintType: null, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);
        context.AddStmt(new R.ExpStmt(exp));
    }

    void VisitTaskStmt(S.TaskStmt taskStmt)
    {
        var visitor = new LambdaVisitor(context.GetVoidType(), paramSyntaxes: default, taskStmt.Body, context, taskStmt);
        var (lambda, args, body) = visitor.Visit();

        context.AddStmt(new R.TaskStmt(lambda, args, body));
    }

    void VisitAwaitStmt(S.AwaitStmt awaitStmt)
    {
        var newContext = context.MakeNestedScopeContext();
        var newVisitor = new StmtVisitor(newContext);
        newVisitor.VisitBody(awaitStmt.Body);

        context.AddStmt(new R.AwaitStmt(newContext.MakeStmts()));
    }

    void VisitAsyncStmt(S.AsyncStmt asyncStmt)
    {
        var visitor = new LambdaVisitor(context.GetVoidType(), default, asyncStmt.Body, context, asyncStmt);
        var(lambda, args, body) = visitor.Visit();
        context.AddStmt(new R.AsyncStmt(lambda, args, body));
    }

    void VisitForeachStmt(S.ForeachStmt foreachStmt)
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
            bodyContext.AddLocalVarInfo(false, itemType, new Name.Normal(foreachStmt.VarName));

            var bodyVisitor = new StmtVisitor(bodyContext);
            // 본문 분석
            bodyVisitor.VisitEmbeddableStmt(foreachStmt.Body);

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
                bodyContext.AddLocalVarInfo(false, itemType, new Name.Normal(foreachStmt.VarName));

                // 본문 분석
                var bodyVisitor = new StmtVisitor(bodyContext);
                bodyVisitor.VisitEmbeddableStmt(foreachStmt.Body);
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
    void VisitYieldStmt(S.YieldStmt yieldStmt)
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

    void VisitDirectiveStmt(S.DirectiveStmt directiveStmt)
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

    public void VisitStmt(S.Stmt stmt)
    {
        switch (stmt)
        {
            case S.VarDeclStmt varDeclStmt: VisitVarDeclStmt(varDeclStmt); return;
            case S.CommandStmt cmdStmt: VisitCommandStmt(cmdStmt); return;
            case S.IfStmt ifStmt: VisitIfStmt(ifStmt); return;
            case S.IfTestStmt ifTestStmt: VisitIfTestStmt(ifTestStmt); return;
            case S.ForStmt forStmt: VisitForStmt(forStmt); return;
            case S.ContinueStmt continueStmt: VisitContinueStmt(continueStmt); return;
            case S.BreakStmt breakStmt: VisitBreakStmt(breakStmt); return;
            case S.ReturnStmt returnStmt: VisitReturnStmt(returnStmt); return;
            case S.BlockStmt blockStmt: VisitBlockStmt(blockStmt); return;
            case S.BlankStmt blankStmt: VisitBlankStmt(); return;
            case S.ExpStmt expStmt: VisitExpStmt(expStmt); return;
            case S.TaskStmt taskStmt: VisitTaskStmt(taskStmt); return;
            case S.AwaitStmt awaitStmt: VisitAwaitStmt(awaitStmt); return;
            case S.AsyncStmt asyncStmt: VisitAsyncStmt(asyncStmt); return;
            case S.ForeachStmt foreachStmt: VisitForeachStmt(foreachStmt); return;
            case S.YieldStmt yieldStmt: VisitYieldStmt(yieldStmt); return;
            case S.DirectiveStmt directiveStmt: VisitDirectiveStmt(directiveStmt); return;
            default: throw new UnreachableCodeException();
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

    R.Exp TranslateAsTopLevelExp(S.Exp expSyntax, IType? hintType, SyntaxAnalysisErrorCode code)
    {
        var exp = ExpVisitor.TranslateAsExp(expSyntax, context, hintType);

        if (!IsTopLevelExp(exp))
            context.AddFatalError(code, expSyntax);

        return exp;
    }
}