using Citron.Collections;
using Citron.Symbol;
using R = Citron.IR0;

namespace Citron.Analysis;

// IntermediateExp
// Resolve하는데 필요한 중간과정
// Namespace는 MemberExp의 Parent의 결과로 만들어질 수 있으니 존재한다
abstract record IntermediateExp
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IIntermediateExpVisitor<TResult>;

    public record class Namespace(NamespaceSymbol Symbol) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
    }
    public record class GlobalFuncs(ImmutableArray<DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>> Infos, ImmutableArray<IType> ParitalTypeArgs) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitGlobalFuncs(this);
    }
    public record class TypeVar(TypeVarType Type) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);
    }
    public record class Class(ClassSymbol Symbol) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
    }

    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null
    public record ClassMemberFuncs(
        ImmutableArray<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>> Infos,
        ImmutableArray<IType> ParitalTypeArgs,
        bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberFuncs(this);
    }

    public record class Struct(StructSymbol Symbol) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
    }

    public record class StructMemberFuncs(
        ImmutableArray<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>> Infos,
        ImmutableArray<IType> ParitalTypeArgs,
        bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberFuncs(this);
    }

    public record class Enum(EnumSymbol Symbol) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
    }

    public record class EnumElem(EnumElemSymbol Symbol) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
    }

    // exp로 사용할 수 있는
    public record class ThisVar(IType Type) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
    }
    public record class LocalVar(IType Type, Name Name) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
    }
    public record class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
    }

    public record class ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
    }
    public record class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
    }

    public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol, ResolvedExp Instance) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
    }

    public record class ListIndexer(R.Loc Instance, R.Exp Index, IType ItemType) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
    }

    // 기타의 경우
    public record class IR0Exp(R.Exp Exp) : IntermediateExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIR0Exp(this);
    }
}
