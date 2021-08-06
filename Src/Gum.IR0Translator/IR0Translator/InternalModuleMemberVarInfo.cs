using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleMemberVarInfo : IModuleMemberVarInfo
    {
        M.AccessModifier accessModifier;
        bool bStatic;
        M.Type declType;
        M.Name name;

        M.AccessModifier IModuleMemberVarInfo.GetAccessModifier()
        {
            return accessModifier;
        }

        M.Type IModuleMemberVarInfo.GetDeclType()
        {
            return declType;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }

        bool IModuleMemberVarInfo.IsStatic()
        {
            return bStatic;
        }
    }
}