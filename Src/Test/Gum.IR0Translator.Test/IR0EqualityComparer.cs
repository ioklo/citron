using Gum.IR0;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Gum.Infra;
using System;
using System.Linq;

namespace Gum.IR0Translator.Test
{
    class IR0EqualityComparer 
        : IEqualityComparer<Script?>
        , IEqualityComparer<Stmt>
        , IEqualityComparer<Exp>
        , IEqualityComparer<StringExpElement>
        , IEqualityComparer<ExpInfo?> 
        , IEqualityComparer<ExpInfo>  // struct
        , IEqualityComparer<Type>     // struct
        , IEqualityComparer<VarDeclElement>
        , IEqualityComparer<LocalVarDecl>        
        , IEqualityComparer<ForStmtInitializer>
        , IEqualityComparer<CaptureInfo>
        , IEqualityComparer<CaptureInfo.Element>
        , IEqualityComparer<FuncDeclId>
        , IEqualityComparer<NewEnumExp.Elem>        
        , IEqualityComparer<TypeDecl>
    {
        public static readonly IR0EqualityComparer Instance = new IR0EqualityComparer();

        private IR0EqualityComparer() { }

        bool IEqualityComparer<Script?>.Equals([AllowNull] Script x, [AllowNull] Script y) => EqualsScript(x, y);
        int IEqualityComparer<Script?>.GetHashCode([DisallowNull] Script obj) => throw new NotImplementedException();
        bool IEqualityComparer<Stmt>.Equals([AllowNull] Stmt x, [AllowNull] Stmt y) => EqualsStmt(x, y);
        int IEqualityComparer<Stmt>.GetHashCode([DisallowNull] Stmt obj) => throw new NotImplementedException();
        bool IEqualityComparer<Exp>.Equals([AllowNull] Exp x, [AllowNull] Exp y) => EqualsExp(x, y);
        int IEqualityComparer<Exp>.GetHashCode([DisallowNull] Exp obj) => throw new NotImplementedException();
        bool IEqualityComparer<StringExpElement>.Equals([AllowNull] StringExpElement x, [AllowNull] StringExpElement y) => EqualsStringExpElement(x, y);
        int IEqualityComparer<StringExpElement>.GetHashCode([DisallowNull] StringExpElement obj) => throw new NotImplementedException();
        bool IEqualityComparer<ExpInfo?>.Equals([AllowNull] ExpInfo? x, [AllowNull] ExpInfo? y) => EqualsExpInfoOptional(x, y);
        int IEqualityComparer<ExpInfo?>.GetHashCode([DisallowNull] ExpInfo? obj) => throw new NotImplementedException();
        bool IEqualityComparer<ExpInfo>.Equals([AllowNull] ExpInfo x, [AllowNull] ExpInfo y) => EqualsExpInfo(x, y);  // struct
        int IEqualityComparer<ExpInfo>.GetHashCode([DisallowNull] ExpInfo obj) => throw new NotImplementedException();
        bool IEqualityComparer<Type>.Equals([AllowNull] Type x, [AllowNull] Type y) => EqualsType(x, y);     // struct
        int IEqualityComparer<Type>.GetHashCode([DisallowNull] Type obj) => throw new NotImplementedException();
        bool IEqualityComparer<VarDeclElement>.Equals([AllowNull] VarDeclElement x, [AllowNull] VarDeclElement y) => EqualsVarDeclElement(x, y);
        int IEqualityComparer<VarDeclElement>.GetHashCode([DisallowNull] VarDeclElement obj) => throw new NotImplementedException();
        bool IEqualityComparer<LocalVarDecl>.Equals([AllowNull] LocalVarDecl x, [AllowNull] LocalVarDecl y) => EqualsLocalVarDecl(x, y);
        int IEqualityComparer<LocalVarDecl>.GetHashCode([DisallowNull] LocalVarDecl obj) => throw new NotImplementedException();
        bool IEqualityComparer<ForStmtInitializer>.Equals([AllowNull] ForStmtInitializer x, [AllowNull] ForStmtInitializer y) => EqualsForStmtInitializer(x, y);
        int IEqualityComparer<ForStmtInitializer>.GetHashCode([DisallowNull] ForStmtInitializer obj) => throw new NotImplementedException();
        bool IEqualityComparer<CaptureInfo>.Equals([AllowNull] CaptureInfo x, [AllowNull] CaptureInfo y) => EqualsCaptureInfo(x, y);
        int IEqualityComparer<CaptureInfo>.GetHashCode([DisallowNull] CaptureInfo obj) => throw new NotImplementedException();
        bool IEqualityComparer<CaptureInfo.Element>.Equals([AllowNull] CaptureInfo.Element x, [AllowNull] CaptureInfo.Element y) => EqualsCaptureInfoElement(x, y);
        int IEqualityComparer<CaptureInfo.Element>.GetHashCode([DisallowNull] CaptureInfo.Element obj) => throw new NotImplementedException();
        bool IEqualityComparer<FuncDeclId>.Equals([AllowNull] FuncDeclId x, [AllowNull] FuncDeclId y) => EqualsFuncDeclId(x, y);
        int IEqualityComparer<FuncDeclId>.GetHashCode([DisallowNull] FuncDeclId obj) => throw new NotImplementedException();
        bool IEqualityComparer<NewEnumExp.Elem>.Equals([AllowNull] NewEnumExp.Elem x, [AllowNull] NewEnumExp.Elem y) => EqualsNewEnumExpElem(x, y);
        int IEqualityComparer<NewEnumExp.Elem>.GetHashCode([DisallowNull] NewEnumExp.Elem obj) => throw new NotImplementedException();
        bool IEqualityComparer<TypeDecl>.Equals([AllowNull] TypeDecl x, [AllowNull] TypeDecl y) => EqualsTypeDecl(x, y);
        int IEqualityComparer<TypeDecl>.GetHashCode([DisallowNull] TypeDecl obj) => throw new NotImplementedException();

        private static new bool Equals(object x, object y)
        {
            // 실수 방지
            throw new InvalidOperationException();
        }

        bool EqualsScript([AllowNull] Script x, [AllowNull] Script y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            if (x.TypeDecls.Length != y.TypeDecls.Length) return false;
            if (x.FuncDecls.Length != y.FuncDecls.Length) return false;
            if (x.TopLevelStmts.Length != y.TopLevelStmts.Length) return false;

            if (x.TypeDecls.Length != 0)
                throw new NotImplementedException();

            for (int i = 0; i < x.FuncDecls.Length; i++)
                if (!EqualsFuncDecl(x.FuncDecls[i], y.FuncDecls[i]))
                    return false;
            
            for (int i = 0; i < x.TopLevelStmts.Length; i++)
                if (!EqualsStmt(x.TopLevelStmts[i], y.TopLevelStmts[i]))
                    return false;

            return true;
        }        
        
        bool EqualsFuncDecl([AllowNull] FuncDecl x, [AllowNull] FuncDecl y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (FuncDecl.Normal normalX, FuncDecl.Normal normalY):
                    return EqualsFuncDeclId(normalX.Id, normalY.Id) &&
                        normalX.IsThisCall == normalY.IsThisCall &&
                        normalX.TypeParams.SequenceEqual(normalY.TypeParams) && // string이라 this 없이 호출
                        normalX.ParamNames.SequenceEqual(normalY.ParamNames) &&
                        EqualsStmt(normalX.Body, normalY.Body);

                case (FuncDecl.Sequence seqX, FuncDecl.Sequence seqY):
                    return EqualsFuncDeclId(seqX.Id, seqY.Id) &&
                        EqualsType(seqX.ElemType, seqX.ElemType) &&
                        seqX.IsThisCall == seqY.IsThisCall &&
                        seqX.TypeParams.SequenceEqual(seqY.TypeParams) && // string이라 this를 넣지 않는다
                        seqX.ParamNames.SequenceEqual(seqY.ParamNames) && // string이라 this를 넣지 않는다
                        EqualsStmt(seqX.Body, seqY.Body);

                default:
                    return false;
            }            
        }        
        
