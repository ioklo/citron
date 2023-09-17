using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_If_Nullable_Reference_Test_Statement
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"class B { }
class C : B { }

void Main()
{
    B b = new C();
    if (C c = b)
    {
        @succeed
    }
}
";
        var expected = @"succeed";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_CantTestValueType()
    {
        var input = @"class C {}

void Main()
{
    var s1 = 0;
    if (s1 is C)  // wrong, 명확한 타입에 대해서는 타입비교 불가
        @false
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_TestUnrelatedClass()
    {
        var input = @"class C { }
class D { }

void Main()
{
	var c = new C();

	if (D d = c); // 미리 잡을 수 있는 경우는 최대한 잡습니다
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_TestInterface()
    {
        var input = @"interface I {}
class C : I {}

void Main()
{
	I i = new C();

	if (C c = i) @true
}
";
        var expected = @"true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_TestClassImplInterface()
    {
        var input = @"interface I {}
class B { }
class C : B, I { }

void Main()
{
	var b = new C();
	if (I i = b)
	{
		@true
	}
}
";
        var expected = @"true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_TestInterfaceImplInterface()
    {
        var input = @"interface I1 { }
interface I2 { }
class B : I1 { }
class C : B, I2 { }

void Main()
{
	I1 i = new C();

	if (I2 i2 = i)
	{
		@true
	}
}


";
        var expected = @"true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
