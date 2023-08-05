using Citron.Symbol;
using R = Citron.IR0;

namespace Citron.Analysis;

interface IIntermediateRefExpVisitor<TResult>
{
    TResult VisitNamespace(IntermediateRefExp.Namespace imRefExp);
    TResult VisitTypeVar(IntermediateRefExp.TypeVar imRefExp);
    TResult VisitClass(IntermediateRefExp.Class imRefExp);
    TResult VisitStruct(IntermediateRefExp.Struct imRefExp);
    TResult VisitEnum(IntermediateRefExp.Enum imRefExp);
    TResult VisitThis(IntermediateRefExp.ThisVar imRefExp);
    TResult VisitLocalVar(IntermediateRefExp.LocalVar imRefExp);
    TResult VisitLambdaMemberVar(IntermediateRefExp.LambdaMemberVar imRefExp);
    TResult VisitClassMemberVar(IntermediateRefExp.ClassMemberVar imRefExp);
    TResult VisitStructMemberVar(IntermediateRefExp.StructMemberVar imRefExp);
    TResult VisitEnumElemMemberVar(IntermediateRefExp.EnumElemMemberVar imRefExp);
    TResult VisitListIndexer(IntermediateRefExp.ListIndexer imRefExp);
    TResult VisitLocalDeref(IntermediateRefExp.LocalDeref imRefExp);
    TResult VisitBoxDeref(IntermediateRefExp.BoxDeref imRefExp);
    TResult VisitIR0Exp(IntermediateRefExp.IR0Exp imRefExp);
}

// &를 붙였을때의 행동이 좀 다르다
abstract record IntermediateRefExp
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IIntermediateRefExpVisitor<TResult>;

    public record class Namespace(NamespaceSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
    }
    
    public record class TypeVar(TypeVarType Type) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);
    }

    public record class Class(ClassSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
    }
    
    public record class Struct(StructSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
    }
    
    public record class Enum(EnumSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
    }
    
    // exp로 사용할 수 있는
    public record class ThisVar(IType Type) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
    }

    public record class LocalVar(IType Type, Name Name) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
    }

    public record class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
    }

    public record class ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
    }

    public record class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
    }

    public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol, ResolvedExp Instance) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
    }

    public record class ListIndexer(ResolvedExp Instance, R.Exp Index, IType ItemType) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
    }

    public record class LocalDeref(ResolvedExp Target) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalDeref(this);
    }

    public record class BoxDeref(ResolvedExp Target) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxDeref(this);
    }

    // 기타의 경우
    public record class IR0Exp(R.Exp Exp) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIR0Exp(this);
    }
}
