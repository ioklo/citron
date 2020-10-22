using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Gum;
using Gum.IR0;

namespace Gum.IR0.Runtime
{   
    public class EvalContext
    {
        class SharedData
        {
            public ImmutableArray<Func> Funcs { get; }
            public ImmutableArray<SeqFunc> SeqFuncs { get; }
            public Dictionary<string, Value> PrivateGlobalVars { get; }

            public SharedData(IEnumerable<Func> funcs, IEnumerable<SeqFunc> seqFuncs)
            {
                Funcs = funcs.ToImmutableArray();
                SeqFuncs = seqFuncs.ToImmutableArray();
                PrivateGlobalVars = new Dictionary<string, Value>();
            }
        }

        private SharedData sharedData;

        private ImmutableDictionary<string, Value> localVars;
        private EvalFlowControl flowControl;
        private ImmutableArray<Task> tasks;
        private Value? thisValue;
        private Value retValue;

        public EvalContext(IEnumerable<Func> funcs, IEnumerable<SeqFunc> seqFuncs)
        {
            sharedData = new SharedData(funcs, seqFuncs);
            
            localVars = ImmutableDictionary<string, Value>.Empty;
            flowControl = EvalFlowControl.None;
            tasks = ImmutableArray<Task>.Empty;
            thisValue = null;
            retValue = VoidValue.Instance;
        }

        public EvalContext(
            EvalContext other,
            ImmutableDictionary<string, Value> localVars,
            EvalFlowControl flowControl,
            ImmutableArray<Task> tasks,
            Value? thisValue,
            Value retValue)
        {
            this.sharedData = other.sharedData;

            this.localVars = localVars;
            this.flowControl = flowControl;
            this.tasks = tasks;
            this.thisValue = thisValue;
            this.retValue = retValue;
        }

        public TypeInst GetTypeInst(TypeId typeId)
        {
            throw new NotImplementedException();
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

        public Value GetStaticValue(TypeId type)
        {
            throw new NotImplementedException();
        }

        public Value GetPrivateGlobalValue(string name)
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

        public Value? GetThisValue()
        {
            return thisValue;
        }

        public async IAsyncEnumerable<Value> ExecInNewTasks(Func<IAsyncEnumerable<Value>> enumerable)
        {
            var prevTasks = tasks;
            tasks = ImmutableArray<Task>.Empty;

            await foreach (var v in enumerable())
                yield return v;
            
            tasks = prevTasks;
        }

        public async IAsyncEnumerable<Value> ExecInNewScopeAsync(Func<IAsyncEnumerable<Value>> action)
        {
            var prevLocalVars = localVars;

            try
            {
                var enumerable = action.Invoke();
                await foreach(var yieldValue in enumerable)
                {
                    yield return yieldValue;
                }
            }
            finally 
            {
                localVars = prevLocalVars;
            }
        }

        public Func GetFunc(FuncId funcId)
        {
            return sharedData.Funcs[funcId.Value];
        }

        public SeqFunc GetSeqFunc(SeqFuncId seqFuncId)
        {
            return sharedData.SeqFuncs[seqFuncId.Value];
        }
    }    
}
