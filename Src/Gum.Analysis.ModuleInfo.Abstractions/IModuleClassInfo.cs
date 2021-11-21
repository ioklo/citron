using Gum.Collections;

namespace Gum.Analysis
{
    public interface IModuleClassInfo : IModuleTypeInfo
    {
        IModuleConstructorInfo? GetTrivialConstructor();

        ImmutableArray<IModuleTypeInfo> GetMemberTypes();
        ImmutableArray<IModuleFuncInfo> GetMemberFuncs();
        ImmutableArray<IModuleConstructorInfo> GetConstructors();
        ImmutableArray<IModuleMemberVarInfo> GetMemberVars();
        IClassTypeValue? GetBaseClass();
    }
}