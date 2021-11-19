using Gum.Infra;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    static class ExternalModuleMisc
    {
        public static IModuleTypeInfo Make(M.TypeInfo typeInfo)
        {
            switch(typeInfo)
            {
                case M.StructInfo structInfo:
                    return new ExternalModuleStructInfo(structInfo);

                case M.EnumInfo enumInfo:
                    return new ExternalModuleEnumInfo(enumInfo);

                case M.EnumElemInfo enumElemInfo:
                    return new ExternalModuleEnumElemInfo(enumElemInfo);

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}
