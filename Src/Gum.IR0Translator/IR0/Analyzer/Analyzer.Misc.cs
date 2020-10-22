using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {
        public static class Misc
        {
            public static ImmutableArray<TypeValue> GetTypeValues(IEnumerable<S.TypeExp> typeExps, Context context)
            {
                return typeExps.Select(typeExp => context.GetTypeValueByTypeExp(typeExp)).ToImmutableArray();
            }

            public static bool IsVarStatic(ModuleItemId varId, Context context)
            {
                var varInfo = context.ModuleInfoService.GetVarInfos(varId).Single();
                return varInfo.bStatic;
            }

            public static bool IsFuncStatic(ModuleItemId funcId, Context context)
            {
                var funcInfo = context.ModuleInfoService.GetFuncInfos(funcId).Single();
                return !funcInfo.bThisCall;
            }
        }
    }
}
