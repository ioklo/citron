using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.IR1;
using Gum.Infra;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using Gum.Misc;

namespace Gum.IR1.Runtime
{
    public partial class Evaluator
    {
        // Services
        ExternalDriverFactory externalDriverFactory;

        public Evaluator(ExternalDriverFactory externalDriverFactory)
        {
            this.externalDriverFactory = externalDriverFactory;
        }

        public async ValueTask<int> RunScriptAsync(Script script)
        {
            var exFuncInsts = script.ExFuncs.Select(exFunc =>
            {
                var driver = externalDriverFactory.GetDriver(exFunc.DriverId);
                var exFuncDelegate = driver.GetDelegate(exFunc.DriverFuncId);

                return new ExternalFuncInst(exFunc.AllocInfoIds, exFuncDelegate);
            });

            Func<Context, ImmutableArray<Value>> globalValueInitializer = context =>
                script.GlobalVars.Select(globalVar => AllocValue(globalVar.AllocInfoId, context)).ToImmutableArray();

            // ExFunc는 ExFuncInst로 변형했지만, Func는 변형할 필요가 없었다
            var context = new Context(exFuncInsts, script.Funcs, globalValueInitializer);

            // Entry부터 시작
            var func = context.GetFunc(script.EntryId);

            var regValues = func.Regs.Select(reg => AllocValue(reg.AllocInfoId, context)).ToList();

            await context.RunInNewFrameAsync(null, regValues, async () =>
            {
                await foreach (var _ in RunCommandAsync(func.Body, context)) { }
            });

            return 1;
        }

        // Scope
        async IAsyncEnumerable<Value> RunScopeAsync(Command.Scope cmd, Context context) 
        {
            cont:
            await foreach (var yieldValue in RunCommandAsync(cmd.Command, context))
                yield return yieldValue;

            if (context.IsControlTarget(cmd.Id))
            {
                if (context.IsControlContinue())
                {
                    context.SetControlNone();
                    goto cont;
                }

                context.SetControlNone();
            }   
        }

        async IAsyncEnumerable<Value> RunSequenceAsync(Command.Sequence cmd, Context context)
        {
            foreach(var innerCmd in cmd.Commands)
            {
                await foreach (var yieldValue in RunCommandAsync(innerCmd, context))
                    yield return yieldValue;

                if (context.ShouldOutOfScope())
                    yield break;
            }
        }

        void RunAssign(Command.Assign cmd, Context context) 
        {
            var destValue = context.GetRegValue<Value>(cmd.DestId);
            var srcValue = context.GetRegValue<Value>(cmd.SrcId);

            destValue.SetValue(srcValue);
        }

        void RunAssignRef(Command.AssignRef cmd, Context context)
        {
            var destRefValue = context.GetRegValue<RefValue>(cmd.DestRefId);
            var destValue = destRefValue.GetTarget();
            var srcValue = context.GetRegValue<Value>(cmd.SrcId);

            destValue.SetValue(srcValue);
        }

        void RunDeref(Command.Deref cmd, Context context)
        {
            var destValue = context.GetRegValue<Value>(cmd.DestId);            
            var srcRefValue = context.GetRegValue<RefValue>(cmd.SrcRefId);
            var srcValue = srcRefValue.GetTarget();

            destValue.SetValue(srcValue);
        }

        async ValueTask RunCallAsync(Command.Call cmd, Context context) 
        {            
            var func = context.GetFunc(cmd.FuncId);

            // 1. 레지스터 할당
            var regValues = func.Regs.Select(reg => AllocValue(reg.AllocInfoId, context)).ToList();

            // 2. 인자 복사
            for (int i = 0; i < cmd.ArgIds.Length; i++)
            {
                var argValue = context.GetRegValue<Value>(cmd.ArgIds[i]);
                regValues[i].SetValue(argValue);
            }

            // 3. returnValue 할당
            RefValue? retValueRef;

            if (cmd.ResultId != null)
            {
                var retValue = context.GetRegValue<Value>(cmd.ResultId.Value);
                retValueRef = new RefValue(retValue);
            }
            else
            {
                retValueRef = null;
            }
            
            await context.RunInNewFrameAsync(retValueRef, regValues, async () =>
            {
                await foreach (var _ in RunCommandAsync(func.Body, context)) { }
            });
        }

