using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Blank_Statement
{

    [Fact]
    public Task Test_For()
    {
        var input = @"int Add(int i)
{
    @$i
    return i + 1;
}

void Main()
{
    for(int i = 0; i < 5; i = Add(i));
}
";
        var expected = @"01234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Foreach()
    {
        var input = @"seq string F()
{
    @hello
    yield ""1"";
    @world
    yield ""2"";
    @1
}

void Main()
{
    foreach(var i in F());
}

";
        var expected = @"helloworld1";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
