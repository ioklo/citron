using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class StructMemberVarRuntimeItem : RuntimeItem
    {
        public abstract Value GetMemberValue(StructValue structValue);
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0StructMemberVarRuntimeItem : StructMemberVarRuntimeItem
        {
            public override R.Name Name => name;
            public override R.ParamHash ParamHash => R.ParamHash.None;

            string name;

            public override Value GetMemberValue(StructValue structValue)
            {
                return structValue.GetMemberValue(name);
            }
        }
    }
}
