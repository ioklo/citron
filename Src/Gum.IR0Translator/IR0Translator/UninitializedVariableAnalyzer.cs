using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using R = Gum.IR0;
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    partial struct UninitializedVariableAnalyzer
    {
        // Mutable
        class Context
        {
            Context? origParent;    // 원본 parent
            Context? parent;
            Dictionary<string, bool> localVars;

            Context(Context? origParent, Context? parent, Dictionary<string, bool> localVars)
            {
                this.origParent = origParent;
                this.parent = parent;
                this.localVars = localVars;
            }

            public Context(Context? origParent)
            {
                this.origParent = origParent;
                this.parent = origParent;
                this.localVars = new Dictionary<string, bool>();
            }            

            public void AddLocalVar(string name, bool initialized)
            {
                localVars.Add(name, initialized);
            }

            public bool IsInitialized(string name)
            {
                if (localVars.TryGetValue(name, out var initialized))
                    return initialized;

                if (parent != null)
                    return parent.IsInitialized(name);

                // 모든 localvariable이 등록되게 되어있으므로 여기에 오면 안된다
                throw new UnreachableCodeException();
            }

            public void SetInitialized(string name)
            {
                if (localVars.ContainsKey(name))
                {
                    localVars[name] = true;
                    return;
                }

                if (parent != null)
                {
                    // 아직 복사 전이라면 
                    EnsureCloneParent();
                    parent.SetInitialized(name);
                }
            }

            void EnsureCloneParent()
            {
                if (origParent == null) return;
                if (origParent != parent) return;

                parent = new Context(origParent.origParent, origParent.parent, new Dictionary<string, bool>(origParent.localVars));
            }

            public void Merge(Context childX, Context childY)
            {
                Debug.Assert(childX.parent != null && childY.parent != null);

                // fast-forward, no modified
                if (this == childX.parent && this == childY.parent)
                {
                    return;
                }
                else if (this != childX.parent && this == childY.parent)
                {
                    parent = childX.parent.parent;
                    localVars = childX.parent.localVars;
                    return;
                }
                else if (this == childX && this != childY)
                {
                    parent = childY.parent.parent;
                    localVars = childY.parent.localVars;
                    return;
                }
                else
                {
                    foreach (var key in localVars.Keys)
                        localVars[key] = childX.parent.localVars[key] && childY.parent.localVars[key];
                }

                if (parent != null)
                {
                    parent.Merge(childX.parent, childY.parent);
                }
            }

            public void Replace(Context child)
            {
                Debug.Assert(child.parent != null);
                parent = child.parent.parent;
                localVars = child.parent.localVars;
            }
        }
    }

    partial struct UninitializedVariableAnalyzer
    {
        IErrorCollector errorCollector;
        Context context;

        public static void Analyze(R.Script script, IErrorCollector errorCollector)
        {
            var analyzer = new UninitializedVariableAnalyzer(errorCollector, new Context(null));
            analyzer.AnalyzeStmts(script.TopLevelStmts);
        }

        UninitializedVariableAnalyzer(IErrorCollector errorCollector, Context context)
        {
            this.errorCollector = errorCollector;
            this.context = context;
        }

        UninitializedVariableAnalyzer NewAnalyzer()
        {
            return new UninitializedVariableAnalyzer(errorCollector, new Context(context));
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
            foreach(var c in commandStmt.Commands)
                AnalyzeStringExp(c);
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
                        context.AddLocalVar(normalElem.Name, false);
                    else
                        context.AddLocalVar(normalElem.Name, true);

                    break;

                case R.VarDeclElement.Ref refElem:
                    // TODO: ref var decl uninitialized
                    context.AddLocalVar(refElem.Name, true);
                    break;
            }
        }

        void AnalyzeIfStmt(R.IfStmt ifStmt)
        {
            var newAnalyzerBody = NewAnalyzer();
            newAnalyzerBody.AnalyzeStmt(ifStmt.Body);

            if (ifStmt.ElseBody != null)
            {
                var newAnalyzerElse = NewAnalyzer();
                newAnalyzerElse.AnalyzeStmt(ifStmt.ElseBody);

                Debug.Assert(newAnalyzerBody.context != null && newAnalyzerElse.context != null);
                context.Merge(newAnalyzerBody.context, newAnalyzerElse.context);
            }
            else
            {
                context.Replace(newAnalyzerBody.context);
            }
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
            var newAnalyzer = NewAnalyzer();

            foreach(var stmt in blockStmt.Stmts)
                newAnalyzer.AnalyzeStmt(stmt);

            // 새 localVars로 덮어 씌운다
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

        void AnalyzeLoadExp(R.LoadExp loadExp)
        {
            var localVarName = AnalyzeLoc(loadExp.Loc);
            if (localVarName != null)
                CheckInitialized(localVarName, loadExp.Loc);
        }

        void AnalyzeStringExp(R.StringExp stringExp)
        {
            foreach (var elem in stringExp.Elements)
                AnalyzeStringExpElem(elem);
        }

        void AnalyzeStringExpElem(R.StringExpElement elem)
        {
            switch(elem)
            {
                case R.ExpStringExpElement expElem:
                    AnalyzeExp(expElem.Exp);
                    break;

                case R.TextStringExpElement:                    
                    break;
            }
        }

        void AnalyzeIntLiteralExp(R.IntLiteralExp intExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeBoolLiteralExp(R.BoolLiteralExp boolExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp)
        {
            AnalyzeExp(ciuoExp.Operand);
        }

        void AnalyzeCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperator ciuaoExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeAssignExp(R.AssignExp assignExp)
        {
            AnalyzeExp(assignExp.Src);

            var localVarName = AnalyzeLoc(assignExp.Dest);
            if (localVarName != null)
                CheckInitialized(localVarName, assignExp.Dest);
        }

        void AnalyzeCallFuncExp(R.CallFuncExp callFuncExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeCallValueExp(R.CallValueExp callValueExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeLambdaExp(R.LambdaExp lambdaExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeListExp(R.ListExp listExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeListIterExp(R.ListIteratorExp listIterExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeNewEnumExp(R.NewEnumElemExp enumExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeNewStructExp(R.NewStructExp newStructExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeNewClassExp(R.NewClassExp newClassExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeCastClassExp(R.CastClassExp castClassExp)
        {
            throw new NotImplementedException();
        }

        // 일단 이름 붙이지 말고, LocalVar이름 리턴하는 것으로
        string? AnalyzeLoc(R.Loc loc)
        {
            switch (loc)
            {
                case R.TempLoc tempLoc: 
                    AnalyzeExp(tempLoc.Exp); 
                    return null;

                case R.GlobalVarLoc globalVarLoc: return null;
                case R.LocalVarLoc localVarLoc:                    
                    return localVarLoc.Name;                    

                case R.CapturedVarLoc capturedVarLoc:
                    return null;

                case R.ListIndexerLoc listIndexerLoc:
                    {
                        var list = AnalyzeLoc(listIndexerLoc.List);
                        if (list != null)
                            CheckInitialized(list, null);

                        AnalyzeExp(listIndexerLoc.Index);
                        return null;
                    }

                case R.StaticMemberLoc staticMemberLoc:
                    return null;

                case R.StructMemberLoc structMemberLoc:
                    {
                        var instance = AnalyzeLoc(structMemberLoc.Instance);
                        if (instance != null)
                            CheckInitialized(instance, structMemberLoc.Instance);

                        return null;
                    }

                case R.ClassMemberLoc classMemberLoc:
                    {
                        var instance = AnalyzeLoc(classMemberLoc.Instance);
                        if (instance != null)
                            CheckInitialized(instance, classMemberLoc.Instance);

                        return null;
                    }

                case R.EnumElemMemberLoc enumMemberLoc:
                    {
                        var instance = AnalyzeLoc(enumMemberLoc.Instance);
                        if (instance != null)
                            CheckInitialized(instance, enumMemberLoc.Instance);

                        return null;
                    }

                case R.ThisLoc thisLoc:
                    return null;

                case R.DerefLocLoc derefLoc:
                    {
                        var innerLoc = AnalyzeLoc(derefLoc.Loc);
                        if (innerLoc != null)
                            CheckInitialized(innerLoc, derefLoc.Loc);

                        return null;
                    }

                case R.DerefExpLoc derefExpLoc:
                    {
                        AnalyzeExp(derefExpLoc.Exp);
                        return null;
                    }

                default:
                    throw new UnreachableCodeException();
            }

        }

        void CheckInitialized(string localVarName, R.INode nodeForErrorReport)
        {
            if (context.IsInitialized(localVarName))
                errorCollector.Add(new AnalyzeError(AnalyzeErrorCode.R0101_UninitializedVaraibleAnalyzer_UseUninitializedValue, null, ""));
        }
    }
}