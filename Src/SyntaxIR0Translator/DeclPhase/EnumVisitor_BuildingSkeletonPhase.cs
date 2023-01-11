using System;

using Citron.Symbol;

using S = Citron.Syntax;
using Citron.Collections;

namespace Citron.Analysis
{
    struct EnumVisitor_BuildingSkeletonPhase<TDeclSymbolNode>
        where TDeclSymbolNode : IDeclSymbolNode, ITypeDeclContainable
    {
        BuildingSkeletonPhaseContext context;
        TDeclSymbolNode node;
        Func<S.AccessModifier?, Accessor> accessorMaker;

        public EnumVisitor_BuildingSkeletonPhase(
            BuildingSkeletonPhaseContext context,
            TDeclSymbolNode node,
            Func<S.AccessModifier?, Accessor> accessorMaker)
        {
            this.context = context;
            this.node = node;
            this.accessorMaker = accessorMaker;
        }

        public void VisitEnumDecl(S.EnumDecl syntax)
        {
            var accessor = accessorMaker.Invoke(syntax.AccessModifier);
            var typeParams = BuilderMisc.VisitTypeParams(syntax.TypeParams);

            var symbol = new EnumDeclSymbol(node, accessor, new Name.Normal(syntax.Name), typeParams);
            node.AddType(symbol);

            var elemsBuilder = ImmutableArray.CreateBuilder<EnumElemDeclSymbol>(syntax.Elems.Length);

            foreach (var enumElemSyntax in syntax.Elems)
                VisitEnumElemDecl(enumElemSyntax, symbol, elemsBuilder);

            symbol.InitElems(elemsBuilder.MoveToImmutable());
        }

        void VisitEnumElemDecl(S.EnumElemDecl syntax, EnumDeclSymbol outer, ImmutableArray<EnumElemDeclSymbol>.Builder declsBuilder)
        {
            var enumElem = new EnumElemDeclSymbol(outer, new Name.Normal(syntax.Name));
            declsBuilder.Add(enumElem);

            var memberVarsBuilder = ImmutableArray.CreateBuilder<EnumElemMemberVarDeclSymbol>(syntax.MemberVars.Length);

            foreach (var memberVar in syntax.MemberVars)
                VisitEnumElemMemberVarDecl(memberVar, enumElem, memberVarsBuilder);

            enumElem.InitMemberVars(memberVarsBuilder.MoveToImmutable());
        }

        void VisitEnumElemMemberVarDecl(S.EnumElemMemberVarDecl syntax, EnumElemDeclSymbol outer, ImmutableArray<EnumElemMemberVarDeclSymbol>.Builder builder)
        {
            var memberVarDecl = new EnumElemMemberVarDeclSymbol(outer, new Name.Normal(syntax.Name));
            builder.Add(memberVarDecl);

            context.RegisterTaskAfterBuildingAllTypeDeclSymbols(context =>
            {
                var declType = context.MakeType(syntax.Type, outer);
                memberVarDecl.InitDeclType(declType);
            });
        }
    }
}