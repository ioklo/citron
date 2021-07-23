using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0Evaluator
{   
    class LocalContext
    {
        ImmutableDictionary<string, Value> localVars;

        public LocalContext(LocalContext other)
        {
            this.localVars = other.localVars;
        }

        public LocalContext(ImmutableDictionary<string, Value> localVars)
        {
            this.localVars = localVars;
        }

        public Value GetLocalValue(string name)
        {
            return localVars[name];
        }

        public void AddLocalVar(string name, Value value)
        {
            localVars = localVars.SetItem(name, value);
        }
    }
}
