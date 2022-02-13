using Citron.Collections;

using S = Citron.Syntax;
using Citron.Infra;

namespace Citron.Analysis
{
    public class TypeExpInfoService
    {
        ImmutableDictionary<S.TypeExp, TypeExpInfo> typeExpInfosByTypeExp;

        public TypeExpInfoService(ImmutableDictionary<S.TypeExp, TypeExpInfo> typeExpInfosByTypeExp)
        {
            this.typeExpInfosByTypeExp = typeExpInfosByTypeExp;
        }

        public TypeExpInfo GetTypeExpInfo(S.TypeExp typeExp)
        {
            return typeExpInfosByTypeExp[typeExp];
        }
    }
}