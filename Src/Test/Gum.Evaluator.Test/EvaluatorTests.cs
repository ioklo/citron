using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Type = Gum.IR0.Type;

namespace Gum.Runtime
{
    public class EvaluatorTests
    {
        class TestCommandProvider : ICommandProvider
        {
            StringBuilder sb;

            public TestCommandProvider()
            {
                sb = new StringBuilder();
            }

            public Task ExecuteAsync(string cmdText)
            {
                sb.Append(cmdText);

                return Task.CompletedTask;
            }

            public string GetOutput()
            {
                return sb.ToString();
            }
        }

        CommandStmt PrintIntCmdStmt(Exp varExp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(new ExpInfo(
                        new CallInternalUnaryOperatorExp(
                            InternalUnaryOperator.ToString_Int_String,
                            new ExpInfo(varExp, TypeId.Int)), TypeId.String))));
        }

        CommandStmt PrintStringCmdStmt(Exp varExp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(new ExpInfo(varExp, TypeId.String))));
        }

        StringExp SimpleString(string text)
        {
            return new StringExp(new TextStringExpElement(text));
        }

        public async Task<string> EvalAsync(IEnumerable<Func>? funcs, IEnumerable<SeqFunc>? seqFuncs, IEnumerable<Stmt> topLevelStmts)
        {
            var (output, _) = await EvalAsyncWithRetValue(funcs, seqFuncs, topLevelStmts);
            return output;
        }

        public async Task<(string Output, int RetValue)> EvalAsyncWithRetValue(IEnumerable<Func>? funcs, IEnumerable<SeqFunc>? seqFuncs, IEnumerable<Stmt> topLevelStmts)
        {
            var commandProvider = new TestCommandProvider();
            var evaluator = new Evaluator(commandProvider);

            var script = new Script(Enumerable.Empty<Type>(), funcs ?? Enumerable.Empty<Func>(), seqFuncs ?? Enumerable.Empty<SeqFunc>(), topLevelStmts);

            var retValue = await evaluator.EvalScriptAsync(script);

            return (commandProvider.GetOutput(), retValue);
        }

        [Fact]
        public async Task ReturnStmt_ReturnProperlyInTopLevel()
        {
            var topLevelStmts = new Stmt[] { new ReturnStmt(new IntLiteralExp(34)) };
            var (_, retValue) = await EvalAsyncWithRetValue(null, null, topLevelStmts);

            Assert.Equal(34, retValue);
        }

        [Fact]
        public async Task CommandStmt_WorksProperly()
        {
            var topLevelStmts = new Stmt[] { new CommandStmt(SimpleString("Hello World")), };
            var output = await EvalAsync(null, null, topLevelStmts);

            Assert.Equal("Hello World", output);
        }

        // Stmt
        // PrivateGlobalVarDeclStmt
        [Fact]
        public async Task PrivateGlobalVariableStmt_DeclProperly()
        {   
            var topLevelStmts = new Stmt[]
            {
                new PrivateGlobalVarDeclStmt(new []
                {
                    new PrivateGlobalVarDeclStmt.Element("x", TypeId.Int, new IntLiteralExp(3)),
                }),

                PrintIntCmdStmt(new PrivateGlobalVarExp("x")),
            };

            var output = await EvalAsync(null, null, topLevelStmts);
            Assert.Equal("3", output);
        }
        
        [Fact]
        public async Task PrivateGlobalVariableExp_GetGlobalValueInFunc()
        {
            var func0Id = new FuncId(0);
            var func0 = new Func(func0Id, false,Enumerable.Empty<string>(), Enumerable.Empty<string>(), new BlockStmt(
                PrintStringCmdStmt(new PrivateGlobalVarExp("x"))));

            var topLevelStmts = new Stmt[]
            {
                new PrivateGlobalVarDeclStmt(new [] { new PrivateGlobalVarDeclStmt.Element("x", TypeId.String, SimpleString("Hello")) }),

                new ExpStmt(new ExpInfo(new CallFuncExp(func0Id, null, Enumerable.Empty<ExpInfo>()), TypeId.Void))
            };

            var output = await EvalAsync(new[] { func0 }, null, topLevelStmts);

            Assert.Equal("Hello", output);
        }

        // SeqCall
        // seq int F(int x, int y) { yield x * 2; yield y + 3;
        // foreach(var e in F(1, 2))
        //    CommandStmt
        [Fact]
        public async Task CallSeqFuncExp_GenerateSequenceInForeach()
        {
            var seqFuncId = new SeqFuncId(0);

            var seqFunc = new SeqFunc(seqFuncId, TypeId.Int, false, Enumerable.Empty<string>(), new[] { "x", "y" }, new BlockStmt(

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Multiply_Int_Int_Int,
                        new ExpInfo(new LocalVarExp("x"), TypeId.Int),
                        new ExpInfo(new IntLiteralExp(2), TypeId.Int))),

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Add_Int_Int_Int,
                        new ExpInfo(new LocalVarExp("y"), TypeId.Int),
                        new ExpInfo(new IntLiteralExp(3), TypeId.Int)))));

            var stmts = new Stmt[]
            {
                new ForeachStmt(TypeId.Int, "e",
                    new ExpInfo(new CallSeqFuncExp(seqFuncId, null, new []{
                        new ExpInfo(new IntLiteralExp(1), TypeId.Int),
                        new ExpInfo(new IntLiteralExp(2), TypeId.Int) }), TypeId.Enumerable),

                    PrintIntCmdStmt(new LocalVarExp("e")))
            };

            var output = await EvalAsync(null, new[] { seqFunc }, stmts);

            Assert.Equal("25", output);
        }

        [Fact]
        public async Task LocalVarDeclStmt_OverlapsParentScope()
        {
            var stmts = new Stmt[]
            {   
                new LocalVarDeclStmt(new LocalVarDecl(new [] {
                    new LocalVarDecl.Element("x", TypeId.Int, new IntLiteralExp(23))
                })),

                new BlockStmt(

                    new LocalVarDeclStmt(new LocalVarDecl(new [] {
                        new LocalVarDecl.Element("x", TypeId.String, SimpleString("Hello"))})),

                    PrintStringCmdStmt(new LocalVarExp("x"))
                ),

                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("Hello23", output);
        }

        [Fact]
        public async Task LocalVarDeclStmt_OverlapsGlobalVar()
        {
            var stmts = new Stmt[]
            {
                new PrivateGlobalVarDeclStmt(new [] {
                    new PrivateGlobalVarDeclStmt.Element("x", TypeId.Int, new IntLiteralExp(23)) }),

                new BlockStmt(

                    new LocalVarDeclStmt(new LocalVarDecl(new [] {
                        new LocalVarDecl.Element("x", TypeId.String, SimpleString("Hello"))})),

                    PrintStringCmdStmt(new LocalVarExp("x"))
                ),

                PrintIntCmdStmt(new PrivateGlobalVarExp("x"))
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("Hello23", output);

        }

        // If
        [Fact]
        public async Task IfStmt_SelectThenBranchWhenConditionTrue()
        {
            var stmts = new Stmt[] { new IfStmt(new BoolLiteralExp(true), PrintStringCmdStmt(SimpleString("True")), null) };
            var output = await EvalAsync(null, null, stmts);

            Assert.Equal("True", output);
        }

        [Fact]
        public async Task IfStmt_SelectElseBranchWhenConditionFalse()
        {
            var stmts = new Stmt[] { new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, PrintStringCmdStmt(SimpleString("False"))) };
            var output = await EvalAsync(null, null, stmts);

            Assert.Equal("False", output);
        }

        // IfTestEnum
        [Fact]
        public Task IfTestEnumStmt_SelectThenBranchWhenTestPassed()
        {
            // 준비물, Enum Decl
            throw new NotImplementedException(); // 일단 제외
        }

        [Fact]
        public Task IfTestEnumStmt_OverrideVariableType()
        {
            // 준비물, EnumDecl, Enum Element Type
            throw new NotImplementedException();
        }

        [Fact]
        public Task IfTestClassStmt_SelectThenBranchWhenTestPassed()
        {
            // 준비물 ClassDecl
            throw new NotImplementedException();
        }

        [Fact]
        public Task TaskIfTestClassStmt_OverrideVariableType()
        {
            throw new NotImplementedException();
        }

        // var decl for initializer, 
        [Fact]
        public async Task VarDeclForStmtInitializer_DeclaresLocalVariable()
        {
            var stmts = new Stmt[] {

                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Int, new IntLiteralExp(34)) })),

                new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new LocalVarDecl.Element("x", TypeId.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new ExpInfo(new LocalVarExp("x"), TypeId.Int), new ExpInfo(new IntLiteralExp(0), TypeId.Int)),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(1)), TypeId.Int),
                    PrintIntCmdStmt(new LocalVarExp("x"))),

                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("034", output);
        }
    }
}
