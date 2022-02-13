using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Citron.LexicalAnalysis
{
    public abstract class Token
    {
    }    

    public class EqualEqualToken : Token { public static readonly EqualEqualToken Instance = new EqualEqualToken(); private EqualEqualToken() { } } // ==
    public class ExclEqualToken : Token { public static readonly ExclEqualToken Instance = new ExclEqualToken(); private ExclEqualToken() { } } // !=

    public class PlusPlusToken : Token { public static readonly PlusPlusToken Instance = new PlusPlusToken(); private PlusPlusToken() { } } // ++
    public class MinusMinusToken : Token { public static readonly MinusMinusToken Instance = new MinusMinusToken(); private MinusMinusToken() { } } // --
    public class LessThanEqualToken : Token { public static readonly LessThanEqualToken Instance = new LessThanEqualToken(); private LessThanEqualToken() { } } // <=
    public class GreaterThanEqualToken : Token { public static readonly GreaterThanEqualToken Instance = new GreaterThanEqualToken(); private GreaterThanEqualToken() { } } // >=    
    public class EqualGreaterThanToken : Token { public static readonly EqualGreaterThanToken Instance = new EqualGreaterThanToken(); private EqualGreaterThanToken() { } } // =>

    public class LessThanToken : Token { public static readonly LessThanToken Instance = new LessThanToken(); private LessThanToken() { } } // <
    public class GreaterThanToken : Token { public static readonly GreaterThanToken Instance = new GreaterThanToken(); private GreaterThanToken() { } } // >

    public class EqualToken : Token { public static readonly EqualToken Instance = new EqualToken(); private EqualToken() { } } // =
    public class CommaToken : Token { public static readonly CommaToken Instance = new CommaToken(); private CommaToken() { } } // ,
    public class SemiColonToken : Token { public static readonly SemiColonToken Instance = new SemiColonToken(); private SemiColonToken() { } } // ;   
    public class LBraceToken : Token { public static readonly LBraceToken Instance = new LBraceToken(); private LBraceToken() { } } // {
    public class RBraceToken : Token { public static readonly RBraceToken Instance = new RBraceToken(); private RBraceToken() { } } // }
    public class LParenToken : Token { public static readonly LParenToken Instance = new LParenToken(); private LParenToken() { } } // (
    public class RParenToken : Token { public static readonly RParenToken Instance = new RParenToken(); private RParenToken() { } } // )
    public class LBracketToken : Token { public static readonly LBracketToken Instance = new LBracketToken(); private LBracketToken() { } } // [
    public class RBracketToken : Token { public static readonly RBracketToken Instance = new RBracketToken(); private RBracketToken() { } } // ]
    
    public class PlusToken : Token { public static readonly PlusToken Instance = new PlusToken(); private PlusToken() { } } // +
    public class MinusToken : Token { public static readonly MinusToken Instance = new MinusToken(); private MinusToken() { } } // -
    public class StarToken : Token { public static readonly StarToken Instance = new StarToken(); private StarToken() { } } // *   
    public class SlashToken : Token { public static readonly SlashToken Instance = new SlashToken(); private SlashToken() { } } // /    
    public class PercentToken : Token { public static readonly PercentToken Instance = new PercentToken(); private PercentToken() { } } // %    
    public class ExclToken : Token { public static readonly ExclToken Instance = new ExclToken(); private ExclToken() { } } // !    
    public class DotToken : Token { public static readonly DotToken Instance = new DotToken(); private DotToken() { } } // .
    public class QuestionToken : Token { public static readonly QuestionToken Instance = new QuestionToken(); private QuestionToken() { } } // .
    
    public class IfToken : Token { public static readonly IfToken Instance = new IfToken(); private IfToken() { } }
    public class ElseToken : Token { public static readonly ElseToken Instance = new ElseToken(); private ElseToken() { } }
    public class ForToken : Token { public static readonly ForToken Instance = new ForToken(); private ForToken() { } }
    public class ContinueToken : Token { public static readonly ContinueToken Instance = new ContinueToken(); private ContinueToken() { } }
    public class BreakToken : Token { public static readonly BreakToken Instance = new BreakToken(); private BreakToken() { } }    
    public class TaskToken : Token { public static readonly TaskToken Instance = new TaskToken(); private TaskToken() { } }
    public class ParamsToken : Token { public static readonly ParamsToken Instance = new ParamsToken(); private ParamsToken() { } }
    public class ReturnToken : Token { public static readonly ReturnToken Instance = new ReturnToken(); private ReturnToken() { } }
    public class AsyncToken : Token { public static readonly AsyncToken Instance = new AsyncToken(); private AsyncToken() { } }
    public class AwaitToken : Token { public static readonly AwaitToken Instance = new AwaitToken(); private AwaitToken() { } }
    public class ForeachToken : Token { public static readonly ForeachToken Instance = new ForeachToken(); private ForeachToken() { } }
    public class InToken : Token { public static readonly InToken Instance = new InToken(); private InToken() { } }
    public class YieldToken : Token { public static readonly YieldToken Instance = new YieldToken(); private YieldToken() { } }
    public class SeqToken : Token { public static readonly SeqToken Instance = new SeqToken(); private SeqToken() { } }
    public class EnumToken : Token { public static readonly EnumToken Instance = new EnumToken(); private EnumToken() { } }
    public class StructToken : Token { public static readonly StructToken Instance = new StructToken(); private StructToken() { } }
    public class ClassToken : Token { public static readonly ClassToken Instance = new ClassToken(); private ClassToken() { } }
    public class IsToken : Token { public static readonly IsToken Instance = new IsToken(); private IsToken() { } }
    public class RefToken : Token { public static readonly RefToken Instance = new RefToken(); private RefToken() { } }
    public class NullToken : Token { public static readonly NullToken Instance = new NullToken(); private NullToken() { } }    

    public class PublicToken : Token { public static readonly PublicToken Instance = new PublicToken(); private PublicToken() { } }
    public class ProtectedToken : Token { public static readonly ProtectedToken Instance = new ProtectedToken(); private ProtectedToken() { } }
    public class PrivateToken : Token { public static readonly PrivateToken Instance = new PrivateToken(); private PrivateToken() { } }
    public class StaticToken : Token { public static readonly StaticToken Instance = new StaticToken(); private StaticToken() { } }
    public class NewToken : Token { public static readonly NewToken Instance = new NewToken(); private NewToken() { } }

    public class ColonToken : Token { public static readonly ColonToken Instance = new ColonToken(); private ColonToken() { } }
    public class BacktickToken : Token { public static readonly BacktickToken Instance = new BacktickToken(); private BacktickToken() { } }

    public class WhitespaceToken : Token { public static readonly WhitespaceToken Instance = new WhitespaceToken(); private WhitespaceToken() { } } // \s
    public class NewLineToken : Token { public static readonly NewLineToken Instance = new NewLineToken(); private NewLineToken() { } }     // \r \n \r\n

    public class DoubleQuoteToken : Token { public static readonly DoubleQuoteToken Instance = new DoubleQuoteToken(); private DoubleQuoteToken() { } } // "
    public class DollarLBraceToken : Token { public static readonly DollarLBraceToken Instance = new DollarLBraceToken(); private DollarLBraceToken() { } }
    public class EndOfFileToken : Token { public static readonly EndOfFileToken Instance = new EndOfFileToken(); private EndOfFileToken() { } }

    // digit
    public class IntToken : Token
    {
        public int Value { get; }
        public IntToken(int value) { Value = value; }
    }

    public class BoolToken : Token 
    { 
        public bool Value { get; }
        public BoolToken(bool value) { Value = value; }
    }

    public class TextToken : Token
    {
        public string Text { get; }
        public TextToken(string text) { Text = text; }
    }
    
    public class IdentifierToken : Token
    {
        public string Value { get; }
        public IdentifierToken(string value) { Value = value; }
    }

    // TODO: QuickSC Token으로 내리기
    public class ExecToken : Token { public static readonly ExecToken Instance = new ExecToken(); private ExecToken() { } }
}
