using Gum.CompileTime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Invoker = Gum.Runtime.RuntimeModuleMisc.Invoker;

namespace Gum.Runtime
{
    class EnumerableBuildInfo : RuntimeModuleTypeBuildInfo.Class
    {
        public EnumerableBuildInfo()
            : base(null, RuntimeModule.EnumerableId, new string[] { "T" }, null, () => new ObjectValue(null))
        {
        }

        public override void Build(RuntimeModuleTypeBuilder builder)
        {            
            var enumeratorId = RuntimeModule.EnumeratorId;

            // T
            var enumerableId = RuntimeModule.EnumerableId;
            var elemTypeValue = new TypeVarTypeValue(0, 0, "T");

            // Enumerator<T>
            var enumeratorTypeValue = new NormalTypeValue(enumeratorId, new TypeSymbol[] { elemTypeValue });

            Invoker wrappedGetEnumerator = 
                (domainService, typeArgs, thisValue, args, result) => EnumerableObject.NativeGetEnumerator(domainService, enumeratorId, typeArgs, thisValue, args, result);

            builder.AddMemberFunc("GetEnumerator",
                false, true, Array.Empty<string>(), enumeratorTypeValue, Enumerable.Empty<TypeSymbol>(),
                wrappedGetEnumerator);
        }
        
    }

    class EnumerableObject : GumObject
    {
        TypeInst typeInst;
        IAsyncEnumerable<Value> enumerable;

        public EnumerableObject(TypeInst typeInst, IAsyncEnumerable<Value> enumerable)
        {
            this.typeInst = typeInst;
            this.enumerable = enumerable;
        }
        
        // Enumerator<T> Enumerable<T>.GetEnumerator()
        internal static ValueTask NativeGetEnumerator(DomainService domainService, ItemId enumeratorId, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(thisValue != null);
            Debug.Assert(result != null);

            var enumerableObject = GetObject<EnumerableObject>(thisValue);

            var enumeratorInst = domainService.GetTypeInst(new NormalTypeValue(enumeratorId, typeArgList)); // 같은 TypeArgList를 사용한다
            
            ((ObjectValue)result).SetObject(new EnumeratorObject(enumeratorInst, enumerableObject.enumerable.GetAsyncEnumerator()));

            return new ValueTask();
        }

        public override TypeInst GetTypeInst()
        {
            return typeInst;
        }
    }
}
