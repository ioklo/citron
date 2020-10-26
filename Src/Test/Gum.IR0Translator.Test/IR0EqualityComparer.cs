using Gum.IR0;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Gum.Infra;
using System;
using System.Linq;

namespace Gum
{
    class IR0EqualityComparer 
        : IEqualityComparer<Script?>
        , IEqualityComparer<Func>
        , IEqualityComparer<SeqFunc>
        , IEqualityComparer<Stmt>
        , IEqualityComparer<Exp>
        , IEqualityComparer<StringExpElement>
        , IEqualityComparer<ExpInfo?> 
        , IEqualityComparer<ExpInfo>  // struct
        , IEqualityComparer<TypeId>   // struct
        , IEqualityComparer<PrivateGlobalVarDeclStmt.Element>
        , IEqualityComparer<LocalVarDecl>
        , IEqualityComparer<LocalVarDecl.Element> // struct
        , IEqualityComparer<ForStmtInitializer>
        , IEqualityComparer<CaptureInfo>
        , IEqualityComparer<CaptureInfo.Element>        
        , IEqualityComparer<FuncId>
        , IEqualityComparer<SeqFuncId>
        , IEqualityComparer<NewEnumExp.Elem>        
    {
        public static IR0EqualityComparer Instance { get; } = new IR0EqualityComparer();

        private IR0EqualityComparer() { }

        private static new bool Equals(object x, object y)
        {
            // 실수 방지
            throw new InvalidOperationException();
        }

        public bool Equals([AllowNull] Script x, [AllowNull] Script y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            if (x.Types.Length != y.Types.Length) return false;
            if (x.Funcs.Length != y.Funcs.Length) return false;
            if (x.SeqFuncs.Length != y.SeqFuncs.Length) return false;
            if (x.TopLevelStmts.Length != y.TopLevelStmts.Length) return false;

            if (x.Types.Length != 0)
                throw new NotImplementedException();

            for (int i = 0; i < x.Funcs.Length; i++)
                if (!Equals(x.Funcs[i], y.Funcs[i]))
                    return false;

            for (int i = 0; i < x.SeqFuncs.Length; i++)
                if (!Equals(x.SeqFuncs[i], y.SeqFuncs[i]))
                    return false;
            
            for (int i = 0; i < x.TopLevelStmts.Length; i++)
                if (!Equals(x.TopLevelStmts[i], y.TopLevelStmts[i]))
                    return false;

            return true;
        }

        public int GetHashCode([DisallowNull] Script obj)
        {
            throw new System.NotImplementedException();
        }

        public bool Equals([AllowNull] Func x, [AllowNull] Func y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return Equals(x.Id, y.Id) &&
                x.IsThisCall == y.IsThisCall &&
                x.TypeParams.SequenceEqual(y.TypeParams) && // string이라 this 없이 호출
                x.ParamNames.SequenceEqual(y.ParamNames) &&
                Equals(x.Body, y.Body);
        }

        public int GetHashCode([DisallowNull] Func obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] SeqFunc x, [AllowNull] SeqFunc y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return Equals(x.Id, y.Id) &&
                Equals(x.ElemTypeId, y.ElemTypeId) &&
                x.IsThisCall == y.IsThisCall &&
                x.TypeParams.SequenceEqual(y.TypeParams) && // string이라 this를 넣지 않는다
                x.ParamNames.SequenceEqual(y.ParamNames) && // string이라 this를 넣지 않는다
                Equals(x.Body, y.Body);
        }

