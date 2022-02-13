using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0Evaluator
{
    class LocalTaskContext
    {
        ImmutableArray<Task> tasks;
        
        public void AddTask(Task task)
        {
            tasks = tasks.Add(task);
        }

        public Task WaitAllAsync()
        {
            return Task.WhenAll(tasks.AsEnumerable());
        }
    }
}
