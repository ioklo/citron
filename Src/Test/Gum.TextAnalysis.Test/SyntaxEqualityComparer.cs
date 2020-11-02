using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gum.Syntax;

namespace Gum
{
    class SyntaxEqualityComparer
        : IEqualityComparer<Script>
        , IEqualityComparer<Script.Element>
        , IEqualityComparer<StringExp>
        , IEqualityComparer<EnumDeclElement>
        , IEqualityComparer<TypeAndName>
        , IEqualityComparer<TypeExp>
        , IEqualityComparer<StructDecl.Element>
        , IEqualityComparer<VarDeclElement>
        , IEqualityComparer<StringExpElement>
        , IEqualityComparer<Stmt>
        , IEqualityComparer<Exp>
        , IEqualityComparer<LambdaExpParam>
        , IEqualityComparer<StructDecl>
        , IEqualityComparer<FuncDecl>
        , IEqualityComparer<EnumDecl>
    {
        public static SyntaxEqualityComparer Instance { get; } = new SyntaxEqualityComparer();
        private SyntaxEqualityComparer() { }

        bool IEqualityComparer<Script>.Equals([AllowNull] Script x, [AllowNull] Script y) => EqualsScript(x, y);
        int IEqualityComparer<Script>.GetHashCode([DisallowNull] Script obj) => throw new NotImplementedException();

        bool IEqualityComparer<Script.Element>.Equals([AllowNull] Script.Element x, [AllowNull] Script.Element y) => EqualsScriptElement(x, y);
        int IEqualityComparer<Script.Element>.GetHashCode([DisallowNull] Script.Element obj) => throw new NotImplementedException();

        bool IEqualityComparer<StringExp>.Equals([AllowNull] StringExp x, [AllowNull] StringExp y) => EqualsExp (x, y);
        int IEqualityComparer<StringExp>.GetHashCode([DisallowNull] StringExp obj) => throw new NotImplementedException();

        bool IEqualityComparer<EnumDeclElement>.Equals([AllowNull] EnumDeclElement x, [AllowNull] EnumDeclElement y) => EqualsEnumDeclElement(x, y);
        int IEqualityComparer<EnumDeclElement>.GetHashCode([DisallowNull] EnumDeclElement obj) => throw new NotImplementedException();

        bool IEqualityComparer<TypeAndName>.Equals([AllowNull] TypeAndName x, [AllowNull] TypeAndName y) => EqualsTypeAndName(x, y);
        int IEqualityComparer<TypeAndName>.GetHashCode([DisallowNull] TypeAndName obj) => throw new NotImplementedException();

        bool IEqualityComparer<TypeExp>.Equals([AllowNull] TypeExp x, [AllowNull] TypeExp y) => EqualsTypeExp(x, y);
        int IEqualityComparer<TypeExp>.GetHashCode([DisallowNull] TypeExp obj) => throw new NotImplementedException();

        bool IEqualityComparer<StructDecl.Element>.Equals([AllowNull] StructDecl.Element x, [AllowNull] StructDecl.Element y) => EqualsStructDeclElement(x, y);
        int IEqualityComparer<StructDecl.Element>.GetHashCode([DisallowNull] StructDecl.Element obj) => throw new NotImplementedException();

        bool IEqualityComparer<VarDeclElement>.Equals([AllowNull] VarDeclElement x, [AllowNull] VarDeclElement y) => EqualsVarDeclElement(x, y);
        int IEqualityComparer<VarDeclElement>.GetHashCode([DisallowNull] VarDeclElement obj) => throw new NotImplementedException();

        bool IEqualityComparer<StringExpElement>.Equals([AllowNull] StringExpElement x, [AllowNull] StringExpElement y) => EqualsStringExpElement(x, y);
        int IEqualityComparer<StringExpElement>.GetHashCode([DisallowNull] StringExpElement obj) => throw new NotImplementedException();

        bool IEqualityComparer<Stmt>.Equals([AllowNull] Stmt x, [AllowNull] Stmt y) => EqualsStmt(x, y);
        int IEqualityComparer<Stmt>.GetHashCode([DisallowNull] Stmt obj) => throw new NotImplementedException();

        bool IEqualityComparer<Exp>.Equals([AllowNull] Exp x, [AllowNull] Exp y) => EqualsExp(x, y);
        int IEqualityComparer<Exp>.GetHashCode([DisallowNull] Exp obj) => throw new NotImplementedException();

        bool IEqualityComparer<LambdaExpParam>.Equals([AllowNull] LambdaExpParam x, [AllowNull] LambdaExpParam y) => EqualsLambdaExpParam(x, y);
        int IEqualityComparer<LambdaExpParam>.GetHashCode([DisallowNull] LambdaExpParam obj) => throw new NotImplementedException();

        bool IEqualityComparer<StructDecl>.Equals([AllowNull] StructDecl x, [AllowNull] StructDecl y) => EqualsStructDecl(x, y);
        int IEqualityComparer<StructDecl>.GetHashCode([DisallowNull] StructDecl obj) => throw new NotImplementedException();

        bool IEqualityComparer<FuncDecl>.Equals([AllowNull] FuncDecl x, [AllowNull] FuncDecl y) => EqualsFuncDecl(x, y);
        int IEqualityComparer<FuncDecl>.GetHashCode([DisallowNull] FuncDecl obj) => throw new NotImplementedException();

        bool IEqualityComparer<EnumDecl>.Equals([AllowNull] EnumDecl x, [AllowNull] EnumDecl y) => EqualsEnumDecl(x, y);
        int IEqualityComparer<EnumDecl>.GetHashCode([DisallowNull] EnumDecl obj) => throw new NotImplementedException();


        bool EqualsScript([AllowNull] Script x, [AllowNull] Script y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Elements.SequenceEqual(y.Elements, this);
        }

        bool EqualsScriptElement([AllowNull] Script.Element x, [AllowNull] Script.Element y)
        {
            switch((x, y))
            {
                case (null, null): return true;
                case (Script.FuncDeclElement funcDeclElemX, Script.FuncDeclElement funcDeclElemY):
                    return EqualsFuncDecl(funcDeclElemX.FuncDecl, funcDeclElemY.FuncDecl);
                case (Script.StmtElement stmtElementX, Script.StmtElement stmtElementY):
                    return EqualsStmt(stmtElementX.Stmt, stmtElementY.Stmt);
                case (Script.EnumDeclElement enumDeclElementX, Script.EnumDeclElement enumDeclElementY):
                    return EqualsEnumDecl(enumDeclElementX.EnumDecl, enumDeclElementY.EnumDecl);
                case (Script.StructDeclElement structDeclElementX, Script.StructDeclElement structDeclElementY):
                    return EqualsStructDecl(structDeclElementX.StructDecl, structDeclElementY.StructDecl);
                default:
                    return false;
            }
        }

        bool EqualsFuncDecl([AllowNull] FuncDecl x, [AllowNull] FuncDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.IsSequence == y.IsSequence &&
                EqualsTypeExp(x.RetType, y.RetType) &&
                x.Name == y.Name &&
                x.TypeParams.SequenceEqual(y.TypeParams) &&
                EqualsFuncParamInfo(x.ParamInfo, y.ParamInfo) &&
                EqualsStmt(x.Body, y.Body);
        }

        bool EqualsStmt([AllowNull] Stmt x, [AllowNull] Stmt y)
        {
            switch((x, y))
            {
                case (null, null): 
                    return true;

                case (CommandStmt cmdStmtX, CommandStmt cmdStmtY):
                    return cmdStmtX.Commands.SequenceEqual(cmdStmtY.Commands, this);

                case (VarDeclStmt varDeclStmtX, VarDeclStmt varDeclStmtY):
                    return EqualsVarDecl(varDeclStmtX.VarDecl, varDeclStmtY.VarDecl);

                case (IfStmt ifStmtX, IfStmt ifStmtY):
                    return EqualsExp(ifStmtX.Cond, ifStmtY.Cond) &&
                        EqualsTypeExp(ifStmtX.TestType, ifStmtY.TestType) &&
                        EqualsStmt(ifStmtX.Body, ifStmtY.Body) &&
                        EqualsStmt(ifStmtX.ElseBody, ifStmtY.ElseBody);

                case (ForStmt forStmtX, ForStmt forStmtY):
                    return EqualsForStmtInitializer(forStmtX.Initializer, forStmtY.Initializer) &&
                        EqualsExp(forStmtX.CondExp, forStmtY.CondExp) &&
                        EqualsExp(forStmtX.ContinueExp, forStmtY.ContinueExp) &&
                        EqualsStmt(forStmtX.Body, forStmtY.Body);

                case (ContinueStmt _, ContinueStmt _):
                    return true;

                case (BreakStmt _, BreakStmt _):
                    return true;

                case (ReturnStmt returnStmtX, ReturnStmt returnStmtY):
                    return EqualsExp(returnStmtX.Value, returnStmtY.Value);

                case (BlockStmt blockStmtX, BlockStmt blockStmtY):
                    return blockStmtX.Stmts.SequenceEqual(blockStmtY.Stmts, this);

                case (BlankStmt blankStmtX, BlankStmt blankStmtY):
                    return true;

                case (ExpStmt expStmtX, ExpStmt expStmtY):
                    return EqualsExp(expStmtX.Exp, expStmtY.Exp);

                case (TaskStmt taskStmtX, TaskStmt taskStmtY):
                    return EqualsStmt(taskStmtX.Body, taskStmtY.Body);

                case (AwaitStmt awaitStmtX, AwaitStmt awaitStmtY):
                    return EqualsStmt(awaitStmtX.Body, awaitStmtY.Body);

                case (AsyncStmt asyncStmtX, AsyncStmt asyncStmtY):
                    return EqualsStmt(asyncStmtX.Body, asyncStmtY.Body);

                case (ForeachStmt foreachStmtX, ForeachStmt foreachStmtY):
                    return EqualsTypeExp(foreachStmtX.Type, foreachStmtY.Type) &&
                        foreachStmtX.VarName == foreachStmtY.VarName &&
                        EqualsExp(foreachStmtX.Iterator, foreachStmtY.Iterator) &&
                        EqualsStmt(foreachStmtX.Body, foreachStmtY.Body);

                case (YieldStmt yieldStmtX, YieldStmt yieldStmtY):
                    return EqualsExp(yieldStmtX.Value, yieldStmtY.Value);


                default:
                    return false;
            }
        }

        bool EqualsEnumDecl([AllowNull] EnumDecl x, [AllowNull] EnumDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Name == y.Name &&
                x.TypeParams.SequenceEqual(y.TypeParams) && // string이라서 this를 안붙임
                x.Elems.SequenceEqual(y.Elems, this);
        }

        bool EqualsEnumDeclElement([AllowNull] EnumDeclElement x, [AllowNull] EnumDeclElement y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Name == y.Name &&
                x.Params.SequenceEqual(y.Params, this);
        }

        bool EqualsTypeAndName([AllowNull] TypeAndName x, [AllowNull] TypeAndName y)
        {
            return EqualsTypeExp(x.Type, y.Type) &&
                x.Name == y.Name;
        }

        bool EqualsTypeExp([AllowNull] TypeExp x, [AllowNull] TypeExp y)
        {
            switch((x, y))
            {
                case (null, null): 
                    return true;

                case (IdTypeExp idTypeExpX, IdTypeExp idTypeExpY):
                    return idTypeExpX.Name == idTypeExpY.Name &&
                        idTypeExpX.TypeArgs.SequenceEqual(idTypeExpY.TypeArgs, this);

                case (MemberTypeExp memberTypeExpX, MemberTypeExp memberTypeExpY):
                    return EqualsTypeExp(memberTypeExpX.Parent, memberTypeExpY.Parent) &&
                        memberTypeExpX.MemberName == memberTypeExpY.MemberName &&
                        memberTypeExpX.TypeArgs.SequenceEqual(memberTypeExpY.TypeArgs, this);

                default:
                    return false;
            }
        }

        bool EqualsStructDecl([AllowNull] StructDecl x, [AllowNull] StructDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.AccessModifier == y.AccessModifier &&
                x.Name == y.Name &&
                x.TypeParams.SequenceEqual(y.TypeParams) &&
                x.BaseTypes.SequenceEqual(y.BaseTypes, this) &&
                x.Elems.SequenceEqual(y.Elems, this);
        }

        bool EqualsStructDeclElement([AllowNull] StructDecl.Element x, [AllowNull] StructDecl.Element y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (StructDecl.VarDeclElement varDeclElemX, StructDecl.VarDeclElement varDeclElemY):
                    return varDeclElemX.AccessModifier == varDeclElemY.AccessModifier &&
                        EqualsTypeExp(varDeclElemX.VarType, varDeclElemY.VarType) &&
                        varDeclElemX.VarNames.SequenceEqual(varDeclElemY.VarNames);

                case (StructDecl.FuncDeclElement funcDeclElemX, StructDecl.FuncDeclElement funcDeclElemY):
                    return funcDeclElemX.AccessModifier == funcDeclElemY.AccessModifier &&
                        funcDeclElemX.IsStatic == funcDeclElemY.IsStatic &&
                        funcDeclElemX.IsSequence == funcDeclElemY.IsSequence &&
                        EqualsTypeExp(funcDeclElemX.RetType, funcDeclElemY.RetType) &&
                        funcDeclElemX.Name == funcDeclElemY.Name &&
                        funcDeclElemX.TypeParams.SequenceEqual(funcDeclElemX.TypeParams) && // string이라서 this 안집어넣음
                        EqualsFuncParamInfo(funcDeclElemX.ParamInfo, funcDeclElemY.ParamInfo) &&
                        EqualsStmt(funcDeclElemX.Body, funcDeclElemY.Body);

                default:
                    return false;
            }
        }

