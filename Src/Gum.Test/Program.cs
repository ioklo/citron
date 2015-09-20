using Gum.Test.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Lang.AbstractSyntax;
using Gum.Test.Type;
using YamlDotNet.Serialization;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Gum.Test
{
    class Program
    {   
        static IDWithTypeArgs SingleType(string name)
        {
            return new IDWithTypeArgs(name, Enumerable.Empty<IDWithTypeArgs>());
        }

        static void Main(string[] args)
        {
            Gum.Test.Parser.ParseExpTest.Test();
            
            /*using (var reader = new StreamReader(@"..\..\Src\Gum.Test\TestData\ExpData.yaml"))
            {
                var deserializer = new Deserializer();
                deserializer.RegisterTypeConverter(new ExpComponentYamlTypeConverter());
                var testSet = deserializer.Deserialize<TestSet>(reader);

            }*/


            /*using (var reader = new StreamReader(@"..\..\Src\Gum.Test\TestData\ExpData.yaml"))
            {
                var deserializer = new Deserializer();
                deserializer.RegisterTypeConverter(new ExpComponentYamlTypeConverter());
                var testSet = deserializer.Deserialize<TestSet>(reader);               

            }*/


            var fileUnit = new FileUnit( new IFileUnitComponent[]
            {
                // new UsingDirective( new string[] {"System"} ),
                new NamespaceDecl( new [] {"Gum"}, new INamespaceComponent[]
                {
                    new NamespaceDecl( new [] {"Test"}, new INamespaceComponent[] 
                    {
                        new VarDecl(
                            new IDWithTypeArgs("ClassA", Enumerable.Empty<IDWithTypeArgs>()),
                            new [] { new NameAndExp("a", null) }),
                        
                        // ClassB<T>
                        new ClassDecl(new []{"T"}, "ClassB", Enumerable.Empty<IDWithTypeArgs>(), Enumerable.Empty<IMemberComponent>()),

                        // Function 
                        // ClassA FuncF<T, U>(ClassB<T> t, U u);
                        new FuncDecl(new []{"T", "U"}, SingleType("ClassA"), "FuncF", 
                            new [] 
                            { 
                                new FuncParam(Enumerable.Empty<FuncParamModifier>(), 
                                    new IDWithTypeArgs("ClassB", new [] { SingleType("T") }), "t"),
                                new FuncParam(Enumerable.Empty<FuncParamModifier>(), SingleType("U"), "u"),
                            }, 
                            new BlockStmt(Enumerable.Empty<IStmtComponent>()))

                    }),

                    new ClassDecl(Enumerable.Empty<string>(), "ClassA", Enumerable.Empty<IDWithTypeArgs>(), Enumerable.Empty<IMemberComponent>()),                     
                })
            });         

            
            var gumMetadata = CollectTypeNames.Collect(fileUnit);
            BuildMetadata.Build(gumMetadata, Enumerable.Empty<IMetadata>(), fileUnit);

            // fileUnit에 type을 넣습니다
            var expTypes = AST.TypeAnnotator.Annotate(fileUnit, gumMetadata, Enumerable.Empty<IMetadata>());
            
            

            /*Gum.App.Compiler.Parser parser = new Gum.App.Compiler.Parser();
            string code =
@"
int a = 7;
string g = ""aaaa"";

void WriteLine(int val);

int main()
{
    int b = a;

    WriteLine(b);
    return 0;
}
";
            Gum.Core.IL.Domain env = new Domain();
            var compiledPgm = Gum.App.Compiler.Compiler.Compile(env, code);

            // 컴파일러는 프로그램을 만들고 VM 인터프리터는 그것을 실행한다
            var vm = new Gum.App.VM.Interpreter();

            // WriteLine이란 함수는 외부함수..
            vm.AddExternFunc("WriteLine", WriteLine);

            // main 부분을 실행한다
            vm.Call(env, "main");          */

        }

        public static object WriteLine(object[] ps)
        {
            Console.WriteLine(ps[0]);
            return null;
        }


    }

}
