using System;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    abstract class RDeclBuilder
    {
        public abstract void Add(R.IDecl decl);
    }

    class GlobalRDeclBuilder : RDeclBuilder
    {
        public override void Add(R.IDecl decl)
        {
            throw new NotImplementedException();
        }
    }
}