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

namespace Gum.IR0Translator
{
    interface ITypeBuilder
    {
        // IModuleTypeInfo를 만들려고 할 때(Automatic Trivial Constructor를 만들때가 아니라), 먼저 만들어야 할 타입들 목록
        ImmutableArray<ItemPath> GetDependentInternalTypes();
        IModuleTypeInfo Build(ImmutableArray<IModuleTypeInfo> innerTypes);
    }

    class InternalModuleTypeInfoBuilderMisc
    {
        static bool IsMatchTrivialConstructorParameters(M.ParamTypes paramTypes, ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            if (memberVars.Length != paramTypes.Length) return false;

            for (int i = 0; i < memberVars.Length; i++)
            {
                // normal로만 자동으로 생성한다
                if (paramTypes[i].Kind != M.ParamKind.Normal) return false;

                var memberVarType = memberVars[i].GetDeclType();
                var paramType = paramTypes[i].Type;

                if (!memberVarType.Equals(paramType)) return false;
            }

            return true;
        }

        public static IModuleConstructorInfo? GetTrivialConstructor(ImmutableArray<IModuleConstructorInfo> constructors, ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            foreach (var constructor in constructors)
            {
                if (IsMatchTrivialConstructorParameters(constructor.GetParamTypes(), memberVars))
                    return constructor;
            }

            return null;
        }

        public static IModuleConstructorInfo MakeAutomaticTrivialConstructor(M.Name typeName, ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            var builder = ImmutableArray.CreateBuilder<M.Param>(memberVars.Length);
            foreach (var memberVar in memberVars)
            {
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new M.Param(M.ParamKind.Normal, type, name);
                builder.Add(param);
            }

            // automatic trivial constructor를 만듭니다
            return new InternalModuleConstructorInfo(M.AccessModifier.Public, typeName, builder.MoveToImmutable());
        }
    }

    [AutoConstructor]
    partial class InternalClassModuleInfoBuilder : ITypeBuilder
    {
        S.ClassDecl classDecl;

        M.Type? mbaseType;
        ImmutableArray<M.Type> interfaces;
        ImmutableArray<ItemPath> dependentInternalTypes;
        ImmutableArray<IModuleFuncInfo> funcs;
        ImmutableArray<IModuleConstructorInfo> constructors;
        ImmutableArray<IModuleMemberVarInfo> memberVars;       

        public ImmutableArray<ItemPath> GetDependentInternalTypes()
        {
            // class C { class P : C { } } // C는 P가 있어야 만들 수 있다 nestedType으로 갖고 있어야 하기 때문에
            return dependentInternalTypes;
        }

        public IModuleTypeInfo Build(ImmutableArray<IModuleTypeInfo> innerTypes)
        {
            var trivialConstructor = InternalModuleTypeInfoBuilderMisc.GetTrivialConstructor(constructors, memberVars);

            bool bNeedGenerateTrivialConstructor = false;
            ImmutableArray<IModuleConstructorInfo> finalConstructors = constructors;
            if (trivialConstructor == null)
            {
                trivialConstructor = InternalModuleTypeInfoBuilderMisc.MakeAutomaticTrivialConstructor(classDecl.Name, memberVars);
                finalConstructors = constructors.Add(trivialConstructor);
                bNeedGenerateTrivialConstructor = true;
            }

            var classInfo = new InternalModuleClassInfo(
                classDecl.Name, classDecl.TypeParams, mbaseType, interfaces,
                innerTypes,
                funcs,
                finalConstructors,
                trivialConstructor,
                bNeedGenerateTrivialConstructor,
                memberVars);

            return classInfo;
        }
    }

    [AutoConstructor]
    partial class InternalStructModuleInfoBuilder : ITypeBuilder
    {
        S.StructDecl structDecl;
        ImmutableArray<ItemPath> dependentInternalTypes;
        ImmutableArray<IModuleFuncInfo> funcs;
        ImmutableArray<IModuleConstructorInfo> constructors;
        ImmutableArray<IModuleMemberVarInfo> memberVars;

