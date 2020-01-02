using Gum.CompileTime;
using Gum.Syntax;
using System.Collections.Immutable;

namespace Gum.StaticAnalysis
{
    public class SyntaxNodeModuleItemService
    {
        private ImmutableDictionary<ISyntaxNode, ModuleItemId> typeIdsByNode;
        private ImmutableDictionary<ISyntaxNode, ModuleItemId> funcIdsByNode;

        public SyntaxNodeModuleItemService(
            ImmutableDictionary<ISyntaxNode, ModuleItemId> typeIdsByNode, 
            ImmutableDictionary<ISyntaxNode, ModuleItemId> funcIdsByNode)
        {
            this.typeIdsByNode = typeIdsByNode;
            this.funcIdsByNode = funcIdsByNode;
        }

        public ModuleItemId GetTypeId(ISyntaxNode node)
        {
            return typeIdsByNode[node];
        }

        public ModuleItemId GetFuncId(ISyntaxNode node)
        {
            return funcIdsByNode[node];
        }

    }
}