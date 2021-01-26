using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
#pragma warning disable CS0660, CS0661
    public abstract partial class TypeValue
    {
        // "var"
        public class Var : TypeValue
        {
            public static Var Instance { get; } = new Var();
            private Var() { }
        }

        // T: depth는 지역적이므로, 주어진 컨텍스트 안에서만 의미가 있다
        public class TypeVar : TypeValue
        {
            public int Depth { get; }
            public int Index { get; }
            public string Name { get; }

            public TypeVar(int depth, int index, string name)
            {
                Depth = depth;                
                Name = name;
            }
        }
        
        public class Normal : TypeValue
        {
            public ModuleName ModuleName { get; }   // global module name, 일단 string
            public NamespacePath NamespacePath { get => Path.NamespacePath; }    // root namespace
            public ImmutableArray<AppliedItemPathEntry> OuterEntries { get => Path.OuterEntries; }
            public AppliedItemPathEntry Entry { get => Path.Entry; }

            public AppliedItemPath Path { get; }

            public Normal(ModuleName moduleName, NamespacePath namespacePath, AppliedItemPathEntry entry)
                : this(moduleName, namespacePath, Array.Empty<AppliedItemPathEntry>(), entry)
            {
            }

            public Normal? GetOuter()
            {
                var outerPath = Path.GetOuter();
                if (outerPath == null) return null;

                return new Normal(ModuleName, outerPath.Value);
            }

            public Normal(ModuleName moduleName, AppliedItemPath path)
            {
                ModuleName = moduleName;
                this.Path = path;
            }

            public Normal(ModuleName moduleName, NamespacePath namespacePath, IEnumerable<AppliedItemPathEntry> outerEntries, AppliedItemPathEntry entry)
            {
                ModuleName = moduleName;
                Path = new AppliedItemPath(namespacePath, outerEntries, entry);
            }

            public Normal(ItemId typeId, params TypeValue[][] typeArgList)
            {
                ModuleName = typeId.ModuleName;
                Debug.Assert(typeId.OuterEntries.Length == typeArgList.Length - 1);

                Path = new AppliedItemPath(
                    typeId.NamespacePath,
                    typeId.OuterEntries.Zip(typeArgList.SkipLast(1), (entry, typeArgs) => new AppliedItemPathEntry(entry.Name, entry.ParamHash, typeArgs)),
                    new AppliedItemPathEntry(typeId.Entry.Name, typeId.Entry.ParamHash, typeArgList[typeArgList.Length - 1])
                );
            }

            public Normal(ItemId typeId)
            {
                ModuleName = typeId.ModuleName;

                Debug.Assert(typeId.OuterEntries.All(entry => entry.TypeParamCount == 0));
                Debug.Assert(typeId.Entry.TypeParamCount == 0);

                Path = new AppliedItemPath(
                    typeId.NamespacePath,
                    typeId.OuterEntries.Select(entry => new AppliedItemPathEntry(entry.Name, entry.ParamHash)),
                    new AppliedItemPathEntry(typeId.Entry.Name, typeId.Entry.ParamHash)
                );
            }

            public ItemId GetTypeId()
            {
                return new ItemId(ModuleName, NamespacePath, OuterEntries.Select(entry => entry.GetItemPathEntry()), Entry.GetItemPathEntry());
            }

            public IEnumerable<AppliedItemPathEntry> GetAllEntries()
            {
                return OuterEntries.Append(Entry);
            }
        }

        // "void"
        public class Void : TypeValue
        {
            public static Void Instance { get; } = new Void();
            private Void() { }
        }

        // ArgTypeValues => RetValueTypes
        public class Func : TypeValue
        {
            public TypeValue Return { get; }
            public ImmutableArray<TypeValue> Params { get; }

            public Func(TypeValue ret, IEnumerable<TypeValue> parameters)
            {
                Return = ret;
                Params = parameters.ToImmutableArray();
            }
        }

        public class EnumElem : TypeValue
        {
            public Normal EnumTypeValue { get; }
            public string Name { get; }

            public EnumElem(Normal enumTypeValue, string name)
            {
                EnumTypeValue = enumTypeValue;
                Name = name;
            }
        }        
    }
    
#pragma warning restore CS0660, CS0661
}
