using Gum.CompileTime;
using System.Collections.Immutable;

using S = Gum.Syntax;

namespace Gum.IR0
{
    class TypeExpInfoService
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