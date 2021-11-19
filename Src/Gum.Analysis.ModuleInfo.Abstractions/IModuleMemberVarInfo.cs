using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // M.MemberVarInfo 대체
    public interface IModuleMemberVarInfo : IModuleItemInfo
    {
        M.AccessModifier GetAccessModifier();
        M.Type GetDeclType();
        bool IsStatic();
    }
}