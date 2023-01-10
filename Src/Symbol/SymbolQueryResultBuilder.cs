using Citron.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Symbol
{
    // utility class for {ClassDeclSymbol, StructDeclSymbol} QueryMember_Type
    public class SymbolQueryResultBuilder : ITypeDeclSymbolVisitor
    {
        ISymbolNode outer;
        SymbolFactory symbolFactory;
        SymbolQueryResult? result;

        SymbolQueryResultBuilder(ISymbolNode outer, SymbolFactory symbolFactory)
        {
            this.outer = outer;
            this.symbolFactory = symbolFactory;
            this.result = null;
        }

        public static SymbolQueryResult Build(ITypeDeclSymbol decl, ISymbolNode outer, SymbolFactory symbolFactory)
        {
            var builder = new SymbolQueryResultBuilder(outer, symbolFactory);
            decl.Accept(builder);

            Debug.Assert(builder.result != null);
            return builder.result;
        }

        public void VisitClass(ClassDeclSymbol classDecl)
        {
            result = new SymbolQueryResult.Class(typeArgs => symbolFactory.MakeClass(outer, classDecl, typeArgs));
        }

        public void VisitStruct(StructDeclSymbol structDecl)
        {
            result = new SymbolQueryResult.Struct(typeArgs => symbolFactory.MakeStruct(outer, structDecl, typeArgs));
        }

        public void VisitEnum(EnumDeclSymbol enumDecl)
        {
            result = new SymbolQueryResult.Enum(typeArgs => symbolFactory.MakeEnum(outer, enumDecl, typeArgs));
        }

        public void VisitEnumElem(EnumElemDeclSymbol enumElemDecl)
        {
            var outerEnum = outer as EnumSymbol;
            Debug.Assert(outerEnum != null);

            result = new SymbolQueryResult.EnumElem(symbolFactory.MakeEnumElem(outerEnum, enumElemDecl));
        }

        public void VisitInterface(InterfaceDeclSymbol interfaceDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitLambda(LambdaDeclSymbol declSymbol)
        {
            var outerFunc = outer as IFuncSymbol;
            Debug.Assert(outerFunc != null);

            result = new SymbolQueryResult.Lambda(symbolFactory.MakeLambda(outerFunc, declSymbol));
        }

        public void VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol)
        {
            var outerLambda = outer as LambdaSymbol;
            Debug.Assert(outerLambda != null);

            result = new SymbolQueryResult.LambdaMemberVar(symbolFactory.MakeLambdaMemberVar(outerLambda, declSymbol));
        }
    }
}
