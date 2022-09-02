using Citron.Infra;
using Citron.Collections;
using Citron.Module;

using static Citron.Infra.Misc;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    public class ModuleDeclBuilder
    {
        SymbolFactory factory;
        Name moduleName;

        Holder<ModuleDeclSymbol> moduleDeclHolder;

        NamespaceBuilderComponent<ModuleDeclBuilder> namespaceComponent;
        TypeBuilderComponent<ModuleDeclBuilder> globalTypeComponent;
        GlobalFuncBuilderComponent<ModuleDeclBuilder> globalFuncComponent;

        public ModuleDeclBuilder(SymbolFactory factory, Name moduleName)
        {
            this.factory = factory;
            this.moduleName = moduleName;
            this.moduleDeclHolder = new Holder<ModuleDeclSymbol>();

            this.namespaceComponent = new NamespaceBuilderComponent<ModuleDeclBuilder>(factory, this, moduleDeclHolder);
            this.globalTypeComponent = new TypeBuilderComponent<ModuleDeclBuilder>(this, factory, moduleDeclHolder, AccessModifier.Private);
            this.globalFuncComponent = new GlobalFuncBuilderComponent<ModuleDeclBuilder>(factory, this, moduleDeclHolder);
        }

        // redirects namespace component
        public NamespaceDeclBuilder<ModuleDeclBuilder> BeginNamespace(string name)
            => namespaceComponent.BeginNamespace(name);

        // redirects type component
        public ClassDeclBuilder<ModuleDeclBuilder> BeginClass(string name)
            => globalTypeComponent.BeginClass(name);

        public StructDeclBuilder<ModuleDeclBuilder> BeginStruct(AccessModifier accessModifier, string name)
            => globalTypeComponent.BeginStruct(accessModifier, name);

        public ModuleDeclBuilder GlobalFunc(ITypeSymbol retType, string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, out globalFuncDecl);

        public ModuleDeclBuilder GlobalFunc(ITypeSymbol retType, string funcName, ITypeSymbol paramType, string paramName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, paramType, paramName, out globalFuncDecl);

        public ModuleDeclBuilder GlobalFunc(IHolder<FuncReturn> funcReturnHolder, string funcName,
            IHolder<ImmutableArray<FuncParameter>> funcParametersHolder, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(funcReturnHolder, funcName, funcParametersHolder, out globalFuncDecl);

        public GlobalFuncDeclBuilder<ModuleDeclBuilder> BeginGlobalFunc(AccessModifier accessModifier, Name funcName, bool bInternal)
            => globalFuncComponent.BeginGlobalFunc(accessModifier, funcName, bInternal);

        public GlobalFuncDeclBuilder<ModuleDeclBuilder> BeginTopLevelFunc()
        {
            return globalFuncComponent.BeginGlobalFunc(AccessModifier.Public, Name.TopLevel, true)
                .FuncReturn(false, factory.MakeVoid());
        }

        public ModuleDeclSymbol Make()
        {
            // 없다면 넣기
            if (!globalFuncComponent.HasTopLevel())
                this.BeginTopLevelFunc().EndGlobalFunc(out var _);

            var moduleDecl = new ModuleDeclSymbol(
                moduleName, 
                namespaceComponent.MakeNamespaceDecls(), 
                types: default, 
                globalFuncComponent.MakeGlobalFuncDecls()
            );

            moduleDeclHolder.SetValue(moduleDecl);

            return moduleDecl;
        }
    }
}
