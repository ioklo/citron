using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Command_Statement
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
    @hi
}
";
        var expected = @"hi";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Interpolated()
    {
        var input = @"void Main()
{
    int i = 177;
    string s = ""world"";
    bool b = false;

    @abc$i abc${s}def $b.84
}
";
        var expected = @"abc177 abcworlddef false.84";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Block()
    {
        var input = @"void Main()
{
    // plain, ignore blank lines, trailing blanks
    @{

        <- no ignore 8 blanks
        
        hello world

    }

    // with other statements
    if (true)
    @{
        good
    }
}
";
        var expected = @"         <- no ignore 8 blanks        hello world        good";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
