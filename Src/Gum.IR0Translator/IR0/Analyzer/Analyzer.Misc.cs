using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

            public static bool IsVarStatic(ItemId varId, Context context)
            {
                var varInfo = context.GetItem<VarInfo>(varId);
                Debug.Assert(varInfo != null);

                return varInfo.bStatic;
            }

            public static bool IsFuncStatic(ItemId funcId, Context context)
            {
                var funcInfo = context.GetItem<FuncInfo>(funcId);
                Debug.Assert(funcInfo != null);

                return !funcInfo.bThisCall;
            }
        }
    }
}
