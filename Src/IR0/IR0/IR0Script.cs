using Pretune;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Citron.Collections;
using Citron.Module;
using Citron.Symbol;

namespace Citron.IR0
{
    // DeclSymbol -> Stmt
    public record struct StmtBody(IDeclSymbolNode declSymbol, ImmutableArray<Stmt> Stmts);
    public record class Script(ModuleDeclSymbol ModuleDeclSymbol,  ImmutableArray<StmtBody> StmtBodies);
}
