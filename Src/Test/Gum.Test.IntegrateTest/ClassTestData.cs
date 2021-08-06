using S = Gum.Syntax;
using R = Gum.IR0;

using static Gum.Syntax.SyntaxFactory;
using static Gum.Infra.Misc;

using static Gum.IR0.IR0Factory;

namespace Gum.Test.IntegrateTest
{
    class ClassTestData : IntegrateTestData<ClassTestData>
    {
        static string moduleName = "TestModule";

        // UnitOfWorkName_ScenarioName_ExpectedBehavior
        static TestData Make_Declaration_DeclareMemberVar_TranslateProperly()
        {
            var code = @"
class C
{
    int x, y;
    public string s;
}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", default, default, Arr<S.ClassMemberDecl>(
                    new S.ClassMemberVarDecl(null, SIntTypeExp(), Arr("x", "y")),
                    new S.ClassMemberVarDecl(S.AccessModifier.Public, SStringTypeExp(), Arr("s"))
                )))
            );

            var rscript = RScript(moduleName, Arr<R.Decl>(
                new R.ClassDecl(R.AccessModifier.Public, "C", default, default, Arr<R.ClassMemberDecl>(
                    new R.ClassMemberVarDecl(R.AccessModifier.Private, R.Path.Int, Arr("x", "y")),
                    new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.String, Arr("s"))
                ))
            ));

            return new ParseTranslateTestData(code, sscript, rscript);
        }

    }
}