        bool EqualsStmt([AllowNull] Stmt x, [AllowNull] Stmt y)
        {
            switch ((x, y))
            {
                case (null, null):
                    return true;

                case (CommandStmt csx, CommandStmt csy):
                    return csx.Commands.SequenceEqual<Exp>(csy.Commands, this);

                case (PrivateGlobalVarDeclStmt px, PrivateGlobalVarDeclStmt py):
                    return px.Elems.SequenceEqual(py.Elems, this);

                case (LocalVarDeclStmt localVarDeclX, LocalVarDeclStmt localVarDeclY):
                    return EqualsLocalVarDecl(localVarDeclX.VarDecl, localVarDeclY.VarDecl);

                case (IfStmt ifStmtX, IfStmt ifStmtY):
                    return EqualsExp(ifStmtX.Cond, ifStmtY.Cond) &&
                        EqualsStmt(ifStmtX.Body, ifStmtY.Body) &&
                        EqualsStmt(ifStmtX.ElseBody, ifStmtY.ElseBody);

                case (IfTestClassStmt ifTestClassX, IfTestClassStmt ifTestClassY):
                    return EqualsExpInfo(ifTestClassX.Target, ifTestClassY.Target) &&
                        EqualsType(ifTestClassX.TestType, ifTestClassY.TestType) &&
                        EqualsStmt(ifTestClassX.Body, ifTestClassY.Body) &&
                        EqualsStmt(ifTestClassX.ElseBody, ifTestClassY.ElseBody);

                case (IfTestEnumStmt ifTestEnumStmtX, IfTestEnumStmt ifTestEnumStmtY):
                    return EqualsExpInfo(ifTestEnumStmtX.Target, ifTestEnumStmtY.Target) &&
                        ifTestEnumStmtX.ElemName == ifTestEnumStmtY.ElemName &&
                        EqualsStmt(ifTestEnumStmtX.Body, ifTestEnumStmtY.Body) &&
                        EqualsStmt(ifTestEnumStmtX.ElseBody, ifTestEnumStmtY.ElseBody);

                case (ForStmt forStmtX, ForStmt forStmtY):
                    return EqualsForStmtInitializer(forStmtX.Initializer, forStmtY.Initializer) &&
                        EqualsExp(forStmtX.CondExp, forStmtY.CondExp) &&
                        EqualsExpInfoOptional(forStmtX.ContinueInfo, forStmtY.ContinueInfo) &&
                        EqualsStmt(forStmtX.Body, forStmtY.Body);

                case (ContinueStmt continueStmtX, ContinueStmt continueStmtY):
                    return true;

                case (BreakStmt breakStmtX, BreakStmt breakStmtY):
                    return true;

                case (ReturnStmt returnStmtX, ReturnStmt returnStmtY):
                    return EqualsExp(returnStmtX.Value, returnStmtY.Value);

                case (BlockStmt blockStmtX, BlockStmt blockStmtY):
                    return blockStmtX.Stmts.SequenceEqual(blockStmtY.Stmts, this);

                case (BlankStmt blankStmtX, BlankStmt blankStmtY):
                    return true;

                case (ExpStmt expStmtX, ExpStmt expStmtY):
                    return EqualsExpInfo(expStmtX.ExpInfo, expStmtY.ExpInfo);

                case (TaskStmt taskStmtX, TaskStmt taskStmtY):
                    return EqualsStmt(taskStmtX.Body, taskStmtY.Body) &&
                        EqualsCaptureInfo(taskStmtX.CaptureInfo, taskStmtY.CaptureInfo);

                case (AwaitStmt awaitStmtX, AwaitStmt awaitStmtY):
                    return EqualsStmt(awaitStmtX.Body, awaitStmtY.Body);

                case (AsyncStmt asyncStmtX, AsyncStmt asyncStmtY):
                    return EqualsStmt(asyncStmtX.Body, asyncStmtY.Body) &&
                        EqualsCaptureInfo(asyncStmtX.CaptureInfo, asyncStmtY.CaptureInfo);

                case (ForeachStmt foreachStmtX, ForeachStmt foreachStmtY):
                    return EqualsType(foreachStmtX.ElemType, foreachStmtY.ElemType) &&
                        foreachStmtX.ElemName == foreachStmtY.ElemName &&
                        EqualsExpInfo(foreachStmtX.IteratorInfo, foreachStmtY.IteratorInfo) &&
                        EqualsStmt(foreachStmtX.Body, foreachStmtY.Body);

                case (YieldStmt yieldStmtX, YieldStmt yieldStmtY):
                    return EqualsExp(yieldStmtX.Value, yieldStmtY.Value);

                default:
                    return false;
            }
        }
        
