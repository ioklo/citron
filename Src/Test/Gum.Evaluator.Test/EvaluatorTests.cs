using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

            public async Task ExecuteAsync(string cmdText)
            {
                if (cmdText.StartsWith("#Sleep"))
                {
                    await Task.Delay(int.Parse(cmdText.Substring(6))).ConfigureAwait(true); // 그냥 ms단위
                    // Task.Yield() // 이거 잘 작동 안할 가능성
                }
                else
                {
                    lock (sb)
                    {
                        sb.Append(cmdText);
                    }
                }
            }

            public string GetOutput()
            {
                return sb.ToString();
            }
        }

        CommandStmt Sleep(int i)
        {
            return new CommandStmt(new StringExp(
                new TextStringExpElement("#Sleep"),
                new ExpStringExpElement(
                    new ExpInfo(
                        new CallInternalUnaryOperatorExp(
                            InternalUnaryOperator.ToString_Int_String,
                            new ExpInfo(new IntLiteralExp(i), TypeId.Int)),
                        TypeId.String
                    )
                )
            ));
        }

        CommandStmt PrintBoolCmdStmt(Exp exp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(new ExpInfo(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Bool_String,
                    new ExpInfo(exp, TypeId.Bool)),
                TypeId.String))));
        }

        CommandStmt PrintIntCmdStmt(Exp varExp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(new ExpInfo(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    new ExpInfo(varExp, TypeId.Int)), 
                TypeId.String))));
        }
        
        CommandStmt PrintStringCmdStmt(Exp varExp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(new ExpInfo(varExp, TypeId.String))));
        }

        CommandStmt PrintStringCmdStmt(string text)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(new ExpInfo(SimpleString(text), TypeId.String))));
        }

        StringExp SimpleString(string text)
        {
            return new StringExp(new TextStringExpElement(text));
        }

        ExpInfo IntLocalVar(string varName)
        {
            return new ExpInfo(new LocalVarExp(varName), TypeId.Int);
        }

        ExpInfo IntValue(int i)
        {
            return new ExpInfo(new IntLiteralExp(i), TypeId.Int);
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
            var stmts = new Stmt[] { new IfStmt(new BoolLiteralExp(true), PrintStringCmdStmt("True"), null) };
            var output = await EvalAsync(null, null, stmts);

            Assert.Equal("True", output);
        }

        [Fact]
        public async Task IfStmt_SelectElseBranchWhenConditionFalse()
        {
            var stmts = new Stmt[] { new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, PrintStringCmdStmt("False")) };
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

        [Fact]
        public async Task ExpForStmtInitializer_EvaluateAtStart()
        {
            var stmts = new Stmt[] {

                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Int, new IntLiteralExp(34)) })),

                PrintIntCmdStmt(new LocalVarExp("x")),

                new ForStmt(
                    new ExpForStmtInitializer(new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(12)), TypeId.Int)),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new ExpInfo(new LocalVarExp("x"), TypeId.Int), new ExpInfo(new IntLiteralExp(12), TypeId.Int)),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(1)), TypeId.Int),
                    PrintIntCmdStmt(new LocalVarExp("x"))),

                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("34121", output);
        }

        [Fact]
        public async Task ForStmt_EvalConditionAtStart()
        {
            var stmts = new Stmt[] {
                
                new ForStmt(
                    null,
                    new BoolLiteralExp(false),
                    null,
                    PrintStringCmdStmt("Wrong")),

                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAtEndOfBody()
        {
            var stmts = new Stmt[] {

                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Bool, new BoolLiteralExp(true)) })),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), TypeId.Bool),
                    PrintStringCmdStmt("Once")),

                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAndEvalCondExpAfterEvalContinueStmt()
        {
            var stmts = new Stmt[] {

                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Bool, new BoolLiteralExp(true)) })),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), TypeId.Bool),
                    new BlockStmt(
                        PrintStringCmdStmt("Once"),
                        ContinueStmt.Instance,
                        PrintStringCmdStmt("Wrong")
                    )
                ),

                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalBreakStmt()
        {
            var stmts = new Stmt[] {

                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Bool, new BoolLiteralExp(true)) })),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), TypeId.Bool),
                    new BlockStmt(
                        PrintStringCmdStmt("Once"),
                        BreakStmt.Instance,
                        PrintStringCmdStmt("Wrong")
                    )
                ),

                PrintBoolCmdStmt(new LocalVarExp("x")),
                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("OnceTrueCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalReturnStmt()
        {
            var stmts = new Stmt[] {

                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Bool, new BoolLiteralExp(true)) })),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), TypeId.Bool),
                    new BlockStmt(
                        PrintStringCmdStmt("Once"),
                        new ReturnStmt(new IntLiteralExp(2)),
                        PrintStringCmdStmt("Wrong")
                    )
                ),
                
                PrintStringCmdStmt("Wrong")
            };

            var (output, result) = await EvalAsyncWithRetValue(null, null, stmts);
            Assert.Equal("Once", output);
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task ContinueStmt_ContinuesInnerMostLoopStmt()
        {
            var stmts = new Stmt[] {

                new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new LocalVarDecl.Element("i", TypeId.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(2)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), TypeId.Int),

                    new BlockStmt(

                        PrintIntCmdStmt(new LocalVarExp("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new LocalVarDecl.Element("j", TypeId.Int, new IntLiteralExp(0)) })),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("j"), IntValue(2)),
                            new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("j")), TypeId.Int),
                            new BlockStmt(
                                PrintIntCmdStmt(new LocalVarExp("j")),
                                ContinueStmt.Instance,
                                PrintIntCmdStmt(new LocalVarExp("Wrong"))
                            )
                        )
                    )
                ),
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("001101", output);
        }

        [Fact]
        public async Task BreakStmt_ExitsInnerMostLoopStmt()
        {
            var stmts = new Stmt[] {

                new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new LocalVarDecl.Element("i", TypeId.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(2)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), TypeId.Int),

                    new BlockStmt(

                        PrintIntCmdStmt(new LocalVarExp("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new LocalVarDecl.Element("j", TypeId.Int, new IntLiteralExp(0)) })),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("j"), IntValue(2)),
                            new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("j")), TypeId.Int),
                            new BlockStmt(
                                PrintIntCmdStmt(new LocalVarExp("j")),
                                BreakStmt.Instance,
                                PrintIntCmdStmt(new LocalVarExp("Wrong"))
                            )
                        )
                    )
                ),
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("0010", output);
        }

        [Fact]
        public async Task ReturnStmt_ExitsFuncImmediately()
        {
            var funcId = new FuncId(0);

            var func = new Func(funcId, false, Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                new BlockStmt(
                    new ReturnStmt(null),
                    PrintStringCmdStmt("Wrong")
                )
            );

            var stmts = new Stmt[] { 
                new ExpStmt(new ExpInfo(new CallFuncExp(funcId, null, Enumerable.Empty<ExpInfo>()), TypeId.Void)),
                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(new[] { func }, null, stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ReturnStmt_ReturnProperlyInTopLevel()
        {
            var topLevelStmts = new Stmt[] { new ReturnStmt(new IntLiteralExp(34)) };
            var (_, retValue) = await EvalAsyncWithRetValue(null, null, topLevelStmts);

            Assert.Equal(34, retValue);
        }

        [Fact]
        public async Task ReturnStmt_SetReturnValueToCaller()
        {
            var funcId = new FuncId(0);

            var func = new Func(funcId, false, Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                new BlockStmt(
                    new ReturnStmt(new IntLiteralExp(77)),
                    PrintStringCmdStmt("Wrong")
                )
            );

            var stmts = new Stmt[] {
                PrintIntCmdStmt(new CallFuncExp(funcId, null, Enumerable.Empty<ExpInfo>())),
            };

            var output = await EvalAsync(new[] { func }, null, stmts);
            Assert.Equal("77", output);
        }

        // BlockStmt에서 LocalVar 범위 테스트는 LocalVarDeclStmt_OverlapsParentScope에서 처리함
        [Fact]
        public async Task BlockStmt_EvalInnerStatementsSequencially()
        {
            var stmts = new Stmt[] {
                PrintIntCmdStmt(new IntLiteralExp(1)),
                PrintIntCmdStmt(new IntLiteralExp(23)),
                PrintIntCmdStmt(new IntLiteralExp(4)),
            };

            var output = await EvalAsync(null, null, stmts);

            Assert.Equal("1234", output);
        }

        // BlankStmt do nothing.. nothing을 테스트하기는 힘들다

        [Fact]
        public async Task ExpStmt_EvalInnerExp()
        {
            var stmts = new Stmt[] {
                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Int, null) })),
                new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(3)), TypeId.Int)),
                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("3", output);
        }

        // Task Await
        [Fact]
        public async Task TaskStmt_EvalParallel()
        {
            Stmt PrintNumbersStmt(int count)
            {
                return new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new LocalVarDecl.Element("i", TypeId.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(count)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), TypeId.Int),
                    PrintIntCmdStmt(new LocalVarExp("i"))
                );
            }

            var stmts = new Stmt[] {
                new AwaitStmt(
                    new BlockStmt(
                        new TaskStmt(PrintNumbersStmt(5), new CaptureInfo(false, Enumerable.Empty<CaptureInfo.Element>())),
                        new TaskStmt(PrintNumbersStmt(5), new CaptureInfo(false, Enumerable.Empty<CaptureInfo.Element>()))
                    )
                )
            };

            var output = await EvalAsync(null, null, stmts);

            // 01234 01234 두개가 그냥 섞여 있을 것이다.

            char cur0 = '0';
            char cur1 = '0';

            foreach (var c in output)
            {
                if (c == cur0)
                    cur0++;
                else if (c == cur1)
                    cur1++;
                else
                {
                    Assert.True(false, "순서가 맞지 않습니다");
                    break;
                }
            }

            Assert.True(cur0 == '5' && cur1 == '5');
        }

        // Task Await
        [Fact]
        public async Task TaskStmt_CapturesLocalVariable()
        {
            Stmt PrintNumbersStmt()
            {
                return new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new LocalVarDecl.Element("i", TypeId.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntLocalVar("count")),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), TypeId.Int),
                    PrintIntCmdStmt(new LocalVarExp("i"))
                );
            }

            var stmts = new Stmt[] {
                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("count", TypeId.Int, new IntLiteralExp(5))})),

                new AwaitStmt(
                    new BlockStmt(
                        new TaskStmt(PrintNumbersStmt(), new CaptureInfo(false, new [] { new CaptureInfo.CopyLocalElement(TypeId.Int, "count") })),
                        new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("count"), new IntLiteralExp(4)), TypeId.Int)),
                        new TaskStmt(PrintNumbersStmt(), new CaptureInfo(false, new [] { new CaptureInfo.CopyLocalElement(TypeId.Int, "count") }))
                    )
                )
            };

            var output = await EvalAsync(null, null, stmts);

            // 01234 01234 두개가 그냥 섞여 있을 것이다.

            char cur0 = '0';
            char cur1 = '0';

            foreach (var c in output)
            {
                if (c == cur0)
                    cur0++;
                else if (c == cur1)
                    cur1++;
                else
                {
                    Assert.True(false, "순서가 맞지 않습니다");
                    break;
                }
            }

            Assert.True((cur0 == '5' && cur1 == '4') || (cur0 == '4' && cur1 == '5'));
        }


        // Async Await
        [Fact]
        public async Task AsyncStmt_EvalAsynchronously()
        {
            Stmt PrintNumbersStmt(int count)
            {
                return new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new LocalVarDecl.Element("i", TypeId.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(count)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), TypeId.Int),
                    new BlockStmt(
                        PrintIntCmdStmt(new LocalVarExp("i")),
                        Sleep(1)
                    )
                );
            }

            var stmts = new Stmt[] {
                new AwaitStmt(
                    new BlockStmt(
                        new AsyncStmt(PrintNumbersStmt(5), new CaptureInfo(false, Enumerable.Empty<CaptureInfo.Element>())),
                        new AsyncStmt(PrintNumbersStmt(5), new CaptureInfo(false, Enumerable.Empty<CaptureInfo.Element>()))
                    )
                )
            };

            var output = await EvalAsync(null, null, stmts);

            Assert.Equal("0011223344", output);
        }

        [Fact]
        public async Task YieldStmt_GenerateElementForInnerMostForeachStmt()
        {
            var seqFunc0Id = new SeqFuncId(0);
            var seqFunc1Id = new SeqFuncId(1);

            var seqFunc0 = new SeqFunc(seqFunc0Id, TypeId.Int, false, Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                new ForeachStmt(TypeId.Int, "elem", new ExpInfo(new CallSeqFuncExp(seqFunc1Id, null, Enumerable.Empty<ExpInfo>()), TypeId.Enumerable),
                    new BlockStmt(
                        PrintIntCmdStmt(new LocalVarExp("elem")),
                        new YieldStmt(new LocalVarExp("elem"))
                    )
                )
            );

            var seqFunc1 = new SeqFunc(seqFunc1Id, TypeId.Int, false, Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                new BlockStmt(
                    new YieldStmt(new IntLiteralExp(34)),
                    new YieldStmt(new IntLiteralExp(56))
                )
            );

            var stmts = new[] {                
                new ForeachStmt(TypeId.Int, "x", new ExpInfo(new CallSeqFuncExp(seqFunc0Id, null, Enumerable.Empty<ExpInfo>()), TypeId.Enumerable),
                    PrintIntCmdStmt(new LocalVarExp("x")))
            };

            var output = await EvalAsync(null, new[] { seqFunc0, seqFunc1 }, stmts);

            Assert.Equal("34345656", output);
        }

        // Exp
        [Fact]
        public Task ExternalGlobalVarExp_GetGlobalValue()
        {
            throw new NotImplementedException();
        }
        
        // PrivateGlobalVarExp 둘다 위에서 함
        // LocalVarExp

        [Fact]
        public Task StaticMemberExp_GetStaticMemberValue()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task StructMemberExp_GetStructMemberValue()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task ClassMemberExp_GetClassMemberValue()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task EnumMemberExp_GetEnumMemberValue()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public async Task BoolLiteralExp_MakesBoolValue()
        {
            var stmts = new[]
            {
                PrintBoolCmdStmt(new BoolLiteralExp(false)),
                PrintBoolCmdStmt(new BoolLiteralExp(true)),
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("FalseTrue", output);
        }

        [Fact]
        public async Task IntLiteralExp_MakesIntValue()
        {
            var stmts = new[]
            {
                PrintIntCmdStmt(new IntLiteralExp(-2)),                
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("-2", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingTextStringExpElements()
        {
            var stmts = new[]
            {
                PrintStringCmdStmt(new StringExp(
                    new TextStringExpElement("Hello "),
                    new TextStringExpElement("World"))),
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingStringExpElements()
        {
            var stmts = new Stmt[]
            {
                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.String, SimpleString("New ")) })),

                PrintStringCmdStmt(new StringExp(
                    new TextStringExpElement("Hello "),
                    new ExpStringExpElement(new ExpInfo(new LocalVarExp("x"), TypeId.String)),
                    new TextStringExpElement("World")))
            };

            var output = await EvalAsync(null, null, stmts);
            Assert.Equal("Hello New World", output);
        }

        [Fact]
        public async Task AssignExp_ReturnsValue()
        {
            var stmts = new Stmt[]
            {
                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("x", TypeId.Int, new IntLiteralExp(3)) })),
                new LocalVarDeclStmt(new LocalVarDecl(new [] { new LocalVarDecl.Element("y", TypeId.Int, new IntLiteralExp(4)) })),

                // y = x = 10;
                new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("y"), new AssignExp(new LocalVarExp("x"), new IntLiteralExp(10))), TypeId.Int)),
                PrintIntCmdStmt(new LocalVarExp("x")),
                PrintIntCmdStmt(new LocalVarExp("y"))
            };

            var output = await EvalAsync(null, null, stmts);

            Assert.Equal("1010", output);
        }

        // CallFunc
        // - Instance -> TODO: ThisExp가 없다
        // - Argument -> 
        //    순서
        // - TypeArguments -> TODO:
        // - return
        // [Fact]
        // public async Task CallFuncExp_Instance;

        // CallSeqFunc

        // CallValue
        // Lambda

        // ListIndexerExp
        // ListExp

        // NewEnum
        // NewStruct
        // NewClass
    }
}
