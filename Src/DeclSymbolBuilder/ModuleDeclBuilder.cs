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
            this.globalFuncComponent = new GlobalFuncBuilderComponent<ModuleDeclBuilder, ModuleDeclSymbol>(factory, this, moduleDeclSymbol);
        }

        // redirects namespace component
        public NamespaceDeclBuilder<ModuleDeclBuilder> BeginNamespace(string name)
            => namespaceComponent.BeginNamespace(name);

        // redirects type component
        public ClassDeclBuilder<ModuleDeclBuilder> BeginClass(string name)
            => globalTypeComponent.BeginClass(name);

        public StructDeclBuilder<ModuleDeclBuilder> BeginStruct(Accessor accessModifier, string name)
            => globalTypeComponent.BeginStruct(accessModifier, name);

        // 모든 인자가 있는 버전
        public ModuleDeclBuilder GlobalFunc(FuncReturn funcReturn, string funcName, ImmutableArray<FuncParameter> funcParams, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(funcReturn, funcName, funcParams, out globalFuncDecl);

        // 인자 없는
        public ModuleDeclBuilder GlobalFunc(ITypeSymbol retType, string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, out globalFuncDecl);

        // 인자 1개
        public ModuleDeclBuilder GlobalFunc(ITypeSymbol retType, string funcName, ITypeSymbol paramType, string paramName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, paramType, paramName, out globalFuncDecl);

        // 인자를 추후에 설정해야 하는 버전, 실수하기 쉬울것 같다
        public ModuleDeclBuilder GlobalFunc(string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(funcName, out globalFuncDecl);

        public GlobalFuncDeclBuilder<ModuleDeclBuilder, ModuleDeclSymbol> BeginGlobalFunc(Accessor accessModifier, Name funcName, bool bInternal)
            => globalFuncComponent.BeginGlobalFunc(accessModifier, funcName, bInternal);

        public GlobalFuncDeclBuilder<ModuleDeclBuilder, ModuleDeclSymbol> BeginTopLevelFunc()
        {
            return globalFuncComponent.BeginGlobalFunc(Accessor.Public, Name.TopLevel, true)
                .FuncReturn(false, factory.MakeVoid());
        }

        public ModuleDeclSymbol Make()
        {
            // 없다면 넣기
            if (!globalFuncComponent.HasTopLevel())
                this.BeginTopLevelFunc().EndGlobalFunc(out var _);

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
