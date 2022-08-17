using Pretune;
using Citron.Collections;
using Citron.Module;
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

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
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

        public IDeclSymbolNode? GetDeclSymbolNode()
        {
            return decl;
        }
    }
}
