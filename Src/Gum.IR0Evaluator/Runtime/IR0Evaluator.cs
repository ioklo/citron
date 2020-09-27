using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.IR0;
using Gum.Infra;
using System.Diagnostics.CodeAnalysis;

namespace Gum.Runtime
{
    public partial class IR0Evaluator
    {
        // Services
        DomainService domainService;
        IRuntimeModule runtimeModule;
        ExternalDriverFactory externalDriverFactory;

        public IR0Evaluator(DomainService domainService, IRuntimeModule runtimeModule, ExternalDriverFactory externalDriverFactory)
        {
            this.domainService = domainService;
            this.runtimeModule = runtimeModule;
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

            // ExFunc는 ExFuncInst로 변형했지만, Func는 변형할 필요가 없었다
            var context = Context.MakeContext(exFuncInsts, script.Funcs);

            // Entry부터 시작
            var func = context.GetFunc(script.EntryId);

            var regValues = func.Regs.Select(reg => AllocValue(reg.AllocInfoId, context)).ToList();

            await context.RunInNewFrameAsync(null, regValues, () =>
            {
                return RunCommandAsync(func.Body, context);
            });

            return 1;
        }

        // Scope
        async ValueTask RunScopeAsync(Command.Scope scopeCmd, Context context) 
        {
            cont:
            await RunCommandAsync(scopeCmd.Command, context);

            if (context.IsControlTarget(scopeCmd.Id))
            {
                if (context.IsControlContinue())
                {
                    context.SetControlNone();
                    goto cont;
                }

                context.SetControlNone();
            }   
        }

        async ValueTask RunSequenceAsync(Command.Sequence seqCmd, Context context)
        {
            foreach(var cmd in seqCmd.Commands)
            {
                await RunCommandAsync(cmd, context);

                if (context.ShouldOutOfScope())
                    return;
            }
        }

        void RunAssign(Command.Assign assignCmd, Context context) 
        {
            var destValue = context.GetRegValue<Value>(assignCmd.DestId);
            var srcValue = context.GetRegValue<Value>(assignCmd.SrcId);

            destValue.SetValue(srcValue);
        }

        void RunAssignRef(Command.AssignRef assignRefCmd, Context context)
        {
            var destRefValue = context.GetRegValue<RefValue>(assignRefCmd.DestRefId);
            var destValue = destRefValue.GetTarget();
            var srcValue = context.GetRegValue<Value>(assignRefCmd.SrcId);

            destValue.SetValue(srcValue);
        }

        void RunDeref(Command.Deref derefCmd, Context context)
        {
            var destValue = context.GetRegValue<Value>(derefCmd.DestId);            
            var srcRefValue = context.GetRegValue<RefValue>(derefCmd.SrcRefId);
            var srcValue = srcRefValue.GetTarget();

            destValue.SetValue(srcValue);
        }

        async ValueTask RunCallAsync(Command.Call callCmd, Context context) 
        {            
            var func = context.GetFunc(callCmd.FuncId);

            // 1. 레지스터 할당
            var regValues = func.Regs.Select(reg => AllocValue(reg.AllocInfoId, context)).ToList();

            // 2. 인자 복사
            for (int i = 0; i < callCmd.ArgIds.Length; i++)
            {
                var argValue = context.GetRegValue<Value>(callCmd.ArgIds[i]);
                regValues[i].SetValue(argValue);
            }

            // 3. returnValue 할당
            RefValue? retValueRef;

            if (callCmd.ResultId != null)
            {
                var retValue = context.GetRegValue<Value>(callCmd.ResultId.Value);
                retValueRef = new RefValue(retValue);
            }
            else
            {
                retValueRef = null;
            }
            
            await context.RunInNewFrameAsync(retValueRef, regValues, () =>
            {
                return RunCommandAsync(func.Body, context);
            });

            // return으로 인한 control 복원
            context.SetControlNone();
        }

