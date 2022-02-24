using Citron.CompileTime;

namespace Citron.Analysis
{
    public interface IFDeclSymbolNode
    {
        FDeclSymbolOuter GetOuterDeclNode();
        DeclSymbolNodeName GetNodeName();
    }

    public static class IFDeclSymbolNodeExtensions
    {
        public static DeclSymbolId GetDeclSymbolId(this IFDeclSymbolNode node)
        {
            var outer = node.GetOuterDeclNode();
            var outerId = outer.GetDeclSymbolId();
            var nodeName = node.GetNodeName();
            return outerId.Child(nodeName.Name, nodeName.TypeParamCount, nodeName.ParamIds);
        }
    }
}