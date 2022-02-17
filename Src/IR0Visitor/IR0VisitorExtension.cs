using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Citron.IR0;

namespace Citron.IR0Visitor
{
    public static class IR0VisitorExtension
    {
        public static void Visit<TVisitor>(this ref TVisitor visitor, R.Stmt stmt)
            where TVisitor : struct, IIR0StmtVisitor
        {
            switch (stmt)
            {
                case R.CommandStmt commandStmt: visitor.VisitCommandStmt(commandStmt); break;
                case R.GlobalVarDeclStmt globalVarDeclStmt: visitor.VisitGlobalVarDeclStmt(globalVarDeclStmt); break;
                case R.LocalVarDeclStmt localVarDeclStmt: visitor.VisitLocalVarDeclStmt(localVarDeclStmt); break;
                case R.IfStmt ifStmt: visitor.VisitIfStmt(ifStmt); break;
                case R.IfTestClassStmt ifTestClassStmt: visitor.VisitIfTestClassStmt(ifTestClassStmt); break;
                case R.IfTestEnumElemStmt ifTestEnumElemStmt: visitor.VisitIfTestEnumElemStmt(ifTestEnumElemStmt); break;
                case R.ForStmt forStmt: visitor.VisitForStmt(forStmt); break;
                case R.ContinueStmt continueStmt: visitor.VisitContinueStmt(continueStmt); break;
                case R.BreakStmt breakStmt: visitor.VisitBreakStmt(breakStmt); break;
                case R.ReturnStmt returnStmt: visitor.VisitReturnStmt(returnStmt); break;
                case R.BlockStmt blockStmt: visitor.VisitBlockStmt(blockStmt); break;
                case R.BlankStmt blankStmt: visitor.VisitBlankStmt(blankStmt); break;
                case R.ExpStmt expStmt: visitor.VisitExpStmt(expStmt); break;
                case R.TaskStmt taskStmt: visitor.VisitTaskStmt(taskStmt); break;
                case R.AwaitStmt awaitStmt: visitor.VisitAwaitStmt(awaitStmt); break;
                case R.AsyncStmt asyncStmt: visitor.VisitAsyncStmt(asyncStmt); break;
                case R.ForeachStmt foreachStmt: visitor.VisitForeachStmt(foreachStmt); break;
                case R.YieldStmt yieldStmt: visitor.VisitYieldStmt(yieldStmt); break;
                case R.DirectiveStmt yieldStmt: visitor.VisitDirectiveStmt(yieldStmt); break;
                default: throw new UnreachableCodeException();
            }
        }

        public static void Visit<TVisitor>(this TVisitor visitor, R.Stmt stmt)
            where TVisitor : class, IIR0StmtVisitor
        {
            switch (stmt)
            {
                case R.CommandStmt commandStmt: visitor.VisitCommandStmt(commandStmt); break;
                case R.GlobalVarDeclStmt globalVarDeclStmt: visitor.VisitGlobalVarDeclStmt(globalVarDeclStmt); break;
                case R.LocalVarDeclStmt localVarDeclStmt: visitor.VisitLocalVarDeclStmt(localVarDeclStmt); break;
                case R.IfStmt ifStmt: visitor.VisitIfStmt(ifStmt); break;
                case R.IfTestClassStmt ifTestClassStmt: visitor.VisitIfTestClassStmt(ifTestClassStmt); break;
                case R.IfTestEnumElemStmt ifTestEnumElemStmt: visitor.VisitIfTestEnumElemStmt(ifTestEnumElemStmt); break;
                case R.ForStmt forStmt: visitor.VisitForStmt(forStmt); break;
                case R.ContinueStmt continueStmt: visitor.VisitContinueStmt(continueStmt); break;
                case R.BreakStmt breakStmt: visitor.VisitBreakStmt(breakStmt); break;
                case R.ReturnStmt returnStmt: visitor.VisitReturnStmt(returnStmt); break;
                case R.BlockStmt blockStmt: visitor.VisitBlockStmt(blockStmt); break;
                case R.BlankStmt blankStmt: visitor.VisitBlankStmt(blankStmt); break;
                case R.ExpStmt expStmt: visitor.VisitExpStmt(expStmt); break;
                case R.TaskStmt taskStmt: visitor.VisitTaskStmt(taskStmt); break;
                case R.AwaitStmt awaitStmt: visitor.VisitAwaitStmt(awaitStmt); break;
                case R.AsyncStmt asyncStmt: visitor.VisitAsyncStmt(asyncStmt); break;
                case R.ForeachStmt foreachStmt: visitor.VisitForeachStmt(foreachStmt); break;
                case R.YieldStmt yieldStmt: visitor.VisitYieldStmt(yieldStmt); break;
                case R.DirectiveStmt yieldStmt: visitor.VisitDirectiveStmt(yieldStmt); break;
                default: throw new UnreachableCodeException();
            }
        }        

