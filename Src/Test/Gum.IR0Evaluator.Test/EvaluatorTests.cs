﻿using Gum.IR0;
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
using System.Reflection;
using System.Diagnostics;
using Gum.Test.Misc;

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

            public void Append(string s) { sb.Append(s); }

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

        Loc IntLoc(int i)
        {
            return new TempLoc(new IntLiteralExp(i), Path.Int);
        }

        async Task<string> EvalAsync(Script script)
        {
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        async Task<string> EvalAsync(ImmutableArray<Stmt> topLevelStmts)
        {
            var script = new Script(moduleName, default, default, default, topLevelStmts);
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        async Task<string> EvalAsync(ImmutableArray<FuncDecl> globalFuncDecls, ImmutableArray<Stmt> topLevelStmts)
        {
            var script = new Script(moduleName, default, globalFuncDecls, default, topLevelStmts);
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        async Task<string> EvalAsync(FuncDecl funcDecl, ImmutableArray<Stmt> topLevelStmts)
        {
            var script = new Script(moduleName, default, Arr(funcDecl), default, topLevelStmts);
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        async Task<string> EvalAsync(ImmutableArray<CallableMemberDecl> callableMemberDecls, ImmutableArray<Stmt> topLevelStmts)
        {
            var script = new Script(moduleName, default, default, callableMemberDecls, topLevelStmts);
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        async Task<(string Output, int RetValue)> EvalAsyncWithRetValue(ImmutableArray<IModuleDriver> moduleDrivers, Script script)
        {   
            var commandProvider = new TestCommandProvider();
            var retValue = await Evaluator.EvalAsync(moduleDrivers, commandProvider, script);
            return (commandProvider.GetOutput(), retValue);
        }

        async Task<(string Output, int RetValue)> EvalAsyncWithRetValue(TestCommandProvider commandProvider, ImmutableArray<IModuleDriver> moduleDrivers, Script script)
        {
            var retValue = await Evaluator.EvalAsync(moduleDrivers, commandProvider, script);
            return (commandProvider.GetOutput(), retValue);
        }


        [Fact]
        public async Task CommandStmt_WorksProperly()
        {
            var topLevelStmts = Arr<Stmt>(RCommand(RString("Hello World")));
            var output = await EvalAsync(topLevelStmts);

            Assert.Equal("Hello World", output);
        }

        // Stmt
        // GlobalVarDeclStmt
        [Fact]
        public async Task GlobalVariableStmt_DeclProperly()
        {   
            var topLevelStmts = Arr<Stmt>
            (
                RGlobalVarDeclStmt(Path.Int, "x", new IntLiteralExp(3)),
                RPrintIntCmdStmt(new GlobalVarLoc("x"))
            );

            var output = await EvalAsync(topLevelStmts);
            Assert.Equal("3", output);
        }

        // without typeArgs
        Path.Nested RootPath(string name)
        {            
            return new Path.Nested(new Path.Root(moduleName), new Name.Normal(name), ParamHash.None, default);
        }

        Path.Nested RootPath(Name name)
        {
            return new Path.Nested(new Path.Root(moduleName), name, ParamHash.None, default);
        }

        Path.Nested RootPath(string name, ImmutableArray<ParamHashEntry> paramWithoutNames, ImmutableArray<Path> typeArgs)
        {
            return new Path.Nested(new Path.Root(moduleName), new Name.Normal(name), new ParamHash(typeArgs.Length, paramWithoutNames), typeArgs);
        }

        Path.Nested RootPath(string name, ImmutableArray<Path> paramTypes, ImmutableArray<Path> typeArgs)
        {
            return new Path.Nested(
                new Path.Root(moduleName), new Name.Normal(name), 
                new ParamHash(
                    typeArgs.Length, paramTypes.Select(paramType => new ParamHashEntry(ParamKind.Default, paramType)).ToImmutableArray()
                ), 
                typeArgs);
        }

        [Fact]
        public async Task GlobalVariableExp_GetGlobalValueInFunc()
        {
            var func0 = new NormalFuncDecl(default, new Name.Normal("TestFunc"), false, default, default, RBlock(
                RPrintStringCmdStmt(new GlobalVarLoc("x"))));
            
            var func = RootPath("TestFunc");

            var topLevelStmts = Arr<Stmt>
            (
                RGlobalVarDeclStmt(Path.String, "x", RString("Hello")),
                new ExpStmt(new CallFuncExp(func, null, default))
            );

            var output = await EvalAsync(func0, topLevelStmts);

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
            var seqFunc = new SequenceFuncDecl(default, new Name.Normal("F"), false, Path.Int, default, Arr(new Param(ParamKind.Default, Path.Int, new Name.Normal("x")), new Param(ParamKind.Default, Path.Int, new Name.Normal("y"))), RBlock(

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Multiply_Int_Int_Int,
                        new LoadExp(RLocalVarLoc("x")),
                        new IntLiteralExp(2))),

                new YieldStmt(
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Add_Int_Int_Int,
                        new LoadExp(RLocalVarLoc("y")),
                        new IntLiteralExp(3)))));

            var funcF = RootPath("F", Arr(Path.Int, Path.Int), default);
            var seqTypeF = funcF;

            var stmts = Arr<Stmt>
            (
                new ForeachStmt(
                    Path.Int,
                    "e",
                    new TempLoc(
                        new CallSeqFuncExp(
                            funcF,
                            null,
                            RArgs(
                                new IntLiteralExp(1),
                                new IntLiteralExp(2)
                            )
                        ),
                        
                        seqTypeF
                    ),

                    RPrintIntCmdStmt(RLocalVarExp("e"))
                )
            );

            var output = await EvalAsync(seqFunc, stmts);

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
                    RPrintStringCmdStmt(RLocalVarLoc("x"))
                ),

                RPrintIntCmdStmt(RLocalVarExp("x"))
            );

            var output = await EvalAsync(stmts);
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
                    RPrintStringCmdStmt(RLocalVarLoc("x"))
                ),

                RPrintIntCmdStmt(new GlobalVarLoc("x"))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("Hello23", output);
        }

        [Fact]
        public async Task GlobalRefVarDeclStmt_RefDeclAndRefExp_WorksProperly()
        {
            // int i = 3;
            // ref int x = ref i;
            // x = 4 + x;
            // @$i
            var stmts = Arr<Stmt>(
                RGlobalVarDeclStmt(Path.Int, "i", RInt(3)),
                RGlobalRefVarDeclStmt("x", new GlobalVarLoc("i")), // type이 빠진다
                new ExpStmt(new AssignExp(
                    new DerefLocLoc(new GlobalVarLoc("x")), 
                    new CallInternalBinaryOperatorExp(
                        InternalBinaryOperator.Add_Int_Int_Int, 
                        RInt(4),
                        new LoadExp(new DerefLocLoc(new GlobalVarLoc("x")))
                    )
                )),

                RPrintIntCmdStmt(new GlobalVarLoc("i")) 
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("7", output);
        }

        // If
        [Fact]
        public async Task IfStmt_SelectThenBranchWhenConditionTrue()
        {
            var stmts = Arr<Stmt>(new IfStmt(new BoolLiteralExp(true), RPrintStringCmdStmt("True"), null));
            var output = await EvalAsync(stmts);

            Assert.Equal("True", output);
        }

        [Fact]
        public async Task IfStmt_SelectElseBranchWhenConditionFalse()
        {
            var stmts = Arr<Stmt>(new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, RPrintStringCmdStmt("False")));
            var output = await EvalAsync(stmts);

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
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new LoadExp(RLocalVarLoc("x")), new IntLiteralExp(0)),
                    new AssignExp(RLocalVarLoc("x"), new IntLiteralExp(1)),
                    RPrintIntCmdStmt(RLocalVarExp("x"))),

                RPrintIntCmdStmt(RLocalVarExp("x"))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("034", output);
        }

        [Fact]
        public async Task ExpForStmtInitializer_EvaluateAtStart()
        {
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Int, "x", new IntLiteralExp(34)),

                RPrintIntCmdStmt(RLocalVarExp("x")),

                new ForStmt(
                    new ExpForStmtInitializer(new AssignExp(RLocalVarLoc("x"), new IntLiteralExp(12))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.Equal_Int_Int_Bool, new LoadExp(RLocalVarLoc("x")), new IntLiteralExp(12)),
                    new AssignExp(RLocalVarLoc("x"), new IntLiteralExp(1)),
                    RPrintIntCmdStmt(RLocalVarExp("x"))),

                RPrintIntCmdStmt(RLocalVarExp("x"))
            );

            var output = await EvalAsync(stmts);
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
                    RPrintStringCmdStmt("Wrong")),

                RPrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAtEndOfBody()
        {
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LoadExp(RLocalVarLoc("x")),
                    new AssignExp(RLocalVarLoc("x"), new BoolLiteralExp(false)),
                    RPrintStringCmdStmt("Once")),

                RPrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAndEvalCondExpAfterEvalContinueStmt()
        {
            var stmts = Arr<Stmt> (


                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LoadExp(RLocalVarLoc("x")),
                    new AssignExp(RLocalVarLoc("x"), new BoolLiteralExp(false)),
                    RBlock(
                        RPrintStringCmdStmt("Once"),
                        ContinueStmt.Instance,
                        RPrintStringCmdStmt("Wrong")
                    )
                ),

                RPrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalBreakStmt()
        {
            var stmts = Arr<Stmt> (

                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),

                new ForStmt(
                    null,
                    new LoadExp(RLocalVarLoc("x")),
                    new AssignExp(RLocalVarLoc("x"), new BoolLiteralExp(false)),
                    RBlock(
                        RPrintStringCmdStmt("Once"),
                        BreakStmt.Instance,
                        RPrintStringCmdStmt("Wrong")
                    )
                ),

                RPrintBoolCmdStmt(new LoadExp(RLocalVarLoc("x"))),
                RPrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("OncetrueCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalReturnStmt()
        {
            var script = RScript(
                moduleName, 
                RLocalVarDeclStmt(Path.Bool, "x", new BoolLiteralExp(true)),
                new ForStmt(
                    null,
                    new LoadExp(RLocalVarLoc("x")),
                    new AssignExp(RLocalVarLoc("x"), new BoolLiteralExp(false)),
                    RBlock(
                        RPrintStringCmdStmt("Once"),
                        new ReturnStmt(new ReturnInfo.Expression(new IntLiteralExp(2))),
                        RPrintStringCmdStmt("Wrong")
                    )
                ),
                
                RPrintStringCmdStmt("Wrong")
            );

            var (output, result) = await EvalAsyncWithRetValue(default, script);
            Assert.Equal("Once", output);
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task ContinueStmt_ContinuesInnerMostLoopStmt()
        {
            var stmts = Arr<Stmt> (

                new ForStmt(
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(RLocalVarLoc("i")), RInt(2)),
                    new CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator.PostfixInc_Int_Int, RLocalVarLoc("i")),

                    RBlock(

                        RPrintIntCmdStmt(RLocalVarExp("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "j", new IntLiteralExp(0))),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(RLocalVarLoc("j")), RInt(2)),
                            new CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator.PostfixInc_Int_Int, RLocalVarLoc("j")),
                            RBlock(
                                RPrintIntCmdStmt(RLocalVarExp("j")),
                                ContinueStmt.Instance,
                                RPrintIntCmdStmt(RLocalVarExp("Wrong"))
                            )
                        )
                    )
                )
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("001101", output);
        }

        [Fact]
        public async Task BreakStmt_ExitsInnerMostLoopStmt()
        {
            var stmts = Arr<Stmt> (

                new ForStmt(
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(RLocalVarLoc("i")), RInt(2)),
                    new CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator.PostfixInc_Int_Int, RLocalVarLoc("i")),

                    RBlock(

                        RPrintIntCmdStmt(RLocalVarExp("i")),

                        new ForStmt(
                            new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "j", new IntLiteralExp(0))),
                            new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(RLocalVarLoc("j")), RInt(2)),
                            new CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator.PostfixInc_Int_Int, RLocalVarLoc("j")),
                            RBlock(
                                RPrintIntCmdStmt(RLocalVarExp("j")),
                                BreakStmt.Instance,
                                RPrintIntCmdStmt(RLocalVarExp("Wrong"))
                            )
                        )
                    )
                )
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("0010", output);
        }

        [Fact]
        public async Task ReturnStmt_ExitsFuncImmediately()
        {
            var funcDecl = new NormalFuncDecl(default, new Name.Normal("F"), false, default, default,
                RBlock(
                    new ReturnStmt(ReturnInfo.None.Instance),
                    RPrintStringCmdStmt("Wrong")
                )
            );

            var func = RootPath("F");

            var stmts = Arr<Stmt> (
                new ExpStmt(new CallFuncExp(func, null, default)),
                RPrintStringCmdStmt("Completed")
            );

            var output = await EvalAsync(funcDecl, stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ReturnStmt_ReturnProperlyInTopLevel()
        {
            var script = RScript(moduleName, new ReturnStmt(new ReturnInfo.Expression(new IntLiteralExp(34))));
            var (_, retValue) = await EvalAsyncWithRetValue(default, script);

            Assert.Equal(34, retValue);
        }

        [Fact]
        public async Task ReturnStmt_SetReturnValueToCaller()
        {   
            var funcDecl = new NormalFuncDecl(default, new Name.Normal("F"), false, default, default,
                RBlock(
                    new ReturnStmt(new ReturnInfo.Expression(new IntLiteralExp(77))),
                    RPrintStringCmdStmt("Wrong")
                )
            );

            var func = RootPath("F");

            var stmts = Arr<Stmt> (
                RPrintIntCmdStmt(new CallFuncExp(func, null, default))
            );

            var output = await EvalAsync(funcDecl, stmts);
            Assert.Equal("77", output);
        }

        // BlockStmt에서 LocalVar 범위 테스트는 LocalVarDeclStmt_OverlapsParentScope에서 처리함
        [Fact]
        public async Task BlockStmt_EvalInnerStatementsSequencially()
        {
            var stmts = Arr<Stmt> (
                RPrintIntCmdStmt(new IntLiteralExp(1)),
                RPrintIntCmdStmt(new IntLiteralExp(23)),
                RPrintIntCmdStmt(new IntLiteralExp(4))
            );

            var output = await EvalAsync(stmts);

            Assert.Equal("1234", output);
        }

        // BlankStmt do nothing.. nothing을 테스트하기는 힘들다
        [Fact]
        public async Task ExpStmt_EvalInnerExp()
        {
            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.Int, "x", RInt(0)),
                new ExpStmt(new AssignExp(RLocalVarLoc("x"), new IntLiteralExp(3))),
                RPrintIntCmdStmt(RLocalVarExp("x"))
            );

            var output = await EvalAsync(stmts);
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
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(RLocalVarLoc("i")), RInt(count)),
                    new CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator.PostfixInc_Int_Int, RLocalVarLoc("i")),
                    RPrintIntCmdStmt(RLocalVarExp("i"))
                );
            }

            var anonymousName0 = new Name.Anonymous(0);
            var anonymousName1 = new Name.Anonymous(1);

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

            var output = await EvalAsync(Arr<CallableMemberDecl>(capturedStmtDecl0, capturedStmtDecl1), stmts);

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
            // for(int i = 0; i < count; i++) i++;
            Stmt PrintNumbersStmt()
            {
                return new ForStmt(
                    new VarDeclForStmtInitializer(RLocalVarDecl(Path.Int, "i", new IntLiteralExp(0))),
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(RLocalVarLoc("i")), new LoadExp(new CapturedVarLoc("count"))),
                    new CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator.PostfixInc_Int_Int, RLocalVarLoc("i")),
                    RPrintIntCmdStmt(RLocalVarExp("i"))
                );
            }

            // [count] { for(int i = 0; i < count; i++) i++; }
            var name0 = new Name.Anonymous(0);
            var capturedStmtDecl0 = new CapturedStatementDecl(name0, new CapturedStatement(null, Arr(new OuterLocalVarInfo(Path.Int, "count")), PrintNumbersStmt()));
            var path0 = new Path.Nested(new Path.Root(moduleName), name0, ParamHash.None, default);            

            var name1 = new Name.Anonymous(1);
            var capturedStmtDecl1 = new CapturedStatementDecl(name1, new CapturedStatement(null, Arr(new OuterLocalVarInfo(Path.Int, "count")), PrintNumbersStmt()));
            var path1 = new Path.Nested(new Path.Root(moduleName), name1, ParamHash.None, default);

            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.Int, "count", new IntLiteralExp(5)),

                new AwaitStmt(
                    RBlock(
                        new TaskStmt(path0),
                        new ExpStmt(new AssignExp(RLocalVarLoc("count"), new IntLiteralExp(4))),
                        new TaskStmt(path1)
                    )
                )
            );

            var output = await EvalAsync(Arr<CallableMemberDecl>(capturedStmtDecl0, capturedStmtDecl1), stmts);

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
                    new CallInternalBinaryOperatorExp(InternalBinaryOperator.LessThan_Int_Int_Bool, new LoadExp(RLocalVarLoc("i")), RInt(count)),
                    new CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator.PostfixInc_Int_Int, RLocalVarLoc("i")),
                    RBlock(
                        RPrintIntCmdStmt(RLocalVarExp("i")),
                        Sleep(1)
                    )
                );
            }

            var name0 = new Name.Anonymous(0);
            var capturedStmtDecl0 = new CapturedStatementDecl(name0, new CapturedStatement(null, default, PrintNumbersStmt(5)));
            var path0 = RootPath(name0);

            var name1 = new Name.Anonymous(1);
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

            var output = await EvalAsync(Arr<CallableMemberDecl>(capturedStmtDecl0, capturedStmtDecl1), stmts);

            Assert.Equal("0011223344", output);
        }

        // seq int F0() { ... }
        // seq int F1() { ... }
        [Fact]
        public async Task YieldStmt_GenerateElementForInnerMostForeachStmt()
        {
            var seqFunc0 = RootPath("F0");
            var seqFunc1 = RootPath("F1");

            var seqFuncDecl0 = new SequenceFuncDecl(default, new Name.Normal("F0"), false, Path.Int, default, default,
                new ForeachStmt(
                    Path.Int, "elem", 
                    new TempLoc(new CallSeqFuncExp(seqFunc1, null, default), seqFunc1),
                    RBlock(
                        RPrintIntCmdStmt(RLocalVarExp("elem")),
                        new YieldStmt(new LoadExp(RLocalVarLoc("elem")))
                    )
                )
            );

            var seqFuncDecl1 = new SequenceFuncDecl(default, new Name.Normal("F1"), false, Path.Int, default, default,
                RBlock(
                    new YieldStmt(new IntLiteralExp(34)),
                    new YieldStmt(new IntLiteralExp(56))
                )
            );

            var stmts = Arr<Stmt>(
                new ForeachStmt(Path.Int, "x", new TempLoc(new CallSeqFuncExp(seqFunc0, null, default), seqFunc0),
                    RPrintIntCmdStmt(RLocalVarExp("x")))
            );

            var output = await EvalAsync(Arr<FuncDecl>(seqFuncDecl0, seqFuncDecl1), stmts);

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
                RPrintBoolCmdStmt(new BoolLiteralExp(false)),
                RPrintBoolCmdStmt(new BoolLiteralExp(true))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("falsetrue", output);
        }

        [Fact]
        public async Task IntLiteralExp_MakesIntValue()
        {
            var stmts = Arr<Stmt>(
                RPrintIntCmdStmt(new IntLiteralExp(-2))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("-2", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingTextStringExpElements()
        {
            var stmts = Arr<Stmt>(
                RPrintStringCmdStmt(RString(
                    new TextStringExpElement("Hello "),
                    new TextStringExpElement("World")
                ))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingStringExpElements()
        {
            var stmts = Arr<Stmt>
            (
                RLocalVarDeclStmt(Path.String, "x", RString("New ")),

                RPrintStringCmdStmt(RString(
                    new TextStringExpElement("Hello "),
                    new ExpStringExpElement(new LoadExp(RLocalVarLoc("x"))),
                    new TextStringExpElement("World")))
            );

            var output = await EvalAsync(stmts);
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
                new ExpStmt(new AssignExp(RLocalVarLoc("y"), new AssignExp(RLocalVarLoc("x"), new IntLiteralExp(10)))),
                RPrintIntCmdStmt(RLocalVarLoc("x")),
                RPrintIntCmdStmt(RLocalVarLoc("y"))
            );

            var output = await EvalAsync(stmts);

            Assert.Equal("1010", output);
        }

        [Fact]
        public async Task CallFuncExp_CallByRef_WorksProperly()
        {
            //void F(ref int i)
            //{
            //    i = 7;
            //}

            //int j = 3;
            //F(ref j);
            //@$j

            // CallExp_RefArgumentTrivial_WorksProperly 에서 가져온
            var script = RScript(
                moduleName,
                default,
                Arr<FuncDecl>(
                    new NormalFuncDecl(
                        default, new Name.Normal("F"), false, default, Arr(new Param(ParamKind.Ref, Path.Int, new Name.Normal("i"))),
                        RBlock(new ExpStmt(new AssignExp(new DerefLocLoc(RLocalVarLoc("i")), RInt(7))))
                    )
                ),

                default,

                RGlobalVarDeclStmt(Path.Int, "j", RInt(3)),
                new ExpStmt(new CallFuncExp(
                    RootPath("F", Arr(new ParamHashEntry(ParamKind.Ref, Path.Int)), default),
                    null, Arr<Argument>(new Argument.Ref(new GlobalVarLoc("j")))
                )),

                RPrintIntCmdStmt(new GlobalVarLoc("j"))
            );

            var output = await EvalAsync(script);
            Assert.Equal("7", output);
        }

        [Fact]
        public async Task CallFuncExp_ReturnGlobalRef_WorksProperly()
        {
            // int x = 3;
            // ref int G() { return ref x; }
            // var i = ref G();
            // i = 4;
            // @$x

            var script = RScript(
                moduleName,
                default, 
                Arr<FuncDecl>(                    
                    new NormalFuncDecl(
                        default, new Name.Normal("G"), false, default, Arr(new Param(ParamKind.Ref, Path.Int, new Name.Normal("i"))),
                        RBlock(new ReturnStmt(new ReturnInfo.Ref(new GlobalVarLoc("x"))))
                    )
                ),
                default,
                RGlobalVarDeclStmt(Path.Int, "x", RInt(3)),
                RGlobalRefVarDeclStmt("i", new DerefExpLoc(
                    new CallFuncExp(
                        RootPath("G", Arr(new ParamHashEntry(ParamKind.Ref, Path.Int)), default),
                        null,
                        default
                    )
                )),
                new ExpStmt(new AssignExp(new DerefLocLoc(new GlobalVarLoc("i")), RInt(4))),
                RPrintIntCmdStmt(new LoadExp(new GlobalVarLoc("x")))
            );

            var output = await EvalAsync(script);
            Assert.Equal("4", output);
        }

        [Fact]
        public Task CallFuncExp_EvaluatesInstanceExpAtStart()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public Task CallFuncExp_PassesInstanceValueAsThisValue()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public Task CallFuncExp_PassesTypeAgumentsProperly()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public async Task CallFuncExp_EvaluatesArgumentsInOrder()
        {
            var printFunc = RootPath("Print", Arr(Path.Int), default);
            var testFunc = RootPath("TestFunc", Arr(Path.Int, Path.Int, Path.Int), default);

            // Print(int x)
            var printFuncDecl = new NormalFuncDecl(default, new Name.Normal("Print"), false, default, RNormalParams((Path.Int, "x")),
                RBlock(
                    RPrintIntCmdStmt(RLocalVarLoc("x")),
                    new ReturnStmt(new ReturnInfo.Expression(new LoadExp(RLocalVarLoc("x"))))
                )
            );

            // TestFunc(int i, int j, int k)
            var testFuncDecl = new NormalFuncDecl(default, new Name.Normal("TestFunc"), false, default, RNormalParams((Path.Int, "i"), (Path.Int, "j"), (Path.Int, "k")),
                RPrintStringCmdStmt("TestFunc")
            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFunc, null, RArgs(new IntLiteralExp(v)));

            var stmts = Arr<Stmt>
            (
                new ExpStmt(
                    new CallFuncExp(
                        testFunc, 
                        null, 
                        RArgs(
                            MakePrintCall(1),
                            MakePrintCall(2),
                            MakePrintCall(3) 
                        )
                    )
                )
            );

            var output = await EvalAsync(Arr<FuncDecl>(printFuncDecl, testFuncDecl), stmts);
            Assert.Equal("123TestFunc", output);
        }

        [Fact]
        public async Task CallFuncExp_ReturnsValueProperly()
        {
            var func = RootPath("F");
            // F() { return "hello world"; }
            var funcDecl = new NormalFuncDecl(default, new Name.Normal("F"), false, default, default, new ReturnStmt(new ReturnInfo.Expression(RString("Hello World"))));

            var stmts = Arr<Stmt>(RPrintStringCmdStmt(new CallFuncExp(func, null, default)));

            var output = await EvalAsync(funcDecl, stmts);
            Assert.Equal("Hello World", output);
        }

        class TestDriver : IModuleDriver
        {
            ModuleName moduleName;
            TestCommandProvider cmdProvider;

            public TestDriver(ModuleName moduleName, TestCommandProvider cmdProvider) 
            {
                this.moduleName = moduleName;
                this.cmdProvider = cmdProvider;
            }

            class TestFuncRuntimeItem : FuncRuntimeItem
            {
                TestCommandProvider cmdProvider;

                public TestFuncRuntimeItem(TestCommandProvider provider) { cmdProvider = provider; }

                public override Name Name => new Name.Normal("F");
                public override ImmutableArray<Param> Parameters => default;
                public override ParamHash ParamHash => new ParamHash(0, default);

                public override ValueTask InvokeAsync(Value? thisValue, ImmutableArray<Value> args, Value result)
                {
                    cmdProvider.Append("Hello World");
                    return ValueTask.CompletedTask;
                }
            }

            class RootTestContainer : IItemContainer
            {
                TestCommandProvider cmdProvider;

                public RootTestContainer(TestCommandProvider cmdProvider) { this.cmdProvider = cmdProvider; }

                public IItemContainer GetContainer(Name name, ParamHash paramHash)
                {
                    throw new NotImplementedException();
                }

                public TRuntimeItem GetRuntimeItem<TRuntimeItem>(Name name, ParamHash paramHash) 
                    where TRuntimeItem : RuntimeItem
                {
                    if (name == new Name.Normal("F"))
                    {
                        RuntimeItem ri = new TestFuncRuntimeItem(cmdProvider);
                        return (TRuntimeItem)ri;
                    }

                    throw new UnreachableCodeException();
                }
            }

            public ImmutableArray<(ModuleName, IItemContainer)> GetRootContainers()
            {
                return Arr<(ModuleName, IItemContainer)>((moduleName, new RootTestContainer(cmdProvider)));
            }
        }

        // 
        [Fact]
        public async Task CallFuncExp_CallVanillaExternal()
        {
            var testExternalModuleName = "TestExternalModule";
            var commandProvider = new TestCommandProvider();
            var testDriver = new TestDriver(testExternalModuleName, commandProvider);

            var func = new Path.Nested(new Path.Root(testExternalModuleName), new Name.Normal("F"), ParamHash.None, default);
            var script = RScript(moduleName, RPrintStringCmdStmt(new CallFuncExp(func, null, default)));

            var (output, _) = await EvalAsyncWithRetValue(commandProvider, Arr<IModuleDriver>(testDriver), script);
            Assert.Equal("Hello World", output);

            // System.Console.WriteLine()
        }

        // [System.Console]* 모든 것, Single
        class TestDotnetDriver : IModuleDriver
        {
            ModuleName moduleName; // System.Console
                                   // System.Console, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a

            public TestDotnetDriver(ModuleName moduleName)
            {
                this.moduleName = moduleName;
            }

            class TestFuncRuntimeItem : FuncRuntimeItem
            {
                MethodInfo methodInfo;
                public override Name Name => new Name.Normal(methodInfo.Name);
                public override ImmutableArray<Param> Parameters { get; }
                public override ParamHash ParamHash { get; }

                public TestFuncRuntimeItem(MethodInfo methodInfo) 
                { 
                    this.methodInfo = methodInfo;

                    int index = 0;
                    var paramInfos = methodInfo.GetParameters();
                    var builder = ImmutableArray.CreateBuilder<Param>(paramInfos.Length);
                    var pathsBuilder = ImmutableArray.CreateBuilder<ParamHashEntry>(paramInfos.Length);
                    foreach(var paramInfo in paramInfos)
                    {
                        var paramName = (paramInfo.Name != null) ? new Name.Normal(paramInfo.Name) : new Name.Normal($"@{index}");

                        builder.Add(new Param(ParamKind.Default, Path.String, paramName));

                        ParamKind paramKind;
                        if (paramInfo.ParameterType.IsByRef)
                            paramKind = ParamKind.Ref;
                        else
                            paramKind = ParamKind.Default;

                        pathsBuilder.Add(new ParamHashEntry(paramKind, Path.String));

                        index++;
                    }

                    Parameters = builder.MoveToImmutable();
                    ParamHash = new ParamHash(methodInfo.GetGenericArguments().Length, pathsBuilder.MoveToImmutable());
                }

                public override ValueTask InvokeAsync(Value? thisValue, ImmutableArray<Value> args, Value result)
                {
                    if (thisValue != null)
                        throw new NotImplementedException();

                    object[] dotnetArgs = new object[args.Length];
                    for (int i = 0; i < args.Length; i++)
                        dotnetArgs[i] = ((StringValue)args[i]).GetString();

                    methodInfo.Invoke(null, dotnetArgs);

                    // TODO: Task를 리턴하는 것이었으면.. await 시켜야

                    return ValueTask.CompletedTask;
                }
            }
           
            record TypeItemContainer(Type type) : IItemContainer
            {
                public IItemContainer GetContainer(Name name, ParamHash paramHash)
                {
                    throw new NotImplementedException();
                }

                bool Match(ParamHash paramHash, MethodInfo methodInfo)
                {
                    // paramHash랑 매칭
                    if (paramHash.TypeParamCount != methodInfo.GetGenericArguments().Length)
                        return false;

                    var methodParams = methodInfo.GetParameters();
                    if (paramHash.Entries.Length != methodParams.Length)
                        return false;

                    // matching
                    for (int i = 0; i < methodParams.Length; i++)
                    {
                        if (paramHash.Entries[i].Kind == ParamKind.Default &&
                            paramHash.Entries[i].Type == Path.String && 
                            methodParams[i].ParameterType == typeof(string))
                            continue;

                        // 나머지는 false
                        return false;
                    }

                    return true;
                }

                public TRuntimeItem GetRuntimeItem<TRuntimeItem>(Name name_name, ParamHash paramHash) where TRuntimeItem : RuntimeItem
                {
                    var name = (Name.Normal)name_name;

                    // WriteLine
                    var memberInfos = type.GetMember(name.Value);

                    foreach (var memberInfo in memberInfos)
                    {
                        var methodInfo = memberInfo as MethodInfo;
                        if (methodInfo == null)
                            continue;

                        if (Match(paramHash, methodInfo))
                        {
                            RuntimeItem ri = new TestFuncRuntimeItem(methodInfo);
                            return (TRuntimeItem)ri;
                        }
                    }

                    throw new InvalidOperationException();
                }
            }

            record NamespaceItemContainer(Assembly assembly, string namespaceName) : IItemContainer
            {
                public IItemContainer GetContainer(Name name_name, ParamHash paramHash)
                {
                    // namespaceName.name
                    Name.Normal name = (Name.Normal)name_name;

                    var fullName = $"{namespaceName}.{name.Value}";
                    var type = assembly.GetType(fullName); // TODO: paramHash고려
                    if (type != null)
                        return new TypeItemContainer(type);

                    return new NamespaceItemContainer(assembly, fullName);
                }

                public TRuntimeItem GetRuntimeItem<TRuntimeItem>(Name name, ParamHash paramHash) where TRuntimeItem : RuntimeItem
                {
                    throw new NotImplementedException();
                }
            }

            record AssemblyItemContainer(Assembly assembly) : IItemContainer
            {
                // namespace 구분이 없어서, type이 있는지 보고 없으면 네임스페이스로
                public IItemContainer GetContainer(Name name_name, ParamHash paramHash)
                {
                    Name.Normal name = (Name.Normal)name_name;
                    var type = assembly.GetType(name.Value); // TODO: paramHash고려
                    if (type != null)
                        return new TypeItemContainer(type);

                    return new NamespaceItemContainer(assembly, name.Value);
                }

                public TRuntimeItem GetRuntimeItem<TRuntimeItem>(Name name, ParamHash paramHash) where TRuntimeItem : RuntimeItem
                {
                    throw new NotImplementedException();
                }
            }

            public ImmutableArray<(ModuleName, IItemContainer)> GetRootContainers()
            {
                var assembly = Assembly.Load(new AssemblyName(moduleName.Value));

                return Arr<(ModuleName, IItemContainer)>((moduleName, new AssemblyItemContainer(assembly)));
            }
        }

        [Fact]
        public async Task CallFuncExp_CallDotnetExternal()
        {
            var testExternalModuleName = "System.Console";
            var testDriver = new TestDotnetDriver(testExternalModuleName);            

            var system = new Path.Nested(new Path.Root(testExternalModuleName), new Name.Normal("System"), ParamHash.None, default);
            var system_Console = new Path.Nested(system, new Name.Normal("Console"), ParamHash.None, default);
            var system_Console_Write = new Path.Nested(system_Console, new Name.Normal("Write"), new ParamHash(0, Arr(new ParamHashEntry(ParamKind.Default, Path.String))), default);
                
            var script = RScript(moduleName, new ExpStmt(new CallFuncExp(system_Console_Write, null, RArgs(RString("Hello World")))));

            using (var writer = new System.IO.StringWriter())
            {
                System.Console.SetOut(writer);

                var (output, _) = await EvalAsyncWithRetValue(Arr<IModuleDriver>(testDriver), script);
                
                Assert.Equal("Hello World", writer.GetStringBuilder().ToString());
            }

            // System.Console.WriteLine()
        }

        // CallSeqFunc
        [Fact]
        public async Task CallSeqFuncExp_ReturnsAnonymousSeqTypeValue()
        {
            var seqFunc = RootPath("F");
            var seqFuncDecl = new SequenceFuncDecl(default, new Name.Normal("F"), false, Path.String, default, default, BlankStmt.Instance); // return nothing

            var stmts = Arr<Stmt>
            (
                // 선언하자 마자 대입하기
                RLocalVarDeclStmt(seqFunc, "x", new CallSeqFuncExp(seqFunc, null, default)),

                // 로컬 변수에 새 값 대입하기
                new ExpStmt(new AssignExp(RLocalVarLoc("x"), new CallSeqFuncExp(seqFunc, null, default)))
            );

            await EvalAsync(seqFuncDecl, stmts);

            // 에러가 안났으면 성공..
        }
        
        [Fact]
        public async Task CallValueExp_EvaluateCallableAndArgumentsInOrder()
        {
            var printFunc = RootPath("Print", Arr(Path.Int), default);
            var makeLambda = RootPath("MakeLambda", Arr<Path>(), default);
            var lambda = new Path.Nested(makeLambda, new Name.Anonymous(0), ParamHash.None, default);

            // Print(int x) { 
            var printFuncDecl = new NormalFuncDecl(default, new Name.Normal("Print"), false, default, RNormalParams((Path.Int, "x")),
                RBlock(
                    RPrintIntCmdStmt(new LocalVarLoc(new Name.Normal("x"))),
                    new ReturnStmt(new ReturnInfo.Expression(new LoadExp(RLocalVarLoc("x"))))
                )
            );

            // MakeLambda() { return (int i, int j, int k) => @"TestFunc";}
            var lambdaDecl = new LambdaDecl(
                new Name.Anonymous(0),
                new CapturedStatement(null, default, RPrintStringCmdStmt("TestFunc")),
                RNormalParams((Path.Int, "i"), (Path.Int, "j"), (Path.Int, "k"))
            );
            
            var makeLambdaDecl = new NormalFuncDecl(Arr<CallableMemberDecl>(lambdaDecl), new Name.Normal("MakeLambda"), false, default, default,
                RBlock(                    
                    RPrintStringCmdStmt("MakeLambda"),
                    new ReturnStmt(new ReturnInfo.Expression(new LambdaExp(lambda)))
                )
            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFunc, null, RArgs(new IntLiteralExp(v)));
            
            var stmts = Arr<Stmt>
            (
                new ExpStmt(
                    new CallValueExp(
                        lambda,
                        new TempLoc(new CallFuncExp(makeLambda, null, default), lambda),
                        RArgs(
                            MakePrintCall(1),
                            MakePrintCall(2),
                            MakePrintCall(3) 
                        )
                    )
                )
            );

            var output = await EvalAsync(Arr<FuncDecl>(printFuncDecl, makeLambdaDecl), stmts);
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
        public async Task LambdaExp_CapturesLocalVariablesWithCopying() // TODO: wrong, need to be fix
        {   
            // [x] () => @"$x";
            var lambdaDecl = new LambdaDecl(new Name.Anonymous(0), new CapturedStatement(null, Arr(new OuterLocalVarInfo(Path.Int, "x")), RPrintIntCmdStmt(new CapturedVarLoc("x"))), default);
            var lambda = RootPath(new Name.Anonymous(0));

            // int x = 3;
            // var func = () => x; // [ref int x = ref x] () => x;
            // x = 34;
            // func();
            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.Int, "x", new IntLiteralExp(3)),
                RLocalVarDeclStmt(lambda, "func", new LambdaExp(lambda)),

                new ExpStmt(new AssignExp(RLocalVarLoc("x"), new IntLiteralExp(34))),
                new ExpStmt(new CallValueExp(lambda, RLocalVarLoc("func"), default))
            );

            var output = await EvalAsync(Arr<CallableMemberDecl>(lambdaDecl), stmts);
            Assert.Equal("3", output);
        }

        // ListIndexerExp
        [Fact]
        public async Task ListIndexerExp_GetElement()
        {
            var stmts = Arr<Stmt> (
                RLocalVarDeclStmt(Path.List(Path.Int), "list", new ListExp(Path.Int, Arr<Exp>(new IntLiteralExp(34), new IntLiteralExp(56)))),
                RPrintIntCmdStmt(new ListIndexerLoc(new LocalVarLoc(new Name.Normal("list")), new IntLiteralExp(1)))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("56", output);
        }

        // ListExp
        [Fact]
        public async Task ListExp_EvaluatesElementExpInOrder()
        {
            var printFunc = RootPath("Print", Arr(Path.Int), default);

            // print(int x)
            var printFuncDecl = new NormalFuncDecl(default, new Name.Normal("Print"), false, default, RNormalParams((Path.Int, "x")),

                RBlock(
                    RPrintIntCmdStmt(new LocalVarLoc(new Name.Normal("x"))),
                    new ReturnStmt(new ReturnInfo.Expression(new LoadExp(new LocalVarLoc(new Name.Normal("x")))))
                )
            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFunc, null, RArgs(new IntLiteralExp(v)));

            var stmts = Arr<Stmt> (
                RGlobalVarDeclStmt(Path.List(Path.Int), "l", 
                    new ListExp(Path.Int, Arr(
                        MakePrintCall(34),
                        MakePrintCall(56)
                    ))
                )
            );

            var output = await EvalAsync(printFuncDecl, stmts);
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
