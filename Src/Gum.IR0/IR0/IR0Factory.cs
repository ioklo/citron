using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Gum.Infra.Misc;

namespace Gum.IR0
{
    public static class IR0Factory
    {
        public static Script RScript(ModuleName moduleName, ImmutableArray<Decl> decls, params Stmt[] optTopLevelStmts)
        {   
            ImmutableArray<Stmt> topLevelStmts = optTopLevelStmts.ToImmutableArray();

            return new Script(moduleName, decls, topLevelStmts);
        }

        public static Script RScript(ModuleName moduleName, params Stmt[] stmts)
            => RScript(moduleName, default, stmts);

        public static CommandStmt RCommand(params StringExp[] cmds)
            => new CommandStmt(Arr(cmds));

        public static BlockStmt RBlock(params Stmt[] stmts)
            => new BlockStmt(Arr(stmts));

        public static StringExp RString(params StringExpElement[] elems)
            => new StringExp(Arr(elems));

        public static StringExp RString(string v)
            => new StringExp(Arr<StringExpElement>(new TextStringExpElement(v)));

        public static GlobalVarDeclStmt RGlobalVarDeclStmt(Path type, string name, Exp? initExp = null)
        {
            if (initExp == null)
                return new GlobalVarDeclStmt(Arr<VarDeclElement>(new VarDeclElement.NormalDefault(type, name)));

            else 
                return new GlobalVarDeclStmt(Arr<VarDeclElement>(new VarDeclElement.Normal(type, name, initExp)));
        }

        public static GlobalVarDeclStmt RGlobalRefVarDeclStmt(string name, Loc loc)
            => new GlobalVarDeclStmt(Arr<VarDeclElement>(new VarDeclElement.Ref(name, loc)));

        public static LocalVarDeclStmt RLocalVarDeclStmt(Path typeId, string name, Exp initExp)
            => new LocalVarDeclStmt(RLocalVarDecl(typeId, name, initExp));

        public static LocalVarDecl RLocalVarDecl(Path typeId, string name, Exp initExp)
            => new LocalVarDecl(Arr<VarDeclElement>(new VarDeclElement.Normal(typeId, name, initExp)));

        public static IntLiteralExp RInt(int v) => new IntLiteralExp(v);
        public static BoolLiteralExp RBool(bool v) => new BoolLiteralExp(v);

        public static ImmutableArray<Param> RParamInfo(params (Path Path, string Name)[] elems)
        {
            return elems.Select(e => new Param(ParamKind.Normal, e.Path, e.Name)).ToImmutableArray();
        }

        public static ImmutableArray<Argument> RArgs(params Exp[] exps)
        {
            return exps.Select(e => (Argument)new Argument.Normal(e)).ToImmutableArray();
        }
    }
}
