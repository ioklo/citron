using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public record DeclSymbolNodeName(M.Name Name, int TypeParamCount, ImmutableArray<FuncParamId> ParamIds);

    // DeclSymbol 간에 참조할 수 있는 인터페이스 확장에는 닫혀있다 
    public interface IDeclSymbolNode
    {
        IDeclSymbolNode? GetOuterDeclNode(); // 최상위 계층에는 Outer가 없다
        DeclSymbolNodeName GetNodeName();
        IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds);

        void Apply(IDeclSymbolNodeVisitor visitor);
    }

    public static class IDeclSymbolNodeExtensions
    {
        public static int GetTypeParamCount(this IDeclSymbolNode node)
        {
            var nodeName = node.GetNodeName();
            return nodeName.TypeParamCount;
        }

        public static bool IsDescendantOf(this IDeclSymbolNode thisDecl, IDeclSymbolNode containerDecl)
        {
            var thisOuterDecl = thisDecl.GetOuterDeclNode();
            if (thisOuterDecl == null) return false;

            if (thisOuterDecl.Equals(containerDecl)) return true;
            return thisOuterDecl.IsDescendantOf(containerDecl);
        }

        public static int GetTotalTypeParamCount(this IDeclSymbolNode node)
        {
            var outerNode = node.GetOuterDeclNode();
            if (outerNode == null) return node.GetTypeParamCount();

            return outerNode.GetTotalTypeParamCount() + node.GetTypeParamCount();
        }

        // tree traversal, 못찾으면 null, declPath가 relative path여도 가능하다
        public static IDeclSymbolNode? GetDeclSymbol(this IDeclSymbolNode node, DeclSymbolPath declPath)
        {
            if (declPath.Outer != null)
            {
                var outerDeclSymbol = node.GetDeclSymbol(declPath.Outer);
                if (outerDeclSymbol == null) return null;

                return outerDeclSymbol.GetMemberDeclNode(declPath.Name, declPath.TypeParamCount, declPath.ParamIds);
            }
            else
            {
                return node.GetMemberDeclNode(declPath.Name, declPath.TypeParamCount, declPath.ParamIds);
            }
        }
    }
}