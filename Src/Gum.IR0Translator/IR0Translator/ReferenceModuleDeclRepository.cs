using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Gum.Infra;
using System.Diagnostics.Contracts;
using Gum.CompileTime;

namespace Gum.Analysis
{   
    class ModuleDeclSymbolBuilder
    {
        class AfterBuildContext
        {
            public ITypeSymbolNode GetTypeSymbolNode(TypeId typeId)
            {
                throw new NotImplementedException();
            }
        }

        void RegisterAfterBuildTask(Action<AfterBuildContext> action)
        {
            throw new NotImplementedException();
        }

        public static ModuleDeclSymbol Build(ModuleDecl decl)
        {
            var builder = new ModuleDeclSymbolBuilder();
            return builder.BuildModuleDecl(decl);
        }

        ModuleDeclSymbol BuildModuleDecl(ModuleDecl decl)
        {
            var moduleHolder = new Holder<ModuleDeclSymbol>();

            var namespaces = BuildNamespaceDecls(moduleHolder, decl.Namespaces);
            var globalTypes = BuildGlobalTypeDecls(moduleHolder, decl.Types);
            var globalFuncs = BuildGlobalFuncs(moduleHolder, decl.Funcs);

            var symbol = new ModuleDeclSymbol(decl.Name, namespaces, globalTypes, globalFuncs);
            moduleHolder.SetValue(symbol);
            return symbol;
        }

        ImmutableArray<NamespaceDeclSymbol> BuildNamespaceDecls(IHolder<ITopLevelDeclSymbolNode> outer, ImmutableArray<NamespaceDecl> namespaces)
        {
            var builder = ImmutableArray.CreateBuilder<NamespaceDeclSymbol>(namespaces.Length);

            foreach(var @namespace in namespaces)
            {
                var namespaceDeclSymbol = BuildNamespace(outer, @namespace);
                builder.Add(namespaceDeclSymbol);
            }

            return builder.MoveToImmutable();
        }

        NamespaceDeclSymbol BuildNamespace(IHolder<ITopLevelDeclSymbolNode> outer, NamespaceDecl @namespace)
        {
            var holder = new Holder<NamespaceDeclSymbol>();

            var childNamespaces = BuildNamespaceDecls(holder, @namespace.Namespaces);
            var globalTypeDecls = BuildGlobalTypeDecls(holder, @namespace.Types);
            var globalFuncDecls = BuildGlobalFuncDecls(@namespace.Funcs);

            var result = new NamespaceDeclSymbol(outer, @namespace.Name, childNamespaces, globalTypeDecls, globalFuncDecls);
            holder.SetValue(result);

            return result;
        }

        GlobalTypeDeclSymbol BuildGlobalTypeDecl(IHolder<ITopLevelDeclSymbolNode> outer, GlobalTypeDecl globalTypeDecl)
        {            
            var typeDeclSymbolNode = BuildTypeDecl(outer, globalTypeDecl.TypeDecl);

            return new GlobalTypeDeclSymbol(globalTypeDecl.AccessModifier, typeDeclSymbolNode);
        }

        ImmutableArray<GlobalTypeDeclSymbol> BuildGlobalTypeDecls(IHolder<ITopLevelDeclSymbolNode> outer, ImmutableArray<GlobalTypeDecl> globalTypeDecls)
        {
            var builder = ImmutableArray.CreateBuilder<GlobalTypeDeclSymbol>(globalTypeDecls.Length);
            foreach(var globalTypeDecl in globalTypeDecls)
            {
                var typeDeclSymbol = BuildGlobalTypeDecl(outer, globalTypeDecl);
                builder.Add(typeDeclSymbol);
            }

            return builder.MoveToImmutable();
        }

        ImmutableArray<ClassMemberTypeDeclSymbol> BuildClassMemberTypeDecls(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassMemberTypeDecl> decls)
        {
            var builder = ImmutableArray.CreateBuilder<ClassMemberTypeDeclSymbol>(decls.Length);

            foreach (var decl in decls)
            {
                var typeDeclSymbolNode = BuildTypeDecl(outer, decl.TypeDecl);
                builder.Add(new ClassMemberTypeDeclSymbol(decl.AccessModifier, typeDeclSymbolNode));
            }

            return builder.MoveToImmutable();
        }

        (ClassConstructorDeclSymbol? TrivialConstructor, ImmutableArray<ClassConstructorDeclSymbol> Constructors) BuildClassConstructorDecls(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassConstructorDecl> decls)
        {
            ClassConstructorDeclSymbol? trivialConstructor = null;

            var builder = ImmutableArray.CreateBuilder<ClassConstructorDeclSymbol>(decls.Length);

            foreach (var decl in decls)
            {
                var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

                RegisterAfterBuildTask(context =>
                {
                    var parameters = BuildFuncParameters(decl.Parameters, context);
                    parametersHolder.SetValue(parameters);
                });

                var constructorDecl = new ClassConstructorDeclSymbol(outer, decl.AccessModifier, parametersHolder, decl.IsTrivial);
                builder.Add(constructorDecl);

                if (decl.IsTrivial)
                {
                    Debug.Assert(trivialConstructor == null);
                    trivialConstructor = constructorDecl;
                }
            }

            return (trivialConstructor, builder.MoveToImmutable());
        }

        ImmutableArray<FuncParameter> BuildFuncParameters(ImmutableArray<Param> parameters, AfterBuildContext context)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(parameters.Length);
            foreach (var param in parameters)
            {
                var paramKind = param.Kind.MakeFuncParameterKind();
                var paramType = context.GetTypeSymbolNode(param.Type);

                builder.Add(new FuncParameter(paramKind, paramType, param.Name));
            }

