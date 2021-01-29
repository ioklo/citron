using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    class EnumeratorBuildInfo : RuntimeModuleTypeBuildInfo.Class
    {
        public EnumeratorBuildInfo()
            : base(null, RuntimeModule.EnumeratorId, new string[] { "T" }, null, () => new ObjectValue(null))
        {
        }

        public override void Build(RuntimeModuleTypeBuilder builder)
        {
            var enumeratorId = RuntimeModule.EnumeratorId;            
            var elemTypeValue = new TypeVarTypeValue(enumeratorId, "T");
            var boolTypeValue = new NormalTypeValue(RuntimeModule.BoolId);

            // bool Enumerator<T>.MoveNext()
            builder.AddMemberFunc(
                "MoveNext",
                false, true,
                Array.Empty<string>(), boolTypeValue, Enumerable.Empty<TypeValue>(),
                EnumeratorObject.NativeMoveNext);

            // T Enumerator<T>.GetCurrent()
            builder.AddMemberFunc("GetCurrent",
                false, true,
                Array.Empty<string>(), elemTypeValue, Enumerable.Empty<TypeValue>(),
                EnumeratorObject.NativeGetCurrent);
        }
    }

    class EnumeratorObject : GumObject
    {
        TypeInst typeInst;
        IAsyncEnumerator<Value> enumerator;

        public EnumeratorObject(TypeInst typeInst, IAsyncEnumerator<Value> enumerator)
        {
            this.typeInst = typeInst;
            this.enumerator = enumerator;
        }

        internal static async ValueTask NativeMoveNext(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(thisValue != null);
            Debug.Assert(result != null);

            var enumeratorObj = GetObject<EnumeratorObject>(thisValue);

            bool bResult = await enumeratorObj.enumerator.MoveNextAsync();

            ((Value<bool>)result).Data = bResult;
        }

        internal static ValueTask NativeGetCurrent(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(thisValue != null);
            Debug.Assert(result != null);

            var enumeratorObj = GetObject<EnumeratorObject>(thisValue);            
            result.SetValue(enumeratorObj.enumerator.Current);

            return new ValueTask();
        }

        public override TypeInst GetTypeInst()
        {
            return typeInst;
        }
    }
}
