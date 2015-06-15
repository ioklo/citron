using Gum.Core.AbstractSyntax;
using Gum.Core.IL;
using Gum.Core.IL.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler
{
    public class StmtEmitter : IStmtVisitor, IFuncStmtVisitor
    {
        CompilerContext context;
        EmitResult emitResult;

        public static EmitResult Emit(IStmt stmt, CompilerContext context)
        {
            StmtEmitter emitter = new StmtEmitter();
            emitter.context = context;
            stmt.Visit(emitter);
            return emitter.emitResult;
        }

        public void Visit(BlockStmt blockStmt)
        {
            context.PushLocalScope();

            var emitResult = new EmitResult();

            foreach(var stmt in blockStmt.Stmts)
            {
                EmitResult childEmitResult = StmtEmitter.Emit(stmt, context);                
                emitResult.PushBack(childEmitResult);
            }

            this.emitResult = emitResult;
            
            context.PopLocalScope();
        }

        public void Visit(VarDecl varDecl)
        {
            var emitResult = new EmitResult();

            foreach(var nameAndExp in varDecl.NameAndExps)
            {
                // 변수를 환경에 추가
                int varRegister = context.AddLocal(varDecl.Type.Value, nameAndExp.Name);

                if (nameAndExp.Exp != null)
                {
                    // TODO: 간단한 최적화, 

                    int childResult;
                    var childEmitResult = ValueEmitter.Emit(out childResult, nameAndExp.Exp, context);
                    emitResult.Push(childEmitResult);
                    emitResult.Push(new MoveReg(varRegister, childResult));
                }
            }

            this.emitResult = emitResult;
        }

        // for 
        public void Visit(ForStmt stmt)
        {            
            context.PushLocalScope();

            var initStmt = StmtEmitter.Emit(stmt.Initializer, context);

            int condResult;
            var condStmt = ValueEmitter.Emit(out condResult, stmt.CondExp, context);
            var bodyStmt = StmtEmitter.Emit(stmt.Body, context);
            var loopStmt = ValueEmitter.Emit(stmt.LoopExp, context);

            var emitResult = new EmitResult();

            emitResult.Push(initStmt);
            emitResult.PushLabel("cond");
            emitResult.Push(condStmt);
            emitResult.PushIfNotJump(condResult, "exit");
            emitResult.Push(bodyStmt);
            emitResult.PushLabel("continue");
            emitResult.Push(loopStmt);
            emitResult.PushJump("cond");
            emitResult.PushLabel("exit");
            emitResult.PushLabel("break");

            this.emitResult = emitResult;
        }

        public void Visit(WhileStmt stmt)
        {
            int condResult;
            var condEmitResult = ValueEmitter.Emit(out condResult, stmt.CondExp, context);
            var bodyEmitResult = StmtEmitter.Emit(stmt.Body, context);

            var emitResult = new EmitResult();

            emitResult.PushLabel("loop");
            emitResult.PushLabel("continue");
            emitResult.Push(condEmitResult);
            emitResult.PushIfNotJump(condResult, "exit");
            emitResult.Push(bodyEmitResult);
            emitResult.PushJump("loop");
            emitResult.PushLabel("exit");
            emitResult.PushLabel("break");

            this.emitResult = emitResult; 
        }

        public void Visit(DoWhileStmt stmt)
        {
            var bodyStmt = StmtEmitter.Emit(stmt.Body, context);

            int condResult = context.AddLocal();
            var condStmt = ValueEmitter.Emit(stmt.CondExp, context);

            var emitResult = new EmitResult();

            emitResult.PushLabel("loop");
            emitResult.Push(bodyStmt);
            emitResult.PushLabel("continue");
            emitResult.Push(condStmt);
            emitResult.PushIfNotJump(condResult, "exit");
            emitResult.PushJump("loop");
            emitResult.PushLabel("exit");
            emitResult.PushLabel("break");

            this.emitResult = emitResult;
        }

        public void Visit(IfStmt stmt)
        {
            int condResult;
            var condStmt = ValueEmitter.Emit(condResult, stmt.CondExp, context);

            var thenStmt = StmtEmitter.Emit(stmt.ThenStmt, context);

            if( stmt.ElseStmt == null)
            {
                var emitResult = new EmitResult();
                emitResult.Push(condStmt);
                emitResult.PushIfNotJump(condResult, "exit");
                emitResult.Push(thenStmt);
                emitResult.PushLabel("exit");

                this.emitResult = emitResult;
            }
            else
            {
                var elseStmt = StmtEmitter.Emit(stmt.ElseStmt, context);

                var emitResult = new EmitResult();
                emitResult.Push(condStmt);
                emitResult.PushIfNotJump(condResult, "else");
                emitResult.Push(thenStmt);
                emitResult.PushJump("exit");
                emitResult.PushLabel("else");
                emitResult.Push(elseStmt);
                emitResult.PushLabel("exit");

                this.emitResult = emitResult;
            }
        }

        public void Visit(ExpStmt stmt)
        {
            emitResult = ValueEmitter.Emit(stmt.Exp, context);
        }

        public void Visit(ReturnStmt stmt)
        {
            if (stmt.ReturnExp != null)
            {
                int returnVal;
                var returnExp = ValueEmitter.Emit(out returnVal, stmt.ReturnExp, context);

                var emitResult = new EmitResult();
                emitResult.Push(returnExp);
                emitResult.Push(new Return(returnVal));

                this.emitResult = emitResult;
            }
            else
            {
                var emitResult = new EmitResult();
                emitResult.Push(new Return(-1));

                this.emitResult = emitResult;
            }
        }

        public void Visit(ContinueStmt stmt)
        {
            emitResult = new EmitResult();
            emitResult.PushJump("continue");
        }

        public void Visit(BreakStmt stmt)
        {
            emitResult = new EmitResult();
            emitResult.PushJump("break");
        }        
    }
}
