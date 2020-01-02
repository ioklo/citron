using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    class IntBuildInfo : RuntimeModuleTypeBuildInfo.Struct
    {
        RuntimeModule runtimeModule;

        public IntBuildInfo(RuntimeModule runtimeModule)
            : base(null, RuntimeModule.IntId, ImmutableArray<string>.Empty, null, () => runtimeModule.MakeInt(0))
        {
            this.runtimeModule = runtimeModule;
        }

        public override void Build(RuntimeModuleTypeBuilder builder)
        {
            TypeValue intTypeValue = TypeValue.MakeNormal(RuntimeModule.IntId);

            builder.AddMemberFunc(SpecialNames.OpInc, false, false, ImmutableArray<string>.Empty, intTypeValue, ImmutableArray.Create(intTypeValue), OperatorInc);
            builder.AddMemberFunc(SpecialNames.OpDec, false, false, ImmutableArray<string>.Empty, intTypeValue, ImmutableArray.Create(intTypeValue), OperatorDec);
        }

        ValueTask OperatorInc(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> argValues, Value resultValue)
        {
            Debug.Assert(argValues.Count == 1);
            var source = argValues[0];

            int intValue = runtimeModule.GetInt(source);
            runtimeModule.SetInt(resultValue, intValue + 1);

            return default;
        }

        ValueTask OperatorDec(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> argValues, Value resultValue)
        {
            Debug.Assert(argValues.Count == 1);
            var source = argValues[0];

            int intValue = runtimeModule.GetInt(source);
            runtimeModule.SetInt(resultValue, intValue - 1);

            return default;
        }
    }
}
