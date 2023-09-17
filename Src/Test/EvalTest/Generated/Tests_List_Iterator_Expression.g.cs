using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_List_Iterator_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"var l = [1, 2, 3]
foreach(var i in l) // ListIterExp(LocalVarLoc(""l""))
{
	@$i
}
";
        var expected = @"123";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
