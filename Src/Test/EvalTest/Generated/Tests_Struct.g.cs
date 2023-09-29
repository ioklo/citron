using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Struct
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"public struct B
{
    int a;
}

struct S : B
{
    int x; // default public
    private int y;

    int Sum() // default public
    {
        return a + x + y;
    }

    int GetY() 
    {
        return y;
    }
}

void Main()
{	
	// 일단 다 적고 나중에 분리
	
	var s1 = S(1, 2, 3); // a, x, y
	@${s1.x}
	@ ${s1.GetY()}
	@ ${s1.Sum()}
	
	S s2 = s1;                // 복사 생성, 오버라이드 불가능
	
	// 고급, box, boxed 타입 S*
	box var* s3 = box S(1, 2, 3);
	
	@${*s3.a}
	s2 = *s3;
}

";
        var expected = @"2 3 6 1";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_AutoTrivialConstructor()
    {
        var input = @"// 2, 3
struct S
{
    int x;
    int y;
}

var s = new S(2, 3);
@${s.x} ${s.y}
";
        var expected = @"2 3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
