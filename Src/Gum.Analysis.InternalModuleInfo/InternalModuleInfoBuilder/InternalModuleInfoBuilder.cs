using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using S = Gum.Syntax;
using M = Gum.CompileTime;
using System.Text;
using Pretune;
using Gum.Analysis;

namespace Gum.Analysis
{
    class InternalModuleTypeInfoBuilderMisc
    {
        static bool IsMatchTrivialConstructorParameters(M.ParamTypes? baseParamTypes, M.ParamTypes paramTypes, ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            int baseParamCount = baseParamTypes?.Length ?? 0;
            int paramCount = paramTypes.Length;
            int totalParamCount = baseParamCount + paramTypes.Length;

            if (memberVars.Length != totalParamCount) return false;

            for( int i = 0; i < baseParamCount; i++)
            {
                // normal로만 자동으로 생성한다
                if (paramTypes[i].Kind != M.ParamKind.Normal) return false;

                var memberVarType = memberVars[i].GetDeclType();
                var paramType = paramTypes[i].Type;

                if (!memberVarType.Equals(paramType)) return false;
            }

            for (int i = 0; i < paramCount; i++)
            {
                // normal로만 자동으로 생성한다
                if (paramTypes[i].Kind != M.ParamKind.Normal) return false;

                var memberVarType = memberVars[i + baseParamCount].GetDeclType();
                var paramType = paramTypes[i].Type;

                if (!memberVarType.Equals(paramType)) return false;
            }

            return true;
        }

        public static IModuleConstructorInfo? GetConstructorHasSameParamWithTrivial(
            IModuleConstructorInfo? baseTrivialConstrutor,
            ImmutableArray<IModuleConstructorInfo> constructors, ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            var baseParamTypes = baseTrivialConstrutor?.GetParamTypes();

            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            foreach (var constructor in constructors)
            {
                if (IsMatchTrivialConstructorParameters(baseParamTypes, constructor.GetParamTypes(), memberVars))
                    return constructor;
            }

            return null;
        }

        public static IModuleConstructorInfo MakeTrivialConstructor(
            IModuleConstructorInfo? baseConstructorInfo,
            ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            var baseConstructorParameters = baseConstructorInfo?.GetParameters();
            var builder = ImmutableArray.CreateBuilder<M.Param>(
                (baseConstructorParameters?.Length ?? 0) + memberVars.Length
            );

            // to prevent conflict between parameter names, using special name $'base'_<name>_index
            // class A { A(int x) {} }
            // class B : A { B(int $base_x0, int x) : base($base_x0) { } }
            // class C : B { C(int $base_x0, int $base_x1, int x) : base($base_x0, $base_x1) { } }
            if (baseConstructorParameters != null)
            {
                int i = 0;
                foreach (var baseParam in baseConstructorParameters)
                {
                    // base로 가는 파라미터들은 다 이름이 base다.
                    var newBaseParam = new M.Param(baseParam.Kind, baseParam.Type, new M.Name.ConstructorParam(i));
                    builder.Add(newBaseParam);
                    i++;
                }
            }

            foreach (var memberVar in memberVars)
            {
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new M.Param(M.ParamKind.Normal, type, name);
                builder.Add(param);
            }

            // trivial constructor를 만듭니다
            return new InternalModuleConstructorInfo(M.AccessModifier.Public, builder.MoveToImmutable());
        }
    }

    // syntax를 다 돌고 난 뒤의 후처리
    class TypeBuilder : IQueryModuleTypeInfo
    {
        Dictionary<ItemPath, InternalModuleClassInfo> classes;
        Dictionary<ItemPath, InternalModuleStructInfo> structs;

        public TypeBuilder()
        {
            classes = new Dictionary<ItemPath, InternalModuleClassInfo>();
            structs = new Dictionary<ItemPath, InternalModuleStructInfo>();
        }

        public void AddClass(ItemPath path, InternalModuleClassInfo classInfo)
        {
            classes.Add(path, classInfo);
        }

        public void AddStruct(ItemPath path, InternalModuleStructInfo structInfo)
        {
            structs.Add(path, structInfo);
        }

