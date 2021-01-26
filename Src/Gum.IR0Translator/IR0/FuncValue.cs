using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    // FuncApp
    public class FuncValue
    {
        public ModuleName ModuleName { get; }   // global module name, 일단 string
        public NamespacePath NamespacePath { get => path.NamespacePath; }    // root namespace
        public ImmutableArray<AppliedItemPathEntry> OuterEntries { get => path.OuterEntries; }
        public AppliedItemPathEntry Entry { get => path.Entry; }

        AppliedItemPath path;

        public FuncValue(ModuleName moduleName, NamespacePath namespacePath, AppliedItemPathEntry entry)
            : this(moduleName, namespacePath, Array.Empty<AppliedItemPathEntry>(), entry)
        {
        }

        public FuncValue(ModuleName moduleName, NamespacePath namespacePath, IEnumerable<AppliedItemPathEntry> outerEntries, AppliedItemPathEntry entry)
        {
            ModuleName = moduleName;
            path = new AppliedItemPath(namespacePath, outerEntries, entry);
        }

        public FuncValue(ItemId funcId, params TypeValue[][] typeArgList)
        {
            ModuleName = funcId.ModuleName;

            Debug.Assert(funcId.OuterEntries.Length == typeArgList.Length - 1);
            path = new AppliedItemPath(
                funcId.NamespacePath,            
                funcId.OuterEntries.Zip(typeArgList.SkipLast(1), (entry, typeArgs) => new AppliedItemPathEntry(entry.Name, entry.ParamHash, typeArgs)),
                new AppliedItemPathEntry(funcId.Entry.Name, funcId.Entry.ParamHash, typeArgList[typeArgList.Length - 1])
            );
        }        

        public ItemId GetFuncId()
        {
            return new ItemId(ModuleName, NamespacePath, OuterEntries.Select(entry => entry.GetItemPathEntry()), Entry.GetItemPathEntry());
        }

        public TypeValue.Normal? GetOuter()
        {
            var outerPath = path.GetOuter();
            if (outerPath == null) return null;

            return new TypeValue.Normal(ModuleName, outerPath.Value);
        }
    }
}
