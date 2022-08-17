using Citron.Collections;
using Citron.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron
{   
    class IR0LocalContext
    {
        ImmutableDictionary<Name, Value> localVars;
        ImmutableArray<Task> tasks;

        public IR0LocalContext(IR0LocalContext other)
        {
            this.localVars = other.localVars;
            this.tasks = other.tasks;
        }

        public IR0LocalContext(ImmutableDictionary<Name, Value> localVars, ImmutableArray<Task> tasks)
        {
            this.localVars = localVars;
            this.tasks = tasks;
        }

        public Value GetLocalValue(Name name)
        {
            return localVars[name];
        }

        public void AddLocalVar(Name name, Value value)
        {
            localVars = localVars.SetItem(name, value);
        }

        public void AddTask(Task task)
        {
            tasks = tasks.Add(task);
        }

        public Task WaitAllAsync()
        {
            return Task.WhenAll(tasks.AsEnumerable());
        }

        public IR0LocalContext NewTaskLocalContext()
        {
            return new IR0LocalContext(localVars, default);
        }
    }
}
