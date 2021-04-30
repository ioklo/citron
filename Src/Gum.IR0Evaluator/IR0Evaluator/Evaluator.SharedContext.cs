using System;
using System.Collections.Generic;
using Gum.Collections;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        class SharedContext
        {
            public ImmutableArray<R.Decl> Decls { get; }
            public Dictionary<string, Value> PrivateGlobalVars { get; }

            public SharedContext(ImmutableArray<R.Decl> decls)
            {
                Decls = decls;
                PrivateGlobalVars = new Dictionary<string, Value>();
            }

            // 여기서 만들어 내면 됩니다
            public FuncInvoker GetFuncInvoker(R.Path path)
            {
                throw new NotImplementedException();
            }

            // X<>.Y<,>.F 가 나온다. TypeContext정보는 따로 
            public SequenceFuncDecl GetSequenceFuncDecl(R.Path seqFunc)
            {
                if (seqFunc is R.Path.Root rootSeqFunc)


            }
        }
    }    
}
