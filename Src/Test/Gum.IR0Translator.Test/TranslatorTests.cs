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
        class TestErrorCollector : IErrorCollector
        {
            public List<IError> Errors { get; }
            public bool HasError => Errors.Count != 0;

            bool raiseAssertionFail;

            public TestErrorCollector(bool raiseAssertionFail)
            {
                Errors = new List<IError>();
                this.raiseAssertionFail = raiseAssertionFail;
            }

            public void Add(IError error)
            {
                Errors.Add(error);
                Assert.True(!raiseAssertionFail || false);
            }
        }

        Script? Translate(S.Script syntaxScript)
        {
            var testErrorCollector = new TestErrorCollector(false);
            var translator = new Translator();

            return translator.Translate("Test", syntaxScript, Array.Empty<IModuleInfo>(), testErrorCollector);
        }

        List<IError> TranslateWithErrors(S.Script syntaxScript, bool raiseAssertionFail = false)
        {
            var testErrorCollector = new TestErrorCollector(raiseAssertionFail);
            var translator = new Translator();

            var script = translator.Translate("Test", syntaxScript, Array.Empty<IModuleInfo>(), testErrorCollector);

            return testErrorCollector.Errors;
        }

        T[] MakeArray<T>(params T[] values)
        {
            return values;
        }

        Script SimpleScript(IEnumerable<Type>? types, IEnumerable<Func>? funcs, IEnumerable<SeqFunc>? seqFuncs, IEnumerable<Stmt> topLevelStmts)
        {
            // TODO: Validator
            int i = 0;
            foreach(var func in funcs ?? Array.Empty<Func>())
            {
                Assert.Equal(i, func.Id.Value);
                i++;
            }

            return new Script(types ?? Array.Empty<Type>(), funcs ?? Array.Empty<Func>(), seqFuncs ?? Array.Empty<SeqFunc>(), topLevelStmts);
        }

        void VerifyError(IEnumerable<IError> errors, AnalyzeErrorCode code, S.ISyntaxNode node)
        {
            var result = errors.OfType<AnalyzeError>()
                .Any(error => error.Code == code && error.Node == node);

            Assert.True(result);
        }

        S.VarDecl SimpleSVarDecl(S.TypeExp typeExp, string name, S.Exp? initExp = null)
        {
            return new S.VarDecl(typeExp, MakeArray(new S.VarDeclElement(name, initExp)));
        }

        S.VarDeclStmt SimpleSVarDeclStmt(S.TypeExp typeExp, string name, S.Exp? initExp = null)
        {
            return new S.VarDeclStmt(SimpleSVarDecl(typeExp, name, initExp));
        }

        S.Script SimpleSScript(params S.Stmt[] stmts)
        {
            return new S.Script(stmts.Select(stmt => new S.Script.StmtElement(stmt)));
        }

        S.IntLiteralExp SimpleSInt(int v) => new S.IntLiteralExp(v);

        S.StringExp SimpleSString(string s) => new S.StringExp(new S.TextStringExpElement(s));

        LocalVarDeclStmt SimpleLocalVarDeclStmt(TypeId typeId, string name, Exp? initExp = null)
            => new LocalVarDeclStmt(SimpleLocalVarDecl(typeId, name, initExp));

        LocalVarDecl SimpleLocalVarDecl(TypeId typeId, string name, Exp? initExp = null) 
            => new LocalVarDecl(MakeArray(new LocalVarDecl.Element(name, typeId, initExp)));

        IntLiteralExp SimpleInt(int v) => new IntLiteralExp(v);
        StringExp SimpleString(string v) => new StringExp(new TextStringExpElement(v));
        
        S.TypeExp intTypeExp = new S.IdTypeExp("int");
        S.TypeExp voidTypeExp = new S.IdTypeExp("void");
        S.TypeExp stringTypeExp = new S.IdTypeExp("string");        


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
                    new ExpStringExpElement(new ExpInfo(new StringExp(new TextStringExpElement("World")), TypeId.String))));

            var expected = new Script(Array.Empty<Type>(), Array.Empty<Func>(), Array.Empty<SeqFunc>(), new[] { expectedStmt });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }
        
        [Fact]
        public void VarDeclStmt_TranslatesIntoPrivateGlobalVarDecl()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(SimpleSVarDeclStmt(intTypeExp, "x", SimpleSInt(1))));
            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, MakeArray(
                new PrivateGlobalVarDeclStmt(MakeArray(new PrivateGlobalVarDeclStmt.Element("x", TypeId.Int, SimpleInt(1))))
            ));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInTopLevelScope()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(intTypeExp, "x", SimpleSInt(1))
                )
            ));
            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, MakeArray(
                new BlockStmt(
                    new LocalVarDeclStmt(new LocalVarDecl(MakeArray(new LocalVarDecl.Element("x", TypeId.Int, SimpleInt(1)))))
                )
            ));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInFuncScope()
        {
            var syntaxScript = new S.Script(
                new S.Script.FuncDeclElement(new S.FuncDecl(false, voidTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null),
                    new S.BlockStmt(
                        SimpleSVarDeclStmt(intTypeExp, "x", SimpleSInt(1))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var func = new Func(new FuncId(0), false, Array.Empty<string>(), Array.Empty<string>(), new BlockStmt(

                new LocalVarDeclStmt(new LocalVarDecl(MakeArray(new LocalVarDecl.Element("x", TypeId.Int, SimpleInt(1)))))

            ));

            var expected = SimpleScript(null, MakeArray(func), null, MakeArray<Stmt>());

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void IfStmt_TranslatesTrivially()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(

                new S.IfStmt(new S.BoolLiteralExp(false), null, S.BlankStmt.Instance, S.BlankStmt.Instance)
                
            ));

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, MakeArray(

                new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, BlankStmt.Instance)

            ));

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
            throw new PrerequisiteRequiredException("Class");
        }

        [Fact]
        public void IfStmt_TranslatesIntoIfTestEnumStmt()
        {
            throw new PrerequisiteRequiredException("Enum");
        }

        [Fact]
        public void ForStmt_TranslatesInitializerTrivially()
        {
            var syntaxScript = new S.Script(
                
                new S.Script.StmtElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SimpleSVarDecl(intTypeExp, "x")),
                    null, null, S.BlankStmt.Instance
                )),

                new S.Script.StmtElement(SimpleSVarDeclStmt(stringTypeExp, "x")),

                new S.Script.StmtElement(new S.ForStmt(
                    new S.ExpForStmtInitializer(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.IdentifierExp("x"), SimpleSString("Hello"))),
                    null, null, S.BlankStmt.Instance
                ))
            );            

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, new Stmt[] { 

                new ForStmt(
                    new VarDeclForStmtInitializer(SimpleLocalVarDecl(TypeId.Int, "x")),
                    null, null, BlankStmt.Instance
                ),

                new PrivateGlobalVarDeclStmt(new [] { new PrivateGlobalVarDeclStmt.Element("x", TypeId.String, null) }),

                new ForStmt(
                    new ExpForStmtInitializer(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleString("Hello")), TypeId.String)),
                    null, null, BlankStmt.Instance
                )
            });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        // 
        [Fact]
        public void ForStmt_ChecksVarDeclInitializerScope() 
        {
            var syntaxScript = new S.Script(

                new S.Script.StmtElement(SimpleSVarDeclStmt(stringTypeExp, "x")),

                new S.Script.StmtElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SimpleSVarDecl(intTypeExp, "x")), // x의 범위는 ForStmt내부에서
                    new S.BinaryOpExp(S.BinaryOpKind.Equal, new S.IdentifierExp("x"), SimpleSInt(3)),
                    null, S.BlankStmt.Instance
                )),

                new S.Script.StmtElement(new S.ExpStmt(new S.BinaryOpExp(S.BinaryOpKind.Assign, new S.IdentifierExp("x"), SimpleSString("Hello"))))
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, new Stmt[] {

                new PrivateGlobalVarDeclStmt(new [] { new PrivateGlobalVarDeclStmt.Element("x", TypeId.String, null) }),

                new ForStmt(
                    new VarDeclForStmtInitializer(SimpleLocalVarDecl(TypeId.Int, "x")),

                    // cond
                    new CallInternalBinaryOperatorExp(
                        InternalBinaryOperator.Equal_Int_Int_Bool,
                        new ExpInfo(new LocalVarExp("x"), TypeId.Int),
                        new ExpInfo(SimpleInt(3), TypeId.Int)
                    ),
                    null, BlankStmt.Instance
                ),

                new ExpStmt(new ExpInfo(new AssignExp(new PrivateGlobalVarExp("x"), SimpleString("Hello")), TypeId.String)),
            });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }


        [Fact]
        public void ForStmt_ChecksConditionShouldBeBool()
        {
            S.Exp cond;

            var syntaxScript = new S.Script(
                new S.Script.StmtElement(new S.ForStmt(
                    new S.VarDeclForStmtInitializer(SimpleSVarDecl(intTypeExp, "x")),
                    cond = SimpleSInt(3),
                    null, S.BlankStmt.Instance
                ))
            );

            var errors = TranslateWithErrors(syntaxScript);
            VerifyError(errors, A1101_ForStmt_ConditionShouldBeBool, cond);
        }


        [Fact]
        public void ForStmt_ChecksExpInitializerShouldBeAssignOrCall()
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
        public void ForStmt_ChecksContinueExpShouldBeAssignOrCall()
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
                new S.ForeachStmt(intTypeExp, "x", new S.ListExp(intTypeExp), S.ContinueStmt.Instance)
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, new Stmt[] {
                new ForStmt(null, null, null, ContinueStmt.Instance),
                new ForeachStmt(TypeId.Int, "x", new ExpInfo(new ListExp(TypeId.Int, Array.Empty<Exp>()), TypeId.List), ContinueStmt.Instance)
            });

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
                    new S.ForeachStmt(intTypeExp, "x", new S.ListExp(intTypeExp), S.BreakStmt.Instance)
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, new Stmt[] {
                new ForStmt(null, null, null, BreakStmt.Instance),
                new ForeachStmt(TypeId.Int, "x", new ExpInfo(new ListExp(TypeId.Int, Array.Empty<Exp>()), TypeId.List), BreakStmt.Instance)
            });

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
            var expected = SimpleScript(null, null, null, new Stmt[]
            {
                new ReturnStmt(SimpleInt(2))
            });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ReturnStmt_TranslatesReturnStmtInSeqFuncTrivially()
        {
            var syntaxScript = new S.Script(new S.Script.FuncDeclElement(new S.FuncDecl(
                true, intTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null),
                new S.BlockStmt(
                    new S.ReturnStmt(null)
                )
            )));

            var script = Translate(syntaxScript);

            var seqFunc = new SeqFunc(new SeqFuncId(0), TypeId.Int, false, Array.Empty<string>(), Array.Empty<string>(), new BlockStmt(new ReturnStmt(null)));

            var expected = SimpleScript(null, null, new[] { seqFunc }, Array.Empty<Stmt>());

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void ReturnStmt_ChecksMatchFuncRetTypeAndRetValue()
        {
            S.Exp retValue;

            var funcDecl = new S.FuncDecl(false, intTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null), new S.BlockStmt(
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

            var funcDecl = new S.FuncDecl(false, intTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null), new S.BlockStmt(
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

            var funcDecl = new S.FuncDecl(true, intTypeExp, "Func", Array.Empty<string>(), new S.FuncParamInfo(Array.Empty<S.TypeAndName>(), null), new S.BlockStmt(
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
        public void BlockStmt_TranslatesVarDeclStmtWithinBlockStmtOfTopLevelStmtIntoLocalVarDeclStmt()
        {
            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(stringTypeExp, "x", SimpleSString("Hello"))
                )
            );

            var script = Translate(syntaxScript);

            var expected = SimpleScript(null, null, null, MakeArray(
                new BlockStmt(
                    SimpleLocalVarDeclStmt(TypeId.String, "x", SimpleString("Hello")) // not PrivateGlobalVarDecl
                )
            ));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void BlockStmt_ChecksIsolatingOverridenTypesOfVariables()
        {
            throw new PrerequisiteRequiredException("IfTestClassStmt, IfTestEnumStmt");
        }

        [Fact]
        public void BlockStmt_ChecksLocalVariableScope()
        {
            S.Exp exp;

            var syntaxScript = SimpleSScript(
                new S.BlockStmt(
                    SimpleSVarDeclStmt(stringTypeExp, "x", SimpleSString("Hello"))
                ),

                new S.CommandStmt(new S.StringExp(new S.ExpStringExpElement(exp = new S.IdentifierExp("x"))))
            );

            var errors = TranslateWithErrors(syntaxScript);

            VerifyError(errors, A0501_IdExp_VariableNotFound, exp);
        }        

        // StringExp
        [Fact]
        public void StringExp_ChecksStringExpElementIsNotStringType()
        {   
            var syntaxCmdStmt = new S.ExpStmt(
                new S.StringExp(
                    new S.ExpStringExpElement(SimpleSInt(3))));

            var syntaxScript = new S.Script(new S.Script.StmtElement(syntaxCmdStmt));

            var errors = TranslateWithErrors(syntaxScript);

            // Assert.True(errors.Exists(error => error.Code == A1004_IfStmt_ConditionShouldBeBool && error.Node == (S.ISyntaxNode)cond));

            throw new PrerequisiteRequiredException("AnalyzeStringExpElement");
        }

    }
}
