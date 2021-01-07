using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.CompileTime
{
    // ModuleName없는 나머지 부분
    public struct ItemPath
    {
        public NamespacePath NamespacePath { get; }    // root namespace
        public ImmutableArray<ItemPathEntry> OuterEntries { get; } // 
        public ItemPathEntry Entry { get; }                       // 최종 엔트리

        public ItemPath(NamespacePath namespacePath, Name name, int typeParamCount = 0, string paramHash = "")
        {
            NamespacePath = namespacePath;
            OuterEntries = ImmutableArray<ItemPathEntry>.Empty;
            Entry = new ItemPathEntry(name, typeParamCount, paramHash);
        }

        public ItemPath(NamespacePath namespacePath, ItemPathEntry entry)
        {
            NamespacePath = namespacePath;
            OuterEntries = ImmutableArray<ItemPathEntry>.Empty;
            Entry = entry;
        }

        public ItemPath(NamespacePath namespacePath, IEnumerable<ItemPathEntry> outerEntries, ItemPathEntry entry)
        {
            NamespacePath = namespacePath;
            OuterEntries = outerEntries.ToImmutableArray();
            Entry = entry;
        }        

        public ItemPath Append(Name name, int typeParamCount = 0, string paramHash = "")
        {
            return new ItemPath(NamespacePath, OuterEntries.Append(Entry), new ItemPathEntry(name, typeParamCount, paramHash));
        }
    }
}