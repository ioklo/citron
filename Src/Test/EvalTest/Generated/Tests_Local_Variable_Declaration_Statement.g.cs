using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Local_Variable_Declaration_Statement
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
    int x = 0;
    @$x
}
";
        var expected = @"0";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Uninitialized()
    {
        var input = @"void Main()
{
    int a;
    int b = 1;

    a = 0;

    @$a $b
}
";
        var expected = @"0 1";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_UseUninitialized()
    {
        var input = @"bool F()
{
    return true;
}

void Main()
{
    int x;
    
    if (F()) { x = 3; } // 심지어 F()가 항상 true를 리턴해도

    // 에러, x가 초기화 되지 않았습니다
    @$x 
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_TypeInference()
    {
        var input = @"int MakeInt()
{
    return 3;
}

void Main()
{
    var i = MakeInt();
    var s = ""hello"";
    var b = false;
    var l = [1, 2, 3, 4];

    var le = l[2];

    @$i $s $b $le
}
";
        var expected = @"3 hello false 3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_CantInferenceType()
    {
        var input = @"void Main()
{
    var x; // 에러
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_VarWithPointerForPointerValue()
    {
        var input = @"void Main()
{
    var i = 3;
    var* x = &i;
    box var* y = box 3;

	int? i = null;
    var? optI = i;
}
";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_VarWithoutPointerSignForPointerValue()
    {
        var input = @"void Main()
{
    var i = 3;
    var x = &i; // 에러
    var y = box 3; // 에러

	int? i = 3;
    var optI = i; // 에러
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_MultipleVarItemsInferSeparately()
    {
        var input = @"void Main()
{
    int a = 0, x;

    var i = 1, s = ""hello"", l = [1, 2], b = false;

    x = 2;

    @$i $s ${l[0]} $b $a $x
}
";
        var expected = @"1 hello 1 false 0 2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_VarWithPointerInferenceSeparately()
    {
        var input = @"void Main()
{
    int a = 0
    string b = ""hi""
    var* x = &a, y = &b;
    @${*x}, ${*y}
}
";
        var expected = @"0 hi";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_VarWithPointerForValue()
    {
        var input = @"void Main()
{
    int a = 0;
    var* x = &a, y = 3; // 에러
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
