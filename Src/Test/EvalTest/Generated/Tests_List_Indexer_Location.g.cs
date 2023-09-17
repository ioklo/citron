using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_List_Indexer_Location
{

    [Fact]
    public Task Test_General()
    {
        var input = @"void Main()
{
	var a = [1, 2, 3];
	@${a[1]}
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
