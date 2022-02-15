using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron
{
    partial struct IR0Evaluator
    {
        IR0GlobalContext globalContext;
        IR0EvalContext evalContext;
        IR0LocalContext localContext;

        internal IR0Evaluator(IR0GlobalContext globalContext, IR0EvalContext context, IR0LocalContext localContext)
        {
            this.globalContext = globalContext;
            this.evalContext = context;
            this.localContext = localContext;
        }
    }
}
