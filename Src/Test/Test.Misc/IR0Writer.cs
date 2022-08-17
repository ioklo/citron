using Citron.Collections;
using Citron.Infra;
using Citron.IR0Visitor;
using Citron.Symbol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Citron.Module;
using R = Citron.IR0;

namespace Citron.Test.Misc
{
    //public partial class IR0Writer
    //{
    //    IndentedTextWriter itw;

    //    IR0Writer(IndentedTextWriter itw)
    //    { 
    //        this.itw = itw; 
    //    }

    //    struct StmtWriter : IIR0StmtVisitor
    //    {
    //        IR0Writer writer;            

    //        public static void Write(IR0Writer writer, R.Stmt stmt)
    //        {
    //            var stmtWriter = new StmtWriter(writer);
    //            stmtWriter.Visit(stmt);
    //        }

    //        StmtWriter(IR0Writer writer) { this.writer = writer; }

    //        public void VisitAsyncStmt(R.AsyncStmt asyncStmt)
    //        {
    //            writer.WriteNew("R.AsyncStmt", asyncStmt.Lambda, asyncStmt.CaptureArgs, asyncStmt.Body);
    //        }

    //        public void VisitAwaitStmt(R.AwaitStmt awaitStmt)
    //        {
    //            writer.WriteNew("R.AwaitStmt", awaitStmt.Body);
    //        }

    //        public void VisitBlankStmt(R.BlankStmt blankStmt)
    //        {
    //            writer.WriteString("R.BlankStmt.Instance");
    //        }

    //        public void VisitBlockStmt(R.BlockStmt blockStmt)
    //        {
    //            writer.WriteNew("R.BlockStmt", blockStmt.Stmts);
    //        }

    //        public void VisitBreakStmt(R.BreakStmt breakStmt)
    //        {
    //            writer.WriteString("R.BreakStmt.Instance");
    //        }

    //        public void VisitCommandStmt(R.CommandStmt commandStmt)
    //        {
    //            writer.WriteNew("R.CommandStmt", commandStmt.Commands);
    //        }

    //        public void VisitContinueStmt(R.ContinueStmt continueStmt)
    //        {
    //            writer.WriteString("R.ContinueStmt.Instance");
    //        }

    //        public void VisitDirectiveStmt(R.DirectiveStmt directiveStmt)
    //        {
    //            DirectiveStmtWriter.Write(writer, directiveStmt);
    //        }

    //        public void VisitExpStmt(R.ExpStmt expStmt)
    //        {
    //            writer.WriteNew("R.ExpStmt", expStmt.Exp);
    //        }

    //        public void VisitForeachStmt(R.ForeachStmt foreachStmt)
    //        {
    //            writer.WriteNew("R.ForeachStmt", foreachStmt.ElemType, foreachStmt.ElemName, foreachStmt.Iterator, foreachStmt.Body);
    //        }

    //        public void VisitForStmt(R.ForStmt forStmt)
    //        {   
    //            writer.WriteNew("R.ForStmt", forStmt.Initializer, forStmt.CondExp, forStmt.ContinueExp, forStmt.Body);
    //        }

    //        public void VisitGlobalVarDeclStmt(R.GlobalVarDeclStmt globalVarDeclStmt)
    //        {
    //            writer.WriteNew("R.GlobalVarDeclStmt", globalVarDeclStmt.Elems);
    //        }

    //        public void VisitIfStmt(R.IfStmt ifStmt)
    //        {
    //            writer.WriteNew("R.IfStmt", ifStmt.Cond, ifStmt.Body, ifStmt.ElseBody);
    //        }

    //        public void VisitIfTestClassStmt(R.IfTestClassStmt ifTestClassStmt)
    //        {
    //            writer.WriteNew("R.IfTestClassStmt", ifTestClassStmt.Target, ifTestClassStmt.TestType, ifTestClassStmt.VarName, ifTestClassStmt.Body, ifTestClassStmt.ElseBody);
    //        }

    //        public void VisitIfTestEnumElemStmt(R.IfTestEnumElemStmt ifTestEnumElemStmt)
    //        {
    //            writer.WriteNew("R.IfTestEnumElemStmt", ifTestEnumElemStmt.Target, ifTestEnumElemStmt.EnumElem, ifTestEnumElemStmt.VarName, ifTestEnumElemStmt.Body, ifTestEnumElemStmt.ElseBody);
    //        }

    //        public void VisitLocalVarDeclStmt(R.LocalVarDeclStmt localVarDeclStmt)
    //        {
    //            writer.WriteNew("R.LocalVarDeclStmt", localVarDeclStmt.VarDecl);
    //        }

    //        public void VisitReturnStmt(R.ReturnStmt returnStmt)
    //        {
    //            writer.WriteNew("R.ReturnStmt", returnStmt.Info);
    //        }

    //        public void VisitTaskStmt(R.TaskStmt taskStmt)
    //        {
    //            writer.WriteNew("R.TaskStmt", taskStmt.CapturedStatementDecl);
    //        }

    //        public void VisitYieldStmt(R.YieldStmt yieldStmt)
    //        {
    //            writer.WriteNew("R.YieldStmt", yieldStmt.Value);
    //        }
    //    }

    //    struct DirectiveStmtWriter : IIR0DirectiveStmtVisitor
    //    {
    //        IR0Writer writer;

