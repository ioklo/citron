using Gum.Lang.AbstractSyntax;
using Gum.Translator.Text2AST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Gum.Test.Text2AST
{
    class ParseExpTestCase : ITestCase
    {
        public string TestName { get; private set; }
        public string Text { get; private set; }
        public IExpComponent Result { get; private set; }
    }

    class ParseExpTest : TestBase<ParseExpTestCase>
    {
        public override void ConfigDeserializer(Deserializer deserializer)
        {
            deserializer.RegisterTagMapping("!BoolExp", typeof(BoolExp));
            deserializer.RegisterTagMapping("!CharExp", typeof(CharExp));
            deserializer.RegisterTagMapping("!IntegerExp", typeof(IntegerExp));
            deserializer.RegisterTagMapping("!StringExp", typeof(StringExp));
            deserializer.RegisterTagMapping("!IDExp", typeof(IDExp));
            deserializer.RegisterTagMapping("!IDWithTypeArgs", typeof(IDWithTypeArgs));
            deserializer.RegisterTagMapping("!MemberExp", typeof(MemberExp));
            deserializer.RegisterTagMapping("!CallExp", typeof(CallExp));
            deserializer.RegisterTagMapping("!NewExp", typeof(NewExp));
            deserializer.RegisterTagMapping("!UnaryExp", typeof(UnaryExp));
            deserializer.RegisterTagMapping("!UnaryExpKind", typeof(UnaryExpKind));
        }

        public override bool Test(ParseExpTestCase testCase)
        {
            var lexer = new Lexer(testCase.Text);
            var parser = new Parser();

            IExpComponent exp;
            if (!parser.ParseExp(lexer, out exp))
                return false;

            return true;
        }
    }
}
