using Gum.Collections;
using Gum.Infra;
using Gum.IR0Visitor;
using System;
using R = Gum.IR0;

namespace Gum.IR0Analyzer.NullRefAnalysis
{
    struct StmtAnalyzer : IIR0StmtVisitor
    {
        GlobalContext globalContext;
        LocalContext localContext;

        StmtAnalyzer(GlobalContext globalContext, LocalContext localContext)
        {
            this.globalContext = globalContext;
            this.localContext = localContext;
        }

        public static void Analyze(R.Stmt stmt, GlobalContext globalContext, LocalContext? parentContext = null)
        {
            var localContext = new LocalContext(parentContext);
            var analyzer = new StmtAnalyzer(globalContext, localContext);
            analyzer.Visit(stmt);
        }

        public static void Analyze(ImmutableArray<R.Stmt> stmts, GlobalContext globalContext, LocalContext? parentContext = null)
        {
            var localContext = new LocalContext(parentContext);
            var analyzer = new StmtAnalyzer(globalContext, localContext);

            foreach (var stmt in stmts)
                analyzer.Visit(stmt);
        }

        void IIR0StmtVisitor.VisitAsyncStmt(R.AsyncStmt asyncStmt)
        {
            // do nothing
        }

        void IIR0StmtVisitor.VisitAwaitStmt(R.AwaitStmt awaitStmt)
        {
            Analyze(awaitStmt.Body, globalContext, localContext);
        }

        void IIR0StmtVisitor.VisitBlankStmt(R.BlankStmt blankStmt)
        {
            // do nothing
        }

        void IIR0StmtVisitor.VisitBlockStmt(R.BlockStmt blockStmt)
        {
            Analyze(blockStmt.Stmts, globalContext, localContext);
        }

        void IIR0StmtVisitor.VisitBreakStmt(R.BreakStmt breakStmt)
        {
            // do nothing
        }

        // string? x = Func();
        // if (x is not null)
        //     @echo $x

        // 이전 단계까지는 x의 null체크를 하지 않기 때문에 그냥 NullableValueLoc(LocalVarLoc("x")) 가 된다
        void IIR0StmtVisitor.VisitCommandStmt(R.CommandStmt commandStmt)
        {
            ExpAnalyzer.Analyze(commandStmt.Commands, globalContext, localContext);
        }

        void IIR0StmtVisitor.VisitContinueStmt(R.ContinueStmt continueStmt)
        {
            // do nothing
        }

        void IIR0StmtVisitor.VisitDirectiveStmt(R.DirectiveStmt directiveStmt)
        {
            switch(directiveStmt)
            {
                case R.DirectiveStmt.Null:
                case R.DirectiveStmt.NotNull:
                case R.DirectiveStmt.StaticNull:
                    throw new NotImplementedException();

                case R.DirectiveStmt.StaticNotNull staticNotNullDir:
                    {
                        var value = LocAnalyzer.Analyze(staticNotNullDir.Loc, globalContext, localContext);
                        if (!value.IsNotNull())
                            globalContext.AddFatalError(new R0101_StaticNotNullDirective_LocationIsNull());

                        break;
                    }

                case R.DirectiveStmt.StaticUnknownNull:
                    throw new NotImplementedException();

                default: // 나머지는 여기서 처리하지 않는다
                    break;
            }
        }

        void IIR0StmtVisitor.VisitExpStmt(R.ExpStmt expStmt)
        {
            ExpAnalyzer.Analyze(expStmt.Exp, EmptyAbstractValue.Instance, globalContext, localContext);
        }

        void IIR0StmtVisitor.VisitForeachStmt(R.ForeachStmt foreachStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitForStmt(R.ForStmt forStmt)
        {
            throw new NotImplementedException();
        }

        bool IsNullableType(R.Path type)
        {
            return type is R.Path.NullableType;
        }

        void IIR0StmtVisitor.VisitGlobalVarDeclStmt(R.GlobalVarDeclStmt globalVarDeclStmt)
        {
            // declare global variables
            foreach(var elem in globalVarDeclStmt.Elems)
            {
                switch(elem)
                {
                    case R.VarDeclElement.Normal normalElem:
                        {
                            // see declaring type
                            bool bNullAllowed = IsNullableType(normalElem.Type);
                            var value = new NullableAbstractValue(bNullAllowed);

                            ExpAnalyzer.Analyze(normalElem.InitExp, value, globalContext, localContext);
                            globalContext.AddGlobalVariable(normalElem.Name, value);

                            break;
                        }

                    case R.VarDeclElement.NormalDefault normalDefaultElem:
                        // see declaring type
                        if (IsNullableType(normalDefaultElem.Type))
                        {
                            var value = new NullableAbstractValue(true);
                            value.SetNull();

                            globalContext.AddGlobalVariable(normalDefaultElem.Name, value);
                        }
                        else
                        {
                            var value = new NullableAbstractValue(false);
                            globalContext.AddGlobalVariable(normalDefaultElem.Name, value);
                        }
                        break;

                    case R.VarDeclElement.Ref:
                        throw new NotImplementedException();
                }
            }
        }

        void IIR0StmtVisitor.VisitIfStmt(R.IfStmt ifStmt)
        {
            if (ifStmt.ElseBody != null)
            {
                var elseGlobalContext = globalContext.Clone();
                var elseContext = localContext.Clone(); // context가 변경될것이기 때문에 미리 복사본을 떠 놓는다

                Analyze(ifStmt.Body, globalContext, localContext);
                Analyze(ifStmt.ElseBody, elseGlobalContext, elseContext);

                globalContext.Merge(elseGlobalContext);
                localContext.Merge(elseContext);
            }
            else
            {
                Analyze(ifStmt.Body, globalContext, localContext);
            }
        }

        void IIR0StmtVisitor.VisitIfTestClassStmt(R.IfTestClassStmt ifTestClassStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitIfTestEnumElemStmt(R.IfTestEnumElemStmt ifTestEnumElemStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitLocalVarDeclStmt(R.LocalVarDeclStmt localVarDeclStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitReturnStmt(R.ReturnStmt returnStmt)
        {
            switch(returnStmt.Info)
            {
                case R.ReturnInfo.None:

                case R.ReturnInfo.Ref:

                case R.ReturnInfo.Expression:
                    break;
            }

            throw new UnreachableCodeException();
        }

        void IIR0StmtVisitor.VisitTaskStmt(R.TaskStmt taskStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitYieldStmt(R.YieldStmt yieldStmt)
        {
            throw new NotImplementedException();
        }
    }
}
