using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gum;
using Gum.Collections;
using Gum.IR0;

using Void = Gum.Infra.Void;

namespace Gum.IR0.Runtime
{   
    public class EvalContext
    {
        class SharedData
        {
            public ImmutableArray<IDecl> Decls { get; }            
            public Dictionary<string, Value> PrivateGlobalVars { get; }

            public SharedData(ImmutableArray<IDecl> decls)
            {
                Decls = decls;
                PrivateGlobalVars = new Dictionary<string, Value>();
            }
        }

        private SharedData sharedData;

        private ImmutableDictionary<string, Value> localVars;
        private EvalFlowControl flowControl;
        private ImmutableArray<Task> tasks;
        private Value? thisValue;
        private Value retValue;
        private Value? yieldValue;

        public EvalContext(ImmutableArray<IDecl> decls)
        {
            sharedData = new SharedData(decls);
            
            localVars = ImmutableDictionary<string, Value>.Empty;
            flowControl = EvalFlowControl.None;
            tasks = ImmutableArray<Task>.Empty;
            thisValue = null;
            retValue = VoidValue.Instance;
        }

        public EvalContext(
            EvalContext evalContext,
            ImmutableDictionary<string, Value> localVars,
            EvalFlowControl flowControl,
            ImmutableArray<Task> tasks,
            Value? thisValue,
            Value retValue)
        {
            this.sharedData = evalContext.sharedData;

            this.localVars = localVars;
            this.flowControl = flowControl;
            this.tasks = tasks;
            this.thisValue = thisValue;
            this.retValue = retValue;
        }       

        public EvalContext SetTasks(ImmutableArray<Task> newTasks)
        {
            tasks = newTasks;
            return this;
        }

        public ImmutableArray<Task> GetTasks()
        {
            return tasks;
        }

        public void AddTask(Task task)
        {
            tasks = tasks.Add(task);
        }

        public async ValueTask ExecInNewFuncFrameAsync(
            ImmutableDictionary<string, Value> newLocalVars, 
            EvalFlowControl newFlowControl, 
            ImmutableArray<Task> newTasks, 
            Value? newThisValue, 
            Value newRetValue,
            Func<ValueTask> ActionAsync)
        {
            var prevValue = (localVars, flowControl, tasks, thisValue, retValue);
            (localVars, flowControl, tasks, thisValue, retValue) = (newLocalVars, newFlowControl, newTasks, newThisValue, newRetValue);

            try
            {
                await ActionAsync();
            }
            finally
            {
                (localVars, flowControl, tasks, thisValue, retValue) = prevValue;
            }
        }

        public Value GetStaticValue(Type type)
        {
            throw new NotImplementedException();
        }

        public Value GetGlobalValue(string name)
        {
            return sharedData.PrivateGlobalVars[name];
        }

        public void AddPrivateGlobalVar(string name, Value value)
        {
            sharedData.PrivateGlobalVars.Add(name, value);
        }

        public Value GetLocalValue(string name)
        {
            return localVars[name];
        }

        public void AddLocalVar(string name, Value value)
        {   
            localVars = localVars.SetItem(name, value);
        }

        public bool IsFlowControl(EvalFlowControl testValue)
        {
            return flowControl == testValue;
        }

        public EvalFlowControl GetFlowControl()
        {
            return flowControl;
        }

        public void SetFlowControl(EvalFlowControl newFlowControl)
        {
            flowControl = newFlowControl;
        }

        public Value GetRetValue()
        {
            return retValue!;
        }

        // struct 이면 refValue, boxed struct 이면 boxValue, class 이면 ClassValue
        public Value? GetThisValue()
        {
            return thisValue;
        }

        public async IAsyncEnumerable<Void> ExecInNewTasks(Func<IAsyncEnumerable<Void>> enumerable)
        {
            var prevTasks = tasks;
            tasks = ImmutableArray<Task>.Empty;

            await foreach (var _ in enumerable())
                yield return Void.Instance;
            
            tasks = prevTasks;
        }

        public async IAsyncEnumerable<Void> ExecInNewScopeAsync(Func<IAsyncEnumerable<Void>> action)
        {
            var prevLocalVars = localVars;

            try
            {
                var enumerable = action.Invoke();
                await foreach(var _ in enumerable)
                {
                    yield return Void.Instance;
                }
            }
            finally 
            {
                localVars = prevLocalVars;
            }
        }

        public TDecl GetDecl<TDecl>(DeclId declId)
            where TDecl : IDecl
        {
            return (TDecl)sharedData.Decls[declId.Value];
        }

        public Func GetFunc(Func func)
        {

        }

        public void SetYieldValue(Value value)
        {
            yieldValue = value;
        }

        public Value GetYieldValue()
        {
            return yieldValue!;
        }
    }    
}
