namespace Citron.Symbol
{
    public interface IDeclSymbolNodeVisitor<TResult>
    {
        TResult VisitModule(ModuleDeclSymbol declSymbol);
        TResult VisitNamespace(NamespaceDeclSymbol declSymbol);

        TResult VisitGlobalFunc(GlobalFuncDeclSymbol declSymbol);

        TResult VisitStruct(StructDeclSymbol declSymbol);
        TResult VisitStructConstructor(StructConstructorDeclSymbol declSymbol);
        TResult VisitStructMemberFunc(StructMemberFuncDeclSymbol declSymbol);
        TResult VisitStructMemberVar(StructMemberVarDeclSymbol declSymbol);        
        TResult VisitClass(ClassDeclSymbol declSymbol);
        TResult VisitClassConstructor(ClassConstructorDeclSymbol declSymbol);        
        TResult VisitClassMemberFunc(ClassMemberFuncDeclSymbol declSymbol);
        TResult VisitClassMemberVar(ClassMemberVarDeclSymbol declSymbol);

        TResult VisitEnum(EnumDeclSymbol declSymbol);
        TResult VisitEnumElem(EnumElemDeclSymbol declSymbol);
        TResult VisitEnumElemMemberVar(EnumElemMemberVarDeclSymbol declSymbol);
        
        TResult VisitLambda(LambdaDeclSymbol declSymbol);
        TResult VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol);

        TResult VisitInterface(InterfaceDeclSymbol declSymbol);
    }
}