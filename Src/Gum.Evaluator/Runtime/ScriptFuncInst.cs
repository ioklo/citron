using Gum;
using Gum.CompileTime;
using Gum.Runtime;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Gum.Runtime
{
    class ScriptFuncInst : FuncInst
    {
        public TypeValue? SeqElemTypeValue { get; }  // seqCall이 아니라면 null이다
        public override bool bThisCall { get; }        // Caller입장에서 this를 전달할지
        public Value? CapturedThis { get; } // 캡쳐한 곳에 있던 this를 쓸지
        public ImmutableArray<Value> Captures { get; } // LocalIndex 0 부터.. 그 뒤에 argument가 붙는다
        public int LocalVarCount { get; }
        public Stmt Body { get; }

        public ScriptFuncInst(TypeValue? seqElemTypeValue, bool bThisCall, Value? capturedThis, IEnumerable<Value> captures, int localVarCount, Stmt body)
        {
            // 둘 중 하나는 false여야 한다
            Debug.Assert(!bThisCall || capturedThis == null);

            SeqElemTypeValue = seqElemTypeValue;
            this.bThisCall = bThisCall;
            CapturedThis = capturedThis;
            Captures = captures.ToImmutableArray();
            LocalVarCount = localVarCount;
            Body = body;
        }
    }
}
