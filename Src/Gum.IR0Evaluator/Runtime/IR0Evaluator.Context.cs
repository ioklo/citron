﻿using Gum.IR0;
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
            Frame frame;
            ControlInfo controlInfo;

            ImmutableArray<ExternalFuncInst> exFuncInsts;
            ImmutableArray<Func> funcs;            

            public static Context MakeContext(IEnumerable<ExternalFuncInst> exFuncInsts, IEnumerable<Func> funcs)
            {
                return new Context(exFuncInsts, funcs);
            }

            private Context(IEnumerable<ExternalFuncInst> exFuncInsts, IEnumerable<Func> funcs)
            {
                this.frame = new Frame(null, Enumerable.Empty<Value>());
                this.controlInfo = ControlInfo.None;

                this.exFuncInsts = exFuncInsts.ToImmutableArray();
                this.funcs = funcs.ToImmutableArray();
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

            public void SetControlReturn()
            {
                controlInfo = ControlInfo.Return;
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
        }
    }
}
