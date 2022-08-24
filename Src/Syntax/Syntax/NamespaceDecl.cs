using Citron.Collections;
using Pretune;

namespace Citron.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class NamespaceDecl : ISyntaxNode
    {
        public ImmutableArray<string> Names { get; } // dot seperated names, NS1.NS2
        public ImmutableArray<NamespaceElement> Elements { get; }
    }
    
}