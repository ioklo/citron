using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Class_Is_Interface_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"interface I { }

class B { }
class C : I { }

void Main()
{
	var b = new C();
	var t = b is I;

	@$t
}
";
        var expected = @"true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_NotRelated()
    {
        var input = @"interface I { }
class C { }

void Main()
{
	var c = new C();
	var t = c is I;
	@$t
}
";
        var expected = @"false";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
