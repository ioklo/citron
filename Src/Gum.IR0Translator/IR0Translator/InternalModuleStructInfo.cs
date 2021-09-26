using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    [ImplementIEquatable]
    partial class InternalModuleStructInfo : IModuleStructInfo
    {
        M.Name name;
        ImmutableArray<string> typeParams;
        M.Type? mbaseStruct;
        ImmutableArray<IModuleTypeInfo> types;
        ImmutableArray<IModuleFuncInfo> funcs;
        ImmutableArray<IModuleConstructorInfo> constructors;        
        ImmutableArray<IModuleMemberVarInfo> memberVars;
        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        // state따라 valid 하지 않을수 있다
        StructTypeValue? baseStruct;
        IModuleConstructorInfo? trivialConstructor;

        ModuleInfoBuildState state;

        public InternalModuleStructInfo(
            M.Name name, ImmutableArray<string> typeParams,
            M.Type? mbaseStruct,
            ImmutableArray<IModuleTypeInfo> types,
            ImmutableArray<IModuleFuncInfo> funcs,
            ImmutableArray<IModuleConstructorInfo> constructors,
            ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            this.name = name;
            this.typeParams = typeParams;
            this.mbaseStruct = mbaseStruct;
            this.types = types;
            this.funcs = funcs;
            this.constructors = constructors;            
            this.memberVars = memberVars;

            this.typeDict = new ModuleTypeDict(types);
            this.funcDict = new ModuleFuncDict(funcs);

            this.baseStruct = null;
            this.trivialConstructor = null;
            this.state = ModuleInfoBuildState.BeforeSetBaseAndBuildTrivialConstructor;
        }

        ModuleInfoBuildState IModuleStructInfo.GetBuildState()
        {
            return state;
        }

        void IModuleStructInfo.SetBaseAndBuildTrivialConstructor(IQueryModuleTypeInfo query, ItemValueFactory itemValueFactory) // throws InvalidOperation
        {
            if (state == ModuleInfoBuildState.Completed) return;

            if (state == ModuleInfoBuildState.DuringSetBaseAndBuildTrivialConstructor)
                throw new InvalidOperationException();

            Debug.Assert(state == ModuleInfoBuildState.BeforeSetBaseAndBuildTrivialConstructor);
            state = ModuleInfoBuildState.DuringSetBaseAndBuildTrivialConstructor;

            IModuleStructInfo? baseStructInfo = null;
            if (mbaseStruct != null)
            {
                baseStructInfo = query.GetStruct(mbaseStruct.ToItemPath());
                baseStruct = (StructTypeValue)itemValueFactory.MakeTypeValueByMType(mbaseStruct);
            }

            var baseTrivialConstructor = baseStructInfo?.GetTrivialConstructor();

            // baseClass가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
            // baseClass가 있고, TrivialConstructor가 있는 경우 => 진행
            // baseClass가 없는 경우 => 없이 만들고 진행 
            if (baseTrivialConstructor != null || baseStruct == null)
            {
                // 같은 인자의 생성자가 없으면 Trivial을 만든다
                if (InternalModuleTypeInfoBuilderMisc.GetConstructorHasSameParamWithTrivial(baseTrivialConstructor, constructors, memberVars) == null)
                {
                    trivialConstructor = InternalModuleTypeInfoBuilderMisc.MakeTrivialConstructor(baseTrivialConstructor, memberVars);
                    constructors = constructors.Add(trivialConstructor);
                }
            }

            state = ModuleInfoBuildState.Completed;
            return;
        }

        IModuleConstructorInfo? IModuleStructInfo.GetTrivialConstructor()
        {
            return trivialConstructor;
        }

        StructTypeValue? IModuleStructInfo.GetBaseStruct()
        {
            Debug.Assert(state == ModuleInfoBuildState.Completed);
            return baseStruct;
        }

        ImmutableArray<IModuleConstructorInfo> IModuleStructInfo.GetConstructors()
        {
            return constructors;
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        ImmutableArray<IModuleFuncInfo> IModuleStructInfo.GetMemberFuncs()
        {
            return funcs;
        }

        ImmutableArray<IModuleTypeInfo> IModuleStructInfo.GetMemberTypes()
        {
            return types;
        }

        ImmutableArray<IModuleMemberVarInfo> IModuleStructInfo.GetMemberVars()
        {
            return memberVars;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return typeParams;
        }
    }
}