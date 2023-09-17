using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Cast_Boxed_Lambda_To_Func_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
	var bf = box () => { return 3; }
	func<int> f = bf; // CastBoxedLambdaToFuncExp(LoadExp(LocalVarLoc(""bf"")), func<int>)

	@${f()}
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
