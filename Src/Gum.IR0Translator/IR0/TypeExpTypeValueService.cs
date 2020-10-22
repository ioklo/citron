using Gum.CompileTime;
using System.Collections.Immutable;

using S = Gum.Syntax;

namespace Gum.IR0
{
    public class TypeExpTypeValueService
    {
        ImmutableDictionary<S.TypeExp, TypeValue> typeValuesByTypeExp;

        public TypeExpTypeValueService(ImmutableDictionary<S.TypeExp, TypeValue> typeValuesByTypeExp)
        {
            this.typeValuesByTypeExp = typeValuesByTypeExp;
        }

        public TypeValue GetTypeValue(S.TypeExp typeExp)
        {
            return typeValuesByTypeExp[typeExp];
        }
    }
}