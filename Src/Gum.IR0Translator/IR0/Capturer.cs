using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using S = Gum.Syntax;

namespace Gum.IR0
{
    public class CaptureResult
    {
        public ImmutableArray<string> NeedCaptures { get; }
        public CaptureResult(IEnumerable<string> needCaptures)
        {
            NeedCaptures = needCaptures.ToImmutableArray();
        }
    }

    class CaptureContext
    {
        ImmutableHashSet<string> boundVars;
        HashSet<string> needCaptures { get; } // bool => ref or copy 

        public CaptureContext(IEnumerable<string> initBoundVars)
        {
            boundVars = ImmutableHashSet.CreateRange<string>(initBoundVars);
            needCaptures = new HashSet<string>();
        }

        public void AddBind(string varName)
        {
            boundVars = boundVars.Add(varName);
        }

        public void AddBinds(IEnumerable<string> names)
        {
            boundVars = boundVars.Union(names);
        }

        public bool IsBound(string name)
        {
            return boundVars.Contains(name);
        }

        public void AddCapture(string name)
        {
            if (needCaptures.Contains(name))                
                return;

            needCaptures.Add(name);
        }

        public ImmutableHashSet<string> GetBoundVars()
        {
            return boundVars;
        }

        public void SetBoundVars(ImmutableHashSet<string> newBoundVars)
        {
            boundVars = newBoundVars;
        }        

        public IEnumerable<string> GetNeedCaptures()
        {
            return needCaptures;
        }
    }

