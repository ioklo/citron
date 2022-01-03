using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using S = Gum.Syntax;
using Pretune;

namespace Gum.IR0Translator
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
        public TypeDeclPath Path { get; }
        public TypeSkeletonKind Kind { get; }
        ImmutableDictionary<TypeName, TypeSkeleton> membersByName;

        public TypeSkeleton(TypeDeclPath path, ImmutableArray<TypeSkeleton> members, TypeSkeletonKind kind)
        {
            Path = path;
            Kind = kind;

            var builder = ImmutableDictionary.CreateBuilder<TypeName, TypeSkeleton>();
            foreach (var member in members)
                builder.Add(member.Path.Name, member);
            membersByName = builder.ToImmutable();
        }
        
        public TypeSkeleton? GetMember(Name memberName, int typeParamCount)
        {
            return membersByName.GetValueOrDefault(new TypeName(memberName, typeParamCount));
        }
    }
}
