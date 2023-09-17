using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Call_Class_Member_Function_Expression
{

    [Fact]
    public Task Test_Instance()
    {
        var input = @"class X
{
    int x;
    
	public void F(int i)
    {
        @$x $i
    }
}

void Main()
{
	X x = new X(2);
    x.F(4);
}
";
        var expected = @"2 4";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Static()
    {
        var input = @"class X
{
	public static void Print(int a)
	{
		@X: $a
	}
}

void Main()
{
	X.Print(3);
}
";
        var expected = @"X: 3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
