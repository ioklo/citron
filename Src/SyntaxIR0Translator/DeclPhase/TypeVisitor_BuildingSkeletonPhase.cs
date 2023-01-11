using System;

using Citron.Infra;

using Citron.Symbol;

using S = Citron.Syntax;
using Pretune;
using Citron.Collections;
using System.Diagnostics;

namespace Citron.Analysis
{
    struct TypeVisitor_BuildingSkeletonPhase
    {  
        public static void VisitTypeDecl<TDeclSymbolNode>(
            S.TypeDecl typeDecl, BuildingSkeletonPhaseContext context,
            TDeclSymbolNode node,
            Func<S.AccessModifier?, Accessor> accessorMaker)
            where TDeclSymbolNode : IDeclSymbolNode, ITypeDeclContainable
        {
            switch (typeDecl)
            {
                case S.EnumDecl enumDecl:
                    {
                        var enumVisitor = new EnumVisitor_BuildingSkeletonPhase<TDeclSymbolNode>(context, node, accessorMaker);
                        enumVisitor.VisitEnumDecl(enumDecl); 
                        break;
                    }

                case S.StructDecl structDecl:
                    {
                        var structVisitor = new StructVisitor_BuildingSkeletonPhase<TDeclSymbolNode>(context, node, accessorMaker);
                        structVisitor.VisitStructDecl(structDecl); 
                        break;
                    }

                case S.ClassDecl classDecl:
                    {
                        var classVisitor = new ClassVisitor_BuildingSkeletonPhase<TDeclSymbolNode>(context, node, accessorMaker);
                        classVisitor.VisitClassDecl(classDecl); 
                        break;
                    }

                default: throw new UnreachableCodeException();
            }
        }
    }
}