using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    // Root(ModuleName, Namespace) . RootItem . Item
    public struct ItemId : IEquatable<ItemId>
    {
        public ModuleName ModuleName { get; }          // global module name
        internal ItemPath path;
        public NamespacePath NamespacePath { get => path.NamespacePath; }    // root namespace
        public ImmutableArray<ItemPathEntry> OuterEntries { get => path.OuterEntries; } // 
        public ItemPathEntry Entry { get => path.Entry; }                       // 최종 엔트리

        public ItemId(ModuleName moduleName, ItemPath path)
        {
            ModuleName = moduleName;
            this.path = path;
        }
        
        public ItemId(ModuleName moduleName, NamespacePath nsPath, ItemPathEntry entry0, params ItemPathEntry[] entries)
        {
            ModuleName = moduleName;
            if (entries.Length == 0)
            {
                path = new ItemPath(nsPath, entry0);
            }
            else
            {
                path = new ItemPath(nsPath, entries.Prepend(entry0).SkipLast(1), entries[entries.Length - 1]);
            }
        }

        public ItemId(ModuleName moduleName, NamespacePath nsPath, IEnumerable<ItemPathEntry> outerEntries, ItemPathEntry entry)
        {
            ModuleName = moduleName;
            path = new ItemPath(nsPath, outerEntries, entry);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public ItemPath GetItemPath()
        {
            return path;
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append($"[{ModuleName}]{NamespacePath}");

            if (NamespacePath.Entries.Length != 0)
                sb.Append('.');

            foreach(var itemEntry in OuterEntries)
            {
                itemEntry.ToString(sb);
                sb.Append('.');
            }

            Entry.ToString(sb);
        }
        
        public static ItemId Make(ModuleName moduleName, NamespacePath nsPath, Name name, int typeParamCount = 0, string paramHash = "")
        {
            return new ItemId(moduleName, nsPath, Array.Empty<ItemPathEntry>(), new ItemPathEntry(name, typeParamCount, paramHash));
        }

        public static ItemId Make(ModuleName moduleName, NamespacePath nsPath, ItemPathEntry entry0, params ItemPathEntry[] restEntries)
        {
            if (restEntries.Length == 0)
                return new ItemId(moduleName, nsPath, Array.Empty<ItemPathEntry>(), entry0);
            else
                return new ItemId(moduleName, nsPath, 
                    Enumerable.Repeat(entry0, 1).Concat(restEntries.Take(restEntries.Length - 1)), 
                    restEntries[restEntries.Length - 1]);
        }

        public ItemId Append(Name name, int typeParamCount = 0, string paramHash = "")
        {
            return new ItemId(ModuleName, path.Append(name, typeParamCount, paramHash));
        }

        public bool Equals(ItemId other)
        {
            throw new NotImplementedException();
        }
    }

    // (System.Runtime, System.X<,>.Y<,,>.T)
    // Absolute, Relative 둘다
}