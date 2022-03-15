namespace Citron.Analysis
{
    public interface IDeclSymbolNodeVisitor
    {
        void VisitModule(ModuleDeclSymbol moduleDeclSymbol);
        void VisitNamespace(NamespaceDeclSymbol namespaceDeclSymbol);

        void VisitGlobalFunc(GlobalFuncDeclSymbol globalFuncDeclSymbol);        
        void VisitStruct(StructDeclSymbol structDeclSymbol);
        void VisitStructConstructor(StructConstructorDeclSymbol structConstructorDeclSymbol);
        void VisitList(ListDeclSymbol listDeclSymbol);
        void VisitStructMemberFunc(StructMemberFuncDeclSymbol structMemberFuncDeclSymbol);
        void VisitString(StringDeclSymbol stringDeclSymbol);
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

        // proxies
        void VisitRuntimeModule(RuntimeModuleDeclSymbol runtimeModuleDeclSymbol);
        void VisitSystemNamespace(SystemNamespaceDeclSymbol systemNamespaceDeclSymbol);
        void VisitBool(BoolDeclSymbol boolDeclSymbol);
        void VisitInt(IntDeclSymbol intDeclSymbol);
    }
}