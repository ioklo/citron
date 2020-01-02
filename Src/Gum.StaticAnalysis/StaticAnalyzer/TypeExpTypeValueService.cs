using Gum.CompileTime;
using Gum.Syntax;
using System.Collections.Immutable;

namespace Gum.StaticAnalysis
{
    public class TypeExpTypeValueService
    {
        ImmutableDictionary<TypeExp, TypeValue> typeValuesByTypeExp;

        public TypeExpTypeValueService(ImmutableDictionary<TypeExp, TypeValue> typeValuesByTypeExp)
        {
            this.typeValuesByTypeExp = typeValuesByTypeExp;
        }

        public TypeValue GetTypeValue(TypeExp typeExp)
        {
            return typeValuesByTypeExp[typeExp];
        }
    }
}