using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using S = Citron.Syntax;
using M = Citron.Module;
using Pretune;
using Citron.Symbol;

namespace Citron.Analysis
{
    enum TypeSkeletonKind
    {
        Class,
        Struct,
        Interface,
        Enum,
        EnumElem,
        TypeVar,
    }

    // TODO: Type뿐 아니라 Namespace 등도 여기서
    class TypeSkeleton
    {
        public DeclSymbolPath Path { get; }
        public TypeSkeletonKind Kind { get; }
        ImmutableDictionary<M.TypeName, TypeSkeleton> membersByName;

        public TypeSkeleton(DeclSymbolPath path, ImmutableArray<TypeSkeleton> members, TypeSkeletonKind kind)
        {
            Path = path;
            Kind = kind;

            var builder = ImmutableDictionary.CreateBuilder<M.TypeName, TypeSkeleton>();
            foreach (var member in members)
                builder.Add(new M.TypeName(member.Path.Name, member.Path.TypeParamCount), member);

            membersByName = builder.ToImmutable();
        }
        
        public TypeSkeleton? GetMember(M.Name memberName, int typeParamCount)
        {
            return membersByName.GetValueOrDefault(new M.TypeName(memberName, typeParamCount));
        }

        public IEnumerable<TypeSkeleton> GetAllMembers()
        {
            return membersByName.Values;
        }
    }
}
