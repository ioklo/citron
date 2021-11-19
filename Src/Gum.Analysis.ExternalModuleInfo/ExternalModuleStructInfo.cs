using Gum.Collections;
using Pretune;
using System;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    partial class ExternalModuleStructInfo : IModuleStructInfo
    {
        M.StructInfo structInfo;
        StructTypeValue? baseStruct;
        ModuleTypeDict typeDict;
        ModuleInfoBuildState state;

        public ExternalModuleStructInfo(M.StructInfo structInfo)
        {
            this.structInfo = structInfo;
            this.typeDict = new ModuleTypeDict(structInfo.MemberTypes);

            this.baseStruct = null;
            this.state = ModuleInfoBuildState.BeforeSetBaseAndBuildTrivialConstructor;
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleTypeInfo> IModuleStructInfo.GetMemberTypes()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleTypeInfo>(structInfo.MemberTypes.Length);

            foreach (var memberType in structInfo.MemberTypes)
            {
                var t = ExternalModuleMisc.Make(memberType);
                builder.Add(t);
            }

            return builder.MoveToImmutable();
        }

        ImmutableArray<IModuleFuncInfo> IModuleStructInfo.GetMemberFuncs()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleFuncInfo>(structInfo.MemberFuncs.Length);

            foreach(var memberFunc in structInfo.MemberFuncs)
            {
                var f = new ExternalModuleFuncInfo(memberFunc);
                builder.Add(f);
            }

            return builder.MoveToImmutable();
        }

        M.Name IModuleItemInfo.GetName()
        {
            return structInfo.Name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return structInfo.TypeParams;
        }

        ImmutableArray<IModuleConstructorInfo> IModuleStructInfo.GetConstructors()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleConstructorInfo>(structInfo.Constructors.Length);

            foreach (var constructor in structInfo.Constructors)
                builder.Add(new ExternalModuleConstructorInfo(constructor));

            return builder.MoveToImmutable();
        }

        ImmutableArray<IModuleMemberVarInfo> IModuleStructInfo.GetMemberVars()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(structInfo.MemberVars.Length);

            foreach (var memberVar in structInfo.MemberVars)
                builder.Add(new ExternalModuleMemberVarInfo(memberVar));

            return builder.MoveToImmutable();
        }

        StructTypeValue? IModuleStructInfo.GetBaseStruct()
        {
            Debug.Assert(state == ModuleInfoBuildState.Completed);
            return baseStruct;
        }

        IModuleConstructorInfo? IModuleStructInfo.GetTrivialConstructor()
        {
            foreach (var constructor in structInfo.Constructors)
                if (constructor.IsTrivial)
                    return new ExternalModuleConstructorInfo(constructor);

            return null;
        }

        ModuleInfoBuildState IModuleStructInfo.GetBuildState()
        {
            return ModuleInfoBuildState.Completed;
        }

        void IModuleStructInfo.SetBaseAndBuildTrivialConstructor(IQueryModuleTypeInfo query, ItemValueFactory itemValueFactory)
        {
            if (state == ModuleInfoBuildState.Completed) return;

            if (structInfo.BaseType == null)
            {
                baseStruct = null;
            }
            else
            {
                baseStruct = (StructTypeValue)itemValueFactory.MakeTypeValueByMType(structInfo.BaseType);
            }

            state = ModuleInfoBuildState.Completed;
            return;
        }
    }
}
