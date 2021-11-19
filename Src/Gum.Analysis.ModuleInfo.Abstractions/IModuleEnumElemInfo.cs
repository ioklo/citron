using Gum.Collections;

namespace Gum.Analysis
{
    // M.EnumElem 대체
    public interface IModuleEnumElemInfo : IModuleTypeInfo
    {
        bool IsStandalone();
        ImmutableArray<IModuleMemberVarInfo> GetFieldInfos();
    }
}