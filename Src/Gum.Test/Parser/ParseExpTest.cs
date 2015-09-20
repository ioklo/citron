using Gum.Lang.AbstractSyntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Gum.Test.Parser
{
    public class ParseExpTest
    {
        public class TestCase
        {
            public string Text { get; private set; }
            public IExpComponent Result { get; private set; }
        }        

        public static void Test()
        {
            var memoryStream = new MemoryStream(TestResource.ParseExpTestData);
            var reader = new StreamReader(memoryStream, Encoding.UTF8);

            var deserializer = new Deserializer();            
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
            var testCases = deserializer.Deserialize<List<TestCase>>(reader);

            foreach( var testCase in testCases )
            {

            }
        }
    }
}
