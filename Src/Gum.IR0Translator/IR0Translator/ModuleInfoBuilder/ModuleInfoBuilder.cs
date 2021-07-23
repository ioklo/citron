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

namespace Gum.IR0Translator
{
    // Script에서 ModuleInfo 정보를 뽑는 역할
    partial struct ModuleInfoBuilder
    {
        TypeExpInfoService typeExpInfoService;

        // Global일 경우 null
        bool bInsideTypeScope;
        ItemPath? itemPath;
        List<M.ConstructorInfo> constructors;
        List<M.TypeInfo> types;
        List<M.FuncInfo> funcs;
        List<M.MemberVarInfo> memberVars;

        public static M.ModuleInfo Build(M.ModuleName moduleName, S.Script script, TypeExpInfoService typeExpTypeValueService)
        {
            var builder = new ModuleInfoBuilder(typeExpTypeValueService, false, null);

            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case S.TypeDeclScriptElement typeDeclElem:
                        builder.VisitTypeDecl(typeDeclElem.TypeDecl);
                        break;

                    case S.GlobalFuncDeclScriptElement globalFuncDeclElem:
                        builder.VisitFuncDecl(globalFuncDeclElem.FuncDecl);
                        break;

                    // skip
                    // case S.StmtScriptElement stmtScriptElem: 
                }
            }

            var moduleInfo = new M.ModuleInfo(moduleName,
                ImmutableArray<M.NamespaceInfo>.Empty,
                builder.types.ToImmutableArray(),
                builder.funcs.ToImmutableArray());

            return moduleInfo;
        }

        ModuleInfoBuilder(TypeExpInfoService typeExpInfoService, bool bInsideTypeScope, ItemPath? itemPath)
        {
            this.typeExpInfoService = typeExpInfoService;
            this.bInsideTypeScope = bInsideTypeScope;
            this.itemPath = itemPath;
            
            types = new List<M.TypeInfo>();
            funcs = new List<M.FuncInfo>();
            constructors = new List<M.ConstructorInfo>();
            memberVars = new List<M.MemberVarInfo>();
        }

        M.Type? GetMType(TypeExpInfo typeExpInfo)
        {
            switch (typeExpInfo)
            {
                case MTypeTypeExpInfo mtypeTypeExpInfo:
                    return mtypeTypeExpInfo.Type;
            }

            return null;
        }

        M.Type? GetMType(S.TypeExp typeExp)
        {
            var typeExpInfo = typeExpInfoService.GetTypeExpInfo(typeExp);
            return GetMType(typeExpInfo);
        }                
        
        // 현재 위치가 Type안에 있는지
        bool IsInsideTypeScope()
        {
            return bInsideTypeScope;
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
            var elemsBuilder = ImmutableArray.CreateBuilder<M.EnumElemInfo>(enumDecl.Elems.Length);
            foreach(var elem in enumDecl.Elems)
            {
                var fieldsBuilder = ImmutableArray.CreateBuilder<M.MemberVarInfo>(elem.Fields.Length);
                foreach(var field in elem.Fields)
                {
                    var type = GetMType(field.Type);
                    Debug.Assert(type != null);

                    var mfield = new M.MemberVarInfo(false, type, field.Name);
                    fieldsBuilder.Add(mfield);
                }

                var fields = fieldsBuilder.MoveToImmutable();
                var enumElemInfo = new M.EnumElemInfo(elem.Name, fields);
                elemsBuilder.Add(enumElemInfo);
            }

            var elems = elemsBuilder.MoveToImmutable();
            var enumInfo = new M.EnumInfo(enumDecl.Name, enumDecl.TypeParams, elems);
            types.Add(enumInfo);
        }

        ModuleInfoBuilder NewModuleInfoBuilder(M.Name name, int typeParamCount, bool bInsideTypeScope)
        {
            ItemPath newItemPath;

            if (itemPath != null)
                newItemPath = itemPath.Value.Append(name, typeParamCount);
            else
                newItemPath = new ItemPath(M.NamespacePath.Root, name, typeParamCount);

            return new ModuleInfoBuilder(typeExpInfoService, bInsideTypeScope, newItemPath);
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            var newBuilder = NewModuleInfoBuilder(structDecl.Name, structDecl.TypeParamCount, true);
            
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

            foreach (var elem in structDecl.Elems)
            {
                switch (elem)
                {
                    case S.FuncStructDeclElement funcDeclElem:
                        newBuilder.VisitFuncDecl(funcDeclElem.FuncDecl);
                        break;

                    case S.TypeStructDeclElement typeDeclElem:
                        newBuilder.VisitTypeDecl(typeDeclElem.TypeDecl);
                        break;

                    case S.VarStructDeclElement varDeclElem:
                        newBuilder.VisitStructVarDeclElement(varDeclElem);
                        break;

                    case S.ConstructorStructDeclElement constructorDeclElem:
                        newBuilder.VisitStructConstructorDeclElement(constructorDeclElem);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }
            
            var structInfo = new M.StructInfo(
                structDecl.Name, structDecl.TypeParams, null, default, 
                newBuilder.types.ToImmutableArray(), 
                newBuilder.funcs.ToImmutableArray(), 
                newBuilder.memberVars.ToImmutableArray(),
                newBuilder.constructors.ToImmutableArray());

            types.Add(structInfo);
        }

        ImmutableArray<M.Param> MakeParams(ImmutableArray<S.FuncParam> sparams)
        {
            var builder = ImmutableArray.CreateBuilder<M.Param>(sparams.Length);
            foreach(var sparam in sparams)
            {
                var mtype = GetMType(sparam.Type);
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

        void VisitFuncDecl(S.FuncDecl funcDecl)
        {   
            bool bThisCall = IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려하도록 한다

            var retType = GetMType(funcDecl.RetType);
            if (retType == null) throw new FatalException();

            var paramInfo = MakeParams(funcDecl.Parameters);

            var funcInfo = new M.FuncInfo(
                funcDecl.Name,
                funcDecl.IsSequence,
                funcDecl.IsRefReturn,
                bThisCall,
                funcDecl.TypeParams,
                retType,
                paramInfo
            );

            funcs.Add(funcInfo);
        }

        void VisitStructVarDeclElement(S.VarStructDeclElement varDeclElem)
        {
            var declType = GetMType(varDeclElem.VarType);
            if (declType == null)
                throw new FatalException();

            foreach(var name in varDeclElem.VarNames)
            {
                bool bStatic = !IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려한다

                var varInfo = new M.MemberVarInfo(bStatic, declType, name);
                memberVars.Add(varInfo);
            }
        }

        void VisitStructConstructorDeclElement(S.ConstructorStructDeclElement constructorDeclElem)
        {
            var paramInfo = MakeParams(constructorDeclElem.Parameters);

            constructors.Add(new M.ConstructorInfo(paramInfo));

            // memberVars.Add(new M.ConstructorInfo)
        }
    }
}