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
            var (script, errors) = TranslateWithErrors(syntaxScript, true);

            return script;
        }

        (Script?, List<AnalyzeError>) TranslateWithErrors(S.Script syntaxScript, bool raiseAssertionFail = false)
        {
            var testErrorCollector = new TestErrorCollector(raiseAssertionFail);
            var translator = new Translator();

            var script = translator.Translate("Test", syntaxScript, Array.Empty<IModuleInfo>(), testErrorCollector);

            var analyzeErrors = testErrorCollector.Errors.OfType<AnalyzeError>().ToList();

            return (script, analyzeErrors);
        }
        

        Script MakeScript(IEnumerable<Type>? types, IEnumerable<Func>? funcs, IEnumerable<SeqFunc>? seqFuncs, IEnumerable<Stmt> topLevelStmts)
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

        S.TypeExp intTypeExp = new S.IdTypeExp("int");
        S.TypeExp voidTypeExp = new S.IdTypeExp("void");


        // Trivial Cases
        [Fact]
        public void CommandStmt_TranslatesTrivially()
        {   
            var syntaxCmdStmt = new S.CommandStmt(
                new S.StringExp(
                    new S.TextStringExpElement("Hello "),
                    new S.ExpStringExpElement(new S.StringExp(new S.TextStringExpElement("World")))));
            var syntaxScript = new S.Script(new S.Script.StmtElement(syntaxCmdStmt));

            var script = Translate(syntaxScript);
            
            var expectedStmt = new CommandStmt(
                new StringExp(
                    new TextStringExpElement("Hello "),
                    new ExpStringExpElement(new ExpInfo(new StringExp(new TextStringExpElement("World")), TypeId.String))));

            var expected = new Script(Array.Empty<Type>(), Array.Empty<Func>(), Array.Empty<SeqFunc>(), new[] { expectedStmt });

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        T[] MakeArray<T>(params T[] values)
        {
            return values;
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoPrivateGlobalVarDecl()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(new S.VarDeclStmt(new S.VarDecl(intTypeExp, new S.VarDeclElement("x", new S.IntLiteralExp(1))))));
            var script = Translate(syntaxScript);

            var expected = MakeScript(null, null, null, MakeArray(
                new PrivateGlobalVarDeclStmt(MakeArray(new PrivateGlobalVarDeclStmt.Element("x", TypeId.Int, new IntLiteralExp(1))))
            ));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void VarDeclStmt_TranslatesIntoLocalVarDeclInTopLevelScope()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(
                new S.BlockStmt(
                    new S.VarDeclStmt(new S.VarDecl(intTypeExp, new S.VarDeclElement("x", new S.IntLiteralExp(1))))
                )
            ));
            var script = Translate(syntaxScript);

            var expected = MakeScript(null, null, null, MakeArray(
                new BlockStmt(
                    new LocalVarDeclStmt(new LocalVarDecl(MakeArray(new LocalVarDecl.Element("x", TypeId.Int, new IntLiteralExp(1)))))
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
                        new S.VarDeclStmt(new S.VarDecl(intTypeExp, new S.VarDeclElement("x", new S.IntLiteralExp(1))))
                    )
                ))
            );

            var script = Translate(syntaxScript);

            var func = new Func(new FuncId(0), false, Array.Empty<string>(), Array.Empty<string>(), new BlockStmt(

                new LocalVarDeclStmt(new LocalVarDecl(MakeArray(new LocalVarDecl.Element("x", TypeId.Int, new IntLiteralExp(1)))))

            ));

            var expected = MakeScript(null, MakeArray(func), null, MakeArray<Stmt>());

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void IfStmt_TranslatesTrivially()
        {
            var syntaxScript = new S.Script(new S.Script.StmtElement(

                new S.IfStmt(new S.BoolLiteralExp(false), null, S.BlankStmt.Instance, S.BlankStmt.Instance)
                
            ));

            var script = Translate(syntaxScript);

            var expected = MakeScript(null, null, null, MakeArray(

                new IfStmt(new BoolLiteralExp(false), BlankStmt.Instance, BlankStmt.Instance)

            ));

            Assert.Equal(expected, script, IR0EqualityComparer.Instance);
        }

        [Fact]
        public void IfStmt_ReportsErrorWhenCondTypeIsNotBool()
        {
            var cond = new S.IntLiteralExp(3);

            var syntaxScript = new S.Script(new S.Script.StmtElement(

                new S.IfStmt(cond, null, S.BlankStmt.Instance, S.BlankStmt.Instance)

            ));

            var (_, errors) = TranslateWithErrors(syntaxScript);

            Assert.True(errors.Exists(error => error.Code == A1004_IfStmt_ConditionShouldBeBool && error.Node == (S.ISyntaxNode)cond));
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

        

        // StringExp
        [Fact]
        public void StringExp_ReportsErrorWhenExpStringExpElementIsNotStringType()
        {   
            var syntaxCmdStmt = new S.ExpStmt(
                new S.StringExp(
                    new S.ExpStringExpElement(new S.IntLiteralExp(3))));

            var syntaxScript = new S.Script(new S.Script.StmtElement(syntaxCmdStmt));

            var (_, errors)= TranslateWithErrors(syntaxScript);

            // Assert.True(errors.Exists(error => error.Code == A1004_IfStmt_ConditionShouldBeBool && error.Node == (S.ISyntaxNode)cond));

            throw new PrerequisiteRequiredException("AnalyzeStringExpElement");
        }

    }
}