        bool EqualsExp([AllowNull] Exp x, [AllowNull] Exp y)
        {
            switch ((x, y))
            {
                case (null, null):
                    return true;

                case (GlobalVarExp globalVarExpX, GlobalVarExp globalVarExpY):
                    return globalVarExpX.Name == globalVarExpY.Name;

                case (LocalVarExp localVarExpX, LocalVarExp localVarExpY):
                    return localVarExpX.Name == localVarExpY.Name;

                case (StringExp sx, StringExp sy):
                    return sx.Elements.SequenceEqual(sy.Elements, this);

                case (IntLiteralExp intLiteralExpX, IntLiteralExp intLiteralExpY):
                    return intLiteralExpX.Value == intLiteralExpY.Value;

                case (BoolLiteralExp boolLiteralExpX, BoolLiteralExp boolLiteralExpY):
                    return boolLiteralExpX.Value == boolLiteralExpY.Value;

                case (CallInternalUnaryOperatorExp callInternalUnaryOperatorExpX, CallInternalUnaryOperatorExp callInternalUnaryOperatorExpY):
                    return callInternalUnaryOperatorExpX.Operator == callInternalUnaryOperatorExpY.Operator &&
                        EqualsExpInfo(callInternalUnaryOperatorExpX.Operand, callInternalUnaryOperatorExpY.Operand);

                case (CallInternalUnaryAssignOperator callInternalUnaryAssignOperatorX, CallInternalUnaryAssignOperator callInternalUnaryAssignOperatorY):
                    return callInternalUnaryAssignOperatorX.Operator == callInternalUnaryAssignOperatorY.Operator &&
                        EqualsExp(callInternalUnaryAssignOperatorX.Operand, callInternalUnaryAssignOperatorY.Operand);

                case (CallInternalBinaryOperatorExp callInternalBinaryOperatorExpX, CallInternalBinaryOperatorExp callInternalBinaryOperatorExpY):
                    return callInternalBinaryOperatorExpX.Operator == callInternalBinaryOperatorExpY.Operator &&
                        EqualsExpInfo(callInternalBinaryOperatorExpX.Operand0, callInternalBinaryOperatorExpY.Operand0) &&
                        EqualsExpInfo(callInternalBinaryOperatorExpX.Operand1, callInternalBinaryOperatorExpY.Operand1);

                case (AssignExp assignExpX, AssignExp assignExpY):
                    return EqualsExp(assignExpX.Dest, assignExpY.Dest) &&
                        EqualsExp(assignExpX.Src, assignExpY.Src);

                case (CallFuncExp callFuncExpX, CallFuncExp callFuncExpY):
                    return EqualsFuncDeclId(callFuncExpX.FuncDeclId, callFuncExpY.FuncDeclId) && 
                        callFuncExpX.TypeArgs.SequenceEqual(callFuncExpY.TypeArgs, this) &&
                        EqualsExpInfoOptional(callFuncExpX.Instance, callFuncExpY.Instance) &&
                        callFuncExpX.Args.SequenceEqual(callFuncExpY.Args, this);

                case (CallSeqFuncExp callSeqFuncExpX, CallSeqFuncExp callSeqFuncExpY):
                    return EqualsFuncDeclId(callSeqFuncExpX.FuncDeclId, callSeqFuncExpY.FuncDeclId) &&
                        callSeqFuncExpX.TypeArgs.SequenceEqual(callSeqFuncExpY.TypeArgs, this) &&
                        EqualsExpInfoOptional(callSeqFuncExpX.Instance, callSeqFuncExpY.Instance) &&
                        callSeqFuncExpX.Args.SequenceEqual(callSeqFuncExpY.Args, this);

                case (CallValueExp callValueExpX, CallValueExp callValueExpY):
                    return EqualsExpInfo(callValueExpX.Callable, callValueExpY.Callable) &&
                        callValueExpX.Args.SequenceEqual(callValueExpY.Args, this);

                case (LambdaExp lambdaExpX, LambdaExp lambdaExpY):
                    return EqualsCaptureInfo(lambdaExpX.CaptureInfo, lambdaExpY.CaptureInfo) &&
                        lambdaExpX.ParamNames.SequenceEqual(lambdaExpY.ParamNames) && // string이므로 this를 넣지 않는다
                        EqualsStmt(lambdaExpX.Body, lambdaExpY.Body);

                case (ListIndexerExp listIndexerExpX, ListIndexerExp listIndexerExpY):
                    return EqualsExpInfo(listIndexerExpX.ListInfo, listIndexerExpY.ListInfo) &&
                        EqualsExpInfo(listIndexerExpX.IndexInfo, listIndexerExpY.IndexInfo);

                case (StaticMemberExp staticMemberExpX, StaticMemberExp staticMemberExpY):
                    return EqualsType(staticMemberExpX.Type, staticMemberExpY.Type) &&
                        staticMemberExpX.MemberName == staticMemberExpY.MemberName;

                case (StructMemberExp structMemberExpX, StructMemberExp structMemberExpY):
                    return EqualsExp(structMemberExpX.Instance, structMemberExpY.Instance) &&
                        structMemberExpX.MemberName == structMemberExpY.MemberName;

                case (ClassMemberExp classMemberExpX, ClassMemberExp classMemberExpY):
                    return EqualsExp(classMemberExpX.Instance, classMemberExpY.Instance) &&
                        classMemberExpX.MemberName == classMemberExpY.MemberName;

                case (EnumMemberExp enumMemberExpX, EnumMemberExp enumMemberExpY):
                    return EqualsExp(enumMemberExpX.Instance, enumMemberExpY.Instance) &&
                        enumMemberExpX.MemberName == enumMemberExpY.MemberName;

                case (ListExp listExpX, ListExp listExpY):
                    return EqualsType(listExpX.ElemType, listExpY.ElemType) &&
                        listExpX.Elems.SequenceEqual(listExpY.Elems, this);

                case (NewEnumExp newEnumExpX, NewEnumExp newEnumExpY):
                    return newEnumExpX.Name == newEnumExpY.Name &&
                        newEnumExpX.Members.SequenceEqual(newEnumExpY.Members, this);

                case (NewStructExp newStructExpX, NewStructExp newStructExpY):
                    return EqualsType(newStructExpX.Type, newStructExpY.Type) &&
                        newStructExpX.Args.SequenceEqual(newStructExpY.Args, this);

                case (NewClassExp newClassExpX, NewClassExp newClassExpY):
                    return EqualsType(newClassExpX.Type, newClassExpY.Type) &&
                        newClassExpX.Args.SequenceEqual(newClassExpY.Args, this);                

                default:
                    return false;
            }
        }
        
