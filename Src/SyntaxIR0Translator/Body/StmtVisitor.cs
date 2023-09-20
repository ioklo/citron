using System.Diagnostics;
using System;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using R = Citron.IR0;

using IStmtVisitor = Citron.Syntax.IStmtVisitor<Citron.Analysis.TranslationResult<Citron.Collections.ImmutableArray<Citron.IR0.Stmt>>>;

using static Citron.Infra.Misc;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

partial struct StmtVisitor : IStmtVisitor
{
    ScopeContext context;

    static TranslationResult<ImmutableArray<R.Stmt>> Stmts(R.Stmt stmt)
    {
        return TranslationResult.Valid(Arr(stmt));
    }

    static TranslationResult<ImmutableArray<R.Stmt>> Valid(ImmutableArray<R.Stmt> stmts)
    {
        return TranslationResult.Valid(stmts);
    }

    static TranslationResult<ImmutableArray<R.Stmt>> Error()
    {
        return TranslationResult.Error<ImmutableArray<R.Stmt>>();
    }

    public static TranslationResult<ImmutableArray<R.Stmt>> Translate(S.Stmt stmt, ScopeContext context)
    {
        var translator = new StmtVisitor { context = context };
        return stmt.Accept<StmtVisitor, TranslationResult<ImmutableArray<R.Stmt>>>(ref translator);
    }

    public static TranslationResult<ImmutableArray<R.Stmt>> TranslateBody(ImmutableArray<S.Stmt> body, ScopeContext context)
    {
        var translator = new StmtVisitor { context = context };
        return translator.VisitBody(body);
    }

    public static TranslationResult<ImmutableArray<R.Stmt>> TranslateEmbeddable(S.EmbeddableStmt stmt, ScopeContext context)
    {
        var translator = new StmtVisitor { context = context };
        return translator.VisitEmbeddable(stmt);
    }

    public static TranslationResult<ImmutableArray<R.Stmt>> TranslateForStmtInitializer(S.ForStmtInitializer forInit, ScopeContext context)
    {
        var translator = new StmtVisitor { context = context };
        return translator.VisitForStmtInitializer(forInit);
    }

