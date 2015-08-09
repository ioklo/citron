using Gum.Test.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Lang.AbstractSyntax;
using Gum.Test.TypeInst;

namespace Gum.Test
{
    class Program
    {
        class FileUnitTypeAnnotator
        {
            Environment env;
            StringMap<ITypeInst> varTypeInsts = new StringMap<ITypeInst>();
            Stack<ITypeInst> retTypeInsts = new Stack<ITypeInst>();

            public FileUnitTypeAnnotator(GumMetadata metadata, IEnumerable<IMetadata> externMetadata)
            {
                env = new Environment(metadata, externMetadata);
            }

            public void Visit(FileUnit fileUnit)
            {
                foreach (var comp in fileUnit.Comps)
                    Visit(comp as dynamic);
            }
            
            public void Visit(UsingDirective usingDirective)
            {
                // using directives
                env.AddUsing(usingDirective.Names);
            }
            
            public void Visit(NamespaceDecl namespaceDecl)
            {
                env.PushNamespace(namespaceDecl.Names);

                foreach(var comp in namespaceDecl.Comps)
                    Visit(comp as dynamic);

                env.PopNamespace();
            }

            public void Visit(VarDecl varDecl)
            {
                var typeInst = env.GetTypeInst(varDecl.Type);

                foreach (var nameAndExp in varDecl.NameAndExps)
                {
                    varTypeInsts.Add(nameAndExp.VarName, typeInst);

                    if(nameAndExp.Exp != null)
                        Visit(nameAndExp.Exp as dynamic);
                }
            }
            
            public void Visit(FuncDecl funcDecl)
            {
                // Type Argument 넣기
                env.PushFuncTypeVars(funcDecl.TypeVars);

                varTypeInsts.Push();

                foreach(var param in funcDecl.Parameters)
                {
                    var paramType = env.GetTypeInst(param.Type);
                    varTypeInsts.Add(param.VarName, paramType);
                }
                
                // 리턴 타입 비교를 위해 넣어줍니다
                var retTypeInst = env.GetTypeInst(funcDecl.ReturnType);
                retTypeInsts.Push(retTypeInst);

                Visit(funcDecl.Body as dynamic);

                varTypeInsts.Pop();
                env.PopTypeArgs();
            }

            public void Visit(BlockStmt blockStmt)
            {
                foreach(var stmt in blockStmt.Stmts)
                {
                    varTypeInsts.Push();
                    Visit(stmt as dynamic);
                    varTypeInsts.Pop();
                }
            }

            public void Visit(BreakStmt breakStmt)
            {
            }

            public void Visit(ContineStmt contineStmt)
            {
            }

            public void Visit(DoWhileStmt doWhileStmt)
            {
                varTypeInsts.Push();

                Visit(doWhileStmt.Body as dynamic);
                CheckBool(doWhileStmt.CondExp);

                varTypeInsts.Pop();
            }

            public void Visit(ExpStmt expStmt)
            {
                Visit(expStmt.Exp as dynamic);
            }

            public void Visit(ForStmt forStmt)
            {
                varTypeInsts.Push();

                Visit(forStmt.Initializer as dynamic);

                CheckBool(forStmt.CondExp);

                Visit(forStmt.LoopExp as dynamic);

                Visit(forStmt.Body as dynamic);
            }

            private void CheckBool(IExpComponent expComponent)
            {
                throw new NotImplementedException();
            }

            public void Visit(IfStmt ifStmt)
            {
                varTypeInsts.Push();
                CheckBool(ifStmt.CondExp);
                Visit(ifStmt.ThenStmt as dynamic);
                Visit(ifStmt.ElseStmt as dynamic);
                varTypeInsts.Pop();
            }

            public void Visit(ReturnStmt returnStmt)
            {
                var returnTypeInst = Visit(returnStmt.ReturnExp);

                Check(returnTypeInst, retTypeInsts.Peek());
            }

            public void Visit(WhileStmt whileStmt)
            {
                varTypeInsts.Push();
                CheckBool(whileStmt.CondExp);
                Visit(whileStmt.Body as dynamic);
                varTypeInsts.Pop();
            }

            public void Check(ITypeInst left, ITypeInst right)
            {
                throw new Exception();
            }

            public ITypeInst Visit(AssignExp assignExp)
            {
                var leftType = Visit(assignExp.Left as dynamic);
                var rightType = Visit(assignExp.Right as dynamic);

                // TypeCheck
                Check(leftType, rightType);

                return leftType;
            }

            public ITypeInst Visit(BinaryExp binaryExp)
            {
                var type1 = Visit(binaryExp.Operand1 as dynamic);
                var type2 = Visit(binaryExp.Operand2 as dynamic);

                // 가능한 binary operation들을 검색해서 returnType을 알아와야 합니다
                FuncDef func = env.GetOperatorFunc(type1, type2, binaryExp.Operation);

                return func.ReturnType;
            }

