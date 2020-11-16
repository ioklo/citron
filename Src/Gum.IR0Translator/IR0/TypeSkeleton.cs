using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.IR0
{
    // TypeSkeleton 정보, 이름별 TypeId와 부속타입 정보, 타입 파라미터 개수
    public class TypeSkeleton
    {
        public ItemPath Path { get; }
        private Dictionary<ItemPathEntry, TypeSkeleton> memberTypeSkeletons;
        private ImmutableHashSet<string> enumElemNames;

        public TypeSkeleton(ItemPath path, IEnumerable<string> enumElemNames)
        {
            Path = path;
            memberTypeSkeletons = new Dictionary<ItemPathEntry, TypeSkeleton>();
            this.enumElemNames = enumElemNames.ToImmutableHashSet();
        }

        public TypeSkeleton? GetMemberTypeSkeleton(string name, int typeParamCount)
        {
            return memberTypeSkeletons.GetValueOrDefault(new ItemPathEntry(name, typeParamCount));
        }

        public bool ContainsEnumElem(string name)
        {
            return enumElemNames.Contains(name);
        }

        public void AddMemberTypeSkeleton(string name, int typeParamCount, TypeSkeleton skeleton)
        {
            Debug.Assert(!enumElemNames.Contains(name));
            memberTypeSkeletons.Add(new ItemPathEntry(name, typeParamCount), skeleton);
        }
    }
}
