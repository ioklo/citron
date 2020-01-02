using Gum.CompileTime;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.StaticAnalysis
{
    public partial class Analyzer
    {
        public static class Misc
        {
            public static ImmutableArray<TypeValue> GetTypeValues(ImmutableArray<TypeExp> typeExps, Context context)
            {
                return ImmutableArray.CreateRange(typeExps, typeExp => context.GetTypeValueByTypeExp(typeExp));
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
