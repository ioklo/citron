using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Gum.CompileTime;

using Gum.Infra;
using Gum.Misc;
using System.Linq;

using static Gum.IR0.AnalyzeErrorCode;
using S = Gum.Syntax;

namespace Gum.IR0
{
    public class TranslatorTests
    {
        Script? Translate(S.Script syntaxScript, bool raiseAssertFailed = true)
        {
            var testErrorCollector = new TestErrorCollector(raiseAssertFailed);
            var translator = new Translator();

            return translator.Translate(syntaxScript, Array.Empty<IModuleInfo>(), testErrorCollector);
        }

        List<IError> TranslateWithErrors(S.Script syntaxScript, bool raiseAssertionFail = false)
        {
            var testErrorCollector = new TestErrorCollector(raiseAssertionFail);
            var translator = new Translator();

            var script = translator.Translate(syntaxScript, Array.Empty<IModuleInfo>(), testErrorCollector);

            return testErrorCollector.Errors;
        }

        static T[] Arr<T>(params T[] values)
        {
            return values;
        }

        static Script SimpleScript(IEnumerable<TypeDecl>? typeDecls, IEnumerable<FuncDecl>? funcDecls, params Stmt[] topLevelStmts)
        {
            // TODO: Validator
            int i = 0;
            foreach(var funcDecl in funcDecls ?? Array.Empty<FuncDecl>())
            {
                Assert.Equal(i, funcDecl.Id.Value);
                i++;
            }

            return new Script(typeDecls ?? Array.Empty<TypeDecl>(), funcDecls ?? Array.Empty<FuncDecl>(), topLevelStmts);
        }

        void VerifyError(IEnumerable<IError> errors, AnalyzeErrorCode code, S.ISyntaxNode node)
        {
            var result = errors.OfType<AnalyzeError>()
                .Any(error => error.Code == code && error.Node == node);

            Assert.True(result, $"Errors doesn't contain (Code: {code}, Node: {node})");
        }

        static S.VarDecl SimpleSVarDecl(S.TypeExp typeExp, string name, S.Exp? initExp = null)
        {
            return new S.VarDecl(typeExp, Arr(new S.VarDeclElement(name, initExp)));
        }

        static S.VarDeclStmt SimpleSVarDeclStmt(S.TypeExp typeExp, string name, S.Exp? initExp = null)
        {
            return new S.VarDeclStmt(SimpleSVarDecl(typeExp, name, initExp));
        }

        static S.Script SimpleSScript(params S.Stmt[] stmts)
        {
            return new S.Script(stmts.Select(stmt => new S.Script.StmtElement(stmt)));
        }

        static S.IntLiteralExp SimpleSInt(int v) => new S.IntLiteralExp(v);
        static S.BoolLiteralExp SimpleSBool(bool v) => new S.BoolLiteralExp(v);
        static S.IdentifierExp SimpleSId(string name) => new S.IdentifierExp(name);

        static S.StringExp SimpleSString(string s) => new S.StringExp(new S.TextStringExpElement(s));

        static PrivateGlobalVarDeclStmt SimpleGlobalVarDeclStmt(Type type, string name, Exp? initExp = null)
            => new PrivateGlobalVarDeclStmt(Arr(new PrivateGlobalVarDeclStmt.Element(name, type, initExp)));

        static LocalVarDeclStmt SimpleLocalVarDeclStmt(Type typeId, string name, Exp? initExp = null)
            => new LocalVarDeclStmt(SimpleLocalVarDecl(typeId, name, initExp));

        static LocalVarDecl SimpleLocalVarDecl(Type typeId, string name, Exp? initExp = null) 
            => new LocalVarDecl(Arr(new LocalVarDecl.Element(name, typeId, initExp)));

        static IntLiteralExp SimpleInt(int v) => new IntLiteralExp(v);
        static BoolLiteralExp SimpleBool(bool v) => new BoolLiteralExp(v);
        static StringExp SimpleString(string v) => new StringExp(new TextStringExpElement(v));
        
        static S.TypeExp VarTypeExp { get => new S.IdTypeExp("var"); }
        static S.TypeExp IntTypeExp { get => new S.IdTypeExp("int"); }
        static S.TypeExp BoolTypeExp { get => new S.IdTypeExp("bool"); }
        static S.TypeExp VoidTypeExp { get => new S.IdTypeExp("void"); }
        static S.TypeExp StringTypeExp { get => new S.IdTypeExp("string"); }


        // Trivial Cases
        [Fact]
        public void CommandStmt_TranslatesTrivially()
        {   
            var syntaxCmdStmt = new S.CommandStmt(
                new S.StringExp(
                    new S.TextStringExpElement("Hello "),
                    new S.ExpStringExpElement(new S.StringExp(new S.TextStringExpElement("World")))));

            var syntaxScript = SimpleSScript(syntaxCmdStmt);

            var script = Translate(syntaxScript);
            
            var expectedStmt = new CommandStmt(
                new StringExp(
                    new TextStringExpElement("Hello "),
                    new ExpStringExpElement(new StringExp(new TextStringExpElement("World")))));

            var expected = new Script(Array.Empty<TypeDecl>(), Array.Empty<FuncDecl>(), new[] { expectedStmt });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }
        
