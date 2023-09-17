using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_New_Class_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"class C
{
	int x;
	int y;

	public void Print()
	{
		@$x $y
	}
}

void Main()
{
	var c = new C(2, 3);
	c.Print();
}
";
        var expected = @"2 3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Generics()
    {
        var input = @"class C<T>
{
	T a;
	public T GetA() { return a; }
}

void Main()
{
	var c = new C<string>(""hello"");
	var a = c.GetA();
	@$a
}
";
        var expected = @"hello";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
