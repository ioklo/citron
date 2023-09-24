namespace Citron.Symbol
{
    public interface ITypeDeclSymbolVisitor<out TResult>
    {
        TResult VisitClass(ClassDeclSymbol declSymbol);
        TResult VisitStruct(StructDeclSymbol declSymbol);
        TResult VisitEnum(EnumDeclSymbol declSymbol);
        TResult VisitEnumElem(EnumElemDeclSymbol declSymbol);        
        TResult VisitInterface(InterfaceDeclSymbol declSymbol);

        TResult VisitLambda(LambdaDeclSymbol declSymbol);
        TResult VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol);
    }
}