            public ITypeInst Visit(BoolExp boolExp)
            {
                return env.GetBoolTypeInst();
            }

            public ITypeInst Visit(IExpComponent expComp)
            {
                return Visit(expComp as dynamic);
            }

            // (FuncExp)<TypeArgs>(Args)
            public ITypeInst Visit(CallExp callExp)
            {
                // 1.FuncExp를 사용해서 functionType을 가져옵니다. 여러 후보가 있을 수 있습니다
                IEnumerable<ITypeInst> argTypes = callExp.Args.Select(Visit);

                return GetFunctionTypes(callExp.FuncExp, callExp.TypeArgs, argTypes);                
            }
            
            // TODO: funcExp를 해석해서 typeArgs와 argTypes에 맞는 함수를 선택합니다
            private ITypeInst GetFunctionTypes(IExpComponent funcExp, IReadOnlyList<IDWithTypeArg> typeArgs, IEnumerable<ITypeInst> argTypes)
            {
                throw new NotImplementedException();
            }

            // (Exp).(ID)
            public ITypeInst Visit(MemberExp memberExp)
            {
                ITypeInst expType = Visit(memberExp.Exp);

                expType.GetMemberTypes(env, memberExp.MemberName);
                throw new NotImplementedException();
            }

            public ITypeInst Visit(IntegerExp integerExp)
            {
                return env.GetIntTypeInst();
            }

            public ITypeInst Visit(NewExp newExp)
            {
                return env.GetTypeInst(newExp.Type);
            }

            public ITypeInst Visit(StringExp stringExp)
            {
                return env.GetStringTypeInst();
            }

            public ITypeInst Visit(UnaryExp unaryExp)
            {
                var operandType = Visit(unaryExp.Operand as dynamic);

                // 가능한 binary operation들을 검색해서 returnType을 알아와야 합니다
                FuncDef func = env.GetOperatorFunc(operandType, unaryExp.Operation);
                return func.ReturnType;
            }

            // ID는 클래스, 구조체, 변수, 함수가 될 수 있다
            public ITypeInst Visit(IDExp idExp)
            {
                // A라는 클래스의 타입은 

                // Class_A : Class
                //   - A의 static 변수/함수                

                // A의 타입은 Class에서 상속받은 Class_A
                // B의 타입은 Class_B 이고, Class_A와는 상관없다

                // Namespace는 제거 하고 갑시다
                // Class, Struct, Function, Variable 모두 Namespace로부터 

                // 1. Class static variable을 참조하기 위한 용도
                // 3. Variable
                // 4. Function
                throw new NotImplementedException();
            }

            public void Visit(ClassDecl classDecl)
            {
            }

            public void Visit(StructDecl structDecl)
            {
            }
        }

        static IDWithTypeArg SingleType(string name)
        {
            return new IDWithTypeArg(name, Enumerable.Empty<IDWithTypeArg>());
        }

        static void Main(string[] args)
        {
            var fileUnit = new FileUnit( new IFileUnitComponent[]
            {
                // new UsingDirective( new string[] {"System"} ),
                new NamespaceDecl( new [] {"Gum"}, new INamespaceComponent[]
                {
                    new NamespaceDecl( new [] {"Test"}, new INamespaceComponent[] 
                    {
                        new VarDecl(
                            new IDWithTypeArg("ClassA", Enumerable.Empty<IDWithTypeArg>()),
                            new [] { new NameAndExp("a", null) }),
                        
                        // ClassB<T>
                        new ClassDecl(new []{"T"}, "ClassB", Enumerable.Empty<IDWithTypeArg>(), Enumerable.Empty<IMemberComponent>()),

                        // Function 
                        // ClassA FuncF<T, U>(ClassB<T> t, U u);
                        new FuncDecl(new []{"T", "U"}, SingleType("ClassA"), "FuncF", 
                            new [] 
                            { 
                                new FuncParam(Enumerable.Empty<FuncParamModifier>(), 
                                    new IDWithTypeArg("ClassB", new [] { SingleType("T") }), "t"),
                                new FuncParam(Enumerable.Empty<FuncParamModifier>(), SingleType("U"), "u"),
                            }, 
                            new BlockStmt(Enumerable.Empty<IStmtComponent>()))

                    }),

                    new ClassDecl(Enumerable.Empty<string>(), "ClassA", Enumerable.Empty<IDWithTypeArg>(), Enumerable.Empty<IMemberComponent>()),                     
                })
            });         

            
            var gumMetadata = CollectTypeNames.Collect(fileUnit);
            BuildMetadata.Build(gumMetadata, Enumerable.Empty<IMetadata>(), fileUnit);

            // fileUnit에 type을 넣습니다
            var annotator = new FileUnitTypeAnnotator(gumMetadata, Enumerable.Empty<IMetadata>());
            annotator.Visit(fileUnit as dynamic);

            //
            

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
