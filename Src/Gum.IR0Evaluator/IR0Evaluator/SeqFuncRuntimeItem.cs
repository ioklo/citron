using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class SeqFuncRuntimeItem : AllocatableRuntimeItem 
    {
        public abstract bool IsThisCall { get; }
        public abstract R.ParamInfo ParamInfo { get; }

        public abstract void Invoke(Evaluator evaluator, Value? thisValue, ImmutableArray<Value> args, Value result);
    }

    public partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0SeqFuncRuntimeItem : SeqFuncRuntimeItem
        {
            public override R.Name Name => seqFuncDecl.Name;
            public override R.ParamHash ParamHash => Misc.MakeParamHash(seqFuncDecl.TypeParams.Length, seqFuncDecl.ParamInfo);
            public override bool IsThisCall => seqFuncDecl.IsThisCall;
            public override R.ParamInfo ParamInfo => seqFuncDecl.ParamInfo;
            R.SequenceFuncDecl seqFuncDecl;

            public override Value Alloc(Evaluator evaluator, TypeContext typeContext)
            {
                return new SeqValue();
            }

            public override void Invoke(Evaluator evaluator, Value? thisValue, ImmutableArray<Value> args, Value result)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();

                for (int i = 0; i < args.Length; i++)
                    builder.Add(seqFuncDecl.ParamInfo.Parameters[i].Name, args[i]);

                // evaluator 복제
                var newEvaluator = evaluator.CloneWithNewContext(thisValue, default, builder.ToImmutable());

                // asyncEnum을 만들기 위해서 내부 함수를 씁니다
                async IAsyncEnumerator<Infra.Void> WrapAsyncEnum()
                {
                    await foreach (var _ in newEvaluator.EvalStmtAsync(seqFuncDecl.Body))
                    {
                        yield return Infra.Void.Instance;
                    }
                }

                var enumerator = WrapAsyncEnum();
                ((SeqValue)result).SetEnumerator(enumerator, newEvaluator);
            }
        }
    }
}
