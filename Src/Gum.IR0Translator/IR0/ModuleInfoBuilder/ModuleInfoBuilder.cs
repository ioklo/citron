using Gum.CompileTime;
using Gum.Infra;
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // Script에서 ModuleInfo 정보를 뽑는 역할
    partial class ModuleInfoBuilder : ISyntaxScriptVisitor
    {
        TypeExpTypeValueService typeExpTypeValueService;

        TypeBuilder? typeBuilder;
        List<TypeInfo> globalTypeInfos; 
        List<FuncInfo> globalFuncInfos; 
        List<VarInfo> globalVarInfos;
        
        public static InternalModuleInfo Build(S.Script script, TypeExpTypeValueService typeExpTypeValueService)
        {
            var builder = new ModuleInfoBuilder(typeExpTypeValueService);

            Misc.VisitScript(script, builder);

            var globalItems = builder.globalTypeInfos.Cast<ItemInfo>()
                .Concat(builder.globalFuncInfos)
                .Concat(builder.globalVarInfos);

            return new InternalModuleInfo(Array.Empty<NamespaceInfo>(), globalItems);
        }

        ModuleInfoBuilder(TypeExpTypeValueService typeExpTypeValueService)
        {
            this.typeExpTypeValueService = typeExpTypeValueService;

            typeBuilder = null;
            globalTypeInfos = new List<TypeInfo>();
            globalFuncInfos = new List<FuncInfo>();
            globalVarInfos = new List<VarInfo>();
        }
        
        ItemPath MakePath(Name name, int typeParamCount = 0, S.FuncParamInfo? paramInfo = null)
        {
            var paramHash = paramInfo != null 
                ? Misc.MakeParamHash(paramInfo.Value.Parameters.Select(parameter => GetTypeValue(parameter.Type)))
                : string.Empty;

            if (typeBuilder != null)
            {
                var typePath = typeBuilder.GetTypePath();
                return typePath.Append(name, typeParamCount, paramHash);
            }
            else
            {
                // TODO: namespace 고려
                return new ItemPath(NamespacePath.Root, name, typeParamCount, paramHash);
            }
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

        // enum E { First(int x, int y), Second } 에서 
        // First(int x, int y) 부분
        EnumElemInfo VisitEnumDeclElement(S.EnumDeclElement elem)
        {
            var fieldInfos = elem.Params.Select(parameter =>
            {
                var typeValue = GetTypeValue(parameter.Type);
                return new EnumElemFieldInfo(typeValue, parameter.Name);
            });

            return new EnumElemInfo(elem.Name, fieldInfos);
        }       

        void VisitTypeDecl(S.TypeDecl typeDecl)
        {
            switch(typeDecl)
            {
                case S.StructDecl structDecl:
                    VisitStructDecl(structDecl);
                    break;

                case S.EnumDecl enumDecl:
                    VisitEnumDecl(enumDecl);
                    break;
            }

            throw new UnreachableCodeException();
        }

        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            var path = MakePath(enumDecl.Name, enumDecl.TypeParamCount);
            var elemInfos = new List<EnumElemInfo>();
            foreach (var elem in enumDecl.Elems)
            {
                var elemInfo = VisitEnumDeclElement(elem);
                elemInfos.Add(elemInfo);
            }

            var enumInfo = new EnumInfo(
                new ItemId(ModuleName.Internal, path),
                enumDecl.TypeParams,
                elemInfos.ToImmutableArray());

            AddTypeInfo(enumInfo);
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            ExecInNewTypeScope(structDecl.Name, structDecl.TypeParams.Length, () => {

                foreach(var elem in structDecl.Elems)
                {
                    switch(elem)
                    {
                        case S.StructDecl.FuncDeclElement funcDeclElem:
                            VisitFuncDecl(funcDeclElem.FuncDecl);
                            break;

                        case S.StructDecl.TypeDeclElement typeDeclElem:
                            VisitTypeDecl(typeDeclElem.TypeDecl);
                            break;

                        case S.StructDecl.VarDeclElement varDeclElem:
                            VisitStructVarDeclElement(varDeclElem);
                            break;
                    }
                }
            });

            throw new NotImplementedException();
            //var structInfo = new StructInfo()
            //AddTypeInfo(structInfo);
        }

        void VisitFuncDecl(S.FuncDecl funcDecl)
        {   
            var path = MakePath(funcDecl.Name, funcDecl.TypeParams.Length, funcDecl.ParamInfo);

            bool bThisCall = IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려하도록 한다

            var funcInfo = new FuncInfo(
                new ItemId(ModuleName.Internal, path),
                funcDecl.IsSequence,
                bThisCall,
                funcDecl.TypeParams,
                GetTypeValue(funcDecl.RetType),
                funcDecl.ParamInfo.Parameters.Select(typeAndName => GetTypeValue(typeAndName.Type)).ToImmutableArray()
            );

            if (typeBuilder == null)
                globalFuncInfos.Add(funcInfo);
            else
                typeBuilder.AddFuncInfo(funcInfo);
        }

        void VisitStructVarDeclElement(S.StructDecl.VarDeclElement varDeclElem)
        {
            Debug.Assert(typeBuilder != null);

            var declType = GetTypeValue(varDeclElem.VarType);
            foreach(var name in varDeclElem.VarNames)
            {
                var varId = new ItemId(ModuleName.Internal, MakePath(name));
                bool bStatic = !IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려한다

                var varInfo = new VarInfo(varId, bStatic, declType);
                AddVarInfo(varInfo);
            }
        }

        void ISyntaxScriptVisitor.VisitGlobalFuncDecl(S.FuncDecl funcDecl)
        {
            VisitFuncDecl(funcDecl);
        }

        void ISyntaxScriptVisitor.VisitTopLevelStmt(S.Stmt stmt)
        {
            // do nothing
        }

        void ISyntaxScriptVisitor.VisitTypeDecl(S.TypeDecl typeDecl)
        {
            VisitTypeDecl(typeDecl);
        }
    }
}