using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.CompileTime
{
    public class FuncInfo : ItemInfo
    {
        public bool bSeqCall { get; }
        public bool bThisCall { get; }
        public ImmutableArray<string> TypeParams { get; }
        public TypeValue RetTypeValue { get; }
        public ImmutableArray<TypeValue> ParamTypeValues { get; }

        public FuncInfo(ItemId id, bool bSeqCall, bool bThisCall, ImmutableArray<string> typeParams, TypeValue retTypeValue, ImmutableArray<TypeValue> paramTypeValues)
            : base(id)
        {
            this.bSeqCall = bSeqCall;
            this.bThisCall = bThisCall;            
            TypeParams = typeParams;
            RetTypeValue = retTypeValue;
            ParamTypeValues = paramTypeValues;
        }

        public FuncInfo(ItemId id, bool bSeqCall, bool bThisCall, IEnumerable<string> typeParams, TypeValue retTypeValue, IEnumerable<TypeValue> paramTypeValues)
            : this(id, bSeqCall, bThisCall, typeParams.ToImmutableArray(), retTypeValue, paramTypeValues.ToImmutableArray()) { }

        public FuncInfo(ItemId id, bool bSeqCall, bool bThisCall, ImmutableArray<string> typeParams, TypeValue retTypeValues, params TypeValue[] paramTypeValues)
            : this(id, bSeqCall, bThisCall, typeParams, retTypeValues, paramTypeValues.ToImmutableArray()) { }
    }
}
