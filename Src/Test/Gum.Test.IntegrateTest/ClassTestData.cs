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

            R.Path.Nested CPath() => new R.Path.Nested(new R.Path.Root(ModuleName), "C", R.ParamHash.None, default);

            var rscript = RScript(ModuleName, Arr<R.Decl>(
                new R.ClassDecl(R.AccessModifier.Private, "C", default, default, Arr<R.ClassMemberDecl>(
                    new R.ClassMemberVarDecl(R.AccessModifier.Private, R.Path.Int, Arr("x", "y")),
                    new R.ClassMemberVarDecl(R.AccessModifier.Public, R.Path.String, Arr("s")),
                    new R.ClassConstructorDecl(
                        R.AccessModifier.Public,
                        default,
                        Arr(
                            new R.Param(R.ParamKind.Normal, R.Path.Int, "x"),
                            new R.Param(R.ParamKind.Normal, R.Path.Int, "y"),
                            new R.Param(R.ParamKind.Normal, R.Path.String, "s")
                        ),
                        RBlock(
                            new R.ExpStmt(new R.AssignExp(
                                new R.ClassMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(CPath(), "x", R.ParamHash.None, default)),
                                new R.LoadExp(new R.LocalVarLoc("x"))
                            )),
                            new R.ExpStmt(new R.AssignExp(
                                new R.ClassMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(CPath(), "y", R.ParamHash.None, default)),
                                new R.LoadExp(new R.LocalVarLoc("y"))
                            )),
                            new R.ExpStmt(new R.AssignExp(
                                new R.ClassMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(CPath(), "s", R.ParamHash.None, default)),
                                new R.LoadExp(new R.LocalVarLoc("s"))
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
class C
{
    public C(int x) { }
}
";

            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(
                    new S.ClassConstructorDecl(S.AccessModifier.Public, "C", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), SBlock())
                )))
            );

            var rscript = RScript(ModuleName,
                Arr<R.Decl>(
                    new R.ClassDecl(R.AccessModifier.Private, "C", Arr<string>(), Arr<R.Path>(), Arr<R.ClassMemberDecl>(

                        new R.ClassConstructorDecl(
                            R.AccessModifier.Public,
                            default,
                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
                            RBlock()
                        ),

                        new R.ClassConstructorDecl(
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
class C
{
    F() { }
}
";
            S.ClassConstructorDecl errorNode;
            // Parsing단계에서는 걸러내지 못하고, Translation 단계에서 걸러낸다
            var sscript = SScript(
                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(
                    errorNode = new S.ClassConstructorDecl(null, "F", Arr<S.FuncParam>(), SBlock())
                )))
            );

            return new ParseTranslateWithErrorTestData(code, sscript, A2502_ClassDecl_CannotDeclConstructorDifferentWithTypeName, errorNode);
        }

//        static TestData Make_VarDecl_UsingConstructor_CallConstructor()
//        {
//            var code = @"
//class C
//{
//    C(int x) { @{$x} } // x출력
//}

//var c = new C(3);
//";

//            var sscript = SScript(
//                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

//                    new S.ClassConstructorDecl(null, "C", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), SBlock(
//                        new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(SId("x"))))))
//                    ))
//                ))),

//                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
//                    "C", new S.VarDeclElemInitializer(false, new S.CallExp(SId("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
//                )))))
//            );

//            var rscript = RScript(ModuleName,
//                Arr<R.Decl>(
//                    new R.StructDecl(R.AccessModifier.Private, "C", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

//                        new R.StructConstructorDecl(
//                            R.AccessModifier.Public,
//                            default,
//                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
//                            RBlock(RCommand(RString(new R.ExpStringExpElement(
//                                new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.ToString_Int_String, new R.LoadExp(new R.LocalVarLoc("x"))))
//                            )))
//                        ),

//                        new R.StructConstructorDecl(
//                            R.AccessModifier.Public,
//                            default,
//                            default,
//                            RBlock()
//                        )
//                    ))
//                ),

//                RGlobalVarDeclStmt(
//                    new R.Path.Nested(new R.Path.Root("TestModule"), "C", R.ParamHash.None, Arr<R.Path>()),
//                    "C",
//                    new R.NewStructExp(
//                        new R.Path.Nested(
//                            new R.Path.Nested(new R.Path.Root("TestModule"), "C", R.ParamHash.None, Arr<R.Path>()),
//                            R.Name.Constructor.Instance,
//                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
//                            default
//                        ),
//                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
//                    )
//                )
//            );

//            var result = "3";

//            return new ParseTranslateEvalTestData(code, sscript, rscript, result);
//        }

        //        // using this            
        //        static TestData Make_Constructor_UsingThis_ReferCurrentInstance()
        //        {
        //            var code = @"
        //struct S { int x; S(int x) { this.x = x; } }
        //var s = S(3);
        //@${s.x}
        //";
        //            var sscript = SScript(
        //                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

        //                    new S.ClassMemberVarDecl(null, SIntTypeExp(), Arr("x")),

        //                    new S.ClassConstructorDecl(null, "C", Arr<S.FuncParam>(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")), SBlock(
        //                        new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.MemberExp(SId("this"), "x", default), SId("x")))
        //                    ))
        //                ))),

        //                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
        //                    "C", new S.VarDeclElemInitializer(false, new S.CallExp(SId("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
        //                ))))),

        //                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
        //                    new S.MemberExp(SId("C"), "x", default)
        //                ))))))
        //            );

        //            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "C", R.ParamHash.None, default);

        //            var rscript = RScript(ModuleName,
        //                Arr<R.Decl>(
        //                    new R.StructDecl(R.AccessModifier.Private, "C", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

        //                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")),

        //                        new R.StructConstructorDecl(
        //                            R.AccessModifier.Public,
        //                            default,
        //                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
        //                            RBlock(
        //                                new R.ExpStmt(new R.AssignExp(
        //                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
        //                                    new R.LoadExp(new R.LocalVarLoc("x"))
        //                                )
        //                            )
        //                        )
        //                    )))
        //                ),

        //                RGlobalVarDeclStmt(
        //                    SPath(),
        //                    "C",
        //                    new R.NewStructExp(
        //                        new R.Path.Nested(
        //                            SPath(),
        //                            R.Name.Constructor.Instance,
        //                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
        //                            default
        //                        ),
        //                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
        //                    )
        //                ),

        //                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
        //                    new R.CallInternalUnaryOperatorExp(
        //                        R.InternalUnaryOperator.ToString_Int_String,
        //                        new R.LoadExp(new R.StructMemberLoc(new R.GlobalVarLoc("C"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)))
        //                    )
        //                ))))

        //            // @${ s.x}
        //            );

        //            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");
        //        }

        //        // 생성자를 자동으로 만들기
        //        static TestData Make_Constructor_DoesntHaveCorrespondingConstructor_MakeAutomatically()
        //        {
        //            var code = @"
        //struct S { int x; } // without constructor S(int x)
        //var s = S(3);       // but can do this
        //@${s.x}
        //";
        //            var sscript = SScript(
        //                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

        //                    new S.ClassMemberVarDecl(null, SIntTypeExp(), Arr("x"))

        //                ))),

        //                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
        //                    "C", new S.VarDeclElemInitializer(false, new S.CallExp(SId("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
        //                ))))),

        //                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
        //                    new S.MemberExp(SId("C"), "x", default)
        //                ))))))
        //            );

        //            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "C", R.ParamHash.None, default);

        //            var rscript = RScript(ModuleName,
        //                Arr<R.Decl>(
        //                    new R.StructDecl(R.AccessModifier.Private, "C", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

        //                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")),

        //                        new R.StructConstructorDecl(
        //                            R.AccessModifier.Public,
        //                            default,
        //                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
        //                            RBlock(
        //                                new R.ExpStmt(new R.AssignExp(
        //                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
        //                                    new R.LoadExp(new R.LocalVarLoc("x"))
        //                                )
        //                            )
        //                        )
        //                    )))
        //                ),

        //                RGlobalVarDeclStmt(
        //                    SPath(),
        //                    "C",
        //                    new R.NewStructExp(
        //                        new R.Path.Nested(
        //                            SPath(),
        //                            R.Name.Constructor.Instance,
        //                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
        //                            default
        //                        ),
        //                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
        //                    )
        //                ),

        //                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
        //                    new R.CallInternalUnaryOperatorExp(
        //                        R.InternalUnaryOperator.ToString_Int_String,
        //                        new R.LoadExp(new R.StructMemberLoc(new R.GlobalVarLoc("C"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)))
        //                    )
        //                ))))

        //            // @${ s.x}
        //            );

        //            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");

        //        }

        //        static TestData Make_ReferenceMember_ReadAndWrite_WorksProperly()
        //        {
        //            var code = @"
        //struct S { int x; }
        //var s = S(2);
        //s.x = 3;
        //@${s.x}
        //";
        //            var sscript = SScript(
        //                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

        //                    new S.ClassMemberVarDecl(null, SIntTypeExp(), Arr("x"))

        //                ))),

        //                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
        //                    "C", new S.VarDeclElemInitializer(false, new S.CallExp(SId("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(2)))))
        //                ))))),

        //                new S.StmtScriptElement(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.MemberExp(SId("C"), "x", default), SInt(3)))),

        //                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
        //                    new S.MemberExp(SId("C"), "x", default)
        //                ))))))
        //            );

        //            R.Path.Nested SPath() => new R.Path.Nested(new R.Path.Root("TestModule"), "C", R.ParamHash.None, default);

        //            var rscript = RScript(ModuleName,
        //                Arr<R.Decl>(
        //                    new R.StructDecl(R.AccessModifier.Private, "C", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(

        //                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr("x")),

        //                        new R.StructConstructorDecl(
        //                            R.AccessModifier.Public,
        //                            default,
        //                            Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "x")),
        //                            RBlock(
        //                                new R.ExpStmt(new R.AssignExp(
        //                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
        //                                    new R.LoadExp(new R.LocalVarLoc("x"))
        //                                )
        //                            )
        //                        )
        //                    )))
        //                ),

        //                // S s = S(2);
        //                RGlobalVarDeclStmt(
        //                    SPath(),
        //                    "C",
        //                    new R.NewStructExp(
        //                        new R.Path.Nested(
        //                            SPath(),
        //                            R.Name.Constructor.Instance,
        //                            new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Normal, R.Path.Int))),
        //                            default
        //                        ),
        //                        Arr<R.Argument>(new R.Argument.Normal(RInt(2)))
        //                    )
        //                ),

        //                // s.x = 3;
        //                new R.ExpStmt(new R.AssignExp(new R.StructMemberLoc(new R.GlobalVarLoc("C"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)), RInt(3))),

        //                // @${s.x}
        //                RCommand(new R.StringExp(Arr<R.StringExpElement>(new R.ExpStringExpElement(
        //                    new R.CallInternalUnaryOperatorExp(
        //                        R.InternalUnaryOperator.ToString_Int_String,
        //                        new R.LoadExp(new R.StructMemberLoc(new R.GlobalVarLoc("C"), new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)))
        //                    )
        //                ))))
        //            );

        //            return new ParseTranslateEvalTestData(code, sscript, rscript, "3");
        //        }

        //        static TestData Make_ReferenceSelf_Assign_CopyValue()
        //        {
        //            var code = @"
        //struct S { int x; }
        //var s1 = S(2);
        //var s2 = S(17);
        //s2 = s1;
        //s1.x = 3;
        //@{${s2.x}} // 2;
        //";

        //            return new EvalTestData(code, "2");
        //        }

        //        static TestData Make_ReferenceSelf_PassingByValue_CopyValue()
        //        {
        //            var code = @"
        //struct S { int x; }
        //void F(S s) { s.x = 2; }

        //var s = S(3);
        //F(s);
        //@${s.x}
        //";

        //            return new EvalTestData(code, "3");
        //        }

        //        static TestData Make_ReferenceSelf_PassingByRef_ReferenceValue()
        //        {
        //            var code = @"
        //struct S { int x; }
        //void F(ref S s) { s.x = 2; }

        //var s = S(3);
        //F(ref s);
        //@${s.x}
        //";
        //            return new EvalTestData(code, "2");
        //        }

        //        static TestData Make_Access_AccessPrivateMemberOutsideStruct_ReportError()
        //        {
        //            var code = @"
        //struct S { private int x; }
        //var s = S(3);
        //@${s.x}
        //";
        //            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        //        }

        //        static TestData Make_Access_AccessPrivateConstructorOutsideStruct_ReportError()
        //        {
        //            var code = @"
        //struct S { private S(int x) { } }
        //var s = S(3);
        //";
        //            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        //        }

        //        static TestData Make_MemberFunc_Declaration_WorksProperly()
        //        {
        //            var code = @"
        //struct S
        //{
        //    int x;
        //    int F(int y) { return x + y; }
        //}

        //var s = S(3);
        //var i = s.F(2);
        //@$i
        //";

        //            var sscript = SScript(
        //                new S.TypeDeclScriptElement(new S.ClassDecl(null, "C", Arr<string>(), Arr<S.TypeExp>(), Arr<S.ClassMemberDecl>(

        //                    new S.ClassMemberVarDecl(null, SIntTypeExp(), Arr("x")),
        //                    new S.ClassMemberFuncDecl(null, false, false, false, SIntTypeExp(), "F", default, Arr(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "y")), SBlock(
        //                        new S.ReturnStmt(new S.ReturnValueInfo(false, new S.BinaryOpExp(S.BinaryOpKind.Add, SId("x"), SId("y"))))
        //                    ))
        //                ))),

        //                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
        //                    "C", new S.VarDeclElemInitializer(false, new S.CallExp(SId("C"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
        //                ))))),

        //                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement(
        //                    "i", new S.VarDeclElemInitializer(false, new S.CallExp(new S.MemberExp(SId("C"), "F", default), Arr<S.Argument>(new S.Argument.Normal(SInt(2)))))
        //                ))))),

        //                new S.StmtScriptElement(new S.CommandStmt(Arr(new S.StringExp(Arr<S.StringExpElement>(new S.ExpStringExpElement(
        //                    SId("i")
        //                ))))))
        //            );

        //            R.Path.Nested SPath() => new R.Path.Root("TestModule").Child("C");

        //            var rscript = RScript(ModuleName,
        //                Arr<R.Decl>(
        //                    new R.StructDecl(R.AccessModifier.Private, "C", Arr<string>(), Arr<R.Path>(), Arr<R.StructMemberDecl>(
        //                        new R.StructMemberVarDecl(R.AccessModifier.Public, R.Path.Int, Arr<string>("x")),

        //                        new R.StructMemberFuncDecl(default, "F", true, default, Arr(new R.Param(R.ParamKind.Normal, R.Path.Int, "y")), RBlock(
        //                            new R.ReturnStmt(new R.ReturnInfo.Expression(new R.CallInternalBinaryOperatorExp(
        //                                R.InternalBinaryOperator.Add_Int_Int_Int,
        //                                new R.LoadExp(new R.StructMemberLoc(R.ThisLoc.Instance, SPath().Child("x"))),
        //                                new R.LoadExp(new R.LocalVarLoc("y"))
        //                            )))
        //                        )),

        //                        new R.StructConstructorDecl(
        //                            R.AccessModifier.Public,
        //                            default,
        //                            Arr(
        //                                new R.Param(R.ParamKind.Normal, R.Path.Int, "x")
        //                            ),
        //                            RBlock(
        //                                new R.ExpStmt(new R.AssignExp(
        //                                    new R.StructMemberLoc(R.ThisLoc.Instance, new R.Path.Nested(SPath(), "x", R.ParamHash.None, default)),
        //                                    new R.LoadExp(new R.LocalVarLoc("x"))
        //                                ))
        //                            )
        //                        )
        //                    ))
        //                ),

        //                RGlobalVarDeclStmt(
        //                    SPath(),
        //                    "C",
        //                    new R.NewStructExp(
        //                        SPath().Child(R.Name.Constructor.Instance, R.Path.Int),
        //                        Arr<R.Argument>(new R.Argument.Normal(RInt(3)))
        //                    )
        //                ),

        //                // var i = s.F(2);

        //                RGlobalVarDeclStmt(
        //                    R.Path.Int,
        //                    "i",
        //                    new R.CallFuncExp(
        //                        SPath().Child("F", R.Path.Int),
        //                        new R.GlobalVarLoc("C"),
        //                        Arr<R.Argument>(new R.Argument.Normal(RInt(2)))
        //                    )
        //                ),

        //                RPrintIntCmdStmt(new R.GlobalVarLoc("i"))
        //            );

        //            return new ParseTranslateEvalTestData(code, sscript, rscript, "5");
        //        }

        //        static TestData Make_MemberFunc_ModifySelf_WorksProperly()
        //        {
        //            var code = @"
        //struct S
        //{
        //    int x;
        //    void SetX(int x) { this.x = x; }
        //    void Print() { @{$x} }
        //}

        //var s1 = S(3);
        //var s2 = s1;
        //s1.SetX(2);
        //s2.SetX(17);

        //s1.Print();
        //s2.Print();
        //";
        //            return new EvalTestData(code, "217");
        //        }

        //        static TestData Make_Access_AccessPrivateMemberFunc_ReportError()
        //        {
        //            var code = @"
        //struct S
        //{
        //private void F() { }
        //}

        //var s = S();
        //s.F();
        //";
        //            return new EvalWithErrorTestData(code, A2011_ResolveIdentifier_TryAccessingPrivateMember);
        //        }

        //        static TestData Make_Access_AccessPrivateMemberFuncInsideMemberFuncOfSameStruct_WorksProperly()
        //        {
        //            var code = @"
        //struct S
        //{
        //    private void E() { @{hi} }
        //    void F() { E(); }
        //}

        //var s = S();
        //s.F();
        //";

        //            return new EvalTestData(code, "hi");

        //        }

        //        static TestData Make_MemberSeqFunc_DependsOnThis_WorksProperly()
        //        {
        //            var code = @"
        //struct S
        //{
        //    int x;
        //    seq int F()
        //    {
        //        yield x + 1;
        //    }
        //}

        //var s = S(3);

        //foreach(var i in s.F())
        //@$i
        //";

        //            return new EvalTestData(code, "4");
        //        }

    }
}
