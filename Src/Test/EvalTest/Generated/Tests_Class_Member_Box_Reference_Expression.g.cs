using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Class_Member_Box_Reference_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"class C
{
	int x;
	
	box int* GetX()
	{
		return &x; // ClassMemberBoxExp(this, C.x)
	}

	void PrintX()
	{
		@$x
	}
}

void Main()
{
	var c = new C(3);
	box var* pX = c.GetX();
	*pX = 4;
	
	c.PrintX();
}

";
        var expected = @"4";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
