using System;
using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using static Citron.Symbol.Name;
using static Citron.Symbol.SymbolQueryResult;
using R = Citron.IR0;

namespace Citron.Analysis;

// Sytnax.Exp를 바로 번역했을 경우 생기는 날것의 결과 
abstract record class ExpResult
{
    // 카테고리
    public record class Valid : ExpResult;
    internal record class NotFound : ExpResult;
    internal record class Error(string Text) : ExpResult;
    

    #region TopLevel
    public record class Namespace : Valid;
    public record class GlobalVar(bool IsRef, IType Type, Name VarName) : Valid;

    // TypeArgsForMatch: partial
    public record class GlobalFuncs(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch) : Valid;
    #endregion

    #region Body
    public record class TypeVar(TypeVarType Type) : Valid;
    public record class ThisVar(IType Type) : Valid;
    public record class LocalVar(bool IsRef, IType Type, Name Name) : Valid;
    public record class LambdaMemberVar(Func<LambdaMemberVarSymbol> SymbolConstructor) : Valid;
    #endregion

    #region Class
    public record class Class(ClassSymbol Symbol) : Valid;

    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null
    public record ClassMemberFuncs(
        ImmutableArray<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>> Infos, 
        ImmutableArray<IType> ParitalTypeArgs, 
        bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;
    public record ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;

    #endregion

    #region Struct
    public record class Struct(StructSymbol Symbol) : Valid;
    public record class StructMemberFuncs(
        ImmutableArray<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>> Infos,
        ImmutableArray<IType> ParitalTypeArgs, 
        bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;
    public record class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;
    #endregion

    #region Enum
    public record class Enum(EnumSymbol Symbol) : Valid;
    public record class EnumElem(EnumElemSymbol Symbol) : Valid;
    #endregion

    // 변역되는 경우
    // public record class Exp(R.Exp Result) : ExpResult;
    // public record class Loc(R.Loc Result, ITypeSymbol TypeSymbol) : ExpResult;
}

static class ExpResults
{
    public readonly static ExpResult NotFound = new ExpResult.NotFound();

    // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
    public readonly static ExpResult MultipleCandiates = new ExpResult.Error("MultipleCandidates");
    public readonly static ExpResult VarWithTypeArg = new ExpResult.Error("VarWithTypeArg");
    public readonly static ExpResult CantGetStaticMemberThroughInstance = new ExpResult.Error("CantGetStaticMemberThroughInstance");
    public readonly static ExpResult CantGetTypeMemberThroughInstance = new ExpResult.Error("CantGetTypeMemberThroughInstance");
    public readonly static ExpResult CantGetInstanceMemberThroughType = new ExpResult.Error("CantGetInstanceMemberThroughType");
    public readonly static ExpResult FuncCantHaveMember = new ExpResult.Error("FuncCantHaveMember");
    public readonly static ExpResult CantGetThis = new ExpResult.Error("CantGetThis");

    public static ExpResult ToErrorIdentifierResult(this SymbolQueryResult.Error errorResult)
    {
        switch (errorResult)
        {
            case SymbolQueryResult.Error.MultipleCandidates:
                return ExpResults.MultipleCandiates;

            case SymbolQueryResult.Error.VarWithTypeArg:
                return ExpResults.VarWithTypeArg;
        }

        throw new UnreachableCodeException();
    }

    struct ExpResultMaker : ISymbolNodeVisitor
    {
        ExpResult result;

        void ISymbolNodeVisitor.VisitClass(ClassSymbol symbol)
        {
            result = new ExpResult.Class(symbol);
        }

        void ISymbolNodeVisitor.VisitClassConstructor(ClassConstructorSymbol symbol)
        {
            throw new RuntimeFatalException();
        }

        void ISymbolNodeVisitor.VisitClassMemberFunc(ClassMemberFuncSymbol symbol)
        {
            result = new ExpResult.ClassMemberFuncs(Arr(new DeclAndConstructor())
        }

        void ISymbolNodeVisitor.VisitClassMemberVar(ClassMemberVarSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitEnum(EnumSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitEnumElem(EnumElemSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitEnumElemMemberVar(EnumElemMemberVarSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitGlobalFunc(GlobalFuncSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitGlobalVar(GlobalVarSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitInterface(InterfaceSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitLambda(LambdaSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitLambdaMemberVarSymbol(LambdaMemberVarSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitModule(ModuleSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitNamespace(NamespaceSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitStruct(StructSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitStructConstructor(StructConstructorSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitStructMemberFunc(StructMemberFuncSymbol symbol)
        {
            throw new NotImplementedException();
        }

        void ISymbolNodeVisitor.VisitStructMemberVar(StructMemberVarSymbol symbol)
        {
            throw new NotImplementedException();
        }
    }

    public static ExpResult MakeExpResult(this ISymbolNode node)
    { 

    }
}
