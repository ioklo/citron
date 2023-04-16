using Citron.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Symbol
{
    // utility class for {ClassDeclSymbol, StructDeclSymbol} QueryMember_Type
    public struct SymbolQueryResultBuilder : ITypeDeclSymbolVisitor
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
            decl.AcceptTypeDeclSymbolVisitor(ref builder);

            Debug.Assert(builder.result != null);
            return builder.result;
        }

        public void VisitClass(ClassDeclSymbol classDecl)
        {
            var thisOuter = outer;
            var thisSymbolFactory = symbolFactory;
            result = new SymbolQueryResult.Class(typeArgs => thisSymbolFactory.MakeClass(thisOuter, classDecl, typeArgs));
        }

        public void VisitStruct(StructDeclSymbol structDecl)
        {
            var thisOuter = outer;
            var thisSymbolFactory = symbolFactory;
            result = new SymbolQueryResult.Struct(typeArgs => thisSymbolFactory.MakeStruct(thisOuter, structDecl, typeArgs));
        }

        public void VisitEnum(EnumDeclSymbol enumDecl)
        {
            var thisOuter = outer;
            var thisSymbolFactory = symbolFactory;
            result = new SymbolQueryResult.Enum(typeArgs => thisSymbolFactory.MakeEnum(thisOuter, enumDecl, typeArgs));
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
            // SymbolQuery로는 Lambda가 나올 수 없다
            throw new RuntimeFatalException();

            //var outerFunc = outer as IFuncSymbol;
            //Debug.Assert(outerFunc != null);

            //result = new SymbolQueryResult.Lambda(symbolFactory.MakeLambda(outerFunc, declSymbol));
        }

        public void VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol)
        {
            var outerLambda = outer as LambdaSymbol;
            Debug.Assert(outerLambda != null);

            result = new SymbolQueryResult.LambdaMemberVar(symbolFactory.MakeLambdaMemberVar(outerLambda, declSymbol));
        }
    }
}
