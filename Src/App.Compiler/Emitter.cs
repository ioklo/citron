using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.App.Compiler.AST;

namespace Gum.App.Compiler
{
    class Emitter : IStmtVisitor, IExpVisitor<int>
    {
        CompilerContext ctx;

        public Emitter(CompilerContext ctx)
        {
            this.ctx = ctx;
        }

        private int CurPoint { get { return ctx.EmitResult.Commands.Count; } }

        private int NewPoint()
        {
            return ctx.EmitResult.NewPoint();
        }

        private void SetPoint(int i, int n)
        {
            ctx.EmitResult.JumpIndice[i] = n;
        }

        private void AddInst(Core.IL.ICommand cmd)
        {
            ctx.EmitResult.Commands.Add(cmd);
        }

        private void Emit(IStmt stmt)
        {
            stmt.Visit(this);
        }

        private int Emit(IExp exp)
        {
            return exp.Visit(this);
        }

        // Block
        public void Visit(BlockStmt stmt)
        {
            // Scope 하나 더 만들기
            ctx.PushLocalScope();

            foreach (var child in stmt.Stmts)
                Emit(child);

            ctx.PopLocalScope();
        }

        public void Visit(VarDeclStmt stmt)
        {
            // 변수 추가
            foreach (var ne in stmt.Decl.NameAndExps)
            {
                // 변수를 환경에 추가
                int r = ctx.AddLocal(ne.Name);

                if (ne.Exp != null)
                {
                    Emit(ne.Exp);
                    AddInst(new Core.IL.StoreLocal(r));
                }
            }
        }

        // for 
        public void Visit(ForStmt stmt)
        {
            ctx.PushLocalScope();

            var bodyPoint = NewPoint();
            var continuePoint = NewPoint();
            var breakPoint = NewPoint();
            
            Emit(stmt.Initializer);
            Emit(stmt.CondExp);
            AddInst(new Core.IL.IfNotJump(breakPoint));

            ctx.PushLoopScope(continuePoint, breakPoint);
            var bodyPt = CurPoint;
            Emit(stmt.Body);
            ctx.PopLoopScope();

            var contPt = CurPoint;
            int stackCount = Emit(stmt.LoopExp);

            for (int t = 0; t < stackCount; t++)
                AddInst(new Core.IL.Pop());

            Emit(stmt.CondExp);
            AddInst(new Core.IL.IfJump(bodyPoint));

            var breakPt = ctx.EmitResult.Commands.Count;

            SetPoint(bodyPoint, bodyPt);
            SetPoint(continuePoint, contPt);
            SetPoint(breakPoint, breakPt);

            ctx.PopLocalScope();
        }

        public void Visit(WhileStmt stmt)
        {            
            // cp: cond
            //     body
            //     jump cp
            // bp: exit
            var continuePoint = NewPoint();
            var breakPoint = NewPoint();

            var contPt = CurPoint;
            Emit(stmt.CondExp);
            AddInst(new Core.IL.IfNotJump(breakPoint));

            ctx.PushLoopScope(continuePoint, breakPoint);
            Emit(stmt.Body);
            ctx.PopLoopScope();
            
            AddInst(new Core.IL.Jump(continuePoint));
            var breakPt = CurPoint;

            SetPoint(continuePoint, contPt);
            SetPoint(breakPoint, breakPt);
        }

        public void Visit(DoWhileStmt stmt)
        {
            var entryPoint = NewPoint();
            var continuePoint = NewPoint();
            var breakPoint = NewPoint();            
            
            // ep: body
            // cp: cond 
            //     condjump ep
            // bp: exit 
            
            ctx.PushLoopScope(continuePoint, breakPoint);
            var entryPt = CurPoint;
            Emit(stmt.Body);
            ctx.PopLoopScope();

            var contPt = CurPoint;
            Emit(stmt.CondExp);                                        
            AddInst(new Core.IL.IfJump(entryPoint));

            var breakPt = CurPoint;           

            SetPoint(entryPoint, entryPt);
            SetPoint(continuePoint, contPt);
            SetPoint(breakPoint, breakPt);
        }

        public void Visit(IfStmt stmt)
        {
            var thenPoint = NewPoint();
            var exitPoint = NewPoint();

            int thenPt;
            Emit(stmt.CondExp);

            if (stmt.ElseStmt != null)
            {
                var elsePoint = NewPoint();
                AddInst(new Core.IL.IfNotJump(elsePoint));

                thenPt = CurPoint;
                Emit(stmt.ThenStmt);
                AddInst(new Core.IL.Jump(exitPoint));

                var elsePt = CurPoint;
                Emit(stmt.ElseStmt);
                SetPoint(elsePoint, elsePt);
            }
            else
            {
                AddInst(new Core.IL.IfNotJump(exitPoint));
                thenPt = CurPoint;
                Emit(stmt.ThenStmt);
            }
            
            var exitPt = CurPoint;

            SetPoint(thenPoint, thenPt);
            SetPoint(exitPoint, exitPt);
        }

