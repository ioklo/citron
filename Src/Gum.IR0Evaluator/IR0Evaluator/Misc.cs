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
        public static R.ParamHash MakeParamHash(int typeParamCount, ImmutableArray<R.Param> parameters)
        {
            var paramTypes = ImmutableArray.CreateRange(parameters, param => new R.ParamHashEntry(param.Kind, param.Type));
            return new R.ParamHash(typeParamCount, paramTypes);
        }
    }
}
