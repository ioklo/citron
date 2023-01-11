namespace Citron.Symbol
{
    public interface ISymbolNodeVisitor
    {
        void VisitModule(ModuleSymbol symbol);
        void VisitNamespace(NamespaceSymbol symbol);
        void VisitGlobalFunc(GlobalFuncSymbol symbol);
        void VisitGlobalVar(GlobalVarSymbol symbol);

        void VisitClass(ClassSymbol symbol);
        void VisitClassConstructor(ClassConstructorSymbol symbol);
        void VisitClassMemberFunc(ClassMemberFuncSymbol symbol);
        void VisitClassMemberVar(ClassMemberVarSymbol symbol);

        void VisitStruct(StructSymbol symbol);
        void VisitStructConstructor(StructConstructorSymbol symbol);
        void VisitStructMemberFunc(StructMemberFuncSymbol symbol);
        void VisitStructMemberVar(StructMemberVarSymbol symbol);

        void VisitEnum(EnumSymbol symbol);
        void VisitEnumElem(EnumElemSymbol symbol);
        void VisitEnumElemMemberVar(EnumElemMemberVarSymbol symbol);

        void VisitLambda(LambdaSymbol symbol);
        void VisitLambdaMemberVarSymbol(LambdaMemberVarSymbol symbol);

        void VisitInterface(InterfaceSymbol symbol);
    }
}