        bool EqualsStringExpElement([AllowNull] StringExpElement x, [AllowNull] StringExpElement y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (TextStringExpElement tx, TextStringExpElement ty):
                    return tx.Text == ty.Text;

                case (ExpStringExpElement ex, ExpStringExpElement ey):
                    return EqualsExp(ex.Exp, ey.Exp);

                default:
                    return false;
            }
        }
        
        bool EqualsExpInfoOptional([AllowNull] ExpInfo? x, [AllowNull] ExpInfo? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return EqualsExpInfo(x.Value, y.Value);
        }        

        bool EqualsType([AllowNull] Type x, [AllowNull] Type y)
        {
            return EqualsTypeDeclId(x.DeclId, y.DeclId) &&
                x.TypeArgs.SequenceEqual(y.TypeArgs, this);
        }

        bool EqualsTypeDeclId([AllowNull] TypeDeclId x, [AllowNull] TypeDeclId y)
        {
            return x.Value == y.Value;
        }

        bool EqualsVarDeclElement([AllowNull] VarDeclElement x, [AllowNull] VarDeclElement y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Name == y.Name &&
                EqualsType(x.Type, y.Type) &&
                EqualsExp(x.InitExp, y.InitExp);
        }

        bool EqualsLocalVarDecl([AllowNull] LocalVarDecl x, [AllowNull] LocalVarDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Elems.SequenceEqual(y.Elems, this);
        }        
        
        bool EqualsForStmtInitializer([AllowNull] ForStmtInitializer x, [AllowNull] ForStmtInitializer y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (ExpForStmtInitializer ex, ExpForStmtInitializer ey):
                    return EqualsExpInfo(ex.ExpInfo, ey.ExpInfo);

                case (VarDeclForStmtInitializer vx, VarDeclForStmtInitializer vy):
                    return EqualsLocalVarDecl(vx.VarDecl, vy.VarDecl);
            }

            return false;
        }
        
        bool EqualsCaptureInfo([AllowNull] CaptureInfo x, [AllowNull] CaptureInfo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.bShouldCaptureThis == y.bShouldCaptureThis &&
                x.Captures.SequenceEqual(y.Captures, this);
        }
        
        bool EqualsCaptureInfoElement([AllowNull] CaptureInfo.Element x, [AllowNull] CaptureInfo.Element y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return EqualsType(x.Type, y.Type) &&
                x.LocalVarName == y.LocalVarName;
        }

        bool EqualsExpInfo([AllowNull] ExpInfo x, [AllowNull] ExpInfo y)
        {
            return EqualsExp(x.Exp, y.Exp) && EqualsType(x.Type, y.Type);
        }
        
        bool EqualsFuncDeclId([AllowNull] FuncDeclId x, [AllowNull] FuncDeclId y)
        {
            return x.Value == y.Value;
        }        

        bool EqualsNewEnumExpElem([AllowNull] NewEnumExp.Elem x, [AllowNull] NewEnumExp.Elem y)
        {
            return x.Name == y.Name &&
                EqualsExpInfo(x.ExpInfo, y.ExpInfo);
        }
        
        bool EqualsTypeDecl([AllowNull] TypeDecl x, [AllowNull] TypeDecl y)
        {
            throw new NotImplementedException();
        }

        bool EqualsExternalGlobalVarId([AllowNull] ExternalGlobalVarId x, [AllowNull] ExternalGlobalVarId y)
        {
            throw new NotImplementedException();
        }
    }
}
