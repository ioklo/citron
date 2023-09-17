using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Cast_Box_Pointer_To_Local_Pointer
{

    [Fact]
    public Task Test_NotFixed()
    {
        var input = @"void Main()
{
    int* s = box 3; // 에러, box가 고정되지 못함
    // 가비지 컬렉터가 돌면서 'box 3'를 힙에서 제거
    @${*s}
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_AsArgument()
    {
        var input = @"void F(int *s)
{
	*s = 3;
}

void Main()
{
	F(box 3); // box 3의 결과는 temporary variable에 저장되고, statement가 끝날때 사라집니다.
}

";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
