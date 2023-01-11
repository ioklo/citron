using System;
using System.Diagnostics;
using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;

using S = Citron.Syntax;

namespace Citron.Analysis
{
    struct ClassVisitor_BuildingSkeletonPhase<TDeclSymbolNode> 
        where TDeclSymbolNode : IDeclSymbolNode, ITypeDeclContainable
    {
        BuildingSkeletonPhaseContext context;
        TDeclSymbolNode node;
        Func<S.AccessModifier?, Accessor> accessorMaker;

        public ClassVisitor_BuildingSkeletonPhase(BuildingSkeletonPhaseContext context, TDeclSymbolNode node, Func<S.AccessModifier?, Accessor> accessorMaker)
        {
            this.context = context;
            this.node = node;
            this.accessorMaker = accessorMaker;
        }

        public void VisitClassDecl(S.ClassDecl syntax)
        {
            var accessor = accessorMaker.Invoke(syntax.AccessModifier);
            var typeParams = BuilderMisc.VisitTypeParams(syntax.TypeParams);

            var declSymbol = new ClassDeclSymbol(node, accessor, new Name.Normal(syntax.Name), typeParams);
            node.AddType(declSymbol);
            
            foreach (var member in syntax.MemberDecls)
            {
                switch (member)
                {
                    case S.ClassMemberTypeDecl typeMember:
                        TypeVisitor_BuildingSkeletonPhase.VisitTypeDecl(typeMember.TypeDecl, context, declSymbol, BuilderMisc.MakeClassMemberAccessor);
                        break;
                }
            }

            context.RegisterTaskAfterBuildingAllTypeDeclSymbols(context2 =>
            {
                var visitor = new ClassVisitor_BuildingFuncDeclPhase(declSymbol, context2);
                visitor.VisitClassDecl(syntax);
            });
        }
    }
}