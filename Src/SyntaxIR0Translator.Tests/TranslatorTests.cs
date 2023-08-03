using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

using Citron.Infra;
using Citron.Symbol;
using Citron.Analysis;
using Citron.Collections;
using Citron.Log;

using Xunit;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Syntax.SyntaxFactory;
using static Citron.Infra.Misc;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

using static Citron.Test.Misc;
using static Citron.Test.SyntaxIR0TranslatorMisc;

namespace Citron.Test
{
    public class TranslatorTests
    {
        Name moduleName;
        SymbolFactory factory;
        R.IR0Factory r;
        ImmutableArray<ModuleDeclSymbol> refModuleDecls;

        public TranslatorTests()
        {
            (moduleName, factory, r, var runtimeModuleD) = TestPreparations.Prepare();
            refModuleDecls = Arr(runtimeModuleD);
        }

        R.Script? Translate(S.Script syntaxScript, bool raiseAssertFailed = true)
        {
            var testLogger = new TestLogger(raiseAssertFailed);
            var factory = new SymbolFactory();
            return SyntaxIR0Translator.Build(moduleName, Arr(syntaxScript), refModuleDecls, factory, testLogger);
        }

        List<ILog> TranslateWithErrors(S.Script syntaxScript, bool raiseAssertionFail = false)
        {
            var testLogger = new TestLogger(raiseAssertionFail);
            var _ = SyntaxIR0Translator.Build(moduleName, Arr(syntaxScript), refModuleDecls, factory, testLogger);

            return testLogger.Logs;
        }

        (ModuleDeclSymbol, GlobalFuncDeclSymbol) NewMain()
        {
            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Main"), typeParams: default);
            funcD.InitFuncReturnAndParams(r.FuncRet(r.IntType()), parameters: default);
            moduleD.AddFunc(funcD);

            return (moduleD, funcD);
        }

