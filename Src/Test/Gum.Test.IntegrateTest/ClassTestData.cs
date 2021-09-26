using S = Gum.Syntax;
using R = Gum.IR0;

using static Gum.Syntax.SyntaxFactory;
using static Gum.Infra.Misc;

using static Gum.IR0.IR0Factory;
using static Gum.IR0Translator.AnalyzeErrorCode;

namespace Gum.Test.IntegrateTest
{
    class ClassTestData : IntegrateTestData<ClassTestData>
    {
        static string ModuleName = "TestModule";

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

            var rscript = RScript(ModuleName, 
                Arr<R.TypeDecl>(new R.ClassDecl(R.AccessModifier.Private, "C", default, null, default, 
                    Arr(new R.ClassConstructorDecl(
                        R.AccessModifier.Public,
                        default,
                        RNormalParams((R.Path.Int, "x"), (R.Path.Int, "y"), (R.Path.String, "s")),
                        null,
                        RBlock(
                            new R.ExpStmt(new R.AssignExp(
                                new R.ClassMemberLoc(R.ThisLoc.Instance, RRoot(ModuleName).Child("C").Child("x")),
                                new R.LoadExp(RLocalVarLoc("x"))
                            )),
                            new R.ExpStmt(new R.AssignExp(
                                new R.ClassMemberLoc(R.ThisLoc.Instance, RRoot(ModuleName).Child("C").Child("y")),
                                new R.LoadExp(RLocalVarLoc("y"))
                            )),
                            new R.ExpStmt(new R.AssignExp(
                                new R.ClassMemberLoc(R.ThisLoc.Instance, RRoot(ModuleName).Child("C").Child("s")),
                                new R.LoadExp(RLocalVarLoc("s"))
                            ))
                        )
                    )),

                    default,

                    Arr(
                        new R.ClassMemberVarDecl(R.AccessModifier.Private, R.Path.Int, Arr("x", "y")),
                        new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.String, Arr("s"))
                    )
                )),

