using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gum.Misc;
using Gum.Syntax;

namespace Gum.TextAnalysis.Test
{
    class SyntaxEqualityComparer
        : IEqualityComparer<Script>
        , IEqualityComparer<ScriptElement>
        , IEqualityComparer<StringExp>
        , IEqualityComparer<EnumDeclElement>
        , IEqualityComparer<TypeAndName>
        , IEqualityComparer<TypeExp>
        , IEqualityComparer<StructDeclElement>
        , IEqualityComparer<VarDeclElement>
        , IEqualityComparer<StringExpElement>
        , IEqualityComparer<Stmt>
        , IEqualityComparer<Exp>
        , IEqualityComparer<LambdaExpParam>
        , IEqualityComparer<FuncDecl>
        , IEqualityComparer<TypeDecl>
    {
        public static readonly SyntaxEqualityComparer Instance = new SyntaxEqualityComparer();
        private SyntaxEqualityComparer() { }

        bool IEqualityComparer<Script>.Equals([AllowNull] Script x, [AllowNull] Script y) => EqualsScript(x, y);
        int IEqualityComparer<Script>.GetHashCode([DisallowNull] Script obj) => throw new NotImplementedException();

        bool IEqualityComparer<ScriptElement>.Equals([AllowNull] ScriptElement x, [AllowNull] ScriptElement y) => EqualsScriptElement(x, y);
        int IEqualityComparer<ScriptElement>.GetHashCode([DisallowNull] ScriptElement obj) => throw new NotImplementedException();

        bool IEqualityComparer<StringExp>.Equals([AllowNull] StringExp x, [AllowNull] StringExp y) => EqualsExp (x, y);
        int IEqualityComparer<StringExp>.GetHashCode([DisallowNull] StringExp obj) => throw new NotImplementedException();

        bool IEqualityComparer<EnumDeclElement>.Equals([AllowNull] EnumDeclElement x, [AllowNull] EnumDeclElement y) => EqualsEnumDeclElement(x, y);
        int IEqualityComparer<EnumDeclElement>.GetHashCode([DisallowNull] EnumDeclElement obj) => throw new NotImplementedException();

        bool IEqualityComparer<TypeAndName>.Equals([AllowNull] TypeAndName x, [AllowNull] TypeAndName y) => EqualsTypeAndName(x, y);
        int IEqualityComparer<TypeAndName>.GetHashCode([DisallowNull] TypeAndName obj) => throw new NotImplementedException();

        bool IEqualityComparer<TypeExp>.Equals([AllowNull] TypeExp x, [AllowNull] TypeExp y) => EqualsTypeExp(x, y);
        int IEqualityComparer<TypeExp>.GetHashCode([DisallowNull] TypeExp obj) => throw new NotImplementedException();

        bool IEqualityComparer<StructDeclElement>.Equals([AllowNull] StructDeclElement x, [AllowNull] StructDeclElement y) => EqualsStructDeclElement(x, y);
        int IEqualityComparer<StructDeclElement>.GetHashCode([DisallowNull] StructDeclElement obj) => throw new NotImplementedException();

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
        
        bool IEqualityComparer<FuncDecl>.Equals([AllowNull] FuncDecl x, [AllowNull] FuncDecl y) => EqualsFuncDecl(x, y);
        int IEqualityComparer<FuncDecl>.GetHashCode([DisallowNull] FuncDecl obj) => throw new NotImplementedException();

        bool IEqualityComparer<TypeDecl>.Equals([AllowNull] TypeDecl x, [AllowNull] TypeDecl y) => EqualsTypeDecl(x, y);
        int IEqualityComparer<TypeDecl>.GetHashCode([DisallowNull] TypeDecl obj) => throw new NotImplementedException();

        public static bool EqualsScript([AllowNull] Script x, [AllowNull] Script y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return SequenceEqual(x.Elements, y.Elements, Instance);
        }

        public static bool EqualsScriptElement([AllowNull] ScriptElement x, [AllowNull] ScriptElement y)
        {
            switch((x, y))
            {
                case (null, null): return true;
                case (GlobalFuncDeclScriptElement funcDeclElemX, GlobalFuncDeclScriptElement funcDeclElemY):
                    return EqualsFuncDecl(funcDeclElemX.FuncDecl, funcDeclElemY.FuncDecl);
                case (StmtScriptElement stmtElementX, StmtScriptElement stmtElementY):
                    return EqualsStmt(stmtElementX.Stmt, stmtElementY.Stmt);
                case (TypeDeclScriptElement typeDeclElementX, TypeDeclScriptElement typeDeclElementY):
                    return EqualsTypeDecl(typeDeclElementX.TypeDecl, typeDeclElementY.TypeDecl);                
                default:
                    throw new UnreachableCodeException();
            }
        }

        public static bool EqualsFuncDecl([AllowNull] FuncDecl x, [AllowNull] FuncDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.IsSequence == y.IsSequence &&
                EqualsTypeExp(x.RetType, y.RetType) &&
                x.Name == y.Name &&
                SequenceEqual(x.TypeParams, y.TypeParams) &&
                EqualsFuncParamInfo(x.ParamInfo, y.ParamInfo) &&
                EqualsStmt(x.Body, y.Body);
        }

        public static bool EqualsStmt([AllowNull] Stmt x, [AllowNull] Stmt y)
        {
            switch((x, y))
            {
                case (null, null): 
                    return true;

                case (CommandStmt cmdStmtX, CommandStmt cmdStmtY):
                    return SequenceEqual(cmdStmtX.Commands, cmdStmtY.Commands, Instance);

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
                    return SequenceEqual(blockStmtX.Stmts, blockStmtY.Stmts, Instance);

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
                    throw new UnreachableCodeException();
            }
        }

        public static bool EqualsTypeDecl([AllowNull] TypeDecl x, [AllowNull] TypeDecl y)
        {
            switch((x, y))
            {
                case (null, null): 
                    return true;

                case (EnumDecl enumDeclX, EnumDecl enumDeclY):
                    return enumDeclX.Name == enumDeclY.Name &&
                    SequenceEqual(enumDeclX.TypeParams, enumDeclY.TypeParams) && // string이라서 Instance를 안붙임
                    SequenceEqual(enumDeclX.Elems, enumDeclY.Elems, Instance);

                case (StructDecl structDeclX, StructDecl structDeclY):
                    return structDeclX.AccessModifier == structDeclY.AccessModifier &&
                        structDeclX.Name == structDeclY.Name &&
                        SequenceEqual(structDeclX.TypeParams, structDeclY.TypeParams) &&
                        SequenceEqual(structDeclX.BaseTypes, structDeclY.BaseTypes, Instance) &&
                        SequenceEqual(structDeclX.Elems, structDeclY.Elems, Instance);

                default:
                    throw new UnreachableCodeException();
            }
        }

        public static bool EqualsEnumDeclElement([AllowNull] EnumDeclElement x, [AllowNull] EnumDeclElement y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Name == y.Name &&
                SequenceEqual(x.Params, y.Params, Instance);
        }

        public static bool EqualsTypeAndName([AllowNull] TypeAndName x, [AllowNull] TypeAndName y)
        {
            return EqualsTypeExp(x.Type, y.Type) &&
                x.Name == y.Name;
        }

        public static bool EqualsTypeExp([AllowNull] TypeExp x, [AllowNull] TypeExp y)
        {
            switch((x, y))
            {
                case (null, null): 
                    return true;

                case (IdTypeExp idTypeExpX, IdTypeExp idTypeExpY):
                    return idTypeExpX.Name == idTypeExpY.Name &&
                        SequenceEqual(idTypeExpX.TypeArgs, idTypeExpY.TypeArgs, Instance);

                case (MemberTypeExp memberTypeExpX, MemberTypeExp memberTypeExpY):
                    return EqualsTypeExp(memberTypeExpX.Parent, memberTypeExpY.Parent) &&
                        memberTypeExpX.MemberName == memberTypeExpY.MemberName &&
                        SequenceEqual(memberTypeExpX.TypeArgs, memberTypeExpY.TypeArgs, Instance);

                default:
                    throw new UnreachableCodeException();
            }
        }

        public static bool EqualsStructFuncDecl(StructFuncDecl? x, StructFuncDecl? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.AccessModifier == y.AccessModifier &&
                x.IsStatic == y.IsStatic &&
                x.IsSequence == y.IsSequence &&
                EqualsTypeExp(x.RetType, y.RetType) &&
                x.Name == y.Name &&
                SequenceEqual(x.TypeParams, y.TypeParams) && // string이라서 Instance 안집어넣음
                EqualsFuncParamInfo(x.ParamInfo, y.ParamInfo) &&
                EqualsStmt(x.Body, y.Body);
        }

        public static bool EqualsStructDeclElement([AllowNull] StructDeclElement x, [AllowNull] StructDeclElement y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (TypeStructDeclElement typeDeclElemX, TypeStructDeclElement typeDeclElemY):
                    return EqualsTypeDecl(typeDeclElemX.TypeDecl, typeDeclElemY.TypeDecl);

                case (VarStructDeclElement varDeclElemX, VarStructDeclElement varDeclElemY):
                    return varDeclElemX.AccessModifier == varDeclElemY.AccessModifier &&
                        EqualsTypeExp(varDeclElemX.VarType, varDeclElemY.VarType) &&
                        SequenceEqual(varDeclElemX.VarNames, varDeclElemY.VarNames);

                case (FuncStructDeclElement funcDeclElemX, FuncStructDeclElement funcDeclElemY):
                    return EqualsStructFuncDecl(funcDeclElemX.FuncDecl, funcDeclElemY.FuncDecl);

                default:
                    throw new UnreachableCodeException();
            }
        }

