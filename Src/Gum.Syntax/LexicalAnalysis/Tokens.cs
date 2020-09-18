using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.LexicalAnalysis
{
    public abstract class Token
    {
    }    

    public class EqualEqualToken : Token { public static EqualEqualToken Instance { get; } = new EqualEqualToken(); private EqualEqualToken() { } } // ==
    public class ExclEqualToken : Token { public static ExclEqualToken Instance { get; } = new ExclEqualToken(); private ExclEqualToken() { } } // !=

    public class PlusPlusToken : Token { public static PlusPlusToken Instance { get; } = new PlusPlusToken(); private PlusPlusToken() { } } // ++
    public class MinusMinusToken : Token { public static MinusMinusToken Instance { get; } = new MinusMinusToken(); private MinusMinusToken() { } } // --
    public class LessThanEqualToken : Token { public static LessThanEqualToken Instance { get; } = new LessThanEqualToken(); private LessThanEqualToken() { } } // <=
    public class GreaterThanEqualToken : Token { public static GreaterThanEqualToken Instance { get; } = new GreaterThanEqualToken(); private GreaterThanEqualToken() { } } // >=    
    public class EqualGreaterThanToken : Token { public static EqualGreaterThanToken Instance { get; } = new EqualGreaterThanToken(); private EqualGreaterThanToken() { } } // =>

    public class LessThanToken : Token { public static LessThanToken Instance { get; } = new LessThanToken(); private LessThanToken() { } } // <
    public class GreaterThanToken : Token { public static GreaterThanToken Instance { get; } = new GreaterThanToken(); private GreaterThanToken() { } } // >

    public class EqualToken : Token { public static EqualToken Instance { get; } = new EqualToken(); private EqualToken() { } } // =
    public class CommaToken : Token { public static CommaToken Instance { get; } = new CommaToken(); private CommaToken() { } } // ,
    public class SemiColonToken : Token { public static SemiColonToken Instance { get; } = new SemiColonToken(); private SemiColonToken() { } } // ;   
    public class LBraceToken : Token { public static LBraceToken Instance { get; } = new LBraceToken(); private LBraceToken() { } } // {
    public class RBraceToken : Token { public static RBraceToken Instance { get; } = new RBraceToken(); private RBraceToken() { } } // }
    public class LParenToken : Token { public static LParenToken Instance { get; } = new LParenToken(); private LParenToken() { } } // (
    public class RParenToken : Token { public static RParenToken Instance { get; } = new RParenToken(); private RParenToken() { } } // )
    public class LBracketToken : Token { public static LBracketToken Instance { get; } = new LBracketToken(); private LBracketToken() { } } // [
    public class RBracketToken : Token { public static RBracketToken Instance { get; } = new RBracketToken(); private RBracketToken() { } } // ]
    
    public class PlusToken : Token { public static PlusToken Instance { get; } = new PlusToken(); private PlusToken() { } } // +
    public class MinusToken : Token { public static MinusToken Instance { get; } = new MinusToken(); private MinusToken() { } } // -
    public class StarToken : Token { public static StarToken Instance { get; } = new StarToken(); private StarToken() { } } // *   
    public class SlashToken : Token { public static SlashToken Instance { get; } = new SlashToken(); private SlashToken() { } } // /    
    public class PercentToken : Token { public static PercentToken Instance { get; } = new PercentToken(); private PercentToken() { } } // %    
    public class ExclToken : Token { public static ExclToken Instance { get; } = new ExclToken(); private ExclToken() { } } // !    
    public class DotToken : Token { public static DotToken Instance { get; } = new DotToken(); private DotToken() { } } // .
    
    public class IfToken : Token { public static IfToken Instance { get; } = new IfToken(); private IfToken() { } }
    public class ElseToken : Token { public static ElseToken Instance { get; } = new ElseToken(); private ElseToken() { } }
    public class ForToken : Token { public static ForToken Instance { get; } = new ForToken(); private ForToken() { } }
    public class ContinueToken : Token { public static ContinueToken Instance { get; } = new ContinueToken(); private ContinueToken() { } }
    public class BreakToken : Token { public static BreakToken Instance { get; } = new BreakToken(); private BreakToken() { } }    
    public class TaskToken : Token { public static TaskToken Instance { get; } = new TaskToken(); private TaskToken() { } }
    public class ParamsToken : Token { public static ParamsToken Instance { get; } = new ParamsToken(); private ParamsToken() { } }
    public class ReturnToken : Token { public static ReturnToken Instance { get; } = new ReturnToken(); private ReturnToken() { } }
    public class AsyncToken : Token { public static AsyncToken Instance { get; } = new AsyncToken(); private AsyncToken() { } }
    public class AwaitToken : Token { public static AwaitToken Instance { get; } = new AwaitToken(); private AwaitToken() { } }
    public class ForeachToken : Token { public static ForeachToken Instance { get; } = new ForeachToken(); private ForeachToken() { } }
    public class InToken : Token { public static InToken Instance { get; } = new InToken(); private InToken() { } }
    public class YieldToken : Token { public static YieldToken Instance { get; } = new YieldToken(); private YieldToken() { } }
    public class SeqToken : Token { public static SeqToken Instance { get; } = new SeqToken(); private SeqToken() { } }
    public class EnumToken : Token { public static EnumToken Instance { get; } = new EnumToken(); private EnumToken() { } }
    public class StructToken : Token { public static StructToken Instance { get; } = new StructToken(); private StructToken() { } }
    public class IsToken : Token { public static IsToken Instance { get; } = new IsToken(); private IsToken() { } }

    public class PublicToken : Token { public static PublicToken Instance { get; } = new PublicToken(); private PublicToken() { } }
    public class ProtectedToken : Token { public static ProtectedToken Instance { get; } = new ProtectedToken(); private ProtectedToken() { } }
    public class PrivateToken : Token { public static PrivateToken Instance { get; } = new PrivateToken(); private PrivateToken() { } }
    public class StaticToken : Token { public static StaticToken Instance { get; } = new StaticToken(); private StaticToken() { } }
    public class NewToken : Token { public static NewToken Instance { get; } = new NewToken(); private NewToken() { } }

    public class ColonToken : Token { public static ColonToken Instance { get; } = new ColonToken(); private ColonToken() { } }



    public class WhitespaceToken : Token { public static WhitespaceToken Instance { get; } = new WhitespaceToken(); private WhitespaceToken() { } } // \s
    public class NewLineToken : Token { public static NewLineToken Instance { get; } = new NewLineToken(); private NewLineToken() { } }     // \r \n \r\n

    public class DoubleQuoteToken : Token { public static DoubleQuoteToken Instance { get; } = new DoubleQuoteToken(); private DoubleQuoteToken() { } } // "
    public class DollarLBraceToken : Token { public static DollarLBraceToken Instance { get; } = new DollarLBraceToken(); private DollarLBraceToken() { } }
    public class EndOfFileToken : Token { public static EndOfFileToken Instance { get; } = new EndOfFileToken(); private EndOfFileToken() { } }

    // digit
    public class IntToken : Token
    {
        public int Value { get; }
        public IntToken(int value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is IntToken token &&
                   Value == token.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(IntToken? left, IntToken? right)
        {
            return EqualityComparer<IntToken?>.Default.Equals(left, right);
        }

        public static bool operator !=(IntToken? left, IntToken? right)
        {
            return !(left == right);
        }
    }

    public class BoolToken : Token 
    { 
        public bool Value { get; }
        public BoolToken(bool value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is BoolToken token &&
                   Value == token.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(BoolToken? left, BoolToken? right)
        {
            return EqualityComparer<BoolToken?>.Default.Equals(left, right);
        }

        public static bool operator !=(BoolToken? left, BoolToken? right)
        {
            return !(left == right);
        }
    }

    public class TextToken : Token
    {
        public string Text { get; }
        public TextToken(string text) { Text = text; }

        public override bool Equals(object? obj)
        {
            return obj is TextToken token &&
                   Text == token.Text;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text);
        }

        public static bool operator ==(TextToken? left, TextToken? right)
        {
            return EqualityComparer<TextToken?>.Default.Equals(left, right);
        }

        public static bool operator !=(TextToken? left, TextToken? right)
        {
            return !(left == right);
        }
    }
    
    public class IdentifierToken : Token
    {
        public string Value { get; }
        public IdentifierToken(string value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is IdentifierToken token &&
                   Value == token.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }

    // TODO: QuickSC Token으로 내리기
    public class ExecToken : Token { public static ExecToken Instance { get; } = new ExecToken(); private ExecToken() { } }
}
