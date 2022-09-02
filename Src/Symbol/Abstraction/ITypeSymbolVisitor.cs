﻿namespace Citron.Symbol
{
    public interface ITypeSymbolVisitor
    {
        void VisitEnumElem(EnumElemSymbol symbol);
        void VisitClass(ClassSymbol symbol);
        void VisitEnum(EnumSymbol symbol);
        void VisitInterface(InterfaceSymbol symbol);
        void VisitStruct(StructSymbol symbol);
        void VisitVoid(VoidSymbol symbol);
        void VisitTuple(TupleSymbol symbol);
        void VisitNullable(NullableSymbol symbol);
        void VisitLambda(LambdaSymbol lambdaSymbol);
        void VisitTypeVar(TypeVarSymbol typeVarSymbol);
    }
}