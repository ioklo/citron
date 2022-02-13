using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Citron.IR0;

namespace Citron.IR0Evaluator
{
    // Evaluation 중에 벌어지는 일들을 추상화
    public abstract class RuntimeItem
    {
        public abstract R.Name Name { get; }
        public abstract R.ParamHash ParamHash { get; }
    }
    
    public abstract class AllocatableRuntimeItem : RuntimeItem
    {
        public abstract Value Alloc(TypeContext typeContext);
    }
}
