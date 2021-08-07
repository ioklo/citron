using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class ClassMemberVarRuntimeItem : RuntimeItem
    {
        public abstract Value GetMemberValue(ClassValue classValue);
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0ClassMemberVarRuntimeItem : ClassMemberVarRuntimeItem
        {
            string name;

            public override R.Name Name => name;
            public override R.ParamHash ParamHash => R.ParamHash.None;

            public override Value GetMemberValue(ClassValue classValue)
            {
                return classValue.GetInstance().GetMemberValue(name);
            }
        }
    }
}
