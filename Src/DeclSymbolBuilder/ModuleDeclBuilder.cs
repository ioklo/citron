using Citron.Infra;
using Citron.Collections;
using Citron.Module;

using static Citron.Infra.Misc;
using Citron.Symbol;

namespace Citron.Test
{
    public class ModuleDeclBuilder
    {
        SymbolFactory factory;
        Name moduleName;

        ModuleDeclSymbol moduleDeclSymbol;

        NamespaceBuilderComponent<ModuleDeclBuilder> namespaceComponent;
        TypeBuilderComponent<ModuleDeclBuilder> globalTypeComponent;
        GlobalFuncBuilderComponent<ModuleDeclBuilder, ModuleDeclSymbol> globalFuncComponent;

        public ModuleDeclBuilder(SymbolFactory factory, Name moduleName, bool bReference)
        {
            this.factory = factory;
            this.moduleName = moduleName;            

            moduleDeclSymbol = new ModuleDeclSymbol(moduleName, bReference);
            this.namespaceComponent = new NamespaceBuilderComponent<ModuleDeclBuilder>(factory, this, moduleDeclSymbol);
            this.globalTypeComponent = new TypeBuilderComponent<ModuleDeclBuilder>(this, factory, moduleDeclSymbol, Accessor.Private);
            this.globalFuncComponent = new GlobalFuncBuilderComponent<ModuleDeclBuilder, ModuleDeclSymbol>(this, moduleDeclSymbol);
        }

        // redirects namespace component
        public NamespaceDeclBuilder<ModuleDeclBuilder> BeginNamespace(string name)
            => namespaceComponent.BeginNamespace(name);

        // redirects type component
        public ClassDeclBuilder<ModuleDeclBuilder> BeginClass(string name)
            => globalTypeComponent.BeginClass(name);

        public StructDeclBuilder<ModuleDeclBuilder> BeginStruct(Accessor accessModifier, string name)
            => globalTypeComponent.BeginStruct(accessModifier, name);

        public ModuleDeclBuilder GlobalFunc(Accessor accessor, FuncReturn funcReturn, string funcName, ImmutableArray<Name> typeParams, ImmutableArray<FuncParameter> funcParams)
            => globalFuncComponent.GlobalFunc(accessor, funcReturn, funcName, typeParams, funcParams);

        public ModuleDeclBuilder GlobalFunc(Accessor accessor, string funcName, ImmutableArray<Name> typeParams, GlobalFuncBuilderComponent<ModuleDeclBuilder, ModuleDeclSymbol>.PostSkeletonPhaseTask task)
            => globalFuncComponent.GlobalFunc(accessor, funcName, typeParams, task);
        
        public ModuleDeclSymbol Make()
        {
            namespaceComponent.DoPostSkeletonPhase();
            globalFuncComponent.DoPostSkeletonPhase();

            // 없다면 넣기
            // if (!globalFuncComponent.HasTopLevel())
            //    this.BeginTopLevelFunc().EndGlobalFunc(out var _);

            return moduleDeclSymbol;

            // TODO: 바로바로 Make했기 때문에 따로 생성하지 않도록 한다

            //var moduleDecl = new ModuleDeclSymbol(
            //    moduleName, 
            //    namespaceComponent.MakeNamespaceDecls(), 
            //    types: default, 
            //    globalFuncComponent.MakeGlobalFuncDecls()
            //);

            //moduleDeclHolder.SetValue(moduleDecl);

            //return moduleDecl;
        }
    }
}
