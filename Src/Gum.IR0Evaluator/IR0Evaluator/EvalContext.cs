using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gum;
using Gum.Collections;
using R = Gum.IR0;
using Void = Gum.Infra.Void;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        partial class EvalContext
        {
            private SharedContext sharedContext;

            private ImmutableDictionary<string, Value> localVars;
            private EvalFlowControl flowControl;
            private ImmutableArray<Task> tasks;
            private Value? thisValue;
            private Value retValue;
            private Value? yieldValue;

            public EvalContext(Value retValue)
            {
                this.sharedContext = new SharedContext();

                this.localVars = ImmutableDictionary<string, Value>.Empty;
                this.flowControl = EvalFlowControl.None;
                this.tasks = ImmutableArray<Task>.Empty;
                this.thisValue = null;
                this.retValue = retValue;
            }

            public EvalContext(
                EvalContext evalContext,
                ImmutableDictionary<string, Value> localVars,
                EvalFlowControl flowControl,
                ImmutableArray<Task> tasks,
                Value? thisValue,
                Value retValue)
            {
                this.sharedContext = evalContext.sharedContext;

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

            public Value GetStaticValue(R.Path type)
            {
                throw new NotImplementedException();
            }

            public R.EnumElement GetEnumElem(R.Path.Nested enumElem)
            {
                return sharedContext.GetEnumElem(enumElem);
            }

            public Value GetGlobalValue(string name)
            {
                return sharedContext.PrivateGlobalVars[name];
            }

            public void AddPrivateGlobalVar(string name, Value value)
            {
                sharedContext.PrivateGlobalVars.Add(name, value);
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

            public TRuntimeItem GetRuntimeItem<TRuntimeItem>(R.Path.Nested path)
                where TRuntimeItem : RuntimeItem
            {
                return sharedContext.GetRuntimeItem<TRuntimeItem>(path);
            }
            
            public R.CapturedStatementDecl GetCapturedStatementDecl(R.Path.Nested path)
            {
                return sharedContext.GetCapturedStatementDecl(path);
            }

            public async IAsyncEnumerable<Void> ExecInNewScopeAsync(Func<IAsyncEnumerable<Void>> action)
            {
                var prevLocalVars = localVars;

                try
                {
                    var enumerable = action.Invoke();
                    await foreach (var _ in enumerable)
                    {
                        yield return Void.Instance;
                    }
                }
                finally
                {
                    localVars = prevLocalVars;
                }
            }

            public FuncRuntimeItem GetFuncInvoker(R.Path.Nested path)
            {
                return sharedContext.GetFuncInvoker(path);
            }

            public void SetYieldValue(Value value)
            {
                yieldValue = value;
            }

            public Value GetYieldValue()
            {
                return yieldValue!;
            }

            public R.LambdaDecl GetLambdaDecl(R.Path.Nested path)
            {
                return sharedContext.GetLambdaDecl(path);
            }

            public void AddRootItemContainer(R.ModuleName moduleName, ItemContainer container)
            {
                sharedContext.AddRootItemContainer(moduleName, container);
            }
        }
    }
}
