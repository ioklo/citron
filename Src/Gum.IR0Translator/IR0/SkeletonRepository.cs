using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.IR0
{
    class SkeletonRepository
    {
        ImmutableArray<Skeleton> globalSkeletons;
        ImmutableDictionary<ItemPath, Skeleton> skeletonsByPath;

        public SkeletonRepository(IEnumerable<Skeleton> globalSkeletons)
        {
            this.globalSkeletons = globalSkeletons.ToImmutableArray();
            skeletonsByPath = MakeDict(globalSkeletons);
        }

        static ImmutableDictionary<ItemPath, Skeleton> MakeDict(IEnumerable<Skeleton> globalSkeletons)
        {
            void Fill(Skeleton skel, ImmutableDictionary<ItemPath, Skeleton>.Builder builder)
            {
                builder.Add(skel.Path, skel);

                if (skel is Skeleton.Type typeSkel)
                    foreach (var member in typeSkel.GetMembers())
                        Fill(member, builder);
            }

            var builder = ImmutableDictionary.CreateBuilder<ItemPath, Skeleton>(ModuleInfoEqualityComparer.Instance);

            foreach(var globalSkel in globalSkeletons)
                Fill(globalSkel, builder);

            return builder.ToImmutable();
        }

        public Skeleton? GetSkeleton(ItemPath path)
        {
            return skeletonsByPath.GetValueOrDefault(path);
        }

        public IEnumerable<Skeleton> GetGlobalSkeletons()
        {
            return globalSkeletons;
        }
    }
}
