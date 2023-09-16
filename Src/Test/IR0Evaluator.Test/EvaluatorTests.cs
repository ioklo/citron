using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

using Citron.Infra;
using Citron.Collections;
using Citron.IR0;
using Citron.Symbol;

using static Citron.Infra.Misc;
using static Citron.Test.Misc;

namespace Citron.Test
{
    public partial class EvaluatorTests
    {
        class TestCommandProvider : IIR0CommandProvider
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

        Name moduleName;
        SymbolFactory factory;
        IR0Factory r;
        ModuleDeclSymbol runtimeModuleD;
        
        public EvaluatorTests()
        {
            (moduleName, factory, r, runtimeModuleD) = TestPreparations.Prepare();
        }
        
        StmtBody AddGlobalFunc(ModuleDeclSymbol moduleD, IType retType, string name, params Stmt[] stmts)
        {
            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName(name), typeParams: default);
            funcD.InitFuncReturnAndParams(r.FuncRet(retType), parameters: default);
            moduleD.AddFunc(funcD);

            return r.StmtBody(funcD, stmts);
        }

        StmtBody AddGlobalFunc(ModuleDeclSymbol moduleD, IType retType, string name, ImmutableArray<(IType Type, string Name)> parameters, params Stmt[] stmts)
        {
            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName(name), typeParams: default);

            var parametersBuilder = ImmutableArray.CreateBuilder<FuncParameter>(parameters.Length);
            foreach(var param in parameters)
            {
                var parameter = new FuncParameter(param.Type, NormalName(param.Name));
                parametersBuilder.Add(parameter);
            }

            funcD.InitFuncReturnAndParams(r.FuncRet(retType), parametersBuilder.MoveToImmutable());
            moduleD.AddFunc(funcD);

