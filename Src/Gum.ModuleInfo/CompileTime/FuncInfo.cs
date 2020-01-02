using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.CompileTime
{
    public class FuncInfo
    {
        public ModuleItemId? OuterId { get; }
        public ModuleItemId FuncId { get; }
        public bool bSeqCall { get; }
        public bool bThisCall { get; }
        public ImmutableArray<string> TypeParams { get; }
        public TypeValue RetTypeValue { get; }
        public ImmutableArray<TypeValue> ParamTypeValues { get; }

        public FuncInfo(ModuleItemId? outerId, ModuleItemId funcId, bool bSeqCall, bool bThisCall, IReadOnlyList<string> typeParams, TypeValue retTypeValue, ImmutableArray<TypeValue> paramTypeValues)
        {
            OuterId = outerId;
            FuncId = funcId;
            this.bSeqCall = bSeqCall;
            this.bThisCall = bThisCall;            
            TypeParams = typeParams.ToImmutableArray();
            RetTypeValue = retTypeValue;
            ParamTypeValues = paramTypeValues;
        }

        public FuncInfo(ModuleItemId? outerId, ModuleItemId funcId, bool bSeqCall, bool bThisCall, IReadOnlyList<string> typeParams, TypeValue retTypeValues, params TypeValue[] paramTypeValues)
        {
            OuterId = outerId;
            FuncId = funcId;
            this.bSeqCall = bSeqCall;
            this.bThisCall = bThisCall;
            TypeParams = typeParams.ToImmutableArray();
            RetTypeValue = retTypeValues;
            ParamTypeValues = ImmutableArray.Create(paramTypeValues);
        }
    }
}