    //        public static void Write(IR0Writer writer, R.DirectiveStmt stmt)
    //        {
    //            var dirStmtWriter = new DirectiveStmtWriter(writer);
    //            dirStmtWriter.Visit(stmt);
    //        }

    //        DirectiveStmtWriter(IR0Writer writer) { this.writer = writer; }

    //        void IIR0DirectiveStmtVisitor.VisitNull(R.DirectiveStmt.Null nullDirective)
    //        {
    //            writer.WriteNew("R.DirectiveStmt.Null", nullDirective.Loc);
    //        }

    //        void IIR0DirectiveStmtVisitor.VisitNotNull(R.DirectiveStmt.NotNull notNullDirective)
    //        {
    //            writer.WriteNew("R.DirectiveStmt.NotNull", notNullDirective.Loc);

    //        }

    //        void IIR0DirectiveStmtVisitor.VisitStaticNull(R.DirectiveStmt.StaticNull staticNullDirective)
    //        {
    //            writer.WriteNew("R.DirectiveStmt.StaticNull", staticNullDirective.Loc);
    //        }

    //        void IIR0DirectiveStmtVisitor.VisitStaticNotNull(R.DirectiveStmt.StaticNotNull staticNotNullDirective)
    //        {
    //            writer.WriteNew("R.DirectiveStmt.StaticNotNull", staticNotNullDirective.Loc);
    //        }

    //        void IIR0DirectiveStmtVisitor.VisitStaticUnknownNull(R.DirectiveStmt.StaticUnknownNull staticUnknownNullDirective)
    //        {
    //            writer.WriteNew("R.DirectiveStmt.StaticUnknownNull", staticUnknownNullDirective.Loc);
    //        }
    //    }

    //    struct ExpWriter : IIR0ExpVisitor
    //    {
    //        IR0Writer writer;
    //        public static void Write(IR0Writer writer, R.Exp exp)
    //        {
    //            var expWriter = new ExpWriter(writer);
    //            expWriter.Visit(exp);
    //        }

    //        ExpWriter(IR0Writer writer) { this.writer = writer; }

    //        public void VisitAssignExp(R.AssignExp assignExp)
    //        {
    //            writer.WriteNew("R.AssignExp", assignExp.Dest, assignExp.Src);
    //        }

    //        public void VisitBoolLiteralExp(R.BoolLiteralExp boolExp)
    //        {
    //            writer.WriteNew("R.BoolLiteralExp", boolExp.Value);
    //        }

    //        public void VisitCallFuncExp(R.CallFuncExp callFuncExp)
    //        {
    //            writer.WriteNew("R.CallFuncExp", callFuncExp.Func, callFuncExp.Instance, callFuncExp.Args);
    //        }

    //        public void VisitCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp)
    //        {
    //            writer.WriteNew("R.CallInternalBinaryOperatorExp", ciboExp.Operator, ciboExp.Operand0, ciboExp.Operand1);
    //        }

    //        public void VisitCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperatorExp ciuaoExp)
    //        {
    //            writer.WriteNew("R.CallInternalUnaryAssignOperatorExp", ciuaoExp.Operator, ciuaoExp.Operand);
    //        }

    //        public void VisitCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp)
    //        {
    //            writer.WriteNew("R.CallInternalUnaryOperatorExp", ciuoExp.Operator, ciuoExp.Operand);
    //        }

    //        public void VisitCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp)
    //        {
    //            writer.WriteNew("R.CallSeqFuncExp", callSeqFuncExp.SeqFunc, callSeqFuncExp.Instance, callSeqFuncExp.Args);
    //        }

    //        public void VisitCallValueExp(R.CallValueExp callValueExp)
    //        {
    //            writer.WriteNew("R.CallValueExp", callValueExp.Lambda, callValueExp.Callable, callValueExp.Args);
    //        }

    //        public void VisitCastClassExp(R.CastClassExp castClassExp)
    //        {
    //            writer.WriteNew("R.CastClassExp", castClassExp.Src, castClassExp.Class);
    //        }

    //        public void VisitCastEnumElemToEnumExp(R.CastEnumElemToEnumExp exp)
    //        {
    //            writer.WriteNew("R.CastEnumElemToEnumExp", exp.Src, exp.EnumElem);
    //        }

    //        public void VisitIntLiteralExp(R.IntLiteralExp intExp)
    //        {
    //            writer.WriteNew("R.IntLiteralExp", intExp.Value);
    //        }

    //        public void VisitLambdaExp(R.LambdaExp lambdaExp)
    //        {
    //            writer.WriteNew("R.LambdaExp", lambdaExp.Lambda);
    //        }

    //        public void VisitListExp(R.ListExp listExp)
    //        {
    //            writer.WriteNew("R.ListExp", listExp.ElemType, listExp.Elems);
    //        }

    //        public void VisitListIteratorExp(R.ListIteratorExp exp)
    //        {
    //            writer.WriteNew("R.ListIteratorExp", exp.ListLoc);
    //        }

    //        public void VisitLoadExp(R.LoadExp loadExp)
    //        {
    //            writer.WriteNew("R.LoadExp", loadExp.Loc);
    //        }

