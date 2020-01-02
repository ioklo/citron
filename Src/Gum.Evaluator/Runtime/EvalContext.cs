using Gum.CompileTime;
using Gum.Syntax;
using Gum.Runtime;
using Gum.StaticAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Gum;

namespace Gum.Runtime
{   
    public class EvalContext
    {
        public IRuntimeModule RuntimeModule { get; }
        public DomainService DomainService { get; }
        public TypeValueService TypeValueService { get; }

        private Value?[] privateGlobalVars;
        private Value?[] localVars;

        private EvalFlowControl flowControl;
        private ImmutableArray<Task> tasks;
        private Value? thisValue;
        private Value retValue;

        private ImmutableDictionary<ISyntaxNode, SyntaxNodeInfo> infosByNode;

        public EvalContext(
            IRuntimeModule runtimeModule, 
            DomainService domainService, 
            TypeValueService typeValueService,
            int privateGlobalVarCount,
            ImmutableDictionary<ISyntaxNode, SyntaxNodeInfo> infosByNode)
        {
            RuntimeModule = runtimeModule;
            DomainService = domainService;
            TypeValueService = typeValueService;
            
            this.infosByNode = infosByNode;
            
            localVars = new Value?[0];
            privateGlobalVars = new Value?[privateGlobalVarCount];
            flowControl = EvalFlowControl.None;
            tasks = ImmutableArray<Task>.Empty; ;
            thisValue = null;
            retValue = VoidValue.Instance;
        }

        public EvalContext(
            EvalContext other,
            Value?[] localVars,
            EvalFlowControl flowControl,
            ImmutableArray<Task> tasks,
            Value? thisValue,
            Value retValue)
        {
            RuntimeModule = other.RuntimeModule;
            DomainService = other.DomainService;
            TypeValueService = other.TypeValueService;
            this.infosByNode = other.infosByNode;
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
            Value?[] newLocalVars, 
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

        public TSyntaxNodeInfo GetNodeInfo<TSyntaxNodeInfo>(ISyntaxNode node) where TSyntaxNodeInfo : SyntaxNodeInfo
        {
            return (TSyntaxNodeInfo)infosByNode[node];
        }

        public Value GetStaticValue(VarValue varValue)
        {
            throw new NotImplementedException();
        }

        public Value GetPrivateGlobalVar(int index)
        {
            return privateGlobalVars[index]!;
        }

        public void InitPrivateGlobalVar(int index, Value value)
        {
            privateGlobalVars[index] = value;
        }

        public Value GetLocalVar(int index)
        {
            return localVars[index]!;
        }

        public void InitLocalVar(int i, Value value)
        {
            // for문 내부에서 decl할 경우 재사용하기 때문에 assert를 넣으면 안된다
            // Debug.Assert(context.LocalVars[storage.LocalIndex] == null);
            localVars[i] = value;
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
