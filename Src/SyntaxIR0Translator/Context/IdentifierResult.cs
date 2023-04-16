using Citron.Collections;
using Citron.Symbol;

namespace Citron.Analysis;

abstract record class IdentifierResult
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IIdentifierResultVisitor<TResult>;

    public record class Namespace(NamespaceSymbol Symbol) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
    }

    public record class LocalVar(IType Type, Name Name) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
    }

    public record class TypeVar(TypeVarType Type) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);
    }

    public record class LambdaMemberVar(LambdaMemberVarSymbol MemberVar) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
    }

    public record class ThisVar(IType Type) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
    }

    // TypeArgsForMatch: partial
    public record class GlobalFuncs(ImmutableArray<DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>> Infos, ImmutableArray<IType> ParitalTypeArgs) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitGlobalFuncs(this);
    }

    #region Class
    public record class Class(ClassSymbol Symbol) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
    }

    public record ThisClassMemberFuncs(
        ImmutableArray<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>> Infos,
        ImmutableArray<IType> ParitalTypeArgs) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThisClassMemberFuncs(this);
    }

    public record ThisClassMemberVar(ClassMemberVarSymbol Symbol) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThisClassMemberVar(this);
    }

    #endregion

    #region Struct
    public record class Struct(StructSymbol Symbol) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
    }

    public record class ThisStructMemberFuncs(
        ImmutableArray<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>> Infos,
        ImmutableArray<IType> ParitalTypeArgs) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThisStructMemberFuncs(this);
    }

    public record class ThisStructMemberVar(StructMemberVarSymbol Symbol) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThisStructMemberVar(this);
    }

    #endregion

    #region Enum

    public record class Enum(EnumSymbol Symbol) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
    }

    public record class EnumElem(EnumElemSymbol Symbol) : IdentifierResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
    }

    #endregion Enum
}
