using Citron.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Symbol
{
    // utility class for {ClassDeclSymbol, StructDeclSymbol} QueryMember_Type
    public struct SymbolQueryResultBuilder : ITypeDeclSymbolVisitor<SymbolQueryResult>
    {
        ISymbolNode outer;
        SymbolFactory symbolFactory;

        SymbolQueryResultBuilder(ISymbolNode outer, SymbolFactory symbolFactory)
        {
            this.outer = outer;
            this.symbolFactory = symbolFactory;
        }

        public static SymbolQueryResult Build(ITypeDeclSymbol decl, ISymbolNode outer, SymbolFactory symbolFactory)
        {
            var builder = new SymbolQueryResultBuilder(outer, symbolFactory);
            return decl.Accept<SymbolQueryResultBuilder, SymbolQueryResult>(ref builder);
        }

        SymbolQueryResult ITypeDeclSymbolVisitor<SymbolQueryResult>.VisitClass(ClassDeclSymbol classDecl)
        {
            var thisOuter = outer;
            var thisSymbolFactory = symbolFactory;
            return new SymbolQueryResult.Class(typeArgs => thisSymbolFactory.MakeClass(thisOuter, classDecl, typeArgs));
        }

        SymbolQueryResult ITypeDeclSymbolVisitor<SymbolQueryResult>.VisitStruct(StructDeclSymbol structDecl)
        {
            var thisOuter = outer;
            var thisSymbolFactory = symbolFactory;
            return new SymbolQueryResult.Struct(typeArgs => thisSymbolFactory.MakeStruct(thisOuter, structDecl, typeArgs));
        }

        SymbolQueryResult ITypeDeclSymbolVisitor<SymbolQueryResult>.VisitEnum(EnumDeclSymbol enumDecl)
        {
            var thisOuter = outer;
            var thisSymbolFactory = symbolFactory;
            return new SymbolQueryResult.Enum(typeArgs => thisSymbolFactory.MakeEnum(thisOuter, enumDecl, typeArgs));
        }

        SymbolQueryResult ITypeDeclSymbolVisitor<SymbolQueryResult>.VisitEnumElem(EnumElemDeclSymbol enumElemDecl)
        {
            var outerEnum = outer as EnumSymbol;
            Debug.Assert(outerEnum != null);

            return new SymbolQueryResult.EnumElem(symbolFactory.MakeEnumElem(outerEnum, enumElemDecl));
        }

        SymbolQueryResult ITypeDeclSymbolVisitor<SymbolQueryResult>.VisitInterface(InterfaceDeclSymbol interfaceDecl)
        {
            throw new NotImplementedException();
        }

        SymbolQueryResult ITypeDeclSymbolVisitor<SymbolQueryResult>.VisitLambda(LambdaDeclSymbol declSymbol)
        {
            // SymbolQuery로는 Lambda가 나올 수 없다
            throw new RuntimeFatalException();

            //var outerFunc = outer as IFuncSymbol;
            //Debug.Assert(outerFunc != null);

            //return new SymbolQueryResult.Lambda(symbolFactory.MakeLambda(outerFunc, declSymbol));
        }

        SymbolQueryResult ITypeDeclSymbolVisitor<SymbolQueryResult>.VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol)
        {
            var outerLambda = outer as LambdaSymbol;
            Debug.Assert(outerLambda != null);

            return new SymbolQueryResult.LambdaMemberVar(symbolFactory.MakeLambdaMemberVar(outerLambda, declSymbol));
        }
    }
}