        public int GetHashCode([DisallowNull] SeqFunc obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] Stmt x, [AllowNull] Stmt y)
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
                    return Equals(localVarDeclX.VarDecl, localVarDeclY.VarDecl);

                case (IfStmt ifStmtX, IfStmt ifStmtY):
                    return Equals(ifStmtX.Cond, ifStmtY.Cond) &&
                        Equals(ifStmtX.Body, ifStmtY.Body) &&
                        Equals(ifStmtX.ElseBody, ifStmtY.ElseBody);

                case (IfTestClassStmt ifTestClassX, IfTestClassStmt ifTestClassY):
                    return Equals(ifTestClassX.Target, ifTestClassY.Target) &&
                        Equals(ifTestClassX.TestType, ifTestClassY.TestType) &&
                        Equals(ifTestClassX.Body, ifTestClassY.Body) &&
                        Equals(ifTestClassX.ElseBody, ifTestClassY.ElseBody);

                case (IfTestEnumStmt ifTestEnumStmtX, IfTestEnumStmt ifTestEnumStmtY):
                    return Equals(ifTestEnumStmtX.Target, ifTestEnumStmtY.Target) &&
                        ifTestEnumStmtX.ElemName == ifTestEnumStmtY.ElemName &&
                        Equals(ifTestEnumStmtX.Body, ifTestEnumStmtY.Body) &&
                        Equals(ifTestEnumStmtX.ElseBody, ifTestEnumStmtY.ElseBody);

                case (ForStmt forStmtX, ForStmt forStmtY):
                    return Equals(forStmtX.Initializer, forStmtY.Initializer) &&
                        Equals(forStmtX.CondExp, forStmtY.CondExp) &&
                        Equals(forStmtX.ContinueInfo, forStmtY.ContinueInfo) &&
                        Equals(forStmtX.Body, forStmtY.Body);

                case (ContinueStmt continueStmtX, ContinueStmt continueStmtY):
                    return true;

                case (BreakStmt breakStmtX, BreakStmt breakStmtY):
                    return true;

                case (ReturnStmt returnStmtX, ReturnStmt returnStmtY):
                    return Equals(returnStmtX.Value, returnStmtY.Value);

                case (BlockStmt blockStmtX, BlockStmt blockStmtY):
                    return blockStmtX.Stmts.SequenceEqual(blockStmtY.Stmts, this);

                case (BlankStmt blankStmtX, BlankStmt blankStmtY):
                    return true;

                case (ExpStmt expStmtX, ExpStmt expStmtY):
                    return Equals(expStmtX.ExpInfo, expStmtY.ExpInfo);

                case (TaskStmt taskStmtX, TaskStmt taskStmtY):
                    return Equals(taskStmtX.Body, taskStmtY.Body) &&
                        Equals(taskStmtX.CaptureInfo, taskStmtY.CaptureInfo);

                case (AwaitStmt awaitStmtX, AwaitStmt awaitStmtY):
                    return Equals(awaitStmtX.Body, awaitStmtY.Body);

                case (AsyncStmt asyncStmtX, AsyncStmt asyncStmtY):
                    return Equals(asyncStmtX.Body, asyncStmtY.Body) &&
                        Equals(asyncStmtX.CaptureInfo, asyncStmtY.CaptureInfo);

                case (ForeachStmt foreachStmtX, ForeachStmt foreachStmtY):
                    return Equals(foreachStmtX.ElemTypeId, foreachStmtY.ElemTypeId) &&
                        foreachStmtX.ElemName == foreachStmtY.ElemName &&
                        Equals(foreachStmtX.ObjInfo, foreachStmtY.ObjInfo) &&
                        Equals(foreachStmtX.Body, foreachStmtY.Body);

                case (YieldStmt yieldStmtX, YieldStmt yieldStmtY):
                    return Equals(yieldStmtX.Value, yieldStmtY.Value);

                default:
                    return false;
            }
        }

        public int GetHashCode([DisallowNull] Stmt obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] Exp x, [AllowNull] Exp y)
        {
            switch ((x, y))
            {
                case (null, null):
                    return true;

                case (ExternalGlobalVarExp externalGlobalVarExpX, ExternalGlobalVarExp externalGlobalVarExpY): 
                    return Equals(externalGlobalVarExpX.VarId, externalGlobalVarExpY.VarId);

                case (PrivateGlobalVarExp privateGlobalVarExpX, PrivateGlobalVarExp privateGlobalVarExpY):
                    return privateGlobalVarExpX.Name == privateGlobalVarExpY.Name;

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
                        Equals(callInternalUnaryOperatorExpX.Operand, callInternalUnaryOperatorExpY.Operand);

                case (CallInternalUnaryAssignOperator callInternalUnaryAssignOperatorX, CallInternalUnaryAssignOperator callInternalUnaryAssignOperatorY):
                    return callInternalUnaryAssignOperatorX.Operator == callInternalUnaryAssignOperatorY.Operator &&
                        Equals(callInternalUnaryAssignOperatorX.Operand, callInternalUnaryAssignOperatorY.Operand);

                case (CallInternalBinaryOperatorExp callInternalBinaryOperatorExpX, CallInternalBinaryOperatorExp callInternalBinaryOperatorExpY):
                    return callInternalBinaryOperatorExpX.Operator == callInternalBinaryOperatorExpY.Operator &&
                        Equals(callInternalBinaryOperatorExpX.Operand0, callInternalBinaryOperatorExpY.Operand0) &&
                        Equals(callInternalBinaryOperatorExpX.Operand1, callInternalBinaryOperatorExpY.Operand1);

                case (AssignExp assignExpX, AssignExp assignExpY):
                    return Equals(assignExpX.Dest, assignExpY.Dest) &&
                        Equals(assignExpX.Src, assignExpY.Src);

                case (CallFuncExp callFuncExpX, CallFuncExp callFuncExpY):
                    return Equals(callFuncExpX.FuncId, callFuncExpY.FuncId) && 
                        callFuncExpX.TypeArgs.SequenceEqual(callFuncExpY.TypeArgs, this) &&
                        Equals(callFuncExpX.Instance, callFuncExpY.Instance) &&
                        callFuncExpX.Args.SequenceEqual(callFuncExpY.Args, this);

                case (CallSeqFuncExp callSeqFuncExpX, CallSeqFuncExp callSeqFuncExpY):
                    return Equals(callSeqFuncExpX.SeqFuncId, callSeqFuncExpY.SeqFuncId) &&
                        callSeqFuncExpX.TypeArgs.SequenceEqual(callSeqFuncExpY.TypeArgs, this) &&
                        Equals(callSeqFuncExpX.Instance, callSeqFuncExpY.Instance) &&
                        callSeqFuncExpX.Args.SequenceEqual(callSeqFuncExpY.Args, this);

                case (CallValueExp callValueExpX, CallValueExp callValueExpY):
                    return Equals(callValueExpX.Callable, callValueExpY.Callable) &&
                        callValueExpX.Args.SequenceEqual(callValueExpY.Args, this);

                case (LambdaExp lambdaExpX, LambdaExp lambdaExpY):
                    return Equals(lambdaExpX.CaptureInfo, lambdaExpY.CaptureInfo) &&
                        lambdaExpX.ParamNames.SequenceEqual(lambdaExpY.ParamNames) && // string이므로 this를 넣지 않는다
                        Equals(lambdaExpX.Body, lambdaExpY.Body);

                case (ListIndexerExp listIndexerExpX, ListIndexerExp listIndexerExpY):
                    return Equals(listIndexerExpX.ListInfo, listIndexerExpY.ListInfo) &&
                        Equals(listIndexerExpX.IndexInfo, listIndexerExpY.IndexInfo);

                case (StaticMemberExp staticMemberExpX, StaticMemberExp staticMemberExpY):
                    return Equals(staticMemberExpX.TypeId, staticMemberExpY.TypeId) &&
                        staticMemberExpX.MemberName == staticMemberExpY.MemberName;

                case (StructMemberExp structMemberExpX, StructMemberExp structMemberExpY):
                    return Equals(structMemberExpX.Instance, structMemberExpY.Instance) &&
                        structMemberExpX.MemberName == structMemberExpY.MemberName;

                case (ClassMemberExp classMemberExpX, ClassMemberExp classMemberExpY):
                    return Equals(classMemberExpX.Instance, classMemberExpY.Instance) &&
                        classMemberExpX.MemberName == classMemberExpY.MemberName;

                case (EnumMemberExp enumMemberExpX, EnumMemberExp enumMemberExpY):
                    return Equals(enumMemberExpX.Instance, enumMemberExpY.Instance) &&
                        enumMemberExpX.MemberName == enumMemberExpY.MemberName;

                case (ListExp listExpX, ListExp listExpY):
                    return Equals(listExpX.ElemTypeId, listExpY.ElemTypeId) &&
                        listExpX.Elems.SequenceEqual(listExpY.Elems, this);

                case (NewEnumExp newEnumExpX, NewEnumExp newEnumExpY):
                    return newEnumExpX.Name == newEnumExpY.Name &&
                        newEnumExpX.Members.SequenceEqual(newEnumExpY.Members, this);

                case (NewStructExp newStructExpX, NewStructExp newStructExpY):
                    return Equals(newStructExpX.TypeId, newStructExpY.TypeId) &&
                        newStructExpX.Args.SequenceEqual(newStructExpY.Args, this);

                case (NewClassExp newClassExpX, NewClassExp newClassExpY):
                    return Equals(newClassExpX.TypeId, newClassExpY.TypeId) &&
                        newClassExpX.Args.SequenceEqual(newClassExpY.Args, this);

                

                default:
                    return false;
            }
        }

        public int GetHashCode([DisallowNull] Exp obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] StringExpElement x, [AllowNull] StringExpElement y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (TextStringExpElement tx, TextStringExpElement ty):
                    return tx.Text == ty.Text;

                case (ExpStringExpElement ex, ExpStringExpElement ey):
                    return Equals(ex.Exp, ey.Exp);

                default:
                    return false;
            }
        }

        public int GetHashCode([DisallowNull] StringExpElement obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] ExpInfo? x, [AllowNull] ExpInfo? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return Equals(x.Value, y.Value);
        }

        public int GetHashCode([DisallowNull] ExpInfo? obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] TypeId x, [AllowNull] TypeId y)
        {
            return x.Value == y.Value;
        }

        public int GetHashCode([DisallowNull] TypeId obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] PrivateGlobalVarDeclStmt.Element x, [AllowNull] PrivateGlobalVarDeclStmt.Element y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Name == y.Name &&
                Equals(x.TypeId, y.TypeId) &&
                Equals(x.InitExp, y.InitExp);
        }

        public int GetHashCode([DisallowNull] PrivateGlobalVarDeclStmt.Element obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] LocalVarDecl x, [AllowNull] LocalVarDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Elems.SequenceEqual(y.Elems, this);
        }

        public int GetHashCode([DisallowNull] LocalVarDecl obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] LocalVarDecl.Element x, [AllowNull] LocalVarDecl.Element y)
        {
            return x.Name == y.Name &&
                Equals(x.TypeId, y.TypeId) &&
                Equals(x.InitExp, y.InitExp);
        }

        public int GetHashCode([DisallowNull] LocalVarDecl.Element obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] ForStmtInitializer x, [AllowNull] ForStmtInitializer y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (ExpForStmtInitializer ex, ExpForStmtInitializer ey):
                    return Equals(ex.ExpInfo, ey.ExpInfo);

                case (VarDeclForStmtInitializer vx, VarDeclForStmtInitializer vy):
                    return Equals(vx.VarDecl, vy.VarDecl);
            }

            return false;
        }

        public int GetHashCode([DisallowNull] ForStmtInitializer obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] CaptureInfo x, [AllowNull] CaptureInfo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.bShouldCaptureThis == y.bShouldCaptureThis &&
                x.Captures.SequenceEqual(y.Captures, this);
        }

        public int GetHashCode([DisallowNull] CaptureInfo obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] CaptureInfo.Element x, [AllowNull] CaptureInfo.Element y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return Equals(x.TypeId, y.TypeId) &&
                x.LocalVarName == y.LocalVarName;
        }

        public int GetHashCode([DisallowNull] CaptureInfo.Element obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] ExpInfo x, [AllowNull] ExpInfo y)
        {
            return Equals(x.Exp, y.Exp) && Equals(x.TypeId, y.TypeId);
        }

        public int GetHashCode([DisallowNull] ExpInfo obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] FuncId x, [AllowNull] FuncId y)
        {
            return x.Value == y.Value;
        }

        public int GetHashCode([DisallowNull] FuncId obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] SeqFuncId x, [AllowNull] SeqFuncId y)
        {
            return x.Value == y.Value;
        }

        public int GetHashCode([DisallowNull] SeqFuncId obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] NewEnumExp.Elem x, [AllowNull] NewEnumExp.Elem y)
        {
            return x.Name == y.Name &&
                Equals(x.ExpInfo, y.ExpInfo);
        }

        public int GetHashCode([DisallowNull] NewEnumExp.Elem obj)
        {
            throw new NotImplementedException();
        }        
    }
}
