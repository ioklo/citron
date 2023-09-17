using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Struct_Member_Box_Reference_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"struct A { int i; }
struct S { A a; }

void Main()
{
	box var* s = box S(A(3));
	box var* x = &s->a.i; // StructMemberBoxRefExp(StructIndirectMemberBoxRefExp(s, S.a), S.i)
	*x = 5;

	@${s->a.i}
}

";
        var expected = @"5";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
