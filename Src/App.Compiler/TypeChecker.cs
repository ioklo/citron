using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.AbstractSyntax;
using Gum.App.Compiler.Exception;

using Domain = Gum.Core.IL.Domain;
using Gum.Core.IL;
using Gum.Core.Runtime;

namespace Gum.App.Compiler
{    
    // Making type information and checking expression's type
    class TypeChecker : IStmtVisitor, IFuncStmtVisitor, IREPLStmtVisitor
    {
        class ExpTypeChecker : IExpVisitor
        {
            IType expType;
            TypeCheckerContext ctx;

            public static IType TypeCheckExp(IExp exp, TypeCheckerContext ctx)
            {
                var typeChecker = new ExpTypeChecker(ctx);
                exp.Visit(typeChecker);
                return typeChecker.expType;
            }

            public ExpTypeChecker(TypeCheckerContext ctx)
            {
                this.ctx = ctx;
            }

            // assignExp a.b.c = 1;
            // abstract syntax만 보고 판별하기
            public void Visit(AssignExp exp)
            {
                throw new NotImplementedException();
                //if (!ctx.TryGetVarType(exp.Left.Name, out type))
                //    throw new LocalVarNotDeclaredException();

                //foreach (var offset in exp.Left.Offsets)
                //{
                //    RefType refType = type as RefType;
                //    if (refType == null)
                //        throw new InvalidOperationException();

                //    var stringOffset = offset as StringOffset;
                //    if (stringOffset == null)
                //        throw new NotImplementedException();

                //    exp.Left.IndexOffsets.Add(refType.Fields[stringOffset.Field].Index);
                //    type = refType.Fields[stringOffset.Field].TypeInfo;
                //}

                //var expType = exp.Exp.Visit(this);

                //if (type != expType)
                //    throw new TypeMismatchException();

                //return type;
            }

            public void Visit(VariableExp exp)
            {
                throw new NotImplementedException();
                //IType varType;

                //if (!ctx.TryGetVarType(exp.Name, out varType))
                //    throw new LocalVarNotDeclaredException();

                //foreach (var offset in exp.Offsets)
                //{
                //    var stringOffset = offset as StringOffset;
                //    if (stringOffset == null)
                //        throw new NotImplementedException();

                //    exp.IndexOffsets.Add(varType.Fields[stringOffset.Field].Index);
                //    varType = varType.Fields[stringOffset.Field].TypeInfo;
                //}

                //return varType;
            }

            public void Visit(IntegerExp exp)
            {
                expType = GlobalDomain.IntType;
            }

            public void Visit(StringExp exp)
            {
                expType = GlobalDomain.StringType;
            }

            public void Visit(BoolExp exp)
            {
                expType = GlobalDomain.BoolType;
            }

            public void Visit(BinaryExp exp)
            {
                var type1 = TypeCheckExp(exp.Operand1, ctx);
                var type2 = TypeCheckExp(exp.Operand2, ctx);

                switch (exp.Operation)
                {
                    // 동치 비교
                    case BinaryExpKind.Equal:
                    case BinaryExpKind.NotEqual:
                        if (type1 != type2 ||
                            (type1 != ctx.TypeManager.IntType && type1 != ctx.TypeManager.StringType && type1 != ctx.TypeManager.VoidType))
                            throw new TypeMismatchException();
                        expType = ctx.TypeManager.BoolType;
                        return;

                    // Complete Order 비교
                    case BinaryExpKind.Less:
                    case BinaryExpKind.Greater:
                    case BinaryExpKind.LessEqual:
                    case BinaryExpKind.GreaterEqual:
                        if (type1 != type2 ||
                            (type1 != ctx.TypeManager.IntType && type1 != ctx.TypeManager.StringType))
                            throw new TypeMismatchException();
                        expType = ctx.TypeManager.BoolType;
                        return;

                    // boolean 
                    case BinaryExpKind.And:
                    case BinaryExpKind.Or:
                        if (type1 != type2 || type1 != ctx.TypeManager.BoolType)
                            throw new TypeMismatchException();
                        expType = type1;
                        return;

                    case BinaryExpKind.Add:
                        if (type1 != type2 ||
                            (type1 != ctx.TypeManager.IntType && type1 != ctx.TypeManager.StringType))
                            throw new TypeMismatchException();
                        expType = type1;
                        return;

                    case BinaryExpKind.Sub:
                    case BinaryExpKind.Mul:
                    case BinaryExpKind.Div:
                    case BinaryExpKind.Mod:
                        if (type1 != type2 || type1 != ctx.TypeManager.IntType)
                            throw new TypeMismatchException();
                        expType = type1;
                        return;
                }

                throw new NotImplementedException();
            }

            public void Visit(UnaryExp exp)
            {
                switch (exp.Operation)
                {
                    case UnaryExpKind.Neg:
                    case UnaryExpKind.PostfixInc:
                    case UnaryExpKind.PostfixDec:
                    case UnaryExpKind.PrefixInc:
                    case UnaryExpKind.PrefixDec:
                        if (exp.Operand.Visit(this) != ctx.TypeManager.IntType)
                            throw new TypeMismatchException();
                        expType = ctx.TypeManager.IntType;
                        return;

                    case UnaryExpKind.Not:
                        if (exp.Operand.Visit(this) != ctx.TypeManager.BoolType)
                            throw new TypeMismatchException();
                        expType = ctx.TypeManager.BoolType;
                        return;
                }

                throw new NotImplementedException();
            }