        void RunExternalCall(Command.ExternalCall exCallCmd, Context context) 
        {
            // 
            ref readonly var exFuncInst = ref context.GetExternalFuncInst(exCallCmd.FuncId);

            // 1. 레지스터 할당
            var regValues = exFuncInst.AllocInfoIds.Select(allocInfoId => AllocValue(allocInfoId, context)).ToArray();

            // 2. 인자 복사
            for (int i = 0; i < exCallCmd.ArgIds.Length; i++)
            {
                var argValue = context.GetRegValue<Value>(exCallCmd.ArgIds[i]);
                regValues[i].SetValue(argValue);
            }

            Value? retValue;
            if (exCallCmd.ResultId != null)
                retValue = context.GetRegValue<Value>(exCallCmd.ResultId.Value);
            else
                retValue = null;

            exFuncInst.Delegate.Invoke(retValue, regValues);
        }

        Value AllocValue(AllocInfoId allocInfoId, Context context)
        {
            switch(allocInfoId.Value)
            {
                case AllocInfoId.RefValue: return new RefValue(null);
                case AllocInfoId.BoolValue: return runtimeModule.MakeBool(false);
                case AllocInfoId.IntValue: return runtimeModule.MakeInt(0);
            }

            ref var compAllocInfo = ref context.GetCompAllocInfo(allocInfoId);
            return new CompValue(compAllocInfo.MemberIds.Select(memberId => AllocValue(memberId, context)));
        }

        void RunHeapAlloc(Command.HeapAlloc heapAllocCmd, Context context)
        {
            var value = AllocValue(heapAllocCmd.AllocInfoId, context);

            var refValue = context.GetRegValue<RefValue>(heapAllocCmd.ResultRefId);
            refValue.SetTarget(value);
        }

        void RunMakeInt(Command.MakeInt makeIntCmd, Context context) 
        {
            var destValue = context.GetRegValue<Value>(makeIntCmd.ResultId);
            runtimeModule.SetInt(destValue, makeIntCmd.Value);
        }

        void RunMakeString(Command.MakeString makeStringCmd, Context context) 
        {
            var destValue = context.GetRegValue<RefValue>(makeStringCmd.ResultId);

            destValue.SetTarget(new StringValue(makeStringCmd.Value));
        }

        void RunMakeBool(Command.MakeBool makeBoolCmd, Context context) 
        {
            var destValue = context.GetRegValue<Value>(makeBoolCmd.ResultId);
            runtimeModule.SetBool(destValue, makeBoolCmd.Value);
        }

        void RunMakeEnumerator(Command.MakeEnumerator makeEnumeratorCmd, Context context) { throw new NotImplementedException(); }

        void RunConcatStrings(Command.ConcatStrings concatStringsCmd, Context context) 
        {
            var sb = new StringBuilder();
            foreach (var strReg in concatStringsCmd.StringIds)
            {
                var strValueRef = context.GetRegValue<RefValue>(strReg);
                var str = ((StringValue)strValueRef.GetTarget()).GetValue();
                sb.Append(str);
            }

            var destValueRef = context.GetRegValue<RefValue>(concatStringsCmd.ResultId);
            var newString = new StringValue(sb.ToString());
            destValueRef.SetTarget(newString);
        }

        async ValueTask RunIfAsync(Command.If ifCmd, Context context) 
        {
            var condValue = context.GetRegValue<Value>(ifCmd.CondId);
            var cond = runtimeModule.GetBool(condValue);

            if (cond)
                await RunCommandAsync(ifCmd.ThenCommand, context);
            else if(ifCmd.ElseCommand != null)
                await RunCommandAsync(ifCmd.ElseCommand, context);
        }
        
        void RunBreak(Command.Break breakCmd, Context context) 
        {
            context.SetControlBreak(breakCmd.ScopeId);            
        }

        void RunContinue(Command.Continue continueCmd, Context context) 
        {
            context.SetControlContinue(continueCmd.ScopeId);
        }

