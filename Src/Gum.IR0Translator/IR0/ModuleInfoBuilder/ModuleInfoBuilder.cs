using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // Script에서 ModuleInfo 정보를 뽑는 역할
    partial class ModuleInfoBuilder
    {
        TypeExpTypeValueService typeExpTypeValueService;

        TypeBuilder? typeBuilder;
        List<TypeInfo> globalTypeInfos; // global 타입 정보
        List<FuncInfo> globalFuncInfos; // All Funcs
        List<VarInfo> globalVarInfos;   // global Variable정보        

        public ModuleInfoBuilder(TypeExpTypeValueService typeExpTypeValueService)
        {
            this.typeExpTypeValueService = typeExpTypeValueService;

            typeBuilder = null;
            globalTypeInfos = new List<TypeInfo>();
            globalFuncInfos = new List<FuncInfo>();
            globalVarInfos = new List<VarInfo>();
            
        }

        TypeValue GetTypeValue(S.TypeExp typeExp)
        {
            return typeExpTypeValueService.GetTypeValue(typeExp);
        }
        
        void AddTypeInfo(TypeInfo typeInfo)
        {
            if (typeBuilder == null)
                globalTypeInfos.Add(typeInfo);
            else
                typeBuilder.AddTypeInfo(typeInfo);
        }

        void AddVarInfo(VarInfo varInfo)
        {
            if (typeBuilder == null)
                globalVarInfos.Add(varInfo);
            else
                typeBuilder.AddVarInfo(varInfo);
        }

        void ExecInNewTypeScope(Name name, int typeParamCount, Action action)
        {
            var prevTypeBuilder = typeBuilder;

            if (typeBuilder != null)
            {
                typeBuilder = new TypeBuilder(typeBuilder.GetTypePath().Append(name, typeParamCount));
            }
            else
            {
                // TODO: namespace
                typeBuilder = new TypeBuilder(new ItemPath(NamespacePath.Root, name, typeParamCount));
            }

            try
            {
                action.Invoke();
            }
            finally
            {
                typeBuilder = prevTypeBuilder;
            }
        }

        // 현재 위치가 Type안에 있는지
        bool IsInsideTypeScope()
        {
            return typeBuilder != null;
        }

        public EnumElemInfo BuildEnumElement(S.EnumDeclElement elem)
        {
            var fieldInfos = elem.Params.Select(parameter =>
            {
                var typeValue = GetTypeValue(parameter.Type);
                return new EnumElemFieldInfo(typeValue, parameter.Name);
            });

            return new EnumElemInfo(elem.Name, fieldInfos);
        }

        public EnumInfo BuildEnum(Skeleton.Enum enumSkel, IEnumerable<EnumElemInfo> elemInfos)
        {
            var enumInfo = new EnumInfo(
                new ItemId(ModuleName.Internal, enumSkel.Path),
                enumSkel.EnumDecl.TypeParams, 
                elemInfos);

            AddTypeInfo(enumInfo);
            return enumInfo;
        }
        
        public StructInfo BuildStruct(Skeleton.Struct structSkel)
        {
            // S<T, U> 
            // 지금 depth는?            
            //int depth = GetTypeDepth();
            //int index = 0;
            //var typeArgs = structDecl.TypeParams.Select(typeParam => {
            //    var v = new TypeValue.TypeVar(depth, index, typeParam);
            //    index++;
            //    return v;
            //});
            // var entry = new AppliedItemPathEntry(structDecl.Name, string.Empty, typeArgs);

            StructInfo Make() { throw new NotImplementedException(); }

            var structInfo = Make();
            AddTypeInfo(structInfo);

            ExecInNewTypeScope(structSkel.StructDecl.Name, structSkel.StructDecl.TypeParams.Length, () => {

            });

            return structInfo;
        }

        public FuncInfo BuildFunc(Skeleton.Func funcSkel)
        {
            var funcDecl = funcSkel.FuncDecl;
            var paramHash = GetParamHash(funcDecl.ParamInfo);

            bool bThisCall = IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려하도록 한다

            var funcInfo = new FuncInfo(
                new ItemId(ModuleName.Internal, funcSkel.Path),
                funcDecl.IsSequence,
                bThisCall,
                funcDecl.TypeParams,
                GetTypeValue(funcDecl.RetType),
                funcDecl.ParamInfo.Parameters.Select(typeAndName => GetTypeValue(typeAndName.Type))
            );

            if (typeBuilder == null)
                globalFuncInfos.Add(funcInfo);
            else
                typeBuilder.AddFuncInfo(funcInfo);

            return funcInfo;
        }

        public void BuildVar(Skeleton.Var varSkel)
        {
            var varId = new ItemId(ModuleName.Internal, varSkel.Path);
            bool bStatic = !IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려한다

            var varDecl = varSkel.VarDecl;
            var varInfo = new VarInfo(varId, bStatic, GetTypeValue(varDecl.Type));
            AddVarInfo(varInfo);
        }

        string GetParamHash(S.FuncParamInfo paramInfo)
        {
            return Misc.MakeParamHash(paramInfo.Parameters.Select(parameter => GetTypeValue(parameter.Type)));
        }

        public ScriptModuleInfo Build()
        {
            var globalItems = globalTypeInfos.Cast<ItemInfo>()
                .Concat(globalFuncInfos)
                .Concat(globalVarInfos);

            return new ScriptModuleInfo(Array.Empty<NamespaceInfo>(), globalItems);
        }
    }
}