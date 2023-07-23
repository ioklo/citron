namespace Citron;

using Xunit;
using static Citron.TextAnalysisTestMisc;
using static Citron.Syntax.SyntaxFactory;
using Citron.Syntax;
using System;

public class TypeExpParserTests
{
    // 고려사항 T?, box T*, T*, T.ID, (T), (T1, T2), func<>

    // 하면 안되는것.
    // A?? => (A?)?
    // box int** => (box int*)*
    // A.(B) => A.B
    // 단일항 괄호 (B) => B
    // 단일항 여러 괄호 ((((C)))) => C

    void TestSuccess(string text, TypeExp expected)
    {
        var lexer = new Lexer();
        var context = MakeParserContext(text);
        TypeExpParser.Parse(lexer, ref context, out var typeExp);

        Assert.True(context.LexerContext.Pos.IsReachEnd());
        Assert.Equal(expected, typeExp);
    }

    void TestFail(string text)
    {
        var lexer = new Lexer();
        var context = MakeParserContext(text);
        TypeExpParser.Parse(lexer, ref context, out _);

        Assert.False(context.LexerContext.Pos.IsReachEnd());
    }

    // 다음을 체크해야 한다
    // 최상위군: T?, box T*, T*, (T) (x), A.B<int>.C
    // T?군: T??(x), box T*?, T*?, (T*)?, A.B<int>.C?
    // box T*군: box T?*(x), box box T**(x), box T**(x), box (T*)*, box A.B<int>.C*
    // T*군: T?*(x), box T**(x), T**, (T*)*, A.B<int>.C*
    // (T)군: (T?)*, (box T*)*, (T*)*, ((T*)) (x), (A.B<int>.C) (x)

    // 최상위군: T?, box T*, T*, (T) (x), A.B<int>.C
    #region TopLevel

    [Fact]
    public void TestParseNullable()
    {
        var expected = SNullableTypeExp(SIdTypeExp("T"));
        TestSuccess("T?", expected);
    }

    [Fact]
    public void TestParseBoxPtr()
    {
        var expected = SBoxPtrTypeExp(SIdTypeExp("T"));
        TestSuccess("box T*", expected);
    }

    [Fact]
    public void TestParseLocalPtr()
    {
        var expected = SLocalPtrTypeExp(SIdTypeExp("int"));
        TestSuccess(@"int*", expected);
    }

    [Fact]
    public void TestParseParenSolo()
    {
        TestFail("(T)");
    }

    [Fact]
    public void TestParseIdChain()
    {
        var expected = SIdTypeExp("A").Member("B", SIdTypeExp("int")).Member("C");
        TestSuccess("A.B<int>.C", expected);
    }

    #endregion TopLevel

    // T?군: T??(x), box T*?, T*?, (T*)?, A.B<int>.C?
    #region Nullable
    [Fact]
    public void TestParseDoubleQuestionMark()
    {
        TestFail("T??");
    }

    [Fact]
    public void TestParseNullableBoxPtr()
    {
        var expected = SNullableTypeExp(SBoxPtrTypeExp(SIdTypeExp("T")));
        TestSuccess("box T*?", expected);
    }

    [Fact]
    public void TestParseNullableLocalPtr()
    {
        var expected = SNullableTypeExp(SLocalPtrTypeExp(SIdTypeExp("T")));
        TestSuccess("T*?", expected);
    }

    [Fact]
    public void TestParseNullableParen()
    {
        var expected = SNullableTypeExp(SLocalPtrTypeExp(SIdTypeExp("T")));
        TestSuccess("(T*)?", expected);
    }

    [Fact]
    public void TestIdChainNullable()
    {
        var expected = SNullableTypeExp(SIdTypeExp("A").Member("B", SIdTypeExp("int")).Member("C"));
        TestSuccess("A.B<int>.C?", expected);
    }

    #endregion Nullable

    // box T*군: box T?*(x), box box T**(x), box T**(x), box (T*)*, box A.B<int>.C*
    #region BoxPtr
    [Fact]
    public void TestParseBoxPtrOfNullableRaw()
    {
        TestFail("Box T?*");
    }

    [Fact]
    public void TestParseNestedBoxPtrs()
    {
        TestFail("box box T**");
    }

    [Fact]
    public void TestParseAmbiguousLocalAndBoxPtrs()
    {
        TestFail("box T**");
    }

    [Fact]
    public void TestParseBoxPtrOfParen() 
    {
        var expected = SBoxPtrTypeExp(SNullableTypeExp(SIdTypeExp("T")));
        TestSuccess("box (T?)*", expected);
    }

    [Fact]
    public void TestBoxPtrOfIdChain()
    {
        var expected = SBoxPtrTypeExp(SIdTypeExp("A").Member("B", SIdTypeExp("int")).Member("C"));
        TestSuccess("box A.B<int>.C*", expected);
    }
    #endregion BoxPtr

    // T*군: T?*(x), box T**(x), T**, (T*)*, A.B<int>.C*
    #region LocalPtr
    [Fact]
    public void TestParseLocalPtrOfNullableRaw()
    {
        TestFail("T?*");
    }

    // box 테스트와 중복
    //[Fact]
    //public void TestParseAmbiguousLocalAndBoxPtrs()
    //{
    //    TestFail("box T**");
    //}

    [Fact]
    public void TestParseNestedLocalPtrs()
    {
        var expected = SLocalPtrTypeExp(SLocalPtrTypeExp(SIdTypeExp("T")));
        TestSuccess("T**", expected);
    }
    

    [Fact]
    public void TestParseLocalPtrOfParen()
    {
        var expected = SLocalPtrTypeExp(SNullableTypeExp(SIdTypeExp("T")));
        TestSuccess("(T?)*", expected);
    }

    [Fact]
    public void TestLocalPtrOfIdChain()
    {
        var expected = SLocalPtrTypeExp(SIdTypeExp("A").Member("B", SIdTypeExp("int")).Member("C"));
        TestSuccess("A.B<int>.C*", expected);
    }
    #endregion LocalPtr

    // (T)군: (T?)*, (box T*)*, (T*)*, ((T*)) (x), (A.B<int>.C) (x)
    #region Paren
    [Fact]
    public void TestParseWrappedNullable()
    {
        var expected = SNullableTypeExp(SNullableTypeExp(SIdTypeExp("T")));
        TestSuccess("(T?)?", expected);
    }

    [Fact]
    public void TestParseWrappedBoxPtr()
    {
        var expected = SNullableTypeExp(SBoxPtrTypeExp(SIdTypeExp("T")));
        TestSuccess("(box T*)?", expected);
    }

    [Fact]
    public void TestParseWrappedLocalPtr()
    {
        var expected = SLocalPtrTypeExp(SLocalPtrTypeExp(SIdTypeExp("T")));
        TestSuccess("(T*)*", expected);
    }

    [Fact]
    public void TestParseWrappedNested()
    {
        TestFail("((T*))?");
    }

    [Fact]
    public void TestParseWrappedIdChain()
    {
        TestFail("(A.B<int>.C)?");
    }






    #endregion Paren

}