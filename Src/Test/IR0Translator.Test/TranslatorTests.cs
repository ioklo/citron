﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

using Citron.Infra;
using System.Linq;

using S = Citron.Syntax;
using M = Citron.CompileTime;
using R = Citron.IR0;

using static Citron.IR0Translator.SyntaxAnalysisErrorCode;
using static Citron.Infra.Misc;
using static Citron.IR0Translator.Test.TestMisc;
using static Citron.Syntax.SyntaxFactory;
using static Citron.IR0.IR0Factory;
using Citron.Collections;
using Citron.Test.Misc;
using Citron.Log;

namespace Citron.IR0Translator.Test
{
    public class TranslatorTests
    {
        M.Name moduleName = new M.Name.Normal("TestModule");
        R.ModuleName rmoduleName = "TestModule";

        R.Script RScript(ImmutableArray<R.TypeDecl> globalTypeDecls, ImmutableArray<R.FuncDecl> globalFuncDecls, ImmutableArray<R.CallableMemberDecl> callableMemberDecls, params R.Stmt[] optTopLevelStmts)
        {
            return Citron.IR0.IR0Factory.RScript(rmoduleName, globalTypeDecls, globalFuncDecls, callableMemberDecls, optTopLevelStmts);
        }

        R.Script RScript(params R.Stmt[] optTopLevelStmts)
        {
            return Citron.IR0.IR0Factory.RScript(rmoduleName, optTopLevelStmts);
        }

        R.Script? Translate(S.Script syntaxScript, bool raiseAssertFailed = true)
        {
            var testLogger = new TestLogger(raiseAssertFailed);
            return Translator.Translate(moduleName, default, syntaxScript, testLogger);
        }

        List<ILog> TranslateWithErrors(S.Script syntaxScript, bool raiseAssertionFail = false)
        {
            var testLogger = new TestLogger(raiseAssertionFail);
            var _ = Translator.Translate(moduleName, default, syntaxScript, testLogger);

            return testLogger.Logs;
        }

        R.Path.Nested MakeRootPath(string name)
        {
            return new R.Path.Nested(new R.Path.Root(rmoduleName), new R.Name.Normal(name), R.ParamHash.None, default);
        }

        R.Path.Nested MakeRootPath(R.Name name)
        {
            return new R.Path.Nested(new R.Path.Root(rmoduleName), name, R.ParamHash.None, default);
        }


        R.Path.Nested MakeRootPath(string name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            return new R.Path.Nested(new R.Path.Root(rmoduleName), new R.Name.Normal(name), paramHash, typeArgs);
        }

        R.Path.Nested MakeRootPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            return new R.Path.Nested(new R.Path.Root(rmoduleName), name, paramHash, typeArgs);
        }

        // Trivial Cases
        [Fact]
        public void CommandStmt_TranslatesTrivially()
        {
            var syntaxCmdStmt = SCommand(
                SString(
                    new S.TextStringExpElement("Hello "),
                    new S.ExpStringExpElement(SString("World"))));

            var syntaxScript = SScript(syntaxCmdStmt);

            var script = Translate(syntaxScript);

            var expectedStmt = RCommand(
                RString(
                    new R.TextStringExpElement("Hello "),
                    new R.ExpStringExpElement(RString("World"))));

            var expected = RScript(expectedStmt);

            Assert.Equal(expected, script);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoGlobalVarDecl()
        {
            var syntaxScript = SScript(new S.StmtScriptElement(SVarDeclStmt(SIntTypeExp(), "x", SInt(1))));
            var script = Translate(syntaxScript);

            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Int, "x", RInt(1))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInTopLevelScope()
        {
            var syntaxScript = SScript(new S.StmtScriptElement(
                SBlock(
                    SVarDeclStmt(SIntTypeExp(), "x", SInt(1))
                )
            ));
            var script = Translate(syntaxScript);

            var expected = RScript(
                RBlock(
                    new R.LocalVarDeclStmt(new R.LocalVarDecl(Arr<R.VarDeclElement>(new R.VarDeclElement.Normal(R.Path.Int, "x", RInt(1)))))
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInFuncScope()
        {
            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(null, false, false, SVoidTypeExp(), "Func", default, default,
                    SBlock(
                        SVarDeclStmt(SIntTypeExp(), "x", SInt(1))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                default,
                Arr<R.FuncDecl>(new R.NormalFuncDecl(default, new R.Name.Normal("Func"), false, default, default, RBlock(
                    new R.LocalVarDeclStmt(new R.LocalVarDecl(Arr<R.VarDeclElement>(new R.VarDeclElement.Normal(R.Path.Int, "x", RInt(1)))))
                ))),
                default
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void VarDeclStmt_InfersVarType()
        {
            var syntaxScript = SScript(
                new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement("x", new S.VarDeclElemInitializer(false, SInt(3))))))
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Int, "x", RInt(3))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void VarDeclStmt_ChecksLocalVarNameIsUniqueWithinScope()
        {
            S.VarDeclElement elem;

            var syntaxScript = SScript(SBlock(
                new S.VarDeclStmt(SVarDecl(SIntTypeExp(), "x", null)),
                new S.VarDeclStmt(new S.VarDecl(false, SIntTypeExp(), Arr(elem = new S.VarDeclElement("x", null))))
            ));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);
        }

        [Fact]
        public void VarDeclStmt_ChecksLocalVarNameIsUniqueWithinScope2()
        {
            S.VarDeclElement element;

            var syntaxScript = SScript(SBlock(
                new S.VarDeclStmt(new S.VarDecl(false, SIntTypeExp(), Arr(new S.VarDeclElement("x", null), element = new S.VarDeclElement("x", null))))
            ));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, element);
        }

        [Fact]
        public void VarDeclStmt_ChecksGlobalVarNameIsUnique()
        {
            S.VarDeclElement elem;

            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.VarDeclStmt(new S.VarDecl(false, SIntTypeExp(), Arr(elem = new S.VarDeclElement("x", null))))

            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);
        }

        [Fact]
        public void VarDeclStmt_ChecksGlobalVarNameIsUnique2()
        {
            S.VarDeclElement elem;

            var syntaxScript = SScript(
                new S.VarDeclStmt(new S.VarDecl(false, SIntTypeExp(), Arr(
                    new S.VarDeclElement("x", null),
                    elem = new S.VarDeclElement("x", null)
                )))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);
        }

        [Fact]
        public void VarDeclStmt_ChecksLocalDeclHasInitializer()
        {
            S.VarDeclElement elem;
            var syntaxScript = SScript(SBlock(
                new S.VarDeclStmt(new S.VarDecl(false, SIntTypeExp(), Arr(
                    elem = new S.VarDeclElement("x", null)
                )))
            ));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0111_VarDecl_LocalVarDeclNeedInitializer, elem);
        }

        [Fact]
        public void IfStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(new S.StmtScriptElement(

                new S.IfStmt(new S.BoolLiteralExp(false), S.BlankStmt.Instance, S.BlankStmt.Instance)

            ));

            var script = Translate(syntaxScript);

            var expected = RScript(

                new R.IfStmt(new R.BoolLiteralExp(false), R.BlankStmt.Instance, R.BlankStmt.Instance)

            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void IfStmt_ReportsErrorWhenCondTypeIsNotBool()
        {
            S.Exp cond;

            var syntaxScript = SScript(new S.StmtScriptElement(

                new S.IfStmt(cond = SInt(3), S.BlankStmt.Instance, S.BlankStmt.Instance)

            ));

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1001_IfStmt_ConditionShouldBeBool, cond);
        }

        [Fact]
        public void IfStmt_TranslatesIntoIfTestClassStmt()
        {
            // Prerequisite
            throw new PrerequisiteRequiredException(Prerequisite.Class);
        }

        [Fact]
        public void IfStmt_TranslatesIntoIfTestEnumStmt()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }

