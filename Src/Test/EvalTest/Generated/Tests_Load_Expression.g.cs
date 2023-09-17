using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Load_Expression
{

    [Fact]
    public Task Test_LoadLocalVar()
    {
        var input = @"void Main()
{
	int a = 1;

	// a는 Local_Variable_Location이고, b에 대입하기 위해 내부적으로 load expression을 사용하게 됩니다
	// local_var_decl_stmt(""b"", load_exp(local_var_loc(""a"")))
	int b = a;

	@$b
}
";
        var expected = @"1";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
