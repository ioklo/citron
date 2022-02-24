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
using Citron.Test.Misc;
using Citron.CompileTime;

using static Citron.Infra.Misc;
using Citron.Analysis;

namespace Citron.Test
{
    public class EvaluatorTests
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

        Name moduleName = new Name.Normal("TestModule");
        SymbolFactory factory;
        FSymbolFactory ffactory;
        IR0Factory r;
        ITypeSymbol boolType, intType, stringType, voidType;
        
        public EvaluatorTests()
        {
            // [System.Runtime] System.Int32
            this.factory = new SymbolFactory();
            this.ffactory = new FSymbolFactory();

            boolType = factory.MakeBool();
            intType = factory.MakeInt();
            stringType = factory.MakeString();
            voidType = factory.MakeVoid();

            this.r = new IR0Factory(boolType, intType, stringType);
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
            return new TempLoc(r.Int(i));
        }

        // with script
        async Task<string> EvalAsync(Script script)
        {
            var (output, _) = await EvalAsyncWithRetValue(default, script);
            return output;
        }

        // TopLevelStmt
        async Task<string> EvalAsync(ImmutableArray<Stmt> topLevelStmts)
        {
            var script = r.Script(new ModuleDeclSymbol(moduleName, default, default, default), Arr(new StmtBody(new DeclSymbolPath(null, Name.TopLevel), topLevelStmts)));
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
            ImmutableArray<Func<ModuleDriverInitializationContext, IModuleDriver>> moduleDrivers, Script script)
        {
            var commandProvider = new TestCommandProvider();
            return EvalAsyncWithRetValue(commandProvider, moduleDrivers, script);
        }

        async Task<(string Output, int RetValue)> EvalAsyncWithRetValue(TestCommandProvider commandProvider, ImmutableArray<Func<ModuleDriverInitializationContext, IModuleDriver>> moduleDrivers, Script script)
        {   
            // moduleDrivers에 추가
            moduleDrivers = moduleDrivers.Add(initContext =>
            {
                var evaluator = new Evaluator();
                var symbolFactory = new SymbolFactory();
                var symbolLoader = IR0Loader.Make(symbolFactory, script);
                var globalContext = new IR0GlobalContext(evaluator, symbolLoader, moduleName, commandProvider);

                return IR0ModuleDriver.Make(initContext, evaluator, globalContext);
            });

            // entry는 TopLevel
            var entry = new ModuleSymbolId(moduleName, new SymbolPath(null, Name.TopLevel));

            var retValue = await Evaluator.EvalAsync(moduleDrivers, entry);
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
            var topLevelStmts = Arr<Stmt>(r.Command(r.String("Hello World")));
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
                r.GlobalVarDecl(intType, "x", r.Int(3)),
                r.PrintInt(r.GlobalVar("x"))
            );

