using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Static_Box_Reference_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"struct C
{
	static int x;
	static C()
	{
		x = 3;
	}
}

void Main()
{
	box var* s = &C.x; // reference operator에 box pointer로 만들어달라는 요청을 줍니다
	@${*s}
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
