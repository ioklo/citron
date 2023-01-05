using Citron.Infra;
using Citron.Symbol;

namespace Citron.Symbol
{
    // module, namespace
    public interface ITopLevelDeclSymbolNode : IDeclSymbolNode, ICyclicEqualityComparableClass<ITopLevelDeclSymbolNode>
    {   
    }
    
    public interface ITopLevelDeclContainable : ITypeDeclContainable
    {
        void AddNamespace(NamespaceDeclSymbol declSymbol);
        void AddFunc(GlobalFuncDeclSymbol funcDeclSymbol);
    }
}