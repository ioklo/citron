using Citron.Collections;
using Citron.CompileTime;
using Citron.Analysis;
using Citron.IR0;
using System.Diagnostics;

namespace Citron.Test.Misc
{
    public class ScriptBuilder
    {
        ModuleDeclSymbol moduleDecl;
        ImmutableArray<StmtBody>.Builder stmtBodiesBuilder;

        public ScriptBuilder(ModuleDeclSymbol moduleDecl)
        {
            this.moduleDecl = moduleDecl;
            this.stmtBodiesBuilder = ImmutableArray.CreateBuilder<StmtBody>();
        }

        public ScriptBuilder Add(IFuncDeclSymbol funcDecl, params Stmt[] bodyStmts)
        {
            var declSymbolId = funcDecl.GetDeclSymbolId();
            Debug.Assert(declSymbolId.ModuleName.Equals(moduleDecl.GetName()));
            Debug.Assert(declSymbolId.Path != null);
            stmtBodiesBuilder.Add(new StmtBody(declSymbolId.Path, bodyStmts.ToImmutableArray()));

            return this;
        }

        public ScriptBuilder AddTopLevel(ImmutableArray<Stmt> topLevelBody)
        {
            stmtBodiesBuilder.Add(new StmtBody(new DeclSymbolPath(null, Name.TopLevel), topLevelBody));
            return this;
        }

        public ScriptBuilder AddTopLevel(params Stmt[] topLevelStmts)
        {
            stmtBodiesBuilder.Add(new StmtBody(new DeclSymbolPath(null, Name.TopLevel), topLevelStmts.ToImmutableArray()));
            return this;
        }

        public Script Make()
        {
            return new Script(moduleDecl, stmtBodiesBuilder.ToImmutable());
        }
    }
}
