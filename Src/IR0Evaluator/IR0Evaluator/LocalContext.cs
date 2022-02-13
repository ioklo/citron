using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Citron.CompileTime;
using R = Citron.IR0;

namespace Citron.IR0Evaluator
{   
    class LocalContext
    {
        ImmutableDictionary<M.Name, Value> localVars;

        public LocalContext(LocalContext other)
        {
            this.localVars = other.localVars;
        }

        public LocalContext(ImmutableDictionary<M.Name, Value> localVars)
        {
            this.localVars = localVars;
        }

        public Value GetLocalValue(M.Name name)
        {
            return localVars[name];
        }

        public void AddLocalVar(M.Name name, Value value)
        {
            localVars = localVars.SetItem(name, value);
        }
    }
}
