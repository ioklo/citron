using Citron.Infra;

namespace Citron.Symbol
{
    // Interface는 built-in이 있고, Symbol이 있다
    public interface IInterfaceType : IType, ICyclicEqualityComparableClass<IInterfaceType>
    {
        bool IsLocal();
    }
}