    public static TranslationResult<IR0ExpResult> TranslateAsTopLevelExp(S.Exp expSyntax, ScopeContext context, IType? hintType, SyntaxAnalysisErrorCode code)
    {
        TranslationResult<IR0ExpResult> Error() => TranslationResult.Error<IR0ExpResult>();

        var result = ExpIR0ExpTranslator.Translate(expSyntax, context, hintType);
        if (!result.IsValid(out var expResult))
            return Error();

        if (!IsTopLevelExp(expResult.Exp))
        {
            context.AddFatalError(code, expSyntax);
            return Error();
        }

        return TranslationResult.Valid(expResult);
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitVarDecl(S.VarDeclStmt stmt)
    {
        // int a;
        // var x = 

        return VarDeclVisitor.Visit(stmt.VarDecl, context);
    }

    // CommandStmt에 있는 expStringElement를 분석한다
    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitCommand(S.CommandStmt cmdStmt)
    {
        var builder = ImmutableArray.CreateBuilder<R.StringExp>();        
        foreach (var cmd in cmdStmt.Commands)
        {
            var expResult = ExpIR0ExpTranslator.TranslateString(cmd, context, hintType: null);
            if (!expResult.IsValid(out var exp))
                return Error();

            builder.Add(exp);
        }

        return Stmts(new R.CommandStmt(builder.ToImmutable()));
    }
    
    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitIfTest(S.IfTestStmt ifTestStmt)
    {
        var varName = new Name.Normal(ifTestStmt.VarName);

        // if (Type varName = e) body 
        var testType = context.MakeType(ifTestStmt.TestTypeExp);

        var targetResult = ExpIR0ExpTranslator.Translate(ifTestStmt.Exp, context, hintType: null);
        if (!targetResult.IsValid(out var targetExpResult))
            return Error();

        var (targetExp, targetType) = targetExpResult;

        var bodyContext = context.MakeNestedScopeContext();
        bodyContext.AddLocalVarInfo(testType, varName);

        var bodyStmtsResult = StmtVisitor.TranslateEmbeddable(ifTestStmt.Body, bodyContext);
        if (!bodyStmtsResult.IsValid(out var bodyStmts))
            return Error();

        ImmutableArray<R.Stmt> elseStmts = default;
        if (ifTestStmt.ElseBody != null)
        {
            var elseContext = context.MakeNestedScopeContext();
            var elseStmtsResult = StmtVisitor.TranslateEmbeddable(ifTestStmt.ElseBody, elseContext);
            if (!elseStmtsResult.IsValid(out elseStmts))
                return Error();
        }

        var result = BodyMisc.MakeAsExp(targetType, testType, targetExp);
        if (!result.IsValid(out var asExpResult))
            return Error();

        if (testType is ClassType || testType is InterfaceType)
            return Stmts(new R.IfNullableRefTestStmt(testType, varName, asExpResult.Exp, bodyStmts, elseStmts));
        else if (testType is EnumType)
            return Stmts(new R.IfNullableValueTestStmt(testType, varName, asExpResult.Exp, bodyStmts, elseStmts));
        else
            throw new NotImplementedException(); // 에러
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitIf(S.IfStmt ifStmt)
    {
        // 순회
        var condResult = ExpIR0ExpTranslator.Translate(ifStmt.Cond, context, context.GetBoolType());
        if (!condResult.IsValid(out var condExpResult))
            return Error();

        var condExp = BodyMisc.TryCastExp_Exp(condExpResult.Exp, condExpResult.ExpType, context.GetBoolType(), context);

        if (condExp == null)
        {
            context.AddFatalError(A1001_IfStmt_ConditionShouldBeBool, ifStmt.Cond);
            return Error();
        }

        var bodyContext = context.MakeNestedScopeContext();
        var bodyStmtsResult = StmtVisitor.TranslateEmbeddable(ifStmt.Body, bodyContext);
        if (!bodyStmtsResult.IsValid(out var bodyStmts))
            return Error();

        ImmutableArray<R.Stmt> elseStmts = default;
        if (ifStmt.ElseBody != null)
        {
            var elseContext = context.MakeNestedScopeContext();
            var elseStmtsResult = StmtVisitor.TranslateEmbeddable(ifStmt.ElseBody, elseContext);
            if (!elseStmtsResult.IsValid(out elseStmts))
                return Error();
        }

        var stmt = new R.IfStmt(condExp, bodyStmts, elseStmts);
        return Stmts(stmt);
    }

    TranslationResult<ImmutableArray<R.Stmt>> VisitForStmtInitializer(S.ForStmtInitializer forInit)
    {
        switch (forInit)
        {
            case S.VarDeclForStmtInitializer varDeclInit:
                return VarDeclVisitor.Visit(varDeclInit.VarDecl, context);

            case S.ExpForStmtInitializer expInit:
                {
                    var initResult = StmtVisitor.TranslateAsTopLevelExp(expInit.Exp, context, hintType: null, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall);
                    if (!initResult.IsValid(out var initExpResult))
                        return Error();

                    return Stmts(new R.ExpStmt(initExpResult.Exp));
                }

            default:
                throw new NotImplementedException();
        }
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitFor(S.ForStmt forStmt)
    {
        // for(
        //     int i = 0; <- forStmtContext 
        //     i < 20; <- condition
        //     i++)
        // {
        // 
        // }

        var forStmtContext = context.MakeNestedScopeContext();

        ImmutableArray<R.Stmt> initStmts = default;
        if (forStmt.Initializer != null)
        {
            var initStmtsResult = StmtVisitor.TranslateForStmtInitializer(forStmt.Initializer, forStmtContext);
            if (!initStmtsResult.IsValid(out initStmts))
                return Error();
        }

        R.Exp? condExp = null;
        if (forStmt.CondExp != null)
        {
            var boolType = forStmtContext.GetBoolType();
            var rawCondResult = ExpIR0ExpTranslator.Translate(forStmt.CondExp, forStmtContext, boolType);
            if (!rawCondResult.IsValid(out var rawCondExpResult))
                return Error();

            condExp = BodyMisc.TryCastExp_Exp(rawCondExpResult.Exp, rawCondExpResult.ExpType, boolType, context);

            if (condExp == null)
            {
                context.AddFatalError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp);
                return Error();
            }
        }

        R.Exp? continueExp = null;
        if (forStmt.ContinueExp != null)
        {
            var continueResult = StmtVisitor.TranslateAsTopLevelExp(forStmt.ContinueExp, forStmtContext, hintType: null, A1103_ForStmt_ContinueExpShouldBeAssignOrCall);
            if (!continueResult.IsValid(out var continueExpResult))
                return Error();

            continueExp = continueExpResult.Exp;
        }

        var bodyContext = forStmtContext.MakeLoopNestedScopeContext();        

        var bodyStmtsResult = StmtVisitor.TranslateEmbeddable(forStmt.Body, bodyContext);
        if (!bodyStmtsResult.IsValid(out var bodyStmts))
            return Error();

        return Stmts(new R.ForStmt(initStmts, condExp, continueExp, bodyStmts));
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitContinue(S.ContinueStmt continueStmt)
    {
        if (!context.IsInLoop())
        {
            context.AddFatalError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);
            return Error();
        }

        return Stmts(new R.ContinueStmt());
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitBreak(S.BreakStmt breakStmt)
    {
        if (!context.IsInLoop())
        {
            context.AddFatalError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt);
            return Error();
        }

        return Stmts(new R.BreakStmt());
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitReturn(S.ReturnStmt returnStmt)
    {
        // seq 함수는 여기서 모두 처리 
        if (context.IsSeqFunc())
        {
            if (returnStmt.Info != null)
            {
                context.AddFatalError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt);
                return Error();
            }

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
                if (funcReturn != null && !BodyMisc.TypeEquals(funcReturn.Value.Type, context.GetVoidType()))
                {
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                    return Error();
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
        //        if (!BodyMisc.TypeEquals(returnType, funcReturn.Value))
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
                var retValueResult = ExpIR0ExpTranslator.Translate(returnStmt.Info.Value.Value, context, hintType: null);
                if (!retValueResult.IsValid(out var retValueExpResult))
                    return Error();

                // 리턴값이 안 적혀 있었으므로 적는다
                context.SetReturn(retValueExpResult.ExpType);
                return Stmts(new R.ReturnStmt(new R.ReturnInfo.Expression(retValueExpResult.Exp)));
            }
            else
            {
                var funcReturn = context.GetReturn();

                // 생성자였다면
                if (funcReturn == null)
                    throw new NotImplementedException(); // 에러 처리

                // 리턴타입을 힌트로 사용한다
                // 현재 함수 시그니처랑 맞춰서 같은지 확인한다

                var retValueResult = ExpIR0ExpTranslator.Translate(returnStmt.Info.Value.Value, context, funcReturn.Value.Type);
                if (!retValueResult.IsValid(out var retValueExpResult))
                    return Error();

                var castRetValueExp = BodyMisc.TryCastExp_Exp(retValueExpResult.Exp, retValueExpResult.ExpType, funcReturn.Value.Type, context);

                // 캐스트 실패시
                if (castRetValueExp == null)
                {
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                    return Error();
                }
                
                return Stmts(new R.ReturnStmt(new R.ReturnInfo.Expression(castRetValueExp)));
            }
        }
    }

    // { }
    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitBlock(S.BlockStmt blockStmt)
    {
        bool bFatal = false;
        ScopeContext blockContext = context.MakeNestedScopeContext();

        var builder = ImmutableArray.CreateBuilder<R.Stmt>();
        foreach (var stmt in blockStmt.Stmts)
        {   
            var innerStmtsResult = StmtVisitor.Translate(stmt, blockContext);
            if (!innerStmtsResult.IsValid(out var innerStmts))
            {
                bFatal = true;
                continue; // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            }

            builder.AddRange(innerStmts.AsEnumerable());
        }

        if (bFatal)
            return Error();
        
        return Stmts(new R.BlockStmt(builder.ToImmutable()));
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitBlank(S.BlankStmt stmt)
    {
        return Stmts(new R.BlankStmt());
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitExp(S.ExpStmt expStmt)
    {
        var result = StmtVisitor.TranslateAsTopLevelExp(expStmt.Exp, context, hintType: null, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);
        if (!result.IsValid(out var expResult))
            return Error();

        return Stmts(new R.ExpStmt(expResult.Exp));
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitTask(S.TaskStmt taskStmt)
    {
        var lambdaInfoResult = LambdaVisitor.Translate(context.GetVoidType(), paramSyntaxes: default, taskStmt.Body, context, taskStmt);
        if (!lambdaInfoResult.IsValid(out var lambdaInfo))
            return Error();
        
        return Stmts(new R.TaskStmt(lambdaInfo.Lambda, lambdaInfo.Args));
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitAwait(S.AwaitStmt awaitStmt)
    {
        var newContext = context.MakeNestedScopeContext();        
        var stmtsResult = StmtVisitor.TranslateBody(awaitStmt.Body, newContext);
        if (!stmtsResult.IsValid(out var stmts))
            return Error();

        return Stmts(new R.AwaitStmt(stmts));
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitAsync(S.AsyncStmt asyncStmt)
    {
        var lambdaInfoResult = LambdaVisitor.Translate(context.GetVoidType(), default, asyncStmt.Body, context, asyncStmt);
        if (!lambdaInfoResult.IsValid(out var lambdaInfo))
            return Error();
        
        return Stmts(new R.AsyncStmt(lambdaInfo.Lambda, lambdaInfo.Args));
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitForeach(S.ForeachStmt foreachStmt)
    {
        // SYNTAX: foreach(var elemVarName in iterator) body
        // IR0: foreach(type elemVarName in iteratorLoc)
        var iterResult = ExpIR0LocTranslator.Translate(foreachStmt.Iterator, context, hintType: null, bWrapExpAsLoc: true, A2015_ResolveIdentifier_ExpressionIsNotLocation);
        if (!iterResult.IsValid(out var iter))
            return Error();

        var (iterLoc, iterType) = iter;

        // 먼저, iteratorResult가 anonymous_seq타입인지 확인한다
        if (context.HasSeqConstraint(iterType, out var seqItemType)) // seq<>
        {
            IType itemType;

            if (BodyMisc.IsVarType(foreachStmt.Type))
            {
                itemType = seqItemType;
            }
            else
            { 
                itemType = context.MakeType(foreachStmt.Type);
            
                // 완전히 같은지 체크
                if (BodyMisc.TypeEquals(itemType, seqItemType))
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

            // 루프 컨텍스트에 로컬을 하나 추가하고
            bodyContext.AddLocalVarInfo(itemType, new Name.Normal(foreachStmt.VarName));
            
            // 본문 분석
            var bodyStmtsResult = StmtVisitor.TranslateEmbeddable(foreachStmt.Body, bodyContext);
            if (!bodyStmtsResult.IsValid(out var bodyStmts))
                return Error();

            return Stmts(new R.ForeachStmt(itemType, foreachStmt.VarName, iterLoc, bodyStmts));
        }
        else
        {
            // 축약형 처리
            // anonymous_seq를 리턴하는 식으로 변경해준다
            // foreach(var i in [1, 2, 3, 4]) => foreach(var i in [1, 2, 3, 4].GetEnumerator())
            if (context.IsListType(iterType, out var listItemType))
            {
                IType itemType;

                if (BodyMisc.IsVarType(foreachStmt.Type))
                {
                    itemType = listItemType;
                }
                else
                { 
                    itemType = context.MakeType(foreachStmt.Type);
                
                    // 완전히 같은지 체크
                    if (BodyMisc.TypeEquals(itemType, listItemType))
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

                var listIterType = context.GetListIterType(listItemType);
                var listIteratorLoc = new R.TempLoc(new R.ListIteratorExp(iterLoc, listIterType), listIterType);

                // 루프 컨텍스트를 하나 열고
                var bodyContext = context.MakeLoopNestedScopeContext();

                // 루프 컨텍스트에 로컬을 하나 추가하고
                bodyContext.AddLocalVarInfo(itemType, new Name.Normal(foreachStmt.VarName));

                // 본문 분석                
                var bodyStmtsResult = StmtVisitor.TranslateEmbeddable(foreachStmt.Body, bodyContext);
                if (!bodyStmtsResult.IsValid(out var bodyStmts))
                    return Error();

                return Stmts(new R.ForeachStmt(itemType, foreachStmt.VarName, listIteratorLoc, bodyStmts));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    // TODO: ref 처리?
    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitYield(S.YieldStmt yieldStmt)
    {
        if (!context.IsSeqFunc())
        {
            context.AddFatalError(A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt);
            return Error();
        }

        // yield에서는 retType이 명시되는 경우만 있을 것이다
        var callableReturn = context.GetReturn();
        Debug.Assert(callableReturn != null);

        // NOTICE: 리턴 타입을 힌트로 넣었다
        var retValueResult = ExpIR0ExpTranslator.Translate(yieldStmt.Value, context, callableReturn.Value.Type);
        if (!retValueResult.IsValid(out var retValueExpResult))
            return Error();

        var castRetValueExpResult = BodyMisc.CastExp_Exp(retValueExpResult.Exp, retValueExpResult.ExpType, callableReturn.Value.Type, yieldStmt.Value, context); // 캐스트 실패시 exception발생
        if (!castRetValueExpResult.IsValid(out var castRetValueExp))
            return Error();
        
        return Stmts(new R.YieldStmt(castRetValueExp));
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitDirective(S.DirectiveStmt directiveStmt)
    {
        switch (directiveStmt.Name)
        {
            case "static_notnull":
                if (directiveStmt.Args.Length != 1)
                {
                    context.AddFatalError(A2801_StaticNotNullDirective_ShouldHaveOneArgument, directiveStmt);
                    return Error();
                }

                var argInfoResult = ExpIR0LocTranslator.Translate(directiveStmt.Args[0], context, hintType: null, bWrapExpAsLoc: false, A2802_StaticNotNullDirective_ArgumentMustBeLocation);
                if (!argInfoResult.IsValid(out var argInfo))
                    return Error();

                return Stmts(new R.DirectiveStmt.StaticNotNull(argInfo.Loc));

            default:
                throw new NotImplementedException(); // 인식할 수 없는 directive입니다
        }
    }

    TranslationResult<ImmutableArray<R.Stmt>> VisitBody(ImmutableArray<S.Stmt> body)
    {
        var builder = ImmutableArray.CreateBuilder<R.Stmt>();

        foreach (var stmt in body)
        {
            var innerStmtsResult = StmtVisitor.Translate(stmt, context);
            if (!innerStmtsResult.IsValid(out var innerStmts))
                return Error();

            builder.AddRange(innerStmts.AsEnumerable());
        }

        return Valid(builder.ToImmutable());
    }

    TranslationResult<ImmutableArray<R.Stmt>> VisitEmbeddable(S.EmbeddableStmt embedStmt)
    {
        // if (...) 'stmt'
        // if (...) '{ stmt... }' 를 받는다
        switch (embedStmt)
        {
            case S.EmbeddableStmt.Single single:
                // TODO: VarDecl은 등장하면 에러를 내도록 한다
                // 지금은 그냥 패스
                return StmtVisitor.Translate(single.Stmt, context);

            case S.EmbeddableStmt.Multiple multiple:

                var builder = ImmutableArray.CreateBuilder<R.Stmt>();

                foreach (var stmt in multiple.Stmts)
                {
                    var stmtsResult = StmtVisitor.Translate(stmt, context);
                    if (!stmtsResult.IsValid(out var stmts))
                        return Error();

                    builder.AddRange(stmts.AsEnumerable());
                }

                return Valid(builder.ToImmutable());

            default:
                throw new NotImplementedException();
        }
    }

    TranslationResult<ImmutableArray<R.Stmt>> IStmtVisitor.VisitEmbeddable(S.EmbeddableStmt stmt)
    {
        throw new NotImplementedException();
    }

    static bool IsTopLevelExp(R.Exp exp)
    {
        switch (exp)
        {
            case R.CallInternalUnaryAssignOperatorExp:
            case R.AssignExp:
            case R.CallGlobalFuncExp:
            case R.CallClassMemberFuncExp:
            case R.CallStructMemberFuncExp:
            case R.CallLambdaExp:
                return true;

            default:
                return false;
        }
    }
}