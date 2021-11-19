using Gum.CompileTime;
using Gum.Collections;

using S = Gum.Syntax;
using Gum.Infra;

namespace Gum.Analysis
{
    public class TypeExpInfoService : IPure
    {
        ImmutableDictionary<S.TypeExp, TypeExpInfo> typeExpInfosByTypeExp;

        public TypeExpInfoService(ImmutableDictionary<S.TypeExp, TypeExpInfo> typeExpInfosByTypeExp)
        {
            this.typeExpInfosByTypeExp = typeExpInfosByTypeExp;
        }

        public void EnsurePure()
        {
        }

        public TypeExpInfo GetTypeExpInfo(S.TypeExp typeExp)
        {
            return typeExpInfosByTypeExp[typeExp];
        }
    }
}