using System;
using System.Diagnostics;
using Citron.Infra;
using static Citron.Symbol.Name;
using static Citron.Symbol.SymbolQueryResult;

namespace Citron.Analysis;

// Syntax.Exp를 바로 번역했을 경우 생기는 날것의 결과 
//abstract record class ExpResult
//{
//    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
//        where TVisitor : struct, IExpResultVisitor<TResult>;

//    // 카테고리
//    public abstract record class Valid : ExpResult;

//    public record class NotFound : ExpResult
//    {
//        internal NotFound() { }
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNotFound(this);        
//    }

//    public record class Error : ExpResult
//    { 
//        public string Text { get; init; }
//        internal Error(string text) { this.Text = text; }
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitError(this);
//    }

//    #region TopLevel
//    public record class Namespace(NamespaceSymbol Symbol) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
//    }

//    // TypeArgsForMatch: partial
//    public record class GlobalFuncs(ImmutableArray<DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>> Infos, ImmutableArray<IType> ParitalTypeArgs) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitGlobalFuncs(this);
//    }
//    #endregion

//    #region Body
//    public record class TypeVar(TypeVarType Type) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);
//    }

//    public record class ThisVar(IType Type) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
//    }

//    public record class LocalVar(IType Type, Name Name) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
//    }

//    public record class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
//    }
//    #endregion

//    #region Class
//    public record class Class(ClassSymbol Symbol) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
//    }

//    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
//    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

//    // C.F => HasExplicitInstance: true, null
//    // x.F => HasExplicitInstance: true, "x"
//    // F   => HasExplicitInstance: false, null
//    public record ClassMemberFuncs(
//        ImmutableArray<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>> Infos, 
//        ImmutableArray<IType> ParitalTypeArgs, 
//        bool HasExplicitInstance, ExpResult? ExplicitInstance) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberFuncs(this);
//    }

//    public record ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, ExpResult? ExplicitInstance) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
//    }

//    #endregion

//    #region Struct
//    public record class Struct(StructSymbol Symbol) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
//    }

//    public record class StructMemberFuncs(
//        ImmutableArray<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>> Infos,
//        ImmutableArray<IType> ParitalTypeArgs, 
//        bool HasExplicitInstance, ExpResult? ExplicitInstance) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberFuncs(this);
//    }

//    public record class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, ExpResult? ExplicitInstance) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
//    }

//    #endregion

//    #region Enum
//    public record class Enum(EnumSymbol Symbol) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
//    }

//    public record class EnumElem(EnumElemSymbol Symbol) : Valid
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
//    }

//    public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol, ExpResult Instance) : Valid // e.x
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
//    }

//    #endregion

//    public record class ListIndexer(R.Loc Instance, R.Exp Index, IType ItemType) : ExpResult
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
//    }

//    // 기타의 경우
//    public record class IR0Exp(R.Exp Exp) : ExpResult
//    {
//        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIR0Exp(this);
//    }    
//}

//static class ExpResults
//{
//    public readonly static ExpResult NotFound = new ExpResult.NotFound();

//    // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
//    public readonly static ExpResult MultipleCandiates = new ExpResult.Error("MultipleCandidates");
//    public readonly static ExpResult VarWithTypeArg = new ExpResult.Error("VarWithTypeArg");
//    public readonly static ExpResult CantGetStaticMemberThroughInstance = new ExpResult.Error("CantGetStaticMemberThroughInstance");
//    public readonly static ExpResult CantGetTypeMemberThroughInstance = new ExpResult.Error("CantGetTypeMemberThroughInstance");
//    public readonly static ExpResult CantGetInstanceMemberThroughType = new ExpResult.Error("CantGetInstanceMemberThroughType");
//    public readonly static ExpResult FuncCantHaveMember = new ExpResult.Error("FuncCantHaveMember");
//    public readonly static ExpResult CantGetThis = new ExpResult.Error("CantGetThis");

//    public static ExpResult ToErrorIdentifierResult(this SymbolQueryResult.Error errorResult)
//    {
//        switch (errorResult)
//        {
//            case SymbolQueryResult.Error.MultipleCandidates:
//                return ExpResults.MultipleCandiates;

//            case SymbolQueryResult.Error.VarWithTypeArg:
//                return ExpResults.VarWithTypeArg;
//        }

//        throw new UnreachableException();
//    }
//}


