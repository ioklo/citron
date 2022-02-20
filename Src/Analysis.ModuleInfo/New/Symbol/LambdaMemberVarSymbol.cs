using Pretune;
using Citron.Collections;
using Citron.CompileTime;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class LambdaMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        LambdaSymbol outer;
        LambdaMemberVarDeclSymbol decl;

        public ISymbolNode Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeLambdaMemberVar(appliedOuter, decl);
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public ITypeSymbol GetDeclType()
        {
            var declType = decl.GetDeclType();
            return declType.Apply(GetTypeEnv());
        }

        public Name GetName()
        {
            return decl.GetName();
        }
    }
}
