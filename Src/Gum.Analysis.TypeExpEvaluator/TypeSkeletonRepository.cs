using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using S = Gum.Syntax;
using System.Diagnostics;
using Gum.Analysis;

namespace Gum.Analysis
{
    // DeclSymbolPath -> TypeSkeleton
    class TypeSkeletonRepository
    {
        ImmutableDictionary<DeclSymbolPath, TypeSkeleton> allMembers;

        public static TypeSkeletonRepository Build(ImmutableArray<TypeSkeleton> rootMembers)
        {
            var allMembersBuilder = ImmutableDictionary.CreateBuilder<DeclSymbolPath, TypeSkeleton>();            

            foreach (var rootMember in rootMembers)
            {
                FillChild(allMembersBuilder, rootMember);

                allMembersBuilder.Add(rootMember.Path, rootMember);
            }

            return new TypeSkeletonRepository(allMembersBuilder.ToImmutable());
        }

        static void FillChild(ImmutableDictionary<DeclSymbolPath, TypeSkeleton>.Builder builder, TypeSkeleton skel)
        {
            foreach (var elem in skel.GetAllMembers())
            {
                FillChild(builder, elem);
                builder.Add(skel.Path, skel);
            }
        }

        TypeSkeletonRepository(ImmutableDictionary<DeclSymbolPath, TypeSkeleton> allMembers)
        {
            this.allMembers = allMembers;
        }

        public TypeSkeleton? GetTypeSkeleton(DeclSymbolPath path)
        {
            return allMembers.GetValueOrDefault(path);
        }
    }
}
