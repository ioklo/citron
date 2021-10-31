using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.IR0Visitor
{
    public interface IIR0DirectiveStmtVisitor
    {
        void VisitNull(R.DirectiveStmt.Null nullDirective);
        void VisitNotNull(R.DirectiveStmt.NotNull notNullDirecive);

        void VisitStaticNull(R.DirectiveStmt.StaticNull staticNullDirective);
        void VisitStaticNotNull(R.DirectiveStmt.StaticNotNull staticNotNullDirective);
        void VisitStaticUnknownNull(R.DirectiveStmt.StaticUnknownNull staticUnknownNullDirective);
    }
}
