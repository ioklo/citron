namespace Citron.Symbol
{
    public interface ITypeSymbolVisitor<out TResult>
    {
        TResult VisitEnumElem(EnumElemSymbol symbol);
        TResult VisitClass(ClassSymbol symbol);
        TResult VisitEnum(EnumSymbol symbol);
        TResult VisitInterface(InterfaceSymbol symbol);
        TResult VisitStruct(StructSymbol symbol);
        TResult VisitLambda(LambdaSymbol symbol);
    }
}