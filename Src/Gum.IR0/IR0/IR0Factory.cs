﻿using System;
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

        public static Stmt RAssignStmt(Loc dest, Exp src)
        {
            return new ExpStmt(new AssignExp(dest, src));
        }

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

        public static ImmutableArray<Param> RNormalParams(params (Path Path, string Name)[] elems)
        {
            return elems.Select(e => new Param(ParamKind.Normal, e.Path, e.Name)).ToImmutableArray();
        }

        public static ImmutableArray<Argument> RArgs(params Exp[] exps)
        {
            return exps.Select(e => (Argument)new Argument.Normal(e)).ToImmutableArray();
        }

        public static CommandStmt RPrintBoolCmdStmt(Exp exp)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Bool_String,
                    exp
                )
            )));
        }

        public static CommandStmt RPrintIntCmdStmt(Loc loc)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    new LoadExp(loc))
            )));
        }


        public static CommandStmt RPrintIntCmdStmt(Exp varExp)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    varExp)
            )));
        }

        public static CommandStmt RPrintStringCmdStmt(Loc loc)
        {
            return RCommand(RString(new ExpStringExpElement(new LoadExp(loc))));
        }


        public static CommandStmt RPrintStringCmdStmt(Exp varExp)
        {
            return RCommand(RString(new ExpStringExpElement(varExp)));
        }

        public static CommandStmt RPrintStringCmdStmt(string text)
        {
            return RCommand(RString(text));
        }

        public static Path.Root RRoot(ModuleName moduleName)
            => new Path.Root(moduleName);

        public static Path.Nested Child(this Path.Normal outer, Name name, ParamHash paramHash, ImmutableArray<Path> typeArgs)
            => new Path.Nested(outer, name, paramHash, typeArgs);

        public static Path.Nested Child(this Path.Normal outer, Name name)
            => new Path.Nested(outer, name, ParamHash.None, default);

        // no typeparams, all normal paramtypes
        public static Path.Nested Child(this Path.Normal outer, Name name, params Path[] types)
            => new Path.Nested(outer, name, new ParamHash(0, types.Select(type => new ParamHashEntry(ParamKind.Normal, type)).ToImmutableArray()), default);

        // no typeparams version
        public static Path.Nested Child(this Path.Normal outer, Name name, params ParamHashEntry[] entries)
            => new Path.Nested(outer, name, new ParamHash(0, entries.ToImmutableArray()), default);

        public static ClassMemberLoc ClassMember(this Loc instance, Path.Nested memberPath)
        {
            return new ClassMemberLoc(instance, memberPath);
        }

        public static Exp RLocalVarExp(string name)
        {
            return new LoadExp(new LocalVarLoc(name));
        }

        public static ParamHash RNormalParamHash(params Path[] paths)
        {
            return new ParamHash(0, paths.Select(path => new ParamHashEntry(ParamKind.Normal, path)).ToImmutableArray());
        }


    }
}
