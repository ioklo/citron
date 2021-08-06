﻿using Gum.Collections;
using System.Linq;

using static Gum.Infra.Misc;

namespace Gum.Syntax
{
    public static class SyntaxFactory
    {
        public static StringExp SString(params StringExpElement[] elems)
            => new StringExp(Arr(elems));

        public static StringExp SString(string s)
            => new StringExp(Arr<StringExpElement>(new TextStringExpElement(s)));

        public static CommandStmt SCommand(params StringExp[] exps)
            => new CommandStmt(Arr(exps));

        public static VarDecl SRefVarDecl(TypeExp typeExp, string name, Exp initExp)
        {
            return new VarDecl(true, typeExp, Arr(new VarDeclElement(name, new VarDeclElemInitializer(true, initExp))));
        }

        public static VarDecl SVarDecl(TypeExp typeExp, string name, Exp? initExp = null)
        {
            return new VarDecl(false, typeExp, Arr(new VarDeclElement(name, initExp == null ? null : new VarDeclElemInitializer(false, initExp))));
        }

        public static VarDeclStmt SVarDeclStmt(TypeExp typeExp, string name, Exp? initExp = null)
        {
            return new VarDeclStmt(SVarDecl(typeExp, name, initExp));
        }

        public static VarDeclStmt SRefVarDeclStmt(TypeExp typeExp, string name, Exp initExp)
        {
            return new VarDeclStmt(SRefVarDecl(typeExp, name, initExp));
        }

        public static Script SScript(params Stmt[] stmts)
        {
            return new Script(stmts.Select(stmt => (ScriptElement)new StmtScriptElement(stmt)).ToImmutableArray());
        }

        public static Script SScript(params ScriptElement[] elems)
            => new Script(Arr(elems));

        public static BlockStmt SBlock(params Stmt[] stmts)
            => new BlockStmt(Arr(stmts));

        public static IntLiteralExp SInt(int v) => new IntLiteralExp(v);
        public static BoolLiteralExp SBool(bool v) => new BoolLiteralExp(v);
        public static IdentifierExp SId(string name) => new IdentifierExp(name, default);
        public static IdentifierExp SId(string name, params TypeExp[] typeArgs) => new IdentifierExp(name, Arr(typeArgs));

        public static TypeExp SVarTypeExp() => new IdTypeExp("var", default);
        public static TypeExp SIntTypeExp() => new IdTypeExp("int", default);
        public static TypeExp SBoolTypeExp() => new IdTypeExp("bool", default);
        public static TypeExp SVoidTypeExp() => new IdTypeExp("void", default);
        public static TypeExp SStringTypeExp() => new IdTypeExp("string", default);

        public static IdTypeExp SIdTypeExp(string name, params TypeExp[] typeArgs) => new IdTypeExp(name, Arr(typeArgs));
    }
}