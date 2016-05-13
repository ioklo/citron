using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Text2AST
{
    public enum TokenType
    {
        Invalid,

        LBrace, RBrace, // { }
        LBracket, RBracket, // []
        LParen, RParen,     // ( )
        Comma,              // ,
        SemiColon,          // ;
        Dot,                // .
        Colon,              // :
        Question,           // ?
        Bar,                // |
        Amper,              // &
        Exclamation,        // !
        Tilde,              // ~
        
        PlusPlus, MinusMinus, // ++, --

        Plus, Minus, Star, Slash, Percent, Caret,  // + - * / % ^
        EqualEqual, NotEqual, Less, LessEqual, Greater, GreaterEqual, // == != < <= > >=
        AmperAmper, BarBar, // && || !        
        LessLess, GreaterGreater, // << >>

        PlusEqual, MinusEqual, StarEqual, SlashEqual, PercentEqual, // +=, -=, *=, /=, %=
        LessLessEqual, GreaterGreaterEqual, // <<= >>=
        AmperEqual, CaretEqual, BarEqual, // &= ^= |=
        Equal,  // =
        

        // 한 종류가 여러가지 값을 가질 수 있는 것들
        Identifier,                                       // [a-zA-Z_][a-zA-Z0-9_]*
        TrueValue, FalseValue,                            // true, false
        IntValue, StringValue,                            // [0-9], "st\\ri\"ng" @"a""b\" <- 우선순위 밀림..

        Return,

        // 기타 keywords.. 
        If, Else, For, While, Do,
        Break, Continue,

        Class, Public, Private, Protected, Virtual, Override,          // class 용

        Out, Params,
        New,

        Using, Namespace, Static
    }
}
