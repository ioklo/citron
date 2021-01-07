using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0
{
    static class TestMisc
    {
        public static T[] Arr<T>(params T[] values)
        {
            return values;
        }

        public static Script SimpleScript(IEnumerable<TypeDecl>? typeDecls, IEnumerable<FuncDecl>? funcDecls, params Stmt[] topLevelStmts)
        {
            // TODO: Validator
            int i = 0;
            foreach (var funcDecl in funcDecls ?? Array.Empty<FuncDecl>())
            {
                Assert.Equal(i, funcDecl.Id.Value);
                i++;
            }

            return new Script(typeDecls ?? Array.Empty<TypeDecl>(), funcDecls ?? Array.Empty<FuncDecl>(), topLevelStmts);
        }

        void VerifyError(IEnumerable<IError> errors, AnalyzeErrorCode code, S.ISyntaxNode node)
        {
            var result = errors.OfType<AnalyzeError>()
                .Any(error => error.Code == code && error.Node == node);

            Assert.True(result, $"Errors doesn't contain (Code: {code}, Node: {node})");
        }



        public static S.VarDecl SimpleSVarDecl(S.TypeExp typeExp, string name, S.Exp? initExp = null)
        {
            return new S.VarDecl(typeExp, Arr(new S.VarDeclElement(name, initExp)));
        }

        public static S.VarDeclStmt SimpleSVarDeclStmt(S.TypeExp typeExp, string name, S.Exp? initExp = null)
        {
            return new S.VarDeclStmt(SimpleSVarDecl(typeExp, name, initExp));
        }

        public static S.Script SimpleSScript(params S.Stmt[] stmts)
        {
            return new S.Script(stmts.Select(stmt => new S.Script.StmtElement(stmt)));
        }

        public static S.IntLiteralExp SimpleSInt(int v) => new S.IntLiteralExp(v);
        public static S.BoolLiteralExp SimpleSBool(bool v) => new S.BoolLiteralExp(v);
        public static S.IdentifierExp SimpleSId(string name) => new S.IdentifierExp(name);

        public static S.StringExp SimpleSString(string s) => new S.StringExp(new S.TextStringExpElement(s));

        public static PrivateGlobalVarDeclStmt SimpleGlobalVarDeclStmt(Type type, string name, Exp? initExp = null)
            => new PrivateGlobalVarDeclStmt(Arr(new PrivateGlobalVarDeclStmt.Element(name, type, initExp)));

        public static LocalVarDeclStmt SimpleLocalVarDeclStmt(Type typeId, string name, Exp? initExp = null)
            => new LocalVarDeclStmt(SimpleLocalVarDecl(typeId, name, initExp));

        public static LocalVarDecl SimpleLocalVarDecl(Type typeId, string name, Exp? initExp = null)
            => new LocalVarDecl(Arr(new LocalVarDecl.Element(name, typeId, initExp)));

        public static IntLiteralExp SimpleInt(int v) => new IntLiteralExp(v);
        public static BoolLiteralExp SimpleBool(bool v) => new BoolLiteralExp(v);
        public static StringExp SimpleString(string v) => new StringExp(new TextStringExpElement(v));

        public static S.TypeExp VarTypeExp { get => new S.IdTypeExp("var"); }
        public static S.TypeExp IntTypeExp { get => new S.IdTypeExp("int"); }
        public static S.TypeExp BoolTypeExp { get => new S.IdTypeExp("bool"); }
        public static S.TypeExp VoidTypeExp { get => new S.IdTypeExp("void"); }
        public static S.TypeExp StringTypeExp { get => new S.IdTypeExp("string"); }

    }
}
