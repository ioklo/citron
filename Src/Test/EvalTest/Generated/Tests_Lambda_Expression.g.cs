using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Lambda_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main() 
{
    var f = () => { @{hi} };
    f();
}
";
        var expected = @"hi";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Usage()
    {
        var input = @"void Main()
{
    var l1 = (int x) => x + 1; // 본문 축약형
    var l2 = (string s) => { return s; } // 완전한 본문

    var v1 = l1(2);
    var v2 = l2(""hi"");

    // 3, ""hi""
    @$v1, $v2
}
";
        var expected = @"3 hi";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_GlobalFunctionAsLambda()
    {
        var input = @"void Func()
{
    @hi
}

void Main()
{
    var f = Func;
    f();
}
";
        var expected = @"hi";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_MemberFunctionAsLambda()
    {
        var input = @"struct S
{
	int x;
	
	void Func()
	{
	    @$x
	}
}

void Main()
{
	var s = S(2);
    var f = s.Func;
    f();
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Capture_Copy()
    {
        var input = @"void Main()
{
    int x = 0;
    var l = () => x;
    x = 1;

    // 0
    @${l()}
}
";
        var expected = @"0";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Capture_LocalPtr()
    {
        var input = @"void Main()
{
    int x = 0;
    int* y = &x;

    // local pointer 함유 lambda, 내부에서밖에 쓸 수 없습니다
    var l = () => *y;
    x = 1;

    // 1
    @${l()}
}
";
        var expected = @"1";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Capture_BoxPtr()
    {
        var input = @"void Main()
{
    box int* x = box 0; // heap을 사용하는 버전
    var l = () => *x;
    
    *x = 2;

    // 2
    @${l()}
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Capture_This()
    {
        var input = @"struct S
{
    int x;
    
    void F()
    {
        var l = () => this->x + 2; // this는 캡쳐대상 S*
        
        x = 3;
        
        // 5
        @${l()}
    }
}


void Main()
{
    var s = S(3);
    s.F();
}
";
        var expected = @"5";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