        public void Visit(NullStmt stmt)
        {
        }

        public void Visit(ExpStmt stmt)
        {
            // exp를 방문하고..
            ctx.PushLocalScope();
            int stackCount = Emit(stmt.Exp);
            ctx.PopLocalScope();
            
            for (int t = 0; t < stackCount; t++)
                AddInst(new Core.IL.Pop());
        }

        public void Visit(ReturnStmt stmt)
        {
            Emit(stmt.ReturnExp);
            AddInst(new Core.IL.Return());
        }

        public void Visit(ContinueStmt stmt)
        {
            int contPoint = ctx.ContinuePoint;

            // TODO: 루프 안이 아니므로 에러처리
            if (contPoint == -1)
                throw new NotImplementedException();
            
            AddInst(new Core.IL.Jump(contPoint));
        }

        public void Visit(BreakStmt stmt)
        {
            int breakPoint = ctx.BreakPoint;

            // TODO: 루프 안이 아니므로 에러처리
            if (breakPoint == -1)
                throw new NotImplementedException();

            AddInst(new Core.IL.Jump(breakPoint));
        }

        // assignment expression
        public int Visit(AssignExp exp)
        {
            // searching for the variable from locals
            // TODO: we should find the variable from 'this' pointer. 
            // compiler will add 'this' when the variable has implicit 'this' indication.
            int localIndex = ctx.GetLocal(exp.Var.Name);

            if (exp.Var.IndexOffsets.Count != 0)
            {
                // TODO: just doing LoadLocal because we know that there's only a reference object.
                //       when we introduce a value object later, we should change the strategy.
                Emit(exp.Exp);
                AddInst(new Core.IL.Dup());
                AddInst(new Core.IL.LoadLocal(localIndex));

                for (int t = 0; t < exp.Var.IndexOffsets.Count - 1; t++)
                    AddInst(new Core.IL.LoadField(exp.Var.IndexOffsets[t]));

                AddInst(new Core.IL.StoreField(exp.Var.IndexOffsets[exp.Var.IndexOffsets.Count - 1]));
            }
            else
            {
                Emit(exp.Exp);
                AddInst(new Core.IL.StoreLocal(localIndex));

                // exp는 끝에 Value를 남겨야 한다
                AddInst(new Core.IL.LoadLocal(localIndex));
            }

            return 1;
        }

        public int Visit(VariableExp exp)
        {
            var index = ctx.GetLocal(exp.Name);

            if (index == -1)
            {
                object val;
                if (ctx.GetGlobal(exp.Name, out val))
                    AddInst(new Core.IL.Push(val));
                else
                    throw new NotImplementedException();
            }
            else if (exp.Offsets.Count != 0)
            {
                AddInst(new Core.IL.LoadLocal(index));

                foreach (var idx in exp.IndexOffsets)
                    AddInst(new Core.IL.LoadField(idx));
            }
            else
            {
                AddInst(new Core.IL.LoadLocal(index));
            }

            return 1;
        }

        public int Visit(IntegerExp exp)
        {
            AddInst(new Core.IL.Push(exp.Value));
            return 1;
        }

        public int Visit(StringExp exp)
        {
            AddInst(new Core.IL.Push(exp.Value));
            return 1;
        }

        public int Visit(BoolExp exp)
        {
            AddInst(new Core.IL.Push(exp.Value));
            return 1;
        }

        Core.IL.OperatorKind ConvertBinOperation(BinaryExpKind kind)
        {
            switch (kind)
            {
                case BinaryExpKind.Equal: return Core.IL.OperatorKind.Equal;
                case BinaryExpKind.NotEqual: return Core.IL.OperatorKind.NotEqual;
                case BinaryExpKind.And: return Core.IL.OperatorKind.And;
                case BinaryExpKind.Or: return Core.IL.OperatorKind.Or;
                case BinaryExpKind.Add: return Core.IL.OperatorKind.Add;
                case BinaryExpKind.Sub: return Core.IL.OperatorKind.Sub;
                case BinaryExpKind.Mul: return Core.IL.OperatorKind.Mul;
                case BinaryExpKind.Div: return Core.IL.OperatorKind.Div;
                case BinaryExpKind.Mod: return Core.IL.OperatorKind.Mod;
                case BinaryExpKind.Less: return Core.IL.OperatorKind.Less;
                case BinaryExpKind.Greater: return Core.IL.OperatorKind.Greater;
                case BinaryExpKind.LessEqual: return Core.IL.OperatorKind.LessEqual;
                case BinaryExpKind.GreaterEqual: return Core.IL.OperatorKind.GreaterEqual;
            }

            throw new NotSupportedException();
        }

