﻿using Citron.Symbol;
using R = Citron.IR0;

namespace Citron.Analysis;

abstract record class ResolvedExp
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IResolvedExpVisitor<TResult>;
    public abstract IType GetExpType();

    public record class ThisVar(IType Type) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
        public override IType GetExpType() { return Type; }
    }
    public record class LocalVar(IType Type, Name Name) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
        public override IType GetExpType() { return Type; }
    }
    public record class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
        public override IType GetExpType() { return Symbol.GetDeclType(); }
    }

    public record class ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedInstanceExp? ExplicitInstance) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
        public override IType GetExpType() { return Symbol.GetDeclType(); }
    }

    public record class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedInstanceExp? ExplicitInstance) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
        public override IType GetExpType() { return Symbol.GetDeclType(); }
    }
    
    public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol, ResolvedInstanceExp Instance) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
        public override IType GetExpType() { return Symbol.GetDeclType(); }
    }

    public record class LocalDeref(ResolvedExp Target) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalDeref(this);
        public override IType GetExpType() { return Target.GetExpType(); }
    }

    public record class BoxDeref(ResolvedExp Target) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxDeref(this);
        public override IType GetExpType() { return Target.GetExpType(); }
    }

    public record class ListIndexer(ResolvedInstanceExp Instance, R.Exp Index, IType ItemType) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
        public override IType GetExpType() { return ItemType; }
    }

    // 기타의 경우, Value
    public record class IR0Exp(R.Exp Exp) : ResolvedExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIR0Exp(this);
        public override IType GetExpType() { return Exp.GetExpType(); }
    }
}
