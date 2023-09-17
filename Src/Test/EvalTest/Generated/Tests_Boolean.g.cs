using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Boolean
{

    [Fact]
    public Task Test_UnaryOperation()
    {
        var input = @"void Main()
{
    bool t = true;
    bool f = false;

    @$t ${!t} $f ${!f}
}
";
        var expected = @"true false false true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_BinOp()
    {
        var input = @"void Main()
{
    bool b;
    
    b = false; // assignment
    
    @$b ${b = true} $b
    
    @ ${false == false} ${false == true} ${true == false} ${true == true}
    
    @ ${false != false} ${false != true} ${true != false} ${true != true}
}
";
        var expected = @"false true true true false false true false true true false";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
