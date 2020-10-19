using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;

namespace Gum.IR0
{
    public class SeqFunc
    {
        public SeqFuncId Id { get; }

        public TypeId ElemTypeId { get; }

        public bool IsThisCall { get; set; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<string> ParamNames { get; }
        public Stmt Body { get; }
        
        public SeqFunc(SeqFuncId id, TypeId elemTypeId, bool bThisCall, IEnumerable<string> typeParams, IEnumerable<string> paramNames, Stmt body)
        {
            Id = id;
            ElemTypeId = elemTypeId;
            IsThisCall = bThisCall;
            TypeParams = typeParams.ToImmutableArray();
            ParamNames = paramNames.ToImmutableArray();
            Body = body;
        }
    }
}
