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
        TypeBuilderComponent<ModuleDeclBuilder, GlobalTypeDeclSymbol> globalTypeComponent;
        GlobalFuncBuilderComponent<ModuleDeclBuilder> globalFuncComponent;

        public ModuleDeclBuilder(SymbolFactory factory, Name moduleName)
        {
            this.factory = factory;
            this.moduleName = moduleName;
            this.moduleDeclHolder = new Holder<ModuleDeclSymbol>();

            this.namespaceComponent = new NamespaceBuilderComponent<ModuleDeclBuilder>(factory, this, moduleDeclHolder);
            this.globalTypeComponent = new TypeBuilderComponent<ModuleDeclBuilder, GlobalTypeDeclSymbol>(this, typeDecl => {
                return new GlobalTypeDeclSymbol(moduleDeclHolder, AccessModifier.Public, typeDecl);
            });
            this.globalFuncComponent = new GlobalFuncBuilderComponent<ModuleDeclBuilder>(factory, this, moduleDeclHolder);
        }

        // redirects namespace component
        public NamespaceDeclBuilder<ModuleDeclBuilder> BeginNamespace(string name)
            => namespaceComponent.BeginNamespace(name);

        // redirects type component
        public ClassDeclBuilder<ModuleDeclBuilder> BeginClass(string name)
            => globalTypeComponent.BeginClass(name);

        public ModuleDeclBuilder GlobalFunc(ITypeSymbol retType, string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, out globalFuncDecl);

        public ModuleDeclBuilder GlobalFunc(ITypeSymbol retType, string funcName, ITypeSymbol paramType, string paramName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, paramType, paramName, out globalFuncDecl);

        public ModuleDeclBuilder GlobalFunc(IHolder<FuncReturn> funcReturnHolder, string funcName,
            IHolder<ImmutableArray<FuncParameter>> funcParametersHolder, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(funcReturnHolder, funcName, funcParametersHolder, out globalFuncDecl);

        public GlobalFuncDeclBuilder<ModuleDeclBuilder> BeginGlobalFunc(IHolder<FuncReturn> funcRetHolder, Name funcName, IHolder<ImmutableArray<FuncParameter>> funcParamHolder)
            => globalFuncComponent.BeginGlobalFunc(funcRetHolder, funcName, funcParamHolder);

        public GlobalFuncDeclBuilder<ModuleDeclBuilder> BeginTopLevelFunc()
        {
            var funcReturnHolder = new Holder<FuncReturn>(new FuncReturn(false, factory.MakeVoid()));
            var funcParamsHolder = new Holder<ImmutableArray<FuncParameter>>(default);

            return globalFuncComponent.BeginGlobalFunc(funcReturnHolder, Name.TopLevel, funcParamsHolder);
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