        [Fact]
        public void ForStmt_TranslatesInitializerTrivially()
        {
            var syntaxScript = SScript(

                new S.StmtScriptElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SVarDecl(SIntTypeExp(), "x", SInt(0))),
                    null, null, S.BlankStmt.Instance
                )),

                new S.StmtScriptElement(SVarDeclStmt(SStringTypeExp(), "x")),

                new S.StmtScriptElement(new S.ForStmt(
                    new S.ExpForStmtInitializer(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SString("Hello"))),
                    null, null, S.BlankStmt.Instance
                ))
            );

            var script = Translate(syntaxScript);

            var expected = RScript(

                new R.ForStmt(
                    new R.VarDeclForStmtInitializer(RLocalVarDecl(R.Path.Int, "x", RInt(0))),
                    null, null, R.BlankStmt.Instance
                ),

                RGlobalVarDeclStmt(R.Path.String, "x", null),

                new R.ForStmt(
                    new R.ExpForStmtInitializer(new R.AssignExp(new R.GlobalVarLoc("x"), RString("Hello"))),
                    null, null, R.BlankStmt.Instance
                )
            );

            Assert.Equal(expected, script);
        }

        // 
        [Fact]
        public void ForStmt_ChecksVarDeclInitializerScope()
        {
            var syntaxScript = SScript(

                new S.StmtScriptElement(SVarDeclStmt(SStringTypeExp(), "x")),

                new S.StmtScriptElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SVarDecl(SIntTypeExp(), "x", SInt(0))), // x의 범위는 ForStmt내부에서
                    new S.BinaryOpExp(S.BinaryOpKind.Equal, SId("x"), SInt(3)),
                    null, S.BlankStmt.Instance
                )),

                new S.StmtScriptElement(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SString("Hello"))))
            );

            var script = Translate(syntaxScript);

            var expected = RScript(

                RGlobalVarDeclStmt(R.Path.String, "x", null),

                new R.ForStmt(
                    new R.VarDeclForStmtInitializer(RLocalVarDecl(R.Path.Int, "x", RInt(0))),

                    // cond
                    new R.CallInternalBinaryOperatorExp(
                        R.InternalBinaryOperator.Equal_Int_Int_Bool,
                        new R.LoadExp(new R.LocalVarLoc(new R.Name.Normal("x"))),
                        RInt(3)
                    ),
                    null, R.BlankStmt.Instance
                ),

                new R.ExpStmt(new R.AssignExp(new R.GlobalVarLoc("x"), RString("Hello")))
            );

            Assert.Equal(expected, script);
        }


        [Fact]
        public void ForStmt_ChecksConditionIsBool()
        {
            S.Exp cond;

            var syntaxScript = SScript(
                new S.StmtScriptElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SVarDecl(SIntTypeExp(), "x", SInt(0))),
                    cond = SInt(3),
                    null, S.BlankStmt.Instance
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A1101_ForStmt_ConditionShouldBeBool, cond);
        }


        [Fact]
        public void ForStmt_ChecksExpInitializerIsAssignOrCall()
        {
            S.Exp exp;

            var syntaxScript = SScript(
                new S.StmtScriptElement(new S.ForStmt(
                    new S.ExpForStmtInitializer(exp = SInt(3)), // error
                    null, null, S.BlankStmt.Instance
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall, exp);
        }

        [Fact]
        public void ForStmt_ChecksContinueExpIsAssignOrCall()
        {
            S.Exp continueExp;

            var syntaxScript = SScript(

                new S.StmtScriptElement(new S.ForStmt(
                    null,
                    null,
                    continueExp = SInt(3),
                    S.BlankStmt.Instance
                ))

            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1103_ForStmt_ContinueExpShouldBeAssignOrCall, continueExp);
        }

        [Fact]
        public void ContinueStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                new S.ForStmt(null, null, null, S.ContinueStmt.Instance),
                new S.ForeachStmt(false, SIntTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), S.ContinueStmt.Instance)
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                new R.ForStmt(null, null, null, R.ContinueStmt.Instance),
                new R.ForeachStmt(R.Path.Int, "x", 
                    new R.TempLoc(
                        new R.ListIteratorExp(new R.TempLoc(new R.ListExp(R.Path.Int, default), R.Path.List(R.Path.Int))),
                        RRoot("System.Runtime").Child("System").Child("List", new R.ParamHash(1, default), Arr(R.Path.Int)).Child(new R.Name.Anonymous(0))
                    ),
                    R.ContinueStmt.Instance
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void ContinueStmt_ChecksUsedInLoop()
        {
            S.ContinueStmt continueStmt;
            var syntaxScript = SScript(continueStmt = S.ContinueStmt.Instance);

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);
        }

        [Fact]
        public void BreakStmt_WithinNestedForAndForeachLoopTranslatesTrivially()
        {
            // for(;;;) 
            //     foreach(int x in <int>[]) break;
            var syntaxScript = SScript(
                new S.ForStmt(null, null, null, 
                    new S.ForeachStmt(false, SIntTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), S.BreakStmt.Instance)
                )
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                new R.ForStmt(null, null, null, 
                    new R.ForeachStmt(R.Path.Int, "x", 
                        new R.TempLoc(
                            new R.ListIteratorExp(new R.TempLoc(new R.ListExp(R.Path.Int, default), R.Path.List(R.Path.Int))),
                            RRoot("System.Runtime").Child("System").Child("List", new R.ParamHash(1, default), Arr(R.Path.Int)).Child(new R.Name.Anonymous(0))
                        ),
                        R.BreakStmt.Instance
                    )
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void BreakStmt_WithinForLoopTranslatesTrivially()
        {
            var syntaxScript = SScript(
                new S.ForStmt(null, null, null, S.BreakStmt.Instance)
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                new R.ForStmt(null, null, null, R.BreakStmt.Instance)
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void BreakStmt_ChecksUsedInLoop()
        {
            S.BreakStmt breakStmt;
            var syntaxScript = SScript(breakStmt = S.BreakStmt.Instance);

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1601_BreakStmt_ShouldUsedInLoop, breakStmt);
        }

        [Fact]
        public void ReturnStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(new S.ReturnStmt(new S.ReturnValueInfo(false, SInt(2))));

            var script = Translate(syntaxScript);
            var expected = RScript(new R.ReturnStmt(new R.ReturnInfo.Expression(RInt(2))));

            Assert.Equal(expected, script);
        }

        [Fact]
        public void ReturnStmt_TranslatesReturnStmtInSeqFuncTrivially()
        {
            var syntaxScript = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                null,
                true, false, SIntTypeExp(), "Func", default, default,
                SBlock(
                    new S.ReturnStmt(null)
                )
            )));

            var script = Translate(syntaxScript);

            var seqFunc = new R.SequenceFuncDecl(default, new R.Name.Normal("Func"), false, R.Path.Int, default, default, RBlock(new R.ReturnStmt(R.ReturnInfo.None.Instance)));            

            var expected = RScript(default, Arr<R.FuncDecl>(seqFunc), default);

            Assert.Equal(expected, script);
        }

        [Fact]
        public void ReturnStmt_ChecksMatchFuncRetTypeAndRetValue()
        {
            S.Exp retValue;

            var funcDecl = new S.GlobalFuncDecl(null, false, false, SIntTypeExp(), "Func", default, default, SBlock(
                new S.ReturnStmt(new S.ReturnValueInfo(false, retValue = SString("Hello")))
            ));

            var syntaxScript = SScript(new S.GlobalFuncDeclScriptElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A2201_Cast_Failed, retValue);
        }

        [Fact]
        public void ReturnStmt_ChecksMatchVoidTypeAndReturnNothing()
        {
            S.ReturnStmt retStmt;

            var funcDecl = new S.GlobalFuncDecl(null, false, false, SIntTypeExp(), "Func", default, default, SBlock(
                retStmt = new S.ReturnStmt(null)
            ));

            var syntaxScript = SScript(new S.GlobalFuncDeclScriptElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, retStmt);
        }

        [Fact]
        public void ReturnStmt_ChecksSeqFuncShouldReturnNothing()
        {
            S.ReturnStmt retStmt;

            var funcDecl = new S.GlobalFuncDecl(null, true, false, SIntTypeExp(), "Func", default, default, SBlock(
                retStmt = new S.ReturnStmt(new S.ReturnValueInfo(false, SInt(2)))
            ));

            var syntaxScript = SScript(new S.GlobalFuncDeclScriptElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1202_ReturnStmt_SeqFuncShouldReturnVoid, retStmt);
        }

        [Fact]
        public void ReturnStmt_ShouldReturnIntWhenUsedInTopLevelStmt()
        {
            S.Exp exp;
            var syntaxScript = SScript(new S.ReturnStmt(new S.ReturnValueInfo(false, exp = SString("Hello"))));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A2201_Cast_Failed, exp);
        }

        [Fact]
        public void ReturnStmt_UsesHintType()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum, Prerequisite.TypeHint);
        }

        [Fact]
        public void BlockStmt_TranslatesVarDeclStmtWithinBlockStmtOfTopLevelStmtIntoLocalVarDeclStmt()
        {
            var syntaxScript = SScript(
                SBlock(
                    SVarDeclStmt(SStringTypeExp(), "x", SString("Hello"))
                )
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                RBlock(
                    RLocalVarDeclStmt(R.Path.String, "x", RString("Hello")) // not GlobalVarDecl
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void BlockStmt_ChecksIsolatingOverridenTypesOfVariables()
        {
            throw new PrerequisiteRequiredException(Prerequisite.IfTestClassStmt, Prerequisite.IfTestEnumStmt);
        }

        [Fact]
        public void BlockStmt_ChecksLocalVariableScope()
        {
            S.Exp exp;

            var syntaxScript = SScript(
                SBlock(
                    SVarDeclStmt(SStringTypeExp(), "x", SString("Hello"))
                ),

                SCommand(SString(new S.ExpStringExpElement(exp = SId("x"))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A2007_ResolveIdentifier_NotFound, exp);
        }

        [Fact]
        public void ExpStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SInt(3)))
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Int, "x", null),
                new R.ExpStmt(new R.AssignExp(new R.GlobalVarLoc("x"), RInt(3)))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void ExpStmt_ChecksExpIsAssignOrCall()
        {
            S.Exp exp;
            var syntaxScript = SScript(
                new S.ExpStmt(exp = SInt(3))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1301_ExpStmt_ExpressionShouldBeAssignOrCall, exp);
        }

        [Fact]
        public void TaskStmt_TranslatesWithGlobalVariable()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.TaskStmt(
                    new S.ExpStmt(
                        new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SInt(3))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var name = new R.Name.Anonymous(0);

            var capturedStmtDecl = new R.CapturedStatementDecl(
                name,
                new R.CapturedStatement(
                    null,
                    default,
                    new R.ExpStmt(new R.AssignExp(new R.GlobalVarLoc("x"), RInt(3))))
            );

            var path = MakeRootPath(name);

            var expected = RScript(
                default,
                default,
                Arr<R.CallableMemberDecl>(capturedStmtDecl),
                RGlobalVarDeclStmt(R.Path.Int, "x", null),
                new R.TaskStmt(path)
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void TaskStmt_ChecksAssignToLocalVariableOutsideLambda()
        {
            S.Exp exp;

            var syntaxScript = SScript(
                SBlock(
                    SVarDeclStmt(SIntTypeExp(), "x"),
                    new S.TaskStmt(
                        new S.ExpStmt(
                            new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SId("x"), SInt(3))
                        )
                    )
                )
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0803_BinaryOp_LeftOperandIsNotAssignable, exp);
        }

        [Fact]
        public void TaskStmt_TranslatesWithLocalVariable()
        {
            // 
            // {
            //     int x = 1;
            //     task { int y = x; }
            // }
            var syntaxScript = SScript(
                SBlock(
                    SVarDeclStmt(SIntTypeExp(), "x", SInt(1)),
                    new S.TaskStmt(
                        SVarDeclStmt(SIntTypeExp(), "y", SId("x"))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var name0 = new R.Name.Anonymous(0);
            var capturedStatementDecl = new R.CapturedStatementDecl(
                name0,
                new R.CapturedStatement(
                    null,
                    Arr(new R.OuterLocalVarInfo(R.Path.Int, "x")),
                    RLocalVarDeclStmt(R.Path.Int, "y", new R.LoadExp(new R.LambdaMemberVar("x")))
                )
            );

            var expected = RScript(
                default, default,
                Arr<R.CallableMemberDecl>(capturedStatementDecl),
                RBlock(
                    RLocalVarDeclStmt(R.Path.Int, "x", RInt(1)),
                    new R.TaskStmt(
                        MakeRootPath(name0)
                    )
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void AwaitStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                new S.AwaitStmt(
                    S.BlankStmt.Instance
                )
            );

            var script = Translate(syntaxScript);

            var expected = RScript(new R.AwaitStmt(R.BlankStmt.Instance));

            Assert.Equal(expected, script);
        }

        [Fact]
        public void AwaitStmt_ChecksLocalVariableScope()
        {
            S.Exp exp;

            var syntaxScript = SScript(
                new S.AwaitStmt(
                    SVarDeclStmt(SStringTypeExp(), "x", SString("Hello"))
                ),

                SCommand(SString(new S.ExpStringExpElement(exp = SId("x"))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A2007_ResolveIdentifier_NotFound, exp);
        }

        [Fact]
        public void AsyncStmt_TranslatesWithGlobalVariable()
        {
            // 
            // int x;
            // async { x = 3; }

            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.AsyncStmt(
                    new S.ExpStmt(
                        new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SInt(3))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var capturedStatementName = new R.Name.Anonymous(0);

            var capturedStatementDecl = new R.CapturedStatementDecl(
                capturedStatementName,
                new R.CapturedStatement(
                    null,
                    default,
                    new R.ExpStmt(new R.AssignExp(new R.GlobalVarLoc("x"), RInt(3)))
                )
            );

            var expected = RScript(
                default, default,
                Arr<R.CallableMemberDecl>(capturedStatementDecl),
                RGlobalVarDeclStmt(R.Path.Int, "x", null),
                new R.AsyncStmt(MakeRootPath(capturedStatementName))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void AsyncStmt_ChecksAssignToLocalVariableOutsideLambda()
        {
            S.Exp exp;

            var syntaxScript = SScript(
                SBlock(
                    SVarDeclStmt(SIntTypeExp(), "x", SInt(0)),
                    new S.AsyncStmt(
                        new S.ExpStmt(
                            new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SId("x"), SInt(3))
                        )
                    )
                )
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0803_BinaryOp_LeftOperandIsNotAssignable, exp);
        }

        [Fact]
        public void AsyncStmt_TranslatesWithLocalVariable()
        {
            // int x = 0;
            // async { int x = x; }
            var syntaxScript = SScript(
                SBlock(
                    SVarDeclStmt(SIntTypeExp(), "x", SInt(0)),
                    new S.AsyncStmt(
                        SVarDeclStmt(SIntTypeExp(), "x", SId("x"))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var capturedStmtName = new R.Name.Anonymous(0);
            var capturedStmtDecl = new R.CapturedStatementDecl(capturedStmtName, new R.CapturedStatement(
                default,
                Arr(new R.OuterLocalVarInfo(R.Path.Int, "x")),
                RLocalVarDeclStmt(R.Path.Int, "x", new R.LoadExp(new R.LambdaMemberVar("x")))
            ));

            var expected = RScript(
                default, default,
                Arr<R.CallableMemberDecl>(capturedStmtDecl),
                RBlock(
                    RLocalVarDeclStmt(R.Path.Int, "x", RInt(0)),
                    new R.AsyncStmt(
                        MakeRootPath(capturedStmtName)
                    )
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void ForeachStmt_TranslatesTrivially()
        {
            var scriptSyntax = SScript(new S.ForeachStmt(false, SIntTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), S.BlankStmt.Instance));

            var script = Translate(scriptSyntax);

            var expected = RScript(new R.ForeachStmt(
                R.Path.Int,
                "x",
                new R.TempLoc(
                    new R.ListIteratorExp(new R.TempLoc(new R.ListExp(R.Path.Int, default), R.Path.List(R.Path.Int))),
                    RRoot("System.Runtime").Child("System").Child("List", new R.ParamHash(1, default), Arr(R.Path.Int)).Child(new R.Name.Anonymous(0))
                ),
                R.BlankStmt.Instance
            ));

            Assert.Equal(expected, script);
        }

        [Fact]
        public void ForeachStmt_ChecksIteratorIsListOrEnumerable()
        {
            S.Exp iterator;
            var scriptSyntax = SScript(new S.ForeachStmt(false, SIntTypeExp(), "x", iterator = SInt(3), S.BlankStmt.Instance));

            var errors = TranslateWithErrors(scriptSyntax);

            VerifyError(errors, A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, iterator);
        }

        [Fact]
        public void ForeachStmt_ChecksIteratorIsListOrEnumerable2()
        {
            // iterator type이 normal type이 아닐때.. 재현하기 쉽지 않은것 같다
            throw new PrerequisiteRequiredException(Prerequisite.Generics);
        }

        [Fact]
        public void ForeachStmt_ChecksElemTypeIsAssignableFromIteratorElemType()
        {
            S.ForeachStmt foreachStmt;
            var scriptSyntax = SScript(foreachStmt = new S.ForeachStmt(false, SStringTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), S.BlankStmt.Instance));

            var errors = TranslateWithErrors(scriptSyntax);

            VerifyError(errors, A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType, foreachStmt);
        }

        [Fact]
        public void YieldStmt_TranslatesTrivially()
        {
            // seq int Func() { yield 3; }
            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    null, true, false, SIntTypeExp(), "Func", default, default,
                    SBlock(
                        new S.YieldStmt(SInt(3))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var seqFunc = new R.SequenceFuncDecl(default, new R.Name.Normal("Func"), false, R.Path.Int, default, default, RBlock(
                new R.YieldStmt(RInt(3))
            ));

            var expected = RScript(default, Arr<R.FuncDecl>(seqFunc), default);

            Assert.Equal(expected, script);
        }

        [Fact]
        public void YieldStmt_ChecksYieldStmtUsedInSeqFunc()
        {
            S.YieldStmt yieldStmt;

            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    null, false, false, SIntTypeExp(), "Func", default, default,
                    SBlock(
                        yieldStmt = new S.YieldStmt(SInt(3))
                    )
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt);
        }

        [Fact]
        public void YieldStmt_ChecksMatchingYieldType()
        {
            S.Exp yieldValue;

            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    null, true, false, SStringTypeExp(), "Func", default, default,
                    SBlock(
                        new S.YieldStmt(yieldValue = SInt(3))
                    )
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A2201_Cast_Failed, yieldValue);
        }

        [Fact]
        public void YieldStmt_UsesHintTypeValue()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum, Prerequisite.TypeHint);
        }

        // IdExp
        [Fact]
        public void IdExp_TranslatesIntoExternalGlobal()
        {
            throw new PrerequisiteRequiredException(Prerequisite.External);
        }

        [Fact]
        public void IdExp_TranslatesIntoGlobalVarExp()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x", SInt(3)),
                SVarDeclStmt(SIntTypeExp(), "y", SId("x"))
            );

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Int, "x", RInt(3)),
                RGlobalVarDeclStmt(R.Path.Int, "y", new R.LoadExp(new R.GlobalVarLoc("x")))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void IdExp_TranslatesLocalVarOutsideLambdaIntoLocalVarExp()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public void IdExp_TranslatesIntoLocalVarExp()
        {
            var syntaxScript = SScript(SBlock(
                SVarDeclStmt(SIntTypeExp(), "x", SInt(3)),
                SVarDeclStmt(SIntTypeExp(), "y", SId("x"))
            ));

            var script = Translate(syntaxScript);
            var expected = RScript(RBlock(
                RLocalVarDeclStmt(R.Path.Int, "x", RInt(3)),
                RLocalVarDeclStmt(R.Path.Int, "y", new R.LoadExp(new R.LocalVarLoc(new R.Name.Normal("x"))))
            ));

            Assert.Equal(expected, script);
        }

        [Fact]
        public void IdExp_TranslatesIntoStaticMemberExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Static);
        }

        [Fact]
        public void IdExp_TranslatesIntoClassMemberExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Class);
        }

        [Fact]
        public void IdExp_TranslatesIntoStructMemberExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Struct);
        }

        [Fact]
        public void IdExp_TranslatesIntoEnumMemberExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }

        [Fact]
        public void IdExp_TranslatesIntoInterfaceMemberExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Interface);
        }

        [Fact]
        public void IdExp_TranslatesIntoNewEnumExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }

        [Fact]
        public void BoolLiteralExp_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SBoolTypeExp(), "b1", new S.BoolLiteralExp(false)),
                SVarDeclStmt(SBoolTypeExp(), "b2", new S.BoolLiteralExp(true)));

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Bool, "b1", new R.BoolLiteralExp(false)),
                RGlobalVarDeclStmt(R.Path.Bool, "b2", new R.BoolLiteralExp(true))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void IntLiteralExp_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "i", new S.IntLiteralExp(34)));

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Int, "i", new R.IntLiteralExp(34))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void StringExp_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "s", SString("Hello")));

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.String, "s", RString("Hello"))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void StringExp_WrapsExpStringExpElementWhenExpIsBoolOrInt()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "s1", SString(new S.ExpStringExpElement(SInt(3)))),
                SVarDeclStmt(SStringTypeExp(), "s2", SString(new S.ExpStringExpElement(SBool(true))))
            );

            var script = Translate(syntaxScript, true);

            var expected = RScript(

                RGlobalVarDeclStmt(R.Path.String, "s1", RString(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.ToString_Int_String, RInt(3))
                ))),

                RGlobalVarDeclStmt(R.Path.String, "s2", RString(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.ToString_Bool_String, RBool(true))
                )))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void StringExp_ChecksStringExpElementIsStringConvertible()
        {
            S.Exp exp;
            // convertible가능하지 않은 것,, Lambda?
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "s", SString(new S.ExpStringExpElement(exp = new S.LambdaExp(
                    Arr<S.LambdaExpParam>(),
                    S.BlankStmt.Instance
                ))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, exp);
        }

        [Fact]
        public void UnaryOpExp_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "x1", new S.UnaryOpExp(S.UnaryOpKind.LogicalNot, SBool(false))),
                SVarDeclStmt(SVarTypeExp(), "x2", new S.UnaryOpExp(S.UnaryOpKind.Minus, SInt(3))),
                SVarDeclStmt(SVarTypeExp(), "x3", new S.UnaryOpExp(S.UnaryOpKind.PrefixInc, SId("x2"))),
                SVarDeclStmt(SVarTypeExp(), "x4", new S.UnaryOpExp(S.UnaryOpKind.PrefixDec, SId("x2"))),
                SVarDeclStmt(SVarTypeExp(), "x5", new S.UnaryOpExp(S.UnaryOpKind.PostfixInc, SId("x2"))),
                SVarDeclStmt(SVarTypeExp(), "x6", new S.UnaryOpExp(S.UnaryOpKind.PostfixDec, SId("x2")))
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Bool, "x1", new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool, RBool(false))),
                RGlobalVarDeclStmt(R.Path.Int, "x2", new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.UnaryMinus_Int_Int, RInt(3))),
                RGlobalVarDeclStmt(R.Path.Int, "x3", new R.CallInternalUnaryAssignOperatorExp(R.InternalUnaryAssignOperator.PrefixInc_Int_Int, new R.GlobalVarLoc("x2"))),
                RGlobalVarDeclStmt(R.Path.Int, "x4", new R.CallInternalUnaryAssignOperatorExp(R.InternalUnaryAssignOperator.PrefixDec_Int_Int, new R.GlobalVarLoc("x2"))),
                RGlobalVarDeclStmt(R.Path.Int, "x5", new R.CallInternalUnaryAssignOperatorExp(R.InternalUnaryAssignOperator.PostfixInc_Int_Int, new R.GlobalVarLoc("x2"))),
                RGlobalVarDeclStmt(R.Path.Int, "x6", new R.CallInternalUnaryAssignOperatorExp(R.InternalUnaryAssignOperator.PostfixDec_Int_Int, new R.GlobalVarLoc("x2")))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfUnaryAssignExpIsIntType()
        {
            S.Exp operand;
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "x", SString("Hello")),
                SVarDeclStmt(SVarTypeExp(), "i", new S.UnaryOpExp(S.UnaryOpKind.PrefixInc, operand = SId("x")))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfUnaryAssignExpIsAssignable()
        {
            S.Exp operand;
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "i", new S.UnaryOpExp(S.UnaryOpKind.PrefixInc, operand = SInt(3)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfLogicalNotIsBoolType()
        {
            S.Exp operand;
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "b", new S.UnaryOpExp(S.UnaryOpKind.LogicalNot, operand = SInt(3)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, operand);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfUnaryMinusIsIntType()
        {
            S.Exp operand;
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "i", new S.UnaryOpExp(S.UnaryOpKind.Minus, operand = SBool(false)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, operand);
        }

        [Fact]
        void BinaryOpExp_TranslatesIntoAssignExp()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SInt(3)))
            );

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Int, "x"),
                new R.ExpStmt(new R.AssignExp(new R.GlobalVarLoc("x"), RInt(3)))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        void BinaryOpExp_AssigningToRefVar_TranslatesDerefBothSides()
        {
            // int x = 3;
            // var i = ref x;
            // i = 7 + i;

            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x", SInt(3)),
                new S.VarDeclStmt(new S.VarDecl(false, SVarTypeExp(), Arr(new S.VarDeclElement("i", new S.VarDeclElemInitializer(true, SId("x")))))),
                new S.ExpStmt(new S.BinaryOpExp(
                    S.BinaryOpKind.Assign,
                    SId("i"),
                    new S.BinaryOpExp(S.BinaryOpKind.Add, SInt(7), SId("i"))
                ))
            );

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Int, "x", RInt(3)),
                RGlobalRefVarDeclStmt("i", new R.GlobalVarLoc("x")),
                new R.ExpStmt(
                    new R.AssignExp(
                        new R.DerefLocLoc(new R.GlobalVarLoc("i")),
                        new R.CallInternalBinaryOperatorExp(R.InternalBinaryOperator.Add_Int_Int_Int, RInt(7), new R.LoadExp(new R.DerefLocLoc(new R.GlobalVarLoc("i"))))
                    )
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        void BinaryOpExp_ChecksCompatibleBetweenOperandsOnAssignOperation()
        {
            S.BinaryOpExp binOpExp;
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.ExpStmt(binOpExp = new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SBool(true)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A2201_Cast_Failed, binOpExp);
        }

        [Fact]
        void BinaryOpExp_ChecksLeftOperandIsAssignableOnAssignOperation()
        {
            S.Exp exp;
            var syntaxScript = SScript(
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SInt(3), SInt(4)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0803_BinaryOp_LeftOperandIsNotAssignable, exp);
        }

        [Fact]
        void BinaryOpExp_ChecksOperatorNotFound()
        {
            S.Exp exp;
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "x", exp = new S.BinaryOpExp(S.BinaryOpKind.Multiply, SString("Hello"), SInt(4)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0802_BinaryOp_OperatorNotFound, exp);
        }

        static IEnumerable<object[]> Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial()
        {
            Func<S.Exp> MakeInt = () => SInt(1);
            Func<R.Exp> MakeRInt = () => RInt(1);

            yield return new object[] { S.BinaryOpKind.Multiply, MakeInt, MakeRInt, R.Path.Int, R.InternalBinaryOperator.Multiply_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Divide, MakeInt, MakeRInt, R.Path.Int, R.InternalBinaryOperator.Divide_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Modulo, MakeInt, MakeRInt, R.Path.Int, R.InternalBinaryOperator.Modulo_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Add, MakeInt, MakeRInt, R.Path.Int, R.InternalBinaryOperator.Add_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Subtract, MakeInt, MakeRInt, R.Path.Int, R.InternalBinaryOperator.Subtract_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.LessThan, MakeInt, MakeRInt, R.Path.Bool, R.InternalBinaryOperator.LessThan_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThan, MakeInt, MakeRInt, R.Path.Bool, R.InternalBinaryOperator.GreaterThan_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.LessThanOrEqual, MakeInt, MakeRInt, R.Path.Bool, R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThanOrEqual, MakeInt, MakeRInt, R.Path.Bool, R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.Equal, MakeInt, MakeRInt, R.Path.Bool, R.InternalBinaryOperator.Equal_Int_Int_Bool };

            Func<S.Exp> MakeString = () => SString("Hello");
            Func<R.Exp> MakeRString = () => RString("Hello");

            yield return new object[] { S.BinaryOpKind.Add, MakeString, MakeRString, R.Path.String, R.InternalBinaryOperator.Add_String_String_String };
            yield return new object[] { S.BinaryOpKind.LessThan, MakeString, MakeRString, R.Path.Bool, R.InternalBinaryOperator.LessThan_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThan, MakeString, MakeRString, R.Path.Bool, R.InternalBinaryOperator.GreaterThan_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.LessThanOrEqual, MakeString, MakeRString, R.Path.Bool, R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThanOrEqual, MakeString, MakeRString, R.Path.Bool, R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.Equal, MakeString, MakeRString, R.Path.Bool, R.InternalBinaryOperator.Equal_String_String_Bool };

            Func<S.Exp> MakeBool = () => SBool(true);
            Func<R.Exp> MakeRBool = () => RBool(true);

            yield return new object[] { S.BinaryOpKind.Equal, MakeBool, MakeRBool, R.Path.Bool, R.InternalBinaryOperator.Equal_Bool_Bool_Bool };

            // NotEqual
        }

        [Theory]
        [MemberData(nameof(Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial))]
        public void BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial(
            S.BinaryOpKind syntaxOpKind, Func<S.Exp> newSOperand, Func<R.Exp> newOperandInfo, R.Path resultType, R.InternalBinaryOperator ir0BinOp)
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "x", new S.BinaryOpExp(syntaxOpKind, newSOperand.Invoke(), newSOperand.Invoke()))
            );

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(resultType, "x", new R.CallInternalBinaryOperatorExp(ir0BinOp,
                    newOperandInfo.Invoke(),
                    newOperandInfo.Invoke()
                ))
            );

            Assert.Equal(expected, script);
        }

        static IEnumerable<object[]> Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual()
        {
            Func<S.Exp> MakeBool = () => SBool(true);
            Func<R.Exp> MakeRBool = () => RBool(true);

            Func<S.Exp> MakeInt = () => SInt(1);
            Func<R.Exp> MakeRInt = () => RInt(1);

            Func<S.Exp> MakeString = () => SString("Hello");
            Func<R.Exp> MakeRString = () => RString("Hello");

            yield return new object[] { MakeBool, MakeRBool, R.InternalBinaryOperator.Equal_Bool_Bool_Bool };
            yield return new object[] { MakeInt, MakeRInt, R.InternalBinaryOperator.Equal_Int_Int_Bool };
            yield return new object[] { MakeString, MakeRString, R.InternalBinaryOperator.Equal_String_String_Bool };
        }

        [Theory]
        [MemberData(nameof(Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual))]
        public void BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual(
            Func<S.Exp> newSOperand, Func<R.Exp> newOperandInfo, R.InternalBinaryOperator ir0BinOperator)
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "x", new S.BinaryOpExp(S.BinaryOpKind.NotEqual, newSOperand.Invoke(), newSOperand.Invoke()))
            );

            var script = Translate(syntaxScript);
            var expected = RScript(
                RGlobalVarDeclStmt(R.Path.Bool, "x",
                    new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                        new R.CallInternalBinaryOperatorExp(ir0BinOperator, newOperandInfo.Invoke(), newOperandInfo.Invoke())
                    )
                )
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        void CallExp_RefArgumentTrivial_WorksProperly()
        {
            //void F(ref int i)
            //{
            //    i = 3;
            //}

            //int j = 3;
            //F(ref j);

            //@$j
            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    null, false, false, SVoidTypeExp(), "F", default,
                    Arr(new S.FuncParam(S.FuncParamKind.Ref, SIntTypeExp(), "i")),
                    SBlock(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("i"), SInt(3))))
                )),

                new S.StmtScriptElement(SVarDeclStmt(SIntTypeExp(), "j", SInt(3))),
                new S.StmtScriptElement(new S.ExpStmt(new S.CallExp(SId("F"), Arr<S.Argument>(new S.Argument.Ref(SId("j"))))))
            );

            var script = Translate(syntaxScript);
            var expected = RScript(
                default,

                Arr<R.FuncDecl>(
                    new R.NormalFuncDecl(
                        default, new R.Name.Normal("F"), false, default, Arr(new R.Param(R.ParamKind.Ref, R.Path.Int, new R.Name.Normal("i"))),
                        RBlock(new R.ExpStmt(new R.AssignExp(new R.DerefLocLoc(new R.LocalVarLoc(new R.Name.Normal("i"))), RInt(3))))
                    )
                ),

                default,

                RGlobalVarDeclStmt(R.Path.Int, "j", RInt(3)),
                new R.ExpStmt(new R.CallFuncExp(
                    MakeRootPath(new R.Name.Normal("F"), new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Ref, R.Path.Int))), default),
                    null, Arr<R.Argument>(new R.Argument.Ref(new R.GlobalVarLoc("j")))
                ))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        void CallExp_TranslatesIntoNewEnumExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }

        [Fact]
        void CallExp_TranslatesIntoCallFuncExp()
        {
            // int Func<T>(int x) { return x; }
            // Func<int>(3);
            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    null, false, false, SIntTypeExp(), "Func", Arr("T"),
                    Arr(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")),
                    SBlock(new S.ReturnStmt(new S.ReturnValueInfo(false, SId("x"))))
                )),

                new S.StmtScriptElement(
                    new S.ExpStmt(new S.CallExp(SId("Func", SIntTypeExp()), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                )
            );

            var script = Translate(syntaxScript);

            var expected = RScript(
                default, 

                Arr<R.FuncDecl>(
                    new R.NormalFuncDecl(
                        default, new R.Name.Normal("Func"), false, Arr("T"), RNormalParams((R.Path.Int, "x")), RBlock(new R.ReturnStmt(new R.ReturnInfo.Expression(new R.LoadExp(RLocalVarLoc("x")))))
                    )
                ),

                default,

                new R.ExpStmt(new R.CallFuncExp(MakeRootPath("Func", new R.ParamHash(1, Arr(new R.ParamHashEntry(R.ParamKind.Default, R.Path.Int))), Arr(R.Path.Int)), null, RArgs(RInt(3))))
            );

            Assert.Equal(expected, script);
        }

        // TODO: TypeArgument지원하면 General버전이랑 통합하기
        [Fact]
        void CallExp_TranslatesIntoCallFuncExpWithoutTypeArgument()
        {
            // int Func(int x) { return x; }
            // Func(3);
            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    null, false, false, SIntTypeExp(), "Func", Arr<string>(),
                    Arr(new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x")),
                    SBlock(new S.ReturnStmt(new S.ReturnValueInfo(false, SId("x"))))
                )),

                new S.StmtScriptElement(
                    new S.ExpStmt(new S.CallExp(SId("Func"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                )
            );

            var script = Translate(syntaxScript);

            var expected = RScript(

                default,

                Arr<R.FuncDecl>(new R.NormalFuncDecl(
                    default, new R.Name.Normal("Func"), false, Arr<string>(), RNormalParams((R.Path.Int, "x")), RBlock(new R.ReturnStmt(new R.ReturnInfo.Expression(new R.LoadExp(RLocalVarLoc("x")))))
                )),

                default,

                new R.ExpStmt(new R.CallFuncExp(
                    MakeRootPath("Func", new R.ParamHash(0, Arr(new R.ParamHashEntry(R.ParamKind.Default, R.Path.Int))), default), null, RArgs(RInt(3))))
            );

            Assert.Equal(expected, script);
        }

        [Fact]
        void CallExp_TranslatesIntoCallValueExp()
        {
            throw new TestNeedToBeWrittenException();
        }

        // A0901
        [Fact]
        void CallExp_ChecksMultipleCandidates()
        {
            throw new TestNeedToBeWrittenException();
        }

        // A0902
        [Fact]
        void CallExp_ChecksCallableExpressionIsNotCallable()
        {
            throw new TestNeedToBeWrittenException();
        }

        // A0903
        [Fact]
        void CallExp_ChecksEnumConstructorArgumentCount()
        {
            throw new TestNeedToBeWrittenException();
        }

        // A0904
        [Fact]
        void CallExp_ChecksEnumConstructorArgumentType()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void LambdaExp_TranslatesTrivially()
        {
            // int x; // global
            // {
            //     int y; // local
            //     var l = (int param) => { x = 3; return param + x + y; };
            // 
            // }

            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void LambdaExp_ChecksAssignToLocalVaraiableOutsideLambda()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void IndexerExp_TranslatesTrivially()
        {
            // var s = [1, 2, 3, 4];
            // var i = s[3];

            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void IndexerExp_ChecksInstanceIsList() // TODO: Indexable로 확장
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void IndexerExp_ChecksIndexIsInt() // TODO: Indexable인자 타입에 따라 달라짐
        {
            throw new TestNeedToBeWrittenException();
        }

        // MemberCallExp
        // 1. x.F();     // instance call (class, struct, interface)
        // 2. C.F();     // static call (class static, struct static, global)
        // 3. E.F(1, 2); // enum constructor
        // 4. x.f();     // instance lambda call (class, struct, interface)
        // 5. C.f();     // static lambda call (class static, struct static, global)

        [Fact]
        void MemberCallExp_TranslatesIntoCallFuncExp() // 1, 2
        {

            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberCallExp_TranslatesIntoNewEnumExp() // 3
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberCallExp_TranslatesIntoCallValueExp() // 4, 5
        {

            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberCallExp_ChecksCallableExpressionIsNotCallable() // 4, 5
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberCallExp_EnumConstructorArgumentCount() // 3
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberCallExp_EnumConstructorArgumentType() // 3
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberCallExp_ChecksMultipleCandidates() // 1, 2, 4, 5
        {
            throw new TestNeedToBeWrittenException();
        }

        // MemberExp
        // 1. E.Second  // enum constructor without parameter
        // 2. e.X       // enum field (EnumMemberExp)        
        // 3. C.x       // static 
        // 4. c.x       // instance  (class, struct, interface)
        [Fact]
        void MemberExp_TranslatesIntoNewEnumExp() // 1.
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberExp_TranslatesIntoEnumMemberExp() // 2
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberExp_TranslatesIntoStaticMemberExp() // 3
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberExp_TranslatesIntoStructMemberExp() // 4
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberExp_TranslatesIntoClassMemberExp() // 4
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void MemberExp_ChecksMemberNotFound() // TODO: enum, class, struct, interface 각각의 경우에 해야 하지 않는가
        {
            throw new TestNeedToBeWrittenException();
        }

        //case S.MemberExp memberExp: return AnalyzeMemberExp(memberExp, context, out outExp, out outTypeValue);

        [Fact]
        void ListExp_TranslatesTrivially() // TODO: 타입이 적힌 빈 리스트도 포함, <int>[]
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        void ListExp_UsesHintTypeToInferElementType() // List<int> s = []; 
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public void ListExp_ChecksCantInferEmptyElementType() // var x = []; // ???
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public void ListExp_ChecksCantInferElementType() // var = ["string", 1, false];
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public void UninitializedVariableAnalysis_UseUninitializedLocalVarDecl_ReturnError()
        {
            // {
            //     int a; // local var without initialize
            //     @$a    // use, error
            // }

            S.Exp e;
            var syntaxScript = SScript(new S.StmtScriptElement(SBlock(
                SVarDeclStmt(SIntTypeExp(), "a", null),
                SCommand(SString(new S.ExpStringExpElement(e = SId("a"))))
            )));

            var errors = TranslateWithErrors(syntaxScript);  

            VerifyError(errors, A0111_VarDecl_LocalVarDeclNeedInitializer, e);
        }

        [Fact]
        public void UninitializedVariableAnalysis_UseInitializedLocalVar_NoErrors()
        {
            // {
            //     int a = 1; // local var 
            //     @$a    // use, no error
            // }           

            S.Exp e;
            var syntaxScript = SScript(new S.StmtScriptElement(SBlock(
                SVarDeclStmt(SIntTypeExp(), "a", SInt(1)),
                SCommand(SString(new S.ExpStringExpElement(e = SId("a"))))
            )));

            var rscript = Translate(syntaxScript);
        }
    }
}