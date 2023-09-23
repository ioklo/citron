using Citron.Infra;

namespace Citron.Symbol
{
    public interface IFuncDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<IFuncDeclSymbol>, ISerializable
    {
        void AddLambda(LambdaDeclSymbol declSymbol);

        int GetParameterCount();
        FuncParameter GetParameter(int index);

        FuncReturn? GetReturn();

        bool IsLastParameterVariadic();
    }
}