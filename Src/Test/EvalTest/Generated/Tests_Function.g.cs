using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Function
{

    [Fact]
    public Task Test_Out()
    {
        var input = @"void F(out int* i)
{
    *i = 2;
}

int j = 3;
F(out &j); // out을 반드시 써줘야 합니다

@$j
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