            var output = await EvalAsync(topLevelStmts);
            Assert.Equal("3", output);
        }

        class LambdaFDeclBuilder
        {
            internal delegate LambdaFDeclSymbol OnFinish(ImmutableArray<LambdaMemberVarFDeclSymbol> memberVarDecls);
            
            FDeclsBuilder outerBuilder;
            OnFinish onFinish;

            Holder<LambdaFDeclSymbol> lambdaDeclHolder;
            ImmutableArray<LambdaMemberVarFDeclSymbol>.Builder builder;

            internal LambdaFDeclBuilder(FDeclsBuilder outerBuilder, OnFinish onFinish)
            {
                this.outerBuilder = outerBuilder;
                this.onFinish = onFinish;

                this.lambdaDeclHolder = new Holder<LambdaFDeclSymbol>();
                this.builder = ImmutableArray.CreateBuilder<LambdaMemberVarFDeclSymbol>();
            }
            
            public LambdaFDeclBuilder MemberVar(ITypeSymbol declType, string name, out LambdaMemberVarFDeclSymbol memberVarDecl)
            {
                memberVarDecl = new LambdaMemberVarFDeclSymbol(lambdaDeclHolder, declType, new Name.Normal(name));
                builder.Add(memberVarDecl);
                return this;
            }
            
            public FDeclsBuilder EndLambda(out LambdaFDeclSymbol lambdaDecl)
            {
                lambdaDecl = onFinish.Invoke(builder.ToImmutable());
                lambdaDeclHolder.SetValue(lambdaDecl);
                return outerBuilder;
            }
        }

        class FDeclsBuilder
        {
            SymbolFactory factory; // for making void 
            int anonymousCount;
            ImmutableArray<IFDeclSymbolNode>.Builder builder;

            public FDeclsBuilder(SymbolFactory factory)
            {
                this.factory = factory;
                this.anonymousCount = 0;
                this.builder = ImmutableArray.CreateBuilder<IFDeclSymbolNode>();
            }

            public LambdaFDeclBuilder BeginLambda(IDeclSymbolNode outer)
            {
                var fouter = new Holder<FDeclSymbolOuter>(new FDeclSymbolOuter.Decl(outer));

                int curCount = anonymousCount; // holder, 지우지 말 것
                var lambdaDeclBuilder = new LambdaFDeclBuilder(this, memberVarDecls =>
                {
                    var lambdaDecl = new LambdaFDeclSymbol(fouter, new Name.Anonymous(curCount),
                        new FuncReturn(false, factory.MakeVoid()), default, memberVarDecls);

                    builder.Add(lambdaDecl);
                    return lambdaDecl;
                });

                anonymousCount++;
                return lambdaDeclBuilder;
            }
            
            public LambdaFDeclBuilder BeginLambda(IDeclSymbolNode outer, FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams)
            {
                int curCount = anonymousCount; // holder, 지우지 말 것
                var fouterHolder = new Holder<FDeclSymbolOuter>(new FDeclSymbolOuter.Decl(outer));

                var lambdaDeclBuilder = new LambdaFDeclBuilder(this, memberVarDecls =>
                {
                    var lambdaDecl = new LambdaFDeclSymbol(fouterHolder, new Name.Anonymous(curCount), funcRet, funcParams, memberVarDecls);
                    builder.Add(lambdaDecl);
                    return lambdaDecl;
                });

                anonymousCount++;
                return lambdaDeclBuilder;
            }

            public FDeclsBuilder Lambda(IDeclSymbolNode outer, out LambdaFDeclSymbol lambdaDecl)
            {
                lambdaDecl = new LambdaFDeclSymbol(
                    new Holder<FDeclSymbolOuter>(new FDeclSymbolOuter.Decl(outer)), new Name.Anonymous(anonymousCount),
                    new FuncReturn(false, factory.MakeVoid()), parameters: default, memberVars: default);
                builder.Add(lambdaDecl);

                anonymousCount++;
                return this;
            }

            // 뭘 리턴해야 하나
            public ImmutableArray<IFDeclSymbolNode> Make()
            {
                return builder.ToImmutable();
            }
        }

        class ModuleDeclBuilder
        {
            SymbolFactory factory;
            Name moduleName;
            Holder<ModuleDeclSymbol> moduleDeclHolder;
            ImmutableArray<GlobalFuncDeclSymbol>.Builder globalFuncDeclsBuilder;
            ImmutableArray<LambdaFDeclSymbol>.Builder lambdaDeclsBuilder;
            int anonymousCount; 

            public ModuleDeclBuilder(SymbolFactory factory, Name moduleName)
            {
                this.factory = factory;
                this.moduleName = moduleName;
                this.moduleDeclHolder = new Holder<ModuleDeclSymbol>();
                this.globalFuncDeclsBuilder = ImmutableArray.CreateBuilder<GlobalFuncDeclSymbol>();
                this.anonymousCount = 0;
            }

            public ModuleDeclBuilder GlobalFunc(string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
            {
                globalFuncDecl = new GlobalFuncDeclSymbol(
                    moduleDeclHolder, AccessModifier.Public,
                    new Holder<FuncReturn>(new FuncReturn(false, factory.MakeVoid())), new Name.Normal(funcName), default,
                    new Holder<ImmutableArray<FuncParameter>>(default),
                    bInternal: true
                );

                globalFuncDeclsBuilder.Add(globalFuncDecl);
                return this;
            }
            

            // void, 인자 1개짜리
            public ModuleDeclBuilder GlobalFunc(string funcName, FuncParameter funcParameter, out GlobalFuncDeclSymbol globalFuncDecl)
            {
                globalFuncDecl = new GlobalFuncDeclSymbol(
                    moduleDeclHolder, AccessModifier.Public,
                    new Holder<FuncReturn>(new FuncReturn(false, factory.MakeVoid())), new Name.Normal(funcName), default,

                    new Holder<ImmutableArray<FuncParameter>>(Arr<FuncParameter>(funcParameter)),
                    bInternal: true
                );

                globalFuncDeclsBuilder.Add(globalFuncDecl);
                return this;
            }

            // 
            public ModuleDeclBuilder GlobalFunc(IHolder<FuncReturn> funcReturnHolder, string funcName, 
                IHolder<ImmutableArray<FuncParameter>> funcParametersHolder, out GlobalFuncDeclSymbol globalFuncDecl)
            {
                globalFuncDecl = new GlobalFuncDeclSymbol(
                    moduleDeclHolder, AccessModifier.Public,
                    funcReturnHolder, new Name.Normal(funcName), default,
                    funcParametersHolder,
                    bInternal: true
                );

                globalFuncDeclsBuilder.Add(globalFuncDecl);
                return this;
            }            

            public ModuleDeclSymbol Make()
            {
                var moduleDecl = new ModuleDeclSymbol(moduleName, default, default, globalFuncDeclsBuilder.ToImmutable());
                moduleDeclHolder.SetValue(moduleDecl);

                return moduleDecl;
            }
        }

        class ScriptBuilder
        {
            ModuleDeclSymbol moduleDecl;
            ImmutableArray<StmtBody>.Builder stmtBodiesBuilder;

            public ScriptBuilder(ModuleDeclSymbol moduleDecl)
            {
                this.moduleDecl = moduleDecl;
                this.stmtBodiesBuilder = ImmutableArray.CreateBuilder<StmtBody>();
            }

            public ScriptBuilder Add(IFuncDeclSymbol funcDecl, params Stmt[] bodyStmts)
            {
                var declSymbolId = funcDecl.GetDeclSymbolId();
                Debug.Assert(declSymbolId.ModuleName.Equals(moduleDecl.GetName()));
                Debug.Assert(declSymbolId.Path != null);
                stmtBodiesBuilder.Add(new StmtBody(declSymbolId.Path, bodyStmts.ToImmutableArray()));

                return this;
            }

            public ScriptBuilder AddTopLevel(params Stmt[] topLevelStmts)
            {
                stmtBodiesBuilder.Add(new StmtBody(new DeclSymbolPath(null, Name.TopLevel), topLevelStmts.ToImmutableArray()));
                return this;
            }

            public Script Make()
            {
                return new Script(moduleDecl, stmtBodiesBuilder.ToImmutable());
            }
        }

        // void TestFunc()
        // {
        //     @$x
        // }
        // 
        // string x = "Hello";
        // TestFunc();
        [Fact]
        public async Task GlobalVariableExp_GetGlobalValueInFunc()
        {
            // 1. decl 
            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .GlobalFunc("TestFunc", out var testFuncDecl)
                .Make();

            // 2. body 
            var module = factory.MakeModule(moduleDecl);
            var testFunc = factory.MakeGlobalFunc(module, testFuncDecl, default);

            var script = new ScriptBuilder(moduleDecl)
                .Add(testFuncDecl,
                    r.PrintString(r.GlobalVar("x"))
                )
                .AddTopLevel(
                    r.GlobalVarDecl(stringType, "x", r.String("Hello")),
                    r.Call(testFunc)
                )
                .Make();
            
            var output = await EvalAsync(script);
            Assert.Equal("Hello", output);
        }

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
        public async Task CallSeqFuncExp_GenerateSequencesInForeach()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Sequence);        
        //    // Sequence
        //    var seqFunc = new SequenceFuncDecl(default, new Name.Normal("F"), false, intType, default, Arr(new Param(ParamKind.Default, intType, new Name.Normal("x")), new Param(ParamKind.Default, intType, new Name.Normal("y"))), r.Block(

        //        new YieldStmt(
        //            r.CallInternalBinary(InternalBinaryOperator.Multiply_Int_Int_Int,
        //                new LoadExp(r.LocalVar("x")),
        //                r.Int(2))),

        //        new YieldStmt(
        //            r.CallInternalBinary(InternalBinaryOperator.Add_Int_Int_Int,
        //                new LoadExp(r.LocalVar("y")),
        //                r.Int(3)))));

        //    var funcF = RootPath("F", Arr(intType, intType), default);
        //    var seqTypeF = funcF;

        //    var stmts = Arr<Stmt>
        //    (
        //        new ForeachStmt(
        //            intType,
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

        // (local) int x = 23; // syntax로는 만들 수 없다
        // {
        //     string x = "Hello";
        //     @$x
        // }
        // @$x
        [Fact]
        public async Task LocalVarDeclStmt_OverlapsParentScope()
        {
            var topLevelStmts = Arr<Stmt>(

                r.LocalVarDecl(intType, "x", r.Int(23)),

                r.Block(
                    r.LocalVarDecl(stringType, "x", r.String("Hello")),
                    r.PrintString(r.LocalVar("x"))
                ),

                r.PrintInt(r.LocalVar("x"))
            );

            var output = await EvalAsync(topLevelStmts);
            Assert.Equal("Hello23", output);
        }

        // int x = 23; // decl global var x
        // {
        //     string x = "Hello"; // decl local var x
        //     @$x
        // }
        // @$x
        [Fact]
        public async Task LocalVarDeclStmt_OverlapsGlobalVar()
        {
            var topLevelStmts = Arr<Stmt>
            (
                r.GlobalVarDecl(intType, "x", r.Int(23)),

                r.Block(
                    r.LocalVarDecl(stringType, "x", r.String("Hello")),
                    r.PrintString(r.LocalVar("x"))
                ),

                r.PrintInt(r.GlobalVar("x"))
            );

            var output = await EvalAsync(topLevelStmts);
            Assert.Equal("Hello23", output);
        }

        // 
        [Fact]
        public async Task GlobalRefVarDeclStmt_RefDeclAndRefExp_WorksProperly()
        {
            // int i = 3;
            // ref int x = ref i;
            // x = 4 + x;
            // @$i
            var topLevelStmts = Arr<Stmt>(
                r.GlobalVarDecl(intType, "i", r.Int(3)),
                r.GlobalRefVarDecl("x", r.GlobalVar("i")), // type이 빠진다
                r.Assign(
                    r.Deref(r.GlobalVar("x")), 
                    r.CallInternalBinary(
                        InternalBinaryOperator.Add_Int_Int_Int, 
                        r.Int(4),
                        r.Load(r.Deref(r.GlobalVar("x")), intType)
                    )
                ),

                r.PrintInt(r.GlobalVar("i")) 
            );

            var output = await EvalAsync(topLevelStmts);
            Assert.Equal("7", output);
        }

        // If
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

                r.LocalVarDecl(intType, "x", r.Int(34)),

                r.For(
                    intType, "x", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.Equal_Int_Int_Bool, r.LoadLocalVar("x", intType), r.Int(0)),
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
            var stmts = Arr<Stmt> (

                r.LocalVarDecl(intType, "x", r.Int(34)),

                r.PrintInt(r.LocalVar("x")),

                r.For(
                    r.AssignExp(r.LocalVar("x"), r.Int(12)),
                    r.CallInternalBinary(InternalBinaryOperator.Equal_Int_Int_Bool, r.LoadLocalVar("x", intType), r.Int(12)),
                    r.AssignExp(r.LocalVar("x"), r.Int(1)),
                    r.PrintInt(r.LocalVar("x"))),

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

                r.LocalVarDecl(boolType, "x", r.Bool(true)),

                r.For(
                    r.LoadLocalVar("x", boolType),
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

                r.LocalVarDecl(boolType, "x", r.Bool(true)),

                r.For(
                    r.LoadLocalVar("x", boolType),
                    r.AssignExp(r.LocalVar("x"), r.Bool(false)),
                    r.Block(
                        r.PrintString("Once"),
                        ContinueStmt.Instance,
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

                r.LocalVarDecl(boolType, "x", r.Bool(true)),

                r.For(
                    r.LoadLocalVar("x", boolType),
                    r.AssignExp(r.LocalVar("x"), r.Bool(false)),
                    r.Block(
                        r.PrintString("Once"),
                        BreakStmt.Instance,
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
            var topLevelStmts = Arr<Stmt>(
                r.LocalVarDecl(boolType, "x", r.Bool(true)),
                r.For(
                    r.LoadLocalVar("x", boolType),
                    r.AssignExp(r.LocalVar("x"), r.Bool(false)),
                    r.Block(
                        r.PrintString("Once"),
                        r.Return(r.Int(2)),                        
                        r.PrintString("Wrong")
                    )
                ),

                r.PrintString("Wrong")
            );

            var script = r.Script(moduleName, topLevelStmts);

            var (output, result) = await EvalAsyncWithRetValue(default, script);
            Assert.Equal("Once", output);
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task ContinueStmt_ContinuesInnerMostLoopStmt()
        {
            var stmts = Arr<Stmt> (

                r.For(
                    intType, "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", intType), r.Int(2)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),

                    r.Block(

                        r.PrintInt(r.LocalVar("i")),

                        r.For(
                            intType, "j", r.Int(0),
                            r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("j", intType), r.Int(2)),
                            r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("j")),
                            r.Block(
                                r.PrintInt(r.LocalVar("j")),
                                ContinueStmt.Instance,
                                r.PrintInt(r.LocalVar("Wrong"))
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

                r.For(
                    intType, "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", intType), r.Int(2)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),

                    r.Block(

                        r.PrintInt(r.LocalVar("i")),

                        r.For(
                            intType, "j", r.Int(0),
                            r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("j", intType), r.Int(2)),
                            r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("j")),
                            r.Block(
                                r.PrintInt(r.LocalVar("j")),
                                BreakStmt.Instance,
                                r.PrintInt(r.LocalVar("Wrong"))
                            )
                        )
                    )
                )
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("0010", output);
        }

        // void F()
        // {
        //     return;
        //     @Wrong
        // }
        // F();
        // @Completed
        [Fact]
        public async Task ReturnStmt_ExitsFuncImmediately()
        {
            // 1. make decl
            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .GlobalFunc("F", out var fFuncDecl)
                .Make();

            // 2. make body
            var module = factory.MakeModule(moduleDecl);
            var fFunc = factory.MakeGlobalFunc(module, fFuncDecl, default);

            var script = new ScriptBuilder(moduleDecl)
                .Add(fFuncDecl,
                    r.Return(),
                    r.PrintString("Wrong")
                )
                .AddTopLevel(
                    r.Call(fFunc),
                    r.PrintString("Completed")
                )
                .Make();

            var output = await EvalAsync(script);
            Assert.Equal("Completed", output);
        }

        [Fact]
        public async Task ReturnStmt_ReturnProperlyInTopLevel()
        {
            // return 34;
            var topLevelStmts = Arr<Stmt>(
                r.Return(r.Int(34))
            );

            var script = r.Script(moduleName, topLevelStmts);
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
            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .GlobalFunc("F", out var fFuncDecl)
                .Make();

            // 2. script            
            var module = factory.MakeModule(moduleDecl);
            var fFunc = factory.MakeGlobalFunc(module, fFuncDecl, default);

            var script = new ScriptBuilder(moduleDecl)
                .Add(fFuncDecl,
                    r.Return(r.Int(77)),
                    r.PrintString("Wrong")
                )
                .AddTopLevel(
                    r.PrintInt(r.CallExp(fFunc))
                )
                .Make();

            var output = await EvalAsync(script);
            Assert.Equal("77", output);
        }

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

        // BlankStmt do nothing.. nothing을 테스트하기는 힘들다
        [Fact]
        public async Task ExpStmt_EvalInnerExp()
        {
            var stmts = Arr<Stmt> (
                r.LocalVarDecl(intType, "x", r.Int(0)),
                r.Assign(r.LocalVar("x"), r.Int(3)),
                r.PrintInt(r.LocalVar("x"))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("3", output);
        }

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
                    intType, "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", intType), r.Int(count)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),
                    r.PrintInt(r.LocalVar("i"))
                );
            }

            // decl
            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .Make();

            var fdecl = new FDeclsBuilder(factory)
                .Lambda(moduleDecl, out var lambdaDecl0)
                .Lambda(moduleDecl, out var lambdaDecl1)
                .Make();

            var module = factory.MakeModule(moduleDecl);
            var lambda0 = ffactory.MakeLambda(module, lambdaDecl0);
            var lambda1 = ffactory.MakeLambda(module, lambdaDecl1);

            // script
            var script = new ScriptBuilder(moduleDecl)
                .AddTopLevel(
                    r.Await(                    
                        r.Task(lambda0, Arr<Stmt>(PrintNumbersStmt(5))),
                        r.Task(lambda1, Arr<Stmt>(PrintNumbersStmt(5)))
                    )
                )
                .Make();

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
        public async Task TaskStmt_CapturesLocalVariable()
        {
            // int count = 5;
            // await 
            // {
            //     task 
            //     {
            //          for(int i = 0; i < count; i++) i++;
            //     }
            //     task 
            //     {
            //          for(int i = 0; i < count; i++) i++;
            //     }
            // }

            // for(int i = 0; i < count; i++) i++;
            Stmt PrintNumbersStmt(LambdaMemberVarFSymbol memberVar)
            {
                return r.For(
                    intType, "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", intType), r.LoadLambdaMember(memberVar)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),
                    r.PrintInt(r.LocalVar("i"))
                );
            }

            // [count] { for(int i = 0; i < count; i++) i++; }
            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .Make();
                
            // make lambda decls
            var lambdasBuilder = new LambdaDeclsBuilder(factory)
                .BeginLambda(moduleDecl)
                    .MemberVar(intType, "count", out var lambdaMemberVarDecl0)
                .EndLambda(out var lambdaDecl0)
                .BeginLambda(moduleDecl)
                    .MemberVar(intType, "count", out var lambdaMemberVarDecl1)
                .EndLambda(out var lambdaDecl1)
                .Make();
            
            // make lambda
            var module = factory.MakeModule(moduleDecl);
            var lambda0 = factory.MakeLambda(module, lambdaDecl0);
            var lambdaMemberVar0 = factory.MakeLambdaMemberVar(lambda0, lambdaMemberVarDecl0);

            var lambda1 = factory.MakeLambda(module, lambdaDecl1);
            var lambdaMemberVar1 = factory.MakeLambdaMemberVar(lambda1, lambdaMemberVarDecl1);

            var script = new ScriptBuilder(moduleDecl)
                .AddTopLevel(
                    r.LocalVarDecl(intType, "count", r.Int(5)),
                    r.Await(
                        r.Task(lambda0, r.Args(r.LoadLocalVar("count", intType)), Arr<Stmt>(PrintNumbersStmt(lambdaMemberVar0))),
                        r.Assign(r.LocalVar("count"), r.Int(4)),
                        r.Task(lambda1, r.Args(r.LoadLocalVar("count", intType)), Arr<Stmt>(PrintNumbersStmt(lambdaMemberVar1)))
                    )
                )
                .Make();

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

            Assert.True((cur0 == '5' && cur1 == '4') || (cur0 == '4' && cur1 == '5'));
        }

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
                    intType, "i", r.Int(0),
                    r.CallInternalBinary(InternalBinaryOperator.LessThan_Int_Int_Bool, r.LoadLocalVar("i", intType), r.Int(count)),
                    r.CallInternalUnaryAssign(InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("i")),
                    r.Block(
                        r.PrintInt(r.LocalVar("i")),
                        Sleep(1)
                    )
                );
            }

            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .Lambda(out var lambdaDecl0)
                .Lambda(out var lambdaDecl1)
                .Make();

            var module = factory.MakeModule(moduleDecl);
            var lambda0 = factory.MakeLambda(module, lambdaDecl0);
            var lambda1 = factory.MakeLambda(module, lambdaDecl1);

            var script = new ScriptBuilder(moduleDecl).AddTopLevel(
                r.Await(
                    r.Async(lambda0, default, Arr<Stmt>(PrintNumbersStmt(5))),
                    r.Async(lambda1, default, Arr<Stmt>(PrintNumbersStmt(5)))
                )
            )
            .Make();

            var output = await EvalAsync(script);
            Assert.Equal("0011223344", output);
        }

        // seq int F0() { ... }
        // seq int F1() { ... }
        [Fact]
        public async Task YieldStmt_GenerateElementForInnerMostForeachStmt()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Sequence);

            //var seqFunc0 = RootPath("F0");
            //var seqFunc1 = RootPath("F1");

            //var seqFuncDecl0 = new SequenceFuncDecl(default, new Name.Normal("F0"), false, intType, default, default,
            //    new ForeachStmt(
            //        intType, "elem", 
            //        new TempLoc(new CallSeqFuncExp(seqFunc1, null, default), seqFunc1),
            //        r.Block(
            //            r.PrintInt(r.LocalVar("elem")),
            //            new YieldStmt(r.LoadLocalVar("elem", intType))
            //        )
            //    )
            //);

            //var seqFuncDecl1 = new SequenceFuncDecl(default, new Name.Normal("F1"), false, intType, default, default,
            //    r.Block(
            //        new YieldStmt(r.Int(34)),
            //        new YieldStmt(r.Int(56))
            //    )
            //);

            //var stmts = Arr<Stmt>(
            //    new ForeachStmt(intType, "x", new TempLoc(new CallSeqFuncExp(seqFunc0, null, default), seqFunc0),
            //        r.PrintInt(r.LocalVar("x")))
            //);

            //var output = await EvalAsync(Arr<FuncDecl>(seqFuncDecl0, seqFuncDecl1), stmts);

            //Assert.Equal("34345656", output);
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
            var stmts = Arr<Stmt>
            (
                r.LocalVarDecl(stringType, "x", r.String("New ")),

                r.PrintString(r.String(
                    r.TextElem("Hello "),
                    r.ExpElem(r.LoadLocalVar("x", stringType)),
                    r.TextElem("World")
                ))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("Hello New World", output);
        }

        [Fact]
        public async Task AssignExp_ReturnsValue()
        {
            var stmts = Arr<Stmt>
            (
                r.LocalVarDecl(intType, "x", r.Int(3)),
                r.LocalVarDecl(intType, "y", r.Int(4)),

                // y = x = 10;
                r.Assign(r.LocalVar("y"), new AssignExp(r.LocalVar("x"), r.Int(10))),
                r.PrintInt(r.LocalVar("x")),
                r.PrintInt(r.LocalVar("y"))
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

            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .GlobalFunc("F", r.RefParam(intType, "i"), out var fFuncDecl)
                .Make();

            var module = factory.MakeModule(moduleDecl);
            var fFunc = factory.MakeGlobalFunc(module, fFuncDecl, default);

            var script = new ScriptBuilder(moduleDecl)
                .Add(fFuncDecl,
                    r.Assign(new DerefLocLoc(r.LocalVar("i")), r.Int(7))
                )
                .AddTopLevel(
                    r.GlobalVarDecl(intType, "j", r.Int(3)),
                    r.Call(fFunc, r.RefArg(r.GlobalVar("j"))),
                    r.PrintInt(r.GlobalVar("j"))
                )
                .Make();

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

            // decl
            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .GlobalFunc(new FuncReturn(false, voidType), "G", default, out var gFuncDecl)
                .Make();

            var module = factory.MakeModule(moduleDecl);
            var gFunc = factory.MakeGlobalFunc(module, gFuncDecl, default);

            var script = new ScriptBuilder(moduleDecl)
                .Add(gFuncDecl, r.ReturnRef(r.GlobalVar("x")))
                .AddTopLevel(
                    r.GlobalVarDecl(intType, "x", r.Int(3)),
                    r.GlobalRefVarDecl("i", new DerefExpLoc(r.CallExp(gFunc))), // var i = ref G();
                    r.Assign(new DerefLocLoc(r.GlobalVar("i")), r.Int(4)),      // i = deref(i)
                    r.PrintInt(r.GlobalVar("x"))
                )
                .Make();
            
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

            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .GlobalFunc(r.FuncRet(intType), "Print", r.FuncParam((intType, "x")), out var printFuncDecl)
                .GlobalFunc(r.FuncRet(voidType), "TestFunc", r.FuncParam((intType, "i"), (intType, "j"), (intType, "k")), out var testFuncFuncDecl)
                .Make();

            var module = factory.MakeModule(moduleDecl);
            var printFunc = factory.MakeGlobalFunc(module, printFuncDecl, default);
            var testFuncFunc = factory.MakeGlobalFunc(module, testFuncFuncDecl, default);

            Argument MakeArg(int v)
            {
                return r.Arg(r.CallExp(printFunc, r.Arg(r.Int(v))));
            }

            var script = new ScriptBuilder(moduleDecl)
                .Add(printFuncDecl,
                    r.PrintInt(r.LocalVar("x")),
                    r.Return(r.LoadLocalVar("x", intType))
                )
                .Add(testFuncFuncDecl,
                    r.PrintString("TestFunc")
                )
                .AddTopLevel(
                    r.Call(testFuncFunc, MakeArg(1), MakeArg(2), MakeArg(3))
                )
                .Make();

            var output = await EvalAsync(script);
            Assert.Equal("123TestFunc", output);
        }

        [Fact]
        public async Task CallFuncExp_ReturnsValueProperly()
        {
            // string F() { return "hello world"; }

            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                .GlobalFunc(r.FuncRet(stringType), "F", default, out var fFuncDecl)
                .Make();

            var module = factory.MakeModule(moduleDecl);
            var fFunc = factory.MakeGlobalFunc(module, fFuncDecl, default);

            var script = new ScriptBuilder(moduleDecl)
                .Add(fFuncDecl, r.Return(r.String("hello world")))
                .AddTopLevel(
                    r.PrintString(r.CallExp(fFunc))
                )
                .Make();

            var output = await EvalAsync(script);
            Assert.Equal("Hello World", output);
        }
        
        class TestDriver : IModuleDriver
        {
            Name moduleName;
            TestCommandProvider cmdProvider;

            public TestDriver(Name moduleName, TestCommandProvider cmdProvider) 
            {
                this.moduleName = moduleName;
                this.cmdProvider = cmdProvider;
            }

            public ValueTask ExecuteGlobalFuncAsync(SymbolPath globalFuncPath, ImmutableArray<Value> args, Value retValue)
            {
                cmdProvider.Append("Hello World");
                return ValueTask.CompletedTask;
            }

            #region Ignore

            public Value Alloc(SymbolPath typePath)
            {
                throw new NotImplementedException();
            }            

            public ValueTask ExecuteClassConstructor(SymbolPath constructorPath, ClassValue thisValue, ImmutableArray<Value> args)
            {
                throw new NotImplementedException();
            }

            public ValueTask ExecuteClassMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue)
            {
                throw new NotImplementedException();
            }

            public void InitializeClassInstance(SymbolPath path, ImmutableArray<Value>.Builder builder)
            {
                throw new NotImplementedException();
            }

            public int GetTotalClassMemberVarCount(SymbolPath classPath)
            {
                throw new NotImplementedException();
            }

            public Value GetClassStaticMemberValue(SymbolPath path)
            {
                throw new NotImplementedException();
            }

            public int GetClassMemberVarIndex(SymbolPath path)
            {
                throw new NotImplementedException();
            }

            public SymbolId? GetBaseClass(SymbolPath path)
            {
                throw new NotImplementedException();
            }

            public ValueTask ExecuteStructMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue)
            {
                throw new NotImplementedException();
            }

            public ValueTask ExecuteStructConstructor(SymbolPath constructorPath, StructValue thisValue, ImmutableArray<Value> args)
            {
                throw new NotImplementedException();
            }

            public int GetStructMemberVarIndex(SymbolPath path)
            {
                throw new NotImplementedException();
            }

            public Value GetStructStaticMemberValue(SymbolPath path)
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
            var testDriver = new TestDriver(testExternalModuleName, commandProvider);
            var moduleInitializer = Arr<Func<ModuleDriverInitializationContext, IModuleDriver>>(
                context => new TestDriver(testExternalModuleName, commandProvider)
            );

            // F();

            // 외부 모듈에 대한 decl symbol 작성
            var moduleDecl = new ModuleDeclBuilder(factory, testExternalModuleName)
                .GlobalFunc("F", out var fFuncDecl)
                .Make();

            var module = factory.MakeModule(moduleDecl);
            var fFunc = factory.MakeGlobalFunc(module, fFuncDecl, default);

            var script = new ScriptBuilder(moduleDecl)
                .AddTopLevel(r.Call(fFunc))
                .Make();            

            var (output, _) = await EvalAsyncWithRetValue(commandProvider, moduleInitializer, script);

            Assert.Equal("Hello World", output);
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

                        builder.Add(new Param(ParamKind.Default, stringType, paramName));

                        ParamKind paramKind;
                        if (paramInfo.ParameterType.IsByRef)
                            paramKind = ParamKind.Ref;
                        else
                            paramKind = ParamKind.Default;

                        pathsBuilder.Add(new ParamHashEntry(paramKind, stringType));

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
                            paramHash.Entries[i].Type == stringType && 
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
            var system_Console_Write = new Path.Nested(system_Console, new Name.Normal("Write"), new ParamHash(0, Arr(new ParamHashEntry(ParamKind.Default, stringType))), default);
                
            var script = RScript(moduleName, new ExpStmt(new CallFuncExp(system_Console_Write, null, RArgs(r.String("Hello World")))));

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
            // ??? MakeLambda() // ??? 안써도 되는?
            // {
            //     return (int i, int j, int k) => @"TestFunc";
            // }
            //
            // MakeLambda()(Print(1), Print(2), Print(3))

            var makeLambdaFuncRetHolder= new Holder<FuncReturn>();

            // make module
            var moduleDecl = new ModuleDeclBuilder(factory, moduleName)
                 .GlobalFunc(r.Holder(r.FuncRet(intType)), "Print", r.Holder(r.FuncParam((intType, "x"))), out var printFuncDecl)
                 .GlobalFunc(makeLambdaFuncRetHolder, "MakeLambda", new Holder<ImmutableArray<FuncParameter>>(default), out var makeLambdaFuncDecl)
                 .Make();
            
            // make lambda
            var lambdaDecl0 = new LambdaFDeclSymbol(r.Holder(makeLambdaFuncDecl), new Name.Anonymous(0), r.FuncRet(stringType),
                r.FuncParam((intType, "i"), (intType, "j"), (intType, "k")), default);
            makeLambdaFuncDecl.AddLambda(lambdaDecl0);

            // script body
            var module = factory.MakeModule(moduleDecl);
            var makeLambdaFunc = factory.MakeGlobalFunc(module, makeLambdaFuncDecl, default);
            var lambda0 = factory.MakeLambda(makeLambdaFunc, lambdaDecl0);

            var script = new ScriptBuilder(moduleDecl)
                .Add(printFuncDecl,
                    r.PrintInt(new LocalVarLoc(new Name.Normal("x"))),
                    r.Return(r.LoadLocalVar("x", intType))
                )
                .Add(makeLambdaFuncDecl,
                    r.PrintString("MakeLambda"),
                    r.Return(new LambdaExp(lambda0, Args: default))
                )
                .Add(lambdaDecl0, 
                    r.PrintString("TestFunc")
                )
                .AddTopLevel(
                    r.Call(lambda, r.TempLoc(r.CallExp(makeLambda)),
                        r.Arg(r.Call(printFunc, r.Int(1))),
                        r.Arg(r.Call(printFunc, r.Int(2))),
                        r.Arg(r.Call(printFunc, r.Int(3)))
                    )
                )
                .Make();

            var output = await EvalAsync(script);
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
            var lambdaDecl = new LambdaDecl(new Name.Anonymous(0), new CapturedStatement(null, Arr(new OuterLocalVarInfo(intType, "x")), r.PrintInt(new LambdaMemberVar("x"))), default);
            var lambda = RootPath(new Name.Anonymous(0));

            // int x = 3;
            // var func = () => x; // [ref int x = ref x] () => x;
            // x = 34;
            // func();
            var stmts = Arr<Stmt> (
                r.LocalVarDecl(intType, "x", r.Int(3)),
                r.LocalVarDecl(lambda, "func", new LambdaExp(lambda)),

                r.Assign(r.LocalVar("x"), r.Int(34)),
                new ExpStmt(new CallValueExp(lambda, r.LocalVar("func"), default))
            );

            var output = await EvalAsync(Arr<CallableMemberDecl>(lambdaDecl), stmts);
            Assert.Equal("3", output);
        }

        // ListIndexerExp
        [Fact]
        public async Task ListIndexerExp_GetElement()
        {
            var stmts = Arr<Stmt> (
                r.LocalVarDecl(Path.List(intType), "list", new ListExp(intType, Arr<Exp>(r.Int(34), r.Int(56)))),
                r.PrintInt(new ListIndexerLoc(new LocalVarLoc(new Name.Normal("list")), r.Int(1)))
            );

            var output = await EvalAsync(stmts);
            Assert.Equal("56", output);
        }

        // ListExp
        [Fact]
        public async Task ListExp_EvaluatesElementExpInOrder()
        {
            var printFunc = RootPath("Print", Arr(intType), default);

            // print(int x)
            var printFuncDecl = new NormalFuncDecl(default, new Name.Normal("Print"), false, default, RNormalParams((intType, "x")),

                r.Block(
                    r.PrintInt(new LocalVarLoc(new Name.Normal("x"))),
                    r.Return(r.LoadLocalVar("x", intType))
                )
            );

            Exp MakePrintCall(int v) =>
                new CallFuncExp(printFunc, null, RArgs(r.Int(v)));

            var stmts = Arr<Stmt> (
                r.GlobalVarDecl(Path.List(intType), "l", 
                    new ListExp(intType, Arr(
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
