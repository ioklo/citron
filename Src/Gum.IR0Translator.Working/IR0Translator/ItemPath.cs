using Gum.CompileTime;
using Pretune;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.IR0Translator
{
    // ModuleName없는 나머지 부분
    public struct ItemPath : IEquatable<ItemPath>
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

        public override bool Equals(object? obj)
        {
            return obj is ItemPath path && Equals(path);
        }

        public bool Equals(ItemPath other)
        {
            return EqualityComparer<NamespacePath>.Default.Equals(NamespacePath, other.NamespacePath) &&
                   OuterEntries.SequenceEqual(other.OuterEntries) &&
                   EqualityComparer<ItemPathEntry>.Default.Equals(Entry, other.Entry);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NamespacePath, OuterEntries, Entry);
        }
    }
}