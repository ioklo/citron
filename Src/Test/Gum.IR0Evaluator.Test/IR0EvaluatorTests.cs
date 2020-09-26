using Gum.IR0;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Gum.CompileTime;

namespace Gum.Runtime
{
    public class IR0EvaluatorTests
    {
        class TestExternalDriver : IExternalDriver
        {
            IRuntimeModule runtimeModule;
            StringBuilder sb;

            public TestExternalDriver(IRuntimeModule runtimeModule, StringBuilder sb)
            {
                this.runtimeModule = runtimeModule;
                this.sb = sb;
            }

            public ExternalFuncDelegate GetDelegate(ExternalDriverFuncId funcId)
            {
                if (funcId.Value == "Trace")
                    return Trace;

                throw new InvalidOperationException();
            }

            public void Trace(Value? retValue, params Value[] values)
            {
                var strValue = (StringValue)((RefValue)values[0]).GetTarget();

                var text = strValue.GetValue();
                sb.Append(text);
            }
        }

        // 가장 기본적으로 ExCall Trace가 동작하는지 테스트
        // Func([0]String)
        // {
        //     [0] MakeString "Hello World"
        //     ExCall Trace [0]
        // }
        [Fact]
        public async Task TraceWorks()
        {
            // setup
            var regs = new List<Reg> { new Reg(new RegId(0), AllocInfoId.RefId) };
            var body = new Command.Sequence(new Command[] {
                new Command.MakeString(new RegId(0), "Hello World"),
                new Command.ExternalCall(null, new ExternalFuncId(0), new[] { new RegId(0) }) });

            var exFunc = new ExternalFunc(new ExternalFuncId(0), new ExternalDriverId("Test"), new ExternalDriverFuncId("Trace"), new[] { AllocInfoId.RefId });

            var func = new Func(new FuncId(0), regs, body);
            var script = new Script(new[] { exFunc }, new [] { func }, new FuncId(0));

            var domainService = new DomainService();
            var runtimeModule = new RuntimeModule("HomeDir", "ScriptDir");

            var sb = new StringBuilder();
            var externalDriverFactory = new ExternalDriverFactory();
            externalDriverFactory.Register(new ExternalDriverId("Test"), new TestExternalDriver(runtimeModule, sb));
            var evaluator = new IR0Evaluator(domainService, runtimeModule, externalDriverFactory);

            // execute 
            await evaluator.RunScriptAsync(script);

            // verify 
            Assert.Equal("Hello World", sb.ToString());
        }


        // Continue가 나왔을 때, Scope앞으로 잘 가는지
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

        [Fact]
        public async Task Continue()
        {
            // setup
            var exFunc = new ExternalFunc(new ExternalFuncId(0), new ExternalDriverId("Test"), new ExternalDriverFuncId("Trace"), new[] { AllocInfoId.RefId });

            var regs = new List<Reg>() {
                new Reg(new RegId(0), AllocInfoId.BoolId),
                new Reg(new RegId(1), AllocInfoId.RefId),
                new Reg(new RegId(2), AllocInfoId.RefId) };

            var body = new Command.Sequence(new Command[] {

                new Command.MakeBool(new RegId(0), false),
                new Command.MakeString(new RegId(1), "1"),
                new Command.MakeString(new RegId(2), "2"),

                new Command.Scope(new ScopeId(0), new Command.Sequence(new Command[]{
                    new Command.ExternalCall(null, new ExternalFuncId(0), new []{ new RegId(1) }),
                    new Command.If(new RegId(0), new Command.Break(new ScopeId(1)), null),
                    new Command.ExternalCall(null, new ExternalFuncId(0), new []{ new RegId(2) }),
                    new Command.MakeBool(new RegId(0), true),
                    new Command.Continue(new ScopeId(0)) })) });

            var func = new Func(new FuncId(0), regs, body);
            var script = new Script(new[] { exFunc }, new []{ func }, new FuncId(0));

            var domainService = new DomainService();
            var runtimeModule = new RuntimeModule("HomeDir", "ScriptDir");

            var sb = new StringBuilder();
            var externalDriverFactory = new ExternalDriverFactory();
            externalDriverFactory.Register(new ExternalDriverId("Test"), new TestExternalDriver(runtimeModule, sb));
            var evaluator = new IR0Evaluator(domainService, runtimeModule, externalDriverFactory);

            // execute 
            await evaluator.RunScriptAsync(script);

            // verify
            Assert.Equal("121", sb.ToString());
        }
    }
}