        public ImmutableArray<ItemPath> GetDependentInternalTypes()
        {
            // class C { class P : C { } } // C는 P가 있어야 만들 수 있다 nestedType으로 갖고 있어야 하기 때문에
            return dependentInternalTypes;
        }

        public IModuleTypeInfo Build(ImmutableArray<IModuleTypeInfo> nestedTypeInfos)
        {
            var finalConstructors = constructors;
            var bNeedGenerateTrivialConstructor = false;
            var trivialConstructor = InternalModuleTypeInfoBuilderMisc.GetTrivialConstructor(constructors, memberVars);
            if (trivialConstructor == null)
            {
                trivialConstructor = InternalModuleTypeInfoBuilderMisc.MakeAutomaticTrivialConstructor(structDecl.Name, memberVars);
                finalConstructors = constructors.Add(trivialConstructor);
                bNeedGenerateTrivialConstructor = true;
            }

            return new InternalModuleStructInfo(
                structDecl.Name, structDecl.TypeParams, null,
                nestedTypeInfos,
                funcs,
                finalConstructors,
                trivialConstructor,
                bNeedGenerateTrivialConstructor,
                memberVars);
        }
    }

    [AutoConstructor]
    partial class InternalEnumModuleInfoBuilder : ITypeBuilder
    {
        S.EnumDecl enumDecl;
        ImmutableArray<InternalModuleEnumElemInfo> elems;        

        public IModuleTypeInfo Build(ImmutableArray<IModuleTypeInfo> innerTypes)
        {
            return new InternalModuleEnumInfo(enumDecl.Name, enumDecl.TypeParams, elems);
        }

        public ImmutableArray<ItemPath> GetDependentInternalTypes()
        {
            return ImmutableArray<ItemPath>.Empty;
        }
    }

    class TypeBuilders
    {
        Dictionary<ItemPath, ITypeBuilder> builders;

        public TypeBuilders()
        {
            builders = new Dictionary<ItemPath, ITypeBuilder>();
        }

        public void AddBuilder(ItemPath path, ITypeBuilder builder)
        {
            builders.Add(path, builder);
        }
        
        void Build(Dictionary<ItemPath, IModuleTypeInfo> builts, HashSet<ItemPath> buildings, Dictionary<ItemPath, List<IModuleTypeInfo>> innerTypesDict, ItemPath path)
        {
            if (builts.ContainsKey(path)) return;
            
            if (buildings.Contains(path)) 
                throw new InvalidOperationException();
            buildings.Add(path);
            
            if (!builders.TryGetValue(path, out var builder))
                throw new InvalidOperationException();

            // 연관 있는 애들을 먼저 빌드
            foreach (var depType in builder.GetDependentInternalTypes())
                Build(builts, buildings, innerTypesDict, depType);            

            // innerType정보를 가지고 빌드
            IModuleTypeInfo type;
            if (innerTypesDict.TryGetValue(path, out var innerTypes))
                type = builder.Build(innerTypes.ToImmutableArray());
            else
                type = builder.Build(ImmutableArray<IModuleTypeInfo>.Empty);

            buildings.Remove(path);
            builts.Add(path, type);
        }

        public Dictionary<ItemPath, IModuleTypeInfo> Build()
        {
            var builts = new Dictionary<ItemPath, IModuleTypeInfo>();
            var buildings = new HashSet<ItemPath>();
            var innerTypesDict = new Dictionary<ItemPath, List<IModuleTypeInfo>>();

            foreach(var path in builders.Keys)
            {
                Build(builts, buildings, innerTypesDict, path);
            }

            return builts;
        }
    }       

    // Script에서 ModuleInfo 정보를 뽑는 역할
    [AutoConstructor]
    partial struct InternalModuleInfoBuilder
    {
        TypeExpInfoService typeExpInfoService;
        List<IModuleFuncInfo> funcs;
        TypeBuilders typeBuilders;

