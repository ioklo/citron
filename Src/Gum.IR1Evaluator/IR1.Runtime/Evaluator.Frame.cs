using Gum.IR1;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR1.Runtime
{
    public partial class Evaluator
    {
        public class Frame
        {
            RefValue? retValueRef;
            ImmutableArray<Value> regValues;
            List<Task> tasks;

            public Frame(RefValue? retValueRef, IEnumerable<Value> regValues)
            {
                this.retValueRef = retValueRef;
                this.regValues = regValues.ToImmutableArray();
                this.tasks = new List<Task>();
            }

            public RefValue? GetRetValueRef()
            {
                return retValueRef;
            }

            public TValue GetRegValue<TValue>(RegId id) where TValue : Value
            {
                return (TValue)regValues[id.Value];
            }

            public void AddTask(Task task)
            {
                tasks.Add(task);
            }

            public IEnumerable<Task> GetTasks()
            {
                return tasks;
            }

            public async IAsyncEnumerable<Value> RunInNewAwaitAsync(Func<IAsyncEnumerable<Value>> func)
            {
                var prevTasks = tasks;
                tasks = new List<Task>();

                try
                {
                    await foreach (var yieldValue in func.Invoke())
                        yield return yieldValue;
                }
                finally
                {
                    tasks = prevTasks;
                }
            }
        }   
    }
}