        public static void Visit<TVisitor>(this ref TVisitor visitor, R.Exp exp)
            where TVisitor : struct, IIR0ExpVisitor
        {
            switch (exp)
            {
                case R.LoadExp loadExp: visitor.VisitLoadExp(loadExp); break;
                case R.StringExp stringExp: visitor.VisitStringExp(stringExp); break;
                case R.IntLiteralExp intExp: visitor.VisitIntLiteralExp(intExp); break;
                case R.BoolLiteralExp boolExp: visitor.VisitBoolLiteralExp(boolExp); break;
                case R.CallInternalUnaryOperatorExp ciuoExp: visitor.VisitCallInternalUnaryOperatorExp(ciuoExp); break;
                case R.CallInternalUnaryAssignOperatorExp ciuaoExp: visitor.VisitCallInternalUnaryAssignOperatorExp(ciuaoExp); break;
                case R.CallInternalBinaryOperatorExp ciboExp: visitor.VisitCallInternalBinaryOperatorExp(ciboExp); break;
                case R.AssignExp assignExp: visitor.VisitAssignExp(assignExp); break;

                case R.CallGlobalFuncExp callGlobalFuncExp: visitor.VisitCallGlobalFuncExp(callGlobalFuncExp); break;
                case R.CallClassMemberFuncExp callClassMemberFuncExp: visitor.VisitCallClassMemberFuncExp(callClassMemberFuncExp); break;
                case R.CallStructMemberFuncExp callStructMemberFuncExp: visitor.VisitCallStructMemberFuncExp(callStructMemberFuncExp); break;

                case R.CallValueExp callValueExp: visitor.VisitCallValueExp(callValueExp); break;
                case R.LambdaExp lambdaExp: visitor.VisitLambdaExp(lambdaExp); break;
                case R.ListExp listExp: visitor.VisitListExp(listExp); break;
                case R.ListIteratorExp listIterExp: visitor.VisitListIteratorExp(listIterExp); break;
                case R.NewEnumElemExp enumExp: visitor.VisitNewEnumElemExp(enumExp); break;
                case R.NewStructExp newStructExp: visitor.VisitNewStructExp(newStructExp); break;
                case R.NewClassExp newClassExp: visitor.VisitNewClassExp(newClassExp); break;
                case R.NewNullableExp newNullableExp: visitor.VisitNewNullableExp(newNullableExp); break;
                case R.CastEnumElemToEnumExp castEnumElemToEnumExp: visitor.VisitCastEnumElemToEnumExp(castEnumElemToEnumExp); break;
                case R.CastClassExp castClassExp: visitor.VisitCastClassExp(castClassExp); break;
                default: throw new UnreachableCodeException();
            }
        }

