using Gum.Collections;

namespace Gum.Analysis
{
    public interface IModuleStructInfo : IModuleTypeInfo
    {
        IModuleConstructorInfo? GetTrivialConstructor();
        ImmutableArray<IModuleTypeInfo> GetMemberTypes();
        ImmutableArray<IModuleFuncInfo> GetMemberFuncs();
        ImmutableArray<IModuleConstructorInfo> GetConstructors();
        ImmutableArray<IModuleMemberVarInfo> GetMemberVars();
        StructTypeValue? GetBaseStruct();
    }
}