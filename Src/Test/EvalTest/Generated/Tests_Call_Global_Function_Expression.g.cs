using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Call_Global_Function_Expression
{

    [Fact]
    public Task Test_General()
    {
        var input = @"void F(int i, string s, bool b)
{    
    @$i $s $b
}

void Main()
{
	F(1, ""2"", false);
}
";
        var expected = @"1 2 false";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Recursive()
    {
        var input = @"void F(int i, int end)
{    
    if (end <= i) return;

    @$i
    F(i + 1, end);
}

void Main()
{
	F(3, 6);
}
";
        var expected = @"345";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Generator()
    {
        var input = @"seq int Func()
{
    yield 1;
    yield 2;
    yield 3;
}

void Main()
{
    foreach(var i in Func())
        @$i
}
";
        var expected = @"123";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
