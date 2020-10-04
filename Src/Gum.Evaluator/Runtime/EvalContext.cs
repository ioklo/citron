using Gum.CompileTime;
using Gum.Runtime;
using Gum.StaticAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Gum;
using Gum.IR0;

namespace Gum.Runtime
{   
    public class EvalContext
    {
        public IRuntimeModule RuntimeModule { get; }
        public DomainService DomainService { get; }
        public TypeValueService TypeValueService { get; }
        
        private ImmutableDictionary<string, Value> privateGlobalVars;
        private ImmutableDictionary<string, Value> localVars;

        private EvalFlowControl flowControl;
        private ImmutableArray<Task> tasks;
        private Value? thisValue;
        private Value retValue;

        public EvalContext(
            IRuntimeModule runtimeModule, 
            DomainService domainService, 
            TypeValueService typeValueService,
            int privateGlobalVarCount)
        {
            RuntimeModule = runtimeModule;
            DomainService = domainService;
            TypeValueService = typeValueService;

            privateGlobalVars = ImmutableDictionary<string, Value>.Empty;
            localVars = ImmutableDictionary<string, Value>.Empty;
            flowControl = EvalFlowControl.None;
            tasks = ImmutableArray<Task>.Empty; ;
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
            RuntimeModule = other.RuntimeModule;
            DomainService = other.DomainService;
            TypeValueService = other.TypeValueService;
            privateGlobalVars = other.privateGlobalVars;

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

        public Value GetStaticValue(VarValue varValue)
        {
            throw new NotImplementedException();
        }

        public Value GetPrivateGlobalValue(string name)
        {
            return privateGlobalVars[name];
        }

        public void AddPrivateGlobalVar(string name, Value value)
        {
            privateGlobalVars = privateGlobalVars.Add(name, value);
        }

        public Value GetLocalValue(string name)
        {
            return localVars[name];
        }

        public void AddLocalVar(string name, Value value)
        {
            // for문 내부에서 decl할 경우 재사용하기 때문에 assert를 넣으면 안된다
            // Debug.Assert(context.LocalVars[storage.LocalIndex] == null);
            localVars = localVars.Add(name, value);
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
    }
}
