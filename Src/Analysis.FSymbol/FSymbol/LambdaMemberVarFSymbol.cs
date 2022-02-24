using Pretune;
using Citron.Collections;
using Citron.CompileTime;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class LambdaMemberVarFSymbol : IFSymbolNode
    {
        FSymbolFactory factory;
        LambdaFSymbol outer;
        LambdaMemberVarFDeclSymbol decl;

        public IFSymbolNode Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeLambdaMemberVar(appliedOuter, decl);
        }

        public IFDeclSymbolNode GetDeclFSymbolNode()
        {
            return decl;
        }

        public FSymbolOuter GetOuter()
        {
            return new FSymbolOuter.FSymbol(outer);
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
