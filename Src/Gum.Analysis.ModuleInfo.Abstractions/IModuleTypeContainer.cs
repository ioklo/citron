using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IModuleTypeContainer
    {
        IModuleTypeInfo? GetType(M.Name name, int typeParamCount); // type.Name.Equals(name) && type.TypeParams.Length == typeParamCount)
    }
}