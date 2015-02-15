using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Translator.Parser;

namespace Gum.Test.ParserGenerator
{
    [TestClass]
    public class ParserGerneratorTest
    {
        // Token = TokenRule * Data        
        public RegexTokenType STRING = new RegexTokenType("STRING", @"\""([^\\""]|\\"")*""");
        public RegexTokenType REGEX = new RegexTokenType("REGEX", @"r\""([^\\""]|\\"")*""");
        public StringTokenType STAR = new StringTokenType("STAR", "*");
        public StringTokenType PLUS = new StringTokenType("PLUS", "+");
        public StringTokenType QUESTION = new StringTokenType("QUESTION", "?");
        public StringTokenType EQUAL = new StringTokenType("EQUAL", "=");
        public StringTokenType LPAREN = new StringTokenType("LPAREN", "(");
        public StringTokenType RPAREN = new StringTokenType("RPAREN", ")");
        public StringTokenType LBRACE = new StringTokenType("LBRACE", "{");
        public StringTokenType RBRACE = new StringTokenType("RBRACE", "}");
        public StringTokenType LBRACKET = new StringTokenType("LBRACKET", "[");
        public StringTokenType RBRACKET = new StringTokenType("RBRACKET", "]");
        public StringTokenType LESSTHAN = new StringTokenType("LESSTHAN", "<");
        public StringTokenType BAR = new StringTokenType("BAR", "|");
        public StringTokenType COMMA = new StringTokenType("COMMA", ",");
        public RegexTokenType ID = new RegexTokenType("ID", @"[_a-zA-Z][_a-zA-Z0-9]*");

        public NonTerminal Spec = new NonTerminal("Spec");
        public NonTerminal Decl = new NonTerminal("Decl");
        public NonTerminal TokenExp = new NonTerminal("TokenExp");
        public NonTerminal RuleExp = new NonTerminal("RuleExp");
        public NonTerminal PrecExp = new NonTerminal("PrecExp");

        public string code = @"
// Tokens
TRUE = ""true""
FALSE = ""false""

DECIMAL = r""[0-9]+""

ID = r""[_a-zA-Z][_a-zA-Z0-9]*""

BARBAR = ""||""
AMPAMP = ""&&""
BAR = ""|""
CARET = ""^""
AMP = ""&""
EQUALEQUAL = ""==""
EXCLEQUAL = ""!=""
LESSTHAN = ""<""
GREATERTHAN = "">""
LESSEQUAL = ""<=""
GREATEREQUAL = "">=""
LESSTHANLESSTHAN = ""<<""
GREATERTHANGREATERTHAN = "">>""

PLUS = ""+""
MINUS = ""-""
STAR = ""*""
SLASH = ""/""
PERCENT = ""%""
EXCL = ""!""
TILDE = ""~""
LPAREN = ""(""
RPAREN = "")""

// 스크립트 모듈
ScriptModule = VarDecl*

VarName(ID varName) { varName } 
VarNameExp(ID varName, Exp exp) { varName EQUAL exp }

// a . = 0, 
// VarName = varName .
// VarNameExp = varName . EQUAL exp <-

VarNameOptExp = 
    | VarName 
    < VarNameExp 

// 변수 선언
VarDecl(ID typeName, VarNameOptExp[] varNameOptExp)
{
	typeName varNameOptExp (COMMA varNameOptExp) * SEMICOLON
}

// Expression

// Logical
CondOr(Exp e1, Exp e2) { e1 BARBAR e2 }
CondAnd(Exp e1, Exp e2) { e1 AMPAMP e2 }
LogicalOr(Exp e1, Exp e2) { e1 BAR e2 }
LogicalXor(Exp e1, Exp e2) { e1 CARET e2 }
LogicalAnd(Exp e1, Exp e2) { e1 AMP e2 }

Equal(Exp e1, Exp e2) { e1 EQUALEQUAL e2 }
NotEqual(Exp e1, Exp e2) { e1 EXCLEQAUL e2 }

LessThan(Exp e1, Exp e2) { e1 LESSTHAN e2 }
GreaterThan(Exp e1, Exp e2) { e1 GREATERTHAN e2 }
LessEqual(Exp e1, Exp e2) { e1 LESSEQUAL e2 }
GreaterEqual(Exp e1, Exp e2) { e1 GREATEREQUAL e2 }

ShiftLeft(Exp e1, Exp e2) { e1 LESSTHANLESSTHAN e2 }
ShiftRight(Exp e1, Exp e2) { e1 GREATERTHANGREATERTHAN e2 }

Add(Exp e1, Exp e2) { e1 PLUS e2 }
Sub(Exp e1, Exp e2) { e1 MINUS e2 }

Mul(Exp e1, Exp e2) { e1 STAR e2 }
Div(Exp e1, Exp e2) { e1 SLASH e2 } 
Rem(Exp e1, Exp e2) { e1 PERCENT e2 } 
    
UnaryPlus(Exp e) { PLUS e } 
UnaryMinus(Exp e) { MINUS e }
LogicalNeg(Exp e) { EXCL e }
BitwiseCompl(Exp e) { TILDE e }

ParenExp(Exp e) { LPAREN e RPAREN }

BooleanLiteral = TRUE | FALSE
IntegerLiteral = INTEGER

Literal = 
    | BooleanLiteral
    | IntegerLiteral

// 복합 룰
// C#을 따라갑니다
// + - * / % ^ && || 
// http://msdn.microsoft.com/en-us/library/aa691323(v=vs.71).aspx
Exp = 
    // TODO: Assignment
    < CondOr[Left]
    < CondAnd[Left]
    < LogicalOr[Left]
    < LogicalXor[Left]
    < LogicalAnd[Left]

