using System;
using S = Gum.Syntax;
using R = Gum.IR0;

using static Gum.Infra.Misc;
using static Gum.Syntax.SyntaxFactory;
using static Gum.IR0.IR0Factory;

namespace Gum.TestData
{
    // All Test 
    // Text -> Syntax -> IR0 -> Result
    //    Parse     Translate Eval
    record ParseTranslateEvalTestData
    {
        public string Code { get; }
        public S.Script SScript { get; }
        public R.Script RScript { get; }
        public string Result { get; }
    }

    // declaration등이 제대로 진행되었는지를 확인하는 용도이다
    partial record ParseTranslateTestData(string Code, S.Script SScript, R.Script RScript);    

    static class StructTestData
    {
        public static string ModuleName => "TestModule";

        // 1. 멤버 변수 정의
        public static ParseTranslateTestData MemberDeclaration 
        { 
            get
            {
                var code = @"
struct S
{
    int x;
    int y;
}
";

                var sscript = SScript(
                    new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(
                        new S.VarStructDeclElement(null, IntTypeExp, Arr<string>("x")),
                        new S.VarStructDeclElement(null, IntTypeExp, Arr<string>("y"))
                    )))
                );

                var rscript = RScript(ModuleName, Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(
                        new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr<string>("x")),
                        new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr<string>("y"))
                    ))
                ));

                return new ParseTranslateTestData(code, sscript, rscript);
            }
        }        
    }
}
