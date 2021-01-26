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
using MType = Gum.CompileTime.Type;

namespace Gum.IR0
{
    // Script에서 ModuleInfo 정보를 뽑는 역할
    partial class ModuleInfoBuilder : ISyntaxScriptVisitor
    {
        TypeExpInfoService typeExpInfoService;

        TypeBuilder? typeBuilder;
        List<TypeInfo> globalTypeInfos; 
        List<FuncInfo> globalFuncInfos; 
        
        public static ModuleInfo Build(ModuleName moduleName, S.Script script, TypeExpInfoService typeExpTypeValueService)
        {
            var builder = new ModuleInfoBuilder(typeExpTypeValueService);

            Misc.VisitScript(script, builder);

            var moduleInfo = new ModuleInfo(moduleName,
                ImmutableArray<NamespaceInfo>.Empty,
                builder.globalTypeInfos.ToImmutableArray(),
                builder.globalFuncInfos.ToImmutableArray());

            return moduleInfo;
        }

        ModuleInfoBuilder(TypeExpInfoService typeExpInfoService)
        {
            this.typeExpInfoService = typeExpInfoService;

            typeBuilder = null;
            globalTypeInfos = new List<TypeInfo>();
            globalFuncInfos = new List<FuncInfo>();
        }
        
        ItemPath MakePath(Name name, int typeParamCount = 0, S.FuncParamInfo? paramInfo = null)
        {
            var paramHash = paramInfo != null 
                ? Misc.MakeParamHash(paramInfo.Value.Parameters.Select(parameter => GetMType(parameter.Type)).ToImmutableArray())
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

        MType? GetMType(S.TypeExp typeExp)
        {
            var typeExpInfo = typeExpInfoService.GetTypeExpInfo(typeExp);
            if( typeExpInfo is MTypeTypeExpInfo mtypeTypeExpInfo)
                return mtypeTypeExpInfo.Type;

            return null;
        }
        
        void AddTypeInfo(TypeInfo typeInfo)
        {
            if (typeBuilder == null)
                globalTypeInfos.Add(typeInfo);
            else
                typeBuilder.AddTypeInfo(typeInfo);
        }

        void AddMemberVarInfo(MemberVarInfo memberVarInfo)
        {
            Debug.Assert(typeBuilder != null);
            typeBuilder.AddMemberVarInfo(memberVarInfo);
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
        //M.EnumElemInfo VisitEnumDeclElement(S.EnumDeclElement elem)
        //{
        //    var fieldInfos = elem.Params.Select(parameter =>
        //    {
        //        var typeValue = GetMType(parameter.Type);
        //        return new EnumElemFieldInfo(typeValue, parameter.Name);
        //    });

        //    return new EnumElemInfo(elem.Name, fieldInfos);
        //}       

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

                default:
                    throw new UnreachableCodeException();
            }
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
                GetMType(funcDecl.RetType),
                funcDecl.ParamInfo.Parameters.Select(typeAndName => GetMType(typeAndName.Type)).ToImmutableArray()
            );

            if (typeBuilder == null)
                globalFuncInfos.Add(funcInfo);
            else
                typeBuilder.AddFuncInfo(funcInfo);
        }

        void VisitStructVarDeclElement(S.StructDecl.VarDeclElement varDeclElem)
        {
            Debug.Assert(typeBuilder != null);

            var declType = GetMType(varDeclElem.VarType);
            foreach(var name in varDeclElem.VarNames)
            {
                bool bStatic = !IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려한다

                var varInfo = new MemberVarInfo(bStatic, declType, name);
                AddMemberVarInfo(varInfo);
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