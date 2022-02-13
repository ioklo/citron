using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Citron.Infra.Misc;
using Citron.Analysis;
using System.Diagnostics.CodeAnalysis;

namespace Citron.IR0
{
    public static class IR0Factory
    {           
        [AllowNull]
        static ITypeSymbol BoolType, IntType, StringType;

        public static void Init(ITypeSymbol boolType, ITypeSymbol intType, ITypeSymbol stringType)
        {
            BoolType = boolType;
            IntType = intType;
            StringType = stringType;
        }

        public static Script RScript(ModuleName moduleName, ImmutableArray<TypeDecl> typeDecls, ImmutableArray<FuncDecl> funcDecls, ImmutableArray<CallableMemberDecl> callableMemberDecls, params Stmt[] optTopLevelStmts)
        {   
            ImmutableArray<Stmt> topLevelStmts = optTopLevelStmts.ToImmutableArray();

            return new Script(moduleName, typeDecls, funcDecls, callableMemberDecls, topLevelStmts);
        }

        public static Script RScript(ModuleName moduleName, params Stmt[] stmts)
            => RScript(moduleName, default, default, default, stmts);

        public static CommandStmt RCommand(params StringExp[] cmds)
            => new CommandStmt(Arr(cmds));

        public static BlockStmt RBlock(params Stmt[] stmts)
            => new BlockStmt(Arr(stmts));

        public static StringExp RString(params StringExpElement[] elems)
            => new StringExp(Arr(elems), StringType);

        public static StringExp RString(string v)
            => new StringExp(Arr<StringExpElement>(new TextStringExpElement(v)), StringType);

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

        public static IntLiteralExp RInt(int v) => new IntLiteralExp(v, IntType);
        public static BoolLiteralExp RBool(bool v) => new BoolLiteralExp(v, BoolType);

        public static ImmutableArray<Param> RNormalParams(params (Path Path, string Name)[] elems)
        {
            return elems.Select(e => new Param(ParamKind.Default, e.Path, new Name.Normal(e.Name))).ToImmutableArray();
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
                    exp,
                    StringType
                )
            )));
        }

        public static CommandStmt RPrintIntCmdStmt(Loc loc)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    new LoadExp(loc, IntType),
                    StringType
                )
            )));
        }


        public static CommandStmt RPrintIntCmdStmt(Exp varExp)
        {
            return RCommand(RString(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    varExp,
                    StringType
                )
            )));
        }

        public static CommandStmt RPrintStringCmdStmt(Loc loc, ITypeSymbol locType)
        {
            return RCommand(RString(new ExpStringExpElement(new LoadExp(loc, locType))));
        }


        public static CommandStmt RPrintStringCmdStmt(Exp varExp)
        {
            return RCommand(RString(new ExpStringExpElement(varExp)));
        }

        public static CommandStmt RPrintStringCmdStmt(string text)
        {
            return RCommand(RString(text));
        }

        public static Path.Root RRoot(string moduleName)
            => new Path.Root(new ModuleName(moduleName));

        public static ClassMemberLoc ClassMember(this Loc instance, ClassMemberVarSymbol memberVar)
        {
            return new ClassMemberLoc(instance, memberVar);
        }

        public static Loc RLocalVarLoc(string name)
        {
            return new LocalVarLoc(new Name.Normal(name));
        }

        public static Exp RLocalVarExp(string name, ITypeSymbol varType)
        {
            return new LoadExp(new LocalVarLoc(new Name.Normal(name)), varType);
        }

        public static ParamHash RNormalParamHash(params Path[] paths)
        {
            return new ParamHash(0, paths.Select(path => new ParamHashEntry(ParamKind.Default, path)).ToImmutableArray());
        }


    }
}