    // Equality
    < Equal[Left] | NotEqual[Left]

    // Relation and testing 
    < LessThan[Left] | GreaterThan[Left] | LessEqual[Left] | GreaterEqual[Left] // ( is as )

    // Consume
    < ShiftLeft[Left] | ShiftRight[Left]

    // Additive
    < Add[Left] | Sub[Left]

    // Multiplicative
    < Mul[Left] | Div[Left] | Rem[Left]
    
    // Unary
    < UnaryPlus | UnaryMinus | LogicalNeg | BitwiseCompl

    // TODO: Primary
    | Literal



";

        public ParserGerneratorTest()
        {
            var SpecRule = new Rule("SpecRule", Spec, new PlusExp(new SymbolExp(Decl, "decl")));
            var TokenDecl = new Rule("TokenDecl", Decl, new SequenceExp(new SymbolExp(ID, "name"), new SymbolExp(EQUAL), new SymbolExp(TokenExp, "exp")));

            var StringExp = new Rule("StringExp", TokenExp, new SymbolExp(STRING, "s"));
            var RegexExp = new Rule("RegexExp", TokenExp, new SymbolExp(REGEX, "r"));

            var RuleDecl = new Rule("RuleDecl", Decl, new SequenceExp(
                new SymbolExp(ID, "reducedSymbol"), 
                new SymbolExp(ID, "ruleName"), 
                new SymbolExp(LPAREN), 
                new OptionalExp(new SequenceExp(
                    new SymbolExp(ID, "captureSymbol"), 
                    new SymbolExp(ID, "captureVar"), 
                    new StarExp(new SequenceExp(
                        new SymbolExp(COMMA), 
                        new SymbolExp(ID, "captureSymbol"), 
                        new SymbolExp(ID, "captureVar"))))), 
                new SymbolExp(RPAREN),
                new SymbolExp(LBRACE),
                new SymbolExp(RuleExp, "ruleExp"),
                new SymbolExp(RBRACE)));

            var SequenceExp = new Rule("SequenceExp", RuleExp, new PlusExp(new SymbolExp(RuleExp, "exp")));
            var OneOrMoreExp = new Rule("OneOrMoreExp", RuleExp, new SequenceExp(new SymbolExp(RuleExp, "exp"), new SymbolExp(PLUS)));
            var ZeroOrMoreExp = new Rule("ZeroOrMoreExp", RuleExp, new SequenceExp(new SymbolExp(RuleExp, "exp"), new SymbolExp(STAR)));
            var OptionalExp = new Rule("OptionalExp", RuleExp, new SequenceExp(new SymbolExp(RuleExp, "exp"), new SymbolExp(QUESTION)));

            var ParenExp = new Rule("ParenExp", RuleExp, new SequenceExp( new SymbolExp(LPAREN), new SymbolExp(RuleExp, "exp"), new SymbolExp(RPAREN)));
            var BarExp = new Rule("BarExp", RuleExp, new SequenceExp(new SymbolExp(RuleExp, "e1"), new SymbolExp(BAR), new SymbolExp(RuleExp, "e2")));
            var SymbolExp = new Rule("SymbolExp", RuleExp, new SymbolExp(ID, "id"));
            var PrecDecl = new Rule("PrecDecl", Decl, new SymbolExp(PrecExp, "exp"));
            var Entry = new Rule("Entry", PrecExp, new SequenceExp(
                new SymbolExp(LBRACE),
                new SymbolExp(ID, "rule"),
                new StarExp(new SequenceExp(
                    new SymbolExp(COMMA),
                    new SymbolExp(ID, "rule"))),
                new SymbolExp(RBRACE),
                new OptionalExp(new SequenceExp(
                    new SymbolExp(LBRACKET),
                    new SymbolExp(ID, "assoc"),
                    new SymbolExp(RBRACKET)
                    ))));

            var LessThan = new Rule("LessThan", PrecExp, new SequenceExp(
                new SymbolExp(PrecExp, "e1"),
                new SymbolExp(LESSTHAN),
                new SymbolExp(PrecExp, "e2")
                ));

            var rules = new RuleSet(SpecRule, TokenDecl, StringExp, RegexExp, RuleDecl, SequenceExp, OneOrMoreExp, OptionalExp, ZeroOrMoreExp, ParenExp, BarExp, SymbolExp, PrecDecl, Entry, LessThan );
            Gum.Translator.Parser.Parser.Accept2(rules, 
        }

        [TestMethod]
        public void DefaultTest()
        {

            //parser.Parse(code);
        }
    }
}
