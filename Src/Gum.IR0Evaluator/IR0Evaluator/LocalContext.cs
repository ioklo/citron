using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{   
    class LocalContext
    {
        ImmutableDictionary<R.Name, Value> localVars;

        public LocalContext(LocalContext other)
        {
            this.localVars = other.localVars;
        }

        public LocalContext(ImmutableDictionary<R.Name, Value> localVars)
        {
            this.localVars = localVars;
        }

        public Value GetLocalValue(R.Name name)
        {
            return localVars[name];
        }

        public void AddLocalVar(R.Name name, Value value)
        {
            localVars = localVars.SetItem(name, value);
        }
    }
}
