namespace Citron.Symbol
{
    public interface ISymbolNodeVisitor<out TResult>
    {
        TResult VisitModule(ModuleSymbol symbol);
        TResult VisitNamespace(NamespaceSymbol symbol);
        TResult VisitGlobalFunc(GlobalFuncSymbol symbol);

        TResult VisitClass(ClassSymbol symbol);
        TResult VisitClassConstructor(ClassConstructorSymbol symbol);
        TResult VisitClassMemberFunc(ClassMemberFuncSymbol symbol);
        TResult VisitClassMemberVar(ClassMemberVarSymbol symbol);

        TResult VisitStruct(StructSymbol symbol);
        TResult VisitStructConstructor(StructConstructorSymbol symbol);
        TResult VisitStructMemberFunc(StructMemberFuncSymbol symbol);
        TResult VisitStructMemberVar(StructMemberVarSymbol symbol);

        TResult VisitEnum(EnumSymbol symbol);
        TResult VisitEnumElem(EnumElemSymbol symbol);
        TResult VisitEnumElemMemberVar(EnumElemMemberVarSymbol symbol);

        TResult VisitLambda(LambdaSymbol symbol);
        TResult VisitLambdaMemberVar(LambdaMemberVarSymbol symbol);

        TResult VisitInterface(InterfaceSymbol symbol);
    }
}