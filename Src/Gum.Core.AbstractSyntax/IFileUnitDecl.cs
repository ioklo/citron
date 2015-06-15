using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    public interface IFileUnitDecl
    {
        void Visit(IFileUnitDeclVisitor visitor);
    }
}
