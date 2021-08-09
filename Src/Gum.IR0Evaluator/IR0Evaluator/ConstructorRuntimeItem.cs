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
        [ImplementIEquatable]
        partial class IR0ConstructorRuntimeItem : ConstructorRuntimeItem
        {
            // base call을 넣어야 한다
            GlobalContext globalContext;            
            R.Name name;
            R.ParamHash paramHash;
            ImmutableArray<R.Param> parameters;            
            Lazy<(ConstructorRuntimeItem BaseConstructorItem, ImmutableArray<R.Argument> Args)?> baseConstructorCallInfo;
            R.Stmt body;

            public override R.Name Name => name;
            public override R.ParamHash ParamHash => paramHash;
            public override ImmutableArray<R.Param> Parameters => parameters;

            public IR0ConstructorRuntimeItem(
                GlobalContext globalContext, 
                R.Name name, R.ParamHash paramHash, 
                ImmutableArray<R.Param> parameters, 
                R.Path.Nested baseConstructorPath, ImmutableArray<R.Argument> baseConstructorArgs, R.Stmt body)
            {
                this.globalContext = globalContext;
                this.name = name;
                this.paramHash = paramHash;
                this.parameters = parameters;
                this.baseConstructorCallInfo = new Lazy<(ConstructorRuntimeItem BaseConstructorItem, ImmutableArray<R.Argument> Args)?>(() =>
                {
                    var baseConstructorItem = globalContext.GetRuntimeItem<ConstructorRuntimeItem>(baseConstructorPath);
                    return (baseConstructorItem, baseConstructorArgs);
                });

                this.body = body;
            }

            public IR0ConstructorRuntimeItem(
                GlobalContext globalContext,
                R.Name name, R.ParamHash paramHash,
                ImmutableArray<R.Param> parameters,
                R.Stmt body)
            {
                this.globalContext = globalContext;
                this.name = name;
                this.paramHash = paramHash;
                this.parameters = parameters;
                this.baseConstructorCallInfo = new Lazy<(ConstructorRuntimeItem BaseConstructorItem, ImmutableArray<R.Argument> Args)?>();
                this.body = body;
            }

            public override ValueTask InvokeAsync(Value thisValue, ImmutableArray<Value> args)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();
                for (int i = 0; i < args.Length; i++)
                    builder.Add(parameters[i].Name, args[i]);
                var argsDict = builder.ToImmutable();

                return StmtEvaluator.EvalConstructorAsync(globalContext, argsDict, thisValue, baseConstructorCallInfo.Value, body);
            }
        }
    }
}
