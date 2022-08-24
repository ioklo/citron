using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using System.Diagnostics;

using S = Citron.Syntax;
using M = Citron.Module;
using Citron.Symbol;

namespace Citron.Analysis
{
    // DeclSymbolPath -> TypeSkeleton
    class SkeletonRepository
    {
        ImmutableDictionary<DeclSymbolPath, Skeleton> allMembers;

        public static SkeletonRepository Build(ImmutableArray<Skeleton> rootMembers)
        {
            var allMembersBuilder = ImmutableDictionary.CreateBuilder<DeclSymbolPath, Skeleton>();            

            foreach (var rootMember in rootMembers)
            {
                FillChild(allMembersBuilder, rootMember);

                allMembersBuilder.Add(rootMember.Path, rootMember);
            }

            return new SkeletonRepository(allMembersBuilder.ToImmutable());
        }

        static void FillChild(ImmutableDictionary<DeclSymbolPath, Skeleton>.Builder builder, Skeleton skel)
        {
            foreach (var elem in skel.GetAllMembers())
            {
                FillChild(builder, elem);
                builder.Add(skel.Path, skel);
            }
        }

        SkeletonRepository(ImmutableDictionary<DeclSymbolPath, Skeleton> allMembers)
        {
            this.allMembers = allMembers;
        }

        public Skeleton? GetTypeSkeleton(DeclSymbolPath path)
        {
            return allMembers.GetValueOrDefault(path);
        }
    }
}
