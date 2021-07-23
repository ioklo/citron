using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gum;
using Gum.Collections;
using R = Gum.IR0;
using Void = Gum.Infra.Void;

namespace Gum.IR0Evaluator
{
    public partial class EvalContext
    {
        ImmutableDictionary<string, Value> capturedVars;
        EvalFlowControl flowControl;
        Value? thisValue;
        Value retValue;
        Value? yieldValue;

        public EvalContext(Value retValue)
        {
            this.capturedVars = ImmutableDictionary<string, Value>.Empty;
            this.flowControl = EvalFlowControl.None;
            this.thisValue = null;
            this.retValue = retValue;
        }

        public EvalContext(
            ImmutableDictionary<string, Value> capturedVars,
            EvalFlowControl flowControl,
            Value? thisValue,
            Value retValue)
        {
            this.capturedVars = capturedVars;
            this.flowControl = flowControl;
            this.thisValue = thisValue;
            this.retValue = retValue;
        }

        public Value GetStaticValue(R.Path type)
        {
            throw new NotImplementedException();
        }
        
        public Value GetCapturedValue(string name)
        {
            return capturedVars[name];
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
        public Value GetThisValue()
        {
            Debug.Assert(thisValue != null);
            return thisValue;
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
