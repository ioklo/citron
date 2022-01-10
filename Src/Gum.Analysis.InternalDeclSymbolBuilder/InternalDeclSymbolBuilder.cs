using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using System.Text;
using Pretune;
using Gum.Analysis;

using S = Gum.Syntax;
using M = Gum.CompileTime;

using static Gum.Infra.Misc;

namespace Gum.Analysis
{   
    public class InternalDeclSymbolBuilder
    {
        // 내부에서 빠져나오는 역할
        class FatalException : Exception
        {
        }

        TypeExpInfoService typeExpInfoService;
        ImmutableArray<ModuleDeclSymbol> referenceModules;

        InternalDeclSymbolBuilder(TypeExpInfoService typeExpInfoService, ImmutableArray<ModuleDeclSymbol> referenceModules)
        {
            this.typeExpInfoService = typeExpInfoService;
            this.referenceModules = referenceModules;
        }

        class AfterBuildContext
        {
            public ITypeSymbolNode GetTypeSymbolNode(S.TypeExp typeExp) // throw FatalException
            {
                throw new NotImplementedException();
            }
        }

        // dependency
        void RegisterAfterBuild(ImmutableArray<DeclSymbolId> dependsOnSymbols, Action<AfterBuildContext> action)
        {
            throw new NotImplementedException();
        }

        public static ModuleDeclSymbol? Build(M.Name moduleName, S.Script script, TypeExpInfoService typeExpInfoService, ImmutableArray<ModuleDeclSymbol> referenceModules) // nothrow
        {
            var builder = new InternalDeclSymbolBuilder(typeExpInfoService, referenceModules);

            try
            {                
                return builder.BuildScript(moduleName, script);
            }
            catch(FatalException)
            {
                // TODO: 에러처리
                return null;
            }
        }

        #region Common Function
        ImmutableArray<FuncParameter> MakeParameters(AfterBuildContext context, ImmutableArray<S.FuncParam> sparams)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(sparams.Length);
            foreach (var sparam in sparams)
            {
                var type = context.GetTypeSymbolNode(sparam.Type);
                if (type == null) throw new FatalException();

                var paramKind = sparam.Kind switch
                {
                    S.FuncParamKind.Normal => FuncParameterKind.Default,
                    S.FuncParamKind.Params => FuncParameterKind.Params,
                    S.FuncParamKind.Ref => FuncParameterKind.Ref,
                    _ => throw new UnreachableCodeException()
                };

                builder.Add(new FuncParameter(paramKind, type, new M.Name.Normal(sparam.Name)));
            }

            return builder.MoveToImmutable();
        }

        // TODO: 이 함수를 쓰지 않도록 syntax를 수정해야할 필요, TypeDecl겉에 정보가 있어야 할 거 같다
        S.AccessModifier? GetAccessModifier(S.TypeDecl decl)
        {
            return decl switch
            {
                S.StructDecl structDecl => structDecl.AccessModifier,
                S.ClassDecl classDecl => classDecl.AccessModifier,
                S.EnumDecl enumDecl => enumDecl.AccessModifier,
                _ => throw new UnreachableCodeException()
            };
        }

        ITypeDeclSymbolNode BuildType(IHolder<IDeclSymbolNode> outerHolder, S.TypeDecl decl)
        {
            switch (decl)
            {
                case S.StructDecl structDecl:
                    return BuildStruct(outerHolder, structDecl);

                case S.ClassDecl classDecl:
                    return BuildClass(outerHolder, classDecl);

                case S.EnumDecl enumDecl:
                    return BuildEnum(outerHolder, enumDecl);

                default:
                    throw new UnreachableCodeException();
            }
        }

        #endregion

