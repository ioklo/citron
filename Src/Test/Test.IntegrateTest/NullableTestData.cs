using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.IR0Analyzer.NullRefAnalysis;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Infra.Misc;
using static Citron.Syntax.SyntaxFactory;
using static Citron.IR0.IR0Factory;

namespace Citron.Test.IntegrateTest
{
    public class NullableTestData : IntegrateTestData<NullableTestData>
    {
        static R.ModuleName moduleName = new R.ModuleName("TestModule");

        static TestData Make_Declaration_GlobalVariable_WorksProperly()
        {
            var code = @"
int? x;
";
            var sscript = SScript(
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    false, new S.NullableTypeExp(new S.IdTypeExp("int", default)),
                    Arr(new S.VarDeclElement("x", null))
                )))
            );

            var rscript = RScript(moduleName, RGlobalVarDeclStmt(new R.Path.NullableType(R.Path.Int), "x"));

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        // assign null
        static TestData Make_Assignment_Null_WorksProperly()
        {
            var code = @"
int? x;
x = null;";

            var sscript = SScript(
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    false, new S.NullableTypeExp(new S.IdTypeExp("int", default)),
                    Arr(new S.VarDeclElement("x", null))
                ))),

                new S.StmtScriptElement(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), S.NullLiteralExp.Instance)))
            );

            var rscript = RScript(moduleName, 
                RGlobalVarDeclStmt(new R.Path.NullableType(R.Path.Int), "x"),
                RAssignStmt(new R.GlobalVarLoc("x"), new R.NewNullableExp(R.Path.Int, null))
            );

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        // assign value
        static TestData Make_Assignment_Value_WorksProplery()
        {
            var code = @"
int? x;
x = 1;
";
            var sscript = SScript(
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    false, new S.NullableTypeExp(new S.IdTypeExp("int", default)),
                    Arr(new S.VarDeclElement("x", null))
                ))),

                new S.StmtScriptElement(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SInt(1))))
            );

            var rscript = RScript(moduleName,
                RGlobalVarDeclStmt(new R.Path.NullableType(R.Path.Int), "x"),
                RAssignStmt(new R.GlobalVarLoc("x"), new R.NewNullableExp(R.Path.Int, RInt(1)))
            );

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        static TestData Make_StaticCheck_AssignNotNullAndCheckNotNull_TranslateProperly()
        {
            var code = @"
int? i = 2;

`static_notnull(i);
";

            var sscript = SScript(
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    false, new S.NullableTypeExp(new S.IdTypeExp("int", default)),
                    Arr(new S.VarDeclElement("i", new S.VarDeclElemInitializer(false, SInt(2))))
                ))),

                new S.StmtScriptElement(new S.DirectiveStmt("static_notnull", Arr<S.Exp>(SId("i"))))
            );

            var rscript = RScript(moduleName,
                RGlobalVarDeclStmt(new R.Path.NullableType(R.Path.Int), "i", new R.NewNullableExp(R.Path.Int, RInt(2))),
                new R.DirectiveStmt.StaticNotNull(new R.GlobalVarLoc("i"))
            );

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        static TestData Make_StaticCheck_AssignNullAndCheckNotNull_ReportError()
        {
            var code = @"
int? i = null;

`static_notnull(i);
";

            S.DirectiveStmt errorNode;
            var sscript = SScript(
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    false, new S.NullableTypeExp(new S.IdTypeExp("int", default)),
                    Arr(new S.VarDeclElement("i", new S.VarDeclElemInitializer(false, S.NullLiteralExp.Instance)))
                ))),

                new S.StmtScriptElement(errorNode = new S.DirectiveStmt("static_notnull", Arr<S.Exp>(SId("i"))))
            );

            // TODO: errorNode 체크
            return new ParseTranslateWithErrorTestData(code, sscript, new R0101_StaticNotNullDirective_LocationIsNull());
        }
    }
}
