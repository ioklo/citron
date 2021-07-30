using System;
using Xunit;

using S = Gum.Syntax;
using R = Gum.IR0;

using static Gum.Infra.Misc;
using static Gum.Syntax.SyntaxFactory;
using static Gum.IR0.IR0Factory;
using static Gum.Test.IntegrateTest.Misc;
using static Gum.IR0Translator.AnalyzeErrorCode;
using System.Threading.Tasks;
using Gum.Test.Misc;

namespace Gum.Test.IntegrateTest
{
    public class T01_StructTests
    {
        public static string ModuleName = "TestModule";

        // UnitOfWorkName_ScenarioName_ExpectedBehavior

        [Fact]
        public Task F01_Declaration_DeclareMemberVar_TranslateProperly()
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

            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, default);

            var rscript = RScript(ModuleName, Arr<R.Decl>(
                new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(
                    new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr<string>("x")),
                    new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr<string>("y")),

                    new R.StructDecl.MemberDecl.Constructor(
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

            return TestParseTranslateAsync(code, sscript, rscript);
        }

        [Fact]
        public Task F02_Constructor_Declaration_WorksProperly()
        {
            var code = @"
struct S
{
    S(int x) { }
}
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(
                    // new S.VarStructDeclElement(null, IntTypeExp, Arr<string>("x")),

                    new S.ConstructorStructDeclElement(null, "S", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, IntTypeExp, "x")), SBlock(
                        // new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.MemberExp(SId("this"), "x", default), SId("x")))
                    ))
                )))
            );

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(                       

                        new R.StructDecl.MemberDecl.Constructor(
                            R.AccessModifier.Public,
                            default,
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock()
                        ),

                        new R.StructDecl.MemberDecl.Constructor(
                            R.AccessModifier.Public,
                            default,
                            default,
                            RBlock()
                        )
                    ))
                )
            );

            return TestParseTranslateAsync(code, sscript, rscript);
        }

        [Fact]
        public Task F03_Constructor_DeclareNameDifferFromStruct_ReportError()
        {
            var code = @"
struct S
{
    F() { }
}
";
            S.ConstructorStructDeclElement errorNode;
            // Parsing단계에서는 걸러내지 못하고, Translation 단계에서 걸러낸다
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(
                    errorNode = new S.ConstructorStructDeclElement(null, "F", Arr<S.FuncParam>(), SBlock())
                )))
            );

            return TestParseTranslateWithErrorAsync(code, sscript, A2402_StructDecl_CannotDeclConstructorDifferentWithTypeName, errorNode);
        }
        
        [Fact]
        public Task F04_VarDecl_UsingConstructor_CallConstructor()
        {
            var code = @"
struct S
{
    S(int x) { @{$x} } // x출력
}

var s = S(3);
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(

                    new S.ConstructorStructDeclElement(null, "S", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, IntTypeExp, "x")), SBlock(
                        new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(SId("x"))))))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, VarTypeExp, Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                )))))
            );

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(

                        new R.StructDecl.MemberDecl.Constructor(
                            R.AccessModifier.Public, 
                            default, 
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock(RCommand(RString(new R.ExpStringExpElement(
                                new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.ToString_Int_String, new R.LoadExp(new R.LocalVarLoc("x"))))
                            )))
                        ),

                        new R.StructDecl.MemberDecl.Constructor(
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

            return TestParseTranslateEvalAsync(code, sscript, rscript, result);
        }
        
        // using this
        [Fact]
        public Task F05_Constructor_UsingThis_ReferCurrentInstance()
        {
            var code = @"
struct S { int x; S(int x) { this.x = x; } }
var s = S(3);
@${s.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(

                    new S.VarStructDeclElement(null, IntTypeExp, Arr("x")),

                    new S.ConstructorStructDeclElement(null, "S", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, IntTypeExp, "x")), SBlock(
                        new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.MemberExp(SId("this"), "x", default), SId("x")))
                    ))
                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, VarTypeExp, Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    new S.MemberExp(SId("s"), "x", default)
                ))))))
            );

            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, default);

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(

                        new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr("x")),

                        new R.StructDecl.MemberDecl.Constructor(
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

            return TestParseTranslateEvalAsync(code, sscript, rscript, "3");
        }

        // 생성자를 자동으로 만들기
        [Fact]
        public Task F06_Constructor_DoesntHaveCorrespondingConstructor_MakeAutomatically()
        {
            var code = @"
struct S { int x; } // without constructor S(int x)
var s = S(3);       // but can do this
@${s.x}
"; 
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(

                    new S.VarStructDeclElement(null, IntTypeExp, Arr("x"))

                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, VarTypeExp, Arr(new S.VarDeclElement(
                    "s", new S.VarDeclElemInitializer(false, new S.CallExp(SId("S"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                ))))),

                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
                    new S.MemberExp(SId("s"), "x", default)
                ))))))
            );

            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "S", R.ParamHash.None, default);

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(

                        new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr("x")),

                        new R.StructDecl.MemberDecl.Constructor(
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

            return TestParseTranslateEvalAsync(code, sscript, rscript, "3");

        }

        [Fact]
        public Task F07_ReferenceMember_ReadAndWrite_WorksProperly()
        {
            var code = @"
struct S { int x; }
var s = S(2);
s.x = 3;
@${s.x}
";
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(

                    new S.VarStructDeclElement(null, IntTypeExp, Arr("x"))

                ))),

                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, VarTypeExp, Arr(new S.VarDeclElement(
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
                    new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(

                        new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr("x")),

                        new R.StructDecl.MemberDecl.Constructor(
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

            return TestParseTranslateEvalAsync(code, sscript, rscript, "3");
        }

        [Fact]
        public Task F08_ReferenceSelf_Assign_CopyValue()
        {
            var code = @"
struct S { int x; }
var s1 = S(2);
var s2 = S(17);
s2 = s1;
s1.x = 3;
@{${s2.x}} // 2;
";
            
            return TestEvalAsync(code, "2");


        }

        [Fact]
        public Task F09_ReferenceSelf_PassingByValue_CopyValue()
        {
            var code = @"
struct S { int x; }
void F(S s) { s.x = 2; }

var s = S(3);
F(s);
@${s.x}
";

            return TestEvalAsync(code, "3");
        }

        [Fact]
        public Task F10_ReferenceSelf_PassingByRef_ReferenceValue()
        {
            var code = @"
struct S { int x; }
void F(ref S s) { s.x = 2; }

var s = S(3);
F(ref s);
@${s.x}
";
            return TestEvalAsync(code, "2");
        }




        // default는 잠깐 미루자
        // 빈 Struct는 자동생성자가 default constructor 역할을 하기 때문에 그냥 생성 가능하다
        [Fact]
        public Task FXX_Constructor_DeclStructValueThatDoesntContainMemberVar_UseAutomaticConstructorAsDefaultConstructor()
        {
            throw new TestNeedToBeWrittenException();
        }

        //        // 초기화 구문 없을때, DefaultConstructor를 사용해서
        //        [Fact]
        //        public Task F02_VarDecl_NoInitializerWithDefaultConstructor_WorksProperly()
        //        {
        //            var code = @"
        //struct S
        //{
        //    int x;
        //    S() { x = 3; }
        //}

        //S s; // S() 호출
        //";

        //            var sscript = SScript(
        //                new S.TypeDeclScriptElement(new S.StructDecl(null, "S", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(
        //                    new S.VarStructDeclElement(null, IntTypeExp, Arr<string>("x")),
        //                    new S.ConstructorStructDeclElement(null,


        //                )))
        //            );

        //            var rscript = RScript(ModuleName, Arr<R.Decl>(
        //                new R.StructDecl(R.AccessModifier.Private, "S", Arr<string>(), Arr<R.Path>(), Arr<R.StructDecl.MemberDecl>(
        //                    new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr<string>("x")),
        //                    new R.StructDecl.MemberDecl.Var(R.AccessModifier.Public, R.Path.Int, Arr<string>("y"))
        //                ))
        //            ));

        //            return TestParseTranslate(code, sscript, rscript);

        //        }

        [Fact]
        public Task FXX_DefaultConstructor_DoesntInitializeAllMemberVariables_ReportError()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public Task FXX_DefaultConstructor_BodyHasCallExp_ReportError()
        {
            throw new TestNeedToBeWrittenException();
        }

        // S s; // S에 Default constructor가 없으면 에러
        [Fact]
        public Task FXX_VarDecl_NoInitializerWithoutDefaultConstructor_ReportError()
        {
            throw new TestNeedToBeWrittenException();
        }
    }
}
