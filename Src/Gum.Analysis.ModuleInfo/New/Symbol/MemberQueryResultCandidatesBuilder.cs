using Gum.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Gum.Analysis
{
    // utility class for {ClassDeclSymbol, StructDeclSymbol} GetMember_Type
    [AutoConstructor]
    partial class MemberQueryResultCandidatesBuilder : ITypeDeclSymbolNodeVisitor
    {
        ISymbolNode outer;
        SymbolFactory symbolFactory;
        Candidates<MemberQueryResult.Valid> candidates;

        public void VisitClassDecl(ClassDeclSymbol classDecl)
        {
            candidates.Add(new MemberQueryResult.Class(typeArgs => symbolFactory.MakeClass(outer, classDecl, typeArgs)));
        }

        public void VisitStructDecl(StructDeclSymbol structDecl)
        {
            candidates.Add(new MemberQueryResult.Struct(typeArgs => symbolFactory.MakeStruct(outer, structDecl, typeArgs)));
        }

        public void VisitEnumDecl(EnumDeclSymbol enumDecl)
        {
            candidates.Add(new MemberQueryResult.Enum(typeArgs => symbolFactory.MakeEnum(outer, enumDecl, typeArgs)));
        }

        public void VisitEnumElemDecl(EnumElemDeclSymbol enumElemDecl)
        {
            var outerEnum = outer as EnumSymbol;
            Debug.Assert(outerEnum != null);

            candidates.Add(new MemberQueryResult.EnumElem(symbolFactory.MakeEnumElem(outerEnum, enumElemDecl)));
        }
    }
}
