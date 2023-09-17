using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Struct_Indirect_Member_Box_Reference_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"struct S
{
	int i;
}

void Main()
{
	box S* bs = new S(3);
	box int* x = &bs->i; // StructIndirectMemberBoxRefExp(bs, S.i)

	*x = 2;

	@${bs->i}
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
