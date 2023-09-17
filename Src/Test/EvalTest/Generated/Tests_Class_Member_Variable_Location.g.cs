using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Class_Member_Variable_Location
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"class X
{
    public int x;
    public X(int x) { this.x = x; }
}

void Main()
{
    X x = new X(2);
    @${x.x}
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Static()
    {
        var input = @"// 11
class C
{
    public static int x = 0;
    public void F()
    {
	    @$x
	}
}

void Main()
{
    C.x = 1;
    @${C.x}

	var c = new C();
	c.F();
}
";
        var expected = @"11";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