                default, default
            );

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        static TestData Make_Constructor_Declaration_WorksProperly()
        {
            var code = @"
class C
{
    public C(int x) { }
}
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(
                    new S.ClassConstructorDecl(S.AccessModifier.Public, "C", SNormalFuncParams((SIntTypeExp(), "x")), null, SBlock())
                )))
            );

            var rscript = RScript(ModuleName,
                Arr<R.TypeDecl>(new R.ClassDecl(R.AccessModifier.Private, "C", default, null, default, 
                    Arr(
                        new R.ClassConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            RNormalParams((R.Path.Int, "x")),
                            null,
                            RBlock()
                        ),

                        new R.ClassConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            default,
                            null,
                            RBlock()
                        )
                    ), default, default
                )),

                default, default
            );

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        static TestData Make_Constructor_DeclareNameDifferFromStruct_ReportError()
        {
            var code = @"
class C
{
    F() { }
}
";
            S.ClassConstructorDecl errorNode;
            // Parsing단계에서는 걸러내지 못하고, Translation 단계에서 걸러낸다
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(
                    errorNode = new S.ClassConstructorDecl(null, "F", default, null, SBlock())
                )))
            );

            return new ParseTranslateWithErrorTestData(code, sscript, A2502_ClassDecl_CannotDeclConstructorDifferentWithTypeName, errorNode);
        }

        static TestData Make_VarDecl_UsingConstructor_CallConstructor()
        {
            var code = @"
class C
{
    public C(int x) { @{$x} } // x출력
}

var c = new C(3);
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

                    new S.ClassConstructorDecl(S.AccessModifier.Public, "C", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), null, SBlock(
                        new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(SId("x"))))))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "c", new S.VarDeclElemInitializer(false, new S.NewExp(SIdTypeExp("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                )))))
            );

            var rscript = RScript(ModuleName,
                Arr<R.TypeDecl>(
                    new R.ClassDecl(R.AccessModifier.Private, "C", default, null, default, Arr(

                        new R.ClassConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            RNormalParams((R.Path.Int, "x")),
                            null,
                            RBlock(RPrintIntCmdStmt(RLocalVarLoc("x")))
                        ),

                        new R.ClassConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            default,
                            null,
                            RBlock()
                        )
                    ), default, default)
                ),

                default, default,

                RGlobalVarDeclStmt(
                    RRoot("TestModule").Child("C"),
                    "c",
                    new R.NewClassExp(
                        RRoot("TestModule").Child("C"),
                        new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
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
class C { public int x; public C(int x) { this.x = x; } }
var c = new C(3);
@${c.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

                    new S.ClassMemberVarDecl(S.AccessModifier.Public, SIntTypeExp(), Arr("x")),

                    new S.ClassConstructorDecl(S.AccessModifier.Public, "C", SNormalFuncParams((SIntTypeExp(), "x")), null, SBlock(
                        SAssignStmt(SId("this").Member("x"), SId("x"))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "c", new S.VarDeclElemInitializer(false, new S.NewExp(SIdTypeExp("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    SId("c").Member("x")
                ))))))
            );

            R.Path.Nested CPath() => RRoot("TestModule").Child("C");

            var rscript = RScript(ModuleName,
                Arr<R.TypeDecl>(new R.ClassDecl(R.AccessModifier.Private, "C", default, null, default, 
                    Arr(new R.ClassConstructorDecl(
                        R.AccessModifier.Public,
                        default,
                        RNormalParams((R.Path.Int, "x")),
                        null,
                        RBlock(RAssignStmt(
                            R.ThisLoc.Instance.ClassMember(CPath().Child("x")),
                            RLocalVarExp("x")
                        ))
                    )),

                    default, 

                    Arr(new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")))
                )), 
                
                default, default,

                RGlobalVarDeclStmt(
                    CPath(),
                    "c",
                    new R.NewClassExp(
                        CPath(),
                        new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
                    )
                ),

                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        new R.LoadExp(new R.ClassMemberLoc(new R.GlobalVarLoc("c"), CPath().Child("x")))
                    )
                ))))
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");
        }

        // 생성자를 자동으로 만들기
        static TestData Make_Constructor_DoesntHaveCorrespondingConstructor_MakeAutomatically()
        {
            var code = @"
class C { public int x; } // without constructor S(int x)
var c = new C(3);         // but can do this
@${c.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

                    new S.ClassMemberVarDecl(S.AccessModifier.Public, SIntTypeExp(), Arr("x"))

                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "c", new S.VarDeclElemInitializer(false, new S.NewExp(SIdTypeExp("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    SId("c").Member("x")
                ))))))
            );

            var rscript = RScript(ModuleName,
                Arr<R.TypeDecl>(new R.ClassDecl(R.AccessModifier.Private, "C", default, null, default, 
                    Arr(new R.ClassConstructorDecl(
                        R.AccessModifier.Public,
                        default,
                        RNormalParams((R.Path.Int, "x")),
                        null,
                        RBlock(RAssignStmt(R.ThisLoc.Instance.ClassMember(RRoot(ModuleName).Child("C").Child("x")), RLocalVarExp("x")))
                    )),

                    default,

                    Arr(new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")))                    
                )),

                default, default,

                RGlobalVarDeclStmt(
                    RRoot(ModuleName).Child("C"),
                    "c",
                    new R.NewClassExp(
                        RRoot(ModuleName).Child("C"),
                        new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
                    )
                ),

                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        new R.LoadExp(new R.ClassMemberLoc(new R.GlobalVarLoc("c"), RRoot(ModuleName).Child("C").Child("x")))
                    )
                ))))
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");
        }

        static TestData Make_ReferenceMember_ReadAndWrite_WorksProperly()
        {
            var code = @"
class C { public int x; }
var c = new C(2);
c.x = 3;
@${c.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", default, default, Arr<S.ClassMemberDecl>(
                    new S.ClassMemberVarDecl(S.AccessModifier.Public, SIntTypeExp(), Arr("x"))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "c", new S.VarDeclElemInitializer(false, new S.NewExp(SIdTypeExp("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(2)))))
                ))))),

                new S.StmtScriptElement(SAssignStmt(SId("c").Member("x"), SInt(3))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    SId("c").Member("x")
                ))))))
            );

            var rscript = RScript(ModuleName,
                Arr<R.TypeDecl>(new R.ClassDecl(R.AccessModifier.Private, "C", default, null, default, 
                    Arr(new R.ClassConstructorDecl(
                        R.AccessModifier.Public,
                        default,
                        RNormalParams((R.Path.Int, "x")),
                        null,
                        RBlock(RAssignStmt(R.ThisLoc.Instance.ClassMember(RRoot(ModuleName).Child("C").Child("x")), RLocalVarExp("x")))
                    )),

                    default,
                    
                    Arr(new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")))
                )),

                default, default,

                // C c = new C(2);
                RGlobalVarDeclStmt(
                    RRoot(ModuleName).Child("C"),
                    "c",
                    new R.NewClassExp(
                        RRoot(ModuleName).Child("C"),
                        new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(2)))
                    )
                ),

                // c.x = 3;
                new R.ExpStmt(new R.AssignExp(new R.ClassMemberLoc(new R.GlobalVarLoc("c"), RRoot(ModuleName).Child("C").Child("x")), RInt(3))),

                // @${c.x}
                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        new R.LoadExp(new R.ClassMemberLoc(new R.GlobalVarLoc("c"), RRoot(ModuleName).Child("C").Child("x")))
                    )
                ))))
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");
        }

        static TestData Make_ReferenceSelf_Assign_RefValue()
        {
            var code = @"
class C { public int x; }
var c1 = new C(2);
var c2 = new C(17);
c2 = c1;
c1.x = 3;
@{${c2.x}} // 3;
";

            return new EvalTestData(code, "3");
        }

        static TestData Make_ReferenceSelf_PassingByValue_CopyValue()
        {
            var code = @"
class C { public int x; }
void F(C c1) { c1.x = 2; }

var c = new C(3);
F(c);
@${c.x}
";

            return new EvalTestData(code, "2");
        }

        static TestData Make_ReferenceSelf_PassingByRef_ReferenceValue()
        {
            var code = @"
class C { public int x; }
void F(ref C c) { c = new C(4); }

var c = new C(3);
F(ref c);
@${c.x}
";
            return new EvalTestData(code, "4");
        }

        static TestData Make_Access_AccessPrivateMemberOutsideStruct_ReportError()
        {
            var code = @"
class C { int x; }
var c = new C(3);
@${c.x}
";
            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        }

        static TestData Make_Access_AccessPrivateConstructorOutsideStruct_ReportError()
        {
            var code = @"
class C { C(int x) { } }
var c = new C(3);
";
            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        }

        static TestData Make_MemberFunc_Declaration_WorksProperly()
        {
            var code = @"
class C
{
    int x;
    public int F(int y) { return x + y; }
}

var c = new C(3);
var i = c.F(2);
@$i
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

                    new S.ClassMemberVarDecl(null, SIntTypeExp(), Arr("x")),
                    new S.ClassMemberFuncDecl(S.AccessModifier.Public, false, false, false, SIntTypeExp(), "F", default, Arr(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "y")), SBlock(
                        new S.ReturnStmt(new S.ReturnValueInfo(false, new S.BinaryOpExp(S.BinaryOpKind.Add, SId("x"), SId("y"))))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "c", new S.VarDeclElemInitializer(false, new S.NewExp(SIdTypeExp("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
                    "i", new S.VarDeclElemInitializer(false, new S.CallExp(SId("c").Member("F"), Arr<S.Argument>(new S.Argument.Normal(SInt(2)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    SId("i")
                ))))))
            );

            R.Path.Nested CPath() => RRoot("TestModule").Child("C");
            R.Path.Nested CxPath() => RRoot("TestModule").Child("C").Child("x");

            var rscript = RScript(ModuleName,
                Arr<R.TypeDecl>(new R.ClassDecl(R.AccessModifier.Private, "C", default, null, default, 
                    Arr(new R.ClassConstructorDecl(
                        R.AccessModifier.Public,
                        default,
                        RNormalParams((R.Path.Int, "x")),
                        null,
                        RBlock(RAssignStmt(R.ThisLoc.Instance.ClassMember(CxPath()), RLocalVarExp("x")))
                    )),

                    Arr<R.FuncDecl>(new R.NormalFuncDecl(default, new R.Name.Normal("F"), true, default, Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, new R.Name.Normal("y"))), RBlock(
                        new R.ReturnStmt(new R.ReturnInfo.Expression(new R.CallInternalBinaryOperatorExp(
                            R.InternalBinaryOperator.Add_Int_Int_Int,
                            new R.LoadExp(new R.ClassMemberLoc(R.ThisLoc.Instance, CxPath())),
                            new R.LoadExp(RLocalVarLoc("y"))
                        )))
                    ))),

                    Arr(new R.ClassMemberVarDecl(R.AccessModifier.Private, R.Path.Int, Arr<string>("x")))
                )),

                default, default,

                RGlobalVarDeclStmt(
                    CPath(),
                    "c",
                    new R.NewClassExp(
                        CPath(),
                        new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
                    )
                ),

                // var i = s.F(2);

                RGlobalVarDeclStmt(
                    R.Path.Int,
                    "i",
                    new R.CallFuncExp(
                        CPath().Child("F", R.Path.Int),
                        new R.GlobalVarLoc("c"),
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
class C
{
    int x;
    public void SetX(int x) { this.x = x; }
    public void Print() { @{$x} }
}

var c1 = new C(3);
var c2 = c1;
c1.SetX(2);
c2.SetX(17);

c1.Print();
c2.Print();
";
            return new EvalTestData(code, "1717");
        }

        static TestData Make_Access_AccessPrivateMemberFunc_ReportError()
        {
            var code = @"
class C
{
    void F() { }
}

var c = new C();
c.F();
";
            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        }

        static TestData Make_Access_AccessPrivateMemberFuncInsideMemberFuncOfSameStruct_WorksProperly()
        {
            var code = @"
class C
{
    void E() { @{hi} }
    public void F() { E(); }
}

var c = new C();
c.F();
";

            return new EvalTestData(code, "hi");
        }

        static TestData Make_MemberSeqFunc_DependsOnThis_WorksProperly()
        {
            var code = @"
class C
{
    int x;
    public seq int F()
    {
        yield x + 1;
    }
}

var c = new C(3);

foreach(var i in c.F())
    @$i
";

            return new EvalTestData(code, "4");
        }

        static TestData Make_Inheritance_UseBase_CallBaseConstructor()
        {
            var code = @"
class B 
{ 
    public int x; 
    public B(int x) { this.x = x; }
}

class C : B
{
    public int y;
    public C(int x, int y) : base(x) { this.y = y; } // base를 부르는 방식, 항상 처음에 불리게 되며, 계산된 값이 base로 흘러가야 할 경우, static method를 쓰세요
}

var c = new C(2, 3);
@{${c.x}} // x가 제대로 들어갔는지 확인
";

            var sscript = SScript(

                // B
                new S.TypeDeclScriptElement(
                    new S.ClassDecl(null, "B", default, default, Arr<S.ClassMemberDecl>(
                        new S.ClassMemberVarDecl(S.AccessModifier.Public, SIntTypeExp(), Arr("x")),
                        new S.ClassConstructorDecl(S.AccessModifier.Public, "B", Arr(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), null, SBlock(
                            SAssignStmt(SId("this").Member("x"), SId("x"))
                        ))
                    ))
                ),

                // C
                new S.TypeDeclScriptElement(
                    new S.ClassDecl(null, "C", default, Arr<S.TypeExp>(SIdTypeExp("B")), Arr<S.ClassMemberDecl>(
                        new S.ClassMemberVarDecl(S.AccessModifier.Public, SIntTypeExp(), Arr("y")),
                        new S.ClassConstructorDecl(
                            S.AccessModifier.Public, "C",
                            SNormalFuncParams((SIntTypeExp(), "x"), (SIntTypeExp(), "y")),
                            SNormalArgs(SId("x")),
                            SBlock(SAssignStmt(SId("this").Member("y"), SId("y")))
                        )
                    ))
                ),

                // var c = new C(2, 3);                
                new S.StmtScriptElement(
                    SVarDeclStmt(SVarTypeExp(), "c", new S.NewExp(SIdTypeExp("C"), SNormalArgs(SInt(2), SInt(3))))
                ),

                // @{${c.x}}
                new S.StmtScriptElement(SCommand(SId("c").Member("x")))
            );

            var rscript = RScript(
                ModuleName,
                Arr<R.TypeDecl>(

                    // B
                    new R.ClassDecl(R.AccessModifier.Private, "B", default, null, default, 
                        Arr(new R.ClassConstructorDecl(R.AccessModifier.Public, default, RNormalParams((R.Path.Int, "x")), null, RBlock(
                            RAssignStmt(R.ThisLoc.Instance.ClassMember(RRoot(ModuleName).Child("B").Child("x")), RLocalVarExp("x"))
                        ))),

                        default,
                        Arr(new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")))
                    ),

                    // C
                    new R.ClassDecl(R.AccessModifier.Private, "C", default, RRoot(ModuleName).Child("B"), default, 
                        Arr(new R.ClassConstructorDecl(
                            R.AccessModifier.Public, default,
                            RNormalParams((R.Path.Int, "x"), (R.Path.Int, "y")),
                            new R.ConstructorBaseCallInfo(
                                RNormalParamHash(R.Path.Int),
                                RArgs(RLocalVarExp("x"))
                            ),
                            RBlock(RAssignStmt(R.ThisLoc.Instance.ClassMember(RRoot(ModuleName).Child("C").Child("y")), RLocalVarExp("y")))
                        )),

                        default,

                        Arr(new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("y")))
                    )
                ),

                default, default,

                RGlobalVarDeclStmt(RRoot(ModuleName).Child("C"), "c", new R.NewClassExp(RRoot(ModuleName).Child("C"),
                    RNormalParamHash(R.Path.Int, R.Path.Int),
                    RArgs(RInt(2), RInt(3))
                )),

                // C.x를 달라고 하면 안되고, B.x를 달라고 해야한다
                RPrintIntCmdStmt(new R.GlobalVarLoc("c").ClassMember(RRoot(ModuleName).Child("B").Child("x")))
            );

            return new ParseTranslateEvalTestData(code, sscript, rscript, "2");
        }

        static TestData Make_TrivialConstructor_BaseClassHasNonTrivialDefaultConstructor_DoesntMakeTrivialConstructor()
        {
            var code = @"
class B { public B() { @{hi} } }
class C : B { }

var c = new C();
";
            return new EvalWithErrorTestData(code, A2602_NewExp_NoMatchedClassConstructor);
        }

        // base protected, private 잘 되는지 확인
        static TestData Make_Inheritance_UsePrivateBaseConstructor_ReportError()
        {
            var code = @"
class B { B() { } }
class C : B { C() : base() { } }
";

            return new EvalWithErrorTestData(code, A2504_ClassDecl_CannotAccessBaseClassConstructor);
        }

        static TestData Make_TrivialConstructor_BaseDoenstHaveTrivialConstructor_DoesntMakeTrivialConstructor()
        {
            var code = @"
class B 
{ 
    public int x; 
    public B(int s) 
    { 
        x = s + 1;
    }
}

class C : B
{
    public int y;
}

C c = new C(2, 3);
@${c.x}${c.y}
";
            return new EvalWithErrorTestData(code, A2602_NewExp_NoMatchedClassConstructor);
        }

        static TestData Make_Inheritance_TrivialConstructor_RecognizeBaseAutoConstructor()
        {
            var code = @"
class B { public int x; }
class C : B
{
    public int y;
}

C c = new C(2, 3);
@${c.x}${c.y}
";
            return new EvalTestData(code, "23");
        }

        static TestData Make_Inheritance_Upcast_WorksProperly()
        {
            var code = @"
class B { }
class C : B { }
B b = new C();
";
            var sscript = SScript(

                // B
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "B", default, default, default)),

                // C
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", default, Arr<S.TypeExp>(SIdTypeExp("B")), default)),

                new S.StmtScriptElement(
                    SVarDeclStmt(SIdTypeExp("B"), "b", new S.NewExp(SIdTypeExp("C"), default))
                )
            );

            var rscript = RScript(
                ModuleName,

                Arr<R.TypeDecl>(
                    new R.ClassDecl(
                        R.AccessModifier.Private, "B", default, null, default,
                        Arr(new R.ClassConstructorDecl(R.AccessModifier.Public, default, default, null, RBlock())),
                        default, default
                    ),

                    new R.ClassDecl(
                        R.AccessModifier.Private, "C", default, RRoot(ModuleName).Child("B"), default,
                        Arr(new R.ClassConstructorDecl(R.AccessModifier.Public, default, default, new R.ConstructorBaseCallInfo(R.ParamHash.None, default), RBlock())),
                        default, default
                    )
                ),

                default, default,

                RGlobalVarDeclStmt(
                    RRoot(ModuleName).Child("B"), "b", 
                    new R.CastClassExp(
                        new R.NewClassExp(RRoot(ModuleName).Child("C"), R.ParamHash.None, default),
                        RRoot(ModuleName).Child("B")
                    )
                )
            );

            return new ParseTranslateTestData(code, sscript, rscript);
        }

        //        static TestData Make_Inheritance_AssertValidDowncast_WorksProperly()
        //        {
        //            var code = @"
        //class B { }
        //class C : B { }

        //void Func(B b)
        //{
        //    var c = b as C; // c is C?
        //    `notnull(c);    
        //}

        //";

        //        }

        // nullable이 먼저 만들어 져야 한다
        static TestData Make_Inheritance_TryDowncastToRelatedClass_CastProperly()
        {
            var code = @"
class B { }
class C : B { int x; } 

var b = new C(3);
var c = b as C; // C? c

if (c is not null)
{
    @${c.x}
}
";

            return new EvalTestData(code, "3");
        }

            //         static TestData Make_Inheritance_TryDowncastToUnrelatedClass_EvalsNull()
            //         {
            //             var code = @"
            // class C { } 
            // class D { }

            // var c = new C();
            // var d = c as D; // D? d
            // ";

            //         }
        }
}
