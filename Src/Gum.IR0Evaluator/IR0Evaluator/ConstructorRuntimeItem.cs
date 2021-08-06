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
            R.StructConstructorDecl constructor;

            public override R.Name Name => R.Name.Constructor.Instance;
            public override R.ParamHash ParamHash { get; }
            public override ImmutableArray<R.Param> Parameters { get; }

            public override ValueTask InvokeAsync(Value thisValue, ImmutableArray<Value> args)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();
                for (int i = 0; i < args.Length; i++)
                    builder.Add(constructor.Parameters[i].Name, args[i]);
                var argsDict = builder.ToImmutable();

                return StmtEvaluator.EvalFuncBodyAsync(globalContext, argsDict, thisValue, VoidValue.Instance, constructor.Body);
            }
        }
    }
}
