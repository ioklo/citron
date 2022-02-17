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
using Citron.CompileTime;

namespace Citron.IR0
{
    public class IR0Factory
    {
        [AllowNull]
        ITypeSymbol boolType;

        [AllowNull]
        ITypeSymbol intType;

        [AllowNull]
        ITypeSymbol stringType;

        public IR0Factory(ITypeSymbol boolType, ITypeSymbol intType, ITypeSymbol stringType)
        {
            this.boolType = boolType;
            this.intType = intType;
            this.stringType = stringType;
        }

        public IR0StmtBody StmtBody(DeclSymbolPath path, Stmt body)
        {
            return new IR0StmtBody(path, body);
        }

        #region Script

        public Script Script(Name moduleName, params Stmt[] stmts)
        {
            return Script(moduleName, default, stmts);
        }

        public Script Script(Name moduleName, ImmutableArray<IR0StmtBody> rstmtBodies, params Stmt[] optTopLevelStmts)
        {   
            ImmutableArray<Stmt> topLevelStmts = optTopLevelStmts.ToImmutableArray();
            return new Script(moduleName, rstmtBodies, topLevelStmts);
        }

        #endregion

        #region Stmt

        #region CommandStmt

        public CommandStmt Command(params StringExp[] cmds)
        { 
            return new CommandStmt(Arr(cmds));
        }

        #endregion

        #region BlockStmt

        public BlockStmt Block(params Stmt[] stmts)
        {
            return new BlockStmt(Arr(stmts));
        }

        #endregion        

        #region ExpStmt

        public ExpStmt Assign(Loc dest, Exp src)
        {
            return new ExpStmt(new AssignExp(dest, src));
        }

        #endregion        

        #region GlobalVarDeclStmt
        public GlobalVarDeclStmt GlobalVarDecl(ITypeSymbol type, string name, Exp? initExp = null)
        {
            if (initExp == null)
                return new GlobalVarDeclStmt(Arr<VarDeclElement>(new VarDeclElement.NormalDefault(type, name)));

            else
                return new GlobalVarDeclStmt(Arr<VarDeclElement>(new VarDeclElement.Normal(type, name, initExp)));
        }

        public GlobalVarDeclStmt GlobalRefVarDecl(string name, Loc loc)
        {
            return new GlobalVarDeclStmt(Arr<VarDeclElement>(new VarDeclElement.Ref(name, loc)));
        }

        #endregion

        #region LocalVarDeclStmt
        public LocalVarDeclStmt LocalVarDecl(ITypeSymbol type, string name, Exp initExp)
        {
            return new LocalVarDeclStmt(new LocalVarDecl(Arr<VarDeclElement>(new VarDeclElement.Normal(type, name, initExp))));
        }
        #endregion

        #region CommandStmt

        public CommandStmt PrintBool(Exp exp)
        {
            return Command(String(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Bool_String,
                    exp,
                    stringType
                )
            )));
        }

        public CommandStmt PrintInt(Loc loc)
        {
            return Command(String(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    new LoadExp(loc, intType),
                    stringType
                )
            )));
        }

        public CommandStmt PrintInt(Exp varExp)
        {
            return Command(String(new ExpStringExpElement(
                new CallInternalUnaryOperatorExp(
                    InternalUnaryOperator.ToString_Int_String,
                    varExp,
                    stringType
                )
            )));
        }

        public CommandStmt PrintStringCmdStmt(Loc loc, ITypeSymbol locType)
        {
            return Command(String(new ExpStringExpElement(new LoadExp(loc, locType))));
        }

        public CommandStmt PrintStringCmdStmt(Exp varExp)
        {
            return Command(String(new ExpStringExpElement(varExp)));
        }

        public CommandStmt PrintStringCmdStmt(string text)
        {
            return Command(String(text));
        }

        #endregion

        #endregion

        #region Exp

        #region StringExp
        public StringExp String(params StringExpElement[] elems)
        {
            return new StringExp(Arr(elems), stringType);
        }

        public StringExp String(string v)
        { 
            return new StringExp(Arr<StringExpElement>(new TextStringExpElement(v)), stringType);
        }

        #endregion

        #region IntLiteralExp

        public IntLiteralExp Int(int v)
        {
            return new IntLiteralExp(v, intType);
        }

        #endregion

        #region BoolLiteralExp

        public BoolLiteralExp Bool(bool v)
        {
            return new BoolLiteralExp(v, boolType);
        }

        #endregion

        #region LoadExp

        public LoadExp LoadLocalVar(string name, ITypeSymbol varType)
        {
            return new LoadExp(new LocalVarLoc(new Name.Normal(name)), varType);
        }

        #endregion

        #endregion

        #region Loc

        public LocalVarLoc LocalVar(string name)
        {
            return new LocalVarLoc(new Name.Normal(name));
        }

        #endregion

        #region Else

        public VarDeclForStmtInitializer VarDeclForStmtInitializer(ITypeSymbol type, string name, Exp initExp)
        {
            return new VarDeclForStmtInitializer(new LocalVarDecl(Arr<VarDeclElement>(new VarDeclElement.Normal(type, name, initExp))));
        }

        public ImmutableArray<Param> NormalParams(params (SymbolId Type, string Name)[] elems)
        {
            return elems.Select(e => new Param(ParamKind.Default, e.Type, new Name.Normal(e.Name))).ToImmutableArray();
        }

        public ImmutableArray<Argument> Args(params Exp[] exps)
        {
            return exps.Select(e => (Argument)new Argument.Normal(e)).ToImmutableArray();
        }

        public ModuleSymbolId Module(string moduleName)
        {
            return new ModuleSymbolId(new Name.Normal(moduleName), null);
        }

        #endregion
    }

    public static class IR0Extensions
    {
        public static ClassMemberLoc ClassMember(this Loc instance, ClassMemberVarSymbol memberVar)
        {
            return new ClassMemberLoc(instance, memberVar);
        }
    }
}