        void RunSetReturnValue(Command.SetReturnValue setReturnValueCmd, Context context) 
        {
            var retValueRef = context.GetRetValueRef()!;

            var destValue = retValueRef.GetTarget();
            var srcValue = context.GetRegValue<Value>(setReturnValueCmd.ValueId);

            destValue.SetValue(srcValue);
        }

        
        void RunYield(Command.Yield yieldCmd, Context context) { throw new NotImplementedException(); }
        void RunTask(Command.Task taskCmd, Context context) { throw new NotImplementedException(); }
        void RunAsync(Command.Async asyncCmd, Context context) { throw new NotImplementedException(); }
        void RunAwait(Command.Await awaitCmd, Context context) { throw new NotImplementedException(); }

        void RunGetGlobalRef(Command.GetGlobalRef getGlobalRefCmd, Context context)
        {
            throw new NotImplementedException();
            //var index = context.GetGlobalVarIndex(getGlobalRefCmd.GlobalVarId);
            //var value = context.GetGlobalValue(index);

            //var destValue = context.GetRegValue<RefValue>(getGlobalRefCmd.ResultId);
            //destValue.SetTarget(value);
        }

        void RunGetMemberRef(Command.GetMemberRef getMemberRefCmd, Context context) 
        {
            // [0] // RefValue
            // [1] GetMemberVar [0] "x"
            var instanceRefValue = context.GetRegValue<RefValue>(getMemberRefCmd.InstanceRefId);
            var compValue = (CompValue)instanceRefValue.GetTarget();
            var index = context.GetMemberVarIndex(getMemberRefCmd.MemberVarId);
            var value = compValue.GetValue(index);

            var destValue = context.GetRegValue<RefValue>(getMemberRefCmd.ResultId);
            destValue.SetTarget(value);
        }

        void RunExternalGetMemberRef(Command.ExternalGetMemberRef exGetMemberRefCmd, Context context) { throw new NotImplementedException(); }

        async ValueTask RunCommandAsync(Command cmd, Context context)
        {
            switch(cmd)
            {
                case Command.Scope scopeCmd: await RunScopeAsync(scopeCmd, context); break;
                case Command.Sequence seqCmd: await RunSequenceAsync(seqCmd, context); break;
                case Command.Assign assignCmd: RunAssign(assignCmd, context); break;
                case Command.AssignRef assignRefCmd: RunAssignRef(assignRefCmd, context); break;
                case Command.Deref derefCmd: RunDeref(derefCmd, context); break;
                case Command.Call callCmd: await RunCallAsync(callCmd, context); break;
                case Command.ExternalCall exCallCmd: RunExternalCall(exCallCmd, context); break;
                case Command.HeapAlloc heapAllocCmd: RunHeapAlloc(heapAllocCmd, context); break;
                case Command.MakeInt makeIntCmd: RunMakeInt(makeIntCmd, context); break;
                case Command.MakeString makeStringCmd: RunMakeString(makeStringCmd, context); break;
                case Command.MakeBool makeBoolCmd: RunMakeBool(makeBoolCmd, context); break;
                case Command.MakeEnumerator makeEnumeratorCmd: RunMakeEnumerator(makeEnumeratorCmd, context); break;
                case Command.ConcatStrings concatStringsCmd: RunConcatStrings(concatStringsCmd, context); break;
                case Command.If ifCmd: await RunIfAsync(ifCmd, context); break;                
                case Command.Break breakCmd: RunBreak(breakCmd, context); break;
                case Command.Continue continueCmd: RunContinue(continueCmd, context); break;
                case Command.SetReturnValue setReturnValueCmd: RunSetReturnValue(setReturnValueCmd, context); break;
                case Command.Yield yieldCmd: RunYield(yieldCmd, context); break;
                case Command.Task taskCmd: RunTask(taskCmd, context); break;
                case Command.Async asyncCmd: RunAsync(asyncCmd, context); break;
                case Command.Await awaitCmd : RunAwait(awaitCmd, context); break;
                case Command.GetGlobalRef getGlobalRefCmd: RunGetGlobalRef(getGlobalRefCmd, context); break;
                case Command.GetMemberRef getMemberRefCmd: RunGetMemberRef(getMemberRefCmd, context); break;
                case Command.ExternalGetMemberRef exGetMemberRefCmd: RunExternalGetMemberRef(exGetMemberRefCmd, context); break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
