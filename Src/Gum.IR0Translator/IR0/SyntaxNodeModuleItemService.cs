using Gum.CompileTime;
using System.Collections.Immutable;

using S = Gum.Syntax;

namespace Gum.IR0
{
    public class SyntaxNodeModuleItemService
    {
        private ImmutableDictionary<S.ISyntaxNode, ModuleItemId> typeIdsByNode;
        private ImmutableDictionary<S.ISyntaxNode, ModuleItemId> funcIdsByNode;

        public SyntaxNodeModuleItemService(
            ImmutableDictionary<S.ISyntaxNode, ModuleItemId> typeIdsByNode, 
            ImmutableDictionary<S.ISyntaxNode, ModuleItemId> funcIdsByNode)
        {
            this.typeIdsByNode = typeIdsByNode;
            this.funcIdsByNode = funcIdsByNode;
        }

        public ModuleItemId GetTypeId(S.ISyntaxNode node)
        {
            return typeIdsByNode[node];
        }

        public ModuleItemId GetFuncId(S.ISyntaxNode node)
        {
            return funcIdsByNode[node];
        }

    }
}