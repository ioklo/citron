using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Invoker = Citron.Runtime.RuntimeModuleMisc.Invoker;

namespace Citron.Runtime
{
    class ListBuildInfo : RuntimeModuleTypeBuildInfo.Class
    {
        public ListBuildInfo()
            : base(null, RuntimeModule.ListId, new[] { "T" }, null, () => new ObjectValue(null))
        {
        }       

        public override void Build(RuntimeModuleTypeBuilder builder)
        {
            ITypeSymbol intTypeValue = new NormalTypeValue(RuntimeModule.IntId);
            ITypeSymbol listElemTypeValue = new TypeVarTypeValue(RuntimeModule.ListId, "T");

            // List<T>.Add
            builder.AddMemberFunc("Add",
                bSeqCall: false, bThisCall: true, Array.Empty<string>(),
                VoidTypeValue.Instance, new[] { listElemTypeValue }, ListObject.NativeAdd);

            // List<T>.RemoveAt(int index)     
            builder.AddMemberFunc("RemoveAt",
                bSeqCall: false, bThisCall: true, Array.Empty<string>(),
                VoidTypeValue.Instance, new[] { intTypeValue }, ListObject.NativeRemoveAt);

            // Enumerator<T> List<T>.GetEnumerator()
            Invoker wrappedGetEnumerator =
                (domainService, typeArgs, thisValue, args, result) => ListObject.NativeGetEnumerator(domainService, RuntimeModule.EnumeratorId, typeArgs, thisValue, args, result);

            builder.AddMemberFunc("GetEnumerator",
                bSeqCall: false, bThisCall: true, Array.Empty<string>(),
                new NormalTypeValue(RuntimeModule.EnumeratorId, TypeArgumentList.Make(listElemTypeValue)), Enumerable.Empty<ITypeSymbol>(), wrappedGetEnumerator);

            // T List<T>.Indexer(int index)
            builder.AddMemberFunc(SpecialNames.IndexerGet,
                bSeqCall: false, bThisCall: true, Array.Empty<string>(),
                listElemTypeValue, new[] { intTypeValue }, ListObject.NativeIndexer);
            
            return;
        }
    }

    // List
    public class ListObject : GumObject
    {
        TypeInst typeInst;
        public List<Value> Elems { get; }

        public ListObject(TypeInst typeInst, List<Value> elems)
        {
            this.typeInst = typeInst;
            Elems = elems;
        }
        
        // Enumerator<T> List<T>.GetEnumerator()
        internal static ValueTask NativeGetEnumerator(DomainService domainService, ItemId enumeratorId, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(thisValue != null);
            Debug.Assert(result != null);
            var list = GetObject<ListObject>(thisValue);

            // enumerator<T>
            var enumeratorInst = domainService.GetTypeInst(new NormalTypeValue(enumeratorId, typeArgList));

            // TODO: Runtime 메모리 관리자한테 new를 요청해야 합니다
            ((ObjectValue)result).SetObject(new EnumeratorObject(enumeratorInst, ToAsyncEnum(list.Elems).GetAsyncEnumerator()));

            return new ValueTask();

#pragma warning disable CS1998
            async IAsyncEnumerable<Value> ToAsyncEnum(IEnumerable<Value> enumerable)
            {
                foreach(var elem in enumerable)
                    yield return elem;
            }
#pragma warning restore CS1998
        }

        internal static ValueTask NativeIndexer(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(args.Count == 1);
            Debug.Assert(thisValue != null);

            var list = GetObject<ListObject>(thisValue);

            result!.SetValue(list.Elems[((Value<int>)args[0]).Data]);

            return new ValueTask();
        }

        // List<T>.Add
        internal static ValueTask NativeAdd(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(args.Count == 1);
            Debug.Assert(thisValue != null);

            var list = GetObject<ListObject>(thisValue);
            list.Elems.Add(args[0]);

            return new ValueTask();
        }

        internal static ValueTask NativeRemoveAt(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(args.Count == 1);
            Debug.Assert(thisValue != null);
            Debug.Assert(result == null);

            var list = GetObject<ListObject>(thisValue);
            list.Elems.RemoveAt(((Value<int>)args[0]).Data);

            return new ValueTask();
        }

        public override TypeInst GetTypeInst()
        {
            return typeInst;
        }
    }
}
