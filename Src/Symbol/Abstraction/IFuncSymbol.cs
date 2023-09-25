using Citron.Infra;

namespace Citron.Symbol
{
    public interface IFuncSymbol : ISymbolNode, ICyclicEqualityComparableClass<IFuncSymbol>
    {
        new IFuncSymbol Apply(TypeEnv typeEnv);
        new IFuncDeclSymbol GetDeclSymbolNode();

        ITypeSymbol? GetOuterType();

        int GetParameterCount();
        FuncParameter GetParameter(int index);

        FuncReturn? GetReturn();

        new TResult Accept<TFuncSymbolVisitor, TResult>(ref TFuncSymbolVisitor visitor)
            where TFuncSymbolVisitor : struct, IFuncSymbolVisitor<TResult>;
    }

    public interface IFuncSymbolVisitor<out TResult>
    {
        TResult VisitClassConstructor(ClassConstructorSymbol symbol);
        TResult VisitClassMemberFunc(ClassMemberFuncSymbol symbol);
        
        TResult VisitStructConstructor(StructConstructorSymbol symbol);
        TResult VisitStructMemberFunc(StructMemberFuncSymbol symbol);

        TResult VisitLambda(LambdaSymbol symbol);
        TResult VisitGlobalFunc(GlobalFuncSymbol symbol);
    }
}