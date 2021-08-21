using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.Test.Misc
{
    // algebraic datatype만 visitor가 개별로 생긴다
    public interface IIR0TypeDeclVisitor
    {
        void VisitStructDecl(R.StructDecl structDecl);
        void VisitClassDecl(R.ClassDecl classDecl);
        void VisitEnumDecl(R.EnumDecl enumDecl);

        public static void Visit<TVisitor>(ref TVisitor visitor, R.TypeDecl typeDecl)
            where TVisitor : struct, IIR0TypeDeclVisitor
        {
            switch (typeDecl)
            {
                case R.StructDecl structDecl: visitor.VisitStructDecl(structDecl); break;
                case R.ClassDecl classDecl: visitor.VisitClassDecl(classDecl); break;
                case R.EnumDecl enumDecl: visitor.VisitEnumDecl(enumDecl); break;
                default: throw new UnreachableCodeException();
            }
        }

        public static void Visit<TVisitor>(TVisitor visitor, R.TypeDecl typeDecl)
            where TVisitor : class, IIR0TypeDeclVisitor
        {
            switch(typeDecl)
            {
                case R.StructDecl structDecl: visitor.VisitStructDecl(structDecl); break;
                case R.ClassDecl classDecl: visitor.VisitClassDecl(classDecl); break;
                case R.EnumDecl enumDecl: visitor.VisitEnumDecl(enumDecl); break;
                default: throw new UnreachableCodeException();
            }
        }
    }

    public interface IIR0FuncDeclVisitor
    {
        void VisitNormalFuncDecl(R.NormalFuncDecl normalFuncDecl);
        void VisitSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl);

        public static void Visit<TVisitor>(ref TVisitor visitor, R.FuncDecl funcDecl)
            where TVisitor : struct, IIR0FuncDeclVisitor
        {
            switch(funcDecl)
            {
                case R.NormalFuncDecl normalFuncDecl: visitor.VisitNormalFuncDecl(normalFuncDecl); break;
                case R.SequenceFuncDecl seqFuncDecl: visitor.VisitSequenceFuncDecl(seqFuncDecl); break;
                default: throw new UnreachableCodeException();
            }
        }
    }

    public interface IIR0CallableMemberDeclVisitor
    {
        void VisitLambdaDecl(R.LambdaDecl lambdaDecl);
        void VisitCapturedStatementDecl(R.CapturedStatementDecl capturedStatementDecl);

        public static void Visit<TVisitor>(ref TVisitor visitor, R.CallableMemberDecl decl)
            where TVisitor : struct, IIR0CallableMemberDeclVisitor
        {
            switch (decl)
            {
                case R.LambdaDecl lambdaDecl: visitor.VisitLambdaDecl(lambdaDecl); break;
                case R.CapturedStatementDecl capturedStatementDecl: visitor.VisitCapturedStatementDecl(capturedStatementDecl); break;
                default: throw new UnreachableCodeException();
            }
        }
    }

    public interface IIR0StmtVisitor
    {
        void VisitCommandStmt(R.CommandStmt commandStmt);
        void VisitGlobalVarDeclStmt(R.GlobalVarDeclStmt globalVarDeclStmt);
        void VisitLocalVarDeclStmt(R.LocalVarDeclStmt localVarDeclStmt);
        void VisitIfStmt(R.IfStmt ifStmt);
        void VisitIfTestClassStmt(R.IfTestClassStmt ifTestClassStmt);
        void VisitIfTestEnumElemStmt(R.IfTestEnumElemStmt ifTestEnumElemStmt);
        void VisitForStmt(R.ForStmt forStmt);
        void VisitContinueStmt(R.ContinueStmt continueStmt);
        void VisitBreakStmt(R.BreakStmt breakStmt);
        void VisitReturnStmt(R.ReturnStmt returnStmt);
        void VisitBlockStmt(R.BlockStmt blockStmt);
        void VisitBlankStmt(R.BlankStmt blankStmt);
        void VisitExpStmt(R.ExpStmt expStmt);
        void VisitTaskStmt(R.TaskStmt taskStmt);
        void VisitAwaitStmt(R.AwaitStmt awaitStmt);
        void VisitAsyncStmt(R.AsyncStmt asyncStmt);
        void VisitForeachStmt(R.ForeachStmt foreachStmt);
        void VisitYieldStmt(R.YieldStmt yieldStmt);
        void VisitDirectiveStmt(R.DirectiveStmt yieldStmt);

        public static void Visit<TVisitor>(ref TVisitor visitor, R.Stmt stmt)
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

        public static void Visit<TVisitor>(TVisitor visitor, R.Stmt stmt)
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
    }

    public interface IIR0ExpVisitor
    {
        void VisitLoadExp(R.LoadExp loadExp);
        void VisitStringExp(R.StringExp stringExp);
        void VisitIntLiteralExp(R.IntLiteralExp intExp);
        void VisitBoolLiteralExp(R.BoolLiteralExp boolExp);
        void VisitCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp);
        void VisitCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperator ciuaoExp);
        void VisitCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp);
        void VisitAssignExp(R.AssignExp assignExp);
        void VisitCallFuncExp(R.CallFuncExp callFuncExp);
        void VisitCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp);
        void VisitCallValueExp(R.CallValueExp callValueExp);
        void VisitLambdaExp(R.LambdaExp lambdaExp);
        void VisitListExp(R.ListExp listExp);
        void VisitListIteratorExp(R.ListIteratorExp listIterExp);
        void VisitNewEnumElemExp(R.NewEnumElemExp enumExp);
        void VisitNewStructExp(R.NewStructExp newStructExp);
        void VisitNewClassExp(R.NewClassExp newClassExp);
        void VisitCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp);
        void VisitCastClassExp(R.CastClassExp castClassExp);

        public static void Visit<TVisitor>(ref TVisitor visitor, R.Exp exp)
            where TVisitor : IIR0ExpVisitor
        {
            switch(exp)
            {
                case R.LoadExp loadExp: visitor.VisitLoadExp(loadExp); break;
                case R.StringExp stringExp: visitor.VisitStringExp(stringExp); break;
                case R.IntLiteralExp intExp: visitor.VisitIntLiteralExp(intExp); break;
                case R.BoolLiteralExp boolExp: visitor.VisitBoolLiteralExp(boolExp); break;
                case R.CallInternalUnaryOperatorExp ciuoExp: visitor.VisitCallInternalUnaryOperatorExp(ciuoExp); break;
                case R.CallInternalUnaryAssignOperator ciuaoExp: visitor.VisitCallInternalUnaryAssignOperatorExp(ciuaoExp); break;
                case R.CallInternalBinaryOperatorExp ciboExp: visitor.VisitCallInternalBinaryOperatorExp(ciboExp); break;
                case R.AssignExp assignExp: visitor.VisitAssignExp(assignExp); break;
                case R.CallFuncExp callFuncExp: visitor.VisitCallFuncExp(callFuncExp); break;
                case R.CallSeqFuncExp callSeqFuncExp: visitor.VisitCallSeqFuncExp(callSeqFuncExp); break;
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

        public static void Visit<TVisitor>(TVisitor visitor, R.Exp exp)
            where TVisitor : IIR0ExpVisitor
        {
            switch (exp)
            {
                case R.LoadExp loadExp: visitor.VisitLoadExp(loadExp); break;
                case R.StringExp stringExp: visitor.VisitStringExp(stringExp); break;
                case R.IntLiteralExp intExp: visitor.VisitIntLiteralExp(intExp); break;
                case R.BoolLiteralExp boolExp: visitor.VisitBoolLiteralExp(boolExp); break;
                case R.CallInternalUnaryOperatorExp ciuoExp: visitor.VisitCallInternalUnaryOperatorExp(ciuoExp); break;
                case R.CallInternalUnaryAssignOperator ciuaoExp: visitor.VisitCallInternalUnaryAssignOperatorExp(ciuaoExp); break;
                case R.CallInternalBinaryOperatorExp ciboExp: visitor.VisitCallInternalBinaryOperatorExp(ciboExp); break;
                case R.AssignExp assignExp: visitor.VisitAssignExp(assignExp); break;
                case R.CallFuncExp callFuncExp: visitor.VisitCallFuncExp(callFuncExp); break;
                case R.CallSeqFuncExp callSeqFuncExp: visitor.VisitCallSeqFuncExp(callSeqFuncExp); break;
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
    }

    public interface IIR0LocVisitor
    {
        void VisitTempLoc(R.TempLoc loc);
        void VisitGlobalVarLoc(R.GlobalVarLoc loc);
        void VisitLocalVarLoc(R.LocalVarLoc loc);
        void VisitCapturedVarLoc(R.CapturedVarLoc loc);
        void VisitListIndexerLoc(R.ListIndexerLoc loc);
        void VisitStaticMemberLoc(R.StaticMemberLoc loc);
        void VisitStructMemberLoc(R.StructMemberLoc loc);
        void VisitClassMemberLoc(R.ClassMemberLoc loc);
        void VisitEnumElemMemberLoc(R.EnumElemMemberLoc loc);
        void VisitThisLoc(R.ThisLoc loc);
        void VisitDerefLocLoc(R.DerefLocLoc loc);
        void VisitDerefExpLoc(R.DerefExpLoc loc);

        public static void Visit<TVisitor>(ref TVisitor visitor, R.Loc loc)
            where TVisitor : struct, IIR0LocVisitor
        {
            switch (loc)
            {
                case R.TempLoc tempLoc: visitor.VisitTempLoc(tempLoc); break;
                case R.GlobalVarLoc globalVarLoc: visitor.VisitGlobalVarLoc(globalVarLoc); break;
                case R.LocalVarLoc localVarLoc: visitor.VisitLocalVarLoc(localVarLoc); break;
                case R.CapturedVarLoc capturedVarLoc: visitor.VisitCapturedVarLoc(capturedVarLoc); break;
                case R.ListIndexerLoc listIndexerLoc: visitor.VisitListIndexerLoc(listIndexerLoc); break;
                case R.StaticMemberLoc staticMemberLoc: visitor.VisitStaticMemberLoc(staticMemberLoc); break;
                case R.StructMemberLoc structMemberLoc: visitor.VisitStructMemberLoc(structMemberLoc); break;
                case R.ClassMemberLoc classMemberLoc: visitor.VisitClassMemberLoc(classMemberLoc); break;
                case R.EnumElemMemberLoc enumElemeMemberLoc: visitor.VisitEnumElemMemberLoc(enumElemeMemberLoc); break;
                case R.ThisLoc thisLoc: visitor.VisitThisLoc(thisLoc); break;
                case R.DerefLocLoc derefLocLoc: visitor.VisitDerefLocLoc(derefLocLoc); break;
                case R.DerefExpLoc derefExpLoc: visitor.VisitDerefExpLoc(derefExpLoc); break;
                default: throw new UnreachableCodeException();
            }
        }
    }

    public class IR0Writer
    {
        IndentedStreamWriter isw;

        IR0Writer(IndentedStreamWriter isw)
        { 
            this.isw = isw; 
        }

        struct IndentedStreamWriter
        {
            class NeedIndent { public bool Value; }

            StreamWriter sw;
            string indent;
            NeedIndent needIndent;

            public IndentedStreamWriter(StreamWriter sw)
            {
                this.sw = sw;
                this.indent = "    ";
                this.needIndent = new NeedIndent() { Value = false };
            }

            IndentedStreamWriter(StreamWriter sw, string indent, NeedIndent needIndent)
            {
                this.sw = sw;
                this.indent = indent;
                this.needIndent = needIndent;
            }

            public void Write(string text)
            {
                if (needIndent.Value)
                {
                    sw.Write(indent);
                    needIndent.Value = false;
                }
               
                sw.Write(text);
            }

            public void Write(int v)
            {
                if (needIndent.Value)
                {
                    sw.Write(indent);
                    needIndent.Value = false;
                }

                sw.Write(v);
            }

            public void WriteLine(string text)
            {
                sw.WriteLine(text);
                needIndent.Value = true;
            }

            public void WriteLine()
            {
                sw.WriteLine();
                needIndent.Value = true;
            }

            public IndentedStreamWriter Push()
            {
                return new IndentedStreamWriter(sw, this.indent + "    ", needIndent);
            }
        }

        struct TypeDeclWriter : IIR0TypeDeclVisitor
        {
            IndentedStreamWriter isw;

            public static void Write(IndentedStreamWriter isw, R.TypeDecl typeDecl)
            {
                var writer = new TypeDeclWriter(isw);
                IIR0TypeDeclVisitor.Visit(ref writer, typeDecl);
            }

            TypeDeclWriter(IndentedStreamWriter isw)
            {
                this.isw = isw;
            }

            public void VisitStructDecl(R.StructDecl structDecl)
            {
                var writer = new IR0Writer(isw);
                writer.WriteNew("R.StructDecl", structDecl.AccessModifier, structDecl.Name, structDecl.TypeParams, structDecl.BaseTypes,
                    structDecl.ConstructorDecls, structDecl.MemberFuncDecls, structDecl.MemberVarDecls);
            }

            public void VisitClassDecl(R.ClassDecl classDecl)
            {
                var writer = new IR0Writer(isw);
                writer.WriteNew("R.ClassDecl", classDecl.AccessModifier, classDecl.Name, classDecl.TypeParams, classDecl.BaseClass, classDecl.Interfaces,
                    classDecl.ConstructorDecls, classDecl.MemberFuncDecls, classDecl.MemberVarDecls);
            }

            public void VisitEnumDecl(R.EnumDecl enumDecl)
            {
                var writer = new IR0Writer(isw);
                writer.WriteNew("R.EnumDecl", enumDecl.Name, enumDecl.TypeParams, enumDecl.Elems);
            }
        }

        struct FuncDeclWriter : IIR0FuncDeclVisitor
        {
            IndentedStreamWriter isw;

            public static void Write(IndentedStreamWriter isw, R.FuncDecl funcDecl)
            {
                var writer = new FuncDeclWriter(isw);
                IIR0FuncDeclVisitor.Visit(ref writer, funcDecl);
            }

            FuncDeclWriter(IndentedStreamWriter isw)
            {
                this.isw = isw;
            }

            public void VisitNormalFuncDecl(R.NormalFuncDecl normalFuncDecl)
            {
                var writer = new IR0Writer(isw);
                writer.WriteNew(
                    "R.NormalFuncDecl",
                    normalFuncDecl.CallableMemberDecls,
                    normalFuncDecl.Name,
                    normalFuncDecl.IsThisCall,
                    normalFuncDecl.TypeParams,
                    normalFuncDecl.Parameters, 
                    normalFuncDecl.Body
                );
            }

            public void VisitSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl)
            {
                var writer = new IR0Writer(isw);
                writer.WriteNew(
                    "R.SequenceFuncDecl", 
                    seqFuncDecl.CallableMemberDecls,
                    seqFuncDecl.Name,
                    seqFuncDecl.IsThisCall,
                    seqFuncDecl.YieldType,
                    seqFuncDecl.TypeParams,
                    seqFuncDecl.Parameters,
                    seqFuncDecl.Body
                );
            }
        }

        struct CallableMemberDeclWriter : IIR0CallableMemberDeclVisitor
        {
            IndentedStreamWriter isw;

            public static void Write(IndentedStreamWriter isw, R.CallableMemberDecl decl)
            {
                var writer = new CallableMemberDeclWriter(isw);
                IIR0CallableMemberDeclVisitor.Visit(ref writer, decl);
            }

            CallableMemberDeclWriter(IndentedStreamWriter isw)
            {
                this.isw = isw;
            }

            public void VisitCapturedStatementDecl(R.CapturedStatementDecl decl)
            {
                var writer = new IR0Writer(isw);
                writer.WriteNew("R.CapturedStatementDecl", decl.Name, decl.CapturedStatement);
            }

            public void VisitLambdaDecl(R.LambdaDecl decl)
            {
                var writer = new IR0Writer(isw);
                writer.WriteNew("R.LambdaDecl", decl.Name, decl.CapturedStatement, decl.Parameters);
            }
        }

        struct StmtWriter : IIR0StmtVisitor
        {
            IR0Writer writer;            

            public static void Write(IR0Writer writer, R.Stmt stmt)
            {
                var stmtWriter = new StmtWriter(writer);
                IIR0StmtVisitor.Visit(ref stmtWriter, stmt);
            }

            StmtWriter(IR0Writer writer) { this.writer = writer; }

            public void VisitAsyncStmt(R.AsyncStmt asyncStmt)
            {
                writer.WriteNew("R.AsyncStmt", asyncStmt.CapturedStatementDecl);
            }

            public void VisitAwaitStmt(R.AwaitStmt awaitStmt)
            {
                writer.WriteNew("R.AwaitStmt", awaitStmt.Body);
            }

            public void VisitBlankStmt(R.BlankStmt blankStmt)
            {
                writer.WriteString("R.BlankStmt.Instance");
            }

            public void VisitBlockStmt(R.BlockStmt blockStmt)
            {
                writer.WriteNew("R.BlockStmt", blockStmt.Stmts);
            }

            public void VisitBreakStmt(R.BreakStmt breakStmt)
            {
                writer.WriteString("R.BreakStmt.Instance");
            }

            public void VisitCommandStmt(R.CommandStmt commandStmt)
            {
                writer.WriteNew("R.CommandStmt", commandStmt.Commands);
            }

            public void VisitContinueStmt(R.ContinueStmt continueStmt)
            {
                writer.WriteString("R.ContinueStmt.Instance");
            }

            public void VisitDirectiveStmt(R.DirectiveStmt yieldStmt)
            {
                throw new NotImplementedException();
            }

            public void VisitExpStmt(R.ExpStmt expStmt)
            {
                writer.WriteNew("R.ExpStmt", expStmt.Exp);
            }

            public void VisitForeachStmt(R.ForeachStmt foreachStmt)
            {
                writer.WriteNew("R.ForeachStmt", foreachStmt.ElemType, foreachStmt.ElemName, foreachStmt.Iterator, foreachStmt.Body);
            }

            public void VisitForStmt(R.ForStmt forStmt)
            {   
                writer.WriteNew("R.ForStmt", forStmt.Initializer, forStmt.CondExp, forStmt.ContinueExp, forStmt.Body);
            }

            public void VisitGlobalVarDeclStmt(R.GlobalVarDeclStmt globalVarDeclStmt)
            {
                writer.WriteNew("R.GlobalVarDeclStmt", globalVarDeclStmt.Elems);
            }

            public void VisitIfStmt(R.IfStmt ifStmt)
            {
                writer.WriteNew("R.IfStmt", ifStmt.Cond, ifStmt.Body, ifStmt.ElseBody);
            }

            public void VisitIfTestClassStmt(R.IfTestClassStmt ifTestClassStmt)
            {
                writer.WriteNew("R.IfTestClassStmt", ifTestClassStmt.Target, ifTestClassStmt.TestType, ifTestClassStmt.VarName, ifTestClassStmt.Body, ifTestClassStmt.ElseBody);
            }

            public void VisitIfTestEnumElemStmt(R.IfTestEnumElemStmt ifTestEnumElemStmt)
            {
                writer.WriteNew("R.IfTestEnumElemStmt", ifTestEnumElemStmt.Target, ifTestEnumElemStmt.EnumElem, ifTestEnumElemStmt.VarName, ifTestEnumElemStmt.Body, ifTestEnumElemStmt.ElseBody);
            }

            public void VisitLocalVarDeclStmt(R.LocalVarDeclStmt localVarDeclStmt)
            {
                writer.WriteNew("R.LocalVarDeclStmt", localVarDeclStmt.VarDecl);
            }

            public void VisitReturnStmt(R.ReturnStmt returnStmt)
            {
                writer.WriteNew("R.ReturnStmt", returnStmt.Info);
            }

            public void VisitTaskStmt(R.TaskStmt taskStmt)
            {
                writer.WriteNew("R.TaskStmt", taskStmt.CapturedStatementDecl);
            }

            public void VisitYieldStmt(R.YieldStmt yieldStmt)
            {
                writer.WriteNew("R.YieldStmt", yieldStmt.Value);
            }
        }

        struct ExpWriter: IIR0ExpVisitor
        {
            IR0Writer writer;
            public static void Write(IR0Writer writer, R.Exp exp)
            {
                var expWriter = new ExpWriter(writer);
                IIR0ExpVisitor.Visit(ref expWriter, exp);
            }

            ExpWriter(IR0Writer writer) { this.writer = writer; }

            public void VisitAssignExp(R.AssignExp assignExp)
            {
                writer.WriteNew("R.AssignExp", assignExp.Dest, assignExp.Src);
            }

            public void VisitBoolLiteralExp(R.BoolLiteralExp boolExp)
            {
                writer.WriteNew("R.BoolLiteralExp", boolExp.Value);
            }

            public void VisitCallFuncExp(R.CallFuncExp callFuncExp)
            {
                writer.WriteNew("R.CallFuncExp", callFuncExp.Func, callFuncExp.Instance, callFuncExp.Args);
            }

            public void VisitCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp)
            {
                writer.WriteNew("R.CallInternalBinaryOperatorExp", ciboExp.Operator, ciboExp.Operand0, ciboExp.Operand1);
            }

            public void VisitCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperator ciuaoExp)
            {
                writer.WriteNew("R.CallInternalUnaryAssignOperatorExp", ciuaoExp.Operator, ciuaoExp.Operand);
            }

            public void VisitCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp)
            {
                writer.WriteNew("R.CallInternalUnaryOperatorExp", ciuoExp.Operator, ciuoExp.Operand);
            }

            public void VisitCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp)
            {
                writer.WriteNew("R.CallSeqFuncExp", callSeqFuncExp.SeqFunc, callSeqFuncExp.Instance, callSeqFuncExp.Args);
            }

            public void VisitCallValueExp(R.CallValueExp callValueExp)
            {
                writer.WriteNew("R.CallValueExp", callValueExp.Lambda, callValueExp.Callable, callValueExp.Args);
            }

            public void VisitCastClassExp(R.CastClassExp castClassExp)
            {
                writer.WriteNew("R.CastClassExp", castClassExp.Src, castClassExp.Class);
            }

            public void VisitCastEnumElemToEnumExp(R.CastEnumElemToEnumExp exp)
            {
                writer.WriteNew("R.CastEnumElemToEnumExp", exp.Src, exp.EnumElem);
            }

            public void VisitIntLiteralExp(R.IntLiteralExp intExp)
            {
                writer.WriteNew("R.IntLiteralExp", intExp.Value);
            }

            public void VisitLambdaExp(R.LambdaExp lambdaExp)
            {
                writer.WriteNew("R.LambdaExp", lambdaExp.Lambda);
            }

            public void VisitListExp(R.ListExp listExp)
            {
                writer.WriteNew("R.ListExp", listExp.ElemType, listExp.Elems);
            }

            public void VisitListIteratorExp(R.ListIteratorExp exp)
            {
                writer.WriteNew("R.ListIteratorExp", exp.ListLoc);
            }

            public void VisitLoadExp(R.LoadExp loadExp)
            {
                writer.WriteNew("R.LoadExp", loadExp.Loc);
            }

            public void VisitNewClassExp(R.NewClassExp newClassExp)
            {
                writer.WriteNew("R.NewClassExp", newClassExp.Class, newClassExp.ConstructorParamHash, newClassExp.Args);
            }

            public void VisitNewEnumElemExp(R.NewEnumElemExp exp)
            {
                writer.WriteNew("R.NewEnumElemExp", exp.Elem, exp.Args);
            }

            public void VisitNewStructExp(R.NewStructExp exp)
            {
                writer.WriteNew("R.NewStructExp", exp.Constructor, exp.Args);
            }

            public void VisitStringExp(R.StringExp stringExp)
            {
                writer.WriteNew("R.StringExp", stringExp.Elements);
            }
        }

        struct LocWriter : IIR0LocVisitor
        {
            IR0Writer writer;

            public static void Write(IR0Writer writer, R.Loc loc)
            {
                var locWriter = new LocWriter(writer);
                IIR0LocVisitor.Visit(ref locWriter, loc);
            }

            public void VisitTempLoc(R.TempLoc loc)
            {
                writer.WriteNew("R.TempLoc", loc.Exp, loc.Type);
            }

            public void VisitGlobalVarLoc(R.GlobalVarLoc loc)
            {
                writer.WriteNew("R.GlobalVarLoc", loc.Name);
            }

            public void VisitLocalVarLoc(R.LocalVarLoc loc)
            {
                writer.WriteNew("R.LocalVarLoc", loc.Name);
            }

            public void VisitCapturedVarLoc(R.CapturedVarLoc loc)
            {
                writer.WriteNew("R.CapturedVarLoc", loc.Name);
            }

            public void VisitListIndexerLoc(R.ListIndexerLoc loc)
            {
                writer.WriteNew("R.ListIndexerLoc", loc.List, loc.Index);
            }

            public void VisitStaticMemberLoc(R.StaticMemberLoc loc)
            {
                writer.WriteNew("R.StaticMemberLoc", loc.MemberPath);
            }

            public void VisitStructMemberLoc(R.StructMemberLoc loc)
            {
                writer.WriteNew("R.StructMemberLoc", loc.Instance, loc.MemberPath);
            }

            public void VisitClassMemberLoc(R.ClassMemberLoc loc)
            {
                writer.WriteNew("R.ClassMemberLoc", loc.Instance, loc.MemberPath);
            }

            public void VisitEnumElemMemberLoc(R.EnumElemMemberLoc loc)
            {
                writer.WriteNew("R.EnumElemMemberLoc", loc.Instance, loc.EnumElemField);
            }

            public void VisitThisLoc(R.ThisLoc loc)
            {
                writer.WriteString("R.ThisLoc.Instance");
            }

            public void VisitDerefLocLoc(R.DerefLocLoc loc)
            {
                writer.WriteNew("R.DerefLocLoc", loc.Loc);
            }

            public void VisitDerefExpLoc(R.DerefExpLoc loc)
            {
                writer.WriteNew("R.DerefExpLoc", loc.Exp);
            }

            LocWriter(IR0Writer writer)
            {
                this.writer = writer;
            }


        }

        void WriteModuleName(R.ModuleName moduleName)
        {
            isw.Write(moduleName.Value);
        }

        void WriteName(R.Name name)
        {
            switch(name)
            {
                case R.Name.Normal normal: WriteNew("R.Name.Normal", normal.Value); break;
                case R.Name.IndexerGet: WriteNew("R.Name.IndexerGet"); break;
                case R.Name.IndexerSet: WriteNew("R.Name.IndexerSet"); break;

                case R.Name.OpInc: WriteNew("R.Name.OpInc"); break;
                case R.Name.OpDec: WriteNew("R.Name.OpDec"); break;

                // anonymous type names        
                case R.Name.Anonymous a: WriteNew("R.Name.Anonymous", a.Id); break;
                case R.Name.Constructor: WriteNew("R.Name.Constructor"); break;
            }
        }

        void WriteAnonymousId(R.AnonymousId anonymousId)
        {
            isw.Write(anonymousId.Value);
        }

        void WriteParamHash(R.ParamHash paramHash)
        {
            if (paramHash.Equals(R.ParamHash.None))
                isw.Write("R.ParamHash.None");
            else
                WriteNew("R.ParamHash", paramHash.Entries);
        }        

        void WriteParamHashEntry(R.ParamHashEntry entry)
        {
            WriteNew("R.ParamHashEntry", entry.Kind, entry.Type);
        }

        void WriteParamKind(R.ParamKind paramKind)
        {
            switch (paramKind)
            {
                case R.ParamKind.Normal: isw.Write("R.ParamKind.Normal"); break;
                case R.ParamKind.Params: isw.Write("R.ParamKind.Params"); break;
                case R.ParamKind.Ref: isw.Write("R.ParamKind.Ref"); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteAccessModifier(R.AccessModifier modifier)
        {
            switch(modifier)
            {
                case R.AccessModifier.Public: isw.Write("R.AccessModifier.Public"); break;
                case R.AccessModifier.Protected: isw.Write("R.AccessModifier.Protected"); break;
                case R.AccessModifier.Private: isw.Write("R.AccessModifier.Private"); break;
            }
        }

        void WritePath(R.Path path)
        {
            switch(path)
            {
                // R.Path.Reserved
                case R.Path.TupleType tuple: WriteNew("R.Path.TupleType", tuple.Elems); break;
                case R.Path.TypeVarType typeVar: WriteNew("R.Path.TypeVarType", typeVar.Index); break;
                case R.Path.VoidType: isw.Write("R.Path.VoidType.Instance"); break;
                case R.Path.BoxType box: WriteNew("R.Path.BoxType", box.Type); break;
                case R.Path.GenericRefType genericRef: WriteNew("R.Path.GenericRefType", genericRef.Type); break;
                case R.Path.FuncType: WriteNew("R.Path.FuncType"); break; // TODO:
                case R.Path.NullableType nullable: WriteNew("R.Path.NullableType", nullable.Type); break;

                // special cases R.Path.Normal
                case R.Path.Normal when path == R.Path.System: isw.Write("R.Path.System"); break;
                case R.Path.Normal when path == R.Path.Bool: isw.Write("R.Path.Bool"); break;
                case R.Path.Normal when path == R.Path.Int: isw.Write("R.Path.Int"); break;
                case R.Path.Normal when path == R.Path.String: isw.Write("R.Path.String"); break;

                // todo: seq, list
                case R.Path.Root root: WriteNew("R.Path.Root", root.ModuleName); break;
                case R.Path.Nested nested: WriteNew("R.Path.Nested", nested.Outer, nested.Name, nested.ParamHash, nested.TypeArgs); break;

                default: throw new UnreachableCodeException();
            }
        }

        void WriteTupleTypeElem(R.TupleTypeElem tte)
        {
            WriteNew("R.TupleTypeElem", tte.Type, tte.Name);
        }

        void WriteParam(R.Param param)
        {
            WriteNew("R.Param", param.Kind, param.Type, param.Name);
        }

        void WriteStructConstructorDecl(R.StructConstructorDecl scd)
        {
            WriteNew("R.StructConstructorDecl", scd.AccessModifier, scd.Parameters, scd.Body);
        }

        void WriteStructMemberVarDecl(R.StructMemberVarDecl memberVar)
        {
            WriteNew("R.StructMemberVarDecl", memberVar.AccessModifier, memberVar.Type, memberVar.Names);
        }

        void WriteConstructorBaseCallinfo(R.ConstructorBaseCallInfo constructorBaseCallInfo)
        {
            WriteNew("R.ConstructorBaseCallInfo", constructorBaseCallInfo.ParamHash, constructorBaseCallInfo.Args);
        }

        void WriteClassConstructorDecl(R.ClassConstructorDecl constructorDecl)
        {
            WriteNew("R.ClassConstructorDecl", constructorDecl.AccessModifier, constructorDecl.CallableMemberDecls, constructorDecl.Parameters, constructorDecl.BaseCallInfo);
        }

        void WriteClassMemberVarDecl(R.ClassMemberVarDecl memberVarDecl)
        {
            WriteNew("R.ClassMemberVarDecl", memberVarDecl.AccessModifier, memberVarDecl.Type, memberVarDecl.Names);
        }

        void WriteEnumElement(R.EnumElement elem)
        {
            WriteNew("R.EnumElement", elem.Name, elem.Fields);
        }

        void WriteEnumElementField(R.EnumElementField field)
        {
            WriteNew("R.EnumElementField", field.Type, field.Name);
        }

        void WriteCapturedStatement(R.CapturedStatement cs)
        {
            WriteNew("R.CapturedStatement", cs.ThisType, cs.OuterLocalVars, cs.Body);
        }

        void WriteOuterLocalVarInfo(R.OuterLocalVarInfo info)
        {
            WriteNew("R.OuterLocalVarInfo", info.Type, info.Name);
        }

        // TODO: WriteForStmtInitializer를 통해서 들어오고, WriteObject에서 빠져있다
        // WriteForStmtInitializer를 없애고 WriteObject에서 직접 들어오게 해야하나
        void WriteExpForStmtInitializer(R.ExpForStmtInitializer expInit)
        {
            WriteNew("R.ExpForStmtInitializer", expInit.Exp);
        }

        void WriteVarDeclForStmtInitializer(R.VarDeclForStmtInitializer varInit)
        {
            WriteNew("R.VarDeclForStmtInitializer", varInit.VarDecl);
        }

        void WriteForStmtInitializer(R.ForStmtInitializer init)
        {
            switch(init)
            {
                case R.ExpForStmtInitializer expInit: WriteExpForStmtInitializer(expInit); break;
                case R.VarDeclForStmtInitializer varDeclInit: WriteVarDeclForStmtInitializer(varDeclInit); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteLocalVarDecl(R.LocalVarDecl decl)
        {
            WriteNew("R.LocalVarDecl", decl.Elems);
        }

        void WriteNormalVarDeclElement(R.VarDeclElement.Normal elem)
        {
            WriteNew("R.VarDeclElement.Normal", elem.Type, elem.Name, elem.InitExp);
        }   

        void WriteNormalDefaultVarDeclElement(R.VarDeclElement.NormalDefault elem)
        {
            WriteNew("R.VarDeclElement.NormalDefault", elem.Type, elem.Name);
        }

        void WriteRefVarDeclElement(R.VarDeclElement.Ref elem)
        {
            WriteNew("R.VarDeclElement.Ref", elem.Name, elem.Loc);
        }

        // 여기도 위의 세개 WriteNormalVarDeclElement, WriteNormalDefault... 는 WriteObject에 포함시키지 않았다
        void WriteVarDeclElement(R.VarDeclElement elem)
        {
            switch(elem)
            {
                case R.VarDeclElement.Normal normalElem: WriteNormalVarDeclElement(normalElem); break;
                case R.VarDeclElement.NormalDefault normalDefaultElem: WriteNormalDefaultVarDeclElement(normalDefaultElem); break;
                case R.VarDeclElement.Ref refElem: WriteRefVarDeclElement(refElem); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteRefReturnInfo(R.ReturnInfo.Ref refInfo)
        {
            WriteNew("R.ReturnInfo.Ref", refInfo.Loc);
        }

        void WriteExpReturnInfo(R.ReturnInfo.Expression expInfo)
        {
            WriteNew("R.ReturnInfo.Exp", expInfo.Exp);
        }

        void WriteNoneReturnInfo(R.ReturnInfo.None noneInfo)
        {
            WriteString("R.ReturnInfo.None.Instance");
        }

        void WriteReturnInfo(R.ReturnInfo info)
        {
            switch(info)
            {
                case R.ReturnInfo.Ref refInfo: WriteRefReturnInfo(refInfo); break;
                case R.ReturnInfo.Expression expInfo: WriteExpReturnInfo(expInfo); break;
                case R.ReturnInfo.None noneInfo: WriteNoneReturnInfo(noneInfo); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteNormalArgument(R.Argument.Normal normalArg)
        {
            WriteNew("R.Argument.Normal", normalArg.Exp);
        }

        void WriteParamsArgument(R.Argument.Params paramsArg)
        {
            WriteNew("R.Argument.Params", paramsArg.Exp, paramsArg.ElemCount);
        }

        void WriteRefArgument(R.Argument.Ref refArg)
        {
            WriteNew("R.Argument.Ref", refArg.Loc);
        }

        void WriteArgument(R.Argument arg)
        {
            switch(arg)
            {
                case R.Argument.Normal normalArg: WriteNormalArgument(normalArg); break;
                case R.Argument.Params paramsArg: WriteParamsArgument(paramsArg); break;
                case R.Argument.Ref refArg: WriteRefArgument(refArg); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteInternalBinaryOperator(R.InternalBinaryOperator op)
        {
            switch(op)
            {
                case R.InternalBinaryOperator.Multiply_Int_Int_Int: isw.Write("R.InternalBinaryOperator.Multiply_Int_Int_Int"); break;
                case R.InternalBinaryOperator.Divide_Int_Int_Int: isw.Write("R.InternalBinaryOperator.Divide_Int_Int_Int"); break;
                case R.InternalBinaryOperator.Modulo_Int_Int_Int: isw.Write("R.InternalBinaryOperator.Modulo_Int_Int_Int"); break;
                case R.InternalBinaryOperator.Add_Int_Int_Int: isw.Write("R.InternalBinaryOperator.Add_Int_Int_Int"); break;
                case R.InternalBinaryOperator.Add_String_String_String: isw.Write("R.InternalBinaryOperator.Add_String_String_String"); break;
                case R.InternalBinaryOperator.Subtract_Int_Int_Int: isw.Write("R.InternalBinaryOperator.Subtract_Int_Int_Int"); break;
                case R.InternalBinaryOperator.LessThan_Int_Int_Bool: isw.Write("R.InternalBinaryOperator.LessThan_Int_Int_Bool"); break;
                case R.InternalBinaryOperator.LessThan_String_String_Bool: isw.Write("R.InternalBinaryOperator.LessThan_String_String_Bool"); break;
                case R.InternalBinaryOperator.GreaterThan_Int_Int_Bool: isw.Write("R.InternalBinaryOperator.GreaterThan_Int_Int_Bool"); break;
                case R.InternalBinaryOperator.GreaterThan_String_String_Bool: isw.Write("R.InternalBinaryOperator.GreaterThan_String_String_Bool"); break;
                case R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool: isw.Write("R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool"); break;
                case R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool: isw.Write("R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool"); break;
                case R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool: isw.Write("R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool"); break;
                case R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool: isw.Write("R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool"); break;
                case R.InternalBinaryOperator.Equal_Int_Int_Bool: isw.Write("R.InternalBinaryOperator.Equal_Int_Int_Bool"); break;
                case R.InternalBinaryOperator.Equal_Bool_Bool_Bool: isw.Write("R.InternalBinaryOperator.Equal_Bool_Bool_Bool"); break;
                case R.InternalBinaryOperator.Equal_String_String_Bool: isw.Write("R.InternalBinaryOperator.Equal_String_String_Bool"); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteInternalUnaryAssignOperator(R.InternalUnaryAssignOperator op)
        {
            switch (op)
            {
                case R.InternalUnaryAssignOperator.PrefixInc_Int_Int: isw.Write("R.InternalUnaryAssignOperator.PrefixInc_Int_Int"); break;
                case R.InternalUnaryAssignOperator.PrefixDec_Int_Int: isw.Write("R.InternalUnaryAssignOperator.PrefixDec_Int_Int"); break;
                case R.InternalUnaryAssignOperator.PostfixInc_Int_Int: isw.Write("R.InternalUnaryAssignOperator.PostfixInc_Int_Int"); break;
                case R.InternalUnaryAssignOperator.PostfixDec_Int_Int: isw.Write("R.InternalUnaryAssignOperator.PostfixDec_Int_Int"); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteInternalUnaryOperator(R.InternalUnaryOperator op)
        {
            switch(op)
            {
                case R.InternalUnaryOperator.LogicalNot_Bool_Bool: isw.Write("R.InternalUnaryOperator.LogicalNot_Bool_Bool"); break;
                case R.InternalUnaryOperator.UnaryMinus_Int_Int: isw.Write("R.InternalUnaryOperator.UnaryMinus_Int_Int"); break;
                case R.InternalUnaryOperator.ToString_Bool_String: isw.Write("R.InternalUnaryOperator.ToString_Bool_String"); break;
                case R.InternalUnaryOperator.ToString_Int_String: isw.Write("R.InternalUnaryOperator.ToString_Int_String"); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteExpStringExpElement(R.ExpStringExpElement elem)
        {
            WriteNew("R.ExpStringExpElement", elem.Exp);
        }

        void WriteTextStringExpElement(R.TextStringExpElement elem)
        {
            WriteNew("R.TextStringExpElement", elem.Text);
        }

        void WriteStringExpElement(R.StringExpElement elem)
        {
            switch(elem)
            {
                case R.ExpStringExpElement expElem: WriteExpStringExpElement(expElem); break;
                case R.TextStringExpElement textElem: WriteTextStringExpElement(textElem); break;
                default: throw new UnreachableCodeException();
            }
        }

        void WriteTypeDecl(R.TypeDecl typeDecl)
        {
            TypeDeclWriter.Write(isw, typeDecl);
        }

        void WriteFuncDecl(R.FuncDecl funcDecl)
        {
            FuncDeclWriter.Write(isw, funcDecl);
        }

        void WriteCallableMemberDecl(R.CallableMemberDecl callableMemberDecl)
        {
            CallableMemberDeclWriter.Write(isw, callableMemberDecl);
        }

        void WriteStmt(R.Stmt stmt)
        {
            StmtWriter.Write(this, stmt);
        }

        void WriteExp(R.Exp exp)
        {
            ExpWriter.Write(this, exp);
        }

        void WriteLoc(R.Loc loc)
        {
            LocWriter.Write(this, loc);
        }

        void WriteString(string text)
        {
            isw.Write(text);
        }

        void WriteInt(int v)
        {
            isw.Write(v);
        }

        void WriteArray<TItem>(Action<TItem> itemWriter, string? itemType, ImmutableArray<TItem> items)
        {
            if (items.Length == 0)
            {
                isw.Write("default");
                return;
            }

            if (items.Length == 1)
            {
                if (itemType != null)
                    isw.Write($"Arr<{itemType}>(");
                else
                    isw.Write($"Arr(");

                itemWriter.Invoke(items[0]);
                isw.Write(")");
                return;
            }

            // Arr<>(
            if (itemType != null)
                isw.Write($"Arr<{itemType}>(");
            else
                isw.Write($"Arr(");

            var isw1 = isw.Push();

            bool bFirst = true;
            foreach (var item in items)
            {
                if (!bFirst) isw1.WriteLine(",");
                else bFirst = false;

                itemWriter.Invoke(item);
            }

            isw.Write(")");
        }

        void WriteObject(object? obj)
        {
            switch(obj)
            {
                case null: isw.Write("null"); break;

                case R.ModuleName mn: WriteModuleName(mn); break;
                case ImmutableArray<R.ModuleName> mns: WriteArray(WriteModuleName, null, mns); break;

                case R.Name name: WriteName(name); break;
                case ImmutableArray<R.Name> names: WriteArray(WriteName, "R.Name", names); break;

                case R.AnonymousId anonymousId: WriteAnonymousId(anonymousId); break;
                case ImmutableArray<R.AnonymousId> anonymousIds: WriteArray(WriteAnonymousId, null, anonymousIds); break;

                case R.ParamHash paramHash: WriteParamHash(paramHash); break;
                case ImmutableArray<R.ParamHash> paramHashes: WriteArray(WriteParamHash, null, paramHashes); break;

                case R.ParamHashEntry phe: WriteParamHashEntry(phe); break;
                case ImmutableArray<R.ParamHashEntry> phes: WriteArray(WriteParamHashEntry, null, phes); break;

                case R.ParamKind paramKind: WriteParamKind(paramKind); break;
                case ImmutableArray<R.ParamKind> paramKinds: WriteArray(WriteParamKind, null, paramKinds); break;

                case R.AccessModifier am: WriteAccessModifier(am); break;
                case ImmutableArray<R.AccessModifier> ams: WriteArray(WriteAccessModifier, null, ams); break;

                case R.Path path: WritePath(path); break;
                case ImmutableArray<R.Path> paths: WriteArray(WritePath, "R.Path", paths); break;

                case R.TupleTypeElem tte: WriteTupleTypeElem(tte); break;
                case ImmutableArray<R.TupleTypeElem> ttes: WriteArray(WriteTupleTypeElem, null, ttes);  break;

                case R.Param param: WriteParam(param); break;
                case ImmutableArray<R.Param> parameters: WriteArray(WriteParam, null, parameters); break;

                case R.StructConstructorDecl scd: WriteStructConstructorDecl(scd); break;
                case ImmutableArray<R.StructConstructorDecl> scds: WriteArray(WriteStructConstructorDecl, null, scds); break;

                case R.StructMemberVarDecl smvd: WriteStructMemberVarDecl(smvd); break;
                case ImmutableArray<R.StructMemberVarDecl> smvds: WriteArray(WriteStructMemberVarDecl, null, smvds); break;

                case R.ConstructorBaseCallInfo cbci: WriteConstructorBaseCallinfo(cbci); break;
                case ImmutableArray<R.ConstructorBaseCallInfo> cbcis: WriteArray(WriteConstructorBaseCallinfo, null, cbcis); break;

                case R.ClassConstructorDecl ccd: WriteClassConstructorDecl(ccd); break;
                case ImmutableArray<R.ClassConstructorDecl> ccds: WriteArray(WriteClassConstructorDecl, null, ccds); break;

                case R.ClassMemberVarDecl cmvd: WriteClassMemberVarDecl(cmvd); break;
                case ImmutableArray<R.ClassMemberVarDecl> cmvds: WriteArray(WriteClassMemberVarDecl, null, cmvds); break;

                case R.EnumElement ee: WriteEnumElement(ee); break;
                case ImmutableArray<R.EnumElement> ees: WriteArray(WriteEnumElement, null, ees); break;

                case R.EnumElementField eef: WriteEnumElementField(eef); break;
                case ImmutableArray<R.EnumElementField> eefs: WriteArray(WriteEnumElementField, null, eefs); break;

                case R.CapturedStatement cs: WriteCapturedStatement(cs); break;
                case ImmutableArray<R.CapturedStatement> css: WriteArray(WriteCapturedStatement, null, css); break;

                case R.OuterLocalVarInfo olvi: WriteOuterLocalVarInfo(olvi); break;
                case ImmutableArray<R.OuterLocalVarInfo> olvis: WriteArray(WriteOuterLocalVarInfo, null, olvis); break;

                case R.ForStmtInitializer fsi: WriteForStmtInitializer(fsi); break;
                case ImmutableArray<R.ForStmtInitializer> fsis: WriteArray(WriteForStmtInitializer, "R.ForStmtInitializer", fsis); break;

                case R.LocalVarDecl lvd: WriteLocalVarDecl(lvd); break;
                case ImmutableArray<R.LocalVarDecl> lvds: WriteArray(WriteLocalVarDecl, null, lvds); break;

                case R.VarDeclElement vde: WriteVarDeclElement(vde); break;
                case ImmutableArray<R.VarDeclElement> vdes: WriteArray(WriteVarDeclElement, "R.VarDeclElement", vdes); break;

                case R.ReturnInfo ri: WriteReturnInfo(ri); break;
                case ImmutableArray<R.ReturnInfo> ris: WriteArray(WriteReturnInfo, "R.ReturnInfo", ris); break;

                case R.Argument arg: WriteArgument(arg); break;
                case ImmutableArray<R.Argument> args: WriteArray(WriteArgument, "R.Argument", args); break;

                case R.InternalBinaryOperator ibo: WriteInternalBinaryOperator(ibo); break;
                case ImmutableArray<R.InternalBinaryOperator> ibos: WriteArray(WriteInternalBinaryOperator, null, ibos); break;

                case R.InternalUnaryAssignOperator iuao: WriteInternalUnaryAssignOperator(iuao); break;
                case ImmutableArray<R.InternalUnaryAssignOperator> iuaos: WriteArray(WriteInternalUnaryAssignOperator, null, iuaos); break;

                case R.InternalUnaryOperator iuo: WriteInternalUnaryOperator(iuo); break;
                case ImmutableArray<R.InternalUnaryOperator> iuos: WriteArray(WriteInternalUnaryOperator, null, iuos); break;

                case R.StringExpElement see: WriteStringExpElement(see); break;
                case ImmutableArray<R.StringExpElement> sees: WriteArray(WriteStringExpElement, "R.StringExpElement", sees); break;

                //////

                case R.TypeDecl td: WriteTypeDecl(td); break;
                case ImmutableArray<R.TypeDecl> tds: WriteArray(WriteTypeDecl, "R.TypeDecl", tds); break;

                case R.FuncDecl fd: WriteFuncDecl(fd); break;
                case ImmutableArray<R.FuncDecl> fds: WriteArray(WriteFuncDecl, "R.FuncDecl", fds); break;

                case R.CallableMemberDecl cmd: WriteCallableMemberDecl(cmd); break;
                case ImmutableArray<R.CallableMemberDecl> cmds: WriteArray(WriteCallableMemberDecl, "R.CallableMemberDecl", cmds); break;

                case R.Stmt stmt: WriteStmt(stmt); break;
                case ImmutableArray<R.Stmt> stmts: WriteArray(WriteStmt, "R.Stmt", stmts); break;

                case R.Exp exp: WriteExp(exp); break;
                case ImmutableArray<R.Exp> exps: WriteArray(WriteExp, "R.Exp", exps); break;

                case R.Loc loc: WriteLoc(loc); break;
                case ImmutableArray<R.Loc> locs: WriteArray(WriteLoc, "R.Loc", locs); break;

                case string s: WriteString(s); break;
                case ImmutableArray<string> strs: WriteArray(WriteString, null, strs); break;

                case int i: WriteInt(i); break;
                case ImmutableArray<int> ints: WriteArray(WriteInt, null, ints); break;

                default:
                    throw new UnreachableCodeException();
            }
        }

        void WriteNew(string typeName, params object?[] objs)
        {
            isw.Write($"new {typeName}(");

            if (objs.Length == 0)
            {
                isw.Write(")");
                return;
            }
            else if (objs.Length == 1)
            {
                WriteObject(objs[0]);
                isw.Write(")");
                return;
            }
            else
            {
                var isw1 = isw.Push();
                var writer1 = new IR0Writer(isw1);

                bool bFirst = true;
                foreach (var obj in objs)
                {
                    if (bFirst) { bFirst = false; isw1.WriteLine(); }
                    else isw1.WriteLine(",");

                    writer1.WriteObject(obj);
                }

                isw.WriteLine();
                isw.Write(")");
            }

            
        }

        public static void WriteScript(StreamWriter sw, R.Script script)
        {
            var isw = new IndentedStreamWriter(sw);
            var writer = new IR0Writer(isw);

            writer.WriteNew(
                "R.Script", 
                script.GlobalTypeDecls, 
                script.GlobalFuncDecls,
                script.CallableMemberDecls,
                script.TopLevelStmts);
        }
    }
}
