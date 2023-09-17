using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_String_Expression
{

    [Fact]
    public Task Test_Literal()
    {
        var input = @"void Main()
{
    string x = ""hello\"""";
    @$x
}
";
        var expected = @"hello""";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_InterpolationVariable()
    {
        var input = @"void Main()
{
    int i = 3;
    string x = ""hello"";
    string y = ""$x.$i $$"";
    
    @$y
}
";
        var expected = @"hello.3 $";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_InterpolationWithBraces()
    {
        var input = @"void Main()
{
    int i = 2;
    string x = ""hell"";
    string y = ""${x + ""o""}.${i + 1}"";
    
    @${y}
}
";
        var expected = @"hello.3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
