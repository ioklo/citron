using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public abstract class FuncRuntimeItem : RuntimeItem
    {
        public abstract ImmutableArray<R.Param> Parameters { get; }
        public abstract ValueTask InvokeAsync(Value? thisValue, ImmutableArray<Value> args, Value result);
    }

    public partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0FuncRuntimeItem : FuncRuntimeItem
        {
            GlobalContext globalContext;
            public override R.Name Name => funcDecl.Name;
            public override R.ParamHash ParamHash => Misc.MakeParamHash(funcDecl.TypeParams.Length, funcDecl.Parameters);
            public override ImmutableArray<R.Param> Parameters => funcDecl.Parameters;
            R.NormalFuncDecl funcDecl;

            public override ValueTask InvokeAsync(Value? thisValue, ImmutableArray<Value> args, Value result)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();

                for (int i = 0; i < args.Length; i++)
                    builder.Add(funcDecl.Parameters[i].Name, args[i]);

                return StmtEvaluator.EvalFuncBodyAsync(globalContext, builder.ToImmutable(), thisValue, result, funcDecl.Body);
            }
        }

        [AutoConstructor]
        partial class IR0StructFuncRuntimeItem : FuncRuntimeItem
        {
            GlobalContext globalContext;
            public override R.Name Name => funcDecl.Name;
            public override R.ParamHash ParamHash => Misc.MakeParamHash(funcDecl.TypeParams.Length, funcDecl.Parameters);
            public override ImmutableArray<R.Param> Parameters => funcDecl.Parameters;
            R.StructMemberFuncDecl funcDecl;

            public override ValueTask InvokeAsync(Value? thisValue, ImmutableArray<Value> args, Value result)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();

                for (int i = 0; i < args.Length; i++)
                    builder.Add(funcDecl.Parameters[i].Name, args[i]);

                return StmtEvaluator.EvalFuncBodyAsync(globalContext, builder.ToImmutable(), thisValue, result, funcDecl.Body);
            }
        }

        [AutoConstructor]
        partial class IR0ClassFuncRuntimeItem : FuncRuntimeItem
        {
            GlobalContext globalContext;
            public override R.Name Name => funcDecl.Name;
            public override R.ParamHash ParamHash => Misc.MakeParamHash(funcDecl.TypeParams.Length, funcDecl.Parameters);
            public override ImmutableArray<R.Param> Parameters => funcDecl.Parameters;
            R.ClassMemberFuncDecl funcDecl;

            public override ValueTask InvokeAsync(Value? thisValue, ImmutableArray<Value> args, Value result)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();

                for (int i = 0; i < args.Length; i++)
                    builder.Add(funcDecl.Parameters[i].Name, args[i]);

                return StmtEvaluator.EvalFuncBodyAsync(globalContext, builder.ToImmutable(), thisValue, result, funcDecl.Body);
            }
        }
    }
}
