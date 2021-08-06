
using S = Gum.Syntax;
using R = Gum.IR0;

using static Gum.Infra.Misc;
using static Gum.Syntax.SyntaxFactory;
using static Gum.IR0.IR0Factory;
using static Gum.IR0Translator.AnalyzeErrorCode;

namespace Gum.Test.IntegrateTest
{
    public class StructTestData : IntegrateTestData<StructTestData>
    {
        public static string ModuleName = "TestModule";

        // UnitOfWorkName_ScenarioName_ExpectedBehavior
        static TestData Make_Declaration_DeclareMemberVar_TranslateProperly()
        {
            var code = @"
struct S
{
    int x;
    int y;
}
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(
                    new S.StructMemberVarDecl(null, SIntTypeExp(), Arr<string>("x")),
                    new S.StructMemberVarDecl(null, SIntTypeExp(), Arr<string>("y"))
                )))
            );

            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, default);

            var rscript = RScript(ModuleName, Arr<R.Decl>(
                new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(
                    new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr<string>("x")),
                    new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr<string>("y")),

                    new R.StructConstructorDecl(
                        R.AccessModifier.Public,
                        default,
                        Arr(
                            new R.Param(R.ParamKind.Normal, R.Path.Int, "x"),
                            new R.Param(R.ParamKind.Normal, R.Path.Int, "y")
                        ),
                        RBlock(
                            new R.ExpStmt(new R.AssignExp(
                                new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
                                new R.LoadExp(new R.LocalVarLoc("x"))
                            )),
                            new R.ExpStmt(new R.AssignExp(
                                new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "y", R.ParamHash.None, default)),
                                new R.LoadExp(new R.LocalVarLoc("y"))
                            ))
                        )
                    )
                ))
            ));

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        static TestData Make_Constructor_Declaration_WorksProperly()
        {
            var code = @"
struct S
{
    S(int x) { }
}
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(
                    // new S.StructMemberVarDecl(null, SIntTypeExp(), Arr<string>("x")),

                    new S.StructConstructorDecl(null, "S", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), SBlock(
                    // new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.MemberExp(SId("this"), "x", default), SId("x")))
                    ))
                )))
            );

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock()
                        ),

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            default,
                            RBlock()
                        )
                    ))
                )
            );

            return new ParseTranslateTestData(code, sscript, rscript);
        }
            
        static TestData Make_Constructor_DeclareNameDifferFromStruct_ReportError()
        {
            var code = @"
struct S
{
    F() { }
}
";
            S.StructConstructorDecl errorNode;
            // Parsing단계에서는 걸러내지 못하고, Translation 단계에서 걸러낸다
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(
                    errorNode = new S.StructConstructorDecl(null, "F", Arr<S.FuncParam>(), SBlock())
                )))
            );

            return new ParseTranslateWithErrorTestData(code, sscript, A2402_StructDecl_CannotDeclConstructorDifferentWithTypeName, errorNode);
        }

        static TestData Make_VarDecl_UsingConstructor_CallConstructor()
        {
            var code = @"
struct S
{
    S(int x) { @{$x} } // x출력
}

var s = S(3);
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(

                    new S.StructConstructorDecl(null, "S", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), SBlock(
                        new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(SId("x"))))))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                )))))
            );

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock(RCommand(RString(new R.ExpStringExpElement(
                                new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.ToString_Int_String, new R.LoadExp(new R.LocalVarLoc("x"))))
                            )))
                        ),

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            default,
                            RBlock()
                        )
                    ))
                ),

                RGlobalVarDeclStmt(
                    new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, Arr<R.Path>()),
                    "s",
                    new R.NewStructExp(
                        new R.Path.Nested(
                            new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, Arr<R.Path>()),
                            R.Name.Constructor.Instance,
                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                            default
                        ),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
                    )
                )
            );

            var result = "3";

            return new ParseTranslateEvalTestData(code, sscript, rscript, result);
        }

        // using this            
        static TestData Make_Constructor_UsingThis_ReferCurrentInstance()
        {
            var code = @"
struct S { int x; S(int x) { this.x = x; } }
var s = S(3);
@${s.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(

                    new S.StructMemberVarDecl(null, SIntTypeExp(), Arr("x")),

                    new S.StructConstructorDecl(null, "S", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), SBlock(
                        new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.MemberExp(SId("this"), "x", default), SId("x")))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    new S.MemberExp(SId("s"), "x", default)
                ))))))
            );

            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, default);

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")),

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock(
                                new R.ExpStmt(new R.AssignExp(
                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
                                    new R.LoadExp(new R.LocalVarLoc("x"))
                                )
                            )
                        )
                    )))
                ),

                RGlobalVarDeclStmt(
                    SPath(),
                    "s",
                    new R.NewStructExp(
                        new R.Path.Nested(
                            SPath(),
                            R.Name.Constructor.Instance,
                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                            default
                        ),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
                    )
                ),

                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        new R.LoadExp(new R.StructMemberLoc(new R.GlobalVarLoc("s"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)))
                    )
                ))))

            // @${ s.x}
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");
        }

        // 생성자를 자동으로 만들기
        static TestData Make_Constructor_DoesntHaveCorrespondingConstructor_MakeAutomatically()
        {
            var code = @"
struct S { int x; } // without constructor S(int x)
var s = S(3);       // but can do this
@${s.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(

                    new S.StructMemberVarDecl(null, SIntTypeExp(), Arr("x"))

                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    new S.MemberExp(SId("s"), "x", default)
                ))))))
            );

            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, default);

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")),

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock(
                                new R.ExpStmt(new R.AssignExp(
                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
                                    new R.LoadExp(new R.LocalVarLoc("x"))
                                )
                            )
                        )
                    )))
                ),

                RGlobalVarDeclStmt(
                    SPath(),
                    "s",
                    new R.NewStructExp(
                        new R.Path.Nested(
                            SPath(),
                            R.Name.Constructor.Instance,
                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                            default
                        ),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
                    )
                ),

                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        new R.LoadExp(new R.StructMemberLoc(new R.GlobalVarLoc("s"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)))
                    )
                ))))

            // @${ s.x}
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");

        }

        static TestData Make_ReferenceMember_ReadAndWrite_WorksProperly()
        {
            var code = @"
struct S { int x; }
var s = S(2);
s.x = 3;
@${s.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(

                    new S.StructMemberVarDecl(null, SIntTypeExp(), Arr("x"))

                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(2)))))
                ))))),

                new S.StmtScriptElement(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.MemberExp(SId("s"), "x", default), SInt(3)))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    new S.MemberExp(SId("s"), "x", default)
                ))))))
            );

            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, default);

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")),

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock(
                                new R.ExpStmt(new R.AssignExp(
                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
                                    new R.LoadExp(new R.LocalVarLoc("x"))
                                )
                            )
                        )
                    )))
                ),

                // S s = S(2);
                RGlobalVarDeclStmt(
                    SPath(),
                    "s",
                    new R.NewStructExp(
                        new R.Path.Nested(
                            SPath(),
                            R.Name.Constructor.Instance,
                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                            default
                        ),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(2)))
                    )
                ),

                // s.x = 3;
                new R.ExpStmt(new R.AssignExp(new R.StructMemberLoc(new R.GlobalVarLoc("s"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)), RInt(3))),

                // @${s.x}
                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        new R.LoadExp(new R.StructMemberLoc(new R.GlobalVarLoc("s"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)))
                    )
                ))))
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");
        }

        static TestData Make_ReferenceSelf_Assign_CopyValue()
        {
            var code = @"
struct S { int x; }
var s1 = S(2);
var s2 = S(17);
s2 = s1;
s1.x = 3;
@{${s2.x}} // 2;
";

            return new EvalTestData(code, "2");
        }

        static TestData Make_ReferenceSelf_PassingByValue_CopyValue()
        {
            var code = @"
struct S { int x; }
void F(S s) { s.x = 2; }

var s = S(3);
F(s);
@${s.x}
";

            return new EvalTestData(code, "3");
        }

        static TestData Make_ReferenceSelf_PassingByRef_ReferenceValue()
        {
            var code = @"
struct S { int x; }
void F(ref S s) { s.x = 2; }

var s = S(3);
F(ref s);
@${s.x}
";
            return new EvalTestData(code, "2");
        }

        static TestData Make_Access_AccessPrivateMemberOutsideStruct_ReportError()
        {
            var code = @"
struct S { private int x; }
var s = S(3);
@${s.x}
";
            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        }

        static TestData Make_Access_AccessPrivateConstructorOutsideStruct_ReportError()
        {
            var code = @"
struct S { private S(int x) { } }
var s = S(3);
";
            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        }

        static TestData Make_MemberFunc_Declaration_WorksProperly()
        {
            var code = @"
struct S
{
    int x;
    int F(int y) { return x + y; }
}

var s = S(3);
var i = s.F(2);
@$i
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(

                    new S.StructMemberVarDecl(null, SIntTypeExp(), Arr("x")),
                    new S.StructMemberFuncDecl(null, false, false, false, SIntTypeExp(), "F", default, Arr(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "y")), SBlock(
                        new S.ReturnStmt(new S.ReturnValueInfo(false, new S.BinaryOpExp(S.BinaryOpKind.Add, SId("x"), SId("y"))))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "i", new S.VarDeclElemInitializer(false, new S.CallExp(new S.MemberExp(SId("s"), "F", default), Arr<S.Argument>(new S.Argument.Normal(SInt(2)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    SId("i")
                ))))))
            );

            R.Path.Nested SPath() => new R.Path.Root("TestModule").Child("S");

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(
                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr<string>("x")),

                        new R.StructMemberFuncDecl(default, "F", true, default, Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "y")), RBlock(
                            new R.ReturnStmt(new R.ReturnInfo.Expression(new R.CallInternalBinaryOperatorExp(
                                R.InternalBinaryOperator.Add_Int_Int_Int,
                                new R.LoadExp(new R.StructMemberLoc(R.ThisLoc.Instance, SPath().Child("x"))),
                                new R.LoadExp(new R.LocalVarLoc("y"))
                            )))
                        )),

                        new R.StructConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            Arr(
                                new R.Param(R.ParamKind.Normal, R.Path.Int, "x")
                            ),
                            RBlock(
                                new R.ExpStmt(new R.AssignExp(
                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
                                    new R.LoadExp(new R.LocalVarLoc("x"))
                                ))
                            )
                        )
                    ))
                ),

                RGlobalVarDeclStmt(
                    SPath(),
                    "s",
                    new R.NewStructExp(
                        SPath().Child(R.Name.Constructor.Instance, R.Path.Int),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
                    )
                ),

                // var i = s.F(2);

                RGlobalVarDeclStmt(
                    R.Path.Int,
                    "i",
                    new R.CallFuncExp(
                        SPath().Child("F", R.Path.Int),
                        new R.GlobalVarLoc("s"),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(2)))
                    )
                ),

                RPrintIntCmdStmt(new R.GlobalVarLoc("i"))
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "5");
        }

        static TestData Make_MemberFunc_ModifySelf_WorksProperly()
        {
            var code = @"
struct S
{
    int x;
    void SetX(int x) { this.x = x; }
    void Print() { @{$x} }
}

var s1 = S(3);
var s2 = s1;
s1.SetX(2);
s2.SetX(17);

s1.Print();
s2.Print();
";
            return new EvalTestData(code, "217");
        }

        static TestData Make_Access_AccessPrivateMemberFunc_ReportError()
        {
            var code = @"
struct S
{
private void F() { }
}

var s = S();
s.F();
";
            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        }

        static TestData Make_Access_AccessPrivateMemberFuncInsideMemberFuncOfSameStruct_WorksProperly()
        {
            var code = @"
struct S
{
    private void E() { @{hi} }
    void F() { E(); }
}

var s = S();
s.F();
";

            return new EvalTestData(code, "hi");

        }

        static TestData Make_MemberSeqFunc_DependsOnThis_WorksProperly()
        {
            var code = @"
struct S
{
    int x;
    seq int F()
    {
        yield x + 1;
    }
}

var s = S(3);

foreach(var i in s.F())
@$i
";

            return new EvalTestData(code, "4");
        }

        //// default는 잠깐 미루자
        //// 빈 Struct는 자동생성자가 default constructor 역할을 하기 때문에 그냥 생성 가능하다
        //[Fact]
        //public Task FXX_Constructor_DeclStructValueThatDoesntContainMemberVar_UseAutomaticConstructorAsDefaultConstructor()
        //{
        //    throw new TestNeedToBeWrittenException();
        //}

        ////        // 초기화 구문 없을때, DefaultConstructor를 사용해서
        ////        [Fact]
        ////        public Task F02_VarDecl_NoInitializerWithDefaultConstructor_WorksProperly()
        ////        {
        ////            var code = @"
        ////struct S
        ////{
        ////    int x;
        ////    S() { x = 3; }
        ////}

        ////S s; // S() 호출
        ////";

        ////            var sscript = SScript(
        ////                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(
        ////                    new S.StructMemberVarDecl(null, SIntTypeExp(), Arr<string>("x")),
        ////                    new S.StructConstructorDecl(null,


        ////                )))
        ////            );

        ////            var rscript = RScript(ModuleName, Arr<R.Decl>(
        ////                new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(
        ////                    new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr<string>("x")),
        ////                    new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr<string>("y"))
        ////                ))
        ////            ));

        ////            return TestParseTranslate(code, sscript, rscript);

        ////        }

        //[Fact]
        //public Task FXX_DefaultConstructor_DoesntInitializeAllMemberVariables_ReportError()
        //{
        //    throw new TestNeedToBeWrittenException();
        //}

        //[Fact]
        //public Task FXX_DefaultConstructor_BodyHasCallExp_ReportError()
        //{
        //    throw new TestNeedToBeWrittenException();
        //}

        //// S s; // S에 Default constructor가 없으면 에러
        //[Fact]
        //public Task FXX_VarDecl_NoInitializerWithoutDefaultConstructor_ReportError()
        //{
        //    throw new TestNeedToBeWrittenException();
        //}

    }
}