        R.StmtBody NewMain(ModuleDeclSymbol moduleD, params R.Stmt[] stmts)
        {
            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Main"), typeParams: default);
            funcD.InitFuncReturnAndParams(r.FuncRet(r.IntType()), parameters: default);
            moduleD.AddFunc(funcD);

            return r.StmtBody(funcD, stmts);
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

            var expectedStmt = r.Command(
                r.String(
                    r.TextElem("Hello "),
                    r.ExpElem(r.String("World"))));

            var expected = r.Script(expectedStmt);
            
            AssertEquals(expected, script);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVariableInMain()
        {
            // int x = 1;

            var syntaxScript = SScript(SVarDeclStmt(SIntTypeExp(), "x", SInt(1)));
            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.LocalVarDecl(r.IntType(), "x", r.Int(1))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInFuncScope()
        {
            // void Func() { int x = 1; }

            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(accessModifier: null, isSequence: false, SVoidTypeExp(), "Func", default, default,
                    SBody(
                        SVarDeclStmt(SIntTypeExp(), "x", SInt(1))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var globalFuncD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Func"), typeParams: default);
            globalFuncD.InitFuncReturnAndParams(r.FuncRet(r.VoidType()), parameters: default);
            moduleD.AddFunc(globalFuncD);

            var globalFuncBody = r.StmtBody(globalFuncD,
                r.LocalVarDecl(r.IntType(), "x", r.Int(1))
            );

            var expected = r.Script(
                moduleD,
                globalFuncBody
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void VarDeclStmt_InfersVarType()
        {
            var syntaxScript = SScript(
                new S.VarDeclStmt(new S.VarDecl(SVarTypeExp(), Arr(new S.VarDeclElement("x", SInt(3)))))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.LocalVarDecl(r.IntType(), "x", r.Int(3))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void VarDeclStmt_ChecksLocalVarNameIsUniqueWithinScope()
        {
            S.VarDeclElement elem;

            var syntaxScript = SScript(
                new S.VarDeclStmt(SVarDecl(SIntTypeExp(), "x", null)),
                new S.VarDeclStmt(new S.VarDecl(SIntTypeExp(), Arr(elem = new S.VarDeclElement("x", null))))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);
        }

        [Fact]
        public void VarDeclStmt_ChecksLocalVarNameIsUniqueWithinScope2()
        {
            S.VarDeclElement element;

            var syntaxScript = SScript(
                new S.VarDeclStmt(new S.VarDecl(SIntTypeExp(), Arr(new S.VarDeclElement("x", null), element = new S.VarDeclElement("x", null))))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, element);
        }

        //[Fact]
        //public void VarDeclStmt_ChecksLocalDeclHasInitializer()
        //{
        //    S.VarDeclElement elem;
        //    var syntaxScript = SScript(
        //        new S.VarDeclStmt(new S.VarDecl(false, SIntTypeExp(), Arr(
        //            elem = new S.VarDeclElement("x", null)
        //        )))
        //    );

        //    var errors = TranslateWithErrors(syntaxScript);
        //    VerifyError(errors, A0111_VarDecl_LocalVarDeclNeedInitializer, elem);
        //}

        #region If

        [Fact]
        public void IfStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                new S.IfStmt(new S.BoolLiteralExp(false), SEmbeddableBlankStmt(), SEmbeddableBlankStmt())
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.If(r.Bool(false))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void IfStmt_ReportsErrorWhenCondTypeIsNotBool()
        {
            S.Exp cond;

            var syntaxScript = SScript(
                new S.IfStmt(cond = SInt(3), SEmbeddableBlankStmt(), SEmbeddableBlankStmt())
            );

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

        #endregion

        #region For
        [Fact]
        public void ForStmt_TranslatesInitializerTrivially()
        {
            var syntaxScript = SScript(

                new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SVarDecl(SIntTypeExp(), "x", SInt(0))),
                    null, null, SEmbeddableBlankStmt()
                ),

                SVarDeclStmt(SStringTypeExp(), "x"),

                new S.ForStmt(
                    new S.ExpForStmtInitializer(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SString("Hello"))),
                    null, null, SEmbeddableBlankStmt()
                )
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(

                r.For(r.IntType(), "x", r.Int(0), cond: null, cont: null),
                
                r.LocalVarDecl(r.StringType(), "x", null),

                r.For(
                    r.AssignExp(r.LocalVar("x"), r.String("Hello")),
                    cond: null, cont: null
                )
            );

            AssertEquals(expected, script);
        }

        // translation만으로는 체크 불가능, type checking을 해야 한다
        //[Fact]
        //public void ForStmt_ChecksVarDeclInitializerScope()
        //{
        //    var syntaxScript = SScript(

        //        SVarDeclStmt(SStringTypeExp(), "x"),

        //        new S.ForStmt(
        //            new S.VarDeclForStmtInitializer(SVarDecl(SIntTypeExp(), "x", SInt(0))), // x의 범위는 ForStmt내부에서
        //            new S.BinaryOpExp(S.BinaryOpKind.Equal, SId("x"), SInt(3)),
        //            null,
        //            SEmbeddableBlankStmt()
        //        ),

        //        new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SString("Hello")))
        //    );

        //    var script = Translate(syntaxScript);

        //    var expected = r.Script(

        //        r.LocalVarDecl(r.StringType(), "x", null),

        //        r.For(
        //            r.IntType(), "x", r.Int(0),

        //            // cond
        //            r.CallInternalBinary(
        //                R.InternalBinaryOperator.Equal_Int_Int_Bool,                        
        //                r.Load(new R.LocalVarLoc(NormalName("x")), r.IntType()),
        //                r.Int(3)
        //            ),

        //            cont: null, 
        //            body: default
        //        ),

        //        r.Assign(r.LocalVar("x"), r.String("Hello"))
        //    );

        //    AssertEquals(expected, script);
        //}

        // 타입체커로
        [Fact]
        public void ForStmt_ChecksConditionIsBool()
        {
            S.Exp cond;

            var syntaxScript = SScript(
                new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SVarDecl(SIntTypeExp(), "x", SInt(0))),
                    cond = SInt(3),
                    null, SEmbeddableBlankStmt()
                )
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A1101_ForStmt_ConditionShouldBeBool, cond);
        }

        [Fact]
        public void ForStmt_ChecksExpInitializerIsAssignOrCall()
        {
            S.Exp exp;

            var syntaxScript = SScript(
                new S.ForStmt(
                    new S.ExpForStmtInitializer(exp = SInt(3)), // error
                    null, null, SEmbeddableBlankStmt()
                )
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall, exp);
        }

        [Fact]
        public void ForStmt_ChecksContinueExpIsAssignOrCall()
        {
            S.Exp continueExp;

            var syntaxScript = SScript(

                new S.ForStmt(
                    null,
                    null,
                    continueExp = SInt(3),
                    SEmbeddableBlankStmt()
                )
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1103_ForStmt_ContinueExpShouldBeAssignOrCall, continueExp);
        }

        #endregion

        #region Continue

        [Fact]
        public void ContinueStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                new S.ForStmt(null, null, null, new S.EmbeddableStmt.Single(new S.ContinueStmt())),
                new S.ForeachStmt(SIntTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), new S.EmbeddableStmt.Single(new S.ContinueStmt()))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.For(cond: null, cont: null, new R.ContinueStmt()),
                r.Foreach(
                    r.IntType(), "x",
                    r.EmptyListIter(r.IntType()),
                    r.Continue()
                )
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void ContinueStmt_ChecksUsedInLoop()
        {
            S.ContinueStmt continueStmt;
            var syntaxScript = SScript(continueStmt = new S.ContinueStmt());

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);
        }

        #endregion

        #region Break
        [Fact]
        public void BreakStmt_WithinNestedForAndForeachLoopTranslatesTrivially()
        {
            // for(;;;) 
            //     foreach(int x in [int]()) break;
            var syntaxScript = SScript(
                new S.ForStmt(null, null, null, new S.EmbeddableStmt.Single(
                    new S.ForeachStmt(SIntTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), new S.EmbeddableStmt.Single(new S.BreakStmt()))
                ))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.For(cond: null, cont: null, 
                    r.Foreach(r.IntType(), "x",
                        r.EmptyListIter(r.IntType()),
                        r.Break()
                    )
                )
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void BreakStmt_WithinForLoopTranslatesTrivially()
        {
            var syntaxScript = SScript(
                new S.ForStmt(null, null, null, new S.EmbeddableStmt.Single(new S.BreakStmt()))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.For(cond: null, cont: null, r.Break())
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void BreakStmt_ChecksUsedInLoop()
        {
            S.BreakStmt breakStmt;
            var syntaxScript = SScript(breakStmt = new S.BreakStmt());

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1601_BreakStmt_ShouldUsedInLoop, breakStmt);
        }

        #endregion

        #region Return

        [Fact]
        public void ReturnStmt_TranslatesTrivially()
        {
            var syntaxScript = new S.Script(Arr<S.ScriptElement>(
                new S.GlobalFuncDeclScriptElement(
                    new S.GlobalFuncDecl(
                        null,
                        isSequence: false,
                        new S.IdTypeExp("int", default), "Main",
                        typeParams: default, parameters: default, 
                        SBody(
                            new S.ReturnStmt(new S.ReturnValueInfo(SInt(2)))
                        )
                    )
                )
            ));
            
            var script = Translate(syntaxScript);

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var entryD = new GlobalFuncDeclSymbol(moduleD,
                Accessor.Private, new Name.Normal("Main"), typeParams: default);

            entryD.InitFuncReturnAndParams(
                new FuncReturn(r.IntType()), default);

            moduleD.AddFunc(entryD);

            var entryBody = r.StmtBody(entryD, r.Return(r.Int(2)));
            var expected = r.Script(moduleD, entryBody);

            AssertEquals(expected, script);
        }

        [Fact]
        public void ReturnStmt_TranslatesReturnStmtInSeqFuncTrivially()
        {
            var syntaxScript = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                accessModifier: null,
                isSequence: true, // seq func
                SIntTypeExp(), "Func", typeParams: default, parameters: default,
                SBody(new S.ReturnStmt(null))
            )));

            var script = Translate(syntaxScript);

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);
            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Func"), typeParams: default);
            funcD.InitFuncReturnAndParams(new FuncReturn(r.IntType()), parameters: default);
            moduleD.AddFunc(funcD);

            var funcBody = r.StmtBody(funcD,
                r.Return()
            );            

            var expected = r.Script(moduleD, funcBody);

            AssertEquals(expected, script);
        }

        [Fact]
        public void ReturnStmt_ChecksMatchFuncRetTypeAndRetValue()
        {
            S.Exp retValue;

            var funcDecl = new S.GlobalFuncDecl(accessModifier: null, isSequence: false, SIntTypeExp(), "Func", default, default, SBody(
                new S.ReturnStmt(new S.ReturnValueInfo(retValue = SString("Hello")))
            ));

            var syntaxScript = SScript(new S.GlobalFuncDeclScriptElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A2201_Cast_Failed, retValue);
        }

        [Fact]
        public void ReturnStmt_ChecksMatchVoidTypeAndReturnNothing()
        {
            S.ReturnStmt retStmt;

            var funcDecl = new S.GlobalFuncDecl(accessModifier: null, isSequence: false, SIntTypeExp(), "Func", default, default, SBody(
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

            var funcDecl = new S.GlobalFuncDecl(accessModifier: null, isSequence: true, SIntTypeExp(), "Func", default, default, SBody(
                retStmt = new S.ReturnStmt(new S.ReturnValueInfo(SInt(2)))
            ));

            var syntaxScript = SScript(new S.GlobalFuncDeclScriptElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1202_ReturnStmt_SeqFuncShouldReturnVoid, retStmt);
        }

        [Fact]
        public void ReturnStmt_ShouldReturnIntWhenUsedInTopLevelStmt()
        {
            S.Exp exp;
            var syntaxScript = SScript(new S.ReturnStmt(new S.ReturnValueInfo(exp = SString("Hello"))));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A2201_Cast_Failed, exp);
        }

        [Fact]
        public void ReturnStmt_UsesHintType()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum, Prerequisite.TypeHint);
        }

