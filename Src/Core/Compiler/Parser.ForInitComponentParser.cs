using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        private IForInitComponent ParseForInit()
        {
            IExpComponent exp;

            if( RollbackIfFailed(out exp, ParseExp) )
            {
                IForInitComponent forInit = exp as IForInitComponent;
                if (forInit == null)
                    throw CreateException();

                return forInit;
            }

            VarDecl varDecl;
            if( RollbackIfFailed(out varDecl, ParseVarDecl))
                return varDecl;

            throw CreateException();
        }
        
    }
}