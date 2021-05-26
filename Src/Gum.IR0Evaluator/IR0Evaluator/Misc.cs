using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    static class Misc
    {
        public static R.ParamHash MakeParamHash(int typeParamCount, R.ParamInfo paramInfo)
        {
            var paramTypes = ImmutableArray.CreateRange(paramInfo.Parameters, param => param.Type);
            return new R.ParamHash(typeParamCount, paramTypes);
        }
    }
}
