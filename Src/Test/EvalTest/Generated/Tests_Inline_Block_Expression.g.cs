using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Inline_Block_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
    int s = 2;
    int x = inline {
        return (s + 4) / 2;
    };

    @$x;
}

";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_InferByReturnType()
    {
        var input = @"void Main()
{
	var x = inline { return 3; };
	@$x
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_InferByAssignTargetType()
    {
        var input = @"void Main()
{
	int x;
	x = inline { return 3; };
	@$x
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_InferByFunctionParameter()
    {
        var input = @"void F(int x)
{
	@$x
}

void Main()
{
	F(inline { return 3; });
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_DifferentReturnType()
    {
        var input = @"class B { }
class C1 : B { }
class C2 : B { }

void Main()
{
	var x = inline {
		if (true) 
		{
			return new C1();
		}
		else 
		{
			return new C2();
		}
	};
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_HintTypeFirst()
    {
        var input = @"class B { }
class C1 : B { }
class C2 : B { }

void Main()
{
	B x = inline {
		if (true) 
		{
			return new C1();
		}
		else 
		{
			return new C2();
		}
	};
}
";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_ShouldNotBeVoid()
    {
        var input = @"void Main()
{
	var s = inline { };
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_TopLevelNotAllowed()
    {
        var input = @"void Main()
{
	inline { @{hello} } 
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
