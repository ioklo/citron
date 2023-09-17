using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Call_Lambda_Expression
{

    [Fact]
    public Task Test_General()
    {
        var input = @"void Main()
{
	var f = (int i, string s, bool b) => { 
	    @$i $s $b
	};
	
	f(1, ""3"", true);
}
";
        var expected = @"1 3 true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_CallInstanceMember()
    {
        var input = @"class C
{
    func<int, void> F;

	public void InvokeF(int i)
	{
		F(i);
	}
}

void Main()
{
    C c = new C(i => {
        @$i
    });

    c.InvokeF(2);
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_CallStaticMember()
    {
        var input = @"class C
{
    public static func<int, void> F;
}

void Main()
{
    C.F = i => {
        @$i
    };


    C.F(2);
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
