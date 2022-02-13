using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using System.Diagnostics;

using S = Citron.Syntax;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    // DeclSymbolPath -> TypeSkeleton
    class TypeSkeletonRepository
    {
        ImmutableDictionary<M.DeclSymbolPath, TypeSkeleton> allMembers;

        public static TypeSkeletonRepository Build(ImmutableArray<TypeSkeleton> rootMembers)
        {
            var allMembersBuilder = ImmutableDictionary.CreateBuilder<M.DeclSymbolPath, TypeSkeleton>();            

            foreach (var rootMember in rootMembers)
            {
                FillChild(allMembersBuilder, rootMember);

                allMembersBuilder.Add(rootMember.Path, rootMember);
            }

            return new TypeSkeletonRepository(allMembersBuilder.ToImmutable());
        }

        static void FillChild(ImmutableDictionary<M.DeclSymbolPath, TypeSkeleton>.Builder builder, TypeSkeleton skel)
        {
            foreach (var elem in skel.GetAllMembers())
            {
                FillChild(builder, elem);
                builder.Add(skel.Path, skel);
            }
        }

        TypeSkeletonRepository(ImmutableDictionary<M.DeclSymbolPath, TypeSkeleton> allMembers)
        {
            this.allMembers = allMembers;
        }

        public TypeSkeleton? GetTypeSkeleton(M.DeclSymbolPath path)
        {
            return allMembers.GetValueOrDefault(path);
        }
    }
}
