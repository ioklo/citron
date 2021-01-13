using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using S = Gum.Syntax;

namespace Gum.IR0
{
    abstract class Skeleton
    {
        public ItemPath Path { get; }
        public Skeleton(ItemPath path)
        {
            Path = path;
        }

        public abstract class Type : Skeleton
        {
            List<Skeleton> members;
            Dictionary<ItemPathEntry, Skeleton> membersByEntry;

            public Type(ItemPath path)
                : base(path)
            {
                members = new List<Skeleton>();
                membersByEntry = new Dictionary<ItemPathEntry, Skeleton>(ModuleInfoEqualityComparer.Instance);
            }

            public IEnumerable<Skeleton> GetMembers()
            {
                return members;
            }

            public void AddMember(Skeleton member)
            {
                members.Add(member);
                membersByEntry.Add(member.Path.Entry, member);
            }

            public Skeleton? GetMember(string memberName, int typeParamCount)
            {
                return membersByEntry.GetValueOrDefault(new ItemPathEntry(memberName, typeParamCount));
            }
        }
        
        public class Enum : Type
        {
            public S.EnumDecl EnumDecl { get; }
            public Enum(ItemPath path, S.EnumDecl enumDecl)
                : base(path) 
            {
                EnumDecl = enumDecl;
            }
        }

        public class Struct : Type
        {
            public S.StructDecl StructDecl { get; }
            public Struct(ItemPath path, S.StructDecl structDecl)
                : base(path) 
            {
                StructDecl = structDecl;
            }
        }

        public class Func : Skeleton
        {
            public S.FuncDecl FuncDecl { get; }
            public Func(ItemPath path, S.FuncDecl decl)
                : base(path)
            {
                FuncDecl = decl;
            }
        }
        
        public class Var : Skeleton
        {
            public S.VarDecl VarDecl { get; }
            public int ElemIndex { get; }
            
            public Var(ItemPath path, S.VarDecl varDecl, int elemIndex)
                : base(path)
            {
                VarDecl = varDecl;
                ElemIndex = elemIndex;
            }
        }
    }    
}
