using Citron.Collections;
using Citron.Infra;
using System;
using M = Citron.CompileTime;
using static Citron.CompileTime.DeclSymbolIdExtensions;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public record DeclSymbolNodeName(M.Name Name, int TypeParamCount, ImmutableArray<M.FuncParamId> ParamIds);

    // DeclSymbol 간에 참조할 수 있는 인터페이스 확장에는 닫혀있다 
    public interface IDeclSymbolNode
    {
        M.AccessModifier GetAccessModifier();
        IDeclSymbolNode? GetOuterDeclNode(); // 최상위 계층에는 Outer가 없다
        DeclSymbolNodeName GetNodeName();
        IEnumerable<IDeclSymbolNode> GetMemberDeclNodes();

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

        public static bool CanAccess(this IDeclSymbolNode user, IDeclSymbolNode target)
        {
            var accessModifier = target.GetAccessModifier();
            var targetOuter = target.GetOuterDeclNode();
            if (targetOuter == null)
                return false;
            
            switch (accessModifier)
            {
                case M.AccessModifier.Public: return true;
                case M.AccessModifier.Protected: throw new NotImplementedException();
                case M.AccessModifier.Private:
                    {
                        // 같은 경우는 허용
                        if (user.Equals(targetOuter))
                            return true;

                        // base클래스가 아니라 container에 속하는지를 본다
                        return user.IsDescendantOf(targetOuter);
                    }

                default: throw new UnreachableCodeException();
            }
        }

        public static int GetTotalTypeParamCount(this IDeclSymbolNode node)
        {
            var outerNode = node.GetOuterDeclNode();
            if (outerNode == null) return node.GetTypeParamCount();

            return outerNode.GetTotalTypeParamCount() + node.GetTypeParamCount();
        }

        public static IDeclSymbolNode? GetMemberDeclNode(this IDeclSymbolNode node, DeclSymbolNodeName name)
        {
            foreach(var memberNode in node.GetMemberDeclNodes())
            {
                if (name.Equals(memberNode.GetNodeName()))
                    return memberNode;
            }

            return null;
        }

        // tree traversal, 못찾으면 null, declPath가 relative path여도 가능하다
        public static IDeclSymbolNode? GetDeclSymbol(this IDeclSymbolNode node, M.DeclSymbolPath declPath)
        {
            var nodeName = new DeclSymbolNodeName(declPath.Name, declPath.TypeParamCount, declPath.ParamIds);

            if (declPath.Outer != null)
            {
                var outerDeclSymbol = node.GetDeclSymbol(declPath.Outer);
                if (outerDeclSymbol == null) return null;

                return outerDeclSymbol.GetMemberDeclNode(nodeName);
            }
            else
            {
                return node.GetMemberDeclNode(nodeName);
            }
        }

        public static M.DeclSymbolId GetDeclSymbolId(this IDeclSymbolNode decl)
        {
            var outerDecl = decl.GetOuterDeclNode();
            if (outerDecl != null)
            {
                var outerDeclId = outerDecl.GetDeclSymbolId();
                var nodeName = decl.GetNodeName();
                return outerDeclId.Child(nodeName.Name, nodeName.TypeParamCount, nodeName.ParamIds);
            }
            else if (decl is ModuleDeclSymbol moduleDecl)
            {
                return new M.DeclSymbolId(moduleDecl.GetName(), null);
            }
            else
            {
                throw new UnreachableCodeException();
            }
        }
    }
}