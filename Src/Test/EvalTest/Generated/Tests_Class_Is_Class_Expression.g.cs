using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Class_Is_Class_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"class B { }
class C : B { }

void Main()
{
	var b = new C();
	
	var t0 = b is B;
	var t1 = b is C;

	@$t0 $t1
}
";
        var expected = @"true true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_NotRelated()
    {
        var input = @"class C { }
class D { }

void Main()
{
	var c = new C();
	var t = c is D;
	@$t
}
";
        var expected = @"false";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
