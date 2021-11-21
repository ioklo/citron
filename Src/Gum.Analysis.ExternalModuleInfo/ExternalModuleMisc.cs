using Gum.Collections;
using Gum.Infra;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    static class ExternalModuleMisc
    {
        public static ModuleTypeDict MakeModuleTypeDict(ImmutableArray<M.TypeInfo> mtypeInfos)
        {
            var typeInfosBuilder = ImmutableArray.CreateBuilder<IModuleTypeInfo>();
            foreach (var mtypeInfo in mtypeInfos)
            {
                var typeInfo = Make(mtypeInfo);
                typeInfosBuilder.Add(typeInfo);
            }
            return new ModuleTypeDict(typeInfosBuilder.ToImmutable());
        }

        public static ModuleFuncDict MakeModuleFuncDict(ImmutableArray<M.FuncInfo> mfuncInfos)
        {
            var funcInfosBuilder = ImmutableArray.CreateBuilder<IModuleFuncInfo>();
            foreach(var mfuncInfo in mfuncInfos)
            {
                var funcInfo = new ExternalModuleFuncInfo(mfuncInfo);
                funcInfosBuilder.Add(funcInfo);
            }

            return new ModuleFuncDict(funcInfosBuilder.ToImmutable());
        }

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
