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
            // deserializer.RegisterTagMapping("!CharExp", typeof(CharExp));
            deserializer.RegisterTagMapping("!IntegerExp", typeof(IntegerExp));
            deserializer.RegisterTagMapping("!StringExp", typeof(StringExp));
            deserializer.RegisterTagMapping("!IDExp", typeof(IDExp));
            deserializer.RegisterTagMapping("!IDWithTypeArgs", typeof(IDWithTypeArgs));
            deserializer.RegisterTagMapping("!MemberExp", typeof(MemberExp));
            deserializer.RegisterTagMapping("!CallExp", typeof(CallExp));
            deserializer.RegisterTagMapping("!NewExp", typeof(NewExp));
            deserializer.RegisterTagMapping("!UnaryExp", typeof(UnaryExp));
            deserializer.RegisterTagMapping("!UnaryExpKind", typeof(UnaryExpKind));
            deserializer.RegisterTagMapping("!BinaryExp", typeof(BinaryExp));
            deserializer.RegisterTagMapping("!BinaryExpKind", typeof(BinaryExpKind));
        }

        public override bool Test(ParseExpTestCase testCase)
        {
            return false;

            //var lexer = new Lexer(testCase.Text);
            //var parser = new EntireParser();

            //IExpComponent exp;
            //if (!parser.ParseExp(lexer, out exp))
            //    return false;

            //var serializer = new Serializer(SerializationOptions.Roundtrip);
            //var stringWriter1 = new StringWriter();
            //serializer.Serialize(stringWriter1, exp);

            //var stringWriter2 = new StringWriter();
            //serializer.Serialize(stringWriter2, testCase.Result);

            //if (stringWriter1.ToString() != stringWriter2.ToString())
            //    return false;

            //return true;
        }
    }
}