        ModuleDeclSymbol BuildScript(M.Name moduleName, S.Script script) // throw FatalException
        {
            var moduleDeclHolder = new Holder<ModuleDeclSymbol>();

            var namespaceDecls = ImmutableArray.CreateBuilder<NamespaceDeclSymbol>();
            var globalTypeDecls = ImmutableArray.CreateBuilder<GlobalTypeDeclSymbol>();
            var globalFuncDecls = ImmutableArray.CreateBuilder<GlobalFuncDeclSymbol>();

            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    // TODO: namespace

                    case S.TypeDeclScriptElement typeDeclElem:
                        var globalTypeDecl = BuildGlobalType(moduleDeclHolder, typeDeclElem.TypeDecl);
                        globalTypeDecls.Add(globalTypeDecl);
                        break;

                    case S.GlobalFuncDeclScriptElement globalFuncDeclElem:
                        var globalFuncDecl = BuildGlobalFunc(moduleDeclHolder, globalFuncDeclElem.FuncDecl);
                        globalFuncDecls.Add(globalFuncDecl);
                        break;

                    case S.StmtScriptElement:
                        // skip
                        break;
                }
            }

            var moduleDecl = new ModuleDeclSymbol(moduleName, namespaceDecls.ToImmutable(), globalTypeDecls.ToImmutable(), globalFuncDecls.ToImmutable());
            moduleDeclHolder.SetValue(moduleDecl);
            return moduleDecl;
        }        
        
        #region Struct

        M.AccessModifier MakeStructMemberAccessModifier(S.AccessModifier? accessModifier) // throws FatalException
        {
            return accessModifier switch
            {
                null => M.AccessModifier.Public,
                S.AccessModifier.Private => M.AccessModifier.Private,
                _ => throw new NotImplementedException() // 에러처리
            };
        }

        StructMemberTypeDeclSymbol BuildStructMemberType(IHolder<StructDeclSymbol> outerHolder, S.StructMemberTypeDecl decl)
        {
            var type = BuildType(outerHolder, decl.TypeDecl);
            var syntaxAccessModifier = GetAccessModifier(decl.TypeDecl);
            var accessModifier = MakeStructMemberAccessModifier(syntaxAccessModifier);

            return new StructMemberTypeDeclSymbol(accessModifier, type);
        }        

        StructMemberFuncDeclSymbol BuildStructMemberFunc(IHolder<StructDeclSymbol> outerHolder, S.StructMemberFuncDecl decl)
        {
            var accessModifier = MakeStructMemberAccessModifier(decl.AccessModifier);
            var returnHolder = new Holder<FuncReturn>();
            var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

            RegisterAfterBuild(default, context =>
            {
                var retType = context.GetTypeSymbolNode(decl.RetType);
                returnHolder.SetValue(new FuncReturn(decl.IsRefReturn, retType));

                var parameters = MakeParameters(context, decl.Parameters);
                parametersHolder.SetValue(parameters);
            });

            return new StructMemberFuncDeclSymbol(outerHolder, accessModifier, decl.IsStatic, returnHolder, new M.Name.Normal(decl.Name), decl.TypeParams, parametersHolder);
        }        

        // 이 노드에서는 var가 여러개 나올수 있으므로 리턴처리하지 않고, 안에서 직접 추가한다
        void BuildStructMemberVar(ImmutableArray<StructMemberVarDeclSymbol>.Builder builder, IHolder<StructDeclSymbol> outerHolder, S.StructMemberVarDecl decl)
        {
            // 아래 값들은 StructMemberVarDeclSymbol들끼리 공유한다
            var accessModifier = MakeStructMemberAccessModifier(decl.AccessModifier);
            var declTypeHolder = new Holder<ITypeSymbolNode>();

            RegisterAfterBuild(default, context =>
            {
                var declType = context.GetTypeSymbolNode(decl.VarType);
                declTypeHolder.SetValue(declType);
            });

            foreach (var name in decl.VarNames)
            {
                // TODO: var decl static지원
                var symbol = new StructMemberVarDeclSymbol(outerHolder, accessModifier, false, declTypeHolder, new M.Name.Normal(name));
                builder.Add(symbol);
            }
        }

        StructConstructorDeclSymbol BuildStructConstructor(IHolder<StructDeclSymbol> outerHolder, S.StructConstructorDecl decl)
        {
            var accessModifier = MakeStructMemberAccessModifier(decl.AccessModifier);
            var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

            RegisterAfterBuild(default, context =>
            {
                var parameters = MakeParameters(context, decl.Parameters);
                parametersHolder.SetValue(parameters);
            });

            // TODO: 타이프 쳐서 만들어진 constructor는 'trivial' 표시를 하기 전까지는 trivial로 인식하지 않는다 (하위 타입의 trivial constructor가 이 constructor를 참조하지 않는다)
            return new StructConstructorDeclSymbol(outerHolder, accessModifier, parametersHolder, bTrivial: false);
        }

        // NOTICE: comparing with baseConstructor and constructor'Decl'
        static bool IsMatchStructTrivialConstructorParameters(StructConstructorSymbol? baseConstructor, StructConstructorDeclSymbol constructorDecl, ImmutableArray<StructMemberVarDeclSymbol> memberVars)
        {
            int baseParamCount = baseConstructor != null ? baseConstructor.GetParameterCount() : 0;
            int paramCount = constructorDecl.GetParameterCount();

            if (memberVars.Length != paramCount) return false;

            // constructorDecl의 앞부분이 baseConstructor와 일치하는지를 봐야 한다
            for (int i = 0; i < baseParamCount; i++)
            {
                Debug.Assert(baseConstructor != null);

                var baseParameter = baseConstructor.GetParameter(i);
                var parameter = constructorDecl.GetParameter(i);

                if (!baseParameter.Equals(parameter)) return false;

                // 추가 조건, normal로만 자동으로 생성한다
                if (baseParameter.Kind != FuncParameterKind.Default ||
                    parameter.Kind != FuncParameterKind.Default) return false;
            }

            // baseParam을 제외한 뒷부분이 memberVarType과 맞는지 봐야 한다
            for (int i = 0; i < paramCount; i++)
            {
                var memberVarType = memberVars[i].GetDeclType();
                var parameter = constructorDecl.GetParameter(i + baseParamCount);

                // 타입을 비교해서 같지 않다면 제외
                if (!parameter.Type.Equals(memberVarType)) return false;

                // 기본으로만 자동으로 생성한다
                if (parameter.Kind != FuncParameterKind.Default) return false;
            }

            return true;
        }

        static StructConstructorDeclSymbol? GetStructConstructorHasSameParamWithTrivial(
            StructConstructorSymbol? baseConstructor,
            ImmutableArray<StructConstructorDeclSymbol>.Builder constructors,
            ImmutableArray<StructMemberVarDeclSymbol> memberVars)
        {
            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            foreach (var constructor in constructors.AsEnumerable())
            {
                if (IsMatchStructTrivialConstructorParameters(baseConstructor, constructor, memberVars))
                    return constructor;
            }

            return null;
        }

        // baseConstructor가 만들어졌다는 가정하에 짜여짐 (인자에서 요구하는 형태)
        static StructConstructorDeclSymbol MakeStructTrivialConstructorDecl(
            IHolder<StructDeclSymbol> outer,
            StructConstructorSymbol? baseConstructor,
            ImmutableArray<StructMemberVarDeclSymbol> memberVars)
        {
            int totalParamCount = (baseConstructor?.GetParameterCount() ?? 0) + memberVars.Length;
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(totalParamCount);

            // to prevent conflict between parameter names, using special name $'base'_<name>_index
            // class A { A(int x) {} }
            // class B : A { B(int $base_x0, int x) : base($base_x0) { } }
            // class C : B { C(int $base_x0, int $base_x1, int x) : base($base_x0, $base_x1) { } }
            if (baseConstructor != null)
            {
                for (int i = 0; i < baseConstructor.GetParameterCount(); i++)
                {
                    var baseParam = baseConstructor.GetParameter(i);

                    // 이름 보정, base로 가는 파라미터들은 다 이름이 ConstructorParam이다.
                    var newBaseParam = new FuncParameter(baseParam.Kind, baseParam.Type, new M.Name.ConstructorParam(i));
                    builder.Add(newBaseParam);
                }
            }

            foreach (var memberVar in memberVars)
            {
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new FuncParameter(FuncParameterKind.Default, type, name);
                builder.Add(param);
            }

            // trivial constructor를 만듭니다
            return new StructConstructorDeclSymbol(outer, M.AccessModifier.Public, new Holder<ImmutableArray<FuncParameter>>(builder.MoveToImmutable()), true);
        }

        StructDeclSymbol BuildStruct(IHolder<IDeclSymbolNode> outerHolder, S.StructDecl decl)
        {
            var structHolder = new Holder<StructDeclSymbol>();
            var baseStructHolder = new Holder<StructSymbol?>();
            var constructorsHolder = new Holder<ImmutableArray<StructConstructorDeclSymbol>>();
            var trivialConstructorHolder = new Holder<StructConstructorDeclSymbol?>();

            var memberTypeDeclsBuilder = ImmutableArray.CreateBuilder<StructMemberTypeDeclSymbol>();
            var memberFuncDeclsBuilder = ImmutableArray.CreateBuilder<StructMemberFuncDeclSymbol>();
            var memberVarDeclsBuilder = ImmutableArray.CreateBuilder<StructMemberVarDeclSymbol>();
            var constructorDeclsBuilder = ImmutableArray.CreateBuilder<StructConstructorDeclSymbol>();

            // 임시, baseType은 한개 (syntax에는 여러개를 받을 수 있게 되어있으므로 하나로 변경한다
            Debug.Assert(decl.BaseTypes.Length <= 1);

            // base 다음에 오도록
            S.TypeExp? baseStruct = decl.BaseTypes.Length == 1 ? decl.BaseTypes[0] : null;
            ImmutableArray<DeclSymbolId> dependsOnSymbolIds = default;
            if (baseStruct != null)
            {
                var baseStructInfo = typeExpInfoService.GetTypeExpInfo(baseStruct);
                if (baseStructInfo.GetSymbolId() is ModuleSymbolId baseTypeModuleSymbolId)
                {
                    var baseTypeDeclSymbolId = baseTypeModuleSymbolId.GetDeclSymbolId();
                    dependsOnSymbolIds = Arr(baseTypeDeclSymbolId);
                }
                else
                {
                    Debug.Assert(false);
                }
            }
            
            foreach(var memberDecl in decl.MemberDecls)
            {
                switch(memberDecl)
                {
                    case S.StructMemberTypeDecl typeDecl:
                        {
                            var symbol = BuildStructMemberType(structHolder, typeDecl);
                            memberTypeDeclsBuilder.Add(symbol);
                            break;
                        }

                    case S.StructMemberFuncDecl funcDecl:
                        {
                            var symbol = BuildStructMemberFunc(structHolder, funcDecl);
                            memberFuncDeclsBuilder.Add(symbol);
                            break;
                        }

                    case S.StructMemberVarDecl varDecl:
                        {
                            BuildStructMemberVar(memberVarDeclsBuilder, structHolder, varDecl);                            
                            break;
                        }

                    case S.StructConstructorDecl constructorDecl:
                        {
                            var symbol = BuildStructConstructor(structHolder, constructorDecl);
                            constructorDeclsBuilder.Add(symbol);
                            break;
                        }

                    default:
                        throw new UnreachableCodeException();
                }
            }

            var memberVarDecls = memberVarDeclsBuilder.ToImmutable();

            RegisterAfterBuild(dependsOnSymbolIds, context =>
            {
                StructSymbol? baseStructSymbol = null;
                if (baseStruct != null)
                {
                    baseStructSymbol = context.GetTypeSymbolNode(baseStruct) as StructSymbol;
                    Debug.Assert(baseStructSymbol != null);
                }

                baseStructHolder.SetValue(baseStructSymbol);
                var baseTrivialConstructor = baseStructSymbol?.GetTrivialConstructor();
                StructConstructorDeclSymbol? trivialConstructor = null;

                // baseStruct가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
                // baseStruct가 있고, TrivialConstructor가 있는 경우 => 진행
                // baseStruct가 없는 경우 => 없이 만들고 진행 
                if (baseTrivialConstructor != null || baseStructSymbol == null)
                {
                    // 같은 인자의 생성자가 없으면 Trivial을 만든다
                    if (GetStructConstructorHasSameParamWithTrivial(baseTrivialConstructor, constructorDeclsBuilder, memberVarDecls) == null)
                    {
                        trivialConstructor = MakeStructTrivialConstructorDecl(structHolder, baseTrivialConstructor, memberVarDecls);
                        constructorDeclsBuilder.Add(trivialConstructor);
                    }
                }

                // TODO: constructors, trivialConstructors
                constructorsHolder.SetValue(constructorDeclsBuilder.ToImmutable());
                trivialConstructorHolder.SetValue(trivialConstructor);
            });

            var @struct = new StructDeclSymbol(outerHolder, new M.Name.Normal(decl.Name), decl.TypeParams, baseStructHolder,
                memberTypeDeclsBuilder.ToImmutable(), memberFuncDeclsBuilder.ToImmutable(), memberVarDecls, 
                constructorsHolder, trivialConstructorHolder);
            structHolder.SetValue(@struct);
            return @struct;
        }

        #endregion

        #region Class

        static bool IsMatchClassTrivialConstructorParameters(ClassConstructorSymbol? baseConstructor, ClassConstructorDeclSymbol constructorDecl, ImmutableArray<ClassMemberVarDeclSymbol> memberVars)
        {
            int baseParamCount = baseConstructor != null ? baseConstructor.GetParameterCount() : 0;
            int paramCount = constructorDecl.GetParameterCount();

            if (memberVars.Length != paramCount) return false;

            // constructorDecl의 앞부분이 baseConstructor와 일치하는지를 봐야 한다
            for (int i = 0; i < baseParamCount; i++)
            {
                Debug.Assert(baseConstructor != null);

                var baseParameter = baseConstructor.GetParameter(i);
                var parameter = constructorDecl.GetParameter(i);

                if (!baseParameter.Equals(parameter)) return false;

                // 추가 조건, normal로만 자동으로 생성한다
                if (baseParameter.Kind != FuncParameterKind.Default ||
                    parameter.Kind != FuncParameterKind.Default) return false;
            }

            // baseParam을 제외한 뒷부분이 memberVarType과 맞는지 봐야 한다
            for (int i = 0; i < paramCount; i++)
            {
                var memberVarType = memberVars[i].GetDeclType();
                var parameter = constructorDecl.GetParameter(i + baseParamCount);

                // 타입을 비교해서 같지 않다면 제외
                if (!parameter.Type.Equals(memberVarType)) return false;

                // 기본으로만 자동으로 생성한다
                if (parameter.Kind != FuncParameterKind.Default) return false;
            }

            return true;
        }

        // baseConstructor가 만들어졌다는 가정하에 짜여짐 (인자에서 요구하는 형태)
        static ClassConstructorDeclSymbol MakeClassTrivialConstructorDecl(
            IHolder<ClassDeclSymbol> outer,
            ClassConstructorSymbol? baseConstructor,
            ImmutableArray<ClassMemberVarDeclSymbol> memberVars)
        {
            int totalParamCount = (baseConstructor?.GetParameterCount() ?? 0) + memberVars.Length;
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(totalParamCount);

            // to prevent conflict between parameter names, using special name $'base'_<name>_index
            // class A { A(int x) {} }
            // class B : A { B(int $base_x0, int x) : base($base_x0) { } }
            // class C : B { C(int $base_x0, int $base_x1, int x) : base($base_x0, $base_x1) { } }
            if (baseConstructor != null)
            {
                for (int i = 0; i < baseConstructor.GetParameterCount(); i++)
                {
                    var baseParam = baseConstructor.GetParameter(i);

                    // 이름 보정, base로 가는 파라미터들은 다 이름이 ConstructorParam이다.
                    var newBaseParam = new FuncParameter(baseParam.Kind, baseParam.Type, new M.Name.ConstructorParam(i));
                    builder.Add(newBaseParam);
                }
            }

            foreach (var memberVar in memberVars)
            {
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new FuncParameter(FuncParameterKind.Default, type, name);
                builder.Add(param);
            }

            // trivial constructor를 만듭니다
            return new ClassConstructorDeclSymbol(outer, M.AccessModifier.Public, new Holder<ImmutableArray<FuncParameter>>(builder.MoveToImmutable()), true);
        }

        static ClassConstructorDeclSymbol? GetClassConstructorHasSameParamWithTrivial(
            ClassConstructorSymbol? baseConstructor,
            ImmutableArray<ClassConstructorDeclSymbol>.Builder constructors,
            ImmutableArray<ClassMemberVarDeclSymbol> memberVars)
        {
            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            foreach (var constructor in constructors.AsEnumerable())
            {
                if (IsMatchClassTrivialConstructorParameters(baseConstructor, constructor, memberVars))
                    return constructor;
            }

            return null;
        }

        static M.AccessModifier MakeClassMemberAccessModifier(S.AccessModifier? accessModifier) // throws FatalException
        {
            return accessModifier switch
            {
                null => M.AccessModifier.Private,
                S.AccessModifier.Public => M.AccessModifier.Public,
                _ => throw new NotImplementedException() // 에러처리
            };
        }

        ClassMemberTypeDeclSymbol BuildClassMemberType(IHolder<ClassDeclSymbol> outerHolder, S.ClassMemberTypeDecl decl)
        {
            var type = BuildType(outerHolder, decl.TypeDecl);
            var syntaxAccessModifier = GetAccessModifier(decl.TypeDecl);
            var accessModifier = MakeClassMemberAccessModifier(syntaxAccessModifier);

            return new ClassMemberTypeDeclSymbol(accessModifier, type);
        }

        ClassMemberFuncDeclSymbol BuildClassMemberFunc(IHolder<ClassDeclSymbol> outerHolder, S.ClassMemberFuncDecl decl)
        {
            var accessModifier = MakeClassMemberAccessModifier(decl.AccessModifier);
            var returnHolder = new Holder<FuncReturn>();
            var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

            RegisterAfterBuild(default, context =>
            {
                var retType = context.GetTypeSymbolNode(decl.RetType);
                returnHolder.SetValue(new FuncReturn(decl.IsRefReturn, retType));

                var parameters = MakeParameters(context, decl.Parameters);
                parametersHolder.SetValue(parameters);
            });

            return new ClassMemberFuncDeclSymbol(outerHolder, accessModifier, returnHolder, new M.Name.Normal(decl.Name), decl.TypeParams, parametersHolder, decl.IsStatic);
        }

        void BuildClassMemberVar(ImmutableArray<ClassMemberVarDeclSymbol>.Builder builder, IHolder<ClassDeclSymbol> outerHolder, S.ClassMemberVarDecl decl)
        {            
            var accessModifier = MakeClassMemberAccessModifier(decl.AccessModifier);

            // 아래 레퍼런스는 ClassMemberVarDeclSymbol들끼리 공유한다
            var declTypeHolder = new Holder<ITypeSymbolNode>();

            RegisterAfterBuild(default, context =>
            {
                var declType = context.GetTypeSymbolNode(decl.VarType);
                declTypeHolder.SetValue(declType);
            });

            foreach (var name in decl.VarNames)
            {
                // TODO: bStatic 지원
                var varDecl = new ClassMemberVarDeclSymbol(outerHolder, accessModifier, bStatic: false, declTypeHolder, new M.Name.Normal(name));
                builder.Add(varDecl);
            }
        }

        ClassConstructorDeclSymbol BuildClassConstructor(IHolder<ClassDeclSymbol> outerHolder, S.ClassConstructorDecl decl)
        {
            var accessModifier = MakeClassMemberAccessModifier(decl.AccessModifier);
            var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

            RegisterAfterBuild(default, context =>
            {
                var parameters = MakeParameters(context, decl.Parameters);
                parametersHolder.SetValue(parameters);
            });

            // TODO: trivial 마킹하면 검사하고 trivial로 만든다
            return new ClassConstructorDeclSymbol(outerHolder, accessModifier, parametersHolder, bTrivial: false);
        }

        ClassDeclSymbol BuildClass(IHolder<IDeclSymbolNode> outerHolder, S.ClassDecl decl)
        {
            var classHolder = new Holder<ClassDeclSymbol>();

            var baseClassHolder = new Holder<ClassSymbol?>();
            var interfacesHolder = new Holder<ImmutableArray<InterfaceSymbol>>();
            var typesBuilder = ImmutableArray.CreateBuilder<ClassMemberTypeDeclSymbol>();
            var funcsBuilder = ImmutableArray.CreateBuilder<ClassMemberFuncDeclSymbol>();
            var varsBuilder = ImmutableArray.CreateBuilder<ClassMemberVarDeclSymbol>();
            var constructorsBuilder = ImmutableArray.CreateBuilder<ClassConstructorDeclSymbol>();
            var constructorsHolder = new Holder<ImmutableArray<ClassConstructorDeclSymbol>>();
            var trivialConstructorHolder = new Holder<ClassConstructorDeclSymbol?>();

            foreach(var member in decl.MemberDecls)
            {
                switch(member)
                {
                    case S.ClassMemberTypeDecl typeMember:
                        {
                            var type = BuildClassMemberType(classHolder, typeMember);
                            typesBuilder.Add(type);
                            break;
                        }

                    case S.ClassMemberFuncDecl funcMember:
                        {
                            var func = BuildClassMemberFunc(classHolder, funcMember);
                            funcsBuilder.Add(func);
                            break;
                        }

                    case S.ClassMemberVarDecl varMember:
                        {
                            // 여러개가 생길 수 있어서 직접 넣는다
                            BuildClassMemberVar(varsBuilder, classHolder, varMember);
                            break;
                        }

                    case S.ClassConstructorDecl constructorMember:
                        {
                            BuildClassConstructor(classHolder, constructorMember);
                            break;
                        }
                }
            }

            var memberVars = varsBuilder.ToImmutable();

            var baseInterfaceInfos = ImmutableArray.CreateBuilder<TypeExpInfo>();             
            IdentifyBaseTypes(out var baseClassInfo, baseInterfaceInfos, decl);

            var dependsOnSymbolIds = MakeClassBuildingDependencies(baseClassInfo);

            // 
            RegisterAfterBuild(dependsOnSymbolIds, context =>
            {
                ClassSymbol? baseClass = null;
                if (baseClassInfo != null)
                {
                    baseClass = context.GetTypeSymbolNode(baseClassInfo.GetTypeExp()) as ClassSymbol;
                    Debug.Assert(baseClass != null);
                }

                baseClassHolder.SetValue(baseClass);
                var baseTrivialConstructor = baseClass?.GetTrivialConstructor();
                ClassConstructorDeclSymbol? trivialConstructor = null;

                // baseStruct가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
                // baseStruct가 있고, TrivialConstructor가 있는 경우 => 진행
                // baseStruct가 없는 경우 => 없이 만들고 진행 
                if (baseTrivialConstructor != null || baseClass == null)
                {
                    // 같은 인자의 생성자가 없으면 Trivial을 만든다
                    if (GetClassConstructorHasSameParamWithTrivial(baseTrivialConstructor, constructorsBuilder, memberVars) == null)
                    {
                        trivialConstructor = MakeClassTrivialConstructorDecl(classHolder, baseTrivialConstructor, memberVars);
                        constructorsBuilder.Add(trivialConstructor);
                    }
                }

                // TODO: constructors, trivialConstructors
                constructorsHolder.SetValue(constructorsBuilder.ToImmutable());
                trivialConstructorHolder.SetValue(trivialConstructor);
            });

            var @class = new ClassDeclSymbol(outerHolder, new M.Name.Normal(decl.Name), decl.TypeParams, baseClassHolder, interfacesHolder, 
                typesBuilder.ToImmutable(), funcsBuilder.ToImmutable(), memberVars, constructorsHolder, trivialConstructorHolder);
            classHolder.SetValue(@class);
            return @class;
        }

        void IdentifyBaseTypes(out TypeExpInfo? baseClassInfo, ImmutableArray<TypeExpInfo>.Builder baseInterfacInfos, S.ClassDecl decl)
        {
            var baseClassInfos = new Candidates<TypeExpInfo>();

            // TODO: 이 작업은 밖에서 해야 할 거 같기도 하다
            foreach (var baseType in decl.BaseTypes)
            {
                var info = typeExpInfoService.GetTypeExpInfo(baseType);
                switch (info.GetKind())
                {
                    case TypeExpInfoKind.Class:
                        baseClassInfos.Add(info);
                        break;

                    case TypeExpInfoKind.Interface:
                        baseInterfacInfos.Add(info);
                        break;

                    default:
                        // 에러 처리                        
                        throw new NotImplementedException();
                }
            }

            baseClassInfo = baseClassInfos.GetSingle();
            if (baseClassInfo == null)
            {
                if (baseClassInfos.HasMultiple)
                {
                    // 에러 처리
                    throw new NotImplementedException();
                }

                // 비어있는건 괜찮다
                Debug.Assert(baseClassInfos.IsEmpty);                
            }
        }

        ImmutableArray<DeclSymbolId> MakeClassBuildingDependencies(TypeExpInfo? baseClassInfo) // throw 
        {
            if (baseClassInfo != null)
            { 
                if (baseClassInfo.GetSymbolId() is ModuleSymbolId baseClassModuleSymbolId)
                {
                    var baseClassDeclSymbolId = baseClassModuleSymbolId.GetDeclSymbolId();
                    return Arr(baseClassDeclSymbolId);
                }
                else
                {
                    // 에러 처리, 특수타입으로 부터 상속받을 수 없습니다
                    throw new NotImplementedException();
                }
            }

            return default;
        }

        #endregion

        #region Enum

        EnumElemMemberVarDeclSymbol BuildEnumElemMemberVar(IHolder<EnumElemDeclSymbol> outerHolder, S.EnumElemMemberVarDecl decl)
        {
            var declTypeHolder = new Holder<ITypeSymbolNode>();

            RegisterAfterBuild(default, context =>
            {
                var declType = context.GetTypeSymbolNode(decl.Type);
                declTypeHolder.SetValue(declType);
            });

            return new EnumElemMemberVarDeclSymbol(outerHolder, declTypeHolder, new M.Name.Normal(decl.Name));
        }

        EnumElemDeclSymbol BuildEnumElem(IHolder<EnumDeclSymbol> outerHolder, S.EnumElemDecl decl)
        {
            var enumElemHolder = new Holder<EnumElemDeclSymbol>();
            var builder = ImmutableArray.CreateBuilder<EnumElemMemberVarDeclSymbol>(decl.MemberVars.Length);

            foreach(var memberVar in decl.MemberVars)
            {
                var symbol = BuildEnumElemMemberVar(enumElemHolder, memberVar);
                builder.Add(symbol);
            }

            var enumElem = new EnumElemDeclSymbol(outerHolder, new M.Name.Normal(decl.Name), builder.MoveToImmutable());
            enumElemHolder.SetValue(enumElem);
            return enumElem;
        }

        EnumDeclSymbol BuildEnum(IHolder<IDeclSymbolNode> outerHolder, S.EnumDecl decl)
        {
            var enumHolder = new Holder<EnumDeclSymbol>();
            var elemsBuilder = ImmutableArray.CreateBuilder<EnumElemDeclSymbol>(decl.Elems.Length);

            foreach(var elem in decl.Elems)
            {
                var elemSymbol = BuildEnumElem(enumHolder, elem);
                elemsBuilder.Add(elemSymbol);
            }

            var @enum = new EnumDeclSymbol(outerHolder, new M.Name.Normal(decl.Name), decl.TypeParams, elemsBuilder.MoveToImmutable());
            enumHolder.SetValue(@enum);
            return @enum;
        }

        #endregion

        #region Global

        M.AccessModifier MakeGlobalAccessModifier(S.AccessModifier? accessModifier) // throws FatalException
        {
            return accessModifier switch
            {
                null => M.AccessModifier.Private,
                S.AccessModifier.Public => M.AccessModifier.Public,
                _ => throw new FatalException()
            };
        }

        GlobalTypeDeclSymbol BuildGlobalType(IHolder<ModuleDeclSymbol> moduleHolder, S.TypeDecl decl) // throw FatalException
        {
            var type = BuildType(moduleHolder, decl);
            var syntaxAccessModifier = GetAccessModifier(decl);

            var accessModifier = MakeGlobalAccessModifier(syntaxAccessModifier);
            return new GlobalTypeDeclSymbol(accessModifier, type);
        }

        GlobalFuncDeclSymbol BuildGlobalFunc(IHolder<ITopLevelDeclSymbolNode> outerHolder, S.GlobalFuncDecl decl) // throw FatalException        
        {
            var returnHolder = new Holder<FuncReturn>();
            var parametersHolder = new Holder<ImmutableArray<FuncParameter>>();

            RegisterAfterBuild(default, context =>
            {
                var retType = context.GetTypeSymbolNode(decl.RetType); // TODO: GetMType대신에 SymbolId
                returnHolder.SetValue(new FuncReturn(decl.IsRefReturn, retType));

                var parameters = MakeParameters(context, decl.Parameters);
                parametersHolder.SetValue(parameters);
            });

            var accessModifier = MakeGlobalAccessModifier(decl.AccessModifier);
            
            return new GlobalFuncDeclSymbol(outerHolder, accessModifier, returnHolder, new M.Name.Normal(decl.Name), decl.TypeParams, parametersHolder, true);
        }

        #endregion
    }
}