        Core.IL.OperatorKind ConvertUnaryOperation(UnaryExpKind kind)
        {
            switch (kind)
            {
                case UnaryExpKind.Neg: return Core.IL.OperatorKind.Neg;
                case UnaryExpKind.Not: return Core.IL.OperatorKind.Not;
            }

            throw new NotSupportedException();
        }

        public int Visit(BinaryExp exp)
        {            
            var binOpKind = ConvertBinOperation(exp.Operation);

            // Short Circuit And support 
            if (binOpKind == Core.IL.OperatorKind.And)
            {
                var exitPoint = NewPoint();

                // (a && b) means (if (a) then b)
                Emit(exp.Operand1);
                AddInst(new Core.IL.Dup()); // duplicate
                AddInst(new Core.IL.IfJump(exitPoint));
                AddInst(new Core.IL.Pop()); // duplicate and pop
                Emit(exp.Operand2);

                SetPoint(exitPoint, CurPoint);
                return 1;
            }

            // Short Circuit Or support 
            if (binOpKind == Core.IL.OperatorKind.Or)
            {
                // ( a || b ) means (if (!a) then b )

                var exitPoint = NewPoint();
                Emit(exp.Operand1);
                AddInst(new Core.IL.Dup()); // duplicate
                AddInst(new Core.IL.IfNotJump(exitPoint));
                AddInst(new Core.IL.Pop()); // duplicate and pop
                Emit(exp.Operand2);

                SetPoint(exitPoint, CurPoint);
                return 1;
            }

            // 1 넣고, 2 넣고, cmd 넣고
            Emit(exp.Operand1);
            Emit(exp.Operand2);
            AddInst(new Core.IL.Operator(binOpKind));

            return 1;
        }

        public int Visit(UnaryExp exp)
        {
            // ++, -- 처리
            if (exp.Operation == UnaryExpKind.PrefixInc || exp.Operation == UnaryExpKind.PrefixDec ||
                exp.Operation == UnaryExpKind.PostfixInc || exp.Operation == UnaryExpKind.PostfixDec)
            {
                VariableExp varExp = exp.Operand as VariableExp;
                if (varExp == null)
                {
                    // location에 대해서만 ++이 가능하다
                    throw new InvalidOperationException();
                }

                if (varExp.Offsets.Count != 0)
                    throw new NotImplementedException();

                int localIndex = ctx.GetLocal(varExp.Name);

                if (exp.Operation == UnaryExpKind.PostfixInc || exp.Operation == UnaryExpKind.PostfixDec)
                    AddInst(new Core.IL.LoadLocal(localIndex));

                AddInst(new Core.IL.LoadLocal(localIndex));
                AddInst(new Core.IL.Push(1));
                if (exp.Operation == UnaryExpKind.PrefixInc || exp.Operation == UnaryExpKind.PostfixInc)
                    AddInst(new Core.IL.Operator(Core.IL.OperatorKind.Add));
                else if (exp.Operation == UnaryExpKind.PrefixDec || exp.Operation == UnaryExpKind.PostfixDec)
                    AddInst(new Core.IL.Operator(Core.IL.OperatorKind.Sub));
                AddInst(new Core.IL.StoreLocal(localIndex));

                if (exp.Operation == UnaryExpKind.PrefixInc || exp.Operation == UnaryExpKind.PrefixDec)
                    AddInst(new Core.IL.LoadLocal(localIndex));
            }
            else
            {
                Emit(exp.Operand);
                AddInst(new Core.IL.Operator(ConvertUnaryOperation(exp.Operation)));
            }

            return 1;
        }

        // Call, 후에 ExternCall은 Call로 바뀐다
        public int Visit(CallExp exp)
        {
            Core.IL.FuncInfo info = ctx.Program.GetFuncInfo(exp.FuncName);

            foreach (var argExp in exp.Args)
                Emit(argExp);

            if (info == null)
                throw new InvalidOperationException();

            AddInst(new Core.IL.StaticCall(info));

            return info.RetValCount;
        }

        public int Visit(NewExp ne)
        {
            AddInst(new Core.IL.New(ctx.Program.Types[ne.TypeName]));
            // TODO: 생성자 부르는 부분
            // AddInst(new IL.StaticCall(ctx));
            return 1;
        }
        

    }
}
