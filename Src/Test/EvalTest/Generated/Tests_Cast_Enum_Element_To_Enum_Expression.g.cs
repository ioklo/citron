using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Cast_Enum_Element_To_Enum_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"enum E { First, Second(int i) }

void Main()
{
	E.Second s = E.Second(2);
	E e = s; // CastEnumElemToEnumExp(LoadExp(LocalVarLoc(s)), E)
}

";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
