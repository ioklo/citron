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

            var rscript = RScript(moduleName, new R.GlobalVarDeclStmt(Arr<R.VarDeclElement>(new R.VarDeclElement.NormalDefault(new R.Path.NullableType(R.Path.Int), "x"))));

            return new ParseTranslateTestData(code, sscript, rscript);
        }
    }
}