        public static void Visit<TVisitor, TArg0>(this ref TVisitor visitor, R.Exp exp, TArg0 arg0)
            where TVisitor : struct, IIR0ExpVisitor<TArg0>
        {
            switch (exp)
            {
                case R.LoadExp loadExp: visitor.VisitLoadExp(loadExp, arg0); break;
                case R.StringExp stringExp: visitor.VisitStringExp(stringExp, arg0); break;
                case R.IntLiteralExp intExp: visitor.VisitIntLiteralExp(intExp, arg0); break;
                case R.BoolLiteralExp boolExp: visitor.VisitBoolLiteralExp(boolExp, arg0); break;
                case R.CallInternalUnaryOperatorExp ciuoExp: visitor.VisitCallInternalUnaryOperatorExp(ciuoExp, arg0); break;
                case R.CallInternalUnaryAssignOperatorExp ciuaoExp: visitor.VisitCallInternalUnaryAssignOperatorExp(ciuaoExp, arg0); break;
                case R.CallInternalBinaryOperatorExp ciboExp: visitor.VisitCallInternalBinaryOperatorExp(ciboExp, arg0); break;
                case R.AssignExp assignExp: visitor.VisitAssignExp(assignExp, arg0); break;
                case R.CallGlobalFuncExp callGlobalFuncExp: visitor.VisitCallGlobalFuncExp(callGlobalFuncExp, arg0); break;
                case R.CallClassMemberFuncExp callClassMemberFuncExp: visitor.VisitCallClassMemberFuncExp(callClassMemberFuncExp, arg0); break;
                case R.CallStructMemberFuncExp callStructMemberFuncExp: visitor.VisitCallStructMemberFuncExp(callStructMemberFuncExp, arg0); break;
                case R.CallValueExp callValueExp: visitor.VisitCallValueExp(callValueExp, arg0); break;
                case R.LambdaExp lambdaExp: visitor.VisitLambdaExp(lambdaExp, arg0); break;
                case R.ListExp listExp: visitor.VisitListExp(listExp, arg0); break;
                case R.ListIteratorExp listIterExp: visitor.VisitListIteratorExp(listIterExp, arg0); break;
                case R.NewEnumElemExp enumExp: visitor.VisitNewEnumElemExp(enumExp, arg0); break;
                case R.NewStructExp newStructExp: visitor.VisitNewStructExp(newStructExp, arg0); break;
                case R.NewClassExp newClassExp: visitor.VisitNewClassExp(newClassExp, arg0); break;
                case R.NewNullableExp newNullableExp: visitor.VisitNewNullableExp(newNullableExp, arg0); break;
                case R.CastEnumElemToEnumExp castEnumElemToEnumExp: visitor.VisitCastEnumElemToEnumExp(castEnumElemToEnumExp, arg0); break;
                case R.CastClassExp castClassExp: visitor.VisitCastClassExp(castClassExp, arg0); break;
                default: throw new UnreachableCodeException();
            }
        }

        public static void Visit<TVisitor>(this TVisitor visitor, R.Exp exp)
            where TVisitor : class, IIR0ExpVisitor
        {
            switch (exp)
            {
                case R.LoadExp loadExp: visitor.VisitLoadExp(loadExp); break;
                case R.StringExp stringExp: visitor.VisitStringExp(stringExp); break;
                case R.IntLiteralExp intExp: visitor.VisitIntLiteralExp(intExp); break;
                case R.BoolLiteralExp boolExp: visitor.VisitBoolLiteralExp(boolExp); break;
                case R.CallInternalUnaryOperatorExp ciuoExp: visitor.VisitCallInternalUnaryOperatorExp(ciuoExp); break;
                case R.CallInternalUnaryAssignOperatorExp ciuaoExp: visitor.VisitCallInternalUnaryAssignOperatorExp(ciuaoExp); break;
                case R.CallInternalBinaryOperatorExp ciboExp: visitor.VisitCallInternalBinaryOperatorExp(ciboExp); break;
                case R.AssignExp assignExp: visitor.VisitAssignExp(assignExp); break;

                case R.CallGlobalFuncExp callGlobalFuncExp: visitor.VisitCallGlobalFuncExp(callGlobalFuncExp); break;
                case R.CallClassMemberFuncExp callClassMemberFuncExp: visitor.VisitCallClassMemberFuncExp(callClassMemberFuncExp); break;
                case R.CallStructMemberFuncExp callStructMemberFuncExp: visitor.VisitCallStructMemberFuncExp(callStructMemberFuncExp); break;

                case R.CallValueExp callValueExp: visitor.VisitCallValueExp(callValueExp); break;
                case R.LambdaExp lambdaExp: visitor.VisitLambdaExp(lambdaExp); break;
                case R.ListExp listExp: visitor.VisitListExp(listExp); break;
                case R.ListIteratorExp listIterExp: visitor.VisitListIteratorExp(listIterExp); break;
                case R.NewEnumElemExp enumExp: visitor.VisitNewEnumElemExp(enumExp); break;
                case R.NewStructExp newStructExp: visitor.VisitNewStructExp(newStructExp); break;
                case R.NewClassExp newClassExp: visitor.VisitNewClassExp(newClassExp); break;
                case R.CastEnumElemToEnumExp castEnumElemToEnumExp: visitor.VisitCastEnumElemToEnumExp(castEnumElemToEnumExp); break;
                case R.CastClassExp castClassExp: visitor.VisitCastClassExp(castClassExp); break;
                default: throw new UnreachableCodeException();
            }
        }

