using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Cast_Class_Expression
{

    [Fact]
    public Task Test_Upcast()
    {
        var input = @"class B { }
class C : B { }

void Main()
{
	var c = new C(); 
	B b = c; // CastClassExp(NewClassExp(C, []), B)
}

";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