    //        public void VisitNewClassExp(R.NewClassExp newClassExp)
    //        {
    //            writer.WriteNew("R.NewClassExp", newClassExp.Class, newClassExp.ConstructorParamHash, newClassExp.Args);
    //        }

    //        public void VisitNewEnumElemExp(R.NewEnumElemExp exp)
    //        {
    //            writer.WriteNew("R.NewEnumElemExp", exp.Elem, exp.Args);
    //        }

    //        public void VisitNewStructExp(R.NewStructExp exp)
    //        {
    //            writer.WriteNew("R.NewStructExp", exp.Constructor, exp.Args);
    //        }

    //        public void VisitStringExp(R.StringExp stringExp)
    //        {
    //            writer.WriteNew("R.StringExp", stringExp.Elements);
    //        }

    //        public void VisitNewNullableExp(R.NewNullableExp newNullableExp)
    //        {
    //            writer.WriteNew("R.NewNullableExp", newNullableExp.InnerType, newNullableExp.ValueExp);
    //        }
    //    }

    //    struct LocWriter : IIR0LocVisitor
    //    {
    //        IR0Writer writer;

    //        public static void Write(IR0Writer writer, R.Loc loc)
    //        {
    //            var locWriter = new LocWriter(writer);
    //            locWriter.Visit(loc);
    //        }

    //        public void VisitTempLoc(R.TempLoc loc)
    //        {
    //            writer.WriteNew("R.TempLoc", loc.Exp, loc.Type);
    //        }

    //        public void VisitGlobalVarLoc(R.GlobalVarLoc loc)
    //        {
    //            writer.WriteNew("R.GlobalVarLoc", loc.Name);
    //        }

    //        public void VisitLocalVarLoc(R.LocalVarLoc loc)
    //        {
    //            writer.WriteNew("R.LocalVarLoc", loc.Name);
    //        }

    //        public void VisitLambdaMemberVarLoc(R.LambdaMemberVar loc)
    //        {
    //            writer.WriteNew("R.LambdaMemberVarLoc", loc.Name);
    //        }

    //        public void VisitListIndexerLoc(R.ListIndexerLoc loc)
    //        {
    //            writer.WriteNew("R.ListIndexerLoc", loc.List, loc.Index);
    //        }

    //        public void VisitStaticMemberLoc(R.StaticMemberLoc loc)
    //        {
    //            writer.WriteNew("R.StaticMemberLoc", loc.MemberPath);
    //        }

    //        public void VisitStructMemberLoc(R.StructMemberLoc loc)
    //        {
    //            writer.WriteNew("R.StructMemberLoc", loc.Instance, loc.MemberPath);
    //        }

    //        public void VisitClassMemberLoc(R.ClassMemberLoc loc)
    //        {
    //            writer.WriteNew("R.ClassMemberLoc", loc.Instance, loc.MemberPath);
    //        }

    //        public void VisitEnumElemMemberLoc(R.EnumElemMemberLoc loc)
    //        {
    //            writer.WriteNew("R.EnumElemMemberLoc", loc.Instance, loc.EnumElemField);
    //        }

    //        public void VisitThisLoc(R.ThisLoc loc)
    //        {
    //            writer.WriteString("R.ThisLoc.Instance");
    //        }

    //        public void VisitDerefLocLoc(R.DerefLocLoc loc)
    //        {
    //            writer.WriteNew("R.DerefLocLoc", loc.Loc);
    //        }

    //        public void VisitDerefExpLoc(R.DerefExpLoc loc)
    //        {
    //            writer.WriteNew("R.DerefExpLoc", loc.Exp);
    //        }

    //        LocWriter(IR0Writer writer)
    //        {
    //            this.writer = writer;
    //        }


    //    }

    //    void WriteName(M.Name name)
    //    {
    //        switch(name)
    //        {                
    //            case M.Name.Singleton singleton: WriteNew($"M.Name.{singleton.DebugText}"); break;

    //            // anonymous type names        
    //            case M.Name.Anonymous a: WriteNew("R.Name.Anonymous", a.Index); break;
    //            case M.Name.ConstructorParam cp: WriteNew("R.Name.ConstructorParam", cp.Index); break;
    //            case M.Name.Normal normal: WriteNew("M.Name.Normal", normal.Text); break;
    //        }
    //    }

    //    void WriteFuncParamId(M.FuncParamId funcParamId)
    //    {
    //        WriteNew("M.FuncParamId", funcParamId.Kind, funcParamId.TypeId);
    //    }

    //    void WriteFuncParam(M.Param param)
    //    {
    //        WriteNew("M.Param", entry.Kind, entry.Type);
    //    }

    //    void WriteFuncParameterKind(M.FuncParameterKind kind)
    //    {
    //        switch (kind)
    //        {
    //            case M.FuncParameterKind.Default: itw.Write("M.FuncParameterKind.Normal"); break;
    //            case M.FuncParameterKind.Params: itw.Write("M.FuncParameterKind.Params"); break;
    //            case M.FuncParameterKind.Ref: itw.Write("M.FuncParameterKindRef"); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteAccessModifier(R.AccessModifier modifier)
    //    {
    //        switch(modifier)
    //        {
    //            case R.AccessModifier.Public: itw.Write("R.AccessModifier.Public"); break;
    //            case R.AccessModifier.Protected: itw.Write("R.AccessModifier.Protected"); break;
    //            case R.AccessModifier.Private: itw.Write("R.AccessModifier.Private"); break;
    //        }
    //    }

