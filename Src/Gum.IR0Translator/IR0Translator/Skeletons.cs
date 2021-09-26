using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using S = Gum.Syntax;
using Pretune;

namespace Gum.IR0Translator
{
    enum TypeSkeletonKind
    {
        Class,
        Struct,
        Interface,
        Enum,
        EnumElem,

    }
    // TODO: Type뿐 아니라 Namespace 등도 여기서
    class TypeSkeleton
    {
        public ItemPathEntry PathEntry { get; }
        public TypeSkeletonKind Kind { get; }
        ImmutableDictionary<ItemPathEntry, TypeSkeleton> membersByEntry;

        public TypeSkeleton(ItemPathEntry pathEntry, ImmutableArray<TypeSkeleton> members, TypeSkeletonKind kind)
        {
            PathEntry = pathEntry;
            Kind = kind;

            var builder = ImmutableDictionary.CreateBuilder<ItemPathEntry, TypeSkeleton>();
            foreach (var member in members)
                builder.Add(member.PathEntry, member);
            membersByEntry = builder.ToImmutable();
        }
        
        public TypeSkeleton? GetMember(Name memberName, int typeParamCount)
        {
            return membersByEntry.GetValueOrDefault(new ItemPathEntry(memberName, typeParamCount));
        }
    }
}
