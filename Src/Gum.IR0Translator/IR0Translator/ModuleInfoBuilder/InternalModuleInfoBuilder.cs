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

namespace Gum.IR0Translator
{
    // Script에서 ModuleInfo 정보를 뽑는 역할
    partial struct InternalModuleInfoBuilder
    {
        TypeExpInfoService typeExpInfoService;

        // Global일 경우 null
        bool bInsideTypeScope;
        ItemPath? itemPath;        
        List<IModuleTypeInfo> types;
        List<IModuleFuncInfo> funcs;
        List<IModuleConstructorInfo> constructors;
        List<IModuleMemberVarInfo> memberVars;
        IModuleConstructorInfo? automaticConstructor;

        bool bCollectingMemberVarsCompleted; // false

        public static InternalModuleInfo Build(M.ModuleName moduleName, S.Script script, TypeExpInfoService typeExpTypeValueService)
        {
            var builder = new InternalModuleInfoBuilder(typeExpTypeValueService, false, null);

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

            return new InternalModuleInfo(moduleName, builder.types, builder.funcs);
        }

        InternalModuleInfoBuilder(TypeExpInfoService typeExpInfoService, bool bInsideTypeScope, ItemPath? itemPath)
        {
            this.typeExpInfoService = typeExpInfoService;
            this.bInsideTypeScope = bInsideTypeScope;
            this.itemPath = itemPath;

            types = new List<IModuleTypeInfo>();
            funcs = new List<IModuleFuncInfo>();
            constructors = new List<IModuleConstructorInfo>();
            memberVars = new List<IModuleMemberVarInfo>();

            this.automaticConstructor = null;
            this.bCollectingMemberVarsCompleted = false;
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
            var elemsBuilder = ImmutableArray.CreateBuilder<InternalModuleEnumElemInfo>(enumDecl.Elems.Length);
            foreach(var elem in enumDecl.Elems)
            {
                var fieldsBuilder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(elem.Fields.Length);
                foreach(var field in elem.Fields)
                {
                    var type = GetMType(field.Type);
                    Debug.Assert(type != null);

                    var mfield = new InternalModuleMemberVarInfo(false, type, field.Name);
                    fieldsBuilder.Add(mfield);
                }

                var fields = fieldsBuilder.MoveToImmutable();
                var enumElemInfo = new InternalModuleEnumElemInfo(elem.Name, fields);
                elemsBuilder.Add(enumElemInfo);
            }

            var elems = elemsBuilder.MoveToImmutable();
            var enumInfo = new InternalModuleEnumInfo(enumDecl.Name, enumDecl.TypeParams, elems);

            types.Add(enumInfo);
        }

        InternalModuleInfoBuilder NewModuleInfoBuilder(M.Name name, int typeParamCount, bool bInsideTypeScope)
        {
            ItemPath newItemPath;

            if (itemPath != null)
                newItemPath = itemPath.Value.Append(name, typeParamCount);
            else
                newItemPath = new ItemPath(M.NamespacePath.Root, name, typeParamCount);

            return new InternalModuleInfoBuilder(typeExpInfoService, bInsideTypeScope, newItemPath);
        }

        bool IsMatchAutomaticConstructorParameter(M.ParamTypes paramTypes)
        {
            if (memberVars.Count != paramTypes.Length) return false;

            for (int i = 0; i < memberVars.Count; i++)
            {
                // normal로만 자동으로 생성한다
                if (paramTypes[i].Kind != M.ParamKind.Normal) return false;

                var memberVarType = memberVars[i].GetDeclType();
                var paramType = paramTypes[i].Type;

                if (!memberVarType.Equals(paramType)) return false;
            }

            return true;
        }

        void TryMakeAutomaticConstructor(M.Name structName)
        {
            // 꼭, memberVar수집이 완료된 다음에 해야한다
            Debug.Assert(bCollectingMemberVarsCompleted);

            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            foreach (var constructor in constructors)
            {   
                if (IsMatchAutomaticConstructorParameter(constructor.GetParamTypes()))
                    return;
            }
         
            var builder = ImmutableArray.CreateBuilder<M.Param>(memberVars.Count);            
            foreach(var memberVar in memberVars)
            {
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new M.Param(M.ParamKind.Normal, type, name);
                builder.Add(param);
            }

            // automatic constructor를 만듭니다
            automaticConstructor = new InternalModuleConstructorInfo(structName, builder.MoveToImmutable());
            constructors.Add(automaticConstructor);
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

            newBuilder.bCollectingMemberVarsCompleted = true;
            newBuilder.TryMakeAutomaticConstructor(structDecl.Name);
            
            var structInfo = new InternalModuleStructInfo(
                structDecl.Name, structDecl.TypeParams, null,
                newBuilder.types, 
                newBuilder.funcs,
                newBuilder.constructors.ToImmutableArray(),
                newBuilder.automaticConstructor,
                newBuilder.memberVars.ToImmutableArray());

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

            var funcInfo = new InternalModuleFuncInfo(
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

        void VisitStructVarDeclElement(S.VarStructDeclElement varDeclElem)
        {
            var declType = GetMType(varDeclElem.VarType);
            if (declType == null)
                throw new FatalException();

            foreach(var name in varDeclElem.VarNames)
            {
                bool bStatic = !IsInsideTypeScope(); // TODO: static 키워드가 추가되면 고려한다

                var varInfo = new InternalModuleMemberVarInfo(bStatic, declType, name);
                memberVars.Add(varInfo);
            }
        }

        void VisitStructConstructorDeclElement(S.ConstructorStructDeclElement constructorDeclElem)
        {
            var paramInfo = MakeParams(constructorDeclElem.Parameters);
            constructors.Add(new InternalModuleConstructorInfo(constructorDeclElem.Name, paramInfo));
        }
    }
}