        [Fact]
        public void VarDeclStmt_TranslatesIntoPrivateGlobalVarDecl()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(SimpleSVarDeclStmt(IntTypeExp, "x", SimpleSInt(1))));
            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.Int, "x", SimpleInt(1))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInTopLevelScope()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(IntTypeExp, "x", SimpleSInt(1))
                )
            ));
            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                new BlockStmt(
                    new LocalVarDeclStmt(new LocalVarDecl(Arr(new LocalVarDecl.Element("x", Type.Int, SimpleInt(1)))))
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInFuncScope()
        {
            var syntaxScript = new S.Script(
                new S.Script.FuncDeclElement(new S.FuncDecl(false, VoidTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null),
                    new S.BlockStmt(
                        SimpleSVarDeclStmt(IntTypeExp, "x", SimpleSInt(1))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var funcDecl = new FuncDecl.Normal(new FuncDeclId(0), false, Array.Empty<string>(), Array.Empty<string>(), new BlockStmt(

                new LocalVarDeclStmt(new LocalVarDecl(Arr(new LocalVarDecl.Element("x", Type.Int, SimpleInt(1)))))

            ));

            var expected = SimpleScript(null, Arr(funcDecl));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void VarDeclStmt_InfersVarType()
        {
            var syntaxScript = SimpleSScript(
                new S.VarDeclStmt(new S.VarDecl(VarTypeExp, new S.VarDeclElement("x", SimpleSInt(3))))
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.Int, "x", SimpleInt(3))            
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void VarDeclStmt_ChecksLocalVarNameIsUniqueWithinScope()
        {
            S.VarDeclElement elem;

            var syntaxScript = SimpleSScript(new S.BlockStmt(
                new S.VarDeclStmt(new S.VarDecl(IntTypeExp, new S.VarDeclElement("x", null))),
                new S.VarDeclStmt(new S.VarDecl(IntTypeExp, elem = new S.VarDeclElement("x", null)))

            ));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);
        }

        [Fact]
        public void VarDeclStmt_ChecksLocalVarNameIsUniqueWithinScope2()
        {
            S.VarDeclElement element;

            var syntaxScript = SimpleSScript(new S.BlockStmt(
                new S.VarDeclStmt(new S.VarDecl(IntTypeExp, new S.VarDeclElement("x", null), element = new S.VarDeclElement("x", null)))
            ));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, element);
        }

        [Fact]
        public void VarDeclStmt_ChecksGlobalVarNameIsUnique()
        {
            S.VarDeclElement elem;

            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "x"),
                new S.VarDeclStmt(new S.VarDecl(IntTypeExp, elem = new S.VarDeclElement("x", null)))

            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);
        }

        [Fact]
        public void VarDeclStmt_ChecksGlobalVarNameIsUnique2()
        {
            S.VarDeclElement elem;

            var syntaxScript = SimpleSScript(
                new S.VarDeclStmt(new S.VarDecl(IntTypeExp, 
                    new S.VarDeclElement("x", null),
                    elem = new S.VarDeclElement("x", null)
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);
        }

        [Fact]
        public void IfStmt_TranslatesTrivially()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(

                new S.IfStmt(new S.BoolLiteralExp(false), null, S.BlankStmt.Instance, S.BlankStmt.Instance)
                
            ));

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null,

                new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, BlankStmt.Instance)

            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void IfStmt_ReportsErrorWhenCondTypeIsNotBool()
        {
            S.Exp cond;

            var syntaxScript = new S.Script(new S.Script.StmtElement(

                new S.IfStmt(cond = SimpleSInt(3), null, S.BlankStmt.Instance, S.BlankStmt.Instance)

            ));

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1004_IfStmt_ConditionShouldBeBool, cond);
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
            var syntaxScript = new S.Script(
                
                new S.Script.StmtElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SimpleSVarDecl(IntTypeExp, "x")),
                    null, null, S.BlankStmt.Instance
                )),

                new S.Script.StmtElement(SimpleSVarDeclStmt(StringTypeExp, "x")),

                new S.Script.StmtElement(new S.ForStmt(
                    new S.ExpForStmtInitializer(new S.BinaryOpExp(S.BinaryOpKind.Assign, SimpleSId("x"), SimpleSString("Hello"))),
                    null, null, S.BlankStmt.Instance
                ))
            );            

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 

                new ForStmt(
                    new VarDeclForStmtInitializer(SimpleLocalVarDecl(Type.Int, "x")),
                    null, null, BlankStmt.Instance
                ),

                SimpleGlobalVarDeclStmt(Type.String, "x", null),

                new ForStmt(
                    new ExpForStmtInitializer(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleString("Hello")), Type.String)),
                    null, null, BlankStmt.Instance
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        // 
        [Fact]
        public void ForStmt_ChecksVarDeclInitializerScope() 
        {
            var syntaxScript = new S.Script(

                new S.Script.StmtElement(SimpleSVarDeclStmt(StringTypeExp, "x")),

                new S.Script.StmtElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SimpleSVarDecl(IntTypeExp, "x")), // x의 범위는 ForStmt내부에서
                    new S.BinaryOpExp(S.BinaryOpKind.Equal, SimpleSId("x"), SimpleSInt(3)),
                    null, S.BlankStmt.Instance
                )),

                new S.Script.StmtElement(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SimpleSId("x"), SimpleSString("Hello"))))
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 

                SimpleGlobalVarDeclStmt(Type.String, "x", null),

                new ForStmt(
                    new VarDeclForStmtInitializer(SimpleLocalVarDecl(Type.Int, "x")),

                    // cond
                    new CallInternalBinaryOperatorExp(
                        InternalBinaryOperator.Equal_Int_Int_Bool,
                        new ExpInfo(new LocalVarExp("x"), Type.Int),
                        new ExpInfo(SimpleInt(3), Type.Int)
                    ),
                    null, BlankStmt.Instance
                ),

                new ExpStmt(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleString("Hello")), Type.String))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }


        [Fact]
        public void ForStmt_ChecksConditionIsBool()
        {
            S.Exp cond;

            var syntaxScript = new S.Script(
                new S.Script.StmtElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SimpleSVarDecl(IntTypeExp, "x")),
                    cond = SimpleSInt(3),
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

            var syntaxScript = new S.Script(
                new S.Script.StmtElement(new S.ForStmt(
                    new S.ExpForStmtInitializer(exp = SimpleSInt(3)), // error
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

            var syntaxScript = new S.Script(

                new S.Script.StmtElement(new S.ForStmt(
                    null,
                    null,
                    continueExp = SimpleSInt(3), 
                    S.BlankStmt.Instance
                ))
                
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1103_ForStmt_ContinueExpShouldBeAssignOrCall, continueExp);
        }

        [Fact]
        public void ContinueStmt_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(
                new S.ForStmt(null, null, null, S.ContinueStmt.Instance),
                new S.ForeachStmt(IntTypeExp, "x", new S.ListExp(IntTypeExp), S.ContinueStmt.Instance)
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                new ForStmt(null, null, null, ContinueStmt.Instance),
                new ForeachStmt(Type.Int, "x", new ExpInfo(new ListExp(Type.Int, Array.Empty<Exp>()), Type.List(Type.Int)), ContinueStmt.Instance)
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ContinueStmt_ChecksUsedInLoop()
        {
            S.ContinueStmt continueStmt;
            var syntaxScript = SimpleSScript(continueStmt = S.ContinueStmt.Instance);

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);
        }

        [Fact]
        public void BreakStmt_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(
                new S.ForStmt(null, null, null, S.BreakStmt.Instance),
                    new S.ForeachStmt(IntTypeExp, "x", new S.ListExp(IntTypeExp), S.BreakStmt.Instance)
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                new ForStmt(null, null, null, BreakStmt.Instance),
                new ForeachStmt(Type.Int, "x", new ExpInfo(new ListExp(Type.Int, Array.Empty<Exp>()), Type.List(Type.Int)), BreakStmt.Instance)
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void BreakStmt_ChecksUsedInLoop()
        {
            S.BreakStmt breakStmt;
            var syntaxScript = SimpleSScript(breakStmt = S.BreakStmt.Instance);

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1601_BreakStmt_ShouldUsedInLoop, breakStmt);
        }
        
        [Fact]
        public void ReturnStmt_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(new S.ReturnStmt(SimpleSInt(2)));

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null, new ReturnStmt(SimpleInt(2)));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ReturnStmt_TranslatesReturnStmtInSeqFuncTrivially()
        {
            var syntaxScript = new S.Script(new S.Script.FuncDeclElement(new S.FuncDecl(
                true, IntTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null),
                new S.BlockStmt(
                    new S.ReturnStmt(null)
                )
            )));

            var script = Translate(syntaxScript);

            var seqFunc = new FuncDecl.Sequence(new FuncDeclId(0), Type.Int, false, Array.Empty<string>(), Array.Empty<string>(), new BlockStmt(new ReturnStmt(null)));

            var expected = SimpleScript(null, new[] { seqFunc });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ReturnStmt_ChecksMatchFuncRetTypeAndRetValue()
        {
            S.Exp retValue;

            var funcDecl = new S.FuncDecl(false, IntTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null), new S.BlockStmt(
                new S.ReturnStmt(retValue = SimpleSString("Hello"))
            ));

            var syntaxScript = new S.Script(new S.Script.FuncDeclElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, retValue);
        }

        [Fact]
        public void ReturnStmt_ChecksMatchVoidTypeAndReturnNothing()
        {
            S.ReturnStmt retStmt;

            var funcDecl = new S.FuncDecl(false, IntTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null), new S.BlockStmt(
                retStmt = new S.ReturnStmt(null)
            ));

            var syntaxScript = new S.Script(new S.Script.FuncDeclElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, retStmt);
        }

        [Fact]
        public void ReturnStmt_ChecksSeqFuncShouldReturnNothing()
        {
            S.ReturnStmt retStmt;

            var funcDecl = new S.FuncDecl(true, IntTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null), new S.BlockStmt(
                retStmt = new S.ReturnStmt(SimpleSInt(2))
            ));

            var syntaxScript = new S.Script(new S.Script.FuncDeclElement(funcDecl));
            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1202_ReturnStmt_SeqFuncShouldReturnVoid, retStmt);
        }

        [Fact]
        public void ReturnStmt_ShouldReturnIntWhenUsedInTopLevelStmt()
        {
            S.Exp exp;
            var syntaxScript = SimpleSScript(new S.ReturnStmt(exp = SimpleSString("Hello")));

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, exp);
        }

        [Fact]
        public void ReturnStmt_UsesHintType()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum, Prerequisite.TypeHint);
        }

        [Fact]
        public void BlockStmt_TranslatesVarDeclStmtWithinBlockStmtOfTopLevelStmtIntoLocalVarDeclStmt()
        {
            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(StringTypeExp, "x", SimpleSString("Hello"))
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                new BlockStmt(
                    SimpleLocalVarDeclStmt(Type.String, "x", SimpleString("Hello")) // not PrivateGlobalVarDecl
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
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

            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(StringTypeExp, "x", SimpleSString("Hello"))
                ),

                new S.CommandStmt(new S.StringExp(new S.ExpStringExpElement(exp = SimpleSId("x"))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A0501_IdExp_VariableNotFound, exp);
        }   
        
        [Fact]
        public void ExpStmt_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "x"),
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SimpleSId("x"), SimpleSInt(3)))
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.Int, "x", null),
                new ExpStmt(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleInt(3)), Type.Int))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ExpStmt_ChecksExpIsAssignOrCall()
        {
            S.Exp exp;
            var syntaxScript = SimpleSScript(
                new S.ExpStmt(exp = SimpleSInt(3))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1301_ExpStmt_ExpressionShouldBeAssignOrCall, exp);
        }

        [Fact]
        public void TaskStmt_TranslatesWithGlobalVariable()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "x"),
                new S.TaskStmt(
                    new S.ExpStmt(
                        new S.BinaryOpExp(S.BinaryOpKind.Assign, SimpleSId("x"), SimpleSInt(3))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.Int, "x", null),
                new TaskStmt(
                    new ExpStmt(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleInt(3)), Type.Int)),
                    new CaptureInfo(false, Array.Empty<CaptureInfo.Element>())
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void TaskStmt_ChecksAssignToLocalVariableOutsideLambda()
        {
            S.Exp exp;

            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(IntTypeExp, "x"),
                    new S.TaskStmt(
                        new S.ExpStmt(
                            new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SimpleSId("x"), SimpleSInt(3))
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
            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(IntTypeExp, "x"),
                    new S.TaskStmt(
                        SimpleSVarDeclStmt(IntTypeExp, "x", SimpleSId("x"))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                new BlockStmt(
                    SimpleLocalVarDeclStmt(Type.Int, "x"),
                    new TaskStmt(
                        SimpleLocalVarDeclStmt(Type.Int, "x", new LocalVarExp("x")),
                        new CaptureInfo(false, new [] { new CaptureInfo.Element(Type.Int, "x") })
                    )
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void AwaitStmt_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(
                new S.AwaitStmt(
                    S.BlankStmt.Instance
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, new AwaitStmt(BlankStmt.Instance));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void AwaitStmt_ChecksLocalVariableScope()
        {
            S.Exp exp;

            var syntaxScript = SimpleSScript(
                new S.AwaitStmt(
                    SimpleSVarDeclStmt(StringTypeExp, "x", SimpleSString("Hello"))
                ),

                new S.CommandStmt(new S.StringExp(new S.ExpStringExpElement(exp = SimpleSId("x"))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A0501_IdExp_VariableNotFound, exp);
        }

        [Fact]
        public void AsyncStmt_TranslatesWithGlobalVariable()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "x"),
                new S.AsyncStmt(
                    new S.ExpStmt(
                        new S.BinaryOpExp(S.BinaryOpKind.Assign, SimpleSId("x"), SimpleSInt(3))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.Int, "x", null),
                new AsyncStmt(
                    new ExpStmt(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleInt(3)), Type.Int)),
                    new CaptureInfo(false, Array.Empty<CaptureInfo.Element>())
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void AsyncStmt_ChecksAssignToLocalVariableOutsideLambda()
        {
            S.Exp exp;

            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(IntTypeExp, "x"),
                    new S.AsyncStmt(
                        new S.ExpStmt(
                            new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SimpleSId("x"), SimpleSInt(3))
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
            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(IntTypeExp, "x"),
                    new S.AsyncStmt(
                        SimpleSVarDeclStmt(IntTypeExp, "x", SimpleSId("x"))
                    )
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, 
                new BlockStmt(
                    SimpleLocalVarDeclStmt(Type.Int, "x"),
                    new AsyncStmt(
                        SimpleLocalVarDeclStmt(Type.Int, "x", new LocalVarExp("x")),
                        new CaptureInfo(false, new [] { new CaptureInfo.Element(Type.Int, "x") })
                    )
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ForeachStmt_TranslatesTrivially()
        {
            var scriptSyntax = SimpleSScript(new S.ForeachStmt(IntTypeExp, "x", new S.ListExp(IntTypeExp), S.BlankStmt.Instance));

            var script = Translate(scriptSyntax);

            var expected = SimpleScript(null, null, new ForeachStmt(Type.Int, "x",
                new ExpInfo(new ListExp(Type.Int, Array.Empty<Exp>()),
                Type.List(Type.Int)), BlankStmt.Instance
            ));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ForeachStmt_ChecksIteratorIsListOrEnumerable()
        {
            S.Exp iterator;
            var scriptSyntax = SimpleSScript(new S.ForeachStmt(IntTypeExp, "x", iterator = SimpleSInt(3), S.BlankStmt.Instance));

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
            var scriptSyntax = SimpleSScript(foreachStmt = new S.ForeachStmt(StringTypeExp, "x", new S.ListExp(IntTypeExp), S.BlankStmt.Instance));

            var errors = TranslateWithErrors(scriptSyntax);

            VerifyError(errors, A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType, foreachStmt);
        }

        [Fact]
        public void YieldStmt_TranslatesTrivially()
        {
            var syntaxScript = new S.Script(
                new S.Script.FuncDeclElement(new S.FuncDecl(
                    true, IntTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null),
                    new S.BlockStmt(
                        new S.YieldStmt(SimpleSInt(3))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var seqFunc = new FuncDecl.Sequence(new FuncDeclId(0), Type.Int, false, Array.Empty<string>(), Array.Empty<string>(), new BlockStmt(
                new YieldStmt(SimpleInt(3))
            ));

            var expected = SimpleScript(null, new[] { seqFunc });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void YieldStmt_ChecksYieldStmtUsedInSeqFunc()
        {
            S.YieldStmt yieldStmt;

            var syntaxScript = new S.Script(
                new S.Script.FuncDeclElement(new S.FuncDecl(
                    false, IntTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null),
                    new S.BlockStmt(
                        yieldStmt = new S.YieldStmt(SimpleSInt(3))
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

            var syntaxScript = new S.Script(
                new S.Script.FuncDeclElement(new S.FuncDecl(
                    true, StringTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null),
                    new S.BlockStmt(
                        new S.YieldStmt(yieldValue = SimpleSInt(3))
                    )
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType, yieldValue);
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
        public void IdExp_TranslatesIntoPrivateGlobalVarExp()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "x", SimpleSInt(3)),
                SimpleSVarDeclStmt(IntTypeExp, "y", SimpleSId("x"))
            );

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null,
                SimpleGlobalVarDeclStmt(Type.Int, "x", SimpleInt(3)),
                SimpleGlobalVarDeclStmt(Type.Int, "y", new PrivateGlobalVarExp("x"))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void IdExp_TranslatesLocalVarOutsideLambdaIntoLocalVarExp()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void IdExp_TranslatesIntoLocalVarExp()
        {
            var syntaxScript = SimpleSScript(new S.BlockStmt(
                SimpleSVarDeclStmt(IntTypeExp, "x", SimpleSInt(3)),
                SimpleSVarDeclStmt(IntTypeExp, "y", SimpleSId("x"))
            ));

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null, new BlockStmt(
                SimpleLocalVarDeclStmt(Type.Int, "x", SimpleInt(3)),
                SimpleLocalVarDeclStmt(Type.Int, "y", new LocalVarExp("x"))
            ));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
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
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(BoolTypeExp, "b1", new S.BoolLiteralExp(false)),
                SimpleSVarDeclStmt(BoolTypeExp, "b2", new S.BoolLiteralExp(true)));

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.Bool, "b1", new BoolLiteralExp(false)),
                SimpleGlobalVarDeclStmt(Type.Bool, "b2", new BoolLiteralExp(true))                    
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void IntLiteralExp_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "i", new S.IntLiteralExp(34)));                

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.Int, "i", new IntLiteralExp(34))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void StringExp_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(StringTypeExp, "s", SimpleSString("Hello")));

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null, 
                SimpleGlobalVarDeclStmt(Type.String, "s", SimpleString("Hello"))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void StringExp_WrapsExpStringExpElementWhenExpIsBoolOrInt()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(StringTypeExp, "s1", new S.StringExp(new S.ExpStringExpElement(SimpleSInt(3)))),
                SimpleSVarDeclStmt(StringTypeExp, "s2", new S.StringExp(new S.ExpStringExpElement(SimpleSBool(true))))
            );

            var script = Translate(syntaxScript, true);

            var expected = SimpleScript(null, null, 

                SimpleGlobalVarDeclStmt(Type.String, "s1", new StringExp(new ExpStringExpElement(
                    new CallInternalUnaryOperatorExp(InternalUnaryOperator.ToString_Int_String, new ExpInfo(SimpleInt(3), Type.Int))
                ))),

                SimpleGlobalVarDeclStmt(Type.String, "s2", new StringExp(new ExpStringExpElement(
                    new CallInternalUnaryOperatorExp(InternalUnaryOperator.ToString_Bool_String, new ExpInfo(SimpleBool(true), Type.Bool))
                )))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void StringExp_ChecksStringExpElementIsStringConvertible()
        {
            S.Exp exp;
            // convertible가능하지 않은 것,, Lambda?
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(StringTypeExp, "s", new S.StringExp(new S.ExpStringExpElement(exp = new S.LambdaExp(
                    new S.LambdaExpParam[] { },
                    S.BlankStmt.Instance                    
                ))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, exp);
        }

        [Fact]
        public void UnaryOpExp_TranslatesTrivially()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(VarTypeExp, "x1", new S.UnaryOpExp(S.UnaryOpKind.LogicalNot, SimpleSBool(false))),
                SimpleSVarDeclStmt(VarTypeExp, "x2", new S.UnaryOpExp(S.UnaryOpKind.Minus, SimpleSInt(3))),
                SimpleSVarDeclStmt(VarTypeExp, "x3", new S.UnaryOpExp(S.UnaryOpKind.PrefixInc, SimpleSId("x2"))),
                SimpleSVarDeclStmt(VarTypeExp, "x4", new S.UnaryOpExp(S.UnaryOpKind.PrefixDec, SimpleSId("x2"))),
                SimpleSVarDeclStmt(VarTypeExp, "x5", new S.UnaryOpExp(S.UnaryOpKind.PostfixInc, SimpleSId("x2"))),
                SimpleSVarDeclStmt(VarTypeExp, "x6", new S.UnaryOpExp(S.UnaryOpKind.PostfixDec, SimpleSId("x2")))
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null,
                SimpleGlobalVarDeclStmt(Type.Bool, "x1", new CallInternalUnaryOperatorExp(InternalUnaryOperator.LogicalNot_Bool_Bool, new ExpInfo(SimpleBool(false), Type.Bool))),
                SimpleGlobalVarDeclStmt(Type.Int, "x2", new CallInternalUnaryOperatorExp(InternalUnaryOperator.UnaryMinus_Int_Int, new ExpInfo(SimpleInt(3), Type.Int))),
                SimpleGlobalVarDeclStmt(Type.Int, "x3", new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PrefixInc_Int_Int, new PrivateGlobalVarExp("x2"))),
                SimpleGlobalVarDeclStmt(Type.Int, "x4", new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PrefixDec_Int_Int, new PrivateGlobalVarExp("x2"))),
                SimpleGlobalVarDeclStmt(Type.Int, "x5", new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixInc_Int_Int, new PrivateGlobalVarExp("x2"))),
                SimpleGlobalVarDeclStmt(Type.Int, "x6", new CallInternalUnaryAssignOperator(InternalUnaryAssignOperator.PostfixDec_Int_Int, new PrivateGlobalVarExp("x2")))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfUnaryAssignExpIsIntType()
        {
            S.Exp operand;
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(StringTypeExp, "x", SimpleSString("Hello")),
                SimpleSVarDeclStmt(VarTypeExp, "i", new S.UnaryOpExp(S.UnaryOpKind.PrefixInc, operand = SimpleSId("x")))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfUnaryAssignExpIsAssignable()
        {
            S.Exp operand;
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(VarTypeExp, "i", new S.UnaryOpExp(S.UnaryOpKind.PrefixInc, operand = SimpleSInt(3)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfLogicalNotIsBoolType()
        {
            S.Exp operand;
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(VarTypeExp, "b", new S.UnaryOpExp(S.UnaryOpKind.LogicalNot, operand = SimpleSInt(3)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, operand);
        }

        [Fact]
        public void UnaryOpExp_ChecksOperandOfUnaryMinusIsIntType()
        {
            S.Exp operand;
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(VarTypeExp, "i", new S.UnaryOpExp(S.UnaryOpKind.Minus, operand = SimpleSBool(false)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, operand);
        }

        [Fact]
        void BinaryOpExp_TranslatesIntoAssignExp()
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "x"),
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, SimpleSId("x"), SimpleSInt(3)))
            );

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null,
                SimpleGlobalVarDeclStmt(Type.Int, "x"),
                new ExpStmt(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleInt(3)), Type.Int))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        void BinaryOpExp_ChecksCompatibleBetweenOperandsOnAssignOperation()
        {
            S.BinaryOpExp binOpExp;
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(IntTypeExp, "x"),
                new S.ExpStmt(binOpExp = new S.BinaryOpExp(S.BinaryOpKind.Assign, SimpleSId("x"), SimpleSBool(true)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType, binOpExp);
        }

        [Fact]
        void BinaryOpExp_ChecksLeftOperandIsAssignableOnAssignOperation()
        {
            S.Exp exp;
            var syntaxScript = SimpleSScript(
                new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, exp = SimpleSInt(3), SimpleSInt(4)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0803_BinaryOp_LeftOperandIsNotAssignable, exp);
        }

        [Fact]
        void BinaryOpExp_ChecksOperatorNotFound()
        {
            S.Exp exp;
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(StringTypeExp, "x", exp = new S.BinaryOpExp(S.BinaryOpKind.Multiply, SimpleSString("Hello"), SimpleSInt(4)))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A0802_BinaryOp_OperatorNotFound, exp);
        }

        static IEnumerable<object[]> Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial()
        {
            Func<S.Exp> SInt = () => SimpleSInt(1);
            Func<ExpInfo> IntInfo = () => new ExpInfo(SimpleInt(1), Type.Int);

            yield return new object[] { S.BinaryOpKind.Multiply, SInt, IntInfo, Type.Int, InternalBinaryOperator.Multiply_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Divide, SInt, IntInfo, Type.Int, InternalBinaryOperator.Divide_Int_Int_Int};
            yield return new object[] { S.BinaryOpKind.Modulo, SInt, IntInfo, Type.Int, InternalBinaryOperator.Modulo_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Add, SInt, IntInfo, Type.Int, InternalBinaryOperator.Add_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.Subtract, SInt, IntInfo, Type.Int, InternalBinaryOperator.Subtract_Int_Int_Int };
            yield return new object[] { S.BinaryOpKind.LessThan, SInt, IntInfo, Type.Bool, InternalBinaryOperator.LessThan_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThan, SInt, IntInfo, Type.Bool, InternalBinaryOperator.GreaterThan_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.LessThanOrEqual, SInt, IntInfo, Type.Bool, InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThanOrEqual, SInt, IntInfo, Type.Bool, InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool };
            yield return new object[] { S.BinaryOpKind.Equal, SInt, IntInfo, Type.Bool, InternalBinaryOperator.Equal_Int_Int_Bool };

            Func<S.Exp> SString = () => SimpleSString("Hello");
            Func<ExpInfo> StringInfo = () => new ExpInfo(SimpleString("Hello"), Type.String);

            yield return new object[] { S.BinaryOpKind.Add, SString, StringInfo, Type.String, InternalBinaryOperator.Add_String_String_String };
            yield return new object[] { S.BinaryOpKind.LessThan, SString, StringInfo, Type.Bool, InternalBinaryOperator.LessThan_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThan, SString, StringInfo, Type.Bool, InternalBinaryOperator.GreaterThan_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.LessThanOrEqual, SString, StringInfo, Type.Bool, InternalBinaryOperator.LessThanOrEqual_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.GreaterThanOrEqual, SString, StringInfo, Type.Bool, InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool };
            yield return new object[] { S.BinaryOpKind.Equal, SString, StringInfo, Type.Bool, InternalBinaryOperator.Equal_String_String_Bool };

            Func<S.Exp> SBool = () => SimpleSBool(true);
            Func<ExpInfo> BoolInfo = () => new ExpInfo(SimpleBool(true), Type.Bool);

            yield return new object[] { S.BinaryOpKind.Equal, SBool, BoolInfo, Type.Bool, InternalBinaryOperator.Equal_Bool_Bool_Bool};

            // NotEqual
        }

        [Theory]
        [MemberData(nameof(Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial))]
        public void BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_Trivial(
            S.BinaryOpKind syntaxOpKind, Func<S.Exp> newSOperand, Func<ExpInfo> newOperandInfo, Type resultType, InternalBinaryOperator ir0BinOp)
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(VarTypeExp, "x", new S.BinaryOpExp(syntaxOpKind, newSOperand.Invoke(), newSOperand.Invoke()))
            );

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null,
                SimpleGlobalVarDeclStmt(resultType, "x", new CallInternalBinaryOperatorExp(ir0BinOp,
                    newOperandInfo.Invoke(),
                    newOperandInfo.Invoke()
                ))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        static IEnumerable<object[]> Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual()
        {
            Func<S.Exp> SBool = () => SimpleSBool(true);
            Func<ExpInfo> BoolInfo = () => new ExpInfo(SimpleBool(true), Type.Bool);

            Func<S.Exp> SInt = () => SimpleSInt(1);
            Func<ExpInfo> IntInfo = () => new ExpInfo(SimpleInt(1), Type.Int);

            Func<S.Exp> SString = () => SimpleSString("Hello");
            Func<ExpInfo> StringInfo = () => new ExpInfo(SimpleString("Hello"), Type.String);

            yield return new object[] { SBool, BoolInfo, InternalBinaryOperator.Equal_Bool_Bool_Bool };
            yield return new object[] { SInt, IntInfo, InternalBinaryOperator.Equal_Int_Int_Bool };
            yield return new object[] { SString, StringInfo, InternalBinaryOperator.Equal_String_String_Bool };
        }

        [Theory]
        [MemberData(nameof(Data_BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual))]
        public void BinaryOpExp_TranslatesIntoCallInternalBinaryOperatorExp_NotEqual(
            Func<S.Exp> newSOperand, Func<ExpInfo> newOperandInfo, InternalBinaryOperator ir0BinOperator)
        {
            var syntaxScript = SimpleSScript(
                SimpleSVarDeclStmt(VarTypeExp, "x", new S.BinaryOpExp(S.BinaryOpKind.NotEqual, newSOperand.Invoke(), newSOperand.Invoke()))
            );

            var script = Translate(syntaxScript);
            var expected = SimpleScript(null, null,
                SimpleGlobalVarDeclStmt(Type.Bool, "x", 
                    new CallInternalUnaryOperatorExp(InternalUnaryOperator.LogicalNot_Bool_Bool, new ExpInfo(
                        new CallInternalBinaryOperatorExp(ir0BinOperator, newOperandInfo.Invoke(), newOperandInfo.Invoke()),
                        Type.Bool
                    ))
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        void CallExp_TranslatesIntoNewEnumExp()
        {
            throw new PrerequisiteRequiredException(Prerequisite.Enum);
        }

        [Fact]
        void CallExp_TranslatesIntoCallFuncExp()
        {
            var syntaxScript = new S.Script(
                new S.Script.FuncDeclElement(new S.FuncDecl(
                    false, IntTypeExp, "Func", Arr("T"), 
                    new S.FuncParamInfo(Arr(new S.TypeAndName(IntTypeExp, "x")), null),
                    new S.BlockStmt(new S.ReturnStmt(SimpleSId("x")))
                )),

                new S.Script.StmtElement(
                    new S.ExpStmt(new S.CallExp(SimpleSId("Func"), Arr( IntTypeExp ), SimpleSInt(3)))
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null,
                Arr(
                    new FuncDecl.Normal(
                        new FuncDeclId(0), false, Arr("T"), Arr("x"), new BlockStmt(new ReturnStmt(new LocalVarExp("x")))
                    )
                )
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        // TODO: TypeArgument지원하면 General버전이랑 통합하기
        [Fact]
        void CallExp_TranslatesIntoCallFuncExpWithoutTypeArgument()
        {
            var syntaxScript = new S.Script(
                new S.Script.FuncDeclElement(new S.FuncDecl(
                    false, IntTypeExp, "Func", Arr<string>(),
                    new S.FuncParamInfo(Arr(new S.TypeAndName(IntTypeExp, "x")), null),
                    new S.BlockStmt(new S.ReturnStmt(SimpleSId("x")))
                )),

                new S.Script.StmtElement(
                    new S.ExpStmt(new S.CallExp(SimpleSId("Func"), Arr<S.TypeExp>(), SimpleSInt(3)))
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null,
                Arr(
                    new FuncDecl.Normal(
                        new FuncDeclId(0), false, Arr<string>(), Arr("x"), new BlockStmt(new ReturnStmt(new LocalVarExp("x")))
                    )
                ),

                new ExpStmt(new ExpInfo(new CallFuncExp(new FuncDeclId(0), Arr<Type>(), null, Arr(new ExpInfo(SimpleInt(3), Type.Int))), Type.Int))
            );

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        void CallExp_TranslatesIntoCallValueExp()
        {
            throw new NotImplementedException();
        }

        // A0901
        [Fact]
        void CallExp_ChecksMultipleCandidates()
        {
            throw new NotImplementedException();
        }

        // A0902
        [Fact]
        void CallExp_ChecksCallableExpressionIsNotCallable()
        {
            throw new NotImplementedException();
        }

        // A0903
        [Fact]
        void CallExp_ChecksEnumConstructorArgumentCount()
        {
            throw new NotImplementedException();
        }

        // A0904
        [Fact]
        void CallExp_ChecksEnumConstructorArgumentType()
        {
            throw new NotImplementedException();
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

            throw new NotImplementedException();
        }

        [Fact]
        void LambdaExp_ChecksAssignToLocalVaraiableOutsideLambda()
        {
            throw new NotImplementedException();
        }

        [Fact]
        void IndexerExp_TranslatesTrivially()
        {
            // var s = [1, 2, 3, 4];
            // var i = s[3];

            throw new NotImplementedException();
        }
        
        [Fact]
        void IndexerExp_ChecksInstanceIsList() // TODO: Indexable로 확장
        {
            throw new NotImplementedException();
        }

        [Fact]
        void IndexerExp_ChecksIndexIsInt() // TODO: Indexable인자 타입에 따라 달라짐
        {
            throw new NotImplementedException();
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
            
            throw new NotImplementedException();
        }

        [Fact]
        void MemberCallExp_TranslatesIntoNewEnumExp() // 3
        {   
            throw new NotImplementedException();
        }

        [Fact]
        void MemberCallExp_TranslatesIntoCallValueExp() // 4, 5
        {
            
            throw new NotImplementedException();
        }

        [Fact]
        void MemberCallExp_ChecksCallableExpressionIsNotCallable() // 4, 5
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberCallExp_EnumConstructorArgumentCount() // 3
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberCallExp_EnumConstructorArgumentType() // 3
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberCallExp_ChecksMultipleCandidates() // 1, 2, 4, 5
        {
            throw new NotImplementedException();
        }

        // MemberExp
        // 1. E.Second  // enum constructor without parameter
        // 2. e.X       // enum field (EnumMemberExp)        
        // 3. C.x       // static 
        // 4. c.x       // instance  (class, struct, interface)
        [Fact]
        void MemberExp_TranslatesIntoNewEnumExp() // 1.
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberExp_TranslatesIntoEnumMemberExp() // 2
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberExp_TranslatesIntoStaticMemberExp() // 3
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberExp_TranslatesIntoStructMemberExp() // 4
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberExp_TranslatesIntoClassMemberExp() // 4
        {
            throw new NotImplementedException();
        }

        [Fact]
        void MemberExp_ChecksMemberNotFound() // TODO: enum, class, struct, interface 각각의 경우에 해야 하지 않는가
        {
            throw new NotImplementedException();
        }

        //case S.MemberExp memberExp: return AnalyzeMemberExp(memberExp, context, out outExp, out outTypeValue);

        [Fact]
        void ListExp_TranslatesTrivially() // TODO: 타입이 적힌 빈 리스트도 포함, <int>[]
        {
            throw new NotImplementedException();
        }

        [Fact]
        void ListExp_UsesHintTypeToInferElementType() // List<int> s = []; 
        {
            throw new NotImplementedException();
        }

        [Fact]
        void ListExp_ChecksCantInferEmptyElementType() // var x = []; // ???
        {
            throw new NotImplementedException();
        }

        [Fact]
        void ListExp_ChecksCantInferElementType() // var = ["string", 1, false];
        {
            throw new NotImplementedException();
        }
    }
}
