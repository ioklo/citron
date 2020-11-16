using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
    // Type<int>.VarName or VarName
    public class VarValue
    {
        public ModuleName ModuleName { get; }   // global module name, 일단 string
        public NamespacePath NamespacePath { get; }    // root namespace
        public ImmutableArray<AppliedItemPathEntry> TypeEntries { get; }
        public Name Name { get; }

        public VarValue(ModuleName moduleName, NamespacePath namespacePath, Name name)
            : this(moduleName, namespacePath, Array.Empty<AppliedItemPathEntry>(), name)
        {
        }
        
        public VarValue(ModuleName moduleName, NamespacePath namespacePath, IEnumerable<AppliedItemPathEntry> typeEntries, Name name)
        {
            ModuleName = moduleName;
            NamespacePath = namespacePath;
            TypeEntries = typeEntries.ToImmutableArray();
            Name = name;
        }

        public VarValue(ItemId id, params TypeValue[][] typeArgList)
        {
            ModuleName = id.ModuleName;
            NamespacePath = id.NamespacePath;
            Debug.Assert(id.OuterEntries.Length == typeArgList.Length);
            TypeEntries = id.OuterEntries.Zip(typeArgList, (entry, typeArgs) => new AppliedItemPathEntry(entry.Name, entry.ParamHash, typeArgs)).ToImmutableArray();
            Name = id.Entry.Name;

            Debug.Assert(id.Entry.TypeParamCount == 0 && id.Entry.ParamHash == string.Empty);
        }

        public ItemId GetItemId()
        {
            if (TypeEntries.Length == 0)
                return new ItemId(ModuleName, NamespacePath, new ItemPathEntry(Name));
            else
                return new ItemId(ModuleName, NamespacePath, TypeEntries.Select(entry => entry.GetItemPathEntry()), new ItemPathEntry(Name));
        }
    }
}
