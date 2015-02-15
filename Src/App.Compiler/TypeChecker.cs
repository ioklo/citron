using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.App.Compiler.AST;
using Gum.App.Compiler.Exception;

namespace Gum.App.Compiler
{    
    // Making type information and checking expression's type
    class TypeChecker : IStmtVisitor, IExpVisitor<TypeInfo>
    {
        TypeCheckerContext ctx;
        TypeManager typeManager;

        public TypeChecker(TypeManager tm)
        {
            ctx = new TypeCheckerContext(tm);
            typeManager = tm;
        }

        public void Visit(BlockStmt stmt)
        {
            ctx.PushScope();

            foreach (var child in stmt.Stmts)
                child.Visit(this);

            ctx.PopScope();
        }

        public void Visit(VarDeclStmt stmt)
        {
            var varType = ctx.TypeManager.Types[stmt.Decl.TypeName];

            foreach( var entry in stmt.Decl.NameAndExps)
            {
                ctx.AddVarType(entry.Name, varType);
                if (entry.Exp == null) continue;

                var expType = entry.Exp.Visit(this);

                if (varType != expType)
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

            if (stmt.CondExp.Visit(this) != ctx.TypeManager.BoolType)
                throw new TypeMismatchException();

            stmt.Body.Visit(this);

            ctx.PopScope();
        }

        public void Visit(DoWhileStmt stmt)
        {
            ctx.PushScope();

            stmt.Body.Visit(this);

            if (stmt.CondExp.Visit(this) != ctx.TypeManager.BoolType)
                throw new TypeMismatchException();

            ctx.PopScope();
        }

        public void Visit(IfStmt stmt)
        {
            if (stmt.CondExp.Visit(this) != ctx.TypeManager.BoolType)
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

        public void Visit(NullStmt stmt)
        {
        }

        public void Visit(ExpStmt stmt)
        {
            stmt.Exp.Visit(this);
        }

        public void Visit(ReturnStmt stmt)
        {
            if (stmt.ReturnExp != null)
            {
                if (stmt.ReturnExp.Visit(this) != ctx.TypeManager.Types[ctx.CurFunc.ReturnTypeName])
                    throw new TypeMismatchException();
            }
            else
            {
                if (stmt.ReturnExp.Visit(this) != ctx.TypeManager.VoidType)
                    throw new TypeMismatchException();
            }
        }

        public void Visit(ContinueStmt stmt)
        {            
        }

        public void Visit(BreakStmt stmt)
        {            
        }

        public TypeInfo Visit(AssignExp exp)
        {
            TypeInfo varType;

            if (!ctx.TryGetVarType(exp.Var.Name, out varType))
                throw new LocalVarNotDeclaredException();

            foreach(var offset in exp.Var.Offsets)
            {
                var stringOffset = offset as StringOffset;
                if (stringOffset == null)
                    throw new NotImplementedException();

                exp.Var.IndexOffsets.Add(varType.Fields[stringOffset.Field].Index);
                varType = varType.Fields[stringOffset.Field].TypeInfo;                
            }
            

            var expType = exp.Exp.Visit(this);

            if (varType != expType)
                throw new TypeMismatchException();

            return varType;
        }

        public TypeInfo Visit(VariableExp exp)
        {
            TypeInfo varType;

            if (!ctx.TryGetVarType(exp.Name, out varType))
                throw new LocalVarNotDeclaredException();

            foreach (var offset in exp.Offsets)
            {
                var stringOffset = offset as StringOffset;
                if (stringOffset == null)
                    throw new NotImplementedException();

                exp.IndexOffsets.Add(varType.Fields[stringOffset.Field].Index);
                varType = varType.Fields[stringOffset.Field].TypeInfo;                
            }

            return varType;
        }

        public TypeInfo Visit(IntegerExp exp)
        {
            return ctx.TypeManager.IntType;
        }

        public TypeInfo Visit(StringExp exp)
        {
            return ctx.TypeManager.StringType;
        }

        public TypeInfo Visit(BoolExp exp)
        {
            return ctx.TypeManager.BoolType;
        }

        public TypeInfo Visit(BinaryExp exp)
        {
            var type1 = exp.Operand1.Visit(this);
            var type2 = exp.Operand2.Visit(this);

            switch (exp.Operation)
            {
                // 동치 비교
                case BinaryExpKind.Equal:                    
                case BinaryExpKind.NotEqual:
                    if (type1 != type2 || 
                        (type1 != ctx.TypeManager.IntType && type1 != ctx.TypeManager.StringType && type1 != ctx.TypeManager.VoidType))
                        throw new TypeMismatchException();
                    return ctx.TypeManager.BoolType;

                // Complete Order 비교
                case BinaryExpKind.Less:   
                case BinaryExpKind.Greater:
                case BinaryExpKind.LessEqual:
                case BinaryExpKind.GreaterEqual:
                    if (type1 != type2 || 
                        (type1 != ctx.TypeManager.IntType && type1 != ctx.TypeManager.StringType))
                        throw new TypeMismatchException();
                    return ctx.TypeManager.BoolType;

                // boolean 
                case BinaryExpKind.And:   
                case BinaryExpKind.Or:
                    if (type1 != type2 || type1 != ctx.TypeManager.BoolType)
                        throw new TypeMismatchException();
                    return type1;

                case BinaryExpKind.Add:
                    if (type1 != type2 || 
                        (type1 != ctx.TypeManager.IntType && type1 != ctx.TypeManager.StringType))
                        throw new TypeMismatchException();
                    return type1;

                case BinaryExpKind.Sub:   
                case BinaryExpKind.Mul:   
                case BinaryExpKind.Div:
                case BinaryExpKind.Mod:
                    if (type1 != type2 || type1 != ctx.TypeManager.IntType)
                        throw new TypeMismatchException();
                    return type1;
            }

            throw new NotImplementedException();
        }

        public TypeInfo Visit(UnaryExp exp)
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
                    return ctx.TypeManager.IntType;

                case UnaryExpKind.Not:
                    if (exp.Operand.Visit(this) != ctx.TypeManager.BoolType)
                        throw new TypeMismatchException();
                    return ctx.TypeManager.BoolType;                    
            }

            throw new NotImplementedException();
        }

