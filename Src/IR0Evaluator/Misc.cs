using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Citron.IR0;

namespace Citron
{
    static class Misc
    {
        public static R.ParamHash MakeParamHash(int typeParamCount, ImmutableArray<R.Param> parameters)
        {
            var builder = ImmutableArray.CreateBuilder<R.ParamHashEntry>(parameters.Length);
            foreach (var param in parameters)
                builder.Add(new R.ParamHashEntry(param.Kind, param.Type));

            return new R.ParamHash(typeParamCount, builder.MoveToImmutable());
        }
    }
}
