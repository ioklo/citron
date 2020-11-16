using Gum.CompileTime;
using System.Collections.Immutable;

using S = Gum.Syntax;

namespace Gum.IR0
{
    public class SyntaxNodeModuleItemService
    {
        private ImmutableDictionary<S.ISyntaxNode, ItemPath> typePathsByNode;
        private ImmutableDictionary<S.ISyntaxNode, ItemPath> funcPathsByNode;

        public SyntaxNodeModuleItemService(
            ImmutableDictionary<S.ISyntaxNode, ItemPath> typePathsByNode, 
            ImmutableDictionary<S.ISyntaxNode, ItemPath> funcPathsByNode)
        {
            this.typePathsByNode = typePathsByNode;
            this.funcPathsByNode = funcPathsByNode;
        }

        public ItemPath GetTypePath(S.ISyntaxNode node)
        {
            return typePathsByNode[node];
        }

        public ItemPath GetFuncPath(S.ISyntaxNode node)
        {
            return funcPathsByNode[node];
        }

    }
}