            public void Visit(NewExp exp)
            {
                expType = ctx.TypeManager.Types[exp.TypeName];
            }

            public void Visit(CallExp exp)
            {
                FuncDecl decl = ctx.GetFunc(exp.FuncName);

                if (decl == null)
                    throw new FuncNotDeclaredException();

                if (decl.Parameters.Count != exp.Args.Count)
                    throw new System.Exception("인자 개수가 맞지 않습니다");

                for (int t = 0; t < decl.Parameters.Count; t++)
                {
                    if (exp.Args[t].Visit(this) != ctx.TypeManager.Types[decl.Parameters[t].Type.Value])
                        throw new TypeMismatchException();
                }

                expType = ctx.TypeManager.Types[decl.ReturnType.Value];
            }
        }      

        

        public TypeChecker(Domain domain)
        {
            ctx = new TypeCheckerContext(domain);            
        }

        public void Visit(BlockStmt stmt)
        {
            ctx.PushScope();

            foreach (var child in stmt.Stmts)
                child.Visit(this);

            ctx.PopScope();
        }

        public void Visit(VarDecl varDecl)
        {
            IType type;

            if( !ctx.Domain.TryGetType(varDecl.Type.Value, out type) )
                throw new InvalidOperationException();

            foreach( var entry in varDecl.NameAndExps)
            {
                ctx.AddVarType(entry.Name, type);
                if (entry.Exp == null) continue;

                var expType = entry.Exp.Visit(this);

                if (type != expType)
                    throw new TypeMismatchException();
            }
        }

        public void Visit(ForStmt stmt)
        {
            ctx.PushScope();
            stmt.Initializer.Visit(this);
            stmt.CondExp.Visit(this);
            stmt.LoopExp.Visit(this);
            stmt.Body.Visit(this);
            ctx.PopScope();
        }

        public void Visit(WhileStmt stmt)
        {
            ctx.PushScope();

            if (stmt.CondExp.Visit(this) != GlobalDomain.BoolType)
                throw new TypeMismatchException();

            stmt.Body.Visit(this);

            ctx.PopScope();
        }

        public void Visit(DoWhileStmt stmt)
        {
            ctx.PushScope();

            stmt.Body.Visit(this);

            if (stmt.CondExp.Visit(this) != GlobalDomain.BoolType)
                throw new TypeMismatchException();

            ctx.PopScope();
        }

        public void Visit(IfStmt stmt)
        {
            if (stmt.CondExp.Visit(this) != GlobalDomain.BoolType)
                throw new TypeMismatchException();

            ctx.PushScope();

            stmt.ThenStmt.Visit(this);

            ctx.PopScope();

            if (stmt.ElseStmt != null)
            {
                ctx.PushScope();
                stmt.ElseStmt.Visit(this);
                ctx.PopScope();

            }
        }

        public void Visit(ExpStmt stmt)
        {
            stmt.Exp.Visit(this);
        }

        public void Visit(ReturnStmt stmt)
        {
            if (stmt.ReturnExp != null)
            {
                IType retType;
                if (!ctx.Domain.TryGetType(ctx.CurFunc.ReturnType.Value, out retType))
                    throw new InvalidOperationException();

                if (stmt.ReturnExp.Visit(this) != retType)
                    throw new TypeMismatchException();
            }
            else
            {
                if (stmt.ReturnExp.Visit(this) != GlobalDomain.VoidType)
                    throw new TypeMismatchException();
            }
        }

        public void Visit(ContinueStmt stmt)
        {            
        }

        public void Visit(BreakStmt stmt)
        {            
        }

        
        //private void VisitClassDecl(ClassDecl decl, TypeCheckerContext typeCtx)
        //{
        //    TypeInfo ti = new TypeInfo(decl.Name);

        //    // because type declaration should know type itself, putting type information although it's not complete
        //    typeCtx.TypeManager.Types.Add(ti.Name, ti);

        //    // TODO: getting function declarations

        //    // fill typeinfo with variable declarations
        //    foreach (var cvd in decl.VarDecls)
        //    {
        //        foreach( var ne in cvd.VarDecl.NameAndExps)
        //        {
        //            AccessModifier am = AccessModifier.Private;

        //            if (cvd.AccessorKind == Lexer.TokenKind.Private)
        //                am = AccessModifier.Private;
        //            else if (cvd.AccessorKind == Lexer.TokenKind.Protected)
        //                am = AccessModifier.Protected;
        //            else if (cvd.AccessorKind == Lexer.TokenKind.Public)
        //                am = AccessModifier.Public;

        //            ti.AddField(am, typeCtx.TypeManager.Types[cvd.VarDecl.TypeName], ne.Name);
        //        }
        //    }
        //}

        public void Visit(FuncDecl funcDecl)
        {
            ctx.FuncDecls.Add(funcDecl); // for recursives
            ctx.CurFunc = funcDecl;

            foreach(var bodyStmt in funcDecl.BodyStmts)
                bodyStmt.Visit(this);
        }

        internal void Check(FileUnit fileUnit)
        {
            foreach(var decl in fileUnit.Decls)
            {
                if (decl is FuncDecl) Visit(decl as FuncDecl);
                if (decl is VarDecl) Visit(decl as VarDecl);
            }
        }

        internal void Check(IREPLStmt stmt)
        {
            stmt.Visit(this);
        }

    }
}
