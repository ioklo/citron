using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleMemberVarInfo : IModuleMemberVarInfo
    {   
        M.MemberVarInfo info;

        M.Name IModuleItemInfo.GetName()
        {
            return info.Name;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }

        M.Type IModuleMemberVarInfo.GetDeclType()
        {
            return info.Type; 
        }

        bool IModuleMemberVarInfo.IsStatic()
        {
            return info.IsStatic;
        }

        M.AccessModifier IModuleMemberVarInfo.GetAccessModifier()
        {
            return info.AccessModifier;
        }
    }
}
