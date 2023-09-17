using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Call_Struct_Member_Function_Expression
{

    [Fact]
    public Task Test_Instance()
    {
        var input = @"struct S
{
	string s;
	void Print()
	{
		@$s
	}
}

void Main()
{
	var s = S(""hello"");
	s.Print();
}
";
        var expected = @"hello";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Static()
    {
        var input = @"struct S
{
	static void Print()
	{
		@hello
	}
}
";
        var expected = @"hello";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