        public static InternalModuleInfo Build(M.ModuleName moduleName, S.Script script, TypeExpInfoService typeExpInfoService)
        {
            var nestedTypePaths = new List<ItemPath>();
            var funcs = new List<IModuleFuncInfo>();
            var typeBuilders = new TypeBuilders();
            var builder = new InternalModuleInfoBuilder(typeExpInfoService, funcs, typeBuilders);            

            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case S.TypeDeclScriptElement typeDeclElem:
                        IntermediateTypeBuilder.BuildType(typeBuilders, nestedTypePaths, typeExpInfoService, null, typeDeclElem.TypeDecl);
                        break;

                    case S.GlobalFuncDeclScriptElement globalFuncDeclElem:                        
                        builder.BuildFuncDecl(globalFuncDeclElem.FuncDecl);
                        break;
                    
                    case S.StmtScriptElement:
                        // skip
                        break;
                }
            }

            var builts = typeBuilders.Build();

            var typeInfosBuilder = ImmutableArray.CreateBuilder<IModuleTypeInfo>(nestedTypePaths.Count);
            foreach (var typePath in nestedTypePaths)
                typeInfosBuilder.Add(builts[typePath]);

            return new InternalModuleInfo(moduleName,typeInfosBuilder.MoveToImmutable(), funcs.ToImmutableArray());
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
                funcDecl.Name,
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
                
