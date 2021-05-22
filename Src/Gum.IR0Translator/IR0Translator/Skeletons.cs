using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    abstract class TypeSkeleton
    {
        public ItemPath Path { get; }
        List<TypeSkeleton> members;
        Dictionary<ItemPathEntry, TypeSkeleton> membersByEntry;

        public TypeSkeleton(ItemPath path)
        {
            Path = path;
            members = new List<TypeSkeleton>();
            membersByEntry = new Dictionary<ItemPathEntry, TypeSkeleton>();
        }        

        public void AddMember(TypeSkeleton member)
        {
            members.Add(member);
            membersByEntry.Add(member.Path.Entry, member);
        }

        public TypeSkeleton? GetMember(string memberName, int typeParamCount)
        {
            return membersByEntry.GetValueOrDefault(new ItemPathEntry(memberName, typeParamCount));
        }
    }

    class RootSkeleton : TypeSkeleton
    {
    }

    class EnumSkeleton : TypeSkeleton
    {
        public S.EnumDecl EnumDecl { get; }
        public EnumSkeleton(ItemPath path, S.EnumDecl enumDecl)
            : base(path)
        {
            EnumDecl = enumDecl;
        }
    }

    class EnumElemSkeleton : TypeSkeleton
    {
    }

    class StructSkeleton : TypeSkeleton
    {
        public S.StructDecl StructDecl { get; }
        public StructSkeleton(ItemPath path, S.StructDecl structDecl)
            : base(path)
        {
            StructDecl = structDecl;
        }
    }    
}