        // TODO: class version
        public static void Visit<TVisitor>(this ref TVisitor visitor, R.Loc loc)
            where TVisitor : struct, IIR0LocVisitor
        {
            switch (loc)
            {
                case R.TempLoc tempLoc: visitor.VisitTempLoc(tempLoc); break;
                case R.GlobalVarLoc globalVarLoc: visitor.VisitGlobalVarLoc(globalVarLoc); break;
                case R.LocalVarLoc localVarLoc: visitor.VisitLocalVarLoc(localVarLoc); break;
                case R.LambdaMemberVarLoc capturedVarLoc: visitor.VisitLambdaMemberVarLoc(capturedVarLoc); break;
                case R.ListIndexerLoc listIndexerLoc: visitor.VisitListIndexerLoc(listIndexerLoc); break;
                case R.StructMemberLoc structMemberLoc: visitor.VisitStructMemberLoc(structMemberLoc); break;
                case R.ClassMemberLoc classMemberLoc: visitor.VisitClassMemberLoc(classMemberLoc); break;
                case R.EnumElemMemberLoc enumElemeMemberLoc: visitor.VisitEnumElemMemberLoc(enumElemeMemberLoc); break;
                case R.ThisLoc thisLoc: visitor.VisitThisLoc(thisLoc); break;
                case R.DerefLocLoc derefLocLoc: visitor.VisitDerefLocLoc(derefLocLoc); break;
                case R.DerefExpLoc derefExpLoc: visitor.VisitDerefExpLoc(derefExpLoc); break;
                default: throw new UnreachableCodeException();
            }
        }

        public static TRet Visit<TVisitor, TRet>(this ref TVisitor visitor, R.Loc loc)
            where TVisitor : struct, IIR0LocVisitorWithRet<TRet>
        {
            switch (loc)
            {
                case R.TempLoc tempLoc: return visitor.VisitTempLoc(tempLoc);
                case R.GlobalVarLoc globalVarLoc: return visitor.VisitGlobalVarLoc(globalVarLoc);
                case R.LocalVarLoc localVarLoc: return visitor.VisitLocalVarLoc(localVarLoc);
                case R.LambdaMemberVarLoc lambdaMemberVarLoc: return visitor.VisitLambdaMemberVarLoc(lambdaMemberVarLoc);
                case R.ListIndexerLoc listIndexerLoc: return visitor.VisitListIndexerLoc(listIndexerLoc);
                case R.StructMemberLoc structMemberLoc: return visitor.VisitStructMemberLoc(structMemberLoc);
                case R.ClassMemberLoc classMemberLoc: return visitor.VisitClassMemberLoc(classMemberLoc);
                case R.EnumElemMemberLoc enumElemeMemberLoc: return visitor.VisitEnumElemMemberLoc(enumElemeMemberLoc);
                case R.ThisLoc thisLoc: return visitor.VisitThisLoc(thisLoc);
                case R.DerefLocLoc derefLocLoc: return visitor.VisitDerefLocLoc(derefLocLoc);
                case R.DerefExpLoc derefExpLoc: return visitor.VisitDerefExpLoc(derefExpLoc);
                default: throw new UnreachableCodeException();
            }
        }

        public static void Visit<TVisitor>(this ref TVisitor visitor, R.DirectiveStmt directiveStmt)
            where TVisitor : struct, IIR0DirectiveStmtVisitor
        {
            switch(directiveStmt)
            {
                case R.DirectiveStmt.NotNull notNullDir: visitor.VisitNotNull(notNullDir); break;
                case R.DirectiveStmt.Null nullDir: visitor.VisitNull(nullDir); break;
                case R.DirectiveStmt.StaticNotNull staticNotNullDir: visitor.VisitStaticNotNull(staticNotNullDir); break;
                case R.DirectiveStmt.StaticNull staticNullDir: visitor.VisitStaticNull(staticNullDir); break;
                case R.DirectiveStmt.StaticUnknownNull staticUnknownNullDir: visitor.VisitStaticUnknownNull(staticUnknownNullDir); break;
                default: throw new UnreachableCodeException();
            }
        }
    }
}
