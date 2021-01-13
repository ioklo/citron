using Gum.IR0;
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Gum.IR0.Runtime
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
                    new CallInternalUnaryOperatorExp(
                        InternalUnaryOperator.ToString_Int_String,
                        new ExpInfo(new IntLiteralExp(i), Type.Int))                    
                )
            ));
        }

        CommandStmt PrintBoolCmdStmt(Exp exp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Bool_String,
                    new ExpInfo(exp, Type.Bool))
            )));
        }

        LocalVarDeclStmt SimpleLocalVarDecl(string name, Type typeId, Exp? initExp)
        {
            return new LocalVarDeclStmt(new LocalVarDecl(new[] { new VarDeclElement(name, typeId, initExp) }));
        }

        CommandStmt PrintIntCmdStmt(Exp varExp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    new ExpInfo(varExp, Type.Int))
            )));
        }
        
        CommandStmt PrintStringCmdStmt(Exp varExp)
        {
            return new CommandStmt(new StringExp(new ExpStringExpElement(varExp)));
        }

        CommandStmt PrintStringCmdStmt(string text)
        {
            return new CommandStmt(new StringExp(new TextStringExpElement(text)));
        }

        StringExp SimpleString(string text)
        {
            return new StringExp(new TextStringExpElement(text));
        }

        ExpInfo LocalVar(string varName, Type typeId)
            => new ExpInfo(new LocalVarExp(varName), typeId);

        ExpInfo IntLocalVar(string varName)
        {
            return new ExpInfo(new LocalVarExp(varName), Type.Int);
        }

        ExpInfo IntValue(int i)
        {
            return new ExpInfo(new IntLiteralExp(i), Type.Int);
        }

        public async Task<string> EvalAsync(IEnumerable<FuncDecl>? funcDecls, IEnumerable<Stmt> topLevelStmts)
        {
            var (output, _) = await EvalAsyncWithRetValue(funcDecls, topLevelStmts);
            return output;
        }

        public async Task<(string Output, int RetValue)> EvalAsyncWithRetValue(IEnumerable<FuncDecl>? optionalFuncDecls, IEnumerable<Stmt> topLevelStmts)
        {
            // 입력 검사
            IEnumerable<FuncDecl> funcDecls = optionalFuncDecls ?? Array.Empty<FuncDecl>();            

            // FuncDeclId validation
            int i = 0;
            foreach (var funcDecl in funcDecls)
            {
                Assert.Equal(i, funcDecl.Id.Value);
                i++;
            }

            var commandProvider = new TestCommandProvider();
            var evaluator = new Evaluator(commandProvider);

            var script = new Script(Array.Empty<TypeDecl>(), funcDecls, topLevelStmts);

            var retValue = await evaluator.EvalScriptAsync(script);

            return (commandProvider.GetOutput(), retValue);
        }        

        [Fact]
        public async Task CommandStmt_WorksProperly()
        {
            var topLevelStmts = new Stmt[] { new CommandStmt(SimpleString("Hello World")), };
            var output = await EvalAsync(null, topLevelStmts);

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
                    new VarDeclElement("x", Type.Int, new IntLiteralExp(3)),
                }),

                PrintIntCmdStmt(new PrivateGlobalVarExp("x")),
            };

            var output = await EvalAsync(null, topLevelStmts);
            Assert.Equal("3", output);
        }
        
        [Fact]
        public async Task PrivateGlobalVariableExp_GetGlobalValueInFunc()
        {
            var func0Id = new FuncDeclId(0);
            var func0 = new FuncDecl.Normal(func0Id, false,Array.Empty<string>(), Array.Empty<string>(), new BlockStmt(
                PrintStringCmdStmt(new PrivateGlobalVarExp("x"))));

            var topLevelStmts = new Stmt[]
            {
                new PrivateGlobalVarDeclStmt(new [] { new VarDeclElement("x", Type.String, SimpleString("Hello")) }),

                new ExpStmt(new ExpInfo(new CallFuncExp(func0Id, Array.Empty<Type>(), null, Array.Empty<ExpInfo>()), Type.Void))
            };

            var output = await EvalAsync(new [] { func0 }, topLevelStmts);

            Assert.Equal("Hello", output);
        }

        // SeqCall
        // seq int F(int x, int y) { yield x * 2; yield y + 3;
        // foreach(var e in F(1, 2))
        //    CommandStmt
        [Fact]
        public async Task CallSeqFuncExp_GenerateSequencesInForeach()
        {
            var funcId = new FuncDeclId(0);

            var seqFunc = new FuncDecl.Sequence(funcId, Type.Int, false, Array.Empty<string>(), new[] { "x", "y" }, new BlockStmt(

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Multiply_Int_Int_Int,
                        new ExpInfo(new LocalVarExp("x"), Type.Int),
                        new ExpInfo(new IntLiteralExp(2), Type.Int))),

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Add_Int_Int_Int,
                        new ExpInfo(new LocalVarExp("y"), Type.Int),
                        new ExpInfo(new IntLiteralExp(3), Type.Int)))));

            var stmts = new Stmt[]
            {
                new ForeachStmt(Type.Int, "e",
                    new ExpInfo(new CallSeqFuncExp(funcId, Array.Empty<Type>(), null, new []{
                        new ExpInfo(new IntLiteralExp(1), Type.Int),
                        new ExpInfo(new IntLiteralExp(2), Type.Int) }), Type.Enumerable(Type.Int)),

                    PrintIntCmdStmt(new LocalVarExp("e")))
            };

            var output = await EvalAsync(new[] { seqFunc }, stmts);

            Assert.Equal("25", output);
        }

        [Fact]
        public async Task LocalVarDeclStmt_OverlapsParentScope()
        {
            var stmts = new Stmt[]
            {   
                SimpleLocalVarDecl("x", Type.Int, new IntLiteralExp(23)),

                new BlockStmt(

                    SimpleLocalVarDecl("x", Type.String, SimpleString("Hello")),

                    PrintStringCmdStmt(new LocalVarExp("x"))
                ),

                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("Hello23", output);
        }

        [Fact]
        public async Task LocalVarDeclStmt_OverlapsGlobalVar()
        {
            var stmts = new Stmt[]
            {
                new PrivateGlobalVarDeclStmt(new [] {
                    new VarDeclElement("x", Type.Int, new IntLiteralExp(23)) }),

                new BlockStmt(

                    SimpleLocalVarDecl("x", Type.String, SimpleString("Hello")),

                    PrintStringCmdStmt(new LocalVarExp("x"))
                ),

                PrintIntCmdStmt(new PrivateGlobalVarExp("x"))
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("Hello23", output);

        }

        // If
        [Fact]
        public async Task IfStmt_SelectThenBranchWhenConditionTrue()
        {
            var stmts = new Stmt[] { new IfStmt(new BoolLiteralExp(true), PrintStringCmdStmt("True"), null) };
            var output = await EvalAsync(null, stmts);

            Assert.Equal("True", output);
        }

        [Fact]
        public async Task IfStmt_SelectElseBranchWhenConditionFalse()
        {
            var stmts = new Stmt[] { new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, PrintStringCmdStmt("False")) };
            var output = await EvalAsync(null, stmts);

            Assert.Equal("False", output);
        }

        // IfTestEnum
        [Fact]
        public Task IfTestEnumStmt_SelectThenBranchWhenTestPassed()
        {
            throw new PrerequisiteRequiredException(Prerequisite.IfTestEnumStmt);
        }

        [Fact]
        public Task IfTestEnumStmt_OverrideVariableType()
        {
            throw new PrerequisiteRequiredException(Prerequisite.IfTestEnumStmt);            
        }

        [Fact]
        public Task IfTestClassStmt_SelectThenBranchWhenTestPassed()
        {
            throw new PrerequisiteRequiredException(Prerequisite.IfTestClassStmt);
        }

        [Fact]
        public Task TaskIfTestClassStmt_OverrideVariableType()
        {
            throw new PrerequisiteRequiredException(Prerequisite.IfTestClassStmt);
        }

        // var decl for initializer, 
        [Fact]
        public async Task VarDeclForStmtInitializer_DeclaresLocalVariable()
        {
            var stmts = new Stmt[] {
                
                SimpleLocalVarDecl("x", Type.Int, new IntLiteralExp(34)),

                new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new VarDeclElement("x", Type.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new ExpInfo(new LocalVarExp("x"), Type.Int), new ExpInfo(new IntLiteralExp(0), Type.Int)),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(1)), Type.Int),
                    PrintIntCmdStmt(new LocalVarExp("x"))),

                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("034", output);
        }

        [Fact]
        public async Task ExpForStmtInitializer_EvaluateAtStart()
        {
            var stmts = new Stmt[] {
                
                SimpleLocalVarDecl("x", Type.Int, new IntLiteralExp(34)),

                PrintIntCmdStmt(new LocalVarExp("x")),

                new ForStmt(
                    new ExpForStmtInitializer(new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(12)), Type.Int)),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new ExpInfo(new LocalVarExp("x"), Type.Int), new ExpInfo(new IntLiteralExp(12), Type.Int)),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(1)), Type.Int),
                    PrintIntCmdStmt(new LocalVarExp("x"))),

                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, stmts);
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

            var output = await EvalAsync(null, stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAtEndOfBody()
        {
            var stmts = new Stmt[] {
                
                SimpleLocalVarDecl("x", Type.Bool, new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), Type.Bool),
                    PrintStringCmdStmt("Once")),

                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAndEvalCondExpAfterEvalContinueStmt()
        {
            var stmts = new Stmt[] {

                
                SimpleLocalVarDecl("x", Type.Bool, new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), Type.Bool),
                    new BlockStmt(
                        PrintStringCmdStmt("Once"),
                        ContinueStmt.Instance,
                        PrintStringCmdStmt("Wrong")
                    )
                ),

                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalBreakStmt()
        {
            var stmts = new Stmt[] {
                                
                SimpleLocalVarDecl("x", Type.Bool, new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), Type.Bool),
                    new BlockStmt(
                        PrintStringCmdStmt("Once"),
                        BreakStmt.Instance,
                        PrintStringCmdStmt("Wrong")
                    )
                ),

                PrintBoolCmdStmt(new LocalVarExp("x")),
                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("OnceTrueCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalReturnStmt()
        {
            var stmts = new Stmt[] {
                
                SimpleLocalVarDecl("x", Type.Bool, new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LocalVarExp("x"),
                    new ExpInfo(new AssignExp(new LocalVarExp("x"), new BoolLiteralExp(false)), Type.Bool),
                    new BlockStmt(
                        PrintStringCmdStmt("Once"),
                        new ReturnStmt(new IntLiteralExp(2)),
                        PrintStringCmdStmt("Wrong")
                    )
                ),
                
                PrintStringCmdStmt("Wrong")
            };

            var (output, result) = await EvalAsyncWithRetValue(null, stmts);
            Assert.Equal("Once", output);
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task ContinueStmt_ContinuesInnerMostLoopStmt()
        {
            var stmts = new Stmt[] {

                new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new VarDeclElement("i", Type.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(2)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), Type.Int),

                    new BlockStmt(

                        PrintIntCmdStmt(new LocalVarExp("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new VarDeclElement("j", Type.Int, new IntLiteralExp(0)) })),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("j"), IntValue(2)),
                            new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("j")), Type.Int),
                            new BlockStmt(
                                PrintIntCmdStmt(new LocalVarExp("j")),
                                ContinueStmt.Instance,
                                PrintIntCmdStmt(new LocalVarExp("Wrong"))
                            )
                        )
                    )
                ),
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("001101", output);
        }

        [Fact]
        public async Task BreakStmt_ExitsInnerMostLoopStmt()
        {
            var stmts = new Stmt[] {

                new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new VarDeclElement("i", Type.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(2)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), Type.Int),

                    new BlockStmt(

                        PrintIntCmdStmt(new LocalVarExp("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(new LocalVarDecl(new [] {new VarDeclElement("j", Type.Int, new IntLiteralExp(0)) })),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("j"), IntValue(2)),
                            new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("j")), Type.Int),
                            new BlockStmt(
                                PrintIntCmdStmt(new LocalVarExp("j")),
                                BreakStmt.Instance,
                                PrintIntCmdStmt(new LocalVarExp("Wrong"))
                            )
                        )
                    )
                ),
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("0010", output);
        }

        [Fact]
        public async Task ReturnStmt_ExitsFuncImmediately()
        {
            var funcId = new FuncDeclId(0);

            var func = new FuncDecl.Normal(funcId, false, Array.Empty<string>(), Array.Empty<string>(),
                new BlockStmt(
                    new ReturnStmt(null),
                    PrintStringCmdStmt("Wrong")
                )
            );

            var stmts = new Stmt[] { 
                new ExpStmt(new ExpInfo(new CallFuncExp(funcId, Array.Empty<Type>(), null, Array.Empty<ExpInfo>()), Type.Void)),
                PrintStringCmdStmt("Completed")
            };

            var output = await EvalAsync(new[] { func }, stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ReturnStmt_ReturnProperlyInTopLevel()
        {
            var topLevelStmts = new Stmt[] { new ReturnStmt(new IntLiteralExp(34)) };
            var (_, retValue) = await EvalAsyncWithRetValue(null, topLevelStmts);

            Assert.Equal(34, retValue);
        }

        [Fact]
        public async Task ReturnStmt_SetReturnValueToCaller()
        {
            var funcId = new FuncDeclId(0);

            var func = new FuncDecl.Normal(funcId, false, Array.Empty<string>(), Array.Empty<string>(),
                new BlockStmt(
                    new ReturnStmt(new IntLiteralExp(77)),
                    PrintStringCmdStmt("Wrong")
                )
            );

            var stmts = new Stmt[] {
                PrintIntCmdStmt(new CallFuncExp(funcId, Array.Empty<Type>(), null, Array.Empty<ExpInfo>())),
            };

            var output = await EvalAsync(new[] { func }, stmts);
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

            var output = await EvalAsync(null, stmts);

            Assert.Equal("1234", output);
        }

        // BlankStmt do nothing.. nothing을 테스트하기는 힘들다

        [Fact]
        public async Task ExpStmt_EvalInnerExp()
        {
            var stmts = new Stmt[] {
                SimpleLocalVarDecl("x", Type.Int, null),
                new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(3)), Type.Int)),
                PrintIntCmdStmt(new LocalVarExp("x"))
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("3", output);
        }

        // Task Await
        [Fact]
        public async Task TaskStmt_EvalParallel()
        {
            Stmt PrintNumbersStmt(int count)
            {
                return new ForStmt(
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new VarDeclElement("i", Type.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(count)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), Type.Int),
                    PrintIntCmdStmt(new LocalVarExp("i"))
                );
            }

            var stmts = new Stmt[] {
                new AwaitStmt(
                    new BlockStmt(
                        new TaskStmt(PrintNumbersStmt(5), new CaptureInfo(false, Array.Empty<CaptureInfo.Element>())),
                        new TaskStmt(PrintNumbersStmt(5), new CaptureInfo(false, Array.Empty<CaptureInfo.Element>()))
                    )
                )
            };

            var output = await EvalAsync(null, stmts);

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
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new VarDeclElement("i", Type.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntLocalVar("count")),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), Type.Int),
                    PrintIntCmdStmt(new LocalVarExp("i"))
                );
            }

            var stmts = new Stmt[] {                
                SimpleLocalVarDecl("count", Type.Int, new IntLiteralExp(5)),

                new AwaitStmt(
                    new BlockStmt(
                        new TaskStmt(PrintNumbersStmt(), new CaptureInfo(false, new [] { new CaptureInfo.Element(Type.Int, "count") })),
                        new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("count"), new IntLiteralExp(4)), Type.Int)),
                        new TaskStmt(PrintNumbersStmt(), new CaptureInfo(false, new [] { new CaptureInfo.Element(Type.Int, "count") }))
                    )
                )
            };

            var output = await EvalAsync(null, stmts);

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
                    new VarDeclForStmtInitializer(new LocalVarDecl(new[] { new VarDeclElement("i", Type.Int, new IntLiteralExp(0)) })),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, IntLocalVar("i"), IntValue(count)),
                    new ExpInfo(new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new LocalVarExp("i")), Type.Int),
                    new BlockStmt(
                        PrintIntCmdStmt(new LocalVarExp("i")),
                        Sleep(1)
                    )
                );
            }

            var stmts = new Stmt[] {
                new AwaitStmt(
                    new BlockStmt(
                        new AsyncStmt(PrintNumbersStmt(5), new CaptureInfo(false, Array.Empty<CaptureInfo.Element>())),
                        new AsyncStmt(PrintNumbersStmt(5), new CaptureInfo(false, Array.Empty<CaptureInfo.Element>()))
                    )
                )
            };

            var output = await EvalAsync(null, stmts);

            Assert.Equal("0011223344", output);
        }

        [Fact]
        public async Task YieldStmt_GenerateElementForInnerMostForeachStmt()
        {
            var seqFunc0Id = new FuncDeclId(0);
            var seqFunc1Id = new FuncDeclId(1);

            var seqFunc0 = new FuncDecl.Sequence(seqFunc0Id, Type.Int, false, Array.Empty<string>(), Array.Empty<string>(),
                new ForeachStmt(Type.Int, "elem", new ExpInfo(new CallSeqFuncExp(seqFunc1Id, Array.Empty<Type>(), null, Array.Empty<ExpInfo>()), Type.Enumerable(Type.Int)),
                    new BlockStmt(
                        PrintIntCmdStmt(new LocalVarExp("elem")),
                        new YieldStmt(new LocalVarExp("elem"))
                    )
                )
            );

            var seqFunc1 = new FuncDecl.Sequence(seqFunc1Id, Type.Int, false, Array.Empty<string>(), Array.Empty<string>(),
                new BlockStmt(
                    new YieldStmt(new IntLiteralExp(34)),
                    new YieldStmt(new IntLiteralExp(56))
                )
            );

            var stmts = new[] {                
                new ForeachStmt(Type.Int, "x", new ExpInfo(new CallSeqFuncExp(seqFunc0Id, Array.Empty<Type>(), null, Array.Empty<ExpInfo>()), Type.Enumerable(Type.Int)),
                    PrintIntCmdStmt(new LocalVarExp("x")))
            };

            var output = await EvalAsync(new[] { seqFunc0, seqFunc1 }, stmts);

            Assert.Equal("34345656", output);
        }

        // Exp
        [Fact]
        public Task ExternalGlobalVarExp_GetGlobalValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.External);
        }
        
        // PrivateGlobalVarExp 둘다 위에서 함
        // LocalVarExp

        [Fact]
        public Task StaticMemberExp_GetStaticMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Static);
        }

        [Fact]
        public Task StructMemberExp_GetStructMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Struct);
        }

        [Fact]
        public Task ClassMemberExp_GetClassMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Class);
        }

        [Fact]
        public Task EnumMemberExp_GetEnumMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }
        
        [Fact]
        public async Task BoolLiteralExp_MakesBoolValue()
        {
            var stmts = new[]
            {
                PrintBoolCmdStmt(new BoolLiteralExp(false)),
                PrintBoolCmdStmt(new BoolLiteralExp(true)),
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("FalseTrue", output);
        }

        [Fact]
        public async Task IntLiteralExp_MakesIntValue()
        {
            var stmts = new[]
            {
                PrintIntCmdStmt(new IntLiteralExp(-2)),                
            };

            var output = await EvalAsync(null, stmts);
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

            var output = await EvalAsync(null, stmts);
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingStringExpElements()
        {
            var stmts = new Stmt[]
            {
                SimpleLocalVarDecl("x", Type.String, SimpleString("New ")),

                PrintStringCmdStmt(new StringExp(
                    new TextStringExpElement("Hello "),
                    new ExpStringExpElement(new LocalVarExp("x")),
                    new TextStringExpElement("World")))
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("Hello New World", output);
        }

        [Fact]
        public async Task AssignExp_ReturnsValue()
        {
            var stmts = new Stmt[]
            {
                SimpleLocalVarDecl("x", Type.Int, new IntLiteralExp(3)),
                SimpleLocalVarDecl("y", Type.Int, new IntLiteralExp(4)),

                // y = x = 10;
                new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("y"), new AssignExp(new LocalVarExp("x"), new IntLiteralExp(10))), Type.Int)),
                PrintIntCmdStmt(new LocalVarExp("x")),
                PrintIntCmdStmt(new LocalVarExp("y"))
            };

            var output = await EvalAsync(null, stmts);

            Assert.Equal("1010", output);
        }

        [Fact]
        public Task CallFuncExp_EvaluatesInstanceExpAtStart()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CallFuncExp_PassesInstanceValueAsThisValue()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CallFuncExp_PassesTypeAgumentsProperly()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CallFuncExp_EvaluatesArgumentsInOrder()
        {
            var printFuncId = new FuncDeclId(0);
            var testFuncId = new FuncDeclId(1);

            var printFunc = new FuncDecl.Normal(printFuncId, false, Array.Empty<string>(), new[] { "x" },

                new BlockStmt(
                    PrintIntCmdStmt(new LocalVarExp("x")),
                    new ReturnStmt(new LocalVarExp("x"))
                )
            );

            var testFunc = new FuncDecl.Normal(testFuncId, false, Array.Empty<string>(), new[] { "i", "j", "k" },

                PrintStringCmdStmt("TestFunc")

            );

            ExpInfo MakePrintCall(int v) =>
                new ExpInfo(new CallFuncExp(printFuncId, Array.Empty<Type>(), null, new[] { new ExpInfo(new IntLiteralExp(v), Type.Int) }), Type.Int);


            var stmts = new Stmt[]
            {
                new ExpStmt(new ExpInfo(
                    new CallFuncExp(testFuncId, Array.Empty<Type>(), null, new [] {
                        MakePrintCall(1),
                        MakePrintCall(2),
                        MakePrintCall(3) }
                    ),
                    Type.Void)
                )
            };

            var output = await EvalAsync(new[] { printFunc, testFunc }, stmts);
            Assert.Equal("123TestFunc", output);
        }

        [Fact]
        public async Task CallFuncExp_ReturnsValueProperly()
        {
            var funcId = new FuncDeclId(0);
            var func = new FuncDecl.Normal(funcId, false, Array.Empty<string>(), Array.Empty<string>(), new ReturnStmt(SimpleString("Hello World")));

            var stmts = new Stmt[] { PrintStringCmdStmt(new CallFuncExp(funcId, Array.Empty<Type>(), null, Array.Empty<ExpInfo>())) };

            var output = await EvalAsync(new[] { func }, stmts);
            Assert.Equal("Hello World", output);
        }

        // CallSeqFunc
        [Fact]
        public async Task CallSeqFuncExp_ReturnsEnumerableType()
        {
            var seqFuncId = new FuncDeclId(0);
            var seqFunc = new FuncDecl.Sequence(seqFuncId, Type.String, false, Array.Empty<string>(), Array.Empty<string>(), BlankStmt.Instance); // return nothing

            var stmts = new Stmt[]
            {
                // 선언하자 마자 대입하기
                SimpleLocalVarDecl("x", Type.Enumerable(Type.String), new CallSeqFuncExp(seqFuncId, Array.Empty<Type>(), null, Array.Empty<ExpInfo>())),

                // TODO: 지금은 value로 얻은 Enumerable을 조작할 수 있는 방법이 없다. foreach에서만 내부적으로 조작할 수 있는데, 그렇게만 하도록 할 것인지 결정을 해야 한다

                // 로컬 변수에 새 값 대입하기
                new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("x"), new CallSeqFuncExp(seqFuncId, Array.Empty<Type>(), null, Array.Empty<ExpInfo>())), Type.Enumerable(Type.String))),
            };

            await EvalAsync(new[] { seqFunc }, stmts);

            // 에러가 안났으면 성공..
        }
        
        [Fact]
        public async Task CallValueExp_EvaluateCallableAndArgumentsInOrder()
        {
            var printFuncId = new FuncDeclId(0);
            var makeLambdaId = new FuncDeclId(1);

            var printFunc = new FuncDecl.Normal(printFuncId, false, Array.Empty<string>(), new[] { "x" },

                new BlockStmt(
                    PrintIntCmdStmt(new LocalVarExp("x")),
                    new ReturnStmt(new LocalVarExp("x"))
                )
            );

            var makeLambda = new FuncDecl.Normal(makeLambdaId, false, Array.Empty<string>(), Array.Empty<string>(),
                new BlockStmt(                    
                    PrintStringCmdStmt("MakeLambda"),
                    
                    new ReturnStmt(new LambdaExp(
                        new CaptureInfo(false, Array.Empty<CaptureInfo.Element>()),
                        new[] { "i", "j", "k" },
                        PrintStringCmdStmt("TestFunc")
                    ))
                )
            );
            
            ExpInfo MakePrintCall(int v) =>
                new ExpInfo(new CallFuncExp(printFuncId, Array.Empty<Type>(), null, new[] { new ExpInfo(new IntLiteralExp(v), Type.Int) }), Type.Int);


            var stmts = new Stmt[]
            {
                new ExpStmt(new ExpInfo(
                    new CallValueExp(
                        new ExpInfo(new CallFuncExp(makeLambdaId, Array.Empty<Type>(), null, Array.Empty<ExpInfo>()), Type.Lambda), 
                        new [] {
                            MakePrintCall(1),
                            MakePrintCall(2),
                            MakePrintCall(3) 
                        }
                    ),
                    Type.Void
                ))
            };

            var output = await EvalAsync(new[] { printFunc, makeLambda }, stmts);
            Assert.Equal("MakeLambda123TestFunc", output);
        }

        // Lambda
        [Fact]
        public Task LambdaExp_CapturesThisWithReferencing()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Class);
        }

        // Lambda Purity
        [Fact]
        public async Task LambdaExp_CapturesLocalVariablesWithCopying()
        {
            var stmts = new Stmt[] {
                SimpleLocalVarDecl("x", Type.Int, new IntLiteralExp(3)),
                SimpleLocalVarDecl("func", Type.Lambda, new LambdaExp(
                    new CaptureInfo(false, new []{
                        new CaptureInfo.Element(Type.Int, "x"),
                    }),

                    Array.Empty<string>(),

                    PrintIntCmdStmt(new LocalVarExp("x"))
                )),

                new ExpStmt(new ExpInfo(new AssignExp(new LocalVarExp("x"), new IntLiteralExp(34)), Type.Int)),
                new ExpStmt(new ExpInfo(new CallValueExp(new ExpInfo(new LocalVarExp("func"), Type.Lambda), Array.Empty<ExpInfo>()), Type.Void))
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("3", output);
        }

        // ListIndexerExp
        [Fact]
        public async Task ListIndexerExp_GetElement()
        {
            var stmts = new Stmt[] {
                SimpleLocalVarDecl("list", Type.List(Type.Int), new ListExp(Type.Int, new [] { new IntLiteralExp(34), new IntLiteralExp(56) })),
                PrintIntCmdStmt(new ListIndexerExp(LocalVar("list", Type.List(Type.Int)), new ExpInfo(new IntLiteralExp(1), Type.Int)))
            };

            var output = await EvalAsync(null, stmts);
            Assert.Equal("56", output);
        }

        // ListExp
        [Fact]
        public async Task ListExp_EvaluatesElementExpInOrder()
        {
            var printFuncId = new FuncDeclId(0);
            var printFunc = new FuncDecl.Normal(printFuncId, false, Array.Empty<string>(), new[] { "x" },

                new BlockStmt(
                    PrintIntCmdStmt(new LocalVarExp("x")),
                    new ReturnStmt(new LocalVarExp("x"))
                )
            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFuncId, Array.Empty<Type>(), null, new[] { new ExpInfo(new IntLiteralExp(v), Type.Int) });

            var stmts = new Stmt[] {
                new ExpStmt(new ExpInfo(
                    new ListExp(Type.Int, new [] { 
                        MakePrintCall(34),
                        MakePrintCall(56)
                    }),
                    Type.List(Type.Int)
                ))
            };

            var output = await EvalAsync(new [] { printFunc }, stmts);
            Assert.Equal("3456", output);
        }

        // NewEnum
        [Fact]
        public Task NewEnumExp_MakesEnumValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }


        // NewStruct
        [Fact]
        public Task NewStructExp_MakesStructValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Struct);
        }

        // NewClass
        [Fact]
        public Task NewClassExp_MakesClassValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Class);
        }
    }
}
