using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Return_Statement
{

    [Fact]
    public Task Test_ControlFlow()
    {
        var input = @"void F()
{    
    @F

    return;

    @wrong
}

void Main()
{
    F();
}

";
        var expected = @"F";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_ReturnValue()
    {
        var input = @"int F(int i)
{    
    @F

    return i * 2;

    @wrong
}

void Main()
{
    @${F(3)}
}
";
        var expected = @"F6";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_LambdaReturn()
    {
        var input = @"void Main()
{
    var f = () => {
        return 3;
    };

    @${f()}
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_SeqReturn()
    {
        var input = @"seq int F()
{
    for(int i = 0; i < 10; i++)
    {
        yield i;
        if (i == 4) return;
    }
}

void Main()
{
    foreach(var e in F())
    {
        @$e
    }
}
";
        var expected = @"01234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
