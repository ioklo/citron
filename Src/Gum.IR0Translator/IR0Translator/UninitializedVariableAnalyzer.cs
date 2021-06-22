using Gum.Collections;
using System;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    struct UninitializedVariableAnalyzer
    {        
        ImmutableDictionary<string, bool> initialized;

        public static void Analyze(R.Script script)
        {
            var analyzer = new UninitializedVariableAnalyzer(ImmutableDictionary<string, bool>.Empty);
            analyzer.AnalyzeStmts(script.TopLevelStmts);
            // analyzer.AnalyzeDecls            
        }

        UninitializedVariableAnalyzer(ImmutableDictionary<string, bool> initialized)
        {
            this.initialized = initialized;
        }

        void AnalyzeStmts(ImmutableArray<R.Stmt> topLevelStmts)
        {
            foreach (var topLevelStmt in topLevelStmts)
                AnalyzeStmt(topLevelStmt);
        }

        void AnalyzeStmt(R.Stmt topLevelStmt)
        {
            switch(topLevelStmt)
            {
                case R.CommandStmt commandStmt: AnalyzeCommandStmt(commandStmt); break;                     
                case R.GlobalVarDeclStmt globalVarDeclStmt: AnalyzeGlobalVarDeclStmt(globalVarDeclStmt); break;
                case R.LocalVarDeclStmt localVarDeclStmt: AnalyzeLocalVarDeclStmt(localVarDeclStmt); break;
                case R.IfStmt ifStmt: AnalyzeIfStmt(ifStmt); break;
                case R.IfTestClassStmt ifTestClassStmt: AnalyzeIfTestClassStmt(ifTestClassStmt); break;
                case R.IfTestEnumElemStmt ifTestEnumElemStmt: AnalyzeIfTestEnumElemStmt(ifTestEnumElemStmt); break;
                case R.ForStmt forStmt: AnalyzeForStmt(forStmt); break;
                case R.ContinueStmt continueStmt: AnalyzeContinueStmt(continueStmt); break;
                case R.BreakStmt breakStmt: AnalyzeBreakStmt(breakStmt); break;
                case R.ReturnStmt returnStmt: AnalyzeReturnStmt(returnStmt); break;
                case R.BlockStmt blockStmt: AnalyzeBlockStmt(blockStmt); break;
                case R.BlankStmt blankStmt: AnalyzeBlankStmt(blankStmt); break;
                case R.ExpStmt expStmt: AnalyzeExpStmt(expStmt); break;
                case R.TaskStmt taskStmt: AnalyzeTaskStmt(taskStmt); break;
                case R.AwaitStmt awaitStmt: AnalyzeAwaitStmt(awaitStmt); break;
                case R.AsyncStmt asyncStmt: AnalyzeAsyncStmt(asyncStmt); break;
                case R.ForeachStmt foreachStmt: AnalyzeForeachStmt(foreachStmt); break;
                case R.YieldStmt yieldStmt: AnalyzeYieldStmt(yieldStmt); break;
            }
        }

        void AnalyzeCommandStmt(R.CommandStmt commandStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeGlobalVarDeclStmt(R.GlobalVarDeclStmt globalVarDeclStmt)
        {
            // throw new NotImplementedException();
        }

        void AnalyzeLocalVarDeclStmt(R.LocalVarDeclStmt localVarDeclStmt)
        {
            AnalyzeLocalVarDecl(localVarDeclStmt.VarDecl);
        }

        void AnalyzeLocalVarDecl(R.LocalVarDecl varDecl)
        {
            foreach (var elem in varDecl.Elems)
                AnalyzeVarDeclElement(elem);
        }

        void AnalyzeVarDeclElement(R.VarDeclElement elem)
        {
            switch(elem)
            {                
                case R.VarDeclElement.Normal normalElem:

                    if (normalElem.InitExp == null)
                        initialized.SetItem(normalElem.Name, false);
                    else
                        initialized.SetItem(normalElem.Name, true);

                    break;

                case R.VarDeclElement.Ref refElem:
                    // TODO: ref var decl uninitialized
                    initialized.SetItem(refElem.Name, true);
                    break;
            }
        }

        void AnalyzeIfStmt(R.IfStmt ifStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeIfTestClassStmt(R.IfTestClassStmt ifTestClassStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeIfTestEnumElemStmt(R.IfTestEnumElemStmt ifTestEnumElemStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeForStmt(R.ForStmt forStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeContinueStmt(R.ContinueStmt continueStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeBreakStmt(R.BreakStmt breakStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeReturnStmt(R.ReturnStmt returnStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeBlockStmt(R.BlockStmt blockStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeBlankStmt(R.BlankStmt blankStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeExpStmt(R.ExpStmt expStmt)
        {
            AnalyzeExp(expStmt.Exp);
        }        

        void AnalyzeTaskStmt(R.TaskStmt taskStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeAwaitStmt(R.AwaitStmt awaitStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeAsyncStmt(R.AsyncStmt asyncStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeForeachStmt(R.ForeachStmt foreachStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeYieldStmt(R.YieldStmt yieldStmt)
        {
            throw new NotImplementedException();
        }

        void AnalyzeExp(R.Exp exp)
        {
            switch(exp)
            {
                case R.LoadExp loadExp: AnalyzeLoadExpAsync(loadExp, result); break;
                case R.StringExp stringExp: AnalyzeStringExpAsync(stringExp, result); break;
                case R.IntLiteralExp intExp: AnalyzeIntLiteralExp(intExp, result); break;
                case R.BoolLiteralExp boolExp: AnalyzeBoolLiteralExp(boolExp, result); break;
                case R.CallInternalUnaryOperatorExp ciuoExp: AnalyzeCallInternalUnaryOperatorExpAsync(ciuoExp, result); break;
                case R.CallInternalUnaryAssignOperator ciuaoExp: AnalyzeCallInternalUnaryAssignOperatorExpAsync(ciuaoExp, result); break;
                case R.CallInternalBinaryOperatorExp ciboExp: AnalyzeCallInternalBinaryOperatorExpAsync(ciboExp, result); break;
                case R.AssignExp assignExp: AnalyzeAssignExpAsync(assignExp, result); break;
                case R.CallFuncExp callFuncExp: AnalyzeCallFuncExpAsync(callFuncExp, result); break;
                case R.CallSeqFuncExp callSeqFuncExp: AnalyzeCallSeqFuncExpAsync(callSeqFuncExp, result); break;
                case R.CallValueExp callValueExp: AnalyzeCallValueExpAsync(callValueExp, result); break;
                case R.LambdaExp lambdaExp: AnalyzeLambdaExp(lambdaExp, result); break;
                case R.ListExp listExp: AnalyzeListExpAsync(listExp, result); break;
                case R.ListIteratorExp listIterExp: AnalyzeListIterExpAsync(listIterExp, result); break;
                case R.NewEnumElemExp enumExp: AnalyzeNewEnumExpAsync(enumExp, result); break;
                case R.NewStructExp newStructExp: AnalyzeNewStructExpAsync(newStructExp, result); break;
                case R.NewClassExp newClassExp: AnalyzeNewClassExpAsync(newClassExp, result); break;
                case R.CastEnumElemToEnumExp castEnumElemToEnumExp: AnalyzeCastEnumElemToEnumExp(castEnumElemToEnumExp, result); break;
                case R.CastClassExp castClassExp: AnalyzeCastClassExp(castClassExp, result); break;
            }
        }
    }
}