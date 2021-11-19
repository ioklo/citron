using Gum.Analysis;
using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleFuncInfo : IModuleFuncInfo
    {
        M.AccessModifier accessModifier;
        bool bInstanceFunc;
        bool bSeqFunc;
        bool bRefReturn;
        M.Type retType;        
        M.Name name;
        ImmutableArray<string> typeParams;
        ImmutableArray<M.Param> parameters;

        M.AccessModifier IModuleCallableInfo.GetAccessModifier()
        {
            return accessModifier;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(parameters);
        }

        M.Type IModuleFuncInfo.GetReturnType()
        {
            return retType;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return typeParams;
        }

        bool IModuleFuncInfo.IsInstanceFunc()
        {
            return bInstanceFunc;
        }

        bool IModuleFuncInfo.IsInternal()
        {
            return true;
        }

        bool IModuleFuncInfo.IsSequenceFunc()
        {
            return bSeqFunc;
        }
    }
}