            return r.StmtBody(funcD, stmts);
        }

        CommandStmt Sleep(int i)
        {
            return r.Command(r.String(
                r.TextElem("#Sleep"),
                r.ExpElem(
                    r.CallInternalUnary(
                        InternalUnaryOperator.ToString_Int_String,
                        r.Int(i)
                    )
                )
            ));
        }

        Loc IntLoc(int i)
        {
            return new TempLoc(r.Int(i), r.IntType());
        }

        // with script
        async Task<string> EvalAsync(Script script)
        {
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        // TopLevelStmt
        async Task<string> EvalAsync(ImmutableArray<Stmt> mainBody)
        {   
            var script = r.Script(mainBody);
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        //async Task<string> EvalAsync(ImmutableArray<StmtBody> stmtBodies, ImmutableArray<Stmt> topLevelStmts)
        //{
        //    var moduleDecl = new ModuleDeclSymbol(moduleName, default, default, default);

        //    var script = r.Script(moduleDecl, Arr(new StmtBody(new DeclSymbolPath(null, Name.TopLevel), topLevelStmts)));

        //    var script = r.Script(moduleName, stmtBodies, topLevelStmts);
        //    var (output, _) = await EvalAsyncWithRetValue(default, script);
        //    return output;
        //}

        //async Task<string> EvalAsync(StmtBody stmtBody, ImmutableArray<Stmt> topLevelStmts)
        //{
        //    var script = new Script(moduleName, Arr(stmtBody), topLevelStmts);
        //    var (output, _) = await EvalAsyncWithRetValue(default, script);
        //    return output;
        //}

        Task<(string Output, int RetValue)> EvalAsyncWithRetValue(
            ImmutableArray<Action<ModuleDriverContext>> moduleDriverInitializers, Script script)
        {
            var commandProvider = new TestCommandProvider();
            return EvalAsyncWithRetValue(commandProvider, moduleDriverInitializers, script);
        }

        async Task<(string Output, int RetValue)> EvalAsyncWithRetValue(TestCommandProvider commandProvider, ImmutableArray<Action<ModuleDriverContext>> moduleDriverInitializers, Script script)
        {   
            // moduleDrivers에 추가
            moduleDriverInitializers = moduleDriverInitializers.Add(driverContext =>
            {
                var evaluator = driverContext.GetEvaluator();

                var symbolFactory = new SymbolFactory();
                var symbolLoader = IR0Loader.Make(symbolFactory, script);
                var globalContext = new IR0GlobalContext(evaluator, symbolLoader, moduleName, commandProvider);

                IR0ModuleDriver.Init(driverContext, evaluator, globalContext, script.ModuleDeclSymbol.GetName());
            });

            // entry는 void Main()
            var entry = new SymbolId(moduleName, new SymbolPath(null, NormalName("Main")));

            var retValue = await Evaluator.EvalAsync(moduleDriverInitializers, entry);
            return (commandProvider.GetOutput(), retValue);
        }

        // without typeArgs
        //Path.Nested RootPath(string name)
        //{
        //    return new Path.Nested(new Path.Root(moduleName), new Name.Normal(name), ParamHash.None, default);
        //}

        //Path.Nested RootPath(Name name)
        //{
        //    return new Path.Nested(new Path.Root(moduleName), name, ParamHash.None, default);
        //}

        //Path.Nested RootPath(string name, ImmutableArray<ParamHashEntry> paramWithoutNames, ImmutableArray<Path> typeArgs)
        //{
        //    return new Path.Nested(new Path.Root(moduleName), new Name.Normal(name), new ParamHash(typeArgs.Length, paramWithoutNames), typeArgs);
        //}

        //Path.Nested RootPath(string name, ImmutableArray<Path> paramTypes, ImmutableArray<Path> typeArgs)
        //{
        //    return new Path.Nested(
        //        new Path.Root(moduleName), new Name.Normal(name),
        //        new ParamHash(
        //            typeArgs.Length, paramTypes.Select(paramType => new ParamHashEntry(ParamKind.Default, paramType)).ToImmutableArray()
        //        ),
        //        typeArgs);
        //}


        [Fact]
        public async Task CommandStmt_WorksProperly()
        {
            var mainBody = Arr<Stmt>(r.Command(r.String("Hello World")));
            var output = await EvalAsync(mainBody);

            Assert.Equal("Hello World", output);
        }

        // Stmt
        
        // TODO: sequence를 살리면 다시 살린다
        // SeqCall
        // seq int F(int x, int y) { yield x * 2; yield y + 3; } // struct anonymous_sequence_#1
        // foreach(var e in F(1, 2)) // e의 타입은 int,
        //    CommandStmt
        // 
        // seq func를 어떻게 할건데
        // seq<int> F(int x, int y)
        // {
        //     // 결국 Seq만드는 특수 명령어를 넣어야 할 것 같다;
        //     return MakeSeq( [x, y],  yield x * 2; yield y + 3; } );
        // }
        [Fact]
        public Task CallSeqFuncExp_GenerateSequencesInForeach()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Sequence);        
        //    // Sequence
        //    var seqFunc = new SequenceFuncDecl(default, new Name.Normal("F"), false, r.IntType(), default, Arr(new Param(ParamKind.Default, r.IntType(), new Name.Normal("x")), new Param(ParamKind.Default, r.IntType(), new Name.Normal("y"))), r.Block(

        //        new YieldStmt(
        //            r.CallInternalBinary(InternalBinaryOperator.Multiply_Int_Int_Int,
        //                new LoadExp(r.LocalVar("x")),
        //                r.Int(2))),

        //        new YieldStmt(
        //            r.CallInternalBinary(InternalBinaryOperator.Add_Int_Int_Int,
        //                new LoadExp(r.LocalVar("y")),
        //                r.Int(3)))));

        //    var funcF = RootPath("F", Arr(r.IntType(), r.IntType()), default);
        //    var seqTypeF = funcF;

        //    var stmts = Arr<Stmt>
        //    (
        //        new ForeachStmt(
        //            r.IntType(),
        //            "e",
        //            new TempLoc(
        //                new CallSeqFuncExp(
        //                    funcF,
        //                    null,
        //                    RArgs(
        //                        r.Int(1),
        //                        r.Int(2)
        //                    )
        //                ),

        //                seqTypeF
        //            ),

        //            r.PrintInt(r.LocalVar("e"))
        //        )
        //    );

        //    var output = await EvalAsync(seqFunc, stmts);

        //    Assert.Equal("25", output);
        }

        #region LocalVarDeclStmt        
        [Fact]
        public async Task LocalVarDeclStmt_OverlapsParentScope()
        {
            // int x = 23;
            // {
            //     string x = "Hello";
            //     @$x
            // }
            // @$x

            var mainBody = Arr<Stmt>(

                r.LocalVarDecl(r.IntType(), "x", r.Int(23)),

                r.Block(
                    r.LocalVarDecl(r.StringType(), "x", r.String("Hello")),
                    r.PrintString(r.LocalVar("x"))
                ),

                r.PrintInt(r.LocalVar("x"))
            );

            var output = await EvalAsync(mainBody);
            Assert.Equal("Hello23", output);
        }

        #endregion LocalVarDeclStmt

        #region IfStmt        
        [Fact]
        public async Task IfStmt_SelectThenBranchWhenConditionTrue()
        {
            // if(true) @True
            var stmts = Arr<Stmt>(r.If(r.Bool(true), r.PrintString("True")));
            var output = await EvalAsync(stmts);

            Assert.Equal("True", output);
        }

        [Fact]
        public async Task IfStmt_SelectElseBranchWhenConditionFalse()
        {
            // if (false) else @False
            var stmts = Arr<Stmt>(r.If(r.Bool(false), r.Blank(), r.PrintString("False")));
            var output = await EvalAsync(stmts);

            Assert.Equal("False", output);
        }

        #endregion If

        #region IfTestEnum
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

        #endregion IfTestEnum

        #region IfTestClass
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

        #endregion IfTestClass

        #region For

        // var decl for initializer, 
        [Fact]
        public async Task VarDeclForStmtInitializer_DeclaresLocalVariable()
        {
            var stmts = Arr<Stmt> (

                r.LocalVarDecl(r.IntType(), "x", r.Int(34)),

                r.For(
                    r.IntType(), "x", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.Equal_Int_Int_Bool, r.LoadLocalVar("x", r.IntType()), r.Int(0)),
                    r.AssignExp(r.LocalVar("x"), r.Int(1)),
                    r.PrintInt(r.LocalVar("x"))),

                r.PrintInt(r.LocalVar("x"))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("034", output);
        }

        [Fact]
        public async Task ExpForStmtInitializer_EvaluateAtStart()
        {
            // int x = 34
            // @$x
            // for(x = 12; x == 12; x = 1)
            //     @$x

            var stmts = Arr<Stmt> (

                r.LocalVarDecl(r.IntType(), "x", r.Int(34)),

                r.PrintInt(r.LocalVar("x")),

                r.For(
                    Arr<Stmt>(r.Assign(r.LocalVar("x"), r.Int(12))),
                    r.CallInternalBinary(InternalBinaryOperator.Equal_Int_Int_Bool, r.LoadLocalVar("x", r.IntType()), r.Int(12)),
                    r.AssignExp(r.LocalVar("x"), r.Int(1)),
                    r.PrintInt(r.LocalVar("x"))
                ),

                r.PrintInt(r.LocalVar("x"))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("34121", output);
        }

        [Fact]
        public async Task ForStmt_EvalConditionAtStart()
        {
            var stmts = Arr<Stmt> (
                
                r.For(
                    r.Bool(false),
                    null,
                    r.PrintString("Wrong")),

                r.PrintString("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAtEndOfBody()
        {
            var stmts = Arr<Stmt> (

                r.LocalVarDecl(r.BoolType(), "x", r.Bool(true)),

                r.For(
                    r.LoadLocalVar("x", r.BoolType()),
                    r.AssignExp(r.LocalVar("x"), r.Bool(false)),
                    r.PrintString("Once")),

                r.PrintString("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_EvalContinueExpAndEvalCondExpAfterEvalContinueStmt()
        {
            // (local) bool x = true;
            // for(; x;x = false)
            // {
            //     @Once
            //     continue;
            //     @Wrong
            // }
            // @Completed

            var stmts = Arr<Stmt> (

                r.LocalVarDecl(r.BoolType(), "x", r.Bool(true)),

                r.For(
                    r.LoadLocalVar("x", r.BoolType()),
                    r.AssignExp(r.LocalVar("x"), r.Bool(false)),
                    r.Block(
                        r.PrintString("Once"),
                        new ContinueStmt(),
                        r.PrintString("Wrong")
                    )
                ),

                r.PrintString("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("OnceCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalBreakStmt()
        {
            // (local) bool x = true;
            // for(; x; x = false)
            // {
            //     @Once
            //     break;
            //     @Wrong
            // }
            // @$x
            // @Completed

            var stmts = Arr<Stmt> (

                r.LocalVarDecl(r.BoolType(), "x", r.Bool(true)),

                r.For(
                    r.LoadLocalVar("x", r.BoolType()),
                    r.AssignExp(r.LocalVar("x"), r.Bool(false)),
                    r.Block(
                        r.PrintString("Once"),
                        r.Break(),
                        r.PrintString("Wrong")
                    )
                ),

                r.PrintBool(r.LocalVar("x")),
                r.PrintString("Completed")
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("OncetrueCompleted", output);
        }

        [Fact]
        public async Task ForStmt_ExitBodyImmediatelyAfterEvalReturnStmt()
        {
            // (local) bool x = true;
            // for(; x; x = false)
            // {
            //     @Once
            //     return 2;
            //     @Wrong
            // }
            // @Wrong
            var mainBody = Arr<Stmt>(
                r.LocalVarDecl(r.BoolType(), "x", r.Bool(true)),
                r.For(
                    r.LoadLocalVar("x", r.BoolType()),
                    r.AssignExp(r.LocalVar("x"), r.Bool(false)),
                    r.Block(
                        r.PrintString("Once"),
                        r.Return(r.Int(2)),                        
                        r.PrintString("Wrong")
                    )
                ),

                r.PrintString("Wrong")
            );

            var script = r.Script(mainBody);

            var (output, result) = await EvalAsyncWithRetValue(default, script);
            Assert.Equal("Once", output);
            Assert.Equal(2, result);
        }

        #endregion ForStmt

        #region Continue

        [Fact]
        public async Task ContinueStmt_ContinuesInnerMostLoopStmt()
        {
            var stmts = Arr<Stmt> (

                r.For(
                    r.IntType(), "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", r.IntType()), r.Int(2)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),

                    r.Block(

                        r.PrintInt(r.LocalVar("i")),

                        r.For(
                            r.IntType(), "j", r.Int(0),
                            r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("j", r.IntType()), r.Int(2)),
                            r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("j")),
                            r.Block(
                                r.PrintInt(r.LocalVar("j")),
                                new ContinueStmt(),
                                r.PrintInt(r.LocalVar("Wrong"))
                            )
                        )
                    )
                )
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("001101", output);
        }

        #endregion Continue

        #region Break

        [Fact]
        public async Task BreakStmt_ExitsInnerMostLoopStmt()
        {
            var stmts = Arr<Stmt> (

                r.For(
                    r.IntType(), "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", r.IntType()), r.Int(2)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),

                    r.Block(

                        r.PrintInt(r.LocalVar("i")),

                        r.For(
                            r.IntType(), "j", r.Int(0),
                            r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("j", r.IntType()), r.Int(2)),
                            r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("j")),
                            r.Block(
                                r.PrintInt(r.LocalVar("j")),
                                r.Break(),
                                r.PrintInt(r.LocalVar("Wrong"))
                            )
                        )
                    )
                )
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("0010", output);
        }

        #endregion Break

        #region Return        
        [Fact]
        public async Task ReturnStmt_ExitsFuncImmediately()
        {
            // void F()
            // {
            //     return;
            //     @Wrong
            // }
            // F();
            // @Completed

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var fD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("F"), typeParams: default);
            fD.InitFuncReturnAndParams(new FuncReturn(r.VoidType()), parameters: default);
            moduleD.AddFunc(fD);
            var f = (GlobalFuncSymbol)fD.MakeOpenSymbol(factory); // for calling
            
            var fBody = r.StmtBody(fD,
                r.Return(),
                r.PrintString("Wrong")
            );

            var mainBody = AddGlobalFunc(moduleD, r.VoidType(), "Main",
                r.Call(f),
                r.PrintString("Completed")
            );            
            var script = r.Script(moduleD, fBody, mainBody);
            
            var output = await EvalAsync(script);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ReturnStmt_ReturnProperlyInMain()
        {
            // return 34;

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var mainBody = AddGlobalFunc(moduleD, r.IntType(), "Main",
                r.Return(r.Int(34))
            );

            var script = r.Script(moduleD, mainBody);
            var (_, retValue) = await EvalAsyncWithRetValue(default, script);

            Assert.Equal(34, retValue);
        }

        [Fact]
        public async Task ReturnStmt_SetReturnValueToCaller()
        {
            // void F()
            // {
            //     return 77;
            //     @Wrong
            // }
            // F();

            // 1. decl
            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var fBody = AddGlobalFunc(moduleD, r.VoidType(), "F",
                r.Return(r.Int(77)),
                r.PrintString("Wrong")
            );

            var f = (GlobalFuncSymbol)fBody.DSymbol.MakeOpenSymbol(factory);

            var mainBody = AddGlobalFunc(moduleD, r.VoidType(), "Main",
                r.PrintInt(r.CallExp(f))
            );

            var script = r.Script(moduleD, fBody, mainBody);
            var output = await EvalAsync(script);
            Assert.Equal("77", output);
        }

        #endregion Return

        #region Block

        // BlockStmt에서 LocalVar 범위 테스트는 LocalVarDeclStmt_OverlapsParentScope에서 처리함
        [Fact]
        public async Task BlockStmt_EvalInnerStatementsSequencially()
        {
            var stmts = Arr<Stmt> (
                r.PrintInt(r.Int(1)),
                r.PrintInt(r.Int(23)),
                r.PrintInt(r.Int(4))
            );

            var output = await EvalAsync(stmts);

            Assert.Equal("1234", output);
        }

        #endregion Block

        #region ExpStmt

        // BlankStmt do nothing.. nothing을 테스트하기는 힘들다
        [Fact]
        public async Task ExpStmt_EvalInnerExp()
        {
            var stmts = Arr<Stmt> (
                r.LocalVarDecl(r.IntType(), "x", r.Int(0)),
                r.Assign(r.LocalVar("x"), r.Int(3)),
                r.PrintInt(r.LocalVar("x"))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("3", output);
        }

        #endregion ExpStmt

        #region Task

        // Task Await
        [Fact]
        public async Task TaskStmt_EvalParallel()
        {
            // await
            // {
            //     task
            //     {
            //         for(int i = 0; i < 5; i++)
            //             @$i
            //     }
            //     task
            //     {
            //         for(int i = 0; i < 5; i++)
            //             @$i
            //     }
            // }

            Stmt PrintNumbersStmt(int count)
            {
                return r.For(
                    r.IntType(), "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", r.IntType()), r.Int(count)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),
                    r.PrintInt(r.LocalVar("i"))
                );
            }

            // decl and body
            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var mainD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Main"), typeParams: default);
            mainD.InitFuncReturnAndParams(new FuncReturn(r.VoidType()), parameters: default);
            moduleD.AddFunc(mainD);

            var lambdaD0 = new LambdaDeclSymbol(mainD, new Name.Anonymous(0), parameters: default);
            lambdaD0.InitReturn(new FuncReturn(r.VoidType()));
            ((IFuncDeclSymbol)mainD).AddLambda(lambdaD0);
            var lambda0 = (LambdaSymbol)lambdaD0.MakeOpenSymbol(factory);

            var lambdaD1 = new LambdaDeclSymbol(mainD, new Name.Anonymous(1), parameters: default);
            lambdaD0.InitReturn(new FuncReturn(r.VoidType()));
            ((IFuncDeclSymbol)mainD).AddLambda(lambdaD1);
            var lambda1 = (LambdaSymbol)lambdaD1.MakeOpenSymbol(factory);

            // decl
            var lambdaBody0 = r.StmtBody(lambdaD0, PrintNumbersStmt(5));
            var lambdaBody1 = r.StmtBody(lambdaD1, PrintNumbersStmt(5));

            var mainBody = r.StmtBody(mainD,
                r.Await(
                    r.Task(lambda0),
                    r.Task(lambda1)
                )
            );

            var script = r.Script(moduleD, lambdaBody0, lambdaBody1, mainBody);

            var output = await EvalAsync(script);

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
        public Task TaskStmt_CapturesLocalVariable()
        {
            // box var* count = box 5;
            // await 
            // {
            //     task 
            //     {
            //          for(int i = 0; i < *count; i++) i++;
            //     }
            //     count = 4;
            //     task 
            //     {
            //          for(int i = 0; i < *count; i++) i++;
            //     }
            // }

            // box deref가 구현되면 살린다
            throw new PrerequisiteRequiredException(Prerequisite.BoxPtr);

            //// for(int i = 0; i < *labmda.count; i++) i++;
            //Stmt PrintNumbersStmt(LambdaMemberVarSymbol memberVar)
            //{
                

            //    return r.For(
            //        r.IntType(), "i", r.Int(0),
            //        r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", r.IntType()), r.Load(r.BoxDeref(r.Load(r.LambdaMember(memberVar), r.BoxRefType()), r.IntType())),
            //        r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),
            //        r.PrintInt(r.LocalVar("i"))
            //    );
            //}

            //// decl and body
            //var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            //var mainD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Main"), typeParams: default);
            //mainD.InitFuncReturnAndParams(new FuncReturn(r.VoidType()), parameters: default);
            //moduleD.AddFunc(mainD);

            //var lambdaD0 = new LambdaDeclSymbol(mainD, new Name.Anonymous(0), parameters: default);
            //lambdaD0.InitReturn(new FuncReturn(r.VoidType()));
            //((IFuncDeclSymbol)mainD).AddLambda(lambdaD0);
            //var lambdaCountD0 = new LambdaMemberVarDeclSymbol(lambdaD0, r.BoxRefType(r.IntType()), NormalName("count"));
            //lambdaD0.AddMemberVar(lambdaCountD0);
            //var lambda0 = (LambdaSymbol)lambdaD0.MakeOpenSymbol(factory);
            //var lambdaCount0 = factory.MakeLambdaMemberVar(lambda0, lambdaCountD0);

            //var lambdaD1 = new LambdaDeclSymbol(mainD, new Name.Anonymous(1), parameters: default);
            //lambdaD0.InitReturn(new FuncReturn(r.VoidType()));
            //((IFuncDeclSymbol)mainD).AddLambda(lambdaD1);
            //var lambdaCountD1 = new LambdaMemberVarDeclSymbol(lambdaD1, r.BoxRefType(r.IntType()), NormalName("count"));
            //lambdaD1.AddMemberVar(lambdaCountD1);
            //var lambda1 = (LambdaSymbol)lambdaD1.MakeOpenSymbol(factory);
            //var lambdaCount1 = factory.MakeLambdaMemberVar(lambda1, lambdaCountD1);

            //// decl
            //var lambdaBody0 = r.StmtBody(lambdaD0, PrintNumbersStmt(lambdaCount0));
            //var lambdaBody1 = r.StmtBody(lambdaD1, PrintNumbersStmt(lambdaCount1));

            //var mainBody = r.StmtBody(mainD,
            //    r.LocalVarDecl(r.BoxRefType(r.IntType()), "count", r.Box(r.Int(5))), // box int& count = box 5

            //    r.Await(
            //        r.Task(lambda0, r.Arg(r.LoadLocalVar("count", r.BoxRefType(r.IntType())))),
            //        r.Assign(r.Deref(r.LocalVar("count")), r.Int(4)), // *count = 4;
            //        r.Task(lambda1, r.Arg(r.LoadLocalVar("count", r.BoxRefType(r.IntType()))))
            //    )
            //);

            //var script = r.Script(moduleD, lambdaBody0, lambdaBody1, mainBody);
            //var output = await EvalAsync(script);

            //// 01234 01234 두개가 그냥 섞여 있을 것이다.

            //char cur0 = '0';
            //char cur1 = '0';

            //foreach (var c in output)
            //{
            //    if (c == cur0)
            //        cur0++;
            //    else if (c == cur1)
            //        cur1++;
            //    else
            //    {
            //        Assert.True(false, "순서가 맞지 않습니다");
            //        break;
            //    }
            //}

            //Assert.True((cur0 == '5' && cur1 == '4') || (cur0 == '4' && cur1 == '5'));
        }

        #endregion Task

        #region Async

        // Async Await
        [Fact]
        public async Task AsyncStmt_EvalAsynchronously()
        {
            // await 
            // {
            //     async
            //     {
            //         for(int i = 0; i < 5; i++)
            //         {
            //             @$i
            //             @Sleep
            //         }   
            //     }
            //     async
            //     {
            //         for(int i = 0; i < 5; i++)
            //         {
            //             @$i
            //             @Sleep
            //         }   
            //     }
            // }

            Stmt PrintNumbersStmt(int count)
            {
                return r.For(
                    r.IntType(), "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", r.IntType()), r.Int(count)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),
                    r.Block(
                        r.PrintInt(r.LocalVar("i")),
                        Sleep(1)
                    )
                );
            }
            
            // decl and body
            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var mainD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Main"), typeParams: default);
            mainD.InitFuncReturnAndParams(new FuncReturn(r.VoidType()), parameters: default);
            moduleD.AddFunc(mainD);

            var lambdaD0 = new LambdaDeclSymbol(mainD, new Name.Anonymous(0), parameters: default);
            lambdaD0.InitReturn(new FuncReturn(r.VoidType()));
            ((IFuncDeclSymbol)mainD).AddLambda(lambdaD0);
            var lambda0 = (LambdaSymbol)lambdaD0.MakeOpenSymbol(factory);

            var lambdaD1 = new LambdaDeclSymbol(mainD, new Name.Anonymous(1), parameters: default);
            lambdaD0.InitReturn(new FuncReturn(r.VoidType()));
            ((IFuncDeclSymbol)mainD).AddLambda(lambdaD1);
            var lambda1 = (LambdaSymbol)lambdaD1.MakeOpenSymbol(factory);

            // decl
            var lambdaBody0 = r.StmtBody(lambdaD0, PrintNumbersStmt(5));
            var lambdaBody1 = r.StmtBody(lambdaD1, PrintNumbersStmt(5));

            var mainBody = r.StmtBody(mainD,
                r.Await(
                    r.Async(lambda0),
                    r.Async(lambda1)
                )
            );

            var script = r.Script(moduleD, lambdaBody0, lambdaBody1, mainBody);

            var output = await EvalAsync(script);
            Assert.Equal("0011223344", output);
        }

        #endregion Async

        #region Yield        
        [Fact]
        public Task YieldStmt_GenerateElementForInnerMostForeachStmt()
        {
            // seq int F0() { ... }
            // seq int F1() { ... }

            throw new PrerequisiteRequiredException(Prerequisite.Sequence);

            //var seqFunc0 = RootPath("F0");
            //var seqFunc1 = RootPath("F1");

            //var seqFuncDecl0 = new SequenceFuncDecl(default, new Name.Normal("F0"), false, r.IntType(), default, default,
            //    new ForeachStmt(
            //        r.IntType(), "elem", 
            //        new TempLoc(new CallSeqFuncExp(seqFunc1, null, default), seqFunc1),
            //        r.Block(
            //            r.PrintInt(r.LocalVar("elem")),
            //            new YieldStmt(r.LoadLocalVar("elem", r.IntType()))
            //        )
            //    )
            //);

            //var seqFuncDecl1 = new SequenceFuncDecl(default, new Name.Normal("F1"), false, r.IntType(), default, default,
            //    r.Block(
            //        new YieldStmt(r.Int(34)),
            //        new YieldStmt(r.Int(56))
            //    )
            //);

            //var stmts = Arr<Stmt>(
            //    new ForeachStmt(r.IntType(), "x", new TempLoc(new CallSeqFuncExp(seqFunc0, null, default), seqFunc0),
            //        r.PrintInt(r.LocalVar("x")))
            //);

            //var output = await EvalAsync(Arr<FuncDecl>(seqFuncDecl0, seqFuncDecl1), stmts);

            //Assert.Equal("34345656", output);
        }

        #endregion Yield

        #region Static

        [Fact]
        public Task StaticMemberExp_GetStaticMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Static);
        }

        // NewStruct
        [Fact]
        public Task NewStructExp_MakesStructValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Struct);
        }

        #endregion static


        #region Struct

        [Fact]
        public Task StructMemberExp_GetStructMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Struct);
        }

        #endregion Struct

        #region Class

        [Fact]
        public Task ClassMemberExp_GetClassMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Class);
        }

        // NewClass
        [Fact]
        public Task NewClassExp_MakesClassValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Class);
        }

        #endregion Class

        #region Enum

        [Fact]
        public Task EnumMemberExp_GetEnumMemberValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }

        // NewEnum
        [Fact]
        public Task NewEnumExp_MakesEnumValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }

        #endregion Enum

        #region Literal

        [Fact]
        public async Task BoolLiteralExp_MakesBoolValue()
        {
            // @${false}
            // @${true}

            var stmts = Arr<Stmt> (            
                r.PrintBool(r.Bool(false)),
                r.PrintBool(r.Bool(true))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("falsetrue", output);
        }

        [Fact]
        public async Task IntLiteralExp_MakesIntValue()
        {
            // @${-2}

            var stmts = Arr<Stmt>(
                r.PrintInt(r.Int(-2))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("-2", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingTextStringExpElements()
        {
            // syntax로 표현 불가능
            // @Hello ${}World

            var stmts = Arr<Stmt>(
                r.PrintString(r.String(
                    r.TextElem("Hello "),
                    r.TextElem("World")
                ))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public async Task StringExp_ConcatStringsUsingStringExpElements()
        {   
            // string x = "New ";
            // @Hello $xWorld

            var stmts = Arr<Stmt>
            (
                r.LocalVarDecl(r.StringType(), "x", r.String("New ")),

                r.PrintString(r.String(
                    r.TextElem("Hello "),
                    r.ExpElem(r.LoadLocalVar("x", r.StringType())),
                    r.TextElem("World")
                ))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("Hello New World", output);
        }

        #endregion Literal

        #region Assign

        [Fact]
        public async Task AssignExp_ReturnsValue()
        {
            var stmts = Arr<Stmt>
            (
                r.LocalVarDecl(r.IntType(), "x", r.Int(3)),
                r.LocalVarDecl(r.IntType(), "y", r.Int(4)),

                // y = x = 10;
                r.Assign(r.LocalVar("y"), new AssignExp(r.LocalVar("x"), r.Int(10))),
                r.PrintInt(r.LocalVar("x")),
                r.PrintInt(r.LocalVar("y"))
            );

            var output = await EvalAsync(stmts);

            Assert.Equal("1010", output);
        }

        #endregion Assign

        #region Call

        [Fact]
        public async Task CallFuncExp_CallByRef_WorksProperly()
        {
            //void F(int* i) // local ptr
            //{
            //    *i = 7;
            //}

            //int j = 3;
            //F(&j);
            //@$j

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var fBody = AddGlobalFunc(moduleD, r.VoidType(), "F", Arr(((IType)r.LocalPtrType(r.IntType()), "i")),
                r.Assign(r.LocalDeref(r.LoadLocalVar("i", r.LocalPtrType(r.IntType()))), r.Int(7))
            );
            var f = (GlobalFuncSymbol)fBody.DSymbol.MakeOpenSymbol(factory);

            var mainBody = AddGlobalFunc(moduleD, r.VoidType(), "Main",
                r.LocalVarDecl(r.IntType(), "j", r.Int(3)),
                r.Call(f, r.Arg(r.LocalRef(r.LocalVar("j"), r.IntType()))),
                r.PrintInt(r.LocalVar("j"))
            );

            var script = r.Script(moduleD, fBody, mainBody);

            var output = await EvalAsync(script);
            Assert.Equal("7", output);
        }

        // GlobalVar가 사라져서 삭제
        //[Fact]
        //public async Task CallFuncExp_ReturnGlobalRef_WorksProperly()
        //{
        //    // int x = 3;
        //    // int& G() { return x; } // return
        //    // var& i = G();
        //    // i = 4;
        //    // @$x

        //    // decl
        //    var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
        //        .GlobalFunc(voidType, "G", out var gFuncDecl)
        //        .Make();

        //    var module = factory.MakeModule(moduleDecl);
        //    var gFunc = factory.MakeGlobalFunc(module, gFuncDecl, default);

        //    var script = neDerefExpw ScriptBuilder(moduleDecl)
        //        .Add(gFuncDecl, r.ReturnRef(r.GlobalVar("x")))
        //        .AddTopLevel(
        //            r.GlobalVarDecl(r.IntType(), "x", r.Int(3)),
        //            r.GlobalRefVarDecl("i", new DerefExpLoc(r.CallExp(gFunc))), // var i = ref G();
        //            r.Assign(new DerefLocLoc(r.GlobalVar("i")), r.Int(4)),      // i = deref(i)
        //            r.PrintInt(r.GlobalVar("x"))
        //        )
        //        .Make();

        //    var output = await EvalAsync(script);
        //    Assert.Equal("4", output);
        //}

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
            // 
            // int Print(int x)
            // {
            //     @$x
            //     return x;
            // }
            // TestFunc(int i, int j, int k)
            // {
            //     @TestFunc
            // }
            //
            // TestFunc(Print(1), Print(2), Print(3));

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var printBody = AddGlobalFunc(moduleD, r.IntType(), "Print", Arr((r.IntType(), "x")),
                r.PrintInt(r.LocalVar("x")),
                r.Return(r.LoadLocalVar("x", r.IntType()))
            );
            var testFuncBody = AddGlobalFunc(moduleD, r.VoidType(), "TestFunc", Arr((r.IntType(), "i"), (r.IntType(), "j"), (r.IntType(), "k")),
                r.PrintString("TestFunc")
            );

            var print = (GlobalFuncSymbol)printBody.DSymbol.MakeOpenSymbol(factory);
            var testFunc = (GlobalFuncSymbol)testFuncBody.DSymbol.MakeOpenSymbol(factory);

            Argument MakeArg(int v)
            {
                return r.Arg(r.CallExp(print, r.Arg(r.Int(v))));
            }

            var mainBody = AddGlobalFunc(moduleD, r.VoidType(), "Main",
                 r.Call(testFunc, MakeArg(1), MakeArg(2), MakeArg(3))
            );

            var script = r.Script(moduleD, printBody, testFuncBody, mainBody);

            var output = await EvalAsync(script);
            Assert.Equal("123TestFunc", output);
        }

        [Fact]
        public async Task CallFuncExp_ReturnsValueProperly()
        {
            // string F() { return "hello world"; }
            // @${F()}

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var fBody = AddGlobalFunc(moduleD, r.StringType(), "F", r.Return(r.String("hello world")));
            var f = (GlobalFuncSymbol)fBody.DSymbol.MakeOpenSymbol(factory);

            var mainBody = AddGlobalFunc(moduleD, r.VoidType(), "Main",
                r.PrintString(r.CallExp(f))
            );

            var script = r.Script(moduleD, fBody, mainBody);

            var output = await EvalAsync(script);
            Assert.Equal("hello world", output);
        }
        
        class TestDriver : IModuleDriver
        {            
            TestCommandProvider cmdProvider;

            public TestDriver(TestCommandProvider cmdProvider) 
            {
                this.cmdProvider = cmdProvider;
            }

            ValueTask IModuleDriver.ExecuteGlobalFuncAsync(SymbolId globalFuncId, ImmutableArray<Value> args, Value retValue)
            {
                cmdProvider.Append("Hello World");
                return ValueTask.CompletedTask;
            }

            #region Ignore

            Value IModuleDriver.Alloc(SymbolId type)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteClassConstructor(SymbolId constructor, ClassValue thisValue, ImmutableArray<Value> args)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteClassMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteStructConstructor(SymbolId constructor, LocalPtrValue thisValue, ImmutableArray<Value> args)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteStructMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue)
            {
                throw new NotImplementedException();
            }

            SymbolId? IModuleDriver.GetBaseClass(SymbolId baseClass)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetClassMemberVarIndex(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            Value IModuleDriver.GetClassStaticMemberValue(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetEnumElemMemberVarIndex(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetStructMemberVarIndex(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            Value IModuleDriver.GetStructStaticMemberValue(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetTotalClassMemberVarCount(SymbolId @class)
            {
                throw new NotImplementedException();
            }

            void IModuleDriver.InitializeClassInstance(SymbolId @class, ImmutableArray<Value>.Builder builder)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetTotalStructMemberVarCount(SymbolId structId)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        // 
        [Fact]
        public async Task CallFuncExp_CallVanillaExternal()
        {
            // TODO: "F"랑 TestDriver 연관을 시켜야 한다

            var commandProvider = new TestCommandProvider();

            // 외부 모듈 작성
            var testExternalModuleName = new Name.Normal("TestExternalModule");
            var testDriver = new TestDriver(commandProvider);
            var moduleInitializer = Arr<Action<ModuleDriverContext>>(
                context =>
                {
                    context.AddDriver(new TestDriver(commandProvider));
                }
            );

            // F();
            var testExternalModuleD = new ModuleDeclSymbol(testExternalModuleName, bReference: true);
            var testExternalModuleFD = new GlobalFuncDeclSymbol(testExternalModuleD, Accessor.Public, NormalName("F"), typeParams: default);
            var testExternalModuleF = (GlobalFuncSymbol)testExternalModuleFD.MakeOpenSymbol(factory);

            var script = r.Script(r.Call(testExternalModuleF));
            var (output, _) = await EvalAsyncWithRetValue(commandProvider, moduleInitializer, script);

            Assert.Equal("Hello World", output);
        }

        // [System.Console]* 모든 것, Single
        class TestDotnetDriver : IModuleDriver
        {
            Name moduleName; // System.Console
                             // System.Console, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a

            public TestDotnetDriver(Name moduleName)
            {
                this.moduleName = moduleName;
            }

            //class TestFuncRuntimeItem : FuncRuntimeItem
            //{
            //    MethodInfo methodInfo;
            //    public override Name Name => new Name.Normal(methodInfo.Name);
            //    public override ImmutableArray<Param> Parameters { get; }
            //    public override ParamHash ParamHash { get; }

            //    public TestFuncRuntimeItem(MethodInfo methodInfo) 
            //    { 
            //        this.methodInfo = methodInfo;

            //        int index = 0;
            //        var paramInfos = methodInfo.GetParameters();
            //        var builder = ImmutableArray.CreateBuilder<Param>(paramInfos.Length);
            //        var pathsBuilder = ImmutableArray.CreateBuilder<ParamHashEntry>(paramInfos.Length);
            //        foreach(var paramInfo in paramInfos)
            //        {
            //            var paramName = (paramInfo.Name != null) ? new Name.Normal(paramInfo.Name) : new Name.Normal($"@{index}");

            //            builder.Add(new Param(ParamKind.Default, stringType, paramName));

            //            ParamKind paramKind;
            //            if (paramInfo.ParameterType.IsByRef)
            //                paramKind = ParamKind.Ref;
            //            else
            //                paramKind = ParamKind.Default;

            //            pathsBuilder.Add(new ParamHashEntry(paramKind, stringType));

            //            index++;
            //        }

            //        Parameters = builder.MoveToImmutable();
            //        ParamHash = new ParamHash(methodInfo.GetGenericArguments().Length, pathsBuilder.MoveToImmutable());
            //    }

            //    public override ValueTask InvokeAsync(Value? thisValue, ImmutableArray<Value> args, Value result)
            //    {
            //        if (thisValue != null)
            //            throw new NotImplementedException();

            //        object[] dotnetArgs = new object[args.Length];
            //        for (int i = 0; i < args.Length; i++)
            //            dotnetArgs[i] = ((StringValue)args[i]).GetString();

            //        methodInfo.Invoke(null, dotnetArgs);

            //        // TODO: Task를 리턴하는 것이었으면.. await 시켜야

            //        return ValueTask.CompletedTask;
            //    }
            //}
           
            //record class TypeItemContainer(Type type) : IItemContainer
            //{
            //    public IItemContainer GetContainer(Name name, ParamHash paramHash)
            //    {
            //        throw new NotImplementedException();
            //    }

            //    bool Match(ParamHash paramHash, MethodInfo methodInfo)
            //    {
            //        // paramHash랑 매칭
            //        if (paramHash.TypeParamCount != methodInfo.GetGenericArguments().Length)
            //            return false;

            //        var methodParams = methodInfo.GetParameters();
            //        if (paramHash.Entries.Length != methodParams.Length)
            //            return false;

            //        // matching
            //        for (int i = 0; i < methodParams.Length; i++)
            //        {
            //            if (paramHash.Entries[i].Kind == ParamKind.Default &&
            //                paramHash.Entries[i].Type == stringType && 
            //                methodParams[i].ParameterType == typeof(string))
            //                continue;

            //            // 나머지는 false
            //            return false;
            //        }

            //        return true;
            //    }

            //    public TRuntimeItem GetRuntimeItem<TRuntimeItem>(Name name_name, ParamHash paramHash) where TRuntimeItem : RuntimeItem
            //    {
            //        var name = (Name.Normal)name_name;

            //        // WriteLine
            //        var memberInfos = type.GetMember(name.Value);

            //        foreach (var memberInfo in memberInfos)
            //        {
            //            var methodInfo = memberInfo as MethodInfo;
            //            if (methodInfo == null)
            //                continue;

            //            if (Match(paramHash, methodInfo))
            //            {
            //                RuntimeItem ri = new TestFuncRuntimeItem(methodInfo);
            //                return (TRuntimeItem)ri;
            //            }
            //        }

            //        throw new InvalidOperationException();
            //    }
            //}

            //record class NamespaceItemContainer(Assembly assembly, string namespaceName) : IItemContainer
            //{
            //    public IItemContainer GetContainer(Name name_name, ParamHash paramHash)
            //    {
            //        // namespaceName.name
            //        Name.Normal name = (Name.Normal)name_name;

            //        var fullName = $"{namespaceName}.{name.Value}";
            //        var type = assembly.GetType(fullName); // TODO: paramHash고려
            //        if (type != null)
            //            return new TypeItemContainer(type);

            //        return new NamespaceItemContainer(assembly, fullName);
            //    }

            //    public TRuntimeItem GetRuntimeItem<TRuntimeItem>(Name name, ParamHash paramHash) where TRuntimeItem : RuntimeItem
            //    {
            //        throw new NotImplementedException();
            //    }
            //}

            //record class AssemblyItemContainer(Assembly assembly) : IItemContainer
            //{
            //    // namespace 구분이 없어서, type이 있는지 보고 없으면 네임스페이스로
            //    public IItemContainer GetContainer(Name name_name, ParamHash paramHash)
            //    {
            //        Name.Normal name = (Name.Normal)name_name;
            //        var type = assembly.GetType(name.Value); // TODO: paramHash고려
            //        if (type != null)
            //            return new TypeItemContainer(type);

            //        return new NamespaceItemContainer(assembly, name.Value);
            //    }

            //    public TRuntimeItem GetRuntimeItem<TRuntimeItem>(Name name, ParamHash paramHash) where TRuntimeItem : RuntimeItem
            //    {
            //        throw new NotImplementedException();
            //    }
            //}

            //public ImmutableArray<(ModuleName, IItemContainer)> GetRootContainers()
            //{
            //    var assembly = Assembly.Load(new AssemblyName(moduleName.Value));

            //    return Arr<(ModuleName, IItemContainer)>((moduleName, new AssemblyItemContainer(assembly)));
            //}

            Value IModuleDriver.Alloc(SymbolId type)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteGlobalFuncAsync(SymbolId globalFunc, ImmutableArray<Value> args, Value retValue)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteClassConstructor(SymbolId constructor, ClassValue thisValue, ImmutableArray<Value> args)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteClassMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue)
            {
                throw new NotImplementedException();
            }

            void IModuleDriver.InitializeClassInstance(SymbolId @class, ImmutableArray<Value>.Builder builder)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetTotalClassMemberVarCount(SymbolId @class)
            {
                throw new NotImplementedException();
            }

            Value IModuleDriver.GetClassStaticMemberValue(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetClassMemberVarIndex(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            SymbolId? IModuleDriver.GetBaseClass(SymbolId baseClass)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteStructMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue)
            {
                throw new NotImplementedException();
            }

            ValueTask IModuleDriver.ExecuteStructConstructor(SymbolId constructor, LocalPtrValue thisValue, ImmutableArray<Value> args)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetStructMemberVarIndex(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            Value IModuleDriver.GetStructStaticMemberValue(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetEnumElemMemberVarIndex(SymbolId memberVar)
            {
                throw new NotImplementedException();
            }

            int IModuleDriver.GetTotalStructMemberVarCount(SymbolId structId)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task CallFuncExp_CallDotnetExternal()
        {
            var testExternalModuleName = new Name.Normal("System.Console");

            void TestDriverInitializer(ModuleDriverContext driverContext)
            {
                var driver = new TestDotnetDriver(testExternalModuleName);

                driverContext.AddDriver(driver);
                driverContext.AddModule(new Name.Normal("System.Console"), driver);
            }

            var testExternalModuleD = new ModuleDeclSymbol(testExternalModuleName, bReference: true);

            var systemD = new NamespaceDeclSymbol(testExternalModuleD, NormalName("System"));
            testExternalModuleD.AddNamespace(systemD);

            var consoleD = new ClassDeclSymbol(systemD, Accessor.Public, NormalName("Console"), typeParams: default);
            consoleD.InitBaseTypes(baseClass: null, interfaces: default);
            systemD.AddType(consoleD);

            var writeD = new ClassMemberFuncDeclSymbol(consoleD, Accessor.Public, NormalName("Write"), typeParams: default, bStatic: true);
            writeD.InitFuncReturnAndParams(new FuncReturn(r.VoidType()), Arr(new FuncParameter(r.StringType(), NormalName("arg"))));
            consoleD.AddFunc(writeD);

            var write = (ClassMemberFuncSymbol)writeD.MakeOpenSymbol(factory);

            var script = r.Script(
                r.Call(write, null, r.Arg(r.String("Hello World")))
            );

            using (var writer = new System.IO.StringWriter())
            {
                System.Console.SetOut(writer);

                var (output, _) = await EvalAsyncWithRetValue(Arr<Action<ModuleDriverContext>>(TestDriverInitializer), script);
                
                Assert.Equal("Hello World", writer.GetStringBuilder().ToString());
            }

            // System.Console.WriteLine()
        }

        // CallSeqFunc
        [Fact]
        public Task CallSeqFuncExp_ReturnsAnonymousSeqTypeValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Sequence);

            // seq string F() { }
            // var x = F();
            // x = F();

            //var seqFunc = RootPath("F");
            //var seqFuncDecl = new SequenceFuncDecl(default, new Name.Normal("F"), false, stringType, default, default, BlankStmt.Instance); // return nothing

            //var stmts = Arr<Stmt>
            //(
            //    // 선언하자 마자 대입하기
            //    r.LocalVarDecl(seqFunc, "x", new CallSeqFuncExp(seqFunc, null, default)),

            //    // 로컬 변수에 새 값 대입하기
            //    r.Assign(r.LocalVar("x"), new CallSeqFuncExp(seqFunc, null, default))
            //);

            //await EvalAsync(seqFuncDecl, stmts);

            // 에러가 안났으면 성공..
        }
        
        [Fact]
        public async Task CallValueExp_EvaluateCallableAndArgumentsInOrder()
        {
            // int Print(int x)
            // {
            //     @$x
            //     return x;
            // }
            // 
            // func<string, int, int, int> MakeLambda()  // FuncType
            // {
            //     // TODO: [9] 람다에 capture하는 변수가 없다면, 앞에 static을 붙여서 heap 할당을 하지 않게 만들 수 있게 한다.
            //     return box (int i, int j, int k) => @"TestFunc"; 
            // }            
            //
            // MakeLambda()(Print(1), Print(2), Print(3))

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var printBody = AddGlobalFunc(moduleD, r.IntType(), "Print", Arr((r.IntType(), "x")),
                r.PrintInt(new LocalVarLoc(new Name.Normal("x"))),
                r.Return(r.LoadLocalVar("x", r.IntType()))
            );

            var makeLambdaD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("MakeLambda"), typeParams: default);
            makeLambdaD.InitFuncReturnAndParams(new FuncReturn(r.FuncType(r.StringType(), r.IntType(), r.IntType(), r.IntType())), parameters: default);
            moduleD.AddFunc(makeLambdaD);

            var lambdaD = new LambdaDeclSymbol(makeLambdaD, new Name.Anonymous(0), r.FuncParams((r.IntType(), "i"), (r.IntType(), "j"), (r.IntType(), "k")));
            lambdaD.InitReturn(new FuncReturn(r.StringType()));
            ((IFuncDeclSymbol)makeLambdaD).AddLambda(lambdaD);

            var lambda = (LambdaSymbol)lambdaD.MakeOpenSymbol(factory);

            var makeLambdaBody = r.StmtBody(makeLambdaD,
                r.PrintString("MakeLambda"),
                r.Return(new CastBoxedLambdaToFuncExp(new BoxExp(new LambdaExp(lambda, Args: default), new LambdaType(lambda)), r.FuncType(r.StringType(), r.IntType(), r.IntType(), r.IntType()))) // TODO: interface cast
            );            

            var lambdaBody = r.StmtBody(lambdaD, r.PrintString("TestFunc"));

            var makeLambda = (GlobalFuncSymbol)makeLambdaBody.DSymbol.MakeOpenSymbol(factory);
            var print = (GlobalFuncSymbol)printBody.DSymbol.MakeOpenSymbol(factory);

            var mainBody = AddGlobalFunc(moduleD, r.VoidType(), "Main",
                r.Call(lambda, r.TempLoc(r.CallExp(makeLambda), new LambdaType(lambda)),
                    r.Arg(r.CallExp(print, r.Arg(r.Int(1)))),
                    r.Arg(r.CallExp(print, r.Arg(r.Int(2)))),
                    r.Arg(r.CallExp(print, r.Arg(r.Int(3))))
                )
            );

            var script = r.Script(moduleD, printBody, makeLambdaBody, lambdaBody, mainBody);

            var output = await EvalAsync(script);
            Assert.Equal("MakeLambda123TestFunc", output);
        }

        #endregion Call

        #region Lambda

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
            // int x = 3;
            // var func = [x] () => @$x; // [int x = x] () => x;
            // x = 34;
            // func();

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var mainD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Main"), typeParams: default);
            mainD.InitFuncReturnAndParams(new FuncReturn(r.VoidType()), parameters: default);
            moduleD.AddFunc(mainD);

            var lambdaD = new LambdaDeclSymbol(mainD, new Name.Anonymous(0), parameters: default);
            lambdaD.InitReturn(new FuncReturn(r.IntType()));
            ((IFuncDeclSymbol)mainD).AddLambda(lambdaD);

            var lambdaMemberVarD = new LambdaMemberVarDeclSymbol(lambdaD, r.IntType(), NormalName("x"));
            lambdaD.AddMemberVar(lambdaMemberVarD);

            var lambdaMemberVar = (LambdaMemberVarSymbol)lambdaMemberVarD.MakeOpenSymbol(factory);
            var lambdaBody = r.StmtBody(lambdaD,
                r.PrintInt(r.LoadLambdaMember(lambdaMemberVar))
            );

            var lambda = (LambdaSymbol)lambdaD.MakeOpenSymbol(factory);
            var mainBody = r.StmtBody(mainD,
                r.LocalVarDecl(r.IntType(), "x", r.Int(3)),
                r.LocalVarDecl(new LambdaType(lambda), "func", r.Lambda(lambda, r.Arg(r.LoadLocalVar("x", r.IntType())))),
                r.Assign(r.LocalVar("x"), r.Int(34)),
                r.Call(lambda, r.LocalVar("func"))
            );

            var script = r.Script(moduleD, mainBody, lambdaBody);
            var output = await EvalAsync(script);
            Assert.Equal("3", output);
        }

        #endregion Lambda

        #region List

        // ListIndexerExp
        [Fact]
        public async Task ListIndexerExp_GetElement()
        {
            // var list = [34, 56];
            // @${list[1]}

            var script = r.Script(
                r.LocalVarDecl(r.ListType(r.IntType()), "list", r.List(r.IntType(), r.Int(34), r.Int(56))),
                r.PrintInt(r.ListIndexer(r.LocalVar("list"), r.TempLoc(r.Int(1), r.IntType())))
            );
            
            var output = await EvalAsync(script);
            Assert.Equal("56", output);
        }

        // ListExp
        [Fact]
        public async Task ListExp_EvaluatesElementExpInOrder()
        {
            // int Print(int x)
            // {
            //     @$x
            //     return x;
            // }

            // var l = [Print(34), Print(56)];

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var printBody = AddGlobalFunc(moduleD, r.IntType(), "Print", Arr((r.IntType(), "x")),
                r.PrintInt(r.LocalVar("x")),
                r.Return(r.LoadLocalVar("x", r.IntType()))
            );

            var printFunc = (GlobalFuncSymbol)printBody.DSymbol.MakeOpenSymbol(factory);

            var mainBody = AddGlobalFunc(moduleD, r.VoidType(), "Main",
                r.LocalVarDecl(r.ListType(r.IntType()), "l",
                    r.List(r.IntType(),
                        r.CallExp(printFunc, r.Arg(r.Int(34))),
                        r.CallExp(printFunc, r.Arg(r.Int(56)))
                    )
                )
            );

            var script = r.Script(moduleD, printBody, mainBody);
            var output = await EvalAsync(script);
            Assert.Equal("3456", output);
        }

        #endregion List

        #region LocalPtr
        [Fact]
        public async Task LocalPtr_MakeLocalPtrFromLocalVariable()
        {
            // var i = 0;
            // var* x = &i;
            // *x = 3;
            // @$i // 3
            var script = r.Script(
                r.LocalVarDecl(r.IntType(), "i", r.Int(0)),
                r.LocalVarDecl(r.LocalPtrType(r.IntType()), "x", r.LocalRef(r.LocalVar("i"), r.IntType())),
                r.Assign(r.LocalDeref(r.LoadLocalVar("x", r.LocalPtrType(r.IntType()))), r.Int(3)),
                r.PrintInt(r.LoadLocalVar("i", r.IntType()))
            );

            var output = await EvalAsync(script);
            Assert.Equal("3", output);
        }

        #endregion LocalPtr

        #region BoxPtr
        // box 기본
        // 생성 1
        [Fact]
        public async Task BoxPtr_MakeBoxPtrFromValueAndDereference()
        {
            // box var* x = box 3;
            // @${*x}

            var script = r.Script(
                r.LocalVarDecl(r.BoxPtrType(r.IntType()), "x", r.Box(r.Int(3), r.IntType())),
                r.PrintInt(r.Load(r.BoxDeref(r.LocalVar("x")), r.IntType()))
            );

            var output = await EvalAsync(script);
            Assert.Equal("3", output);
        }

        [Fact]
        public async Task BoxPtr_MakeBoxPtrFromClassInstanceAndDereference()
        {
            // class C { int a; }
            // var c = new C();
            // box int* x = &c.a;
            // *x = 3;
            // print(c.a);

            var moduleDS = new ModuleDeclSymbol(moduleName, bReference: false);
            var cDS = new ClassDeclSymbol(moduleDS, Accessor.Public, NormalName("C"), typeParams: default);
            cDS.InitBaseTypes(baseClass: null, interfaces: default);
            moduleDS.AddType(cDS);

            var caDS = new ClassMemberVarDeclSymbol(cDS, Accessor.Public, bStatic: false, r.IntType(), NormalName("a"));
            cDS.AddMemberVar(caDS);

            var cconstructorDS = new ClassConstructorDeclSymbol(cDS, Accessor.Public, parameters: default, bTrivial: false, bLastParameterVariadic: false);
            cDS.AddConstructor(cconstructorDS);
            
            var cS = (ClassSymbol)cDS.MakeOpenSymbol(factory);
            var cconstructorS = (ClassConstructorSymbol)cconstructorDS.MakeOpenSymbol(factory);
            var caS = (ClassMemberVarSymbol)caDS.MakeOpenSymbol(factory);


            var boxIntPtr = r.BoxPtrType(r.IntType());

            var mainBody = AddGlobalFunc(moduleDS, r.VoidType(), "Main",
                r.LocalVarDecl(new ClassType(cS), "c", new NewClassExp(cconstructorS, Args: default)),
                r.LocalVarDecl(boxIntPtr, "x", new ClassMemberBoxRefExp(r.LocalVar("c"), caS)),
                r.Assign(r.BoxDeref(r.LocalVar("x")), r.Int(3)),
                r.PrintInt(new ClassMemberLoc(r.LocalVar("c"), caS))
            );

            var cconstructorBody = r.StmtBody(cconstructorDS,
                r.Assign(new ClassMemberLoc(new ThisLoc(), caS), r.Int(1))
            );

            var script = r.Script(moduleDS, cconstructorBody, mainBody);

            var output = await EvalAsync(script);
            Assert.Equal("3", output);
        }

        [Fact]
        public async Task BoxPtr_MakeBoxPtrFromBoxedStructMemberVarAndDereference()
        {
            // struct S { int a; }
            // box S* pS = box S();
            // box int* x = &(*pS).a;
            // *x = 3;
            // print((*pS).a);

            var moduleDS = new ModuleDeclSymbol(moduleName, bReference: false);
            var sDS = new StructDeclSymbol(moduleDS, Accessor.Public, NormalName("S"), typeParams: default);
            sDS.InitBaseTypes(baseStruct: null, interfaces: default);
            moduleDS.AddType(sDS);

            var saDS = new StructMemberVarDeclSymbol(sDS, Accessor.Public, bStatic: false, r.IntType(), NormalName("a"));
            sDS.AddMemberVar(saDS);

            var sconstructorDS = new StructConstructorDeclSymbol(sDS, Accessor.Public, parameters: default, bTrivial: false, bLastParameterVariadic: false);
            sDS.AddConstructor(sconstructorDS);

            var sS = (StructSymbol)sDS.MakeOpenSymbol(factory);
            var sconstructorS = (StructConstructorSymbol)sconstructorDS.MakeOpenSymbol(factory);
            var saS = (StructMemberVarSymbol)saDS.MakeOpenSymbol(factory);

            var boxSPtr = r.BoxPtrType(new StructType(sS));
            var boxIntPtr = r.BoxPtrType(r.IntType());

            var mainBody = AddGlobalFunc(moduleDS, r.VoidType(), "Main",
                r.LocalVarDecl(boxSPtr, "pS", new BoxExp(new NewStructExp(sconstructorS, Args: default), new StructType(sS))),
                r.LocalVarDecl(boxIntPtr, "x", new StructIndirectMemberBoxRefExp(r.LocalVar("pS"), saS)),
                r.Assign(r.BoxDeref(r.LocalVar("x")), r.Int(3)),
                r.PrintInt(new StructMemberLoc(r.BoxDeref(r.LocalVar("pS")), saS))
            );

            var sconstructorBody = r.StmtBody(sconstructorDS,
                r.Assign(new StructMemberLoc(new LocalDerefLoc(r.Load(new ThisLoc(), boxSPtr)), saS), r.Int(1))
            );

            var script = r.Script(moduleDS, sconstructorBody, mainBody);

            var output = await EvalAsync(script);
            Assert.Equal("3", output);
        }

        #endregion BoxPtr
    }
}
