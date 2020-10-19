using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.IR0
{   
    public class Func
    {
        public FuncId Id { get; }
        public bool IsThisCall { get; set; }        

        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<string> ParamNames { get; }        
        public Stmt Body { get; }

        public Func(
            FuncId id,
            bool bThisCall,
            IEnumerable<string> typeParams, 
            IEnumerable<string> paramNames, 
            Stmt body)
        {
            Id = id;
            IsThisCall = bThisCall;

            TypeParams = typeParams.ToImmutableArray();
            ParamNames = paramNames.ToImmutableArray();
            Body = body;
        }
    }
}