        bool EqualsFuncParamInfo([AllowNull] FuncParamInfo x, [AllowNull] FuncParamInfo y)
        {
            return x.Parameters.SequenceEqual(y.Parameters, this) &&
                x.VariadicParamIndex == y.VariadicParamIndex;
        }

        bool EqualsVarDecl([AllowNull] VarDecl x, [AllowNull] VarDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return EqualsTypeExp(x.Type, y.Type) &&
                x.Elems.SequenceEqual(y.Elems, this);
        }

        bool EqualsVarDeclElement([AllowNull] VarDeclElement x, [AllowNull] VarDeclElement y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.VarName == y.VarName &&
                EqualsExp(x.InitExp, y.InitExp);
        }

        bool EqualsExp([AllowNull] Exp x, [AllowNull] Exp y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (IdentifierExp identifierExpX, IdentifierExp identifierExpY):
                    return identifierExpX.Value == identifierExpY.Value &&
                        identifierExpX.TypeArgs.SequenceEqual(identifierExpY.TypeArgs, this);

                case (StringExp stringExpX, StringExp stringExpY):
                    return stringExpX.Elements.SequenceEqual(stringExpY.Elements, this);

                case (IntLiteralExp intLiteralExpX, IntLiteralExp intLiteralExpY):
                    return intLiteralExpX.Value == intLiteralExpY.Value;

                case (BoolLiteralExp boolLiteralExpX, BoolLiteralExp boolLiteralExpY):
                    return boolLiteralExpX.Value == boolLiteralExpY.Value;

                case (BinaryOpExp binaryOpExpX, BinaryOpExp binaryOpExpY):
                    return binaryOpExpX.Kind == binaryOpExpY.Kind &&
                        EqualsExp(binaryOpExpX.Operand0, binaryOpExpY.Operand0) &&
                        EqualsExp(binaryOpExpX.Operand1, binaryOpExpY.Operand1);

                case (UnaryOpExp unaryOpExpX, UnaryOpExp unaryOpExpY):
                    return unaryOpExpX.Kind == unaryOpExpY.Kind &&
                        EqualsExp(unaryOpExpX.Operand, unaryOpExpY.Operand);

                case (CallExp callExpX, CallExp callExpY):
                    return EqualsExp(callExpX.Callable, callExpY.Callable) &&
                        callExpX.TypeArgs.SequenceEqual(callExpY.TypeArgs, this) &&
                        callExpX.Args.SequenceEqual(callExpY.Args, this);

                case (LambdaExp lambdaExpX, LambdaExp lambdaExpY):
                    return lambdaExpX.Params.SequenceEqual(lambdaExpY.Params, this) &&
                        EqualsStmt(lambdaExpX.Body, lambdaExpY.Body);

                case (IndexerExp indexerExpX, IndexerExp indexerExpY):
                    return EqualsExp(indexerExpX.Object, indexerExpY.Object) &&
                        EqualsExp(indexerExpX.Index, indexerExpY.Index);

                case (MemberCallExp memberCallExpX, MemberCallExp memberCallExpY):
                    return EqualsExp(memberCallExpX.Object, memberCallExpY.Object) &&
                        memberCallExpX.MemberName == memberCallExpY.MemberName &&
                        memberCallExpX.MemberTypeArgs.SequenceEqual(memberCallExpY.MemberTypeArgs, this) &&
                        memberCallExpX.Args.SequenceEqual(memberCallExpY.Args, this);

                case (MemberExp memberExpX, MemberExp memberExpY):
                    return EqualsExp(memberExpX.Object, memberExpY.Object) &&
                        memberExpX.MemberName == memberExpY.MemberName &&
                        memberExpX.MemberTypeArgs.SequenceEqual(memberExpY.MemberTypeArgs, this);

                case (ListExp listExpX, ListExp listExpY):
                    return EqualsTypeExp(listExpX.ElemType, listExpY.ElemType) &&
                        listExpX.Elems.SequenceEqual(listExpY.Elems, this);

                case (NewExp newExpX, NewExp newExpY):
                    return EqualsTypeExp(newExpX.Type, newExpY.Type) &&
                        newExpX.Args.SequenceEqual(newExpY.Args, this);

                default:
                    return false;
            }
        }

        bool EqualsStringExpElement([AllowNull] StringExpElement x, [AllowNull] StringExpElement y)
        {
            switch ((x, y))
            {
                case (null, null):
                    return true;

                case (TextStringExpElement textStringExpElemX, TextStringExpElement textStringExpElemY):
                    return textStringExpElemX.Text == textStringExpElemY.Text;

                case (ExpStringExpElement expStringExpElemX, ExpStringExpElement expStringExpElemY):
                    return EqualsExp(expStringExpElemX.Exp, expStringExpElemY.Exp);

                default:
                    return false;
            }
        }

        bool EqualsLambdaExpParam([AllowNull] LambdaExpParam x, [AllowNull] LambdaExpParam y)
        {
            return EqualsTypeExp(x.Type, y.Type) &&
                x.Name == y.Name;
        }

        bool EqualsForStmtInitializer([AllowNull] ForStmtInitializer x, [AllowNull] ForStmtInitializer y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (VarDeclForStmtInitializer varDeclForStmtInitX, VarDeclForStmtInitializer varDeclForStmtInitY):
                    return EqualsVarDecl(varDeclForStmtInitX.VarDecl, varDeclForStmtInitY.VarDecl);

                case (ExpForStmtInitializer expForStmtInitX, ExpForStmtInitializer expForStmtInitY):
                    return EqualsExp(expForStmtInitX.Exp, expForStmtInitY.Exp);                

                default:
                    return false;
            }
        }
    }
}