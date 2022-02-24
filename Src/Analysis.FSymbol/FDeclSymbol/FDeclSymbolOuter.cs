using Citron.CompileTime;

namespace Citron.Analysis
{
    public abstract record FDeclSymbolOuter
    {
        public abstract DeclSymbolId GetDeclSymbolId();

        public record Decl(IDeclSymbolNode DeclSymbol) : FDeclSymbolOuter
        {
            public override DeclSymbolId GetDeclSymbolId()
            {
                return DeclSymbol.GetDeclSymbolId();
            }
        }

        public record FDecl(IFDeclSymbolNode FDeclSymbol) : FDeclSymbolOuter
        {
            public override DeclSymbolId GetDeclSymbolId()
            {
                return FDeclSymbol.GetDeclSymbolId();
            }
        }
    }
}