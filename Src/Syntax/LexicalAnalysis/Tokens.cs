using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Citron.LexicalAnalysis
{
    public abstract class Token
    {
    }

    public class SingleToken : Token
    {
        string debugText;
        public SingleToken(string debugText) { this.debugText = debugText; }
    }

    public static class Tokens
    {
        public static readonly SingleToken EqualEqual = new SingleToken("==");
        public static readonly SingleToken ExclEqual = new SingleToken("!="); // !=

        public static readonly SingleToken PlusPlus = new SingleToken("++"); // ++
        public static readonly SingleToken MinusMinus = new SingleToken("--"); // --
        public static readonly SingleToken LessThanEqual = new SingleToken("<="); // <=
        public static readonly SingleToken GreaterThanEqual = new SingleToken(">="); // >=    
        public static readonly SingleToken EqualGreaterThan = new SingleToken("=>"); // =>
        public static readonly SingleToken MinusGreaterThan = new SingleToken("->"); // ->

        public static readonly SingleToken LessThan = new SingleToken("<"); // <
        public static readonly SingleToken GreaterThan = new SingleToken(">"); // >

        public static readonly SingleToken Equal = new SingleToken("="); // =
        public static readonly SingleToken Comma = new SingleToken(","); // ,
        public static readonly SingleToken SemiColon = new SingleToken(";"); // ;   
        public static readonly SingleToken LBrace = new SingleToken("{"); // {
        public static readonly SingleToken RBrace = new SingleToken("}"); // }
        public static readonly SingleToken LParen = new SingleToken("("); // (
        public static readonly SingleToken RParen = new SingleToken(")"); // )
        public static readonly SingleToken LBracket = new SingleToken("["); // [
        public static readonly SingleToken RBracket = new SingleToken("]"); // ]

        public static readonly SingleToken Plus = new SingleToken("+"); // +
        public static readonly SingleToken Minus = new SingleToken("-"); // -
        public static readonly SingleToken Star = new SingleToken("*"); // *   
        public static readonly SingleToken Slash = new SingleToken("/"); // /    
        public static readonly SingleToken Percent = new SingleToken("%"); // %    
        public static readonly SingleToken Excl = new SingleToken("!"); // !    
        public static readonly SingleToken Dot = new SingleToken("."); // .
        public static readonly SingleToken Question = new SingleToken("?"); // ?
        public static readonly SingleToken Ampersand = new SingleToken("&"); // &

        public static readonly SingleToken If = new SingleToken("if");
        public static readonly SingleToken Else = new SingleToken("else");
        public static readonly SingleToken For = new SingleToken("for");
        public static readonly SingleToken Continue = new SingleToken("continue");
        public static readonly SingleToken Break = new SingleToken("break");
        public static readonly SingleToken Task = new SingleToken("task");
        public static readonly SingleToken Params = new SingleToken("params");
        public static readonly SingleToken Return = new SingleToken("return");
        public static readonly SingleToken Async = new SingleToken("async");
        public static readonly SingleToken Await = new SingleToken("await");
        public static readonly SingleToken Foreach = new SingleToken("foreach");
        public static readonly SingleToken In = new SingleToken("in");
        public static readonly SingleToken Yield = new SingleToken("yield");
        public static readonly SingleToken Seq = new SingleToken("seq");
        public static readonly SingleToken Enum = new SingleToken("enum");
        public static readonly SingleToken Struct = new SingleToken("struct");
        public static readonly SingleToken Class = new SingleToken("class");
        public static readonly SingleToken Is = new SingleToken("is");
        public static readonly SingleToken As = new SingleToken("as");

        public static readonly SingleToken Ref = new SingleToken("ref");
        public static readonly SingleToken Box = new SingleToken("box");
        public static readonly SingleToken Local = new SingleToken("local");
        public static readonly SingleToken Null = new SingleToken("null");

        public static readonly SingleToken Public = new SingleToken("public");
        public static readonly SingleToken Protected = new SingleToken("protected");
        public static readonly SingleToken Private = new SingleToken("private");
        public static readonly SingleToken Static = new SingleToken("static");
        public static readonly SingleToken New = new SingleToken("new");
        public static readonly SingleToken Namespace = new SingleToken("namespace");

        public static readonly SingleToken Colon = new SingleToken(":");
        public static readonly SingleToken Backtick = new SingleToken("`");

        public static readonly SingleToken Whitespace = new SingleToken("<whitespace>"); // \s
        public static readonly SingleToken NewLine = new SingleToken("<newline>");     // \r \n \r\n

        public static readonly SingleToken DoubleQuote = new SingleToken("\""); // "
        public static readonly SingleToken DollarLBrace = new SingleToken("${");
        public static readonly SingleToken EndOfFile = new SingleToken("<eof>");

        // TODO: QuickSC Token으로 내리기
        public static readonly SingleToken At = new SingleToken("@");
    }

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

    
}
