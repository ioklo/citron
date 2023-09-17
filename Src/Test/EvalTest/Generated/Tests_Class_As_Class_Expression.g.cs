using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Class_As_Class_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"class B { }
class C : B { public int x; }

void Main()
{
	var b = new C(2);
	
	var c = b is C; // c는 nullable C 타입
	if (c != null)	
		@${c.x}
}
";
        var expected = @"2";

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
	var d = c as D;
	
	if (d == null)
		@ok
}
";
        var expected = @"ok";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
