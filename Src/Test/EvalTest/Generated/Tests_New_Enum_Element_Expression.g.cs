using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_New_Enum_Element_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"enum E { First, Second(int i) }
void Main()
{
	var e = E.First;
	e = E.Second(2);
}
";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Shorthand()
    {
        var input = @"enum E { First, Second(int i) }
void Main()
{
	E e = .First;
	e = .Second(2);
}
";
        var expected = @" ";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
