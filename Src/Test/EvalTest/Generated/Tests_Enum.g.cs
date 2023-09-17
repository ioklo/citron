using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Enum
{

    [Fact]
    public Task Test_Complex()
    {
        var input = @"// 선언
enum Coord2D<T>
{
    Rect(T x, T y),            // 기본 C syntax와 비슷한 느낌을 주려고 콤마로 구분합니다
    Polar(T radius, T angle),
}

int GetLengthSq(Coord2D<int> m)
{
    if (m is .Rect)
        return m.x * m.x + m.y * m.y;

    else if (m is .Polar) 
        return m.radius * m.radius;
}

void Main()
{
    Coord2D<int> m = .Rect(20, 30); // 타입힌트가 있어서 Coord2D<int>.Rect로 쓰지 않아도 됩니다
    var lenSq = GetLengthSq(m);

    @$lenSq
}
";
        var expected = @"1300";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_ConstructStandalone()
    {
        var input = @"enum E { First }

void Main()
{   
    var e = E.First; // e는 E 타입입니다
    
}
";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_ConstructWIthArgument()
    {
        var input = @"enum E { Second(int x) }

void Main()
{
    var e = E.Second(2); // e는 E 타입입니다
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Generics()
    {
        var input = @"enum Option<T>
{
    None,
    Some(T value)
}

Option<int> i = None;
Option<string> s = Some(""Hi"");

if (s is Option<string>.Some some)
    @${some.value}

";
        var expected = @"Hi";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_IfTest()
    {
        var input = @"enum E { First, Second(int x) }

void Main()
{
    var e = E.First;

    if (e is E.First)
        @true
    else if (e is E.Second s)
        @{s.x}
}

";
        var expected = @"true";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_SwitchTest()
    {
        var input = @"enum E { First, Second(int x, bool y), Third(string s) }
void Main()
{
    var e = E.Second(2);
    switch (e)
    {
        case E.First:
            @First
        
        case E.Second(var x, _):
            @$x
            
        case E.Third x:
            @${x.s}
    }
}
";
        var expected = @"2";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_TypeHint()
    {
        var input = @"enum E 
{
    First,
    Second(int x)
}


// 함수 인자
void F1(E e)
{
}

E F2()
{
    return .First;
}

void Main()
{
    // 1. local variable declaration의 initialization 부분
    E e = .Second(2);

    // 2. 함수 인자
    F1(.First);
    
    // 3. 함수 리턴
    e = F2();
}

";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