    //    void WritePath(R.Path path)
    //    {
    //        switch(path)
    //        {
    //            // R.Path.Reserved
    //            case R.Path.TupleType tuple: WriteNew("R.Path.TupleType", tuple.Elems); break;
    //            case R.Path.TypeVarType typeVar: WriteNew("R.Path.TypeVarType", typeVar.Index); break;
    //            case R.Path.VoidType: itw.Write("R.Path.VoidType.Instance"); break;
    //            case R.Path.BoxType box: WriteNew("R.Path.BoxType", box.Type); break;
    //            case R.Path.GenericRefType genericRef: WriteNew("R.Path.GenericRefType", genericRef.Type); break;
    //            case R.Path.FuncType: WriteNew("R.Path.FuncType"); break; // TODO:
    //            case R.Path.NullableType nullable: WriteNew("R.Path.NullableType", nullable.Type); break;

    //            // special cases R.Path.Normal
    //            case R.Path.Normal when path == R.Path.System: itw.Write("R.Path.System"); break;
    //            case R.Path.Normal when path == R.Path.Bool: itw.Write("R.Path.Bool"); break;
    //            case R.Path.Normal when path == R.Path.Int: itw.Write("R.Path.Int"); break;
    //            case R.Path.Normal when path == R.Path.String: itw.Write("R.Path.String"); break;

    //            // todo: seq, list
    //            case R.Path.Root root: WriteNew("R.Path.Root", root.ModuleName); break;
    //            case R.Path.Nested nested: WriteNew("R.Path.Nested", nested.Outer, nested.Name, nested.ParamHash, nested.TypeArgs); break;

    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteTupleTypeElem(R.TupleTypeElem tte)
    //    {
    //        WriteNew("R.TupleTypeElem", tte.Type, tte.Name);
    //    }

    //    void WriteParam(R.Param param)
    //    {
    //        WriteNew("R.Param", param.Kind, param.Type, param.Name);
    //    }

    //    void WriteStructConstructorDecl(R.StructConstructorDecl scd)
    //    {
    //        WriteNew("R.StructConstructorDecl", scd.AccessModifier, scd.Parameters, scd.Body);
    //    }

    //    void WriteStructMemberVarDecl(R.StructMemberVarDecl memberVar)
    //    {
    //        WriteNew("R.StructMemberVarDecl", memberVar.AccessModifier, memberVar.Type, memberVar.Names);
    //    }

    //    void WriteConstructorBaseCallinfo(R.ConstructorBaseCallInfo constructorBaseCallInfo)
    //    {
    //        WriteNew("R.ConstructorBaseCallInfo", constructorBaseCallInfo.ParamHash, constructorBaseCallInfo.Args);
    //    }

    //    void WriteClassConstructorDecl(R.ClassConstructorDecl constructorDecl)
    //    {
    //        WriteNew("R.ClassConstructorDecl", constructorDecl.AccessModifier, constructorDecl.CallableMemberDecls, constructorDecl.Parameters, constructorDecl.BaseCallInfo);
    //    }

    //    void WriteClassMemberVarDecl(R.ClassMemberVarDecl memberVarDecl)
    //    {
    //        WriteNew("R.ClassMemberVarDecl", memberVarDecl.AccessModifier, memberVarDecl.Type, memberVarDecl.Names);
    //    }

    //    void WriteEnumElement(R.EnumElement elem)
    //    {
    //        WriteNew("R.EnumElement", elem.Name, elem.Fields);
    //    }

    //    void WriteEnumElementField(R.EnumElementField field)
    //    {
    //        WriteNew("R.EnumElementField", field.Type, field.Name);
    //    }

    //    void WriteCapturedStatement(R.CapturedStatement cs)
    //    {
    //        WriteNew("R.CapturedStatement", cs.ThisType, cs.OuterLocalVars, cs.Body);
    //    }

    //    void WriteOuterLocalVarInfo(R.OuterLocalVarInfo info)
    //    {
    //        WriteNew("R.OuterLocalVarInfo", info.Type, info.Name);
    //    }

    //    // TODO: WriteForStmtInitializer를 통해서 들어오고, WriteObject에서 빠져있다
    //    // WriteForStmtInitializer를 없애고 WriteObject에서 직접 들어오게 해야하나
    //    void WriteExpForStmtInitializer(R.ExpForStmtInitializer expInit)
    //    {
    //        WriteNew("R.ExpForStmtInitializer", expInit.Exp);
    //    }

    //    void WriteVarDeclForStmtInitializer(R.VarDeclForStmtInitializer varInit)
    //    {
    //        WriteNew("R.VarDeclForStmtInitializer", varInit.VarDecl);
    //    }

    //    void WriteForStmtInitializer(R.ForStmtInitializer init)
    //    {
    //        switch(init)
    //        {
    //            case R.ExpForStmtInitializer expInit: WriteExpForStmtInitializer(expInit); break;
    //            case R.VarDeclForStmtInitializer varDeclInit: WriteVarDeclForStmtInitializer(varDeclInit); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteLocalVarDecl(R.LocalVarDecl decl)
    //    {
    //        WriteNew("R.LocalVarDecl", decl.Elems);
    //    }

