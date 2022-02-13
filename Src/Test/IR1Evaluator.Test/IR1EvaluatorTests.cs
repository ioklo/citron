using Citron.IR1;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Citron.CompileTime;
using static Citron.IR1.Runtime.Evaluator;
using static Citron.IR1.Command;

using Task = System.Threading.Tasks.Task;
using IR1Task = Citron.IR1.Command.Task;

namespace Citron.IR1.Runtime
{
    public class IR1EvaluatorTests
    {
        class TestExternalDriver : IExternalDriver
        {
            StringBuilder sb;

            public TestExternalDriver(StringBuilder sb)
            {
                this.sb = sb;
            }

            public ExternalFuncDelegate GetDelegate(ExternalDriverFuncId funcId)
            {
                if (funcId.Value == "TraceString")
                    return TraceString;

                if (funcId.Value == "TraceInt")
                    return TraceInt;

                if (funcId.Value == "TraceBool")
                    return TraceBool;

                if (funcId.Value == "Sleep")
                    return Sleep;

                throw new InvalidOperationException();
            }

            public ValueTask TraceString(Value? retValue, params Value[] values)
            {
                var strValue = (StringValue)((RefValue)values[0]).GetTarget();

                var text = strValue.GetString();
                lock (sb) 
                    sb.Append(text);

                return new ValueTask(Task.CompletedTask);
            }

            public ValueTask TraceInt(Value? retValue, params Value[] values)
            {
                int value = ((IntValue)values[0]).GetInt();
                lock (sb) 
                    sb.Append(value);

                return new ValueTask(Task.CompletedTask);
            }

            public ValueTask TraceBool(Value? retValue, params Value[] values)
            {
                bool value = ((BoolValue)values[0]).GetBool();

                lock(sb)
                    sb.Append(value);
                

                return new ValueTask(Task.CompletedTask);
            }

            public async ValueTask Sleep(Value? retValue, params Value[] values)
            {
                var sec = ((IntValue)values[0]).GetInt();

                await Task.Delay(1000 * sec);
            }
        }

        ExternalFuncId traceStringId = new ExternalFuncId(0);
        ExternalFuncId traceIntId = new ExternalFuncId(1);
        ExternalFuncId traceBoolId = new ExternalFuncId(2);
        ExternalFuncId sleepId = new ExternalFuncId(3);

        ValueTask<string> EvaluateAsync(FuncId entryId, params Func[] funcs)
        {
            return EvaluateAsync(entryId, Enumerable.Empty<AllocInfoId>(), funcs);
        }

        async ValueTask<string> EvaluateAsync(FuncId entryId, IEnumerable<AllocInfoId> globalAllocInfoIds, params Func[] funcs)
        {
            var exFunc0 = new ExternalFunc(traceStringId, new ExternalDriverId("Test"), new ExternalDriverFuncId("TraceString"), new[] { AllocInfoId.RefId });
            var exFunc1 = new ExternalFunc(traceIntId, new ExternalDriverId("Test"), new ExternalDriverFuncId("TraceInt"), new[] { AllocInfoId.IntId });
            var exFunc2 = new ExternalFunc(traceBoolId, new ExternalDriverId("Test"), new ExternalDriverFuncId("TraceBool"), new[] { AllocInfoId.BoolId });
            var exFunc3 = new ExternalFunc(sleepId, new ExternalDriverId("Test"), new ExternalDriverFuncId("Sleep"), new[] { AllocInfoId.IntId });

            var globalVars = new List<GlobalVar>();
            int i = 0;
            foreach(var allocInfoId in globalAllocInfoIds)
            {
                globalVars.Add(new GlobalVar(new GlobalVarId(i), allocInfoId));
                i++;
            }

            var script = new Script(new[] { exFunc0, exFunc1, exFunc2, exFunc3 }, globalVars, funcs, entryId);

            var sb = new StringBuilder();
            var externalDriverFactory = new ExternalDriverFactory();
            externalDriverFactory.Register(new ExternalDriverId("Test"), new TestExternalDriver(sb));
            var evaluator = new Evaluator(externalDriverFactory);

            // execute 
            await evaluator.RunScriptAsync(script);

            return sb.ToString();
        }

