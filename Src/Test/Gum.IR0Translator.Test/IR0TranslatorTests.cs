using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Gum.CompileTime;

using S = Gum.Syntax;
using Gum.Infra;

namespace Gum.IR0
{
    public class IR0TranslatorTests
    {
        class TestErrorCollector : IErrorCollector
        {
            List<(object, string)> messages = new List<(object, string)>();

            public bool HasError => messages.Count != 0;

            public void Add(object obj, string msg)
            {
                messages.Add((obj, msg));
            }
        }

        // Trivial Cases
        [Fact]
        public void CommandStmt_TranslatesTrivially()
        {   
            var syntaxCmdStmt = new S.CommandStmt(
                new S.StringExp(
                    new S.TextStringExpElement("Hello "),
                    new S.ExpStringExpElement(new S.StringExp(new S.TextStringExpElement("World")))));
            var syntaxScript = new S.Script(new S.Script.StmtElement(syntaxCmdStmt));
            var testErrorCollector = new TestErrorCollector();
            var translator = new IR0Translator();
            var ir0Script = translator.Translate("Test", syntaxScript, Array.Empty<IModuleInfo>(), testErrorCollector);

            var expectedStmt = new CommandStmt(
                new StringExp(
                    new TextStringExpElement("Hello "),
                    new ExpStringExpElement(new ExpInfo(new StringExp(new TextStringExpElement("World")), TypeId.String))));

            var expected = new Script(Array.Empty<Type>(), Array.Empty<Func>(), Array.Empty<SeqFunc>(), new[] { expectedStmt });

            Assert.Equal(expected, ir0Script, IR0EqualityComparer.Instance);
        }

        // StringExp
        [Fact]
        public void StringExp_ReportsErrorWhenExpStringExpElementIsNotStringType()
        {   
            var syntaxCmdStmt = new S.ExpStmt(
                new S.StringExp(
                    new S.ExpStringExpElement(new S.IntLiteralExp(3))));

            var syntaxScript = new S.Script(new S.Script.StmtElement(syntaxCmdStmt));
            var testErrorCollector = new TestErrorCollector();
            var translator = new IR0Translator();
            var ir0Script = translator.Translate("Test", syntaxScript, Array.Empty<IModuleInfo>(), testErrorCollector);

            Assert.Null(ir0Script);

            // TODO: Error 코드를 메세지에 추가해야 한다
            throw new NotImplementedException();
        }
        



    }
}