    //    void WriteNormalVarDeclElement(R.VarDeclElement.Normal elem)
    //    {
    //        WriteNew("R.VarDeclElement.Normal", elem.Type, elem.Name, elem.InitExp);
    //    }   

    //    void WriteNormalDefaultVarDeclElement(R.VarDeclElement.NormalDefault elem)
    //    {
    //        WriteNew("R.VarDeclElement.NormalDefault", elem.Type, elem.Name);
    //    }

    //    void WriteRefVarDeclElement(R.VarDeclElement.Ref elem)
    //    {
    //        WriteNew("R.VarDeclElement.Ref", elem.Name, elem.Loc);
    //    }

    //    // 여기도 위의 세개 WriteNormalVarDeclElement, WriteNormalDefault... 는 WriteObject에 포함시키지 않았다
    //    void WriteVarDeclElement(R.VarDeclElement elem)
    //    {
    //        switch(elem)
    //        {
    //            case R.VarDeclElement.Normal normalElem: WriteNormalVarDeclElement(normalElem); break;
    //            case R.VarDeclElement.NormalDefault normalDefaultElem: WriteNormalDefaultVarDeclElement(normalDefaultElem); break;
    //            case R.VarDeclElement.Ref refElem: WriteRefVarDeclElement(refElem); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteRefReturnInfo(R.ReturnInfo.Ref refInfo)
    //    {
    //        WriteNew("R.ReturnInfo.Ref", refInfo.Loc);
    //    }

    //    void WriteExpReturnInfo(R.ReturnInfo.Expression expInfo)
    //    {
    //        WriteNew("R.ReturnInfo.Exp", expInfo.Exp);
    //    }

    //    void WriteNoneReturnInfo(R.ReturnInfo.None noneInfo)
    //    {
    //        WriteString("R.ReturnInfo.None.Instance");
    //    }

    //    void WriteReturnInfo(R.ReturnInfo info)
    //    {
    //        switch(info)
    //        {
    //            case R.ReturnInfo.Ref refInfo: WriteRefReturnInfo(refInfo); break;
    //            case R.ReturnInfo.Expression expInfo: WriteExpReturnInfo(expInfo); break;
    //            case R.ReturnInfo.None noneInfo: WriteNoneReturnInfo(noneInfo); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteNormalArgument(R.Argument.Normal normalArg)
    //    {
    //        WriteNew("R.Argument.Normal", normalArg.Exp);
    //    }

    //    void WriteParamsArgument(R.Argument.Params paramsArg)
    //    {WriteNew("R.Argument.Params", paramsArg.Exp, paramsArg.ElemCount);
    //    }

    //    void WriteRefArgument(R.Argument.Ref refArg)
    //    {
    //        WriteNew("R.Argument.Ref", refArg.Loc);
    //    }

    //    void WriteArgument(R.Argument arg)
    //    {
    //        switch(arg)
    //        {
    //            case R.Argument.Normal normalArg: WriteNormalArgument(normalArg); break;
    //            case R.Argument.Params paramsArg: WriteParamsArgument(paramsArg); break;
    //            case R.Argument.Ref refArg: WriteRefArgument(refArg); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteInternalBinaryOperator(R.InternalBinaryOperator op)
    //    {
    //        switch(op)
    //        {
    //            case R.InternalBinaryOperator.Multiply_Int_Int_Int: itw.Write("R.InternalBinaryOperator.Multiply_Int_Int_Int"); break;
    //            case R.InternalBinaryOperator.Divide_Int_Int_Int: itw.Write("R.InternalBinaryOperator.Divide_Int_Int_Int"); break;
    //            case R.InternalBinaryOperator.Modulo_Int_Int_Int: itw.Write("R.InternalBinaryOperator.Modulo_Int_Int_Int"); break;
    //            case R.InternalBinaryOperator.Add_Int_Int_Int: itw.Write("R.InternalBinaryOperator.Add_Int_Int_Int"); break;
    //            case R.InternalBinaryOperator.Add_String_String_String: itw.Write("R.InternalBinaryOperator.Add_String_String_String"); break;
    //            case R.InternalBinaryOperator.Subtract_Int_Int_Int: itw.Write("R.InternalBinaryOperator.Subtract_Int_Int_Int"); break;
    //            case R.InternalBinaryOperator.LessThan_Int_Int_Bool: itw.Write("R.InternalBinaryOperator.LessThan_Int_Int_Bool"); break;
    //            case R.InternalBinaryOperator.LessThan_String_String_Bool: itw.Write("R.InternalBinaryOperator.LessThan_String_String_Bool"); break;
    //            case R.InternalBinaryOperator.GreaterThan_Int_Int_Bool: itw.Write("R.InternalBinaryOperator.GreaterThan_Int_Int_Bool"); break;
    //            case R.InternalBinaryOperator.GreaterThan_String_String_Bool: itw.Write("R.InternalBinaryOperator.GreaterThan_String_String_Bool"); break;
    //            case R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool: itw.Write("R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool"); break;
    //            case R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool: itw.Write("R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool"); break;
    //            case R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool: itw.Write("R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool"); break;
    //            case R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool: itw.Write("R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool"); break;
    //            case R.InternalBinaryOperator.Equal_Int_Int_Bool: itw.Write("R.InternalBinaryOperator.Equal_Int_Int_Bool"); break;
    //            case R.InternalBinaryOperator.Equal_Bool_Bool_Bool: itw.Write("R.InternalBinaryOperator.Equal_Bool_Bool_Bool"); break;
    //            case R.InternalBinaryOperator.Equal_String_String_Bool: itw.Write("R.InternalBinaryOperator.Equal_String_String_Bool"); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteInternalUnaryAssignOperator(R.InternalUnaryAssignOperator op)
    //    {
    //        switch (op)
    //        {
    //            case R.InternalUnaryAssignOperator.PrefixInc_Int_Int: itw.Write("R.InternalUnaryAssignOperator.PrefixInc_Int_Int"); break;
    //            case R.InternalUnaryAssignOperator.PrefixDec_Int_Int: itw.Write("R.InternalUnaryAssignOperator.PrefixDec_Int_Int"); break;
    //            case R.InternalUnaryAssignOperator.PostfixInc_Int_Int: itw.Write("R.InternalUnaryAssignOperator.PostfixInc_Int_Int"); break;
    //            case R.InternalUnaryAssignOperator.PostfixDec_Int_Int: itw.Write("R.InternalUnaryAssignOperator.PostfixDec_Int_Int"); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteInternalUnaryOperator(R.InternalUnaryOperator op)
    //    {
    //        switch(op)
    //        {
    //            case R.InternalUnaryOperator.LogicalNot_Bool_Bool: itw.Write("R.InternalUnaryOperator.LogicalNot_Bool_Bool"); break;
    //            case R.InternalUnaryOperator.UnaryMinus_Int_Int: itw.Write("R.InternalUnaryOperator.UnaryMinus_Int_Int"); break;
    //            case R.InternalUnaryOperator.ToString_Bool_String: itw.Write("R.InternalUnaryOperator.ToString_Bool_String"); break;
    //            case R.InternalUnaryOperator.ToString_Int_String: itw.Write("R.InternalUnaryOperator.ToString_Int_String"); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteExpStringExpElement(R.ExpStringExpElement elem)
    //    {
    //        WriteNew("R.ExpStringExpElement", elem.Exp);
    //    }

