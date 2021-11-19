using Gum.Collections;
using Gum.Infra;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleFuncInfo : IModuleFuncInfo
    {
        M.FuncInfo funcInfo;

        M.AccessModifier IModuleCallableInfo.GetAccessModifier()
        {
            return funcInfo.AccessModifier;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return funcInfo.Name;
        }

        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return funcInfo.Parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(funcInfo.Parameters);
        }

        M.Type IModuleFuncInfo.GetReturnType()
        {
            return funcInfo.RetType;
        }
        
        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return funcInfo.TypeParams;
        }

        bool IModuleFuncInfo.IsInstanceFunc()
        {
            return funcInfo.IsInstanceFunc;
        }

        bool IModuleFuncInfo.IsInternal()
        {
            return false;
        }

        bool IModuleFuncInfo.IsSequenceFunc()
        {
            return funcInfo.IsSequenceFunc;
        }        
    }
}
