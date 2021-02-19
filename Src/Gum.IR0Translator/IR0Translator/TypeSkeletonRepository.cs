using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    // ItemPath -> TypeSkeleton
    class TypeSkeletonRepository
    {
        ImmutableDictionary<ItemPath, TypeSkeleton> typeSkeletonsByPath;

        public TypeSkeletonRepository(ImmutableDictionary<ItemPath, TypeSkeleton> typeSkeletonsByPath)
        {
            this.typeSkeletonsByPath = typeSkeletonsByPath;
        }

        public TypeSkeleton? GetTypeSkeleton(ItemPath path)
        {
            return typeSkeletonsByPath.GetValueOrDefault(path);
        }
    }
}
