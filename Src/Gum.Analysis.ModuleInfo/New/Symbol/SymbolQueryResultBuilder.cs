using Gum.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Analysis
{
    // utility class for {ClassDeclSymbol, StructDeclSymbol} QueryMember_Type
    class SymbolQueryResultBuilder : ITypeDeclSymbolVisitor
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

        public static SymbolQueryResult Build(ITypeDeclSymbolContainer container, ISymbolNode outer, SymbolFactory symbolFactory)
        {
            var builder = new SymbolQueryResultBuilder(outer, symbolFactory);
            container.Apply(builder);

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

        public void VisitLambda(LambdaDeclSymbol lambdaDecl)
        {
            // 람다를 QueryResult로 줄 수 없다
            throw new UnreachableCodeException();

            //var outerFunc = outer as IFuncSymbol;
            //Debug.Assert(outerFunc != null);

            //result = new SymbolQueryResult.Lambda(symbolFactory.MakeLambda(outerFunc, lambdaDecl));
        }
    }
}
