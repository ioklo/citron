using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IModuleEnumInfo : IModuleTypeInfo
    {
        IModuleEnumElemInfo? GetElem(M.Name memberName);
    }
}