        // ���� �⺻������ ExCall Trace�� �����ϴ��� �׽�Ʈ
        // Func([0]String)
        // {
        //     [0] MakeString "Hello World"
        //     ExCall Trace [0]
        // }
        [Fact]
        public async Task TraceWorks()
        {
            // setup
            var funcId = new FuncId(0);
            var regs = new List<Reg> { 
                new Reg(new RegId(0), AllocInfoId.RefId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.BoolId),
            };
            var body = new Scope(new ScopeId(0), new Sequence(new Command[] {
                new MakeString(new RegId(0), "Hello World"),
                new ExternalCall(null, traceStringId, new[] { new RegId(0) }),

                new MakeInt(new RegId(1), 3),
                new ExternalCall(null, traceIntId, new[] { new RegId(1) }),

                new MakeBool(new RegId(2), false),
                new ExternalCall(null, traceBoolId, new[] { new RegId(2) }),
            }));
            var func = new Func(funcId, regs, body);

            var result = await EvaluateAsync(funcId, func);

            // verify 
            Assert.Equal("Hello World3False", result);
        }


        // Continue�� ������ ��, Scope������ �� ������
        // 
        // Func([0]Bool, [1]String, [2]String)
        // {
        //     [0] MakeBool false
        //     [1] MakeString "1"
        //     [2] MakeString "2"
        //     Scope S1
        //     {
        //         ExCall Trace [1]
        //         If [0] break S1;
        //         ExCall Trace [2]
        //         [0] MakeBool true
        //         continue S1;
        //     }
        // }
        // 121
        // MakeBool, MakeString, Scope, Sequence, Continue, Break, ExCall, If
        [Fact]
        public async Task Continue()
        {
            // setup
            var funcId = new FuncId(0);

            var regs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.BoolId),
                new Reg(new RegId(1), AllocInfoId.RefId),
                new Reg(new RegId(2), AllocInfoId.RefId) };

            var body = new Scope(new ScopeId(0), new Sequence(new Command[] {

                new MakeBool(new RegId(0), false),
                new MakeString(new RegId(1), "1"),
                new MakeString(new RegId(2), "2"),

                new Scope(new ScopeId(1), new Sequence(new Command[]{
                    new ExternalCall(null, traceStringId, new []{ new RegId(1) }),
                    new If(new RegId(0), new Break(new ScopeId(1)), null),
                    new ExternalCall(null, traceStringId, new []{ new RegId(2) }),
                    new MakeBool(new RegId(0), true),
                    new Continue(new ScopeId(1)) })) }));

            var func = new Func(funcId, regs, body);

            var result = await EvaluateAsync(funcId, func);

