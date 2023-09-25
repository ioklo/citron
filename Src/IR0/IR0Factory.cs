﻿using System;
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

            entryD.InitFuncReturnAndParams(new FuncReturn(voidType), parameters: default, bLastParamVariadic: false);

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
        public ForeachStmt Foreach(IType enumeratorType, Exp enumeratorExp, IType itemType, string varName, Exp nextExp, params Stmt[] body)
        {
            return new ForeachStmt(enumeratorType, enumeratorExp, itemType, new Name.Normal(varName), nextExp, body.ToImmutableArray());
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

            return new CallInternalUnaryOperatorExp(op, exp);
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

            return new CallInternalBinaryOperatorExp(op, operand0, operand1);
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


            return new CallInternalUnaryAssignOperatorExp(op, operand);
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

        public ExpStmt Call(LambdaSymbol lambdaSymbol, Loc loc, params Argument[] args)
        {
            return new ExpStmt(new CallLambdaExp(lambdaSymbol, loc, args.ToImmutableArray()));
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
                new TempLoc(
                    new CallInternalUnaryOperatorExp(
                        InternalUnaryOperator.ToString_Bool_String,
                        exp
                    ),
                    stringType
                )
            )));
        }

        public CommandStmt PrintInt(Loc loc)
        {
            return Command(String(new ExpStringExpElement(
                new TempLoc(
                    new CallInternalUnaryOperatorExp(
                        InternalUnaryOperator.ToString_Int_String,
                        new LoadExp(loc, intType)
                    ),
                    stringType
                )
            )));
        }

        public CommandStmt PrintInt(Exp varExp)
        {
            return Command(String(new ExpStringExpElement(
                new TempLoc(
                    new CallInternalUnaryOperatorExp(
                        InternalUnaryOperator.ToString_Int_String,
                        varExp
                    ),
                    stringType
                )
            )));
        }

        public CommandStmt PrintString(Loc loc)
        {
            return Command(String(new ExpStringExpElement(loc)));
        }

        public CommandStmt PrintString(Exp exp)
        {
            return Command(String(new ExpStringExpElement(new TempLoc(exp, stringType))));
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
            return new StringExp(Arr(elems));
        }

        public StringExp String(string v)
        {
            return new StringExp(Arr<StringExpElement>(new TextStringExpElement(v)));
        }

        #endregion

        #region IntLiteralExp

        public IntLiteralExp Int(int v)
        {
            return new IntLiteralExp(v);
        }

        #endregion

        #region BoolLiteralExp

        public BoolLiteralExp Bool(bool v)
        {
            return new BoolLiteralExp(v);
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

        #endregion LoadExp

        #endregion Exp

        #region Loc

        public LocalVarLoc LocalVar(string name)
        {
            return new LocalVarLoc(new Name.Normal(name));
        }

        public LambdaMemberVarLoc LambdaMember(LambdaMemberVarSymbol symbol)
        {
            return new LambdaMemberVarLoc(symbol);
        }
        
        public TempLoc TempLoc(Exp e, IType type)
        {
            return new TempLoc(e, type);
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
            return elems.Select(e => new FuncParameter(bOut: false, e.Type, new Name.Normal(e.Name))).ToImmutableArray();
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
            return new ExpStringExpElement(new TempLoc(exp, stringType));
        }

        public ExpStringExpElement ExpElem(Loc loc)
        {
            return new ExpStringExpElement(loc);
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

        public ListIndexerLoc ListIndexer(LocalVarLoc list, Loc index)
        {
            return new ListIndexerLoc(list, index);
        }

        // empty
        public Loc EmptyListIter(IType itemType)
        {
            var listType = listTypeConstructor.Invoke(itemType);
            var listIterType = ListIterType(itemType);

            return new TempLoc(
                new ListIteratorExp(
                    new TempLoc(new ListExp(Elems: default, itemType), listType),
                    listIterType
                ),
                listIterType
            );
        }

        public YieldStmt Yield(Exp exp)
        {
            return new YieldStmt(exp);
        }

        // 파라미터에 out표시가 없는 FuncType
        public FuncType FuncType(bool bLocal, IType retType, params IType[] paramTypes)
        {
            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParameter>(paramTypes.Length);

            int i = 0;
            foreach (var paramType in paramTypes)
            {
                var name = new Name.Anonymous(i); // 이름은 임의로 짓는다
                paramsBuilder.Add(new FuncParameter(bOut: false, paramType, name));
            }

            return new FuncType(bLocal, new FuncReturn(retType), paramsBuilder.MoveToImmutable());
        }

        public LocalPtrType LocalPtrType(IType innerType)
        {
            return new LocalPtrType(innerType);
        }

        public BoxPtrType BoxPtrType(IType innerType)
        {
            return new BoxPtrType(innerType);
        }

        public Exp LocalRef(Loc loc)
        {
            return new LocalRefExp(loc);
        }

        public Loc LocalDeref(Exp exp)
        {
            return new LocalDerefLoc(exp);
        }

        public Loc BoxDeref(Loc loc)
        {
            return new BoxDerefLoc(loc);
        }

        public Exp Box(Exp innerExp, IType innerType)
        {
            return new BoxExp(innerExp, innerType);
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
