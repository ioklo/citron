using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.App.Compiler.AST;
using System.Diagnostics;
using IL = Gum.Core.IL;
using Gum.App.Compiler.Exception;

namespace Gum.App.Compiler
{
    public class Compiler
    {
        CompilerContext ctx;
        Emitter emitter;
        ConstantEvaluator constEvaluator;

        public Compiler()
        {
            ctx = new CompilerContext();
            emitter = new Emitter(ctx);
            constEvaluator = new ConstantEvaluator(ctx);
        }
        
        public void CompileVarDecl(VarDecl vd, CompilerContext ctx)
        {
            // Global 변수 테이블에 등록 
            foreach (var nameExp in vd.NameAndExps)
            {
                object value = nameExp.Exp.Visit(constEvaluator);
                ctx.AddGlobal(nameExp.Name, value);
            }
        }

        // Function 
        public void CompileFuncDecl(FuncDecl fd, CompilerContext ctx)
        {
            // 초기화 작업
            ctx.ClearJumpPoints();
            ctx.ClearLocal();
            ctx.ClearLoopScope();

            // 레지스터 할당: 변수 이름 => 레지스터 번호
            // 인자는 0번 레지스터 부터 쓴다고            
            foreach (TypeAndName tn in fd.Parameters)
            {
                ctx.AddLocal(tn.Name);
            }

            int retValCount = (ctx.TypeManager.Types[fd.ReturnTypeName] == ctx.TypeManager.VoidType ) ? 0 : 1;

            if (fd.Body != null)
            {
                // TODO: 이 부분 정비 Func 추가하기 위해 알아야 할 정보가 너무 많다
                fd.Body.Visit(emitter);
                ctx.Program.AddFunc(fd.Name, fd.Parameters.Count, ctx.MaxLocalCount, retValCount, ctx.EmitResult.Commands, ctx.EmitResult.JumpIndice);
            }
            else
            {
                ctx.Program.AddExternFunc(fd.Name, fd.Parameters.Count, retValCount);
            }
        }
        
        // AST에서 IL로 변환, 타입체크
        public Core.IL.Program Compile(string code)
        {
            Parser parser = new Parser();
            Program prog;
            if (!parser.ParseProgram(code, out prog))
                return null;

            TypeChecker typeChecker = new TypeChecker(ctx.TypeManager);
            typeChecker.Check(prog);
            
            
            foreach (var decl in prog.Decls)
            {
                if (decl is ClassDecl)
                    CompileClassDecl((ClassDecl)decl, ctx);

                if (decl is FuncDecl)
                    CompileFuncDecl((FuncDecl)decl, ctx);

                else if (decl is VarDecl)
                    CompileVarDecl((VarDecl)decl, ctx);
            }

            // Stack 체크
            StackHeightChecker stackChecker = new StackHeightChecker();
            stackChecker.Check(ctx.Program);

            return ctx.Program;
        }

        private void CompileClassDecl(ClassDecl classDecl, CompilerContext ctx)
        {
            // to make 'TypeInfo's and put them into 'Program'
            // things needed during compilation and execution are different

            // throw new NotImplementedException();
        }
    }
}
