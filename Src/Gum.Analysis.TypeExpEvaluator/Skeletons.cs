using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using S = Gum.Syntax;
using Pretune;

namespace Gum.Analysis
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
        public DeclSymbolPath Path { get; }
        public TypeSkeletonKind Kind { get; }
        ImmutableDictionary<TypeName, TypeSkeleton> membersByName;

        public TypeSkeleton(DeclSymbolPath path, ImmutableArray<TypeSkeleton> members, TypeSkeletonKind kind)
        {
            Path = path;
            Kind = kind;

            var builder = ImmutableDictionary.CreateBuilder<TypeName, TypeSkeleton>();
            foreach (var member in members)
                builder.Add(new TypeName(member.Path.Name, member.Path.TypeParamCount), member);

            membersByName = builder.ToImmutable();
        }
        
        public TypeSkeleton? GetMember(Name memberName, int typeParamCount)
        {
            return membersByName.GetValueOrDefault(new TypeName(memberName, typeParamCount));
        }

        public IEnumerable<TypeSkeleton> GetAllMembers()
        {
            return membersByName.Values;
        }
    }
}
