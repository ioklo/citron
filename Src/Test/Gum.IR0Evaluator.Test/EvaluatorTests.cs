using Gum.IR0;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using static Gum.Infra.Misc;
using static Gum.IR0.IR0Factory;
using Gum.Infra;

namespace Gum.IR0Evaluator.Test
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

        ModuleName moduleName = new ModuleName("TestModule");

        CommandStmt Sleep(int i)
        {
            return RCommand(RString(
                new TextStringExpElement("#Sleep"),
                new ExpStringExpElement(                    
                    new CallInternalUnaryOperatorExp(
                        InternalUnaryOperator.ToString_Int_String,
                        new IntLiteralExp(i)
                    )
                )
            ));
        }

        CommandStmt PrintBoolCmdStmt(Exp exp)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Bool_String,
                    exp
                )
            )));
        }

        CommandStmt PrintIntCmdStmt(Loc loc)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    new LoadExp(loc))
            )));
        }


        CommandStmt PrintIntCmdStmt(Exp varExp)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    varExp)
            )));
        }

        CommandStmt PrintStringCmdStmt(Loc loc)
        {
            return RCommand(RString(new ExpStringExpElement(new LoadExp(loc))));
        }


        CommandStmt PrintStringCmdStmt(Exp varExp)
        {
            return RCommand(RString(new ExpStringExpElement(varExp)));
        }

        CommandStmt PrintStringCmdStmt(string text)
        {
            return RCommand(RString(text));
        }

        Loc LocalVar(string varName)
        {
            return new LocalVarLoc(varName);
        }

        Loc IntLoc(int i)
        {
            return new TempLoc(new IntLiteralExp(i), Path.Int);
        }

        public async Task<string> EvalAsync(ImmutableArray<Decl> decls, ImmutableArray<Stmt> topLevelStmts)
        {
            var (output, _) = await EvalAsyncWithRetValue(decls, topLevelStmts);
            return output;
        }
        
        public async Task<(string Output, int RetValue)> EvalAsyncWithRetValue(
            ImmutableArray<Decl> decls,
            ImmutableArray<Stmt> topLevelStmts)
        {   
            var commandProvider = new TestCommandProvider();
            var script = new Script(moduleName, decls, topLevelStmts);

            var evaluator = new Evaluator(commandProvider, script);            

            var retValue = await evaluator.EvalAsync();

            return (commandProvider.GetOutput(), retValue);
        }        

        [Fact]
        public async Task CommandStmt_WorksProperly()
        {
            var topLevelStmts = Arr<Stmt>(RCommand(RString("Hello World")));
            var output = await EvalAsync(default, topLevelStmts);

            Assert.Equal("Hello World", output);
        }

        // Stmt
        // PrivateGlobalVarDeclStmt
        [Fact]
        public async Task PrivateGlobalVariableStmt_DeclProperly()
        {   
            var topLevelStmts = Arr<Stmt>
            (
                RGlobalVarDeclStmt(Path.Int, "x", new IntLiteralExp(3)),
                PrintIntCmdStmt(new GlobalVarLoc("x"))
            );

            var output = await EvalAsync(default, topLevelStmts);
            Assert.Equal("3", output);
        }

        // without typeArgs
        Path.Nested RootPath(Name name)
        {            
            return new Path.Nested(new Path.Root(moduleName), name, ParamHash.None, default);
        }

        Path.Nested RootPath(Name name, ImmutableArray<Path> paramTypes, ImmutableArray<Path> typeArgs)
        {
            return new Path.Nested(new Path.Root(moduleName), name, new ParamHash(typeArgs.Length, paramTypes), typeArgs);
        }

        [Fact]
        public async Task PrivateGlobalVariableExp_GetGlobalValueInFunc()
        {
            var func0 = new NormalFuncDecl(default, "TestFunc", false, default, default, RBlock(
                PrintStringCmdStmt(new GlobalVarLoc("x"))));
            
            var func = RootPath("TestFunc");

            var topLevelStmts = Arr<Stmt>
            (
                RGlobalVarDeclStmt(Path.String, "x", RString("Hello")),
                new ExpStmt(new CallFuncExp(func, null, default))
            );

            var output = await EvalAsync(Arr<Decl>(func0), topLevelStmts);

            Assert.Equal("Hello", output);
        }

        // SeqCall
        // seq int F(int x, int y) { yield x * 2; yield y + 3; } // struct anonymous_sequence_#1
        // foreach(var e in F(1, 2)) // e의 타입은 int,
        //    CommandStmt
        [Fact]
        public async Task CallSeqFuncExp_GenerateSequencesInForeach()
        {
            // Sequence
            var seqFunc = new SequenceFuncDecl(default, "F", false, Path.Int, default, new ParamInfo(null, Arr(new TypeAndName(Path.Int, "x"), new TypeAndName(Path.Int, "y"))), RBlock(

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Multiply_Int_Int_Int,
                        new LoadExp(LocalVar("x")),
                        new IntLiteralExp(2))),

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Add_Int_Int_Int,
                        new LoadExp(LocalVar("y")),
                        new IntLiteralExp(3)))));

            var funcF = RootPath("F", Arr(Path.Int, Path.Int), default);
            var seqTypeF = new Path.AnonymousSeqType(funcF);

            var stmts = Arr<Stmt>
            (
                new ForeachStmt(
                    Path.Int,
                    "e",
                    new TempLoc(
                        new CallSeqFuncExp(
                            funcF,
                            null,
                            Arr<Argument>(
                                new Argument.Normal(new IntLiteralExp(1)),
                                new Argument.Normal(new IntLiteralExp(2))
                            )
                        ),
                        
                        seqTypeF
                    ),

                    PrintIntCmdStmt(new LocalVarLoc("e"))
                )
            );

            var output = await EvalAsync(Arr<Decl>(seqFunc), stmts);

            Assert.Equal("25", output);
        }

        [Fact]
        public async Task LocalVarDeclStmt_OverlapsParentScope()
        {
            var stmts = Arr<Stmt>
            (
                RLocalVarDeclStmt(Path.Int, "x", new IntLiteralExp(23)),

                RBlock(
                    RLocalVarDeclStmt(Path.String, "x", RString("Hello")),
                    PrintStringCmdStmt(LocalVar("x"))
                ),

                PrintIntCmdStmt(new LocalVarLoc("x"))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("Hello23", output);
        }

        [Fact]
        public async Task LocalVarDeclStmt_OverlapsGlobalVar()
        {
            var stmts = Arr<Stmt>
            (
                RGlobalVarDeclStmt(Path.Int, "x", new IntLiteralExp(23)),

                RBlock(
                    RLocalVarDeclStmt(Path.String, "x", RString("Hello")),
                    PrintStringCmdStmt(LocalVar("x"))
                ),

                PrintIntCmdStmt(new GlobalVarLoc("x"))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("Hello23", output);
        }

        // If
        [Fact]
        public async Task IfStmt_SelectThenBranchWhenConditionTrue()
        {
            var stmts = Arr<Stmt>(new IfStmt(new BoolLiteralExp(true), PrintStringCmdStmt("True"), null));
            var output = await EvalAsync(default, stmts);

            Assert.Equal("True", output);
        }

        [Fact]
        public async Task IfStmt_SelectElseBranchWhenConditionFalse()
        {
            var stmts = Arr<Stmt>(new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, PrintStringCmdStmt("False")));
            var output = await EvalAsync(default, stmts);

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
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Int, "x", new IntLiteralExp(34)),

                new ForStmt(
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "x", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new LoadExp(LocalVar("x")), new IntLiteralExp(0)),
                    new AssignExp(LocalVar("x"), new IntLiteralExp(1)),
                    PrintIntCmdStmt(new LocalVarLoc("x"))),

                PrintIntCmdStmt(new LocalVarLoc("x"))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("034", output);
        }

        [Fact]
        public async Task ExpForStmtInitializer_EvaluateAtStart()
        {
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Int, "x", new IntLiteralExp(34)),

                PrintIntCmdStmt(new LocalVarLoc("x")),

                new ForStmt(
                    new ExpForStmtInitializer(new AssignExp(LocalVar("x"), new IntLiteralExp(12))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new LoadExp(LocalVar("x")), new IntLiteralExp(12)),
                    new AssignExp(LocalVar("x"), new IntLiteralExp(1)),
                    PrintIntCmdStmt(new LocalVarLoc("x"))),

                PrintIntCmdStmt(new LocalVarLoc("x"))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("34121", output);
        }

        [Fact]
        public async Task ForStmt_EvalConditionAtStart()
        {
            var stmts = Arr<Stmt> (
                
                new ForStmt(
                    null,
                    new BoolLiteralExp(false),
                    null,
                    PrintStringCmdStmt("Wrong")),

                PrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAtEndOfBody()
        {
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LoadExp(LocalVar("x")),
                    new AssignExp(LocalVar("x"), new BoolLiteralExp(false)),
                    PrintStringCmdStmt("Once")),

                PrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAndEvalCondExpAfterEvalContinueStmt()
        {
            var stmts = Arr<Stmt> (


                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LoadExp(LocalVar("x")),
                    new AssignExp(LocalVar("x"), new BoolLiteralExp(false)),
                    RBlock(
                        PrintStringCmdStmt("Once"),
                        ContinueStmt.Instance,
                        PrintStringCmdStmt("Wrong")
                    )
                ),

                PrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalBreakStmt()
        {
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LoadExp(LocalVar("x")),
                    new AssignExp(LocalVar("x"), new BoolLiteralExp(false)),
                    RBlock(
                        PrintStringCmdStmt("Once"),
                        BreakStmt.Instance,
                        PrintStringCmdStmt("Wrong")
                    )
                ),

                PrintBoolCmdStmt(new LoadExp(LocalVar("x"))),
                PrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("OnceTrueCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalReturnStmt()
        {
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LoadExp(LocalVar("x")),
                    new AssignExp(LocalVar("x"), new BoolLiteralExp(false)),
                    RBlock(
                        PrintStringCmdStmt("Once"),
                        new ReturnStmt(new IntLiteralExp(2)),
                        PrintStringCmdStmt("Wrong")
                    )
                ),
                
                PrintStringCmdStmt("Wrong")
            );

            var (output, result) = await EvalAsyncWithRetValue(default,  stmts);
            Assert.Equal("Once", output);
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task ContinueStmt_ContinuesInnerMostLoopStmt()
        {
            var stmts = Arr<Stmt> (

                new ForStmt(
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(LocalVar("i")), RInt(2)),
                    new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, LocalVar("i")),

                    RBlock(

                        PrintIntCmdStmt(new LocalVarLoc("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "j", new IntLiteralExp(0))),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(LocalVar("j")), RInt(2)),
                            new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, LocalVar("j")),
                            RBlock(
                                PrintIntCmdStmt(new LocalVarLoc("j")),
                                ContinueStmt.Instance,
                                PrintIntCmdStmt(new LocalVarLoc("Wrong"))
                            )
                        )
                    )
                )
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("001101", output);
        }

        [Fact]
        public async Task BreakStmt_ExitsInnerMostLoopStmt()
        {
            var stmts = Arr<Stmt> (

                new ForStmt(
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(LocalVar("i")), RInt(2)),
                    new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, LocalVar("i")),

                    RBlock(

                        PrintIntCmdStmt(new LocalVarLoc("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "j", new IntLiteralExp(0))),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(LocalVar("j")), RInt(2)),
                            new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, LocalVar("j")),
                            RBlock(
                                PrintIntCmdStmt(new LocalVarLoc("j")),
                                BreakStmt.Instance,
                                PrintIntCmdStmt(new LocalVarLoc("Wrong"))
                            )
                        )
                    )
                )
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("0010", output);
        }

        [Fact]
        public async Task ReturnStmt_ExitsFuncImmediately()
        {
            var funcDecl = new NormalFuncDecl(default, "F", false, default, default,
                RBlock(
                    new ReturnStmt(null),
                    PrintStringCmdStmt("Wrong")
                )
            );

            var func = RootPath("F");

            var stmts = Arr<Stmt> (
                new ExpStmt(new CallFuncExp(func, null, default)),
                PrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(Arr<Decl>(funcDecl), stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ReturnStmt_ReturnProperlyInTopLevel()
        {
            var topLevelStmts = Arr<Stmt>(new ReturnStmt(new IntLiteralExp(34)));
            var (_, retValue) = await EvalAsyncWithRetValue(default,  topLevelStmts);

            Assert.Equal(34, retValue);
        }

        [Fact]
        public async Task ReturnStmt_SetReturnValueToCaller()
        {   
            var funcDecl = new NormalFuncDecl(default, "F", false, default, default,
                RBlock(
                    new ReturnStmt(new IntLiteralExp(77)),
                    PrintStringCmdStmt("Wrong")
                )
            );

            var func = RootPath("F");

            var stmts = Arr<Stmt> (
                PrintIntCmdStmt(new CallFuncExp(func, null, default))
            );

            var output = await EvalAsync(Arr<Decl>(funcDecl), stmts);
            Assert.Equal("77", output);
        }

        // BlockStmt에서 LocalVar 범위 테스트는 LocalVarDeclStmt_OverlapsParentScope에서 처리함
        [Fact]
        public async Task BlockStmt_EvalInnerStatementsSequencially()
        {
            var stmts = Arr<Stmt> (
                PrintIntCmdStmt(new IntLiteralExp(1)),
                PrintIntCmdStmt(new IntLiteralExp(23)),
                PrintIntCmdStmt(new IntLiteralExp(4))
            );

            var output = await EvalAsync(default, stmts);

            Assert.Equal("1234", output);
        }

        // BlankStmt do nothing.. nothing을 테스트하기는 힘들다
        [Fact]
        public async Task ExpStmt_EvalInnerExp()
        {
            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.Int, "x", null),
                new ExpStmt(new AssignExp(LocalVar("x"), new IntLiteralExp(3))),
                PrintIntCmdStmt(new LocalVarLoc("x"))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("3", output);
        }

        // Task Await
        [Fact]
        public async Task TaskStmt_EvalParallel()
        {
            Stmt PrintNumbersStmt(int count)
            {
                return new ForStmt(
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(LocalVar("i")), RInt(count)),
                    new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, LocalVar("i")),
                    PrintIntCmdStmt(new LocalVarLoc("i"))
                );
            }

            var anonymousName0 = new Name.Anonymous(new AnonymousId(0));
            var anonymousName1 = new Name.Anonymous(new AnonymousId(1));

            var capturedStmtDecl0 = new CapturedStatementDecl(anonymousName0, new CapturedStatement(null, default, PrintNumbersStmt(5)));
            var capturedStmtDecl1 = new CapturedStatementDecl(anonymousName1, new CapturedStatement(null, default, PrintNumbersStmt(5)));

            var path0 = RootPath(anonymousName0);
            var path1 = RootPath(anonymousName1);

            var stmts = Arr<Stmt> (
                new AwaitStmt(
                    RBlock(
                        new TaskStmt(path0),
                        new TaskStmt(path1)
                    )
                )
            );

            var output = await EvalAsync(Arr<Decl>(capturedStmtDecl0, capturedStmtDecl1), stmts);

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
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(LocalVar("i")), new LoadExp(LocalVar("count"))),
                    new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, LocalVar("i")),
                    PrintIntCmdStmt(new LocalVarLoc("i"))
                );
            }

            var name0 = new Name.Anonymous(new AnonymousId(0));
            var capturedStmtDecl0 = new CapturedStatementDecl(name0, new CapturedStatement(null, Arr(new TypeAndName(Path.Int, "count")), PrintNumbersStmt()));
            var path0 = new Path.Nested(new Path.Root(moduleName), name0, ParamHash.None, default);            

            var name1 = new Name.Anonymous(new AnonymousId(1));
            var capturedStmtDecl1 = new CapturedStatementDecl(name1, new CapturedStatement(null, Arr(new TypeAndName(Path.Int, "count")), PrintNumbersStmt()));
            var path1 = new Path.Nested(new Path.Root(moduleName), name1, ParamHash.None, default);

            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.Int, "count", new IntLiteralExp(5)),

                new AwaitStmt(
                    RBlock(
                        new TaskStmt(path0),
                        new ExpStmt(new AssignExp(LocalVar("count"), new IntLiteralExp(4))),
                        new TaskStmt(path1)
                    )
                )
            );

            var output = await EvalAsync(Arr<Decl>(capturedStmtDecl0, capturedStmtDecl1), stmts);

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
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(LocalVar("i")), RInt(count)),
                    new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, LocalVar("i")),
                    RBlock(
                        PrintIntCmdStmt(new LocalVarLoc("i")),
                        Sleep(1)
                    )
                );
            }

            var name0 = new Name.Anonymous(new AnonymousId(0));
            var capturedStmtDecl0 = new CapturedStatementDecl(name0, new CapturedStatement(null, default, PrintNumbersStmt(5)));
            var path0 = RootPath(name0);

            var name1 = new Name.Anonymous(new AnonymousId(1));
            var capturedStmtDecl1 = new CapturedStatementDecl(name1, new CapturedStatement(null, default, PrintNumbersStmt(5)));
            var path1 = RootPath(name1);

            var stmts = Arr<Stmt> (
                new AwaitStmt(
                    RBlock(
                        new AsyncStmt(path0),
                        new AsyncStmt(path1)
                    )
                )
            );

            var output = await EvalAsync(Arr<Decl>(capturedStmtDecl0, capturedStmtDecl1), stmts);

            Assert.Equal("0011223344", output);
        }

        // seq int F0() { ... }
        // seq int F1() { ... }
        [Fact]
        public async Task YieldStmt_GenerateElementForInnerMostForeachStmt()
        {
            var seqFunc0 = RootPath("F0");
            var seqFunc1 = RootPath("F1");

            var seqFuncDecl0 = new SequenceFuncDecl(default, "F0", false, Path.Int, default, default,
                new ForeachStmt(
                    Path.Int, "elem", 
                    new TempLoc(new CallSeqFuncExp(seqFunc1, null, default), new Path.AnonymousSeqType(seqFunc1)),
                    RBlock(
                        PrintIntCmdStmt(new LocalVarLoc("elem")),
                        new YieldStmt(new LoadExp(LocalVar("elem")))
                    )
                )
            );

            var seqFuncDecl1 = new SequenceFuncDecl(default, "F1", false, Path.Int, default, default,
                RBlock(
                    new YieldStmt(new IntLiteralExp(34)),
                    new YieldStmt(new IntLiteralExp(56))
                )
            );

            var stmts = Arr<Stmt>(
                new ForeachStmt(Path.Int, "x", new TempLoc(new CallSeqFuncExp(seqFunc0, null, default), new Path.AnonymousSeqType(seqFunc0)),
                    PrintIntCmdStmt(new LocalVarLoc("x")))
            );

            var output = await EvalAsync(Arr<Decl>(seqFuncDecl0, seqFuncDecl1), stmts);

            Assert.Equal("34345656", output);
        }

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
            var stmts = Arr<Stmt> (            
                PrintBoolCmdStmt(new BoolLiteralExp(false)),
                PrintBoolCmdStmt(new BoolLiteralExp(true))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("FalseTrue", output);
        }

        [Fact]
        public async Task IntLiteralExp_MakesIntValue()
        {
            var stmts = Arr<Stmt>(
                PrintIntCmdStmt(new IntLiteralExp(-2))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("-2", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingTextStringExpElements()
        {
            var stmts = Arr<Stmt>(
                PrintStringCmdStmt(RString(
                    new TextStringExpElement("Hello "),
                    new TextStringExpElement("World")
                ))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingStringExpElements()
        {
            var stmts = Arr<Stmt>
            (
                RLocalVarDeclStmt(Path.String, "x", RString("New ")),

                PrintStringCmdStmt(RString(
                    new TextStringExpElement("Hello "),
                    new ExpStringExpElement(new LoadExp(LocalVar("x"))),
                    new TextStringExpElement("World")))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("Hello New World", output);
        }

        [Fact]
        public async Task AssignExp_ReturnsValue()
        {
            var stmts = Arr<Stmt>
            (
                RLocalVarDeclStmt(Path.Int, "x", new IntLiteralExp(3)),
                RLocalVarDeclStmt(Path.Int, "y", new IntLiteralExp(4)),

                // y = x = 10;
                new ExpStmt(new AssignExp(LocalVar("y"), new AssignExp(LocalVar("x"), new IntLiteralExp(10)))),
                PrintIntCmdStmt(new LocalVarLoc("x")),
                PrintIntCmdStmt(new LocalVarLoc("y"))
            );

            var output = await EvalAsync(default, stmts);

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
            var printFunc = RootPath("Print", Arr(Path.Int), default);
            var testFunc = RootPath("TestFunc", Arr(Path.Int, Path.Int, Path.Int), default);

            // Print(int x)
            var printFuncDecl = new NormalFuncDecl(default, "Print", false, default, Arr(new ParamInfo(Path.Int, "x")),

                RBlock(
                    PrintIntCmdStmt(new LocalVarLoc("x")),
                    new ReturnStmt(new LoadExp(LocalVar("x")))
                )
            );

            // TestFunc(int i, int j, int k)
            var testFuncDecl = new NormalFuncDecl(default, "TestFunc", false, default, Arr(new ParamInfo(Path.Int, "i"), new ParamInfo(Path.Int, "j"), new ParamInfo(Path.Int, "k")),

                PrintStringCmdStmt("TestFunc")

            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFunc, null, Arr<Exp>(new IntLiteralExp(v)));

            var stmts = Arr<Stmt>
            (
                new ExpStmt(
                    new CallFuncExp(
                        testFunc, 
                        null, 
                        Arr(
                            MakePrintCall(1),
                            MakePrintCall(2),
                            MakePrintCall(3) 
                        )
                    )
                )
            );

            var output = await EvalAsync(Arr<Decl>(printFuncDecl, testFuncDecl), stmts);
            Assert.Equal("123TestFunc", output);
        }

        [Fact]
        public async Task CallFuncExp_ReturnsValueProperly()
        {
            var func = RootPath("F");
            // F() { return "hello world"; }
            var funcDecl = new NormalFuncDecl(default, "F", false, default, default, new ReturnStmt(RString("Hello World")));

            var stmts = Arr<Stmt>(PrintStringCmdStmt(new CallFuncExp(func, null, default)));

            var output = await EvalAsync(Arr<Decl>(funcDecl), stmts);
            Assert.Equal("Hello World", output);
        }

        // CallSeqFunc
        [Fact]
        public async Task CallSeqFuncExp_ReturnsAnonymousSeqTypeValue()
        {
            var seqFunc = RootPath("F");
            var seqFuncDecl = new SequenceFuncDecl(default, "F", false, Path.String, default, default, BlankStmt.Instance); // return nothing

            var stmts = Arr<Stmt>
            (
                // 선언하자 마자 대입하기
                RLocalVarDeclStmt(new Path.AnonymousSeqType(seqFunc), "x", new CallSeqFuncExp(seqFunc, null, default)),

                // 로컬 변수에 새 값 대입하기
                new ExpStmt(new AssignExp(LocalVar("x"), new CallSeqFuncExp(seqFunc, null, default)))
            );

            await EvalAsync(Arr<Decl>(seqFuncDecl), stmts);

            // 에러가 안났으면 성공..
        }
        
        [Fact]
        public async Task CallValueExp_EvaluateCallableAndArgumentsInOrder()
        {
            var printFunc = RootPath("Print", Arr(Path.Int), default);
            var makeLambda = RootPath("MakeLambda", default, default);
            var lambda = new Path.Nested(makeLambda, new Name.Anonymous(new AnonymousId(0)), ParamHash.None, default);

            // Print(int x) { 
            var printFuncDecl = new NormalFuncDecl(default, "Print", false, default, Arr(new ParamInfo(Path.Int, "x")),
                RBlock(
                    PrintIntCmdStmt(new LocalVarLoc("x")),
                    new ReturnStmt(new LoadExp(LocalVar("x")))
                )
            );

            // MakeLambda() { return (int i, int j, int k) => @"TestFunc";}
            var lambdaDecl = new LambdaDecl(
                new Name.Anonymous(new AnonymousId(0)),
                new CapturedStatement(null, default, PrintStringCmdStmt("TestFunc")),
                Arr(new ParamInfo(Path.Int, "i"), new ParamInfo(Path.Int, "j"), new ParamInfo(Path.Int, "k"))
            );
            
            var makeLambdaDecl = new NormalFuncDecl(Arr<Decl>(lambdaDecl), "MakeLambda", false, default, default,
                RBlock(                    
                    PrintStringCmdStmt("MakeLambda"),
                    new ReturnStmt(new LambdaExp(lambda))
                )
            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFunc, null, Arr<Exp>(new IntLiteralExp(v)));
            
            var stmts = Arr<Stmt>
            (
                new ExpStmt(
                    new CallValueExp(
                        lambda,
                        new TempLoc(new CallFuncExp(makeLambda, null, default), new Path.AnonymousLambdaType(lambda)),
                        Arr(
                            MakePrintCall(1),
                            MakePrintCall(2),
                            MakePrintCall(3) 
                        )
                    )
                )
            );

            var output = await EvalAsync(Arr<Decl>(printFuncDecl, makeLambdaDecl), stmts);
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
            // [x] () => @"$x";
            var lambdaDecl = new LambdaDecl(new Name.Anonymous(new AnonymousId(0)), new CapturedStatement(null, Arr(new TypeAndName(Path.Int, "x")), PrintIntCmdStmt(new LocalVarLoc("x"))), default);
            var lambda = RootPath(new Name.Anonymous(new AnonymousId(0)));

            // int x = 3;
            // var func = () => x;
            // x = 34;
            // func();
            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.Int, "x", new IntLiteralExp(3)),
                RLocalVarDeclStmt(new Path.AnonymousLambdaType(lambda), "func", new LambdaExp(lambda)),

                new ExpStmt(new AssignExp(LocalVar("x"), new IntLiteralExp(34))),
                new ExpStmt(new CallValueExp(lambda, LocalVar("func"), default))
            );

            var output = await EvalAsync(Arr<Decl>(lambdaDecl), stmts);
            Assert.Equal("3", output);
        }

        // ListIndexerExp
        [Fact]
        public async Task ListIndexerExp_GetElement()
        {
            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.List(Path.Int), "list", new ListExp(Path.Int, Arr<Exp>(new IntLiteralExp(34), new IntLiteralExp(56)))),
                PrintIntCmdStmt(new ListIndexerLoc(new LocalVarLoc("list"), new TempLoc(new IntLiteralExp(1), Path.Int)))
            );

            var output = await EvalAsync(default, stmts);
            Assert.Equal("56", output);
        }

        // ListExp
        [Fact]
        public async Task ListExp_EvaluatesElementExpInOrder()
        {
            var printFunc = RootPath("Print", Arr(Path.Int), default);

            // print(int x)
            var printFuncDecl = new NormalFuncDecl(default, "Print", false, default, Arr(new ParamInfo(Path.Int, "x")),

                RBlock(
                    PrintIntCmdStmt(new LocalVarLoc("x")),
                    new ReturnStmt(new LoadExp(new LocalVarLoc("x")))
                )
            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFunc, null, Arr<Exp>(new IntLiteralExp(v)));

            var stmts = Arr<Stmt> (
                RGlobalVarDeclStmt(Path.List(Path.Int), "l", 
                    new ListExp(Path.Int, Arr(
                        MakePrintCall(34),
                        MakePrintCall(56)
                    ))
                )
            );

            var output = await EvalAsync(Arr<Decl>(printFuncDecl), stmts);
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
