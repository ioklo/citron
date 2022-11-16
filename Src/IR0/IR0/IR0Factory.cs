using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Citron.Infra.Misc;
using System.Diagnostics.CodeAnalysis;
using Citron.Module;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.IR0
{
    public class IR0Factory
    {
        public delegate ITypeSymbol ListTypeConstructor(ITypeSymbol itemType);

        ITypeSymbol boolType, intType, stringType;
        ListTypeConstructor listTypeConstructor;

        public IR0Factory(ITypeSymbol boolType, ITypeSymbol intType, ITypeSymbol stringType, ListTypeConstructor listTypeConstructor)
        {
            this.boolType = boolType;
            this.intType = intType;
            this.stringType = stringType;
            this.listTypeConstructor = listTypeConstructor;
        }

        public StmtBody StmtBody(DeclSymbolPath path, ImmutableArray<Stmt> body)
        {
            return new StmtBody(path, body);
        }

        #region Script

        public Script Script(ModuleDeclSymbol moduleDecl, ImmutableArray<StmtBody> rstmtBodies)
        {   
            return new Script(moduleDecl, rstmtBodies);
        }

        // only have top level stmts
        public Script Script(Name moduleName, ImmutableArray<Stmt> topLevelStmts)
        {
            var moduleDeclHolder = new Holder<ModuleDeclSymbol>();
            var topLevelFuncDecl = new GlobalFuncDeclSymbol(
                moduleDeclHolder, 
                AccessModifier.Public, 
                new Holder<FuncReturn>(new FuncReturn(false, intType)), Name.TopLevel, typeParams: default, paramIds: default,
                new Holder<ImmutableArray<FuncParameter>>(default), true, lambdaDecls: default);

            var moduleDecl = new ModuleDeclSymbol(moduleName, default, default, Arr(topLevelFuncDecl));
            moduleDeclHolder.SetValue(moduleDecl);
            var stmtBodies = Arr(new StmtBody(new DeclSymbolPath(null, Name.TopLevel), topLevelStmts));
            

            return new Script(moduleDecl, stmtBodies);
        }

        #endregion

        #region Stmt

        #region CommandStmt

        public CommandStmt Command(params StringExp[] cmds)
        { 
            return new CommandStmt(Arr(cmds));
        }

        #endregion

        #region IfStmt
        public IfStmt If(Exp cond, Stmt body, Stmt? elseBody = null)
        {
            return new IfStmt(cond, body, elseBody);
        }
        #endregion

        #region ForStmt

        // skip initializer
        public ForStmt For(Exp? condExp, Exp? contExp, Stmt body)
        {
            return new ForStmt(null, condExp, contExp, body);
        }

        // with local item var decl
        public ForStmt For(ITypeSymbol itemType, string itemName, Exp itemInit, Exp? cond, Exp? cont, Stmt body)
        {
            return new ForStmt(
                new VarDeclForStmtInitializer(new LocalVarDecl(Arr<VarDeclElement>(new VarDeclElement.Normal(itemType, itemName, itemInit)))),
                cond, cont, body
            );
        }

        public ForStmt For(Exp init, Exp? cond, Exp? cont, Stmt body)
        {
            return new ForStmt(new ExpForStmtInitializer(init), cond, cont, body);
        }

        #endregion

        #region BlockStmt

        public BlockStmt Block(params Stmt[] stmts)
        {
            return new BlockStmt(Arr(stmts));
        }

        public CallInternalUnaryOperatorExp CallInternalUnary(InternalUnaryOperator op, Exp exp)
        {
            var type = op switch
            {
                InternalUnaryOperator.LogicalNot_Bool_Bool => boolType,
                InternalUnaryOperator.UnaryMinus_Int_Int => intType,
                InternalUnaryOperator.ToString_Bool_String => stringType,
                InternalUnaryOperator.ToString_Int_String => stringType,
                _ => throw new UnreachableCodeException()
            };
            
            return new CallInternalUnaryOperatorExp(op, exp, type);
        }

        public Exp CallInternalBinary(InternalBinaryOperator op, Exp operand0, Exp operand1)
        {
            var type = op switch
            {
                InternalBinaryOperator.Multiply_Int_Int_Int => intType,
                InternalBinaryOperator.Divide_Int_Int_Int => intType,
                InternalBinaryOperator.Modulo_Int_Int_Int => intType,
                InternalBinaryOperator.Add_Int_Int_Int => intType,
                InternalBinaryOperator.Add_String_String_String => stringType,
                InternalBinaryOperator.Subtract_Int_Int_Int => intType,
                InternalBinaryOperator.LessThan_Int_Int_Bool => boolType,
                InternalBinaryOperator.LessThan_String_String_Bool => boolType,
                InternalBinaryOperator.GreaterThan_Int_Int_Bool => boolType,
                InternalBinaryOperator.GreaterThan_String_String_Bool => boolType,
                InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool => boolType,
                InternalBinaryOperator.LessThanOrEqual_String_String_Bool => boolType,
                InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool => boolType,
                InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool => boolType,
                InternalBinaryOperator.Equal_Int_Int_Bool => boolType,
                InternalBinaryOperator.Equal_Bool_Bool_Bool => boolType,
                InternalBinaryOperator.Equal_String_String_Bool => boolType,
                _ => throw new UnreachableCodeException()
            };

            return new CallInternalBinaryOperatorExp(op, operand0, operand1, type);
        }

        public Exp CallInternalUnaryAssign(InternalUnaryAssignOperator op, Loc operand)
        {
            var type = op switch
            {
                InternalUnaryAssignOperator.PrefixInc_Int_Int => intType,
                InternalUnaryAssignOperator.PrefixDec_Int_Int => intType,
                InternalUnaryAssignOperator.PostfixInc_Int_Int => intType,
                InternalUnaryAssignOperator.PostfixDec_Int_Int => intType,

                _ => throw new UnreachableCodeException()
            };


            return new CallInternalUnaryAssignOperatorExp(op, operand, type);
        }

        #endregion

        #region ExpStmt

        public ExpStmt Assign(Loc dest, Exp src)
        {
            return new ExpStmt(new AssignExp(dest, src));
        }        

        public ExpStmt Call(GlobalFuncSymbol globalFunc, params Argument[] args)
        {
            return new ExpStmt(new CallGlobalFuncExp(globalFunc, args.ToImmutableArray()));
        }

        public ExpStmt Call(ClassMemberFuncSymbol classMemberFunc, Loc? instance, params Argument[] args)
        {
            return new ExpStmt(new CallClassMemberFuncExp(classMemberFunc, instance, args.ToImmutableArray()));
        }

        public ExpStmt Call(LambdaSymbol lambda, Loc loc, params Argument[] args)
        {
            return new ExpStmt(new CallValueExp(lambda, loc, args.ToImmutableArray()));
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

        #region BlankStmt
        public BlankStmt Blank()
        {
            return new BlankStmt();
        }
        #endregion

        #region CommandStmt

        public CommandStmt PrintBool(Loc loc)
        {
            return PrintBool(new LoadExp(loc, boolType));
        }

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

        public CommandStmt PrintString(Loc loc)
        {
            return Command(String(new ExpStringExpElement(new LoadExp(loc, stringType))));
        }

        public CommandStmt PrintString(Exp exp)
        {
            return Command(String(new ExpStringExpElement(exp)));
        }

        public CommandStmt PrintString(string text)
        {
            return Command(String(text));
        }

        #endregion

        #region ReturnStmt
        public ReturnStmt Return(Exp retValue)
        {
            return new ReturnStmt(new ReturnInfo.Expression(retValue));
        }

        public ReturnStmt Return()
        {
            return new ReturnStmt(ReturnInfo.None.Instance);
        }

        public ReturnStmt ReturnRef(Loc loc)
        {
            return new ReturnStmt(new ReturnInfo.Ref(loc));
        }

        #endregion

        #region TaskStmt
        public TaskStmt Task(LambdaSymbol lambda, ImmutableArray<Stmt> body)
        {
            return new TaskStmt(lambda, default, body);
        }

        public TaskStmt Task(LambdaSymbol lambda, ImmutableArray<Argument> args, ImmutableArray<Stmt> body)
        {
            return new TaskStmt(lambda, args, body);
        }
        #endregion

        #region AsyncStmt
        public AsyncStmt Async(LambdaSymbol lambda, ImmutableArray<Argument> args, ImmutableArray<Stmt> body)
        {
            return new AsyncStmt(lambda, args, body);
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

        public LoadExp LoadGlobalVar(string name, ITypeSymbol varType)
        {
            return new LoadExp(new GlobalVarLoc(new Name.Normal(name)), varType);
        }

        public LoadExp Load(Loc loc, ITypeSymbol typeSymbol)
        {
            return new LoadExp(loc, typeSymbol);
        }

        public LoadExp LoadLambdaMember(LambdaMemberVarSymbol memberVar)
        {
            return new LoadExp(new LambdaMemberVarLoc(memberVar), memberVar.GetDeclType());
        }

        #endregion

        #endregion

        #region Loc

        public LocalVarLoc LocalVar(string name)
        {
            return new LocalVarLoc(new Name.Normal(name));
        }

        public Loc GlobalVar(string name)
        {
            return new GlobalVarLoc(new Name.Normal(name));
        }

        public Loc Deref(Loc loc)
        {
            return new DerefLocLoc(loc);
        }

        public TempLoc TempLoc(Exp e)
        {
            return new TempLoc(e);
        }

        #endregion

        #region Else        

        public ImmutableArray<FuncParameter> FuncParam(params (ITypeSymbol Type, string Name)[] elems)
        {
            return elems.Select(e => new FuncParameter(FuncParameterKind.Default, e.Type, new Name.Normal(e.Name))).ToImmutableArray();
        }

        public IHolder<ImmutableArray<FuncParameter>> FuncParamHolder()
        {
            return new Holder<ImmutableArray<FuncParameter>>(default);
        }

        public IHolder<ImmutableArray<FuncParameter>> FuncParamHolder(params (ITypeSymbol Type, string Name)[] elems)
        {
            return new Holder<ImmutableArray<FuncParameter>>(elems.Select(e => new FuncParameter(FuncParameterKind.Default, e.Type, new Name.Normal(e.Name))).ToImmutableArray());
        }

        public IHolder<ImmutableArray<FuncParameter>> FuncParamHolder(params FuncParameter[] funcParams)
        {
            return new Holder<ImmutableArray<FuncParameter>>(funcParams.ToImmutableArray());
        }


        public ImmutableArray<Argument> Args(params Exp[] exps)
        {
            return exps.Select(e => (Argument)new Argument.Normal(e)).ToImmutableArray();
        }

        public ModuleSymbolId Module(string moduleName)
        {
            return new ModuleSymbolId(new Name.Normal(moduleName), null);
        }

        public TextStringExpElement TextElem(string text)
        {
            return new TextStringExpElement(text);
        }

        public ExpStringExpElement ExpElem(Exp exp)
        {
            return new ExpStringExpElement(exp);
        }

        public Exp AssignExp(Loc dest, Exp src)
        {
            return new AssignExp(dest, src);
        }

        public CallGlobalFuncExp CallExp(GlobalFuncSymbol globalFunc, params Argument[] args)
        {
            return new CallGlobalFuncExp(globalFunc, args.ToImmutableArray());
        }

        public AwaitStmt Await(params Stmt[] stmts)
        {
            return new AwaitStmt(new BlockStmt(stmts.ToImmutableArray()));
        }

        public FuncParameter RefParam(ITypeSymbol type, string name)
        {
            return new FuncParameter(FuncParameterKind.Ref, type, new Name.Normal(name));
        }

        public IHolder<FuncReturn> FuncRetHolder(ITypeSymbol type)
        {
            return new Holder<FuncReturn>(new FuncReturn(false, type));
        }

        public FuncReturn FuncRet(ITypeSymbol type)
        {
            return new FuncReturn(false, type);
        }

        public Argument RefArg(Loc loc)
        {
            return new Argument.Ref(loc);
        }

        public Argument Arg(Exp exp)
        {
            return new Argument.Normal(exp);
        }

        public LambdaExp Lambda(LambdaSymbol lambda, params Argument[] args)
        {
            return new LambdaExp(lambda, args.ToImmutableArray());
        }

        public ListExp List(ITypeSymbol itemType, params Exp[] exps)
        {
            var listType = listTypeConstructor.Invoke(itemType);
            return new ListExp(exps.ToImmutableArray(), listType);
        }

        public ListIndexerLoc ListIndexer(LocalVarLoc list, Exp index)
        {
            return new ListIndexerLoc(list, index);
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
