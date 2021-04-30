using System;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    abstract class RDeclBuilder
    {
        public abstract void Add(R.Decl decl);
    }

    class GlobalRDeclBuilder : RDeclBuilder
    {
        public override void Add(R.Decl decl)
        {
            throw new NotImplementedException();
        }
    }
}