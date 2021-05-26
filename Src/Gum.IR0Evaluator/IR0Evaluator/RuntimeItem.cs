using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    // Evaluation 중에 벌어지는 일들을 추상화
    abstract class RuntimeItem
    {
        public abstract R.Name Name { get; }
        public abstract R.ParamHash ParamHash { get; }
    }
}