        ValueTask RunExternalCallAsync(Command.ExternalCall cmd, Context context) 
        {
            // 
            ref readonly var exFuncInst = ref context.GetExternalFuncInst(cmd.FuncId);

            // 1. 레지스터 할당
            var regValues = exFuncInst.AllocInfoIds.Select(allocInfoId => AllocValue(allocInfoId, context)).ToArray();

            // 2. 인자 복사
            for (int i = 0; i < cmd.ArgIds.Length; i++)
            {
                var argValue = context.GetRegValue<Value>(cmd.ArgIds[i]);
                regValues[i].SetValue(argValue);
            }

            Value? retValue;
            if (cmd.ResultId != null)
                retValue = context.GetRegValue<Value>(cmd.ResultId.Value);
            else
                retValue = null;

            return exFuncInst.Delegate.Invoke(retValue, regValues);
        }

        Value AllocValue(AllocInfoId allocInfoId, Context context)
        {
            switch(allocInfoId.Value)
            {
                case AllocInfoId.RefValue: return new RefValue(null);
                case AllocInfoId.BoolValue: return new BoolValue(false);
                case AllocInfoId.IntValue: return new IntValue(0);
            }

            ref var compAllocInfo = ref context.GetCompAllocInfo(allocInfoId);
            return new CompValue(compAllocInfo.MemberIds.Select(memberId => AllocValue(memberId, context)));
        }

        void RunHeapAlloc(Command.HeapAlloc cmd, Context context)
        {
            var value = AllocValue(cmd.AllocInfoId, context);

            var refValue = context.GetRegValue<RefValue>(cmd.ResultRefId);
            refValue.SetTarget(value);
        }

        void RunMakeInt(Command.MakeInt cmd, Context context) 
        {
            var destValue = context.GetRegValue<IntValue>(cmd.ResultId);
            destValue.SetInt(cmd.Value);
        }

        void RunMakeString(Command.MakeString cmd, Context context) 
        {
            var destValue = context.GetRegValue<RefValue>(cmd.ResultId);

            destValue.SetTarget(new StringValue(cmd.Value));
        }

        void RunMakeBool(Command.MakeBool cmd, Context context) 
        {
            var destValue = context.GetRegValue<BoolValue>(cmd.ResultId);
            destValue.SetBool(cmd.Value);
        }

        void RunMakeEnumerator(Command.MakeEnumerator cmd, Context context) 
        {
            var func = context.GetFunc(cmd.FuncId);

            // 1. 레지스터 할당
            var regValues = func.Regs.Select(reg => AllocValue(reg.AllocInfoId, context)).ToList();

            // 2. 인자 복사
            for (int i = 0; i < cmd.ArgIds.Length; i++)
            {
                var argValue = context.GetRegValue<Value>(cmd.ArgIds[i]);
                regValues[i].SetValue(argValue);
            }

            // 3. yieldValue 할당
            var yieldValue = AllocValue(cmd.YieldAllocId, context);
            var yieldValueRef = new RefValue(yieldValue);

            var newContext = new Context(context, yieldValueRef, regValues);

            var asyncEnumerable = RunCommandAsync(func.Body, newContext);
            var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
            var enumeratorValue = new EnumeratorValue(asyncEnumerator);

            var destRefValue = context.GetRegValue<RefValue>(cmd.ResultId);
            destRefValue.SetTarget(enumeratorValue);
        }

        void RunConcatStrings(Command.ConcatStrings cmd, Context context) 
        {
            var sb = new StringBuilder();
            foreach (var strReg in cmd.StringIds)
            {
                var strValueRef = context.GetRegValue<RefValue>(strReg);
                var str = ((StringValue)strValueRef.GetTarget()).GetString();
                sb.Append(str);
            }

            var destValueRef = context.GetRegValue<RefValue>(cmd.ResultId);
            var newString = new StringValue(sb.ToString());
            destValueRef.SetTarget(newString);
        }