            return builder.MoveToImmutable();
        }

        ImmutableArray<ClassMemberFuncDeclSymbol> BuildClassMemberFuncDecls(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassMemberFuncDecl> decls)
        {
            var builder = ImmutableArray.CreateBuilder<ClassMemberFuncDeclSymbol>(decls.Length);

            foreach(var decl in decls)
            {
                // typeId였는데, ITypeSymbolNode로 바꿀 수 있는가
                // 지금 바꿔야 하는가.. 어느시점에 확정이 되는가                
                var returnHolder = new Holder<FuncReturn>();
                var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

                // 빌드가 끝난 후
                RegisterAfterBuildTask(context =>
                {
                    var retType = context.GetTypeSymbolNode(decl.RetType);
                    returnHolder.SetValue(new FuncReturn(decl.IsRefReturn, retType));

                    var parameters = BuildFuncParameters(decl.Parameters, context);
                    parametersHolder.SetValue(parameters);
                });

                var symbol = new ClassMemberFuncDeclSymbol(outer, decl.AccessModifier, returnHolder, decl.Name, decl.TypeParams, parametersHolder, decl.IsStatic);
                builder.Add(symbol);
            }

            return builder.MoveToImmutable();
        }

        ImmutableArray<ClassMemberVarDeclSymbol> BuildClassMemberVarDecls(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassMemberVarDecl> decls)
        {
            var builder = ImmutableArray.CreateBuilder<ClassMemberVarDeclSymbol>(decls.Length);

            foreach(var decl in decls)
            {
                var declTypeHolder = new Holder<ITypeSymbolNode>();

                RegisterAfterBuildTask(context =>
                {
                    var declType = context.GetTypeSymbolNode(decl.Type);
                    declTypeHolder.SetValue(declType);
                });

                var symbol = new ClassMemberVarDeclSymbol(outer, decl.AccessModifier, decl.IsStatic, declTypeHolder, decl.Name);
                builder.Add(symbol);
            }

            return builder.MoveToImmutable();
        }

        ClassDeclSymbol BuildClassDecl(IHolder<IDeclSymbolNode> outer, ClassDecl classDecl)
        {
            var holder = new Holder<ClassDeclSymbol>();

            var memberTypes = BuildClassMemberTypeDecls(holder, classDecl.MemberTypes);
            var (trivialConstructor, constructors) = BuildClassConstructorDecls(holder, classDecl.Constructors);
            var memberFuncs = BuildClassMemberFuncDecls(holder, classDecl.MemberFuncs);
            var memberVars = BuildClassMemberVarDecls(holder, classDecl.MemberVars);
            

            // 확정
            var trivialConstructorHolder = new Holder<ClassConstructorDeclSymbol?>(trivialConstructor);
            var constructorsHolder = new Holder<ImmutableArray<ClassConstructorDeclSymbol>>(constructors);

            // 미 확정 
            var baseClassHolder = new Holder<ClassSymbol>();
            var interfacesHolder = new Holder<ImmutableArray<InterfaceSymbol>>();            

            RegisterAfterBuildTask(context =>
            {
                if(classDecl.BaseType != null)
                {
                    var baseClass = context.GetTypeSymbolNode(classDecl.BaseType) as ClassSymbol;

                    // TODO: 에러 처리
                    Debug.Assert(baseClass != null);                    
                    baseClassHolder.SetValue(baseClass);
                }

                var interfacesBuilder = ImmutableArray.CreateBuilder<InterfaceSymbol>(classDecl.Interfaces.Length);
                foreach (var @interface in classDecl.Interfaces)
                {
                    var interfaceSymbol = context.GetTypeSymbolNode(@interface) as InterfaceSymbol;

                    // TODO: 에러 처리
                    Debug.Assert(interfaceSymbol != null);

                    interfacesBuilder.Add(interfaceSymbol);
                }

                interfacesHolder.SetValue(interfacesBuilder.MoveToImmutable());
            });

            var symbol = new ClassDeclSymbol(outer, classDecl.Name, classDecl.TypeParams, baseClassHolder, interfacesHolder, memberTypes, memberFuncs, memberVars, constructorsHolder, trivialConstructorHolder);
            holder.SetValue(symbol);

            return symbol;
        }

        ITypeDeclSymbolNode BuildTypeDecl(IHolder<IDeclSymbolNode> outer, TypeDecl typeDecl)
        {
            switch(typeDecl)
            {
                case ClassDecl classDecl: return BuildClassDecl(outer, classDecl);
            }
        }
    }

    // 'Translation 단계에서만' 사용하는 레퍼런스 검색 (TypeExpEvaluator, Analyzer에서 사용한다)
    [Pure]
    public class ReferenceModuleDeclRepository 
    {
        ImmutableArray<ModuleDeclSymbol> moduleInfos;

        public static ReferenceModuleDeclRepository Build(ImmutableArray<ModuleDecl> decls)
        {


        }

        public ReferenceModuleDeclRepository(ImmutableArray<M.ModuleDecl> moduleInfos)
        {
            var builder = ImmutableArray.CreateBuilder<ExternalModuleDecl>(moduleInfos.Length);

            foreach (var moduleInfo in moduleInfos)
                builder.Add(new ExternalModuleDecl(moduleInfo));

            this.moduleInfos = builder.MoveToImmutable();
        }

        public ImmutableArray<ExternalModuleDecl> GetAllModules()
        {
            return moduleInfos;
        }
    }
}
