using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Local_Variable_Reference_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
    int s = 3;
    var* i = &s; // LocalVarRefExp(LocalVar(""i""))

    @{${*i}}
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Nested()
    {
        var input = @"void Main()
{
	int s = 3;
	int* i = &s;  
	int** j = &i;
	**j = 4;
}
@$s
";
        var expected = @"4";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Uninitialized()
    {
        var input = @"void Main()
{
	int i;       // uninitialized
	int* p = &i; // 에러, uninitialized는 포인터로 가리킬 수 없습니다
	@{$p} 
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
