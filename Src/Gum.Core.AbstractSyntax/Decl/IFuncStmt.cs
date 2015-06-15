using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gum.Core.AbstractSyntax;

namespace Gum.Core.AbstractSyntax
{
    public interface IFuncStmt
    {
        void Visit(IFuncStmtVisitor visitor);
    }
}
