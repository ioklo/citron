using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using Pretune;
using System.Diagnostics;

namespace Citron.Syntax
{
    public abstract record class TypeExp : ISyntaxNode
    {
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, ITypeExpVisitor<TResult>;
    }

    public record class IdTypeExp(string Name, ImmutableArray<TypeExp> TypeArgs) : TypeExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitId(this);        
    }

    public record class MemberTypeExp(TypeExp Parent, string MemberName, ImmutableArray<TypeExp> TypeArgs) : TypeExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitMember(this);
    }

    public record class NullableTypeExp(TypeExp InnerTypeExp) : TypeExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNullable(this);
    }

    public record class LocalPtrTypeExp(TypeExp InnerTypeExp) : TypeExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalPtr(this);
    }

    public record class BoxPtrTypeExp(TypeExp InnerTypeExp) : TypeExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxPtr(this);
    }    

    public interface ITypeExpVisitor<TResult>
    {
        TResult VisitId(IdTypeExp typeExp);
        TResult VisitMember(MemberTypeExp typeExp);
        TResult VisitNullable(NullableTypeExp typeExp);
        TResult VisitLocalPtr(LocalPtrTypeExp typeExp);
        TResult VisitBoxPtr(BoxPtrTypeExp typeExp);
    }
}