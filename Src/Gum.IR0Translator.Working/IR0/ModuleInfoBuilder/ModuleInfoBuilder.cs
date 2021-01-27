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
using M = Gum.CompileTime;

namespace Gum.IR0
{
    // Script에서 ModuleInfo 정보를 뽑는 역할
    partial class ModuleInfoBuilder : ISyntaxScriptVisitor
    {
        TypeExpInfoService typeExpInfoService;

        TypeBuilder? outerTypeBuilder;
        TypeBuilder? typeBuilder;
        List<M.TypeInfo> globalTypeInfos; 
        List<M.FuncInfo> globalFuncInfos; 
        
        public static M.ModuleInfo Build(M.ModuleName moduleName, S.Script script, TypeExpInfoService typeExpTypeValueService)
        {
            var builder = new ModuleInfoBuilder(typeExpTypeValueService);

            Misc.VisitScript(script, builder);

            var moduleInfo = new M.ModuleInfo(moduleName,
                ImmutableArray<M.NamespaceInfo>.Empty,
                builder.globalTypeInfos.ToImmutableArray(),
                builder.globalFuncInfos.ToImmutableArray());

            return moduleInfo;
        }

        ModuleInfoBuilder(TypeExpInfoService typeExpInfoService)
        {
            this.typeExpInfoService = typeExpInfoService;

            outerTypeBuilder = null;
            typeBuilder = null;
            globalTypeInfos = new List<M.TypeInfo>();
            globalFuncInfos = new List<M.FuncInfo>();
        }
        
        ItemPath MakePath(M.Name name, int typeParamCount = 0, S.FuncParamInfo? paramInfo = null)
        {
            string MakeParamHash()
            {
                if (paramInfo == null) return string.Empty;
                
                var builder = ImmutableArray.CreateBuilder<M.Type>();
                foreach (var param in paramInfo.Value.Parameters)
                {
                    var mtype = GetMType(param.Type);
                    if (mtype == null) throw new FatalException();

                    builder.Add(mtype);
                }

                return Misc.MakeParamHash(builder.ToImmutable());
            }

            var paramHash = MakeParamHash();

            if (typeBuilder != null)
            {
                var typePath = typeBuilder.GetTypePath();
                return typePath.Append(name, typeParamCount, paramHash);
            }
            else
            {
                // TODO: namespace 고려
                return new ItemPath(M.NamespacePath.Root, name, typeParamCount, paramHash);
            }
        }

        M.Type? GetMType(S.TypeExp typeExp)
        {
            var typeExpInfo = typeExpInfoService.GetTypeExpInfo(typeExp);
            if( typeExpInfo is MTypeTypeExpInfo mtypeTypeExpInfo)
                return mtypeTypeExpInfo.Type;

            return null;
        }
        
        void AddTypeInfoToOuter(M.TypeInfo typeInfo)
        {
            if (outerTypeBuilder == null)
                globalTypeInfos.Add(typeInfo);
            else
                outerTypeBuilder.AddTypeInfo(typeInfo);
        }

        void AddMemberVarInfo(M.MemberVarInfo memberVarInfo)
        {
            Debug.Assert(typeBuilder != null);
            typeBuilder.AddMemberVarInfo(memberVarInfo);
        }

        void ExecInNewTypeScope(M.Name name, int typeParamCount, Action action)
        {
            TypeBuilder newBuilder;
            if (typeBuilder != null)
            {
                newBuilder = new TypeBuilder(typeBuilder.GetTypePath().Append(name, typeParamCount));
            }
            else
            {
                // TODO: namespace
                newBuilder = new TypeBuilder(new ItemPath(M.NamespacePath.Root, name, typeParamCount));
            }

            var prevOuterTypeBuilder = outerTypeBuilder;
            outerTypeBuilder = typeBuilder;
            typeBuilder = newBuilder;
            try
            {
                action.Invoke();
            }
            finally
            {
                typeBuilder = outerTypeBuilder;
                outerTypeBuilder = prevOuterTypeBuilder;
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
            //var path = MakePath(enumDecl.Name, enumDecl.TypeParamCount);
            //var elemInfos = new List<EnumElemInfo>();
            //foreach (var elem in enumDecl.Elems)
            //{
            //    var elemInfo = VisitEnumDeclElement(elem);
            //    elemInfos.Add(elemInfo);
            //}

            //var enumInfo = new EnumInfo(
            //    new ItemId(ModuleName.Internal, path),
            //    enumDecl.TypeParams,
            //    elemInfos.ToImmutableArray());

            //AddTypeInfo(enumInfo);
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            ExecInNewTypeScope(structDecl.Name, structDecl.TypeParams.Length, () => {

                // base type
                var mbaseTypesBuilder = ImmutableArray.CreateBuilder<M.Type>(structDecl.BaseTypes.Length);
                foreach (var baseType in structDecl.BaseTypes)
                {
                    var mbaseType = GetMType(baseType);
                    if (mbaseType == null) throw new FatalException();

                    mbaseTypesBuilder.Add(mbaseType);
                }
                var mbaseTypes = mbaseTypesBuilder.MoveToImmutable();

                foreach (var elem in structDecl.Elems)
                {
                    switch (elem)
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

                Debug.Assert(typeBuilder != null);
                var structInfo = typeBuilder.MakeTypeInfo((types, funcs, vars) =>
                    new M.StructInfo(structDecl.Name, structDecl.TypeParams, mbaseTypes, types, funcs, vars));

                AddTypeInfoToOuter(structInfo);
            });            
        }

        void VisitFuncDecl(S.FuncDecl funcDecl)
        {   
            var path = MakePath(funcDecl.Name, funcDecl.TypeParams.Length, funcDecl.ParamInfo);

            bool bThisCall = IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려하도록 한다

            var retType = GetMType(funcDecl.RetType);
            if (retType == null) throw new FatalException();            

            var funcInfo = new M.FuncInfo(
                funcDecl.Name,
                funcDecl.IsSequence,
                bThisCall,
                funcDecl.TypeParams,
                retType,
                funcDecl.ParamInfo.Parameters.Select(typeAndName =>
                {
                    var mtype = GetMType(typeAndName.Type);
                    if (mtype == null) throw new FatalException();

                    return mtype;
                }).ToImmutableArray()
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
            if (declType == null)
                throw new FatalException();

            foreach(var name in varDeclElem.VarNames)
            {
                bool bStatic = !IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려한다

                var varInfo = new M.MemberVarInfo(bStatic, declType, name);
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