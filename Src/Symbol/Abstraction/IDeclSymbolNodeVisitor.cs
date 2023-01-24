namespace Citron.Symbol
{
    public interface IDeclSymbolNodeVisitor
    {
        void VisitModule(ModuleDeclSymbol declSymbol);
        void VisitNamespace(NamespaceDeclSymbol declSymbol);

        void VisitGlobalFunc(GlobalFuncDeclSymbol declSymbol);
        void VisitGlobalVar(GlobalVarDeclSymbol declSymbol);

        void VisitStruct(StructDeclSymbol declSymbol);
        void VisitStructConstructor(StructConstructorDeclSymbol declSymbol);
        void VisitStructMemberFunc(StructMemberFuncDeclSymbol declSymbol);
        void VisitStructMemberVar(StructMemberVarDeclSymbol declSymbol);        
        void VisitClass(ClassDeclSymbol declSymbol);
        void VisitClassConstructor(ClassConstructorDeclSymbol declSymbol);        
        void VisitClassMemberFunc(ClassMemberFuncDeclSymbol declSymbol);
        void VisitClassMemberVar(ClassMemberVarDeclSymbol declSymbol);

        void VisitEnum(EnumDeclSymbol declSymbol);
        void VisitEnumElem(EnumElemDeclSymbol declSymbol);
        void VisitEnumElemMemberVar(EnumElemMemberVarDeclSymbol declSymbol);
        
        void VisitLambda(LambdaDeclSymbol declSymbol);
        void VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol);

        void VisitInterface(InterfaceDeclSymbol declSymbol);
    }
}