    //    void WriteTextStringExpElement(R.TextStringExpElement elem)
    //    {
    //        WriteNew("R.TextStringExpElement", elem.Text);
    //    }

    //    void WriteStringExpElement(R.StringExpElement elem)
    //    {
    //        switch(elem)
    //        {
    //            case R.ExpStringExpElement expElem: WriteExpStringExpElement(expElem); break;
    //            case R.TextStringExpElement textElem: WriteTextStringExpElement(textElem); break;
    //            default: throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteTypeDecl(R.TypeDecl typeDecl)
    //    {
    //        TypeDeclWriter.Write(itw, typeDecl);
    //    }

    //    void WriteFuncDecl(R.FuncDecl funcDecl)
    //    {
    //        FuncDeclWriter.Write(itw, funcDecl);
    //    }

    //    void WriteCallableMemberDecl(R.CallableMemberDecl callableMemberDecl)
    //    {
    //        CallableMemberDeclWriter.Write(itw, callableMemberDecl);
    //    }

    //    void WriteStmt(R.Stmt stmt)
    //    {
    //        StmtWriter.Write(this, stmt);
    //    }

    //    void WriteExp(R.Exp exp)
    //    {
    //        ExpWriter.Write(this, exp);
    //    }

    //    void WriteLoc(R.Loc loc)
    //    {
    //        LocWriter.Write(this, loc);
    //    }

    //    void WriteString(string text)
    //    {
    //        itw.Write(text);
    //    }

    //    void WriteBool(bool v)
    //    {
    //        itw.Write(v);
    //    }

    //    void WriteInt(int v)
    //    {
    //        itw.Write(v);
    //    }

    //    void WriteArray<TItem>(Action<TItem> itemWriter, string? itemType, ImmutableArray<TItem> items)
    //    {
    //        if (items.Length == 0)
    //        {
    //            itw.Write("default");
    //            return;
    //        }

    //        if (items.Length == 1)
    //        {
    //            if (itemType != null)
    //                itw.Write($"Arr<{itemType}>(");
    //            else
    //                itw.Write($"Arr(");

    //            itemWriter.Invoke(items[0]);
    //            itw.Write(")");
    //            return;
    //        }

    //        // Arr<>(
    //        if (itemType != null)
    //            itw.Write($"Arr<{itemType}>(");
    //        else
    //            itw.Write($"Arr(");

    //        var itw1 = itw.Push();

    //        bool bFirst = true;
    //        foreach (var item in items)
    //        {
    //            if (!bFirst) itw1.WriteLine(",");
    //            else bFirst = false;

    //            itemWriter.Invoke(item);
    //        }

    //        itw.Write(")");
    //    }

    //    void WriteObject(object? obj)
    //    {
    //        switch(obj)
    //        {
    //            case null: itw.Write("null"); break;

    //            case M.Name name: WriteName(name); break;
    //            case ImmutableArray<M.Name> names: WriteArray(WriteName, "R.Name", names); break;