        IModuleClassInfo IQueryModuleTypeInfo.GetClass(ItemPath path)
        {
            // TODO: external 처리
            return classes[path];
        }

        IModuleStructInfo IQueryModuleTypeInfo.GetStruct(ItemPath path)
        {
            return structs[path];
        }
        
        public void SetBasesAndBuildTrivialConstructors(ItemValueFactory itemValueFactory)
        {
            foreach(IModuleClassInfo c in classes.Values)
                c.SetBaseAndBuildTrivialConstructor(this, itemValueFactory);

            foreach (IModuleStructInfo s in structs.Values)
                s.SetBaseAndBuildTrivialConstructor(this, itemValueFactory);
        }
    }

    // Script에서 ModuleInfo 정보를 뽑는 역할    
    public partial struct InternalModuleInfoBuilder
    {
        TypeExpInfoService typeExpInfoService;
        TypeBuilder typeBuilder;
        List<IModuleTypeInfo> types;
        List<IModuleFuncInfo> funcs;            
            
        public static (InternalModuleInfo, ItemValueFactory, GlobalItemValueFactory) Build(
            M.ModuleName moduleName, 
            S.Script script, 
            TypeExpInfoService typeExpInfoService,
            ExternalModuleInfoRepository externalModuleInfoRepo)
        {
            var types = new List<IModuleTypeInfo>();
            var funcs = new List<IModuleFuncInfo>();
            var typeBuilder = new TypeBuilder(); // 만들어진 타입 정보에 후처리 정보 추가
            var builder = new InternalModuleInfoBuilder(typeExpInfoService, typeBuilder, types, funcs);
            
            // 순회
            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case S.TypeDeclScriptElement typeDeclElem:
                        builder.BuildTypeDecl(moduleName, typeDeclElem.TypeDecl);
                        break;

                    case S.GlobalFuncDeclScriptElement globalFuncDeclElem:                        
                        builder.BuildFuncDecl(globalFuncDeclElem.FuncDecl);
                        break;
                    
                    case S.StmtScriptElement:
                        // skip
                        break;
                }
            }

            var internalModuleInfo = new InternalModuleInfo(moduleName, types.ToImmutableArray(), funcs.ToImmutableArray());
            var typeInfoRepo = new TypeInfoRepository(internalModuleInfo, externalModuleInfoRepo);
            var globalItemValueFactory = new GlobalItemValueFactory(internalModuleInfo, externalModuleInfoRepo);
            var ritemFactory = new RItemFactory();
            var itemValueFactory = new ItemValueFactory(typeInfoRepo, ritemFactory);

            // 후처리
            typeBuilder.SetBasesAndBuildTrivialConstructors(itemValueFactory);

