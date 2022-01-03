using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using S = Gum.Syntax;
using System.Diagnostics;
using Gum.Analysis;

namespace Gum.IR0Translator
{
    // ItemPath -> TypeSkeleton
    class TypeSkeletonRepository
    {
        ImmutableDictionary<TypeDeclPath, TypeSkeleton> members;

        public TypeSkeletonRepository(ImmutableArray<TypeSkeleton> rootMembers)
        {
            var builder = ImmutableDictionary.CreateBuilder<TypeDeclPath, TypeSkeleton>();

            foreach (var member in rootMembers)
                builder.Add(member.Path, member);

            this.members = builder.ToImmutable();
        }

        public TypeSkeleton? GetRootTypeSkeleton(TypeDeclPath path)
        {
            return members.GetValueOrDefault(path);
        }
    }
}