    //            case M.FuncParamId funcParamId: WriteFuncParamId(funcParamId); break;
    //            case ImmutableArray<M.FuncParamId> funcParamIds: WriteArray(WriteFuncParamId, null, funcParamIds); break;

    //            case R.ParamHashEntry phe: WriteParamHashEntry(phe); break;
    //            case ImmutableArray<R.ParamHashEntry> phes: WriteArray(WriteParamHashEntry, null, phes); break;

    //            case M.FuncParameterKind kind: WriteFuncParameterKind(kind); break;
    //            case ImmutableArray<M.FuncParameterKind> kinds: WriteArray(kind, null, paramKinds); break;

    //            case R.AccessModifier am: WriteAccessModifier(am); break;
    //            case ImmutableArray<R.AccessModifier> ams: WriteArray(WriteAccessModifier, null, ams); break;

    //            case R.Path path: WritePath(path); break;
    //            case ImmutableArray<R.Path> paths: WriteArray(WritePath, "R.Path", paths); break;

    //            // TODO: special case
    //            case ImmutableArray<R.Path.Nested> pns: WriteArray(WritePath, "R.Path.Nested", pns); break;

    //            case R.TupleTypeElem tte: WriteTupleTypeElem(tte); break;
    //            case ImmutableArray<R.TupleTypeElem> ttes: WriteArray(WriteTupleTypeElem, null, ttes);  break;

    //            case R.Param param: WriteParam(param); break;
    //            case ImmutableArray<R.Param> parameters: WriteArray(WriteParam, null, parameters); break;

    //            case R.StructConstructorDecl scd: WriteStructConstructorDecl(scd); break;
    //            case ImmutableArray<R.StructConstructorDecl> scds: WriteArray(WriteStructConstructorDecl, null, scds); break;

    //            case R.StructMemberVarDecl smvd: WriteStructMemberVarDecl(smvd); break;
    //            case ImmutableArray<R.StructMemberVarDecl> smvds: WriteArray(WriteStructMemberVarDecl, null, smvds); break;

    //            case R.ConstructorBaseCallInfo cbci: WriteConstructorBaseCallinfo(cbci); break;
    //            case ImmutableArray<R.ConstructorBaseCallInfo> cbcis: WriteArray(WriteConstructorBaseCallinfo, null, cbcis); break;

    //            case R.ClassConstructorDecl ccd: WriteClassConstructorDecl(ccd); break;
    //            case ImmutableArray<R.ClassConstructorDecl> ccds: WriteArray(WriteClassConstructorDecl, null, ccds); break;

    //            case R.ClassMemberVarDecl cmvd: WriteClassMemberVarDecl(cmvd); break;
    //            case ImmutableArray<R.ClassMemberVarDecl> cmvds: WriteArray(WriteClassMemberVarDecl, null, cmvds); break;

    //            case R.EnumElement ee: WriteEnumElement(ee); break;
    //            case ImmutableArray<R.EnumElement> ees: WriteArray(WriteEnumElement, null, ees); break;

    //            case R.EnumElementField eef: WriteEnumElementField(eef); break;
    //            case ImmutableArray<R.EnumElementField> eefs: WriteArray(WriteEnumElementField, null, eefs); break;

    //            case R.CapturedStatement cs: WriteCapturedStatement(cs); break;
    //            case ImmutableArray<R.CapturedStatement> css: WriteArray(WriteCapturedStatement, null, css); break;

    //            case R.OuterLocalVarInfo olvi: WriteOuterLocalVarInfo(olvi); break;
    //            case ImmutableArray<R.OuterLocalVarInfo> olvis: WriteArray(WriteOuterLocalVarInfo, null, olvis); break;

    //            case R.ForStmtInitializer fsi: WriteForStmtInitializer(fsi); break;
    //            case ImmutableArray<R.ForStmtInitializer> fsis: WriteArray(WriteForStmtInitializer, "R.ForStmtInitializer", fsis); break;

    //            case R.LocalVarDecl lvd: WriteLocalVarDecl(lvd); break;
    //            case ImmutableArray<R.LocalVarDecl> lvds: WriteArray(WriteLocalVarDecl, null, lvds); break;

    //            case R.VarDeclElement vde: WriteVarDeclElement(vde); break;
    //            case ImmutableArray<R.VarDeclElement> vdes: WriteArray(WriteVarDeclElement, "R.VarDeclElement", vdes); break;

    //            case R.ReturnInfo ri: WriteReturnInfo(ri); break;
    //            case ImmutableArray<R.ReturnInfo> ris: WriteArray(WriteReturnInfo, "R.ReturnInfo", ris); break;

    //            case R.Argument arg: WriteArgument(arg); break;
    //            case ImmutableArray<R.Argument> args: WriteArray(WriteArgument, "R.Argument", args); break;

    //            case R.InternalBinaryOperator ibo: WriteInternalBinaryOperator(ibo); break;
    //            case ImmutableArray<R.InternalBinaryOperator> ibos: WriteArray(WriteInternalBinaryOperator, null, ibos); break;

    //            case R.InternalUnaryAssignOperator iuao: WriteInternalUnaryAssignOperator(iuao); break;
    //            case ImmutableArray<R.InternalUnaryAssignOperator> iuaos: WriteArray(WriteInternalUnaryAssignOperator, null, iuaos); break;

