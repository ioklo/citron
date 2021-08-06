using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleConstructorInfo : IModuleConstructorInfo
    {
        M.AccessModifier accessModifier;
        M.Name name;        
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

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return default;
        }
    }
}