        IAsyncEnumerable<Value> RunIfAsync(Command.If cmd, Context context) 
        {
            var condValue = context.GetRegValue<BoolValue>(cmd.CondId);
            var cond = condValue.GetBool();

            if (cond)
                return RunCommandAsync(cmd.ThenCommand, context);
            else if(cmd.ElseCommand != null)
                return RunCommandAsync(cmd.ElseCommand, context);

            return AsyncEnumerable.Empty<Value>();
        }
        
        void RunBreak(Command.Break cmd, Context context) 
        {
            context.SetControlBreak(cmd.ScopeId);            
        }

        void RunContinue(Command.Continue cmd, Context context) 
        {
            context.SetControlContinue(cmd.ScopeId);
        }

        void RunSetReturnValue(Command.SetReturnValue cmd, Context context) 
        {
            var retValueRef = context.GetRetValueRef()!;

            var destValue = retValueRef.GetTarget();
            var srcValue = context.GetRegValue<Value>(cmd.ValueId);

            destValue.SetValue(srcValue);
        }

        async ValueTask RunEnumeratorMoveNextAsync(Command.EnumeratorMoveNext cmd, Context context)
        {
            var enumeratorRefValue = context.GetRegValue<RefValue>(cmd.EnumeratorRefId);
            var ev = (EnumeratorValue)enumeratorRefValue.GetTarget();

            var destValue = context.GetRegValue<BoolValue>(cmd.DestId);

            var enumerator = ev.GetEnumerator();
            bool result = await enumerator.MoveNextAsync();

            destValue.SetBool(result);
        }

        void RunEnumeratorGetValue(Command.EnumeratorGetValue cmd, Context context)
        {
            var enumeratorRefValue = context.GetRegValue<RefValue>(cmd.EnumeratorRefId);
            var ev = (EnumeratorValue)enumeratorRefValue.GetTarget();
            var destValue = context.GetRegValue<Value>(cmd.DestId);

            var enumerator = ev.GetEnumerator();

            destValue.SetValue(enumerator.Current);
        }
        
        Value RunYield(Command.Yield cmd, Context context) 
        {
            var srcValue = context.GetRegValue<Value>(cmd.ValueId);

            var yieldValueRef = context.GetYieldValueRef();
            var target = yieldValueRef.GetTarget();
            target.SetValue(srcValue);

            return target;
        }

        void RunTask(Command.Task cmd, Context context) 
        {
            var func = context.GetFunc(cmd.FuncId);

            // 1. 레지스터 할당
            var regValues = func.Regs.Select(reg => AllocValue(reg.AllocInfoId, context)).ToList();

            // 2. 인자 복사
            for (int i = 0; i < cmd.ArgIds.Length; i++)
            {
                var argValue = context.GetRegValue<Value>(cmd.ArgIds[i]);
                regValues[i].SetValue(argValue);
            }

            var newContext = new Context(context, null, regValues);

            var task = Task.Run(async () =>
            {
                await foreach (var _ in RunCommandAsync(func.Body, newContext)) { }
            });

            // context.AddTask 추가한다
            context.AddTask(task);
        }

        void RunAsync(Command.Async cmd, Context context)
        {
            var func = context.GetFunc(cmd.FuncId);

            // 1. 레지스터 할당
            var regValues = func.Regs.Select(reg => AllocValue(reg.AllocInfoId, context)).ToList();

            // 2. 인자 복사
            for (int i = 0; i < cmd.ArgIds.Length; i++)
            {
                var argValue = context.GetRegValue<Value>(cmd.ArgIds[i]);
                regValues[i].SetValue(argValue);
            }

            var newContext = new Context(context, null, regValues);

            Func<Task> asyncFunc = async () =>
            {
                await foreach (var _ in RunCommandAsync(func.Body, newContext)) { }
            };

            var task = asyncFunc.Invoke();

            // context.AddTask 추가한다
            context.AddTask(task);
        }

        IAsyncEnumerable<Value> RunAwaitAsync(Command.Await cmd, Context context)
        {
            async IAsyncEnumerable<Value> Wrapper()
            {
                await foreach (var yieldValue in RunCommandAsync(cmd.Command, context))
                    yield return yieldValue;

                await Task.WhenAll(context.GetTasks());
            }

            return context.RunInNewAwaitAsync(Wrapper);
        }

