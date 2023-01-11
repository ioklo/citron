using Pretune;
using Citron.Collections;
using Citron.Infra;

namespace Citron.Symbol
{
    [AutoConstructor]
    public partial class LambdaMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        LambdaSymbol outer;
        LambdaMemberVarDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public LambdaMemberVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeLambdaMemberVar(appliedOuter, decl);
        }

        public IDeclSymbolNode GetDeclFSymbolNode()
        {
            return decl;
        }

        public ISymbolNode GetOuter()
        {
            return outer;
        }

        IType ISymbolNode.GetTypeArg(int i)
        {
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public IType GetDeclType()
        {
            var declType = decl.GetDeclType();
            return declType.Apply(GetTypeEnv());
        }

        public Name GetName()
        {
            return decl.GetName();
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }
        

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
        {
            throw new System.NotImplementedException();
        }

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitLambdaMemberVarSymbol(this);
        }
    }
}
