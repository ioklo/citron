using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    public partial class IR0Evaluator
    {
        class Context
        {
            RefValue? yieldValueRef;
            Frame frame;
            ControlInfo controlInfo;

            ImmutableArray<ExternalFuncInst> exFuncInsts;
            ImmutableArray<Func> funcs;            

            public Context(IEnumerable<ExternalFuncInst> exFuncInsts, IEnumerable<Func> funcs)
            {
                this.yieldValueRef = null;
                this.frame = new Frame(null, Enumerable.Empty<Value>());
                this.controlInfo = ControlInfo.None;

                this.exFuncInsts = exFuncInsts.ToImmutableArray();
                this.funcs = funcs.ToImmutableArray();
            }
            
            public Context(Context other, RefValue? yieldValueRef, IEnumerable<Value> regValues)
            {
                this.yieldValueRef = yieldValueRef;
                this.frame = new Frame(null, regValues);
                this.controlInfo = ControlInfo.None;

                exFuncInsts = other.exFuncInsts;
                funcs = other.funcs;
            }

            public TValue GetRegValue<TValue>(RegId regId) where TValue : Value
            {
                return frame.GetRegValue<TValue>(regId);
            }            

            public bool IsControlContinue()
            {
                return controlInfo.Flag == ControlFlag.Continue;
            }

            public bool IsControlTarget(ScopeId id)
            {
                return controlInfo.ScopeId.HasValue && controlInfo.ScopeId == id;
            }

            public void SetControlNone()
            {
                controlInfo = ControlInfo.None;
            }

            public void SetControlBreak(ScopeId scopeId)
            {
                controlInfo = ControlInfo.Break(scopeId);
            }

            public void SetControlContinue(ScopeId scopeId)
            {
                controlInfo = ControlInfo.Continue(scopeId);
            }
            
            public bool ShouldOutOfScope()
            {
                return controlInfo.Flag != ControlFlag.None;
            }

            public int GetMemberVarIndex(MemberVarId memberVar)
            {
                throw new NotImplementedException();
                // return memberVarInfos[memberVar.Value].Index;
            }

            public ref CompAllocInfo GetCompAllocInfo(AllocInfoId allocInfoId)
            {
                throw new NotImplementedException();
            }

            public Func GetFunc(FuncId funcId)
            {
                return funcs[funcId.Value];
            }

            public async ValueTask RunInNewFrameAsync(RefValue? retValueRef, IEnumerable<Value> regValues, Func<ValueTask> action)
            {
                var prevFrame = frame;
                frame = new Frame(retValueRef, regValues);

                try
                {
                    await action.Invoke();
                }
                finally
                {
                    frame = prevFrame;
                }
            }

            public RefValue? GetRetValueRef()
            {
                return frame.GetRetValueRef();
            }

            public ref readonly ExternalFuncInst GetExternalFuncInst(ExternalFuncId funcId)
            {
                return ref exFuncInsts.ItemRef(funcId.Value);
            }

            public RefValue GetYieldValueRef()
            {
                return yieldValueRef!;
            }

            public void AddTask(Task task)
            {
                frame.AddTask(task);
            }

            public IEnumerable<Task> GetTasks()
            {
                return frame.GetTasks();
            }

            public IAsyncEnumerable<Value> RunInNewAwaitAsync(Func<IAsyncEnumerable<Value>> func)
            {
                return frame.RunInNewAwaitAsync(func);
            }
        }
    }
}
