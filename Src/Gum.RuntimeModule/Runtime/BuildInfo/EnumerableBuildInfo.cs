using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Invoker = Gum.Runtime.RuntimeModuleMisc.Invoker;

namespace Gum.Runtime
{
    class EnumerableBuildInfo : RuntimeModuleTypeBuildInfo.Class
    {
        public EnumerableBuildInfo()
            : base(null, RuntimeModule.EnumerableId, ImmutableArray.Create("T"), null, () => new ObjectValue(null))
        {
        }

        public override void Build(RuntimeModuleTypeBuilder builder)
        {            
            var enumeratorId = RuntimeModule.EnumeratorId;

            // T
            var enumerableId = RuntimeModule.EnumerableId;
            var elemTypeValue = TypeValue.MakeTypeVar(enumerableId, "T");

            // Enumerator<T>
            var enumeratorTypeValue = TypeValue.MakeNormal(enumeratorId, TypeArgumentList.Make(elemTypeValue));

            var funcIdsBuilder = ImmutableArray.CreateBuilder<ModuleItemId>();

            Invoker wrappedGetEnumerator = 
                (domainService, typeArgs, thisValue, args, result) => EnumerableObject.NativeGetEnumerator(domainService, enumeratorId, typeArgs, thisValue, args, result);

            builder.AddMemberFunc(Name.MakeText("GetEnumerator"),
                false, true, ImmutableArray<string>.Empty, enumeratorTypeValue, ImmutableArray<TypeValue>.Empty,
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
        internal static ValueTask NativeGetEnumerator(DomainService domainService, ModuleItemId enumeratorId, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(thisValue != null);
            Debug.Assert(result != null);

            var enumerableObject = GetObject<EnumerableObject>(thisValue);

            var enumeratorInst = domainService.GetTypeInst(TypeValue.MakeNormal(enumeratorId, typeArgList)); // 같은 TypeArgList를 사용한다
            
            ((ObjectValue)result).SetObject(new EnumeratorObject(enumeratorInst, enumerableObject.enumerable.GetAsyncEnumerator()));

            return new ValueTask();
        }

        public override TypeInst GetTypeInst()
        {
            return typeInst;
        }
    }
}
