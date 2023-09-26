using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Foreach_Statement
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
    foreach(var e in [1, 2, 3, 4])
        @$e
}
";
        var expected = @"1234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_WithManualEnumerable()
    {
        var input = @"struct EnumerableX
{
	EnumeratorX GetEnumerator()
	{
		return EnumeratorX();
	}
}

struct EnumeratorX
{
	int i;
	int count;
	
	EnumeratorX()
	{
		this->i = 0;
		this->count = 5;
	}

	bool Next(out int* x)
	{
		if (i == count) return false;
	
		*x = i;
		i++;
		return true;
	}
}

void Main()
{
	foreach(var i in EnumerableX())
	{
		@$i
	}
}
";
        var expected = @"01234";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_WithSeqFunc()
    {
        var input = @"void Main()
{
    seq string F()
    {
        yield ""Hello"";
        yield ""World"";
    }

    foreach(var e in F())
        @$e
}
";
        var expected = @" HelloWorld";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Scope()
    {
        var input = @"void Main()
{
    int i = 7;

    foreach(var i in [1, 2, 3, 4]);

    @$i
}
";
        var expected = @"7";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_LambdaAsItem()
    {
        var input = @"void Main()
{
    list<func<void>> fs = [];

    foreach(var x in [1, 2, 3, 4])
        fs.Add(() => @{$x});

    int a = 1, b = 2;
    fs.Add(() => ${a + b}); // a, b두개가 캡쳐되서 들어간다

    foreach(var f in fs)
        f();
}
";
        var expected = @"12343";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
