using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Gum.Infra.Misc;

namespace Gum.TextAnalysis.Test
{
    static class TestMisc
    {
        public static StringExp SimpleSStringExp(string value)
            => new StringExp(Arr<StringExpElement>(new TextStringExpElement(value)));

        public static IdentifierExp SimpleSId(string id)
            => new IdentifierExp(id, default);

        public static IdTypeExp SimpleSIdTypeExp(string id)
            => new IdTypeExp(id, default);

        public static BlockStmt SimpleSBlockStmt(params Stmt[] stmts)
            => new BlockStmt(stmts.ToImmutableArray());

        public static Script SimpleSScript(params ScriptElement[] elems)
            => new Script(elems.ToImmutableArray());

        public static VarDeclStmt SimpleSVarDeclStmt(TypeExp typeExp, params VarDeclElement[] elems)
            => new VarDeclStmt(SimpleSVarDecl(typeExp, elems));

        public static VarDecl SimpleSVarDecl(TypeExp typeExp, params VarDeclElement[] elems)
            => new VarDecl(typeExp, elems.ToImmutableArray());

    }
}
