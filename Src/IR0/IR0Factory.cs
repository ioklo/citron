using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Citron.Infra.Misc;
using System.Diagnostics.CodeAnalysis;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.IR0
{
    public class IR0Factory
    {
        public delegate IType ListTypeConstructor(IType itemType);
        public delegate IType ListIterTypeConstructor(IType itemType);

        Name moduleName;
        IType voidType, boolType, intType, stringType;
        ListTypeConstructor listTypeConstructor;
        ListIterTypeConstructor listIterTypeConstructor;

        public IR0Factory(Name moduleName, IType voidType, IType boolType, IType intType, IType stringType, 
            ListTypeConstructor listTypeConstructor,
            ListIterTypeConstructor listIterTypeConstructor)
        {
            this.moduleName = moduleName;
            this.voidType = voidType;
            this.boolType = boolType;
            this.intType = intType;
            this.stringType = stringType;
            this.listTypeConstructor = listTypeConstructor;
            this.listIterTypeConstructor = listIterTypeConstructor;
        }

        public StmtBody StmtBody(IFuncDeclSymbol symbol, params Stmt[] body)
        {
            return new StmtBody(symbol, body.ToImmutableArray());
        }

        #region Script

        public Script Script(ModuleDeclSymbol moduleDecl, params StmtBody[] rstmtBodies)
        {   
            return new Script(moduleDecl, rstmtBodies.ToImmutableArray());
        }

        public Script Script(params Stmt[] stmts)
        {
            return Script(stmts.ToImmutableArray());
        }

        // void Main(), int 리턴이 필요하면 직접 제작한다
        public Script Script(ImmutableArray<Stmt> stmts)
        {
            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var entryD = new GlobalFuncDeclSymbol(moduleD,
                Accessor.Private, new Name.Normal("Main"), typeParams: default);

            entryD.InitFuncReturnAndParams(
                new FuncReturn(voidType), default);

            moduleD.AddFunc(entryD);
            
            var stmtBodies = Arr(new StmtBody(entryD, stmts));
            return new Script(moduleD, stmtBodies);
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
        public IfStmt If(Exp cond, ImmutableArray<Stmt> body, ImmutableArray<Stmt> elseBody)
        {
            return new IfStmt(cond, body, elseBody);
        }

        public IfStmt If(Exp cond, params Stmt[] body)
        {
            return new IfStmt(cond, body.ToImmutableArray(), ElseBody: default);
        }
        #endregion

        #region ForStmt

        // skip initializer
        public ForStmt For(Exp? cond, Exp? cont, params Stmt[] body)
        {
            return new ForStmt(InitStmts: default, cond, cont, body.ToImmutableArray());
        }

        // with local item var decl
        public ForStmt For(IType itemType, string itemName, Exp itemInit, Exp? cond, Exp? cont, params Stmt[] body)
        {
            return new ForStmt(
                Arr<Stmt>(new LocalVarDeclStmt(itemType, itemName, itemInit)),
                cond, cont, body.ToImmutableArray()
            );
        }

        public ForStmt For(Exp init, Exp? cond, Exp? cont, params Stmt[] body)
        {
            return new ForStmt(
                Arr<Stmt>(new ExpStmt(init)), cond, cont, body.ToImmutableArray());
        }

        public ForStmt For(ImmutableArray<Stmt> initStmts, Exp? cond, Exp? cont, params Stmt[] body)
        {
            return new ForStmt(initStmts, cond, cont, body.ToImmutableArray());
        }

        #endregion

        #region Foreach
        public ForeachStmt Foreach(IType itemType, string elemName, Loc iterLoc, params Stmt[] body)
        {
            return new ForeachStmt(itemType, elemName, iterLoc, body.ToImmutableArray());
        }

        public ForeachStmt Foreach(IType itemType, string elemName, Exp iterExp, params Stmt[] body)
        {
            return new ForeachStmt(itemType, elemName, new TempLoc(iterExp), body.ToImmutableArray());
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
                _ => throw new UnreachableException()
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
                _ => throw new UnreachableException()
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

                _ => throw new UnreachableException()
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

        #region LocalVarDeclStmt
        public LocalVarDeclStmt LocalVarDecl(IType type, string name, Exp? initExp)
        {
            return new LocalVarDeclStmt(type, name, initExp);
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

        #region Continue
        public Stmt Continue() 
        {
            return new ContinueStmt();
        }
        #endregion

        #region Break
        public Stmt Break() { return new BreakStmt(); }
        #endregion

        #region ReturnStmt
        public ReturnStmt Return(Exp retValue)
        {
            return new ReturnStmt(new ReturnInfo.Expression(retValue));
        }

        public ReturnStmt Return()
        {
            return new ReturnStmt(new ReturnInfo.None());
        }
        
        #endregion

        #region TaskStmt
        public TaskStmt Task(LambdaSymbol lambda)
        {
            return new TaskStmt(lambda, default);
        }

        public TaskStmt Task(LambdaSymbol lambda, params Argument[] args)
        {
            return new TaskStmt(lambda, args.ToImmutableArray());
        }
        #endregion

        #region AsyncStmt
        public AsyncStmt Async(LambdaSymbol lambda, params Argument[] args)
        {
            return new AsyncStmt(lambda, args.ToImmutableArray());
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

        public LoadExp LoadLocalVar(string name, IType varType)
        {
            return new LoadExp(new LocalVarLoc(new Name.Normal(name)), varType);
        }

        public LoadExp Load(Loc loc, IType typeSymbol)
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

        public LambdaMemberVarLoc LambdaMember(LambdaMemberVarSymbol symbol)
        {
            return new LambdaMemberVarLoc(symbol);
        }

        public Loc LocalDeref(Loc innerLoc)
        {
            return new LocalDerefLoc(innerLoc);
        }
        
        public TempLoc TempLoc(Exp e)
        {
            return new TempLoc(e);
        }

        #endregion

        #region Type
        public IType VoidType()
        {
            return voidType;
        }

        public IType BoolType()
        {
            return boolType;
        }

        public IType IntType()
        {
            return intType;
        }

        public IType StringType()
        {
            return stringType;
        }

        public IType ListType(IType itemType)
        {
            return listTypeConstructor.Invoke(itemType);
        }

        public IType ListIterType(IType itemType)
        {
            return listIterTypeConstructor.Invoke(itemType);
        }

        #endregion

        #region Else
        
        public ImmutableArray<FuncParameter> FuncParams(params (IType Type, string Name)[] elems)
        {
            return elems.Select(e => new FuncParameter(e.Type, new Name.Normal(e.Name))).ToImmutableArray();
        }

        public ImmutableArray<Argument> Args(params Exp[] exps)
        {
            return exps.Select(e => (Argument)new Argument.Normal(e)).ToImmutableArray();
        }

        public SymbolId Module(string moduleName)
        {
            return new SymbolId(new Name.Normal(moduleName), null);
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
            return new AwaitStmt(stmts.ToImmutableArray());
        }
        
        public FuncReturn FuncRet(IType type)
        {
            return new FuncReturn(type);
        }

        public Argument Arg(Exp exp)
        {
            return new Argument.Normal(exp);
        }

        public LambdaExp Lambda(LambdaSymbol lambda, params Argument[] args)
        {
            return new LambdaExp(lambda, args.ToImmutableArray());
        }

        public ListExp List(IType itemType, params Exp[] exps)
        {
            var listType = listTypeConstructor.Invoke(itemType);
            return new ListExp(exps.ToImmutableArray(), listType);
        }

        public ListIndexerLoc ListIndexer(LocalVarLoc list, Exp index)
        {
            return new ListIndexerLoc(list, index);
        }

        // empty
        public ListIteratorExp EmptyListIter(IType itemType)
        {
            return new ListIteratorExp(
                new TempLoc(new ListExp(Elems: default, itemType)),
                ListIterType(itemType)
            );
        }

        public YieldStmt Yield(Exp exp)
        {
            return new YieldStmt(exp);
        }

        public IType LocalRefType(IType type)
        {
            return new LocalRefType(type);
        }

        public Exp Box(Exp exp)
        {
            return new BoxExp(exp);
        }

        public FuncType FuncType(IType retType, params IType[] paramTypes)
        {
            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParameter>(paramTypes.Length);

            int i = 0;
            foreach (var paramType in paramTypes)
            {
                var name = new Name.Anonymous(i); // 이름은 임의로 짓는다
                paramsBuilder.Add(new FuncParameter(paramType, name));
            }

            return new FuncType(new FuncReturn(retType), paramsBuilder.MoveToImmutable());
        }

        public Exp LocalRef(Loc loc, IType locType)
        {
            return new LocalRefExp(loc, locType);
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