            return (internalModuleInfo, itemValueFactory, globalItemValueFactory);
        }

        InternalModuleInfoBuilder(TypeExpInfoService typeExpInfoService, TypeBuilder typeBuilder, List<IModuleTypeInfo> types, List<IModuleFuncInfo> funcs)
        {
            this.typeExpInfoService = typeExpInfoService;
            this.typeBuilder = typeBuilder;
            this.types = types;
            this.funcs = funcs;
        }

        // 여긴 Global
        void BuildTypeDecl(M.ModuleName moduleName, S.TypeDecl typeDecl)
        {
            IntermediateTypeBuilder.BuildType(typeBuilder, types, typeExpInfoService, new RootItemPath(moduleName), typeDecl);
        }

        void BuildFuncDecl(S.GlobalFuncDecl funcDecl)
        {
            var retType = GetMType(typeExpInfoService, funcDecl.RetType);
            if (retType == null) throw new FatalException();

            var paramInfo = MakeParams(typeExpInfoService, funcDecl.Parameters);

            M.AccessModifier accessModifier;
            switch (funcDecl.AccessModifier)
            {
                case null: accessModifier = M.AccessModifier.Private; break;
                case S.AccessModifier.Public: accessModifier = M.AccessModifier.Public; break;
                case S.AccessModifier.Protected: throw new FatalException();
                case S.AccessModifier.Private: throw new FatalException();
                default: throw new UnreachableCodeException();
            }

            var funcInfo = new InternalModuleFuncInfo(
                accessModifier,
                bInstanceFunc: false,
                bSeqFunc: funcDecl.IsSequence,
                bRefReturn: funcDecl.IsRefReturn,
                retType,
                new M.Name.Normal(funcDecl.Name),
                funcDecl.TypeParams,
                paramInfo
            );

            funcs.Add(funcInfo);
        }

        static M.Type? GetMType(TypeExpInfo typeExpInfo)
        {
            switch (typeExpInfo)
            {
                case MTypeTypeExpInfo mtypeTypeExpInfo:
                    return mtypeTypeExpInfo.Type;
            }

            return null;
        }

        static M.Type? GetMType(TypeExpInfoService typeExpInfoService, S.TypeExp typeExp)
        {
            var typeExpInfo = typeExpInfoService.GetTypeExpInfo(typeExp);
            return GetMType(typeExpInfo);
        }
        
        static ImmutableArray<M.Param> MakeParams(TypeExpInfoService typeExpInfoService, ImmutableArray<S.FuncParam> sparams)
        {
            var builder = ImmutableArray.CreateBuilder<M.Param>(sparams.Length);
            foreach(var sparam in sparams)
            {
                var mtype = GetMType(typeExpInfoService, sparam.Type);
                if (mtype == null) throw new FatalException();

                M.ParamKind paramKind = sparam.Kind switch
                {
                    S.FuncParamKind.Normal => M.ParamKind.Normal,
                    S.FuncParamKind.Params => M.ParamKind.Params,
                    S.FuncParamKind.Ref => M.ParamKind.Ref,
                    _ => throw new UnreachableCodeException()
                };
                
                builder.Add(new M.Param(paramKind, mtype, new M.Name.Normal(sparam.Name)));
            }

            return builder.MoveToImmutable();
        }
        
        partial struct IntermediateTypeBuilder
        {
            public static void BuildType(TypeBuilder typeBuilder, List<IModuleTypeInfo> types, TypeExpInfoService typeExpInfoService, ItemPath parentPath, S.TypeDecl typeDecl)
            {
                switch (typeDecl)
                {
                    case S.StructDecl structDecl:                        
                        IntermediateStructBuilder.Build(typeBuilder, types, typeExpInfoService, parentPath, structDecl);
                        break;

                    case S.ClassDecl classDecl:                        
                        IntermediateClassBuilder.Build(typeBuilder, types, typeExpInfoService, parentPath, classDecl);
                        break;

                    case S.EnumDecl enumDecl:
                        IntermediateEnumBuilder.Build(types, typeExpInfoService, enumDecl);                        
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }
        }
        
        partial struct IntermediateEnumBuilder
        {
            public static void Build(
                List<IModuleTypeInfo> types,
                TypeExpInfoService typeExpInfoService, 
                S.EnumDecl enumDecl)
            {
                var elemsBuilder = ImmutableArray.CreateBuilder<InternalModuleEnumElemInfo>(enumDecl.Elems.Length);
                foreach (var elem in enumDecl.Elems)
                {
                    var fieldsBuilder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(elem.Fields.Length);
                    foreach (var field in elem.Fields)
                    {
                        var type = GetMType(typeExpInfoService, field.Type);
                        Debug.Assert(type != null);

                        var mfield = new InternalModuleMemberVarInfo(M.AccessModifier.Public, false, type, new M.Name.Normal(field.Name));
                        fieldsBuilder.Add(mfield);
                    }

                    var fields = fieldsBuilder.MoveToImmutable();
                    var enumElemInfo = new InternalModuleEnumElemInfo(new M.Name.Normal(elem.Name), fields);
                    elemsBuilder.Add(enumElemInfo);
                }

                var elems = elemsBuilder.MoveToImmutable();
                var enumInfo = new InternalModuleEnumInfo(new M.Name.Normal(enumDecl.Name), enumDecl.TypeParams, elems);
                types.Add(enumInfo);
            }
        }

        [AutoConstructor]
        partial struct IntermediateClassBuilder
        {
            TypeExpInfoService typeExpInfoService;
            List<IModuleFuncInfo> funcs;
            List<IModuleConstructorInfo> constructors;
            List<IModuleMemberVarInfo> memberVars;

            public static void Build(
                TypeBuilder typeBuilder,
                List<IModuleTypeInfo> types,
                TypeExpInfoService typeExpInfoService, 
                ItemPath parentPath, S.ClassDecl classDecl)
            {
                var classPath = parentPath.Child(new M.Name.Normal(classDecl.Name), classDecl.TypeParams.Length);
                var nestedTypes = new List<IModuleTypeInfo>();
                var nestedFuncs = new List<IModuleFuncInfo>();
                var constructors = new List<IModuleConstructorInfo>();
                var memberVars = new List<IModuleMemberVarInfo>();
                var builder = new IntermediateClassBuilder(typeExpInfoService, nestedFuncs, constructors, memberVars);

                // base & interfaces
                var baseTypeCandidates = new Candidates<M.Type>();
                var interfacesBuilder = ImmutableArray.CreateBuilder<M.Type>();
                foreach (var baseType in classDecl.BaseTypes)
                {
                    var baseTypeExpInfo = typeExpInfoService.GetTypeExpInfo(baseType);
                    switch (baseTypeExpInfo.GetKind())
                    {
                        case TypeExpInfoKind.Class:
                            {
                                var baseTypeCandidate = baseTypeExpInfo.GetMType();
                                if (baseTypeCandidate == null) throw new FatalException();

                                baseTypeCandidates.Add(baseTypeCandidate);
                                break;
                            }

                        case TypeExpInfoKind.Interface:
                            {
                                var minterfaceType = baseTypeExpInfo.GetMType();
                                if (minterfaceType == null) throw new FatalException();

                                interfacesBuilder.Add(minterfaceType);
                                break;
                            }

                        default:
                            throw new FatalException();
                    }
                }

                var baseTypeExpInfoResult = baseTypeCandidates.GetSingle();
                if (baseTypeExpInfoResult == null && baseTypeCandidates.HasMultiple)
                    throw new FatalException(); // TODO: 에러 처리
                Debug.Assert(baseTypeExpInfoResult != null || baseTypeCandidates.IsEmpty);

                foreach (var elem in classDecl.MemberDecls)
                {
                    switch (elem)
                    {
                        case S.ClassMemberFuncDecl funcDecl:
                            builder.VisitClassMemberFuncDecl(funcDecl);
                            break;

                        case S.ClassMemberTypeDecl typeDecl:
                            IntermediateTypeBuilder.BuildType(typeBuilder, nestedTypes, typeExpInfoService, classPath, typeDecl.TypeDecl);
                            break;

                        case S.ClassMemberVarDecl varDecl:
                            builder.VisitClassMemberVarDecl(varDecl);
                            break;

                        case S.ClassConstructorDecl constructorDecl:
                            builder.VisitClassConstructorDeclElement(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }                

                // 자식 type들에 dependency가 걸린다
                var classInfo = new InternalModuleClassInfo(
                    new M.Name.Normal(classDecl.Name), classDecl.TypeParams, baseTypeExpInfoResult, interfacesBuilder.ToImmutable(), 
                    nestedTypes.ToImmutableArray(),
                    nestedFuncs.ToImmutableArray(), 
                    constructors.ToImmutableArray(), 
                    memberVars.ToImmutableArray());

                typeBuilder.AddClass(classPath, classInfo);
                types.Add(classInfo);
            }

            void VisitClassMemberFuncDecl(S.ClassMemberFuncDecl funcDecl)
            {
                bool bThisCall = true; // TODO: static 키워드가 추가되면 고려하도록 한다

                var retType = GetMType(typeExpInfoService, funcDecl.RetType);
                if (retType == null) throw new FatalException();

                var paramInfo = MakeParams(typeExpInfoService, funcDecl.Parameters);

                M.AccessModifier accessModifier;
                switch (funcDecl.AccessModifier)
                {
                    // default는 private이다
                    case null: accessModifier = M.AccessModifier.Private; break;
                    case S.AccessModifier.Public: accessModifier = M.AccessModifier.Public; break;
                    case S.AccessModifier.Protected: accessModifier = M.AccessModifier.Protected; break;
                    case S.AccessModifier.Private: throw new FatalException();
                    default: throw new UnreachableCodeException();
                }

                var funcInfo = new InternalModuleFuncInfo(
                    accessModifier,
                    bInstanceFunc: bThisCall,
                    bSeqFunc: funcDecl.IsSequence,
                    bRefReturn: funcDecl.IsRefReturn,
                    retType,
                    new M.Name.Normal(funcDecl.Name),
                    funcDecl.TypeParams,
                    paramInfo
                );

                funcs.Add(funcInfo);
            }

            void VisitClassMemberVarDecl(S.ClassMemberVarDecl varDecl)
            {
                var declType = GetMType(typeExpInfoService, varDecl.VarType);
                if (declType == null)
                    throw new FatalException();

                var accessModifier = varDecl.AccessModifier switch
                {
                    null => M.AccessModifier.Private,
                    S.AccessModifier.Public => M.AccessModifier.Public,
                    S.AccessModifier.Protected => M.AccessModifier.Protected,
                    S.AccessModifier.Private => throw new UnreachableCodeException(), // 미리 에러를 잡는다
                    _ => throw new UnreachableCodeException()
                };

                foreach (var name in varDecl.VarNames)
                {
                    bool bStatic = false; // TODO: static 키워드가 추가되면 고려한다
                    var varInfo = new InternalModuleMemberVarInfo(accessModifier, bStatic, declType, new M.Name.Normal(name));
                    memberVars.Add(varInfo);
                }
            }

            void VisitClassConstructorDeclElement(S.ClassConstructorDecl constructorDecl)
            {
                M.AccessModifier accessModifier;
                switch (constructorDecl.AccessModifier)
                {
                    // 기본 private
                    case null: accessModifier = M.AccessModifier.Private; break;
                    case S.AccessModifier.Public: accessModifier = M.AccessModifier.Public; break;
                    case S.AccessModifier.Protected: accessModifier = M.AccessModifier.Protected; break;
                    case S.AccessModifier.Private: throw new FatalException();
                    default: throw new UnreachableCodeException();
                }

                var paramInfo = MakeParams(typeExpInfoService, constructorDecl.Parameters);

                constructors.Add(new InternalModuleConstructorInfo(accessModifier, paramInfo));
            }
        }

        [AutoConstructor]
        partial struct IntermediateStructBuilder
        {
            TypeExpInfoService typeExpInfoService;
            List<IModuleFuncInfo> funcs;
            List<IModuleConstructorInfo> constructors;
            List<IModuleMemberVarInfo> memberVars;

            public static void Build(
                TypeBuilder typeBuilder, 
                List<IModuleTypeInfo> types,
                TypeExpInfoService typeExpInfoService,
                ItemPath parentPath, S.StructDecl structDecl)
            {
                var structPath = parentPath.Child(new M.Name.Normal(structDecl.Name), structDecl.TypeParams.Length);
                var nestedTypes = new List<IModuleTypeInfo>();
                var nestedFuncs = new List<IModuleFuncInfo>();
                var constructors = new List<IModuleConstructorInfo>();
                var memberVars = new List<IModuleMemberVarInfo>();
                var builder = new IntermediateStructBuilder(typeExpInfoService, nestedFuncs, constructors, memberVars);

                if (structDecl.BaseTypes.Length != 0)
                    throw new NotImplementedException();

                // base & interfaces
                //var mbaseTypesBuilder = ImmutableArray.CreateBuilder<M.Type>(structDecl.BaseTypes.Length);
                //foreach (var baseType in structDecl.BaseTypes)
                //{
                //    var mbaseType = GetMType(baseType);
                //    if (mbaseType == null) throw new FatalException();
                //    mbaseTypesBuilder.Add(mbaseType);
                //}
                //var mbaseTypes = mbaseTypesBuilder.MoveToImmutable();

                foreach (var elem in structDecl.MemberDecls)
                {
                    switch (elem)
                    {
                        case S.StructMemberFuncDecl funcDecl:
                            builder.VisitStructMemberFuncDecl(M.AccessModifier.Public, funcDecl);
                            break;

                        case S.StructMemberTypeDecl typeDecl:
                            IntermediateTypeBuilder.BuildType(typeBuilder, nestedTypes, typeExpInfoService, structPath, typeDecl.TypeDecl);
                            break;

                        case S.StructMemberVarDecl varDecl:
                            builder.VisitStructMemberVarDecl(varDecl);
                            break;

                        case S.StructConstructorDecl constructorDecl:
                            builder.VisitStructConstructorDeclElement(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }

                var structInfo = new InternalModuleStructInfo(
                    new M.Name.Normal(structDecl.Name), structDecl.TypeParams, null, // TODO: baseStruct 지원
                    nestedTypes.ToImmutableArray(), nestedFuncs.ToImmutableArray(), constructors.ToImmutableArray(), memberVars.ToImmutableArray());

                typeBuilder.AddStruct(structPath, structInfo);
                types.Add(structInfo);
            }

            void VisitStructMemberFuncDecl(M.AccessModifier defaultAccessModifier, S.StructMemberFuncDecl funcDecl)
            {
                bool bThisCall = true; // TODO: static 키워드가 추가되면 고려하도록 한다

                var retType = GetMType(typeExpInfoService, funcDecl.RetType);
                if (retType == null) throw new FatalException();

                var paramInfo = MakeParams(typeExpInfoService, funcDecl.Parameters);

                M.AccessModifier accessModifier;
                switch (funcDecl.AccessModifier)
                {
                    case null: accessModifier = defaultAccessModifier; break;
                    case S.AccessModifier.Public:
                        if (defaultAccessModifier == M.AccessModifier.Public) throw new FatalException();
                        accessModifier = M.AccessModifier.Public;
                        break;

                    case S.AccessModifier.Protected:
                        if (defaultAccessModifier == M.AccessModifier.Protected) throw new FatalException();
                        accessModifier = M.AccessModifier.Protected;
                        break;

                    case S.AccessModifier.Private:
                        if (defaultAccessModifier == M.AccessModifier.Private) throw new FatalException();
                        accessModifier = M.AccessModifier.Private;
                        break;

                    default:
                        throw new UnreachableCodeException();
                }

                var funcInfo = new InternalModuleFuncInfo(
                    accessModifier,
                    bInstanceFunc: bThisCall,
                    bSeqFunc: funcDecl.IsSequence,
                    bRefReturn: funcDecl.IsRefReturn,
                    retType,
                    new M.Name.Normal(funcDecl.Name),
                    funcDecl.TypeParams,
                    paramInfo
                );

                funcs.Add(funcInfo);
            }

            void VisitStructMemberVarDecl(S.StructMemberVarDecl varDecl)
            {
                var declType = GetMType(typeExpInfoService, varDecl.VarType);
                if (declType == null)
                    throw new FatalException();

                var accessModifier = varDecl.AccessModifier switch
                {
                    null => M.AccessModifier.Public,
                    S.AccessModifier.Public => throw new UnreachableCodeException(), // 미리 에러를 잡는다
                    S.AccessModifier.Protected => M.AccessModifier.Protected,
                    S.AccessModifier.Private => M.AccessModifier.Private,
                    _ => throw new UnreachableCodeException()
                };

                foreach (var name in varDecl.VarNames)
                {
                    bool bStatic = false; // TODO: static 키워드가 추가되면 고려한다

                    var varInfo = new InternalModuleMemberVarInfo(accessModifier, bStatic, declType, new M.Name.Normal(name));
                    memberVars.Add(varInfo);
                }
            }

            void VisitStructConstructorDeclElement(S.StructConstructorDecl constructorDecl)
            {
                M.AccessModifier accessModifier;
                switch (constructorDecl.AccessModifier)
                {
                    case null: accessModifier = M.AccessModifier.Public; break;
                    case S.AccessModifier.Public: throw new FatalException();
                    case S.AccessModifier.Protected: accessModifier = M.AccessModifier.Protected; break;
                    case S.AccessModifier.Private: accessModifier = M.AccessModifier.Private; break;
                    default: throw new UnreachableCodeException();
                }

                var paramInfo = MakeParams(typeExpInfoService, constructorDecl.Parameters);
                
                // 이름관련 처리를 여기서 할까
                constructors.Add(new InternalModuleConstructorInfo(accessModifier, paramInfo));
            }
        }
    }

    
}