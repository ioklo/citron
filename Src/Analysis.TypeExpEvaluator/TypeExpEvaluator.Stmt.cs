using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using S = Citron.Syntax;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        struct StmtVisitor
        {
            LocalContext localContext;
            GlobalContext globalContext;

            public StmtVisitor(LocalContext localContext, GlobalContext globalContext)
            {
                this.localContext = localContext;
                this.globalContext = globalContext;
            }

            // Body version
            public static void Visit(ImmutableArray<S.Stmt> body, LocalContext localContext, GlobalContext globalContext)
            {
                var visitor = new StmtVisitor(localContext, globalContext);

                foreach(var stmt in body)
                    visitor.VisitStmt(stmt);
            }

            void VisitCommandStmt(S.CommandStmt cmdStmt)
            {
                foreach (var cmd in cmdStmt.Commands)
                    ExpVisitor.Visit(cmd.Elements, localContext, globalContext);
            }

            void VisitVarDecl(S.VarDecl varDecl)
            {
                TypeExpVisitor.Visit(varDecl.Type, localContext, globalContext);

                foreach (var varDeclElem in varDecl.Elems)
                {
                    if (varDeclElem.Initializer != null)
                        ExpVisitor.Visit(varDeclElem.Initializer.Value.Exp, localContext, globalContext);
                }
            }

            void VisitVarDeclStmt(S.VarDeclStmt varDeclStmt)
            {
                VisitVarDecl(varDeclStmt.VarDecl);
            }

            void VisitIfStmt(S.IfStmt ifStmt)
            {
                ExpVisitor.Visit(ifStmt.Cond, localContext, globalContext);
                VisitStmt(ifStmt.Body);

                if (ifStmt.ElseBody != null)
                    VisitStmt(ifStmt.ElseBody);
            }

            void VisitIfTestStmt(S.IfTestStmt ifTestStmt)
            {
                ExpVisitor.Visit(ifTestStmt.Exp, localContext, globalContext);
                TypeExpVisitor.Visit(ifTestStmt.TestType, localContext, globalContext);
                VisitStmt(ifTestStmt.Body);
                if (ifTestStmt.ElseBody != null)
                    VisitStmt(ifTestStmt.ElseBody);
            }

            void VisitForStmtInitializer(S.ForStmtInitializer initializer)
            {
                switch (initializer)
                {
                    case S.ExpForStmtInitializer expInit: ExpVisitor.Visit(expInit.Exp, localContext, globalContext); break;
                    case S.VarDeclForStmtInitializer varDeclInit: VisitVarDecl(varDeclInit.VarDecl); break;
                    default: throw new UnreachableCodeException();
                }
            }

            void VisitForStmt(S.ForStmt forStmt)
            {
                if (forStmt.Initializer != null)
                    VisitForStmtInitializer(forStmt.Initializer);

                if (forStmt.CondExp != null)
                    ExpVisitor.Visit(forStmt.CondExp, localContext, globalContext);

                if (forStmt.ContinueExp != null)
                    ExpVisitor.Visit(forStmt.ContinueExp, localContext, globalContext);

                VisitStmt(forStmt.Body);
            }

            void VisitContinueStmt(S.ContinueStmt continueStmt)
            {
            }

            void VisitBreakStmt(S.BreakStmt breakStmt)
            {
            }

            void VisitReturnStmt(S.ReturnStmt returnStmt)
            {
                if (returnStmt.Info != null)
                    ExpVisitor.Visit(returnStmt.Info.Value.Value, localContext, globalContext);
            }

            void VisitBlockStmt(S.BlockStmt blockStmt)
            {
                foreach (var stmt in blockStmt.Stmts)
                    VisitStmt(stmt);
            }

            void VisitExpStmt(S.ExpStmt expStmt)
            {
                ExpVisitor.Visit(expStmt.Exp, localContext, globalContext);
            }

            void VisitTaskStmt(S.TaskStmt taskStmt)
            {
                VisitStmt(taskStmt.Body);
            }

            void VisitAwaitStmt(S.AwaitStmt awaitStmt)
            {
                VisitStmt(awaitStmt.Body);
            }

            void VisitAsyncStmt(S.AsyncStmt asyncStmt)
            {
                VisitStmt(asyncStmt.Body);
            }

            void VisitForeachStmt(S.ForeachStmt foreachStmt)
            {
                TypeExpVisitor.Visit(foreachStmt.Type, localContext, globalContext);
                ExpVisitor.Visit(foreachStmt.Iterator, localContext, globalContext);
                VisitStmt(foreachStmt.Body);
            }

            void VisitYieldStmt(S.YieldStmt yieldStmt)
            {
                ExpVisitor.Visit(yieldStmt.Value, localContext, globalContext);
            }

            void VisitDirectiveStmt(S.DirectiveStmt directiveStmt)
            {
                foreach (var arg in directiveStmt.Args)
                    ExpVisitor.Visit(arg, localContext, globalContext);
            }

            public void VisitStmt(S.Stmt stmt)
            {
                switch (stmt)
                {
                    case S.CommandStmt cmdStmt: VisitCommandStmt(cmdStmt); break;
                    case S.VarDeclStmt varDeclStmt: VisitVarDeclStmt(varDeclStmt); break;
                    case S.IfStmt ifStmt: VisitIfStmt(ifStmt); break;
                    case S.IfTestStmt ifTestStmt: VisitIfTestStmt(ifTestStmt); break;
                    case S.ForStmt forStmt: VisitForStmt(forStmt); break;
                    case S.ContinueStmt continueStmt: VisitContinueStmt(continueStmt); break;
                    case S.BreakStmt breakStmt: VisitBreakStmt(breakStmt); break;
                    case S.ReturnStmt returnStmt: VisitReturnStmt(returnStmt); break;
                    case S.BlockStmt blockStmt: VisitBlockStmt(blockStmt); break;
                    case S.BlankStmt _: break;
                    case S.ExpStmt expStmt: VisitExpStmt(expStmt); break;
                    case S.TaskStmt taskStmt: VisitTaskStmt(taskStmt); break;
                    case S.AwaitStmt awaitStmt: VisitAwaitStmt(awaitStmt); break;
                    case S.AsyncStmt asyncStmt: VisitAsyncStmt(asyncStmt); break;
                    case S.ForeachStmt foreachStmt: VisitForeachStmt(foreachStmt); break;
                    case S.YieldStmt yieldStmt: VisitYieldStmt(yieldStmt); break;
                    case S.DirectiveStmt directiveStmt: VisitDirectiveStmt(directiveStmt); break;
                    default: throw new UnreachableCodeException();
                };
            }
        }
    }
}
