using System.Diagnostics;
using System;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using R = Citron.IR0;

using IStmtVisitor = Citron.Syntax.IStmtVisitor<Citron.Collections.ImmutableArray<Citron.IR0.Stmt>>;

using static Citron.Infra.Misc;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

partial struct StmtVisitor : IStmtVisitor
{
    ScopeContext context;

    ImmutableArray<R.Stmt> Stmts(R.Stmt stmt)
    {
        return Arr<R.Stmt>(stmt);
    }

    public StmtVisitor(ScopeContext context)
    {
        this.context = context;
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitVarDecl(S.VarDeclStmt stmt)
    {
        // int a;
        // var x = 

        return VarDeclVisitor.Visit(stmt.VarDecl, context);
    }

    // CommandStmt에 있는 expStringElement를 분석한다
    ImmutableArray<R.Stmt> IStmtVisitor.VisitCommand(S.CommandStmt cmdStmt)
    {
        var builder = ImmutableArray.CreateBuilder<R.StringExp>();        
        foreach (var cmd in cmdStmt.Commands)
        {
            var exp = CoreExpIR0ExpTranslator.Translate(cmd, context, hintType: null);
            builder.Add((R.StringExp)exp);
        }

        return Stmts(new R.CommandStmt(builder.ToImmutable()));
    }
    
    ImmutableArray<R.Stmt> IStmtVisitor.VisitIfTest(S.IfTestStmt ifTestStmt)
    {
        // if (e is Type varName) body 
        // if (var varName = e as Type) body // 변수 선언은 var로 시작해야 하지 않을까
        // IfTestStmt -> IfTestClassStmt, IfTestEnumElemStmt

        var targetResult = ExpResolvedExpTranslator.TranslateAsLoc(ifTestStmt.Exp, context, hintType: null, bWrapExpAsLoc: true);
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
            var bodyStmts = bodyStmtVisitor.VisitEmbeddable(ifTestStmt.Body);

            ImmutableArray<R.Stmt> elseStmts = default;
            if (ifTestStmt.ElseBody != null)
            {
                var elseContext = context.MakeNestedScopeContext();
                var elseStmtVisitor = new StmtVisitor(elseContext);
                elseStmts = elseStmtVisitor.VisitEmbeddable(ifTestStmt.ElseBody);
            }

            var stmt = new R.IfTestEnumElemStmt(targetLoc, enumElemType.Symbol, ifTestStmt.VarName, bodyStmts, elseStmts);
            return Stmts(stmt);
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
        //    throw new UnreachableException();
        //}
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitIf(S.IfStmt ifStmt)
    {
        // 순회
        var condExp = ExpResolvedExpTranslator.TranslateAsExp(ifStmt.Cond, context, context.GetBoolType());
        condExp = BodyMisc.TryCastExp_Exp(condExp, context.GetBoolType());

        if (condExp == null)
            context.AddFatalError(A1001_IfStmt_ConditionShouldBeBool, ifStmt.Cond);

        var bodyContext = context.MakeNestedScopeContext();
        var bodyVisitor = new StmtVisitor(bodyContext);
        var bodyStmts = bodyVisitor.VisitEmbeddable(ifStmt.Body);

        ImmutableArray<R.Stmt> elseStmts = default;
        if (ifStmt.ElseBody != null)
        {
            var elseContext = context.MakeNestedScopeContext();
            var elseVisitor = new StmtVisitor(elseContext);
            elseStmts = elseVisitor.VisitEmbeddable(ifStmt.ElseBody);
        }

        var stmt = new R.IfStmt(condExp, bodyStmts, elseStmts);
        return Stmts(stmt);
    }

    ImmutableArray<R.Stmt> VisitForStmtInitializer(S.ForStmtInitializer forInit)
    {
        switch (forInit)
        {
            case S.VarDeclForStmtInitializer varDeclInit:
                return VarDeclVisitor.Visit(varDeclInit.VarDecl, context);

            case S.ExpForStmtInitializer expInit:
                {
                    var exp = TranslateAsTopLevelExp(expInit.Exp, hintType: null, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall);
                    return Stmts(new R.ExpStmt(exp));
                }

            default:
                throw new NotImplementedException();
        }
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitFor(S.ForStmt forStmt)
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
            initStmts = initVisitor.VisitForStmtInitializer(forStmt.Initializer);
        }

        R.Exp? condExp = null;
        if (forStmt.CondExp != null)
        {
            var boolType = forStmtContext.GetBoolType();
            var rawCondExp = ExpIR0ExpTranslator.Translate(forStmt.CondExp, forStmtContext, boolType);
            condExp = BodyMisc.TryCastExp_Exp(rawCondExp, boolType);

            if (condExp == null)
                context.AddFatalError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp);
        }

        R.Exp? continueExp = null;
        if (forStmt.ContinueExp != null)
            continueExp = initVisitor.TranslateAsTopLevelExp(forStmt.ContinueExp, hintType: null, A1103_ForStmt_ContinueExpShouldBeAssignOrCall);

        var bodyContext = context.MakeLoopNestedScopeContext();
        var bodyVisitor = new StmtVisitor(bodyContext);

        var bodyStmts = bodyVisitor.VisitEmbeddable(forStmt.Body);

        return Stmts(new R.ForStmt(initStmts, condExp, continueExp, bodyStmts));
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitContinue(S.ContinueStmt continueStmt)
    {
        if (!context.IsInLoop())
            context.AddFatalError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);

        return Stmts(new R.ContinueStmt());
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitBreak(S.BreakStmt breakStmt)
    {
        if (!context.IsInLoop())
            context.AddFatalError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt);

        return Stmts(new R.BreakStmt());
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitReturn(S.ReturnStmt returnStmt)
    {
        // seq 함수는 여기서 모두 처리 
        if (context.IsSeqFunc())
        {
            if (returnStmt.Info != null)
                context.AddFatalError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt);

            return Stmts(new R.ReturnStmt(new R.ReturnInfo.None()));
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

            return Stmts(new R.ReturnStmt(new R.ReturnInfo.None()));
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
                var retValueExp = ExpResolvedExpTranslator.TranslateAsExp(returnStmt.Info.Value.Value, context, hintType: null);

                // 리턴값이 안 적혀 있었으므로 적는다
                context.SetReturn(retValueExp.GetExpType());
                return Stmts(new R.ReturnStmt(new R.ReturnInfo.Expression(retValueExp)));
            }
            else
            {
                var funcReturn = context.GetReturn();

                // 생성자였다면
                if (funcReturn == null)
                    throw new NotImplementedException(); // 에러 처리

                // 리턴타입을 힌트로 사용한다
                // 현재 함수 시그니처랑 맞춰서 같은지 확인한다

                var retValueExp = ExpIR0ExpTranslator.Translate(returnStmt.Info.Value.Value, context, funcReturn.Value.Type);
                var castRetValueExp = BodyMisc.TryCastExp_Exp(retValueExp, funcReturn.Value.Type);

                // 캐스트 실패시
                if (castRetValueExp == null)
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                
                return Stmts(new R.ReturnStmt(new R.ReturnInfo.Expression(castRetValueExp)));
            }
        }
    }

    // { }
    ImmutableArray<R.Stmt> IStmtVisitor.VisitBlock(S.BlockStmt blockStmt)
    {
        AnalyzerFatalException? fatalException = null;
        ScopeContext blockContext = context.MakeNestedScopeContext();
        var blockVisitor = new StmtVisitor(blockContext);

        var builder = ImmutableArray.CreateBuilder<R.Stmt>();

        foreach (var stmt in blockStmt.Stmts)
        {
            try
            {
                var innerStmt = stmt.Accept<StmtVisitor, ImmutableArray<R.Stmt>>(ref blockVisitor);
                builder.AddRange(innerStmt.AsEnumerable());
            }
            catch (AnalyzerFatalException e) // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            {
                fatalException = e;
            }
        }

        if (fatalException != null)
            throw fatalException;
        
        return Stmts(new R.BlockStmt(builder.ToImmutable()));
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitBlank(S.BlankStmt stmt)
    {
        return Stmts(new R.BlankStmt());
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitExp(S.ExpStmt expStmt)
    {
        var exp = TranslateAsTopLevelExp(expStmt.Exp, hintType: null, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);
        return Stmts(new R.ExpStmt(exp));
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitTask(S.TaskStmt taskStmt)
    {
        var visitor = new LambdaVisitor(context.GetVoidType(), paramSyntaxes: default, taskStmt.Body, context, taskStmt);
        var (lambda, args) = visitor.Visit();
        
        return Stmts(new R.TaskStmt(lambda, args));
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitAwait(S.AwaitStmt awaitStmt)
    {
        var newContext = context.MakeNestedScopeContext();
        var newVisitor = new StmtVisitor(newContext);
        var stmts = newVisitor.VisitBody(awaitStmt.Body);

        return Stmts(new R.AwaitStmt(stmts));
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitAsync(S.AsyncStmt asyncStmt)
    {
        var visitor = new LambdaVisitor(context.GetVoidType(), default, asyncStmt.Body, context, asyncStmt);
        var(lambda, args) = visitor.Visit();
        return Stmts(new R.AsyncStmt(lambda, args));
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitForeach(S.ForeachStmt foreachStmt)
    {
        // SYNTAX: foreach(var elemVarName in iterator) body
        // IR0: foreach(type elemVarName in iteratorLoc)
        var iterResult = ExpResolvedExpTranslator.TranslateAsLoc(foreachStmt.Iterator, context, hintType: null, bWrapExpAsLoc: true);
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
            var bodyStmts = bodyVisitor.VisitEmbeddable(foreachStmt.Body);
            return Stmts(new R.ForeachStmt(itemType, foreachStmt.VarName, iterLoc, bodyStmts));
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
                var bodyStmts = bodyVisitor.VisitEmbeddable(foreachStmt.Body);

                return Stmts(new R.ForeachStmt(itemType, foreachStmt.VarName, listIterator, bodyStmts));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    // TODO: ref 처리?
    ImmutableArray<R.Stmt> IStmtVisitor.VisitYield(S.YieldStmt yieldStmt)
    {
        if (!context.IsSeqFunc())
            context.AddFatalError(A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt);

        // yield에서는 retType이 명시되는 경우만 있을 것이다
        var callableReturn = context.GetReturn();
        Debug.Assert(callableReturn != null);

        // NOTICE: 리턴 타입을 힌트로 넣었다
        var retValueExp = ExpIR0ExpTranslator.Translate(yieldStmt.Value, context, callableReturn.Value.Type);
        var castRetValueExp = BodyMisc.CastExp_Exp(retValueExp, callableReturn.Value.Type, yieldStmt.Value, context); // 캐스트 실패시 exception발생
        
        return Stmts(new R.YieldStmt(castRetValueExp));
    }

    ImmutableArray<R.Stmt> IStmtVisitor.VisitDirective(S.DirectiveStmt directiveStmt)
    {
        switch (directiveStmt.Name)
        {
            case "static_notnull":
                if (directiveStmt.Args.Length != 1)
                    context.AddFatalError(A2801_StaticNotNullDirective_ShouldHaveOneArgument, directiveStmt);

                var argResult = ExpResolvedExpTranslator.TranslateAsLoc(directiveStmt.Args[0], context, hintType: null, bWrapExpAsLoc: false);
                if (argResult == null)
                    context.AddFatalError(A2802_StaticNotNullDirective_ArgumentMustBeLocation, directiveStmt);

                var (argLoc, _) = argResult.Value;

                return Stmts(new R.DirectiveStmt.StaticNotNull(argLoc));

            default:
                throw new NotImplementedException(); // 인식할 수 없는 directive입니다
        }
    }

    public ImmutableArray<R.Stmt> VisitBody(ImmutableArray<S.Stmt> body)
    {
        var builder = ImmutableArray.CreateBuilder<R.Stmt>();

        foreach (var stmt in body)
        {
            var innerStmts = stmt.Accept<StmtVisitor, ImmutableArray<R.Stmt>>(ref this);
            builder.AddRange(innerStmts.AsEnumerable());
        }

        return builder.ToImmutable();
    }

    public ImmutableArray<R.Stmt> VisitEmbeddable(S.EmbeddableStmt embedStmt)
    {
        // if (...) 'stmt'
        // if (...) '{ stmt... }' 를 받는다
        switch (embedStmt)
        {
            case S.EmbeddableStmt.Single single:
                // TODO: VarDecl은 등장하면 에러를 내도록 한다
                // 지금은 그냥 패스
                return single.Stmt.Accept<StmtVisitor, ImmutableArray<R.Stmt>>(ref this);

            case S.EmbeddableStmt.Multiple multiple:

                var builder = ImmutableArray.CreateBuilder<R.Stmt>();

                foreach (var stmt in multiple.Stmts)
                {
                    var rstmts = stmt.Accept<StmtVisitor, ImmutableArray<R.Stmt>>(ref this);
                    builder.AddRange(rstmts.AsEnumerable());
                }

                return builder.ToImmutable();

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
        var exp = ExpResolvedExpTranslator.TranslateAsExp(expSyntax, context, hintType);

        if (!IsTopLevelExp(exp))
            context.AddFatalError(code, expSyntax);

        return exp;
    }
}