        void RunGetGlobalRef(Command.GetGlobalRef cmd, Context context)
        {
            var value = context.GetGlobalValue(cmd.GlobalVarId.Value);

            var destRefValue = context.GetRegValue<RefValue>(cmd.ResultId);
            destRefValue.SetTarget(value);
        }

        void RunGetMemberRef(Command.GetMemberRef cmd, Context context) 
        {
            // [0] // RefValue
            // [1] GetMemberVar [0] "x"
            var instanceRefValue = context.GetRegValue<RefValue>(cmd.InstanceRefId);
            var compValue = (CompValue)instanceRefValue.GetTarget();
            var index = context.GetMemberVarIndex(cmd.MemberVarId);
            var value = compValue.GetValue(index);

            var destValue = context.GetRegValue<RefValue>(cmd.ResultId);
            destValue.SetTarget(value);
        }

        void RunExternalGetMemberRef(Command.ExternalGetMemberRef cmd, Context context) { throw new NotImplementedException(); }
        
        async IAsyncEnumerable<Value> RunCommandAsync(Command cmd, Context context)
        {
            switch(cmd)
            {
                case Command.Scope scopeCmd: 
                    await foreach(var yieldValue in RunScopeAsync(scopeCmd, context)) 
                        yield return yieldValue; 
                    break;

                case Command.Sequence seqCmd:
                    await foreach (var yieldValue in RunSequenceAsync(seqCmd, context))
                        yield return yieldValue;
                    break;

                case Command.Assign assignCmd: RunAssign(assignCmd, context); break;
                case Command.AssignRef assignRefCmd: RunAssignRef(assignRefCmd, context); break;
                case Command.Deref derefCmd: RunDeref(derefCmd, context); break;
                case Command.Call callCmd: await RunCallAsync(callCmd, context); break;
                case Command.ExternalCall exCallCmd: await RunExternalCallAsync(exCallCmd, context); break;
                case Command.HeapAlloc heapAllocCmd: RunHeapAlloc(heapAllocCmd, context); break;
                case Command.MakeInt makeIntCmd: RunMakeInt(makeIntCmd, context); break;
                case Command.MakeString makeStringCmd: RunMakeString(makeStringCmd, context); break;
                case Command.MakeBool makeBoolCmd: RunMakeBool(makeBoolCmd, context); break;
                case Command.MakeEnumerator makeEnumeratorCmd: RunMakeEnumerator(makeEnumeratorCmd, context); break;
                case Command.ConcatStrings concatStringsCmd: RunConcatStrings(concatStringsCmd, context); break;
                case Command.If ifCmd:
                    await foreach (var yieldValue in RunIfAsync(ifCmd, context))
                        yield return yieldValue;
                    break;                

                case Command.Break breakCmd: RunBreak(breakCmd, context); break;
                case Command.Continue continueCmd: RunContinue(continueCmd, context); break;
                case Command.SetReturnValue setReturnValueCmd: RunSetReturnValue(setReturnValueCmd, context); break;
                case Command.EnumeratorMoveNext enumNextCmd: await RunEnumeratorMoveNextAsync(enumNextCmd, context); break;
                case Command.EnumeratorGetValue enumValueCmd: RunEnumeratorGetValue(enumValueCmd, context); break;
                case Command.Yield yieldCmd: 
                    yield return RunYield(yieldCmd, context); break;
                case Command.Task taskCmd: RunTask(taskCmd, context); break;
                case Command.Async asyncCmd: RunAsync(asyncCmd, context); break;
                case Command.Await awaitCmd:
                    await foreach (var yieldValue in RunAwaitAsync(awaitCmd, context))
                        yield return yieldValue;
                    break;

                case Command.GetGlobalRef getGlobalRefCmd: RunGetGlobalRef(getGlobalRefCmd, context); break;
                case Command.GetMemberRef getMemberRefCmd: RunGetMemberRef(getMemberRefCmd, context); break;
                case Command.ExternalGetMemberRef exGetMemberRefCmd: RunExternalGetMemberRef(exGetMemberRefCmd, context); break;

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}
