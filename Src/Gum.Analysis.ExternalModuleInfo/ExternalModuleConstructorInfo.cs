using Gum.Collections;
using Gum.Infra;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleConstructorInfo : IModuleConstructorInfo
    {
        M.ConstructorInfo constructorInfo;

        M.AccessModifier IModuleCallableInfo.GetAccessModifier()
        {
            return constructorInfo.AccessModifier;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return constructorInfo.Name;
        }

        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return constructorInfo.Parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(constructorInfo.Parameters);
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }
    }
}
