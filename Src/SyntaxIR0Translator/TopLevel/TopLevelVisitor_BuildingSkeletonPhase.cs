using System;
using Citron.Collections;
using Citron.Symbol;

using S = Citron.Syntax;

namespace Citron.Analysis
{
    struct TopLevelVisitor_BuildingSkeletonPhase<TDeclSymbolNode>
        where TDeclSymbolNode : ITopLevelDeclSymbolNode, ITopLevelDeclContainable
    {
        BuildingSkeletonPhaseContext context;
        TDeclSymbolNode node;

        public TopLevelVisitor_BuildingSkeletonPhase(BuildingSkeletonPhaseContext context, TDeclSymbolNode node)
        {
            this.context = context;
            this.node = node;
        }

        void VisitNamespaceElements(ImmutableArray<S.NamespaceElement> elems)
        {
            foreach (var elem in elems)
            {
                switch (elem)
                {
                    case S.NamespaceDeclNamespaceElement namespaceElem:
                        VisitNamespaceDecl(namespaceElem.NamespaceDecl);
                        break;

                    case S.GlobalFuncDeclNamespaceElement funcElem:
                        VisitGlobalFuncDecl(funcElem.FuncDecl);
                        break;

                    case S.TypeDeclNamespaceElement typeElem:
                        VisitTypeDecl(typeElem.TypeDecl);
                        break;
                }
            }
        }

        public void VisitGlobalFuncDecl(S.GlobalFuncDecl syntax)
        {
            // 이름을 만드려면 인자의 타입이 확정되어야 되서, 다음 단계에서 해야 한다
            var node = this.node;
            context.AddBuildingMemberDeclPhaseTask(context =>
            {
                var accessor = BuilderMisc.MakeGlobalMemberAccessor(syntax.AccessModifier);
                var typeParams = BuilderMisc.VisitTypeParams(syntax.TypeParams);

                // <T, U>
                var declSymbol = new GlobalFuncDeclSymbol(
                    node,
                    accessor,
                    new Name.Normal(syntax.Name),
                    typeParams);

                var (funcRet, funcParams) = context.MakeFuncReturnAndParams(declSymbol, syntax.RetType, syntax.Parameters);
                declSymbol.InitFuncReturnAndParams(funcRet, funcParams);

                // NOTICE: MUST add after initFuncReturnAndParams
                node.AddFunc(declSymbol);

                context.AddBuildingBodyPhaseTask(context =>
                {
                    return TopLevelVisitor_BuildingBodyPhase.VisitGlobalFuncDecl(syntax.Body, context, declSymbol, bSeqFunc: syntax.IsSequence);
                });
            });
        }

        // 중첩이름을 처리해야 한다 NS1.NS2.NS3
        public void VisitNamespaceDecl(S.NamespaceDecl decl)
        {
            var context = this.context;

            void VisitElem<TNode>(int index, TNode curNode)
                where TNode : ITopLevelDeclSymbolNode, ITopLevelDeclContainable
            {
                var namespaceName = new Name.Normal(decl.Names[index]);
                var declNode = curNode.GetMemberDeclNode(new DeclSymbolNodeName(namespaceName, 0, default));

                NamespaceDeclSymbol? namespaceDecl = null;

                if (declNode == null)
                {
                    namespaceDecl = new NamespaceDeclSymbol(curNode, namespaceName);
                    curNode.AddNamespace(namespaceDecl);
                }
                else if (declNode is NamespaceDeclSymbol namespaceDeclNode)
                {
                    namespaceDecl = namespaceDeclNode;
                }
                else
                {
                    throw new NotImplementedException(); // 에러 처리
                }

                if (index < decl.Names.Length - 1)
                {
                    VisitElem(index + 1, namespaceDecl);
                }
                else
                {
                    var childVisitor = new TopLevelVisitor_BuildingSkeletonPhase<NamespaceDeclSymbol>(context, namespaceDecl);
                    childVisitor.VisitNamespaceElements(decl.Elements);
                }
            }

            // 리턴은 가장 최상위 네임스페이스(NS1)
            // 가장 마지막 네임스페이스 (NS3)를 얻어서 하위 처리를 해야한다
            VisitElem(0, node);
        }

        public void VisitTypeDecl(S.TypeDecl decl)
        {
            TypeVisitor_BuildingSkeletonPhase.VisitTypeDecl(decl, context, node, BuilderMisc.MakeGlobalMemberAccessor);
        }
    }
}