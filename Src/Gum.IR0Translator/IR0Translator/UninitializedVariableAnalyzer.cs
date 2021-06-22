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
                case R.LoadExp loadExp: AnalyzeLoadExp(loadExp); break;
                case R.StringExp stringExp: AnalyzeStringExp(stringExp); break;
                case R.IntLiteralExp intExp: AnalyzeIntLiteralExp(intExp); break;
                case R.BoolLiteralExp boolExp: AnalyzeBoolLiteralExp(boolExp); break;
                case R.CallInternalUnaryOperatorExp ciuoExp: AnalyzeCallInternalUnaryOperatorExp(ciuoExp); break;
                case R.CallInternalUnaryAssignOperator ciuaoExp: AnalyzeCallInternalUnaryAssignOperatorExp(ciuaoExp); break;
                case R.CallInternalBinaryOperatorExp ciboExp: AnalyzeCallInternalBinaryOperatorExp(ciboExp); break;
                case R.AssignExp assignExp: AnalyzeAssignExp(assignExp); break;
                case R.CallFuncExp callFuncExp: AnalyzeCallFuncExp(callFuncExp); break;
                case R.CallSeqFuncExp callSeqFuncExp: AnalyzeCallSeqFuncExp(callSeqFuncExp); break;
                case R.CallValueExp callValueExp: AnalyzeCallValueExp(callValueExp); break;
                case R.LambdaExp lambdaExp: AnalyzeLambdaExp(lambdaExp); break;
                case R.ListExp listExp: AnalyzeListExp(listExp); break;
                case R.ListIteratorExp listIterExp: AnalyzeListIterExp(listIterExp); break;
                case R.NewEnumElemExp enumExp: AnalyzeNewEnumExp(enumExp); break;
                case R.NewStructExp newStructExp: AnalyzeNewStructExp(newStructExp); break;
                case R.NewClassExp newClassExp: AnalyzeNewClassExp(newClassExp); break;
                case R.CastEnumElemToEnumExp castEnumElemToEnumExp: AnalyzeCastEnumElemToEnumExp(castEnumElemToEnumExp); break;
                case R.CastClassExp castClassExp: AnalyzeCastClassExp(castClassExp); break;
            }
        }

        private void AnalyzeLoadExp(R.LoadExp loadExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeStringExp(R.StringExp stringExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeIntLiteralExp(R.IntLiteralExp intExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeBoolLiteralExp(R.BoolLiteralExp boolExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperator ciuaoExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeAssignExp(R.AssignExp assignExp)
        {   
            switch(assignExp.Dest)
            {
                case R.LocalVarLoc localDest:
                    initialized.SetItem(localDest.Name, true);
                    break;

                case R.TempLoc tempDest:
                    AnalyzeExp(tempDest.Exp);
                    break;

                default:
                    break;
            }
        }

        private void AnalyzeCallFuncExp(R.CallFuncExp callFuncExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeCallValueExp(R.CallValueExp callValueExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeLambdaExp(R.LambdaExp lambdaExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeListExp(R.ListExp listExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeListIterExp(R.ListIteratorExp listIterExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeNewEnumExp(R.NewEnumElemExp enumExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeNewStructExp(R.NewStructExp newStructExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeNewClassExp(R.NewClassExp newClassExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp)
        {
            throw new NotImplementedException();
        }

        private void AnalyzeCastClassExp(R.CastClassExp castClassExp)
        {
            throw new NotImplementedException();
        }
    }
}