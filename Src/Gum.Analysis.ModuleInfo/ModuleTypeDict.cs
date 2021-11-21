using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ExcludeComparison]
    public partial struct ModuleTypeDict
    {
        ImmutableDictionary<(M.Name Name, int TypeParamCount), IModuleTypeInfo> types;

        public ModuleTypeDict(ImmutableArray<IModuleTypeInfo> types)
        {
            var typesBuilder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount), IModuleTypeInfo>();
            foreach (var type in types)
            {
                typesBuilder.Add((type.GetName(), type.GetTypeParams().Length), type);
            }

            this.types = typesBuilder.ToImmutable();
        }

        public IModuleTypeInfo? Get(M.Name name, int typeParamCount)
        {
            return types.GetValueOrDefault((name, typeParamCount));
        }
    }
}
