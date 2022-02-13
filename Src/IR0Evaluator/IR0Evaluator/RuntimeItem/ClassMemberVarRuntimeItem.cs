using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Citron.IR0;

namespace Citron.IR0Evaluator
{
    abstract class ClassMemberVarRuntimeItem : RuntimeItem
    {
        public abstract Value GetMemberValue(ClassValue classValue);
    }

    
        partial class IR0ClassMemberVarRuntimeItem : ClassMemberVarRuntimeItem
        {   
            string name;

            Lazy<IR0ClassRuntimeItem> classItem;
            Lazy<int> globalIndex;

            public override R.Name Name => new R.Name.Normal(name);
            public override R.ParamHash ParamHash => R.ParamHash.None;

            public IR0ClassMemberVarRuntimeItem(IR0GlobalContext globalContext, R.Path.Nested classPath, string name, int localIndex)
            {
                this.name = name;

                classItem = new Lazy<IR0ClassRuntimeItem>(() => globalContext.GetRuntimeItem<IR0ClassRuntimeItem>(classPath));
                globalIndex = new Lazy<int>(() => classItem.Value.GetBaseIndex() + localIndex);
            }

            public override Value GetMemberValue(ClassValue classValue)
            {
                return classValue.GetInstance().GetMemberValue(globalIndex.Value);
            }
        }
}
