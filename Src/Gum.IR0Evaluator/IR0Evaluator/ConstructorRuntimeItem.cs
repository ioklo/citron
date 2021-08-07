using Gum.Collections;
using Pretune;
using System;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public abstract class ConstructorRuntimeItem : RuntimeItem
    {
        public abstract ImmutableArray<R.Param> Parameters { get; }
        public abstract ValueTask InvokeAsync(Value thisValue, ImmutableArray<Value> args);
    }

    public partial class Evaluator
    {
        [AutoConstructor, ImplementIEquatable]
        partial class IR0ConstructorRuntimeItem : ConstructorRuntimeItem
        {
            GlobalContext globalContext;            
            R.Name name;
            R.ParamHash paramHash;
            ImmutableArray<R.Param> parameters;
            R.Stmt body;

            public override R.Name Name => name;
            public override R.ParamHash ParamHash => paramHash;
            public override ImmutableArray<R.Param> Parameters => parameters;

            public override ValueTask InvokeAsync(Value thisValue, ImmutableArray<Value> args)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();
                for (int i = 0; i < args.Length; i++)
                    builder.Add(parameters[i].Name, args[i]);
                var argsDict = builder.ToImmutable();

                return StmtEvaluator.EvalFuncBodyAsync(globalContext, argsDict, thisValue, VoidValue.Instance, body);
            }
        }
    }
}
