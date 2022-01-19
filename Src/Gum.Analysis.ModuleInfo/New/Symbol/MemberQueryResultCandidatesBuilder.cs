using Gum.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Gum.Analysis
{
    // utility class for {ClassDeclSymbol, StructDeclSymbol} QueryMember_Type
    [AutoConstructor]
    partial class MemberQueryResultCandidatesBuilder : ITypeDeclSymbolVisitor
    {
        ISymbolNode outer;
        SymbolFactory symbolFactory;
        Candidates<SymbolQueryResult.Valid> candidates;

        public void VisitClass(ClassDeclSymbol classDecl)
        {
            candidates.Add(new SymbolQueryResult.Class(typeArgs => symbolFactory.MakeClass(outer, classDecl, typeArgs)));
        }

        public void VisitStruct(StructDeclSymbol structDecl)
        {
            candidates.Add(new SymbolQueryResult.Struct(typeArgs => symbolFactory.MakeStruct(outer, structDecl, typeArgs)));
        }

        public void VisitEnum(EnumDeclSymbol enumDecl)
        {
            candidates.Add(new SymbolQueryResult.Enum(typeArgs => symbolFactory.MakeEnum(outer, enumDecl, typeArgs)));
        }

        public void VisitEnumElem(EnumElemDeclSymbol enumElemDecl)
        {
            var outerEnum = outer as EnumSymbol;
            Debug.Assert(outerEnum != null);

            candidates.Add(new SymbolQueryResult.EnumElem(symbolFactory.MakeEnumElem(outerEnum, enumElemDecl)));
        }

        public void VisitInterface(InterfaceDeclSymbol interfaceDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitLambda(LambdaDeclSymbol lambdaDecl)
        {
            var outerFunc = outer as IFuncSymbol;
            Debug.Assert(outerFunc != null);

            candidates.Add(new SymbolQueryResult.Lambda(symbolFactory.MakeLambda(outerFunc, lambdaDecl)));
        }
    }
}