        #endregion

        #region Block

        [Fact]
        public void BlockStmt_TranslatesVarDeclStmtWithinBlockStmtOfTopLevelStmtIntoLocalVarDeclStmt()
        {
            var syntaxScript = SScript(
                SBlock(
                    SVarDeclStmt(SStringTypeExp(), "x", SString("Hello"))
                )
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.Block(
                    r.LocalVarDecl(r.StringType(), "x", r.String("Hello")) // not GlobalVarDecl
                )
            );

            AssertEquals(expected, script);
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

        #endregion

        #region ExpStmt

        [Fact]
        public void ExpStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SInt(3)))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.LocalVarDecl(r.IntType(), "x", null),
                r.Assign(r.LocalVar("x"), r.Int(3))
            );

            AssertEquals(expected, script);
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


        #endregion

        #region Task

        [Fact]
        public void TaskStmt_ChecksAssignToLocalVariableOutsideLambda()
        {
            // 에러
            // int x; // 'local' int x;
            // task { x = 3; } // x가 복사해서 들어가는데 캐치가 안됨

            // 이렇게 해야 함
            // box var* x = box 0; // x는 scope를 벗어나서도 사용할 수 있는 변수다
            // task { *x = 3; }
            
            S.Exp exp;

            var syntaxScript = SScript(                
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.TaskStmt(SBody(
                    new S.ExpStmt(
                        new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SId("x"), SInt(3))
                    )
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);

            // 에러, x는 로컬이라 
            VerifyError(errors, A0803_BinaryOp_LeftOperandIsNotAssignable, exp);
            
        }

        [Fact]
        public void TaskStmt_TranslatesWithLocalVariable()
        {
            // int x = 1;
            // task { int y = x; }
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x", SInt(1)),
                new S.TaskStmt(SBody(
                    SVarDeclStmt(SIntTypeExp(), "y", SId("x"))
                ))
            );

            var script = Translate(syntaxScript);

            var (moduleD, funcD) = NewMain();

            var lambdaD = new LambdaDeclSymbol(funcD, new Name.Anonymous(0), parameters: default);
            ((IFuncDeclSymbol)funcD).AddLambda(lambdaD);

            var lambdaMemberVarD = new LambdaMemberVarDeclSymbol(lambdaD, r.IntType(), NormalName("x"));
            lambdaD.AddMemberVar(lambdaMemberVarD);            

            var lambdaSymbol = (LambdaSymbol)lambdaD.MakeOpenSymbol(factory);
            var lambdaMemberVarSymbol = (LambdaMemberVarSymbol)lambdaMemberVarD.MakeOpenSymbol(factory);

            var funcBody = r.StmtBody(funcD,
                r.Task(lambdaSymbol, r.Arg(r.LoadLocalVar("x", r.IntType())))
            );

            var lambdaBody = r.StmtBody(lambdaD,
                r.LocalVarDecl(r.IntType(), "y", r.LoadLambdaMember(lambdaMemberVarSymbol))
            );

            var expected = r.Script(moduleD, funcBody, lambdaBody);

            AssertEquals(expected, script);
        }

        #endregion

        #region Await


        [Fact]
        public void AwaitStmt_TranslatesTrivially()
        {
            var syntaxScript = SScript(
                new S.AwaitStmt(SBody(
                    new S.BlankStmt()
                ))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(new R.AwaitStmt(Body: default));

            AssertEquals(expected, script);
        }

        [Fact]
        public void AwaitStmt_ChecksLocalVariableScope()
        {
            // await 
            // {
            //     var x = "Hello"
            // }
            // @$x

            S.Exp exp;

            var syntaxScript = SScript(
                new S.AwaitStmt(SBody(
                    SVarDeclStmt(SStringTypeExp(), "x", SString("Hello"))
                )),

                SCommand(SString(new S.ExpStringExpElement(exp = SId("x"))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A2007_ResolveIdentifier_NotFound, exp);
        }


        #endregion

        #region Async        

        [Fact]
        public void AsyncStmt_ChecksAssignToLocalVariableOutsideLambda()
        {
            S.Exp exp;

            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x", SInt(0)),
                new S.AsyncStmt(SBody(
                    new S.ExpStmt(
                        new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SId("x"), SInt(3))
                    )
                ))
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
                SVarDeclStmt(SIntTypeExp(), "x", SInt(0)),
                new S.AsyncStmt(SBody(
                    SVarDeclStmt(SIntTypeExp(), "x", SId("x"))
                ))
                
            );

            var script = Translate(syntaxScript);

            var (moduleD, mainD) = NewMain();
            var lambdaD = new LambdaDeclSymbol(mainD, new Name.Anonymous(0), parameters: default);
            ((IFuncDeclSymbol)mainD).AddLambda(lambdaD);

            var xD = new LambdaMemberVarDeclSymbol(lambdaD, r.IntType(), NormalName("x"));
            lambdaD.AddMemberVar(xD);

            var lambdaSymbol = (LambdaSymbol)lambdaD.MakeOpenSymbol(factory);
            var xSymbol = (LambdaMemberVarSymbol)xD.MakeOpenSymbol(factory);

            var mainBody = r.StmtBody(mainD,
                r.Async(
                    lambdaSymbol,
                    r.Arg(r.LoadLocalVar("x", r.IntType()))
                )
            );

            var lambdaBody = r.StmtBody(lambdaD,
                r.LocalVarDecl(r.IntType(), "x", r.LoadLambdaMember(xSymbol))
            );

            var expected = r.Script(moduleD, mainBody, lambdaBody);
            
            AssertEquals(expected, script);
        }

        #endregion

        #region Foreach

        [Fact]
        public void ForeachStmt_TranslatesTrivially()
        {
            // foreach(int x in list<int>());
            var scriptSyntax = SScript(new S.ForeachStmt(SIntTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), SEmbeddableBlankStmt()));

            var script = Translate(scriptSyntax);

            var expected = r.Script(
                r.Foreach(r.IntType(), "x", r.EmptyListIter(r.IntType()))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void ForeachStmt_ChecksIteratorIsListOrEnumerable()
        {
            S.Exp iterator;
            var scriptSyntax = SScript(new S.ForeachStmt(SIntTypeExp(), "x", iterator = SInt(3), SEmbeddableBlankStmt()));

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
            var scriptSyntax = SScript(foreachStmt = new S.ForeachStmt(SStringTypeExp(), "x", new S.ListExp(SIntTypeExp(), default), SEmbeddableBlankStmt()));

            var errors = TranslateWithErrors(scriptSyntax);

            VerifyError(errors, A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType, foreachStmt);
        }

        #endregion

        #region Yield

        [Fact]
        public void YieldStmt_TranslatesTrivially()
        {
            // seq int Func() { yield 3; }
            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    accessModifier: null, isSequence: true, SIntTypeExp(), "Func", default, default,
                    SBody(
                        new S.YieldStmt(SInt(3))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Func"), typeParams: default);
            funcD.InitFuncReturnAndParams(r.FuncRet(r.IntType()), parameters: default);
            moduleD.AddFunc(funcD);

            var funcBody = r.StmtBody(funcD,
                r.Yield(r.Int(3))
            );

            var expected = r.Script(moduleD, funcBody);

            AssertEquals(expected, script);
        }

        [Fact]
        public void YieldStmt_ChecksYieldStmtUsedInSeqFunc()
        {
            S.YieldStmt yieldStmt;

            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    accessModifier: null, isSequence: false, SIntTypeExp(), "Func", default, default,
                    SBody(
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
                    accessModifier: null, isSequence: true, SStringTypeExp(), "Func", default, default,
                    SBody(
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

        #endregion

        #region Id

        // IdExp
        [Fact]
        public void IdExp_TranslatesIntoExternalGlobal()
        {
            throw new PrerequisiteRequiredException(Prerequisite.External);
        }
        
        [Fact]
        public void IdExp_TranslatesLocalVarOutsideLambdaIntoLocalVarExp()
        {
            throw new TestNeedToBeWrittenException();
        }

        [Fact]
        public void IdExp_TranslatesIntoLocalVarExp()
        {
            // x = 3;
            // y = x;

            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x", SInt(3)),
                SVarDeclStmt(SIntTypeExp(), "y", SId("x"))
            );

            var script = Translate(syntaxScript);
            var expected = r.Script(
                r.LocalVarDecl(r.IntType(), "x", r.Int(3)),
                r.LocalVarDecl(r.IntType(), "y", r.LoadLocalVar("x", r.IntType()))
            );

            AssertEquals(expected, script);
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

        #endregion // IdExp

        #region Literal

        [Fact]
        public void BoolLiteralExp_TranslatesTrivially()
        {
            // bool b1 = false;
            // bool b2 = true;

            var syntaxScript = SScript(
                SVarDeclStmt(SBoolTypeExp(), "b1", new S.BoolLiteralExp(false)),
                SVarDeclStmt(SBoolTypeExp(), "b2", new S.BoolLiteralExp(true)));

            var script = Translate(syntaxScript);
            var expected = r.Script(
                r.LocalVarDecl(r.BoolType(), "b1", r.Bool(false)),
                r.LocalVarDecl(r.BoolType(), "b2", r.Bool(true))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void IntLiteralExp_TranslatesTrivially()
        {
            // int i = 34;
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "i", new S.IntLiteralExp(34)));

            var script = Translate(syntaxScript);
            var expected = r.Script(
                r.LocalVarDecl(r.IntType(), "i", r.Int(34))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void StringExp_TranslatesTrivially()
        {
            // string s = "Hello"
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "s", SString("Hello")));

            var script = Translate(syntaxScript);
            var expected = r.Script(
                r.LocalVarDecl(r.StringType(), "s", r.String("Hello"))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void StringExp_WrapsExpStringExpElementWhenExpIsBoolOrInt()
        {
            // string s1 = "{3}"
            // string s2 = "{true}"

            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "s1", SString(new S.ExpStringExpElement(SInt(3)))),
                SVarDeclStmt(SStringTypeExp(), "s2", SString(new S.ExpStringExpElement(SBool(true))))
            );

            var script = Translate(syntaxScript, true);

            var expected = r.Script(
                r.LocalVarDecl(r.StringType(), "s1", r.String(r.ExpElem(r.CallInternalUnary(R.InternalUnaryOperator.ToString_Int_String, r.Int(3))))),
                r.LocalVarDecl(r.StringType(), "s2", r.String(r.ExpElem(r.CallInternalUnary(R.InternalUnaryOperator.ToString_Bool_String, r.Int(3)))))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void StringExp_ChecksStringExpElementIsStringConvertible()
        {
            // string s = "{() => {}}"

            S.Exp exp;
            // convertible가능하지 않은 것, lambda?
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "s", SString(new S.ExpStringExpElement(exp = new S.LambdaExp(
                    Arr<S.LambdaExpParam>(),
                    SBody()
                ))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, exp);
        }

        #endregion // Literal

        #region UnaryOp

        [Fact]
        public void UnaryOpExp_TranslatesTrivially()
        {
            // var x1 = false;
            // var x2 = 3;
            // var x3 = ++x2;
            // var x4 = --x2;
            // var x5 = x2++;
            // var x6 = x2--;

            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "x1", new S.UnaryOpExp(S.UnaryOpKind.LogicalNot, SBool(false))),
                SVarDeclStmt(SVarTypeExp(), "x2", new S.UnaryOpExp(S.UnaryOpKind.Minus, SInt(3))),
                SVarDeclStmt(SVarTypeExp(), "x3", new S.UnaryOpExp(S.UnaryOpKind.PrefixInc, SId("x2"))),
                SVarDeclStmt(SVarTypeExp(), "x4", new S.UnaryOpExp(S.UnaryOpKind.PrefixDec, SId("x2"))),
                SVarDeclStmt(SVarTypeExp(), "x5", new S.UnaryOpExp(S.UnaryOpKind.PostfixInc, SId("x2"))),
                SVarDeclStmt(SVarTypeExp(), "x6", new S.UnaryOpExp(S.UnaryOpKind.PostfixDec, SId("x2")))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.LocalVarDecl(r.BoolType(), "x1", r.CallInternalUnary(R.InternalUnaryOperator.LogicalNot_Bool_Bool, r.Bool(false))),
                r.LocalVarDecl(r.IntType(), "x2", r.CallInternalUnary(R.InternalUnaryOperator.UnaryMinus_Int_Int, r.Int(3))),
                r.LocalVarDecl(r.IntType(), "x3", r.CallInternalUnaryAssign(R.InternalUnaryAssignOperator.PrefixInc_Int_Int, r.LocalVar("x2"))),
                r.LocalVarDecl(r.IntType(), "x4", r.CallInternalUnaryAssign(R.InternalUnaryAssignOperator.PrefixDec_Int_Int, r.LocalVar("x2"))),
                r.LocalVarDecl(r.IntType(), "x5", r.CallInternalUnaryAssign(R.InternalUnaryAssignOperator.PostfixInc_Int_Int, r.LocalVar("x2"))),
                r.LocalVarDecl(r.IntType(), "x6", r.CallInternalUnaryAssign(R.InternalUnaryAssignOperator.PostfixDec_Int_Int, r.LocalVar("x2")))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfUnaryAssignExpIsIntType()
        {
            // string x = "Hello";
            // var i = ++x;

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
            // var i = ++3;

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
            // var b = !3;
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
            // var i = -false;
            S.Exp operand;
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "i", new S.UnaryOpExp(S.UnaryOpKind.Minus, operand = SBool(false)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, operand);
        }

        #endregion // UnaryOp

        #region BinaryOp

        [Fact]
        void BinaryOpExp_TranslatesIntoAssignExp()
        {
            // int x;
            // x = 3;

            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "x"),
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("x"), SInt(3)))
            );

            var script = Translate(syntaxScript);
            var expected = r.Script(
                r.LocalVarDecl(r.IntType(), "x", initExp: null),
                r.Assign(r.LocalVar("x"), r.Int(3))
            );

            AssertEquals(expected, script);
        }

        [Fact]
        void BinaryOpExp_UsingDerefOnBoxPtrVar_TranslateToBoxDeref()
        {
            // box int* i = box 3;            
            // *x = 7 + *x;
            var syntaxScript = SScript(
                new S.VarDeclStmt(new S.VarDecl(SBoxPtrTypeExp(SIntTypeExp()), Arr(new S.VarDeclElement("x", new S.BoxExp(SInt(3)))))),
                new S.ExpStmt(new S.BinaryOpExp(
                    S.BinaryOpKind.Assign,
                    SDerefExp(SId("x")),
                    new S.BinaryOpExp(S.BinaryOpKind.Add, SInt(7), SDerefExp(SId("x")))
                ))
            );

            var script = Translate(syntaxScript);

            var expected = r.Script(
                r.LocalVarDecl(r.BoxPtrType(r.IntType()), "x", r.Box(r.Int(3))),
                r.Assign(
                    r.BoxDeref(r.LoadLocalVar("x", r.BoxPtrType(r.IntType()))),
                    r.CallInternalBinary(R.InternalBinaryOperator.Add_Int_Int_Int, r.Int(7), r.Load(r.BoxDeref(r.LoadLocalVar("x", r.BoxPtrType(r.IntType()))), r.IntType()))
                )
            );

            AssertEquals(expected, script);
        }

        [Fact]
        void BinaryOpExp_UsingDerefOnLocalPtrVar_TranslateToLocalDeref()
        {
            // int i = 3;
            // int* x = &i;
            // *x = 7 + *x;
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "i", SInt(3)),
                new S.VarDeclStmt(new S.VarDecl(SLocalPtrTypeExp(SIntTypeExp()), Arr(new S.VarDeclElement("x", SRefExp(SId("i")))))),
                new S.ExpStmt(new S.BinaryOpExp(
                    S.BinaryOpKind.Assign,
                    SDerefExp(SId("x")),
                    new S.BinaryOpExp(S.BinaryOpKind.Add, SInt(7), SDerefExp(SId("x")))
                ))
            );

            var script = Translate(syntaxScript); 
            
            var expected = r.Script(
                r.LocalVarDecl(r.IntType(), "i", r.Int(3)),
                r.LocalVarDecl(r.LocalPtrType(r.IntType()), "x", r.LocalRef(r.LocalVar("i"), r.IntType())),
                r.Assign(
                    r.LocalDeref(r.LoadLocalVar("x", r.LocalPtrType(r.IntType()))),
                    r.CallInternalBinary(R.InternalBinaryOperator.Add_Int_Int_Int, r.Int(7), r.Load(r.LocalDeref(r.LoadLocalVar("x", r.LocalPtrType(r.IntType()))), r.IntType()))
                )
            );

            AssertEquals(expected, script);
        }

        [Fact]
        void BinaryOpExp_ChecksCompatibleBetweenOperandsOnAssignOperation()
        {
            // int x;
            // x = true;

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
            // 3 = 4;
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
            // string x = Hello * 4;
            S.Exp exp;
            var syntaxScript = SScript(
                SVarDeclStmt(SStringTypeExp(), "x", exp = new S.BinaryOpExp(S.BinaryOpKind.Multiply, SString("Hello"), SInt(4)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0802_BinaryOp_OperatorNotFound, exp);
        }

        public enum DataType
        {
            Bool, Int, String, 
        }

        public static IEnumerable<object[]> Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial()
        {
            yield return new object[] { S.BinaryOpKind.Multiply, DataType.Int, DataType.Int, R.InternalBinaryOperator.Multiply_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Divide, DataType.Int, DataType.Int, R.InternalBinaryOperator.Divide_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Modulo, DataType.Int, DataType.Int, R.InternalBinaryOperator.Modulo_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Add, DataType.Int, DataType.Int, R.InternalBinaryOperator.Add_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Subtract, DataType.Int, DataType.Int, R.InternalBinaryOperator.Subtract_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.LessThan, DataType.Int, DataType.Bool, R.InternalBinaryOperator.LessThan_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThan, DataType.Int, DataType.Bool, R.InternalBinaryOperator.GreaterThan_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.LessThanOrEqual, DataType.Int, DataType.Bool, R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThanOrEqual, DataType.Int, DataType.Bool, R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.Equal, DataType.Int, DataType.Bool, R.InternalBinaryOperator.Equal_Int_Int_Bool };

            yield return new object[] { S.BinaryOpKind.Add, DataType.String, DataType.String, R.InternalBinaryOperator.Add_String_String_String };
            yield return new object[] { S.BinaryOpKind.LessThan, DataType.String, DataType.Bool, R.InternalBinaryOperator.LessThan_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThan, DataType.String, DataType.Bool, R.InternalBinaryOperator.GreaterThan_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.LessThanOrEqual, DataType.String, DataType.Bool, R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThanOrEqual, DataType.String, DataType.Bool, R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.Equal, DataType.String, DataType.Bool, R.InternalBinaryOperator.Equal_String_String_Bool };

            yield return new object[] { S.BinaryOpKind.Equal, DataType.Bool, DataType.Bool, R.InternalBinaryOperator.Equal_Bool_Bool_Bool };

            // NotEqual
        }

        S.Exp NewSOperand(DataType type)
        {
            switch(type)
            {
                case DataType.Int: return SInt(3);
                case DataType.Bool: return SBool(false);
                case DataType.String: return SString("hi");
            }

            throw new RuntimeFatalException();
        }

        R.Exp NewROperand(DataType type)
        {
            switch (type)
            {
                case DataType.Int: return r.Int(3);
                case DataType.Bool: return r.Bool(false);
                case DataType.String: return r.String("hi");
            }

            throw new RuntimeFatalException();
        }

        IType NewType(DataType type)
        {
            switch (type)
            {
                case DataType.Int: return r.IntType();
                case DataType.Bool: return r.BoolType();
                case DataType.String: return r.StringType();
            }

            throw new RuntimeFatalException();
        }

        [Theory]
        [MemberData(nameof(Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial))]
        public void BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial(
            S.BinaryOpKind syntaxOpKind, DataType inputType, DataType outputType, R.InternalBinaryOperator ir0BinOp)
        {
            // var x = {inputType} {syntaxOpKind} {inputType}
            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "x", new S.BinaryOpExp(syntaxOpKind, NewSOperand(inputType), NewSOperand(inputType)))
            );

            var script = Translate(syntaxScript);

            // {outputType} x = {inputType} {syntaxOpKind} {inputType}
            var expected = r.Script(
                r.LocalVarDecl(NewType(outputType), "x", r.CallInternalBinary(ir0BinOp,
                    NewROperand(inputType),
                    NewROperand(inputType)
                ))
            );

            AssertEquals(expected, script);
        }

        public static IEnumerable<object[]> Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual()
        {   
            yield return new object[] { DataType.Bool, R.InternalBinaryOperator.Equal_Bool_Bool_Bool };
            yield return new object[] { DataType.Int, R.InternalBinaryOperator.Equal_Int_Int_Bool };
            yield return new object[] { DataType.String, R.InternalBinaryOperator.Equal_String_String_Bool };
        }

        [Theory]
        [MemberData(nameof(Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual))]
        public void BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual(
            DataType inputType, R.InternalBinaryOperator ir0BinOperator)
        {
            // var x = {inputType} != {inputType}

            var syntaxScript = SScript(
                SVarDeclStmt(SVarTypeExp(), "x", new S.BinaryOpExp(S.BinaryOpKind.NotEqual, NewSOperand(inputType), NewSOperand(inputType)))
            );

            var script = Translate(syntaxScript);

            // bool x = {inputType} != {inputType}
            var expected = r.Script(
                r.LocalVarDecl(r.BoolType(), "x",
                    r.CallInternalUnary(R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                        r.CallInternalBinary(ir0BinOperator, NewROperand(inputType), NewROperand(inputType))
                    )
                )
            );

            AssertEquals(expected, script);
        }

        #endregion // BinaryOp

        // 쓸데 없을것 같아서 drop, 그냥 Private으로 유지해도 될것 같다
        //[Fact]
        //void FuncDecl_EntryFuncPublicDefault()
        //{
        //    // int Main() { } <- public
        //    var scriptSyntax = SScript(
        //        new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
        //            accessModifier: null, // <- 지정하지 않아도
        //            isSequence: false,
        //            SIntTypeExp(),
        //            "Main",
        //            typeParams: default,
        //            parameters: default,
        //            SBody()
        //        ))
        //    );

        //    var result = Translate(scriptSyntax);

        //    var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

        //    var mainD = new GlobalFuncDeclSymbol(
        //        moduleD, 
        //        Accessor.Public, // Public으로 
        //        NormalName("Main"),
        //        typeParams: default
        //    );
        //    mainD.InitFuncReturnAndParams(r.FuncRet(r.IntType()), parameters: default);
        //    moduleD.AddFunc(mainD);

        //    var mainBody = r.StmtBody(mainD);

        //    var expected = r.Script(moduleD, mainBody);
        //    AssertEquals(expected, result);
        //}

        #region Call

        [Fact]
        void CallExp_RefArgumentTrivial_WorksProperly()
        {
            //void F(int* i) 
            //{
            //    *i = 4;
            //}

            //int j = 3;
            //F(&j);

            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    accessModifier: null, isSequence: false, SVoidTypeExp(), "F", typeParams: default,
                    Arr(new S.FuncParam(HasParams: false, SLocalPtrTypeExp(SIntTypeExp()), "i")),
                    SBody(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SId("i"), SInt(4))))
                )),
                
                SMain(
                    SVarDeclStmt(SIntTypeExp(), "j", SInt(3)),
                    new S.ExpStmt(new S.CallExp(SId("F"), Arr<S.Argument>(new S.Argument.Normal(SRefExp(SId("j"))))))
                )
            );

            var script = Translate(syntaxScript);

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var fD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("F"), typeParams: default);
            fD.InitFuncReturnAndParams(r.FuncRet(r.VoidType()), r.FuncParams((r.IntType(), "i")));
            moduleD.AddFunc(fD);
            var fBody = r.StmtBody(fD,
                r.Assign(r.LocalDeref(r.LoadLocalVar("i", r.LocalPtrType(r.IntType()))), r.Int(4))
            );

            var f = (GlobalFuncSymbol)fD.MakeOpenSymbol(factory);

            var mainBody = NewMain(moduleD,
                r.LocalVarDecl(r.IntType(), "j", r.Int(3)),
                r.Call(f, r.Arg(r.LocalRef(r.LocalVar("j"), r.IntType())))
            );

            var expected = r.Script(moduleD, fBody, mainBody);
            AssertEquals(expected, script);
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
                    accessModifier: null, isSequence: false, SIntTypeExp(), "Func", Arr(new S.TypeParam("T")),
                    Arr(new S.FuncParam(HasParams: false, SIntTypeExp(), "x")),
                    SBody(new S.ReturnStmt(new S.ReturnValueInfo(SId("x"))))
                )),

                SMain(
                    new S.ExpStmt(new S.CallExp(SId("Func", SIntTypeExp()), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                )
            );

            var script = Translate(syntaxScript);

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Func"), Arr(NormalName("T")));
            funcD.InitFuncReturnAndParams(r.FuncRet(r.IntType()), r.FuncParams((r.IntType(), "x")));
            moduleD.AddFunc(funcD);
            var funcBody = r.StmtBody(funcD, 
                r.Return(r.LoadLocalVar("x", r.IntType()))
            );

            var module = factory.MakeModule(moduleD);
            var func_Int = factory.MakeGlobalFunc(module, funcD, Arr(r.IntType()));

            var mainBody = NewMain(moduleD,
                r.Call(func_Int, r.Arg(r.Int(3)))
            );

            var expected = r.Script(moduleD, funcBody, mainBody);
            AssertEquals(expected, script);
        }

        // TODO: TypeArgument지원하면 General버전이랑 통합하기
        [Fact]
        void CallExp_TranslatesIntoCallFuncExpWithoutTypeArgument()
        {
            // int Func(int x) { return x; }
            // Func(3);
            var syntaxScript = SScript(
                new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                    accessModifier: null, isSequence: false, SIntTypeExp(), "Func", typeParams: default,
                    Arr(new S.FuncParam(HasParams: false, SIntTypeExp(), "x")),
                    SBody(new S.ReturnStmt(new S.ReturnValueInfo(SId("x"))))
                )),

                SMain(
                    new S.ExpStmt(new S.CallExp(SId("Func"), Arr<S.Argument>(new S.Argument.Normal(SInt(3)))))
                )
            );

            var script = Translate(syntaxScript);

            var moduleD = new ModuleDeclSymbol(moduleName, bReference: false);

            var funcD = new GlobalFuncDeclSymbol(moduleD, Accessor.Private, NormalName("Func"), typeParams: default);
            funcD.InitFuncReturnAndParams(r.FuncRet(r.IntType()), r.FuncParams((r.IntType(), "x")));
            moduleD.AddFunc(funcD);

            var funcBody = r.StmtBody(funcD,
                r.Return(r.LoadLocalVar("x", r.IntType()))
            );

            var func = (GlobalFuncSymbol)funcD.MakeOpenSymbol(factory);

            var mainBody = NewMain(moduleD,
                r.Call(func, r.Arg(r.Int(3)))
            );

            var expected = r.Script(moduleD, funcBody, mainBody);
            AssertEquals(expected, script);
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

        #endregion // Call

        #region Lambda

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

        #endregion // Lambda

        #region Indexer

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

        #endregion Indexer

        #region MemberCall

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

        #endregion

        #region List

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

        #endregion

        #region UninitializeVariable

        [Fact]
        public void UninitializedVariableAnalysis_UseUninitializedLocalVarDecl_ReturnError()
        {
            // {
            //     int a; // local var without initialize
            //     @$a    // use, error
            // }

            S.Exp e;
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "a", null),
                SCommand(SString(new S.ExpStringExpElement(e = SId("a"))))
            );

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
            var syntaxScript = SScript(
                SVarDeclStmt(SIntTypeExp(), "a", SInt(1)),
                SCommand(SString(new S.ExpStringExpElement(e = SId("a"))))
            );

            var expected = Translate(syntaxScript);
        }

        #endregion UninitializeVariable
    }
}
