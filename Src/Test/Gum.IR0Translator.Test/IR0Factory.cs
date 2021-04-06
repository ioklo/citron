using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.IR0;
using static Gum.Infra.Misc;

using Type = Gum.IR0.Type;

namespace Gum.IR0Translator.Test
{
    static class IR0Factory
    {
        public static Script RScript(ImmutableArray<TypeDecl> typeDecls, ImmutableArray<FuncDecl> funcDecls, params Stmt[] optTopLevelStmts)
        {
            // TODO: Validator
            for(int i = 0; i < funcDecls.Length; i++)
                Debug.Assert(i == funcDecls[i].Id.Value);
            
            ImmutableArray<Stmt> topLevelStmts = optTopLevelStmts.ToImmutableArray();

            return new Script(typeDecls, funcDecls, topLevelStmts);
        }

        public static Script RScript(params Stmt[] stmts)
            => RScript(default, default, stmts);

        public static CommandStmt RCommand(params StringExp[] cmds)
            => new CommandStmt(Arr(cmds));

        public static BlockStmt RBlock(params Stmt[] stmts)
            => new BlockStmt(Arr(stmts));

        public static StringExp RString(params StringExpElement[] elems)
            => new StringExp(Arr(elems));

        public static StringExp RString(string v)
            => new StringExp(Arr<StringExpElement>(new TextStringExpElement(v)));

        

        public static PrivateGlobalVarDeclStmt RGlobalVarDeclStmt(Type type, string name, Exp? initExp = null)
            => new PrivateGlobalVarDeclStmt(Arr(new VarDeclElement(name, type, initExp)));

        public static LocalVarDeclStmt RLocalVarDeclStmt(Type typeId, string name, Exp? initExp = null)
            => new LocalVarDeclStmt(RLocalVarDecl(typeId, name, initExp));

        public static LocalVarDecl RLocalVarDecl(Type typeId, string name, Exp? initExp = null)
            => new LocalVarDecl(Arr(new VarDeclElement(name, typeId, initExp)));

        public static IntLiteralExp RInt(int v) => new IntLiteralExp(v);
        public static BoolLiteralExp RBool(bool v) => new BoolLiteralExp(v);
    }
}
