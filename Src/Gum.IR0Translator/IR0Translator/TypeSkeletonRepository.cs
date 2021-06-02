using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using S = Gum.Syntax;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    // ItemPath -> TypeSkeleton
    class TypeSkeletonRepository
    {
        ImmutableDictionary<ItemPathEntry, TypeSkeleton> rootMembers;

        public TypeSkeletonRepository(ImmutableArray<TypeSkeleton> rootMembers)
        {
            var builder = ImmutableDictionary.CreateBuilder<ItemPathEntry, TypeSkeleton>();
            foreach (var member in rootMembers)
                builder.Add(member.PathEntry, member);
            this.rootMembers = builder.ToImmutable();
        }

        public TypeSkeleton? GetRootTypeSkeleton(NamespacePath namespacePath, ItemPathEntry entry)
        {
            // TODO: namespace 까지 TypeSkeleton에 편입
            Debug.Assert(namespacePath.IsRoot);
            return rootMembers.GetValueOrDefault(entry);
        }
    }
}