                builder.Add(new M.Param(paramKind, mtype, sparam.Name));
            }

            return builder.MoveToImmutable();
        }
        
        partial struct IntermediateTypeBuilder
        {
            public static void BuildType(TypeBuilders typeBuilders, List<ItemPath> nestedTypePaths, TypeExpInfoService typeExpInfoService, ItemPath? parentPath, S.TypeDecl typeDecl)
            {
                switch (typeDecl)
                {
                    case S.StructDecl structDecl:
                        var structPath = parentPath == null
                            ? new ItemPath(M.NamespacePath.Root, structDecl.Name, structDecl.TypeParams.Length)
                            : parentPath.Value.Append(structDecl.Name, structDecl.TypeParams.Length);
                        IntermediateStructBuilder.Build(typeBuilders, typeExpInfoService, structPath, structDecl);
                        nestedTypePaths.Add(structPath);
                        break;

                    case S.ClassDecl classDecl:
                        var classPath = parentPath == null
                            ? new ItemPath(M.NamespacePath.Root, classDecl.Name, classDecl.TypeParams.Length)
                            : parentPath.Value.Append(classDecl.Name, classDecl.TypeParams.Length);
                        IntermediateClassBuilder.Build(typeBuilders, typeExpInfoService, classPath, classDecl);
                        nestedTypePaths.Add(classPath);
                        break;

                    case S.EnumDecl enumDecl:
                        var enumPath = parentPath == null
                            ? new ItemPath(M.NamespacePath.Root, enumDecl.Name, enumDecl.TypeParams.Length)
                            : parentPath.Value.Append(enumDecl.Name, enumDecl.TypeParams.Length);
                        IntermediateEnumBuilder.Build(typeBuilders, typeExpInfoService, enumPath, enumDecl);
                        nestedTypePaths.Add(enumPath);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }
        }
        
        partial struct IntermediateEnumBuilder
        {
            public static void Build(TypeBuilders typeBuilders, TypeExpInfoService typeExpInfoService, ItemPath enumPath, S.EnumDecl enumDecl)
            {
                var elemsBuilder = ImmutableArray.CreateBuilder<InternalModuleEnumElemInfo>(enumDecl.Elems.Length);
                foreach (var elem in enumDecl.Elems)
                {
                    var fieldsBuilder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(elem.Fields.Length);
                    foreach (var field in elem.Fields)
                    {
                        var type = GetMType(typeExpInfoService, field.Type);
                        Debug.Assert(type != null);

                        var mfield = new InternalModuleMemberVarInfo(M.AccessModifier.Public, false, type, field.Name);
                        fieldsBuilder.Add(mfield);
                    }

                    var fields = fieldsBuilder.MoveToImmutable();
                    var enumElemInfo = new InternalModuleEnumElemInfo(elem.Name, fields);
                    elemsBuilder.Add(enumElemInfo);
                }

                var elems = elemsBuilder.MoveToImmutable();
                var builder = new InternalEnumModuleInfoBuilder(enumDecl, elems);
                typeBuilders.AddBuilder(enumPath, builder);
            }
        }

        [AutoConstructor]
        partial struct IntermediateClassBuilder
        {
            TypeExpInfoService typeExpInfoService;
            S.ClassDecl classDecl;
            List<IModuleFuncInfo> funcs;
            List<IModuleConstructorInfo> constructors;
            List<IModuleMemberVarInfo> memberVars;

            public static void Build(TypeBuilders typeBuilders, TypeExpInfoService typeExpInfoService, ItemPath classPath, S.ClassDecl classDecl)
            {
                var funcs = new List<IModuleFuncInfo>();
                var constructors = new List<IModuleConstructorInfo>();
                var memberVars = new List<IModuleMemberVarInfo>();
                var builder = new IntermediateClassBuilder(typeExpInfoService, classDecl, funcs, constructors, memberVars);

                // base & interfaces
                var baseTypeCandidates = new Candidates<TypeExpInfo>();
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

                                baseTypeCandidates.Add(baseTypeExpInfo);
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

                var nestedTypePaths = new List<ItemPath>();

                foreach (var elem in classDecl.MemberDecls)
                {
                    switch (elem)
                    {
                        case S.ClassMemberFuncDecl funcDecl:
                            builder.VisitClassMemberFuncDecl(funcDecl);
                            break;

                        case S.ClassMemberTypeDecl typeDecl:
                            IntermediateTypeBuilder.BuildType(typeBuilders, nestedTypePaths, typeExpInfoService, classPath, typeDecl.TypeDecl);
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
                var classBuilder = new InternalClassModuleInfoBuilder(
                    classDecl, baseTypeExpInfoResult?.GetMType(), interfacesBuilder.ToImmutable(), nestedTypePaths.ToImmutableArray(),
                    funcs.ToImmutableArray(), constructors.ToImmutableArray(), memberVars.ToImmutableArray());

                typeBuilders.AddBuilder(classPath, classBuilder);
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
                    funcDecl.Name,
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
                    var varInfo = new InternalModuleMemberVarInfo(accessModifier, bStatic, declType, name);
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

                constructors.Add(new InternalModuleConstructorInfo(accessModifier, constructorDecl.Name, paramInfo));
            }
        }

        [AutoConstructor]
        partial struct IntermediateStructBuilder
        {
            TypeExpInfoService typeExpInfoService;
            List<IModuleFuncInfo> funcs;
            List<IModuleConstructorInfo> constructors;
            List<IModuleMemberVarInfo> memberVars;

            public static void Build(TypeBuilders typeBuilders, TypeExpInfoService typeExpInfoService, ItemPath structPath, S.StructDecl structDecl)
            {
                var funcs = new List<IModuleFuncInfo>();
                var constructors = new List<IModuleConstructorInfo>();
                var memberVars = new List<IModuleMemberVarInfo>();
                var builder = new IntermediateStructBuilder(typeExpInfoService, funcs, constructors, memberVars);

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

                var nestedTypePaths = new List<ItemPath>();

                foreach (var elem in structDecl.MemberDecls)
                {
                    switch (elem)
                    {
                        case S.StructMemberFuncDecl funcDecl:
                            builder.VisitStructMemberFuncDecl(M.AccessModifier.Public, funcDecl);
                            break;

                        case S.StructMemberTypeDecl typeDecl:
                            IntermediateTypeBuilder.BuildType(typeBuilders, nestedTypePaths, typeExpInfoService, structPath, typeDecl.TypeDecl);
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
                
                var structBuilder = new InternalStructModuleInfoBuilder(structDecl, nestedTypePaths.ToImmutableArray(), funcs.ToImmutableArray(), constructors.ToImmutableArray(), memberVars.ToImmutableArray());
                typeBuilders.AddBuilder(structPath, structBuilder);
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
                    funcDecl.Name,
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

                    var varInfo = new InternalModuleMemberVarInfo(accessModifier, bStatic, declType, name);
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

                constructors.Add(new InternalModuleConstructorInfo(accessModifier, constructorDecl.Name, paramInfo));
            }
        }
    }

    
}