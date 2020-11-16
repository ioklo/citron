using Gum.CompileTime;
using System.Collections.Immutable;

using S = Gum.Syntax;

namespace Gum.IR0
{
    public class TypeExpTypeValueService
    {
        ImmutableDictionary<S.TypeExp, TypeValue> typeValuesByTypeExp;
        ImmutableDictionary<S.Exp, TypeValue> typeValuesByExp;        

        public TypeExpTypeValueService(
            ImmutableDictionary<S.TypeExp, TypeValue> typeValuesByTypeExp,
            ImmutableDictionary<S.Exp, TypeValue> typeValuesByExp)
        {
            this.typeValuesByTypeExp = typeValuesByTypeExp;
            this.typeValuesByExp = typeValuesByExp;
        }

        public TypeValue GetTypeValue(S.TypeExp typeExp)
        {
            return typeValuesByTypeExp[typeExp];
        }

        public TypeValue? GetTypeValue(S.Exp exp)
        {
            return typeValuesByExp.GetValueOrDefault(exp);
        }
    }
}