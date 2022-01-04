﻿using System;
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

        // TODO: dependency (moduleDecl에 있어야 할 것 같다)
        public static ImmutableArray<ModuleDeclSymbol> Build(ImmutableArray<ModuleDecl> decls)
        {
            var builder = new ModuleDeclSymbolBuilder();

            var symbolsBuilder = ImmutableArray.CreateBuilder<ModuleDeclSymbol>(decls.Length);
            foreach (var decl in decls)
            {
                var symbol = builder.BuildModule(decl);
                symbolsBuilder.Add(symbol);
            }

            return symbolsBuilder.MoveToImmutable();
        }

        ModuleDeclSymbol BuildModule(ModuleDecl decl)
        {
            var moduleHolder = new Holder<ModuleDeclSymbol>();

            var namespaces = BuildNamespaces(moduleHolder, decl.Namespaces);
            var globalTypes = BuildGlobalTypes(moduleHolder, decl.Types);
            var globalFuncs = BuildGlobalFuncs(moduleHolder, decl.Funcs);

            var symbol = new ModuleDeclSymbol(decl.Name, namespaces, globalTypes, globalFuncs);
            moduleHolder.SetValue(symbol);
            return symbol;
        }

        ImmutableArray<NamespaceDeclSymbol> BuildNamespaces(IHolder<ITopLevelDeclSymbolNode> outer, ImmutableArray<NamespaceDecl> namespaces)
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
            var namespaceSymbolHolder = new Holder<NamespaceDeclSymbol>();

            var childNamespaces = BuildNamespaces(namespaceSymbolHolder, @namespace.Namespaces);
            var globalTypeDecls = BuildGlobalTypes(namespaceSymbolHolder, @namespace.Types);
            var globalFuncDecls = BuildGlobalFuncs(namespaceSymbolHolder, @namespace.Funcs);

            var namespaceSymbol = new NamespaceDeclSymbol(outer, @namespace.Name, childNamespaces, globalTypeDecls, globalFuncDecls);
            namespaceSymbolHolder.SetValue(namespaceSymbol);
            return namespaceSymbol;
        }

        GlobalTypeDeclSymbol BuildGlobalType(IHolder<ITopLevelDeclSymbolNode> outer, GlobalTypeDecl globalTypeDecl)
        {            
            var typeDeclSymbolNode = BuildType(outer, globalTypeDecl.TypeDecl);

            return new GlobalTypeDeclSymbol(globalTypeDecl.AccessModifier, typeDeclSymbolNode);
        }

        ImmutableArray<GlobalTypeDeclSymbol> BuildGlobalTypes(IHolder<ITopLevelDeclSymbolNode> outer, ImmutableArray<GlobalTypeDecl> globalTypeDecls)
        {
            var builder = ImmutableArray.CreateBuilder<GlobalTypeDeclSymbol>(globalTypeDecls.Length);
            foreach(var globalTypeDecl in globalTypeDecls)
            {
                var typeDeclSymbol = BuildGlobalType(outer, globalTypeDecl);
                builder.Add(typeDeclSymbol);
            }

            return builder.MoveToImmutable();
        }

        GlobalFuncDeclSymbol BuildGlobalFunc(IHolder<ITopLevelDeclSymbolNode> outerHolder, GlobalFuncDecl decl)
        {
            var returnHolder = new Holder<FuncReturn>();
            var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

            RegisterAfterBuildTask(context =>
            {
                var retType = context.GetTypeSymbolNode(decl.RetType);
                returnHolder.SetValue(new FuncReturn(decl.IsRefReturn, retType));

                var parameters = BuildFuncParameters(decl.Parameters, context);
                parametersHolder.SetValue(parameters);
            });

            return new GlobalFuncDeclSymbol(outerHolder, decl.AccessModifier, returnHolder, decl.Name, decl.TypeParams, parametersHolder, false);
        }

        ImmutableArray<GlobalFuncDeclSymbol> BuildGlobalFuncs(IHolder<ITopLevelDeclSymbolNode> outerHolder, ImmutableArray<GlobalFuncDecl> globalFuncDecls)
        {
            var builder = ImmutableArray.CreateBuilder<GlobalFuncDeclSymbol>(globalFuncDecls.Length);
            foreach (var globalFuncDecl in globalFuncDecls)
            {
                var funcDeclSymbol = BuildGlobalFunc(outerHolder, globalFuncDecl);
                builder.Add(funcDeclSymbol);
            }

            return builder.MoveToImmutable();
        }

        ImmutableArray<ClassMemberTypeDeclSymbol> BuildClassMemberTypes(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassMemberTypeDecl> decls)
        {
            var builder = ImmutableArray.CreateBuilder<ClassMemberTypeDeclSymbol>(decls.Length);

            foreach (var decl in decls)
            {
                var typeDeclSymbolNode = BuildType(outer, decl.TypeDecl);
                builder.Add(new ClassMemberTypeDeclSymbol(decl.AccessModifier, typeDeclSymbolNode));
            }

            return builder.MoveToImmutable();
        }

        (ClassConstructorDeclSymbol? TrivialConstructor, ImmutableArray<ClassConstructorDeclSymbol> Constructors) BuildClassConstructors(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassConstructorDecl> decls)
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

        ImmutableArray<ClassMemberFuncDeclSymbol> BuildClassMemberFuncs(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassMemberFuncDecl> decls)
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

        ImmutableArray<ClassMemberVarDeclSymbol> BuildClassMemberVars(IHolder<ClassDeclSymbol> outer, ImmutableArray<ClassMemberVarDecl> decls)
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

        ClassDeclSymbol BuildClass(IHolder<IDeclSymbolNode> outer, ClassDecl classDecl)
        {
            var holder = new Holder<ClassDeclSymbol>();

            var memberTypes = BuildClassMemberTypes(holder, classDecl.MemberTypes);
            var (trivialConstructor, constructors) = BuildClassConstructors(holder, classDecl.Constructors);
            var memberFuncs = BuildClassMemberFuncs(holder, classDecl.MemberFuncs);
            var memberVars = BuildClassMemberVars(holder, classDecl.MemberVars);
            

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

        ImmutableArray<StructMemberTypeDeclSymbol> BuildStructMemberTypes(IHolder<StructDeclSymbol> outer, ImmutableArray<StructMemberTypeDecl> decls)
        {
            var builder = ImmutableArray.CreateBuilder<StructMemberTypeDeclSymbol>(decls.Length);

            foreach (var decl in decls)
            {
                var typeDeclSymbolNode = BuildType(outer, decl.TypeDecl);
                builder.Add(new StructMemberTypeDeclSymbol(decl.AccessModifier, typeDeclSymbolNode));
            }

            return builder.MoveToImmutable();
        }

        (StructConstructorDeclSymbol? TrivialConstructor, ImmutableArray<StructConstructorDeclSymbol> Constructors) BuildStructConstructors(IHolder<StructDeclSymbol> outer, ImmutableArray<StructConstructorDecl> decls)
        {
            StructConstructorDeclSymbol? trivialConstructor = null;

            var builder = ImmutableArray.CreateBuilder<StructConstructorDeclSymbol>(decls.Length);

            foreach (var decl in decls)
            {
                var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

                RegisterAfterBuildTask(context =>
                {
                    var parameters = BuildFuncParameters(decl.Parameters, context);
                    parametersHolder.SetValue(parameters);
                });

                var constructorDecl = new StructConstructorDeclSymbol(outer, decl.AccessModifier, parametersHolder, decl.IsTrivial);
                builder.Add(constructorDecl);

                if (decl.IsTrivial)
                {
                    Debug.Assert(trivialConstructor == null);
                    trivialConstructor = constructorDecl;
                }
            }

            return (trivialConstructor, builder.MoveToImmutable());
        }

        ImmutableArray<StructMemberFuncDeclSymbol> BuildStructMemberFuncs(IHolder<StructDeclSymbol> outer, ImmutableArray<StructMemberFuncDecl> decls)
        {
            var builder = ImmutableArray.CreateBuilder<StructMemberFuncDeclSymbol>(decls.Length);

            foreach (var decl in decls)
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

                var symbol = new StructMemberFuncDeclSymbol(outer, decl.AccessModifier, decl.IsStatic, returnHolder, decl.Name, decl.TypeParams, parametersHolder);
                builder.Add(symbol);
            }

            return builder.MoveToImmutable();
        }

        ImmutableArray<StructMemberVarDeclSymbol> BuildStructMemberVars(IHolder<StructDeclSymbol> outer, ImmutableArray<StructMemberVarDecl> decls)
        {
            var builder = ImmutableArray.CreateBuilder<StructMemberVarDeclSymbol>(decls.Length);

            foreach (var decl in decls)
            {
                var declTypeHolder = new Holder<ITypeSymbolNode>();

                RegisterAfterBuildTask(context =>
                {
                    var declType = context.GetTypeSymbolNode(decl.Type);
                    declTypeHolder.SetValue(declType);
                });

                var symbol = new StructMemberVarDeclSymbol(outer, decl.AccessModifier, decl.IsStatic, declTypeHolder, decl.Name);
                builder.Add(symbol);
            }

            return builder.MoveToImmutable();
        }


        StructDeclSymbol BuildStruct(IHolder<IDeclSymbolNode> outer, StructDecl structDecl)
        {
            var holder = new Holder<StructDeclSymbol>();

            var memberTypes = BuildStructMemberTypes(holder, structDecl.MemberTypes);
            var (trivialConstructor, constructors) = BuildStructConstructors(holder, structDecl.Constructors);
            var memberFuncs = BuildStructMemberFuncs(holder, structDecl.MemberFuncs);
            var memberVars = BuildStructMemberVars(holder, structDecl.MemberVars);


            // 확정
            var trivialConstructorHolder = new Holder<StructConstructorDeclSymbol?>(trivialConstructor);
            var constructorsHolder = new Holder<ImmutableArray<StructConstructorDeclSymbol>>(constructors);

            // 미 확정 
            var baseStructHolder = new Holder<StructSymbol>();
            //var interfacesHolder = new Holder<ImmutableArray<InterfaceSymbol>>();

            RegisterAfterBuildTask(context =>
            {
                if (structDecl.BaseType != null)
                {
                    var baseStruct = context.GetTypeSymbolNode(structDecl.BaseType) as StructSymbol;

                    // TODO: 에러 처리
                    Debug.Assert(baseStruct != null);
                    baseStructHolder.SetValue(baseStruct);
                }

                // 현재 막아놓음
                Debug.Assert(structDecl.Interfaces.Length == 0);
                //var interfacesBuilder = ImmutableArray.CreateBuilder<InterfaceSymbol>(structDecl.Interfaces.Length);
                //foreach (var @interface in structDecl.Interfaces)
                //{
                //    var interfaceSymbol = context.GetTypeSymbolNode(@interface) as InterfaceSymbol;

                //    // TODO: 에러 처리
                //    Debug.Assert(interfaceSymbol != null);

                //    interfacesBuilder.Add(interfaceSymbol);
                //}

                //interfacesHolder.SetValue(interfacesBuilder.MoveToImmutable());
            });

            var symbol = new StructDeclSymbol(outer, structDecl.Name, structDecl.TypeParams, baseStructHolder, memberTypes, memberFuncs, memberVars, constructorsHolder, trivialConstructorHolder);
            holder.SetValue(symbol);

            return symbol;
        }

        EnumElemMemberVarDeclSymbol BuildEnumElemMemberVar(IHolder<EnumElemDeclSymbol> outerHolder, EnumElemMemberVarDecl decl)
        {
            var declTypeHolder = new Holder<ITypeSymbolNode>();

            RegisterAfterBuildTask(context =>
            {
                var declType = context.GetTypeSymbolNode(decl.DeclType);
                declTypeHolder.SetValue(declType);
            });

            return new EnumElemMemberVarDeclSymbol(outerHolder, declTypeHolder, decl.Name);
        }        

        EnumElemDeclSymbol BuildEnumElem(IHolder<EnumDeclSymbol> outerHolder, EnumElemDecl decl)
        {
            var enumElemHolder = new Holder<EnumElemDeclSymbol>();

            var builder = ImmutableArray.CreateBuilder<EnumElemMemberVarDeclSymbol>(decl.MemberVars.Length);
            foreach(var memberVar in decl.MemberVars)
            {
                var memberVarSymbol = BuildEnumElemMemberVar(enumElemHolder, memberVar);
                builder.Add(memberVarSymbol);
            }

            var enumElem = new EnumElemDeclSymbol(outerHolder, decl.Name, builder.MoveToImmutable());
            enumElemHolder.SetValue(enumElem);
            return enumElem;
        }

        EnumDeclSymbol BuildEnum(IHolder<IDeclSymbolNode> outer, EnumDecl decl)
        {
            var enumHolder = new Holder<EnumDeclSymbol>();

            var elemsBuilder = ImmutableArray.CreateBuilder<EnumElemDeclSymbol>(decl.ElemDecls.Length);
            foreach (var elem in decl.ElemDecls)
            {
                var elemSymbol = BuildEnumElem(enumHolder, elem);
                elemsBuilder.Add(elemSymbol);
            }

            var @enum = new EnumDeclSymbol(outer, decl.Name, decl.TypeParams, elemsBuilder.MoveToImmutable());
            enumHolder.SetValue(@enum);

            return @enum;
        }

        ITypeDeclSymbolNode BuildType(IHolder<IDeclSymbolNode> outer, TypeDecl typeDecl)
        {
            switch(typeDecl)
            {
                case ClassDecl classDecl: return BuildClass(outer, classDecl);
                case StructDecl structDecl: return BuildStruct(outer, structDecl);
                case EnumDecl enumDecl: return BuildEnum(outer, enumDecl);
                default: throw new UnreachableCodeException();
            }
        }
    }
}
