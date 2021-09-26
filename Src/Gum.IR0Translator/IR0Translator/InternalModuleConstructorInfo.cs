using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleConstructorInfo : IModuleConstructorInfo
    {
        M.AccessModifier accessModifier;
        ImmutableArray<M.Param> parameters;

        M.AccessModifier IModuleCallableInfo.GetAccessModifier()
        {
            return accessModifier;
        }
        
        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(parameters);
        }

        M.Name IModuleItemInfo.GetName()
        {
            return M.Name.Constructor;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return default;
        }
    }
}