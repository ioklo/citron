namespace Citron;

using Xunit;
using static Citron.TextAnalysisTestMisc;
using static Citron.Syntax.SyntaxFactory;

public class TypeExpParserTests
{

    // TYPE_EXP0  = TYPE_EXP1 TYPE_EXP0' 
    // TYPE_EXP0' = ? TYPE_EXP0' | e

    // TYPE_EXP1  = box TYPE_EXP2 * 
    //            | TYPE_EXP2 TYPE_EXP1'
    // TYPE_EXP1' = * TYPE_EXP1'
    //            | e

    // TYPE_EXP2  = ID TYPE_EXP2'
    //            | ( TYPE_EXP0 ) 
    // TYPE_EXP2' = . ID TYPE_EXP2'
    //            | e    



    // 고려사항 T?, box T*, T*, T.ID, (T), (T1, T2), func<>

    // 하면 안되는것.
    // A?? => (A?)?
    // box int** => (box int*)*
    // A.(B) => A.B
    // 단일항 괄호 (B) => B
    // 단일항 여러 괄호 ((((C)))) => C

    // 배열을 다시 해야 할 것 같다

    // 문법측면에서 단일항을 체크할 방법이 있는가?

    // 다음을 체크해야 한다
    // T?군: T??(x), box T*?, T*?, (T*)?, A.B<int>.C?
    // box T*군: box T?*(x), box box T**(x), box T**(x), box (T*)*, box A.B<int>.C*
    // T*군: T?*(x), box T**(x), T**, (T*)*, A.B<int>.C*
    // (T)군: (T?)*, (box T*)*, (T*)*, ((T*)) (x), (A.B<int>.C) (x)
    // 최상위군: T?, box T*, T*, (T) (x), A.B<int>.C

    [Fact]
    public void TestParseLocalPtrTypeExp()
    {
        var lexer = new Lexer();
        var context = MakeParserContext(@"int**");
        TypeExpParser.Parse(lexer, ref context, out var typeExp);

        var expected = SLocalPtrTypeExp(SLocalPtrTypeExp(SIdTypeExp("int")));

        Assert.Equal(expected, typeExp);
    }
}