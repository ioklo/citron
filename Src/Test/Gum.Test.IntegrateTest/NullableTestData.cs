using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Gum.Syntax;
using R = Gum.IR0;

using static Gum.Infra.Misc;
using static Gum.Syntax.SyntaxFactory;
using static Gum.IR0.IR0Factory;
using static Gum.IR0Translator.AnalyzeErrorCode;

namespace Gum.Test.IntegrateTest
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


    }
}
