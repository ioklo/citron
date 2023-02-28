using System;

using Citron.Infra;
using Citron.Symbol;

using S = Citron.Syntax;
using System.Diagnostics;
using System.Collections.Generic;

namespace Citron.Analysis
{
    struct StructVisitor_BuildingSkeletonPhase<TDeclSymbolNode>
        where TDeclSymbolNode : IDeclSymbolNode, ITypeDeclContainable
    {
        BuildingSkeletonPhaseContext context;
        TDeclSymbolNode node;
        Func<S.AccessModifier?, Accessor> accessorMaker;

        public StructVisitor_BuildingSkeletonPhase(
            BuildingSkeletonPhaseContext context,
            TDeclSymbolNode node,
            Func<S.AccessModifier?, Accessor> accessorMaker)
        {
            this.context = context;
            this.node = node;
            this.accessorMaker = accessorMaker;
        }

        public void VisitStructDecl(S.StructDecl syntax)
        {
            var accessor = accessorMaker.Invoke(syntax.AccessModifier);
            var typeParams = BuilderMisc.VisitTypeParams(syntax.TypeParams);

            var declSymbol = new StructDeclSymbol(node, accessor, new Name.Normal(syntax.Name), typeParams);
            node.AddType(declSymbol);

            // 할수 있는데까지 다 해본다
            foreach (var memberDecl in syntax.MemberDecls)
            {
                switch (memberDecl)
                {
                    case S.StructMemberTypeDecl typeDecl:
                        TypeVisitor_BuildingSkeletonPhase.VisitTypeDecl(typeDecl.TypeDecl, context, declSymbol, BuilderMisc.MakeStructMemberAccessor);
                        break;
                }
            }

            context.AddBuildingMemberDeclPhaseTask(context2 => {
                // 멤버함수와, 멤버변수, 명시적 Constructor만들기
                var visitor = new StructVisitor_BuildingMemberDeclPhase(declSymbol, context2);
                visitor.VisitStructDecl(syntax);
            });
        }
    }
}