    //            case R.InternalUnaryOperator iuo: WriteInternalUnaryOperator(iuo); break;
    //            case ImmutableArray<R.InternalUnaryOperator> iuos: WriteArray(WriteInternalUnaryOperator, null, iuos); break;

    //            case R.StringExpElement see: WriteStringExpElement(see); break;
    //            case ImmutableArray<R.StringExpElement> sees: WriteArray(WriteStringExpElement, "R.StringExpElement", sees); break;

    //            //////

    //            case R.TypeDecl td: WriteTypeDecl(td); break;
    //            case ImmutableArray<R.TypeDecl> tds: WriteArray(WriteTypeDecl, "R.TypeDecl", tds); break;

    //            case R.FuncDecl fd: WriteFuncDecl(fd); break;
    //            case ImmutableArray<R.FuncDecl> fds: WriteArray(WriteFuncDecl, "R.FuncDecl", fds); break;

    //            case R.CallableMemberDecl cmd: WriteCallableMemberDecl(cmd); break;
    //            case ImmutableArray<R.CallableMemberDecl> cmds: WriteArray(WriteCallableMemberDecl, "R.CallableMemberDecl", cmds); break;

    //            case R.Stmt stmt: WriteStmt(stmt); break;
    //            case ImmutableArray<R.Stmt> stmts: WriteArray(WriteStmt, "R.Stmt", stmts); break;

    //            case R.Exp exp: WriteExp(exp); break;
    //            case ImmutableArray<R.Exp> exps: WriteArray(WriteExp, "R.Exp", exps); break;

    //            // TODO: special cases
    //            case ImmutableArray<R.StringExp> stringExps: WriteArray(WriteExp, null, stringExps); break;

    //            case R.Loc loc: WriteLoc(loc); break;
    //            case ImmutableArray<R.Loc> locs: WriteArray(WriteLoc, "R.Loc", locs); break;

    //            case string s: WriteString(s); break;
    //            case ImmutableArray<string> strs: WriteArray(WriteString, null, strs); break;

    //            case bool b: WriteBool(b); break;
    //            case ImmutableArray<bool> bs: WriteArray(WriteBool, null, bs); break;

    //            case int i: WriteInt(i); break;
    //            case ImmutableArray<int> ints: WriteArray(WriteInt, null, ints); break;

    //            default:
    //                throw new UnreachableCodeException();
    //        }
    //    }

    //    void WriteNew(string typeName, params object?[] objs)
    //    {
    //        itw.Write($"new {typeName}(");

    //        if (objs.Length == 0)
    //        {
    //            itw.Write(")");
    //            return;
    //        }
    //        else if (objs.Length == 1)
    //        {
    //            WriteObject(objs[0]);
    //            itw.Write(")");
    //            return;
    //        }
    //        else
    //        {
    //            var itw1 = itw.Push();
    //            var writer1 = new IR0Writer(itw1);

    //            bool bFirst = true;
    //            foreach (var obj in objs)
    //            {
    //                if (bFirst) { bFirst = false; itw1.WriteLine(); }
    //                else itw1.WriteLine(",");

    //                writer1.WriteObject(obj);
    //            }

    //            itw.WriteLine();
    //            itw.Write(")");
    //        }
    //    }

    //    public static string ToString(R.Script script)
    //    {
    //        using (var stringWriter = new StringWriter())
    //        {
    //            WriteScript(stringWriter, script);
    //            return stringWriter.ToString();
    //        }
    //    }

    //    public static void WriteScript(TextWriter sw, R.Script script)
    //    {
    //        var itw = new IndentedTextWriter(sw);
    //        var writer = new IR0Writer(itw);

    //        writer.WriteNew(
    //            "R.Script", 
    //            script.GlobalTypeDecls, 
    //            script.GlobalFuncDecls,
    //            script.CallableMemberDecls,
    //            script.TopLevelStmts
    //        );
    //    }
    //}

    public partial struct IR0Writer
    {
        IndentedTextWriter itw;
        public IR0Writer(IndentedTextWriter itw)
        {
            this.itw = itw;
        }

        public void Write_String(string? s)
        {
            if (s == null) { itw.Write("null"); return; }
            itw.Write(s);
        }

        public void Write_Int32(int i)
        {
            itw.Write(i);
        }

        public void Write_Boolean(bool b)
        {
            itw.Write(b);
        }

        public void Write_ISymbolNode(ISymbolNode node)
        {
            throw new NotImplementedException();
        }

        void Write_ImmutableArray<TItem>(Action<TItem> itemWriter, string? itemType, ImmutableArray<TItem> items)
        {
            if (items.Length == 0)
            {
                itw.Write("default");
                return;
            }

            if (items.Length == 1)
            {
                if (itemType != null)
                    itw.Write($"Arr<{itemType}>(");
                else
                    itw.Write($"Arr(");

                itemWriter.Invoke(items[0]);
                itw.Write(")");
                return;
            }

            // Arr<>(
            if (itemType != null)
                itw.Write($"Arr<{itemType}>(");
            else
                itw.Write($"Arr(");

            var itw1 = itw.Push();

            bool bFirst = true;
            foreach (var item in items)
            {
                if (!bFirst) itw1.WriteLine(",");
                else bFirst = false;

                itemWriter.Invoke(item);
            }

            itw.Write(")");
        }
    }
}
