using Gum.Analysis;
using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    partial class InternalModuleClassInfo : IModuleClassInfo
    {
        M.Name name;
        ImmutableArray<string> typeParams;
        M.Type? mbaseClass;        
        ImmutableArray<M.Type> interfaceTypes;
        ImmutableArray<IModuleTypeInfo> types;
        ImmutableArray<IModuleFuncInfo> funcs;
        ImmutableArray<IModuleConstructorInfo> constructors;        
        ImmutableArray<IModuleMemberVarInfo> memberVars;
        
        IClassTypeValue? baseClass; // Class선언 시점 typeEnv를 적용한 baseClass
        IModuleConstructorInfo? trivialConstructor;

        ModuleInfoBuildState state;

        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public InternalModuleClassInfo(
            M.Name name, ImmutableArray<string> typeParams,
            M.Type? mbaseClass,
            ImmutableArray<M.Type> interfaceTypes,
            ImmutableArray<IModuleTypeInfo> types,
            ImmutableArray<IModuleFuncInfo> funcs,
            ImmutableArray<IModuleConstructorInfo> constructors,
            ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            this.name = name;
            this.typeParams = typeParams;
            this.mbaseClass = mbaseClass;            
            this.interfaceTypes = interfaceTypes;
            this.types = types;
            this.funcs = funcs;
            this.constructors = constructors;            
            this.memberVars = memberVars;

            this.baseClass = null;
            this.trivialConstructor = null;
            this.state = ModuleInfoBuildState.BeforeSetBaseAndBuildTrivialConstructor;

            this.typeDict = new ModuleTypeDict(types);
            this.funcDict = new ModuleFuncDict(funcs);
        }
        
        // trivial constructor를 하려면
        public void SetBaseAndBuildTrivialConstructor(IQueryModuleTypeInfo query, IItemValueFactoryByMType factory) // throws InvalidOperationException
        {
            if (state == ModuleInfoBuildState.Completed) return;
            if (state == ModuleInfoBuildState.DuringSetBaseAndBuildTrivialConstructor)
                throw new InvalidOperationException();

            Debug.Assert(state == ModuleInfoBuildState.BeforeSetBaseAndBuildTrivialConstructor);
            state = ModuleInfoBuildState.DuringSetBaseAndBuildTrivialConstructor;

            IModuleClassInfo? baseClassInfo = null;
            // base 클래스가 있다면
            if (mbaseClass != null)
            {
                // M.Type -> ItemPath
                // X<bool>.Dict<int, S> -> X<>.List<,>
                // ClassValue(X<>.List<,>, [bool, int, S]) 을 만들어야 한다
                var baseClassPath = mbaseClass.ToItemPath();

                baseClassInfo = query.GetClass(baseClassPath);
                baseClass = factory.MakeClassTypeValue(mbaseClass);
            }

            var baseTrivialConstructor = baseClassInfo?.GetTrivialConstructor();

            // baseClass가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
            // baseClass가 있고, TrivialConstructor가 있는 경우 => 진행
            // baseClass가 없는 경우 => 없이 만들고 진행 
            if (baseTrivialConstructor != null || baseClassInfo == null)
            {
                // 같은 인자의 생성자가 없으면 Trivial을 만든다
                if (InternalModuleTypeInfoBuilderMisc.GetConstructorHasSameParamWithTrivial(baseTrivialConstructor, constructors, memberVars) == null)
                {
                    trivialConstructor = InternalModuleTypeInfoBuilderMisc.MakeTrivialConstructor(baseTrivialConstructor, memberVars);
                    constructors = constructors.Add(trivialConstructor);
                }
            }

            state = ModuleInfoBuildState.Completed;
        }

        IModuleConstructorInfo? IModuleClassInfo.GetTrivialConstructor()
        {
            Debug.Assert(state == ModuleInfoBuildState.Completed);
            return trivialConstructor;
        }        

        // Info자체에는 environment가 없으므로, typeEnv가 있어야
        IClassTypeValue? IModuleClassInfo.GetBaseClass()
        {
            Debug.Assert(state == ModuleInfoBuildState.Completed);
            return baseClass;
        }

        ImmutableArray<IModuleConstructorInfo> IModuleClassInfo.GetConstructors()
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

        ImmutableArray<IModuleFuncInfo> IModuleClassInfo.GetMemberFuncs()
        {
            return funcs;
        }

        ImmutableArray<IModuleTypeInfo> IModuleClassInfo.GetMemberTypes()
        {
            return types;
        }

        ImmutableArray<IModuleMemberVarInfo> IModuleClassInfo.GetMemberVars()
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
