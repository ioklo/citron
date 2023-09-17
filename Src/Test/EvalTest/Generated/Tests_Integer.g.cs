using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Integer
{

    [Fact]
    public Task Test_Literal()
    {
        var input = @"void Main() 
{ 
    int i = 1024;
    @$i
}
";
        var expected = @"1024";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_UnaryOperation()
    {
        var input = @"void Main()
{
    int i = -3;
    @$i ${-i}
    @ ${i++} ${i--} ${++i} ${--i}
}
";
        var expected = @"-3 3 -3 -2 -2 -3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_BinaryOperation()
    {
        var input = @"void Main()
{
    int i;
    
    i = -3; // assignment
    
    @$i ${i = 4} $i
    
    @ ${3 == 4} ${3 == 3} ${-3 == -3}
    
    @ ${-3 - 3 + 2} ${-3 - 3} ${2 + 4 * -7} ${3 - 3 / 2} ${2 + 7 % 3}
    
    @ ${2 < 4} ${4 < 2} ${2 <= 2} ${1 <= 2} ${3 <= 2} ${-10 > 20} ${20 > -10} ${-10 >= 20} ${20 >= -10} ${28 >= 28}
}
";
        var expected = @"-3 4 4 false true true -4 -6 -26 2 3 true false true true false false true false true true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
