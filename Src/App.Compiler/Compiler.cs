using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.AbstractSyntax;

using System.Diagnostics;
using IL = Gum.Core.IL;
using Gum.App.Compiler.Exception;
using Gum.Core.IL;
using Gum.Core.IL.Commands;
using Gum.Core.Runtime;

namespace Gum.App.Compiler
{
    public class Compiler : IREPLStmtVisitor, IFileUnitDeclVisitor
    {
        CompilerContext ctx;

        public Compiler(IL.Domain env)
        {
            ctx = new CompilerContext(env);
        }

        public void Visit(ExpStmt expStmt)
        {
            StmtEmitter.Emit(expStmt, ctx);
        }
        
        public void Visit(VarDecl vd)
        {
            // Global 변수 테이블에 등록 
            foreach (var nameExp in vd.NameAndExps)
            {
                object value = ConstantEvaluator.Visit(nameExp.Exp, ctx);
                ctx.AddGlobal(nameExp.Name, value);
            }
        }

        // Function 
        public void Visit(FuncDecl fd)
        {
            // 초기화 작업
            ctx.ClearJumpPoints();
            ctx.ClearLocal();
            ctx.ClearLoopScope();

            // 레지스터 할당: 변수 이름 => 레지스터 번호
            // 인자는 0번 레지스터 부터 쓴다고            
            foreach (FuncParameter tn in fd.Parameters)
            {
                ctx.AddLocal(tn.Name.Value);
            }

            IType retType;
            if (!ctx.Env.TryGetType(fd.ReturnType.Value, out retType))
                throw new InvalidOperationException();

            int retValCount = (retType == GlobalDomain.VoidType ) ? 0 : 1;

            // TODO: 이 부분 정비 Func 추가하기 위해 알아야 할 정보가 너무 많다
            foreach (var funcStmt in fd.BodyStmts)
                StmtEmitter.EmitFuncStmt(funcStmt, ctx);

            ctx.Env.AddVariable(fd.Name.Value, new IL.Function(fd.Name.Value, fd.Parameters.Count, ctx.MaxLocalCount, retValCount, ctx.EmitResult.Commands, ctx.EmitResult.JumpIndice));
        }
        
        // AST에서 IL로 변환, 타입체크
        public static Core.IL.Program Compile(IL.Domain env, string code)
        {
            Compiler compiler = new Compiler(env);

            Parser parser = new Parser();
            FileUnit fileUnit;
            if (!parser.ParseFileUnit(code, out fileUnit))
                return null;            

            TypeChecker typeChecker = new TypeChecker(compiler.ctx.TypeManager);
            typeChecker.Check(fileUnit);

            foreach (var decl in fileUnit.Decls)
                decl.Visit(compiler);

            // Stack 체크
            var prog = new IL.Program(compiler.ctx.Env);

            StackHeightChecker stackChecker = new StackHeightChecker();
            stackChecker.Check(prog);

            return prog;
        }

        //private void CompileClassDecl(ClassDecl classDecl, CompilerContext ctx)
        //{
        //    // to make 'TypeInfo's and put them into 'Program'
        //    // things needed during compilation and execution are different

        //    // throw new NotImplementedException();
        //}

        public static List<ICommand> CompileREPLStmt(Gum.Core.IL.Domain env, string code)
        {
            Compiler compiler = new Compiler(env);

            Parser parser = new Parser();
            IREPLStmt replStmt;
            if (!parser.ParseREPLStmt(code, out replStmt))
                return null;

            TypeChecker typeChecker = new TypeChecker(compiler.ctx.TypeManager);
            typeChecker.Check(replStmt);

            replStmt.Visit(compiler);

            // Stack 체크
            // var prog = new IL.Program(ctx.Env);

            // StackHeightChecker stackChecker = new StackHeightChecker();
            // stackChecker.Check(prog);

            return compiler.ctx.EmitResult.Commands;
        }


        
    }
}
