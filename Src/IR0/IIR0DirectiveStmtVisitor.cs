using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0;

public interface IIR0DirectiveStmtVisitor
{
    void VisitNull(DirectiveStmt.Null nullDirective);
    void VisitNotNull(DirectiveStmt.NotNull notNullDirecive);

    void VisitStaticNull(DirectiveStmt.StaticNull staticNullDirective);
    void VisitStaticNotNull(DirectiveStmt.StaticNotNull staticNotNullDirective);
    void VisitStaticUnknownNull(DirectiveStmt.StaticUnknownNull staticUnknownNullDirective);
}
