namespace Citron.Symbol
{
    public interface IDeclSymbolNodeVisitor
    {
        void VisitModule(ModuleDeclSymbol moduleDeclSymbol);
        void VisitNamespace(NamespaceDeclSymbol namespaceDeclSymbol);

        void VisitGlobalFunc(GlobalFuncDeclSymbol globalFuncDeclSymbol);
        void VisitGlobalVar(GlobalVarDeclSymbol declSymbol);

        void VisitStruct(StructDeclSymbol structDeclSymbol);
        void VisitStructConstructor(StructConstructorDeclSymbol structConstructorDeclSymbol);
        void VisitStructMemberFunc(StructMemberFuncDeclSymbol structMemberFuncDeclSymbol);
        void VisitStructMemberVar(StructMemberVarDeclSymbol structMemberVarDeclSymbol);        
        void VisitClass(ClassDeclSymbol classDeclSymbol);
        void VisitClassConstructor(ClassConstructorDeclSymbol classConstructorDeclSymbol);        
        void VisitClassMemberFunc(ClassMemberFuncDeclSymbol classMemberFuncDeclSymbol);
        void VisitClassMemberVar(ClassMemberVarDeclSymbol classMemberVarDeclSymbol);

        void VisitEnum(EnumDeclSymbol enumDeclSymbol);
        void VisitEnumElem(EnumElemDeclSymbol enumElemDeclSymbol);
        void VisitEnumElemMemberVar(EnumElemMemberVarDeclSymbol enumElemMemberVarDeclSymbol);
        
        void VisitLambda(LambdaDeclSymbol lambdaDeclSymbol);
        void VisitLambdaMemberVar(LambdaMemberVarDeclSymbol lambdaMemberVarDeclSymbol);

        void VisitInterface(InterfaceDeclSymbol interfaceDeclSymbol);
    }
}