            // verify
            Assert.Equal("121", result);
        }

        // [0] MakeInt 3
        // [1] MakeInt 4
        // ExCall Trace [0]
        // Assign [0] [1]
        // ExCall [0]
        [Fact]
        public async Task AssignBetweenRegisters()
        {
            // setup
            var funcId = new FuncId(0);

            var regs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.IntId) };

            var body = new Scope(new ScopeId(0), new Sequence(new Command[] {

                new MakeInt(new RegId(0), 3),
                new MakeInt(new RegId(1), 4),

                new ExternalCall(null, traceIntId, new []{ new RegId(0) }),
                new Assign(new RegId(0), new RegId(1)),
                new ExternalCall(null, traceIntId, new []{ new RegId(0) })
            }));

            var func = new Func(funcId, regs, body);

            var result = await EvaluateAsync(funcId, func);

            // verify
            Assert.Equal("34", result);
        }

        // Call
        // Func "Selector" [0] [1]
        // SetReturnValue [1]        
        
        // main [0] Int
        // [0] MakeInt 35
        // [1] MakeInt 37
        // [2] Call Selector [0] [1]
        // ExCall Trace [2]
        [Fact]
        public async Task CallCommandReturnsCorrectValue()
        {
            // setup

            var selectorId = new FuncId(0);
            var mainId = new FuncId(1);

            // Func selector
            var selectorRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.IntId) };

            var selectorBody = new Scope(new ScopeId(0), new SetReturnValue(new RegId(1)));

            var selectorFunc = new Func(selectorId, selectorRegs, selectorBody);

            // Func main
            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.IntId),
            };

            var mainBody = new Scope(new ScopeId(1), new Sequence(new Command[]
            {
                new MakeInt(new RegId(0), 35),
                new MakeInt(new RegId(1), 37),
                new Call(new RegId(2), selectorId, new [] { new RegId(0), new RegId(1) } ),
                new ExternalCall(null, new ExternalFuncId(1), new [] { new RegId(2) } )
            }));

            var mainFunc = new Func(mainId, mainRegs, mainBody);
            var result = await EvaluateAsync(mainId, selectorFunc, mainFunc);

            Assert.Equal("37", result);
        }

        // Call
        // Func "Func0"(int [0], string [1], bool [2])
        //     [2] = Call "Func1" [1] [0] 
        //     SetReturnValue [2]
        //
        // Func "Func1"(string [0], int [1], bool [2])
        //     ExFunc Trace "TraceString" [0]
        //     ExFunc Trace "TraceInt" [1]
        //     [2] = MakeBool true
        //     SetReturnValue [2]

        // Func "main"(bool [0], int [1], string [2])
        // {
        //     [1] = MakeInt 1
        //     [2] = MakeString "x"
        //     [0] = Call "Func0" [1] [2]
        //     ExCall "TraceBool" [0]
        // }
        // x1true

        [Fact]
        public async Task ChainOfCallCommandsWork()
        {
            var func0Id = new FuncId(0);
            var func1Id = new FuncId(1);
            var mainId = new FuncId(2);

            // Func selector
            var func0Regs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.RefId),
                new Reg(new RegId(2), AllocInfoId.BoolId) };

            var func0Body = new Scope(new ScopeId(0), new Sequence(new Command[] {
                new Call(new RegId(2), func1Id, new []{ new RegId(1), new RegId(0) }),
                new SetReturnValue(new RegId(2)) }));

            var func0 = new Func(func0Id, func0Regs, func0Body);

            var func1Regs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.RefId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.BoolId) };

            var func1Body = new Scope(new ScopeId(1), new Sequence(new Command[] {
                new ExternalCall(null, traceStringId, new []{ new RegId(0) }),
                new ExternalCall(null, traceIntId, new[] { new RegId(1)}),
                new MakeBool(new RegId(2), true),
                new SetReturnValue(new RegId(2))
            }));

            var func1 = new Func(func1Id, func1Regs, func1Body);

            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.BoolId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.RefId) };

            var mainBody = new Scope(new ScopeId(2), new Sequence(new Command[] {
                new MakeInt(new RegId(1), 1),
                new MakeString(new RegId(2), "x"),
                new Call(new RegId(0), func0Id, new [] { new RegId(1), new RegId(2) }),

                new ExternalCall(null, traceBoolId, new []{ new RegId(0) }),
            }));

            var main = new Func(mainId, mainRegs, mainBody);

            var result = await EvaluateAsync(mainId, func0, func1, main);

            Assert.Equal("x1True", result);
        }

        // TODO: recursive call

        // HeapAlloc, AssignRef, Deref
        // main(ref [0], int [1], int [2])
        // {
        //     [0] HeapAlloc int
        //     [2] MakeInt 2
        //     AssignRef [0] [2]
        //     [1] Deref [0]
        //     ExCall TraceInt [1]
        // }

        [Fact]
        public async Task HeapAllocCommandSetRefValueToDestRegister()
        {
            var mainId = new FuncId(0);

            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.RefId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.IntId) };

            var mainBody = new Scope(new ScopeId(0), new Sequence(new Command[] {

                new HeapAlloc(new RegId(0), AllocInfoId.IntId),
                new MakeInt(new RegId(2), 2),
                new AssignRef(new RegId(0), new RegId(2)),

                new Deref(new RegId(1), new RegId(0)),

                new ExternalCall(null, traceIntId, new []{ new RegId(1) }),
            }));

            var main = new Func(mainId, mainRegs, mainBody);

            var result = await EvaluateAsync(mainId, main);

            Assert.Equal("2", result);
        }
        
        // ConcatStrings
        // main(string [0], string [1], string [2])
        // [0] = MakeString "Hello"
        // [1] = MakeString " World"
        // [2] = MakeString " From Gum"
        // [3] = ConcatString [0] [1] [2]
        // [4] = ConcatString [3] [2]
        // ExCall TraceString [3]
        // ExCall TraceString [4]
        [Fact]
        public async Task ConcatStringsCommandMakeStringFromRegisterStrings()
        {
            var mainId = new FuncId(0);

            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.RefId),
                new Reg(new RegId(1), AllocInfoId.RefId),
                new Reg(new RegId(2), AllocInfoId.RefId),
                new Reg(new RegId(3), AllocInfoId.RefId),
                new Reg(new RegId(4), AllocInfoId.RefId) };

            var mainBody = new Scope(new ScopeId(0), new Sequence(new Command[] {

                new MakeString(new RegId(0), "Hello"),
                new MakeString(new RegId(1), " World"),
                new MakeString(new RegId(2), " From Gum"),

                new ConcatStrings(new RegId(3), new []{ new RegId(0), new RegId(1), new RegId(2) }),
                new ConcatStrings(new RegId(4), new []{ new RegId(3), new RegId(2) }),

                new ExternalCall(null, traceStringId, new []{ new RegId(3) }),
                new ExternalCall(null, traceStringId, new []{ new RegId(4) }),
            }));

            var main = new Func(mainId, mainRegs, mainBody);

            var result = await EvaluateAsync(mainId, main);

            Assert.Equal("Hello World From GumHello World From Gum From Gum", result);
        }

        // MakeEnumerator, EnumeratorNext, EnumeratorValue,  Yield
        // Func GetNumbers(Int [0], Int [1], String [2])
        //    [2] MakeString "Start"
        //    ExCall TraceString [2]        
        //    Yield [0]
        //    Yield [1]
        //    [2] MakeString "End"
        //    ExCall TraceString [2]

        // Func Main(int [0], int [1], ref [2], bool [3], int [4])
        //     [0] MakeInt 3
        //     [1] MakeInt 4
        //     [2] MakeEnum "GetNumbers" Int [0] [1]

        //     [3] EnumNext [2]
        //     ExCall TraceBool [3]

        //     [4] EnumValue [2]
        //     ExCall TraceInt [4]

        //     [3] EnumNext [2]
        //     ExCall TraceBool[3]

        //     [4] EnumValue [2]
        //     ExCall TraceInt [4]

        //     [3] EnumNext [2]
        //     ExCall TraceBool [3]

        [Fact]
        public async Task SequenceFunctionWorks()
        {
            var getNumbersId = new FuncId(0);
            var mainId = new FuncId(1);

            var getNumbersRegs = new List<Reg>() {                
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.RefId) };

            var getNumbersBody = new Scope(new ScopeId(0), new Sequence(new Command[] {

                new MakeString(new RegId(2), "Start"),
                new ExternalCall(null, traceStringId, new []{ new RegId(2) }),

                new Yield(new RegId(0)),
                new Yield(new RegId(1)),

                new MakeString(new RegId(2), "End"),
                new ExternalCall(null, traceStringId, new []{ new RegId(2) }),

            }));

            var getNumbers = new Func(getNumbersId, getNumbersRegs, getNumbersBody);

            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.RefId),
                new Reg(new RegId(3), AllocInfoId.BoolId),
                new Reg(new RegId(4), AllocInfoId.IntId) };

            var mainBody = new Scope(new ScopeId(1), new Sequence(new Command[] {

                new MakeInt(new RegId(0), 3),
                new MakeInt(new RegId(1), 4),

                new MakeEnumerator(new RegId(2), getNumbersId, AllocInfoId.IntId, new [] {new RegId(0), new RegId(1) } ),

                new EnumeratorMoveNext(new RegId(3), new RegId(2)),
                new ExternalCall(null, traceBoolId, new []{ new RegId(3) }),

                new EnumeratorGetValue(new RegId(4), new RegId(2)),
                new ExternalCall(null, traceIntId, new []{ new RegId(4) }),

                new EnumeratorMoveNext(new RegId(3), new RegId(2)),
                new ExternalCall(null, traceBoolId, new []{ new RegId(3) }),

                new EnumeratorGetValue(new RegId(4), new RegId(2)),
                new ExternalCall(null, traceIntId, new []{ new RegId(4) }),

                new EnumeratorMoveNext(new RegId(3), new RegId(2)),
                new ExternalCall(null, traceBoolId, new []{ new RegId(3) }),
            }));

            var main = new Func(mainId, mainRegs, mainBody);

            var result = await EvaluateAsync(mainId, getNumbers, main);

            Assert.Equal("StartTrue3True4EndFalse", result);
        }

        // Task Await 
        // f0(arg string [0], string [1], string [2])
        // {        
        //     [1] MakeString "Finish "
        //     [2] ConcatStrings [0] [1]
        //     ExCall TraceString [0]
        // }
        //
        // main(string [0])
        // {
        //     Await  
        //         [0] MakeString "1"
        //         Task "f0" [0]
        // 
        //         [0] MakeString "2"
        //         Task "f0" [0]
        // 
        //     [0] MakeString "End"
        //     ExCall TraceString [0]
        // }
        [Fact]
        public async Task AwaitCommandWaitsTaskCommand()
        {
            var f0Id = new FuncId(0);
            var mainId = new FuncId(1);

            var f0Regs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.RefId),
                new Reg(new RegId(1), AllocInfoId.RefId),
                new Reg(new RegId(2), AllocInfoId.RefId) };

            var f0Body = new Scope(new ScopeId(0), new Sequence(new Command[] {

                new MakeString(new RegId(1), "Finish "),
                new ConcatStrings(new RegId(2), new [] { new RegId(1), new RegId(0) }),
                new ExternalCall(null, traceStringId, new [] { new RegId(2) })

            }));

            var f0 = new Func(f0Id, f0Regs, f0Body);

            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.RefId) 
            };

            var mainBody = new Scope(new ScopeId(1), new Sequence(new Command[] {

                new Await(new Sequence(new Command[] {
                    new MakeString(new RegId(0), "Task1"),
                    new IR1Task(f0Id, new []{ new RegId(0) }),

                    new MakeString(new RegId(0), "Task2"),
                    new IR1Task(f0Id, new []{ new RegId(0) }),
                })),

                new MakeString(new RegId(0), "End"),
                new ExternalCall(null, traceStringId, new []{ new RegId(0) })
            }));
            
            var main = new Func(mainId, mainRegs, mainBody);

            var result = await EvaluateAsync(mainId, f0, main);

            // �� ���� �ϳ�
            Assert.True(result == "Finish Task1Finish Task2End" ||
                result == "Finish Task2Finish Task1End" );
        }

        // Async
        // f0(arg int [0], ref [1])
        // {        
        //     [1] MakeString "Wait"
        //     ExCall TraceString [1]
        //     ExCall TraceInt [0]

        //     ExCall "Sleep" [0]

        //     [1] MakeString "Finish"
        //     ExCall TraceString [1]
        //     ExCall TraceInt [0]
        // }
        //
        // main(int [0], string [1])
        // {
        //     Await  
        //         [0] MakeInt 1
        //         Async "f0" [0]
        // 
        //         [0] MakeInt 2
        //         Async "f0" [0]
        // 
        //     [1] MakeString "End"
        //     ExCall TraceString [1]
        // }
        [Fact]
        public async Task AsyncCommandExecutesFuncAsyncronously()
        {
            var f0Id = new FuncId(0);
            var mainId = new FuncId(1);

            var f0Regs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.RefId) };

            var f0Body = new Scope(new ScopeId(0), new Sequence(new Command[] {

                new MakeString(new RegId(1), "Wait"),
                new ExternalCall(null, traceStringId, new [] { new RegId(1) }),
                new ExternalCall(null, traceIntId, new [] { new RegId(0) }),

                new ExternalCall(null, sleepId, new [] { new RegId(0) }),

                new MakeString(new RegId(1), "Finish"),
                new ExternalCall(null, traceStringId, new [] { new RegId(1) }),
                new ExternalCall(null, traceIntId, new [] { new RegId(0) }),
            }));

            var f0 = new Func(f0Id, f0Regs, f0Body);

            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.IntId),
                new Reg(new RegId(1), AllocInfoId.RefId)
            };

            var mainBody = new Scope(new ScopeId(1), new Sequence(new Command[] {

                new Await(new Sequence(new Command[] {
                    new MakeInt(new RegId(0), 1),
                    new Async(f0Id, new []{ new RegId(0) }),

                    new MakeInt(new RegId(0), 2),
                    new Async(f0Id, new []{ new RegId(0) }),
                })),

                new MakeString(new RegId(1), "End"),
                new ExternalCall(null, traceStringId, new []{ new RegId(1) })
            }));

            var main = new Func(mainId, mainRegs, mainBody);

            var result = await EvaluateAsync(mainId, f0, main);

            Assert.Equal("Wait1Wait2Finish1Finish2End", result);
        }

        // GetGlobalRef, AssignRef, Deref
        // GlobalVar(int g0)
        // 
        // main(ref [0], int [1], ref [2] int [3])
        //     [0] GetGlobalRef "g0"
        //     [1] MakeInt 3
        //     AssignRef [0] [1]
        //     [0] GetGlobalRef "g0"
        //     [3] Deref [2]
        //     ExCall TraceInt [3]
        // "3"

        [Fact]
        async Task GetGlobalRefCommandGetReferenceOfGlobalVariable()
        {
            var mainId = new FuncId(0);

            var mainRegs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.RefId),
                new Reg(new RegId(1), AllocInfoId.IntId),
                new Reg(new RegId(2), AllocInfoId.RefId),
                new Reg(new RegId(3), AllocInfoId.IntId),
            };

            var mainBody = new Scope(new ScopeId(1), new Sequence(new Command[] {
                new GetGlobalRef(new RegId(0), new GlobalVarId(0)),
                new MakeInt(new RegId(1), 3),
                new AssignRef(new RegId(0), new RegId(1)),

                new GetGlobalRef(new RegId(2), new GlobalVarId(0)),
                new Deref(new RegId(3), new RegId(2)),
                new ExternalCall(null, traceIntId, new [] { new RegId(3) })
            }));

            var main = new Func(mainId, mainRegs, mainBody);

            var result = await EvaluateAsync(mainId, new[] { AllocInfoId.IntId }, main);
            Assert.Equal("3", result);
        }

        // GetMemberRef
        // 1. �ڱ� ��⿡�� MemberIndex�� Ȯ������ ���� ���� ����.. Translation������ Ȯ���� �ȵȻ��·� ���ɾ �ۼ��� ���� �ִ�
        // 2. IR1�� �ۼ��� �ٸ� ��⿡�� MemberIndex�� Ȯ������ �ʾ��� ���� �ִ�
        // 3. IR1�� �ۼ����� ���� �ٸ� ����� ExternalGetMemberRef�� ����Ѵ�
        // 4. IR1�� �ۼ��� �ٸ� ��� ������ ExternalGetMemberRef�� ����ؾ� �ϴ°� �ƴѰ� => �ƴѰ� ����. Value�� ���ϴ� ����� �ٸ���..������� ���ɾ ������ �Ѵ�
        // Types 
        //     0, { "T", { (intId, "x"), ("y"), ("z") } }
        //     1, { "S", { 
        // 
        // MemberVars
        //     0, { TypeId "S", "x" }
        //     1, { TypeId "S", "y" }
        // 
        // �ε��� 
        // CompAllocInfos // �̰� �ε��� �����ؾ� �ڴ�
        //     0, { IntId, StringId, RefId }
        // 
        // 


        // ExternalGetMemberRef
    }
}