    class Capturer
    {
        bool CaptureStringExpElements(ImmutableArray<S.StringExpElement> elems, CaptureContext context)
        {
            foreach (var elem in elems)
            {
                if (elem is S.TextStringExpElement)
                {
                    continue;
                }
                else if (elem is S.ExpStringExpElement expElem)
                {
                    if (!CaptureExp(expElem.Exp, context))
                        return false;
                    continue;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return true;
        }

        bool CaptureCommandStmt(S.CommandStmt cmdStmt, CaptureContext context)
        {
            foreach (var command in cmdStmt.Commands)
            {
                if (!CaptureStringExpElements(command.Elements, context))
                    return false;
            }

            return true;
        }

        bool CaptureVarDecl(S.VarDecl varDecl, CaptureContext context)
        {
            context.AddBinds(varDecl.Elems.Select(elem => elem.VarName));
            return true;
        }

        bool CaptureVarDeclStmt(S.VarDeclStmt varDeclStmt, CaptureContext context)
        {
            return CaptureVarDecl(varDeclStmt.VarDecl, context);
        }

        bool CaptureIfStmt(S.IfStmt ifStmt, CaptureContext context) 
        {
            if (!CaptureExp(ifStmt.Cond, context))
                return false;

            // TestType은 capture할 것이 없다

            if (!CaptureStmt(ifStmt.Body, context))
                return false;

            if (ifStmt.ElseBody != null)
            {
                if (!CaptureStmt(ifStmt.ElseBody, context))
                    return false;
            }

            return true;
        }

        bool CaptureForStmtInitialize(S.ForStmtInitializer forInitStmt, CaptureContext context)
        {
            return forInitStmt switch
            {
                S.VarDeclForStmtInitializer varDeclInit => CaptureVarDecl(varDeclInit.VarDecl, context),
                S.ExpForStmtInitializer expInit => CaptureExp(expInit.Exp, context),
                _ => throw new NotImplementedException()
            };
        }

        bool CaptureForStmt(S.ForStmt forStmt, CaptureContext context)         
        {
            var prevBoundVars = context.GetBoundVars();

            if (forStmt.Initializer != null)
            {
                if (!CaptureForStmtInitialize(forStmt.Initializer, context))
                    return false;
            }

            if (forStmt.CondExp != null)
            {
                if (!CaptureExp(forStmt.CondExp, context))
                    return false;
            }

            if (forStmt.ContinueExp != null )
            {
                if (!CaptureExp(forStmt.ContinueExp, context))
                    return false;
            }

            if (!CaptureStmt(forStmt.Body, context))
                return false;

            context.SetBoundVars(prevBoundVars);
            return true;
        }

        bool CaptureContinueStmt(S.ContinueStmt continueStmt, CaptureContext context) { return true; }
        bool CaptureBreakStmt(S.BreakStmt breakStmt, CaptureContext context) { return true; }

        bool CaptureReturnStmt(S.ReturnStmt returnStmt, CaptureContext context)
        {
            if (returnStmt.Value != null)
                return CaptureExp(returnStmt.Value, context);
            else
                return true;
        }

        bool CaptureBlockStmt(S.BlockStmt blockStmt, CaptureContext context) 
        {
            var prevBoundVars = context.GetBoundVars();

            foreach(var stmt in blockStmt.Stmts)
            {
                if (!CaptureStmt(stmt, context))
                    return false;
            }

            context.SetBoundVars(prevBoundVars);
            return true;
        }

        bool CaptureExpStmt(S.ExpStmt expStmt, CaptureContext context)
        {
            return CaptureExp(expStmt.Exp, context);
        }

        bool CaptureTaskStmt(S.TaskStmt stmt, CaptureContext context)
        {
            var prevBoundVars = context.GetBoundVars();

            if (!CaptureStmt(stmt.Body, context))
                return false;

            context.SetBoundVars(prevBoundVars);
            return true;
        }

        bool CaptureAwaitStmt(S.AwaitStmt stmt, CaptureContext context)
        {
            var prevBoundVars = context.GetBoundVars();

            if (!CaptureStmt(stmt.Body, context))
                return false;

            context.SetBoundVars(prevBoundVars);
            return true;
        }

        bool CaptureAsyncStmt(S.AsyncStmt stmt, CaptureContext context)
        {
            var prevBoundVars = context.GetBoundVars();

            if (!CaptureStmt(stmt.Body, context))
                return false;

            context.SetBoundVars(prevBoundVars);
            return true;
        }

        bool CaptureForeachStmt(S.ForeachStmt foreachStmt, CaptureContext context)
        {
            var prevBoundVars = context.GetBoundVars();

            if (!CaptureExp(foreachStmt.Obj, context))
                return false;

            context.AddBind(foreachStmt.VarName);

            if (!CaptureStmt(foreachStmt.Body, context))
                return false;

            context.SetBoundVars(prevBoundVars);
            return true;
        }

        bool CaptureYieldStmt(S.YieldStmt yieldStmt, CaptureContext context)
        {
            return CaptureExp(yieldStmt.Value, context);
        }

        bool CaptureStmt(S.Stmt stmt, CaptureContext context)
        {
            return stmt switch
            {
                S.CommandStmt cmdStmt => CaptureCommandStmt(cmdStmt, context),
                S.VarDeclStmt varDeclStmt => CaptureVarDeclStmt(varDeclStmt, context),
                S.IfStmt ifStmt => CaptureIfStmt(ifStmt, context),
                S.ForStmt forStmt => CaptureForStmt(forStmt, context),
                S.ContinueStmt continueStmt => CaptureContinueStmt(continueStmt, context),
                S.BreakStmt breakStmt => CaptureBreakStmt(breakStmt, context),
                S.ReturnStmt returnStmt => CaptureReturnStmt(returnStmt, context),
                S.BlockStmt blockStmt => CaptureBlockStmt(blockStmt, context),
                S.BlankStmt blankStmt => true,
                S.ExpStmt expStmt => CaptureExpStmt(expStmt, context),
                S.TaskStmt taskStmt => CaptureTaskStmt(taskStmt, context),
                S.AwaitStmt awaitStmt => CaptureAwaitStmt(awaitStmt, context),
                S.AsyncStmt asyncStmt => CaptureAsyncStmt(asyncStmt, context),
                S.ForeachStmt foreachStmt => CaptureForeachStmt(foreachStmt, context),
                S.YieldStmt yieldStmt => CaptureYieldStmt(yieldStmt, context),

                _ => throw new NotImplementedException()
            };
        }

        bool RefCaptureIdExp(S.IdentifierExp idExp, CaptureContext context)
        {
            // 에러: 레퍼런스는 캡쳐할 수 없게 한다
            return false;            
        }

        bool RefCaptureExp(S.Exp exp, CaptureContext context)
        {
            return exp switch
            {
                S.IdentifierExp idExp => RefCaptureIdExp(idExp, context),
                S.BoolLiteralExp boolExp => throw new InvalidOperationException(),
                S.IntLiteralExp intExp => throw new InvalidOperationException(),
                S.StringExp stringExp => throw new InvalidOperationException(),
                S.UnaryOpExp unaryOpExp => throw new InvalidOperationException(),
                S.BinaryOpExp binaryOpExp => throw new InvalidOperationException(),
                S.CallExp callExp => throw new InvalidOperationException(),
                S.LambdaExp lambdaExp => throw new InvalidOperationException(),
                S.MemberCallExp memberCallExp => throw new InvalidOperationException(),
                S.MemberExp memberExp => CaptureMemberExp(memberExp, context),
                S.ListExp listExp => throw new InvalidOperationException(),

                _ => throw new NotImplementedException()
            };
        }

        bool CaptureIdExp(S.IdentifierExp idExp, CaptureContext context) 
        {            
            var varName = idExp.Value;

            // 바인드에 있는지 보고 
            if (!context.IsBound(varName))
            {
                // 캡쳐에 추가
                context.AddCapture(varName);
            }

            return true;
        }

        bool CaptureBoolLiteralExp(S.BoolLiteralExp boolExp, CaptureContext context) => true;
        bool CaptureIntLiteralExp(S.IntLiteralExp intExp, CaptureContext context) => true;
        bool CaptureStringExp(S.StringExp stringExp, CaptureContext context)
        {
            return CaptureStringExpElements(stringExp.Elements, context);
        }

        bool CaptureUnaryOpExp(S.UnaryOpExp unaryOpExp, CaptureContext context) 
        {
            // ++i, i++은 ref를 유발한다
            if (unaryOpExp.Kind == S.UnaryOpKind.PostfixInc ||
                unaryOpExp.Kind == S.UnaryOpKind.PostfixDec ||
                unaryOpExp.Kind == S.UnaryOpKind.PrefixInc ||
                unaryOpExp.Kind == S.UnaryOpKind.PrefixDec)
                return RefCaptureExp(unaryOpExp.Operand, context);
            else
                return CaptureExp(unaryOpExp.Operand, context);
        }

        bool CaptureBinaryOpExp(S.BinaryOpExp binaryOpExp, CaptureContext context) 
        { 
            if (binaryOpExp.Kind == S.BinaryOpKind.Assign)
            {
                if (!RefCaptureExp(binaryOpExp.Operand0, context))
                    return false;
            }
            else
            {
                if (!CaptureExp(binaryOpExp.Operand0, context))
                    return false;
            }

            if (!CaptureExp(binaryOpExp.Operand1, context))
                return false;

            return true;
        }

        bool CaptureCallExp(S.CallExp callExp, CaptureContext context) 
        {
            if (!CaptureExp(callExp.Callable, context))
                return false;

            foreach (var arg in callExp.Args)
            {
                if (!CaptureExp(arg, context))
                    return false;
            }

            return true;
        }

        bool CaptureLambdaExp(S.LambdaExp exp, CaptureContext context)
        {
            var prevBoundVars = context.GetBoundVars();

            context.AddBinds(exp.Params.Select(param => param.Name));

            if (!CaptureStmt(exp.Body, context))
                return false;            

            context.SetBoundVars(prevBoundVars);
            return true;
        }

        bool CaptureMemberCallExp(S.MemberCallExp exp, CaptureContext context)
        {
            // a.b.c(); 라면 a만 캡쳐하면 된다
            return CaptureExp(exp.Object, context);
        }

        bool CaptureMemberExp(S.MemberExp exp, CaptureContext context)
        {
            return CaptureExp(exp.Object, context);
        }

        bool CaptureListExp(S.ListExp exp, CaptureContext context)
        {
            foreach(var elem in exp.Elems)
            {
                if (!CaptureExp(elem, context))
                    return false;
            }

            return true;
        }

        bool CaptureExp(S.Exp exp, CaptureContext context)
        {
            return exp switch
            {
                S.IdentifierExp idExp => CaptureIdExp(idExp, context),
                S.BoolLiteralExp boolExp => CaptureBoolLiteralExp(boolExp, context),
                S.IntLiteralExp intExp => CaptureIntLiteralExp(intExp, context),
                S.StringExp stringExp => CaptureStringExp(stringExp, context),
                S.UnaryOpExp unaryOpExp => CaptureUnaryOpExp(unaryOpExp, context),
                S.BinaryOpExp binaryOpExp => CaptureBinaryOpExp(binaryOpExp, context),
                S.CallExp callExp => CaptureCallExp(callExp, context),
                S.LambdaExp lambdaExp => CaptureLambdaExp(lambdaExp, context),
                S.MemberCallExp memberCallExp => CaptureMemberCallExp(memberCallExp, context),
                S.MemberExp memberExp => CaptureMemberExp(memberExp, context),
                S.ListExp listExp => CaptureListExp(listExp, context),

                _ => throw new NotImplementedException()
            };
        }

        // entry
        public bool Capture(IEnumerable<string> initBoundVars, S.Stmt stmt, [NotNullWhen(true)] out CaptureResult? outCaptureResult)
        {
            var context = new CaptureContext(initBoundVars);

            if (!CaptureStmt(stmt, context))
            {
                outCaptureResult = null;
                return false;
            }

            // TODO: 일단 Capture this는 false이다
            outCaptureResult = new CaptureResult(context.GetNeedCaptures());
            return true;
        }
    }
}