        public static bool EqualsFuncParamInfo([AllowNull] FuncParamInfo x, [AllowNull] FuncParamInfo y)
        {
            return SequenceEqual(x.Parameters, y.Parameters, Instance) &&
                x.VariadicParamIndex == y.VariadicParamIndex;
        }

        public static bool EqualsVarDecl([AllowNull] VarDecl x, [AllowNull] VarDecl y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return EqualsTypeExp(x.Type, y.Type) &&
                SequenceEqual(x.Elems, y.Elems, Instance);
        }

        public static bool EqualsVarDeclElement([AllowNull] VarDeclElement x, [AllowNull] VarDeclElement y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.VarName == y.VarName &&
                EqualsExp(x.InitExp, y.InitExp);
        }

        public static bool EqualsExp([AllowNull] Exp x, [AllowNull] Exp y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (IdentifierExp identifierExpX, IdentifierExp identifierExpY):
                    return identifierExpX.Value == identifierExpY.Value &&
                        SequenceEqual(identifierExpX.TypeArgs, identifierExpY.TypeArgs, Instance);

                case (StringExp stringExpX, StringExp stringExpY):
                    return SequenceEqual(stringExpX.Elements, stringExpY.Elements, Instance);

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
                        SequenceEqual(callExpX.Args, callExpY.Args, Instance);

                case (LambdaExp lambdaExpX, LambdaExp lambdaExpY):
                    return SequenceEqual(lambdaExpX.Params, lambdaExpY.Params, Instance) &&
                        EqualsStmt(lambdaExpX.Body, lambdaExpY.Body);

                case (IndexerExp indexerExpX, IndexerExp indexerExpY):
                    return EqualsExp(indexerExpX.Object, indexerExpY.Object) &&
                        EqualsExp(indexerExpX.Index, indexerExpY.Index);

                case (MemberCallExp memberCallExpX, MemberCallExp memberCallExpY):
                    return EqualsExp(memberCallExpX.Object, memberCallExpY.Object) &&
                        memberCallExpX.MemberName == memberCallExpY.MemberName &&
                        SequenceEqual(memberCallExpX.MemberTypeArgs, memberCallExpY.MemberTypeArgs, Instance) &&
                        SequenceEqual(memberCallExpX.Args, memberCallExpY.Args, Instance);

                case (MemberExp memberExpX, MemberExp memberExpY):
                    return EqualsExp(memberExpX.Parent, memberExpY.Parent) &&
                        memberExpX.MemberName == memberExpY.MemberName &&
                        SequenceEqual(memberExpX.MemberTypeArgs, memberExpY.MemberTypeArgs, Instance);

                case (ListExp listExpX, ListExp listExpY):
                    return EqualsTypeExp(listExpX.ElemType, listExpY.ElemType) &&
                        SequenceEqual(listExpX.Elems, listExpY.Elems, Instance);

                case (NewExp newExpX, NewExp newExpY):
                    return EqualsTypeExp(newExpX.Type, newExpY.Type) &&
                        SequenceEqual(newExpX.Args, newExpY.Args, Instance);

                default:
                    throw new UnreachableCodeException();
            }
        }

        public static bool EqualsStringExpElement([AllowNull] StringExpElement x, [AllowNull] StringExpElement y)
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
                    throw new UnreachableCodeException();
            }
        }

        public static bool EqualsLambdaExpParam([AllowNull] LambdaExpParam x, [AllowNull] LambdaExpParam y)
        {
            return EqualsTypeExp(x.Type, y.Type) &&
                x.Name == y.Name;
        }

        public static bool EqualsForStmtInitializer([AllowNull] ForStmtInitializer x, [AllowNull] ForStmtInitializer y)
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
                    throw new UnreachableCodeException();
            }
        }

        static bool SequenceEqual<TElem>(ImmutableArray<TElem> x, ImmutableArray<TElem> y)
            => SequenceEqual<TElem>(x, y, EqualityComparer<TElem>.Default);

        static bool SequenceEqual<TElem>(ImmutableArray<TElem> x, ImmutableArray<TElem> y, IEqualityComparer<TElem> comparer)
        {
            if (x.IsDefaultOrEmpty && y.IsDefaultOrEmpty) return true;
            if (x.IsDefaultOrEmpty || y.IsDefaultOrEmpty) return false;

            return ImmutableArrayExtensions.SequenceEqual(x, y, comparer);
        }
    }
}