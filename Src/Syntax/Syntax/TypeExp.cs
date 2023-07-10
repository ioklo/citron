﻿using Citron.Syntax;
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

    public record class LocalRefTypeExp(TypeExp InnerTypeExp) : TypeExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalRef(this);
    }

    public record class BoxRefTypeExp(TypeExp InnerTypeExp) : TypeExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxRef(this);
    }

    public interface ITypeExpVisitor<TResult>
    {
        TResult VisitId(IdTypeExp typeExp);
        TResult VisitMember(MemberTypeExp typeExp);
        TResult VisitNullable(NullableTypeExp typeExp);
        TResult VisitLocalRef(LocalRefTypeExp typeExp);
        TResult VisitBoxRef(BoxRefTypeExp typeExp);
    }
}