        public TypeInfo Visit(NewExp exp)
        {
            return ctx.TypeManager.Types[exp.TypeName];
        }

        public TypeInfo Visit(CallExp exp)
        {
            FuncDecl decl = ctx.GetFunc(exp.FuncName);

            if (decl == null)
                throw new FuncNotDeclaredException();

            if (decl.Parameters.Count != exp.Args.Count)
                throw new System.Exception("인자 개수가 맞지 않습니다");

            for(int t = 0; t < decl.Parameters.Count; t++)
            {
                if (exp.Args[t].Visit(this) != ctx.TypeManager.Types[decl.Parameters[t].TypeName])
                    throw new TypeMismatchException();
            }            

            return ctx.TypeManager.Types[decl.ReturnTypeName];
        }

        private void VisitClassDecl(ClassDecl decl, TypeCheckerContext typeCtx)
        {
            TypeInfo ti = new TypeInfo(decl.Name);

            // because type declaration should know type itself, putting type information although it's not complete
            typeCtx.TypeManager.Types.Add(ti.Name, ti);

            // TODO: getting function declarations

            // fill typeinfo with variable declarations
            foreach (var cvd in decl.VarDecls)
            {
                foreach( var ne in cvd.VarDecl.NameAndExps)
                {
                    AccessModifier am = AccessModifier.Private;

                    if (cvd.AccessorKind == Lexer.TokenKind.Private)
                        am = AccessModifier.Private;
                    else if (cvd.AccessorKind == Lexer.TokenKind.Protected)
                        am = AccessModifier.Protected;
                    else if (cvd.AccessorKind == Lexer.TokenKind.Public)
                        am = AccessModifier.Public;

                    ti.AddField(am, typeCtx.TypeManager.Types[cvd.VarDecl.TypeName], ne.Name);
                }
            }
        }

        internal void Check(Program prog)
        {            
            var funcs = prog.Decls.Where(decl => decl is FuncDecl).Cast<FuncDecl>().ToList();

            foreach (var decl in prog.Decls)
            {
                if (decl is ClassDecl)
                {
                    VisitClassDecl((ClassDecl)decl, ctx);
                }

                if (decl is FuncDecl)
                {
                    var funcDecl = decl as FuncDecl;
                    ctx.FuncDecls.Add(funcDecl); // for recursives
                    ctx.CurFunc = funcDecl;

                    if( funcDecl.Body != null)
                        funcDecl.Body.Visit(this);
                }
                
                if (decl is VarDecl)
                {
                    VarDecl varDecl = decl as VarDecl;

                    foreach (var ne in varDecl.NameAndExps)
                        ctx.AddVarType(ne.Name, ctx.TypeManager.Types[varDecl.TypeName]);
                }
            }
        }
    }
}
