using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class GlobalVarSymbol : ITopLevelSymbolNode, ICyclicEqualityComparableClass<GlobalVarSymbol>
    {
        SymbolFactory factory;
        ITopLevelSymbolNode outer;
        GlobalVarDeclSymbol declSymbol;

        public GlobalVarSymbol(SymbolFactory factory, ITopLevelSymbolNode outer, GlobalVarDeclSymbol declSymbol)
        {
            this.factory = factory;
            this.outer = outer;
            this.declSymbol = declSymbol;
        }

        GlobalVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeGlobalVar(appliedOuter, declSymbol);
        }


        ITopLevelSymbolNode ITopLevelSymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is GlobalVarSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITopLevelSymbolNode>.CyclicEquals(ITopLevelSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is GlobalVarSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<GlobalVarSymbol>.CyclicEquals(GlobalVarSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(GlobalVarSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(declSymbol, other.declSymbol))
                return false;

            return true;
        }

        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode()
        {
            return declSymbol;
        }

        ISymbolNode? ISymbolNode.GetOuter()
        {
            return outer;
        }

        IType ISymbolNode.GetTypeArg(int i)
        {
            throw new RuntimeFatalException();
        }

        TypeEnv ISymbolNode.GetTypeEnv()
        {
            return outer?.GetTypeEnv() ?? TypeEnv.Empty;
        }

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitGlobalVar(this);
        }
    }
}