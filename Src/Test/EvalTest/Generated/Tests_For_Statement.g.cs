using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_For_Statement
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
    for(int i = 0; i < 5; i++)
        @$i
}
";
        var expected = @"01234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Initializer()
    {
        var input = @"void F()
{
    @hi
}

void Main()
{
    int i = 2;
    for(F(); i < 5; i++)
        @$i
}
";
        var expected = @"hi234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_EmptyInitializer()
    {
        var input = @"void Main()
{
    int i = 0;

    for(; i < 5; i++)
        @$i
}

";
        var expected = @"01234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Scope()
    {
        var input = @"void Main()
{
    int i = 3, j = 4;

    for(int i = 0; i < 5; i++)
    {
        int j = i * 2;
        @$i$j
    }

    @$i$j
}
";
        var expected = @"001224364834";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_EmptyCond()
    {
        var input = @"void Main()
{
    for(int i = 0; ; i++)
    {
        if (5 <= i) break;
        @$i
    }
}
";
        var expected = @"01234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_EmptyContinueExp()
    {
        var input = @"void Main()
{
    for(int i = 0; i < 5;)
    {
        @$i
        i++;
    }
}
";
        var expected = @"01234 ";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_EmptyAll()
    {
        var input = @"void Main()
{
    int i = 0;
    for(;;)
    {
        if (5 <= i) break;
        @$i
        i++;
    }
}
";
        var expected = @"01234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
