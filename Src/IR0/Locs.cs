using Citron.Symbol;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0
{
    public abstract record class Loc : INode
    {
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, IIR0LocVisitor<TResult>;
    }

    // 임시 값을 만들어서 Exp를 실행해서 대입해주는 역할
    public record class TempLoc(Exp Exp) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTemp(this);
    }

    public record class LocalVarLoc(Name Name) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
    }

    // only this member allowed, so no need this
    public record class LambdaMemberVarLoc(LambdaMemberVarSymbol MemberVar) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
    }

    // l[b], l is list    
    public record class ListIndexerLoc(Loc List, Exp Index) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
    }

    // Instance가 null이면 static
    public record class StructMemberLoc(Loc? Instance, StructMemberVarSymbol MemberVar) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMember(this);
    }

    public record class ClassMemberLoc(Loc? Instance, ClassMemberVarSymbol MemberVar) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMember(this);
    }

    public record class EnumElemMemberLoc(Loc Instance, EnumElemMemberVarSymbol MemberVar) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMember(this);
    }

    public record class ThisLoc() : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
    }
    
    // dereference pointer, *
    public record class LocalDerefLoc(Loc InnerLoc) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalDeref(this);
    }

    // dereference box pointer, *
    public record class BoxDerefLoc(Loc InnerLoc) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxDeref(this);
    }

    // nullable value에서 value를 가져온다
    public record class NullableValueLoc(Loc Loc) : Loc
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNullableValue(this);
    }
}
