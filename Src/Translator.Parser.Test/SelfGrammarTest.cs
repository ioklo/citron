using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Translator.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gum.Translator.Parser.Test
{
    [TestClass]
    public class SelfGrammarTest
    {
        [TestMethod]
        public void SelfGrammarTestMain()
        {
            // Multiline comment는 다음줄에
            // string ret = Regex.Replace(input, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            RegexTokenType COMMENT = new RegexTokenType("COMMENT", @"//[^\n]*\n");
            RegexTokenType WHITESPACE = new RegexTokenType("WHITESPACE", @"\s+");

            RegexTokenType STRING = new RegexTokenType("STRING", @"""([^\""]|\""|\\)*""");
            RegexTokenType REGEX = new RegexTokenType("REGEX",  @"r""([^\""]|\""|\\)*""");
            StringTokenType STAR = new StringTokenType("STAR", "*");
            StringTokenType PLUS = new StringTokenType("PLUS", "+");
            StringTokenType QUESTION = new StringTokenType("QUESTION", "?");
            StringTokenType EQUAL = new StringTokenType("EQUAL", "=");
            StringTokenType LPAREN = new StringTokenType("LPAREN", "(");
            StringTokenType RPAREN = new StringTokenType("RPAREN", ")");
            StringTokenType LBRACE = new StringTokenType("LBRACE", "{");
            StringTokenType RBRACE = new StringTokenType("RBRACE", "}");
            StringTokenType LBRACKET = new StringTokenType("LBRACKET", "[");
            StringTokenType RBRACKET = new StringTokenType("RBRACKET", "]");
            StringTokenType LESSTHAN = new StringTokenType("LESSTHAN", "<");
            StringTokenType BAR = new StringTokenType("BAR", "|");
            StringTokenType COMMA = new StringTokenType("COMMA", ",");
            RegexTokenType ID = new RegexTokenType("ID", @"[_a-zA-Z][_a-zA-Z0-9]*");

            NonTerminal Spec = new NonTerminal("Spec");
            NonTerminal Decl = new NonTerminal("Decl");
            NonTerminal TokenExp = new NonTerminal("TokenExp");
            NonTerminal RuleExp = new NonTerminal("RuleExp");
            NonTerminal PrecExp = new NonTerminal("PrecExp");

            string code = @"
// TokenDecl
ID = r""[_a-zA-Z][_a-zA-Z0-9]*""
STRING = r""\""([^\\\""]|\\""|\\\\)*\""""
REGEX = r""r\""([^\\\""]|\\""|\\\\next)*\""""
STAR = ""*""
PLUS = ""+""
QUESTION = ""?""
EQUAL = ""=""
LPAREN = ""(""
RPAREN = "")""
LBRACE = ""{""
RBRACE = ""}""
LBRACKET = ""[""
RBRACKET = ""]""
LESSTHAN = ""<""
BAR = ""|""

Spec Spec(Decl decl) { decl+ }
Decl TokenDecl(ID name, TokenExp exp) { name EQUAL exp }
TokenExp StringExp(STRING s) { s }
TokenExp RegexExp(REGEX r) { r }
Decl RuleDecl(ID reducedSymbol, ID ruleName, ID captureSymbol, ID captureVar, RuleExp ruleExp)
{
    reducedSymbol ruleName LPAREN (captureSymbol captureVar (COMMA captureSymbol captureVar)*)? RPAREN
    LBRACE
        ruleExp
    RBRACE
}
RuleExp SequenceExp(RuleExp exp) { exp+ }
RuleExp OneOrMoreExp(RuleExp exp) { exp PLUS }
RuleExp ZeroOrMoreExp(RuleExp exp) { exp STAR }
RuleExp OptionalExp(RuleExp exp) { exp QUESTION }
RuleExp ParenExp(RuleExp exp) { LPAREN exp RPAREN }
RuleExp BarExp(RuleExp e1, RuleExp e2) { e1 BAR e2 }
RuleExp SymbolExp(ID id) { id }
Decl PrecDecl(PrecExp exp) { exp }
PrecExp Entry(ID rule, ID assoc) { LBRACE rule (COMMA rule)* RBRACE (LBRACKET assoc RBRACKET)? }
PrecExp LessThan(PrecExp e1, PrecExp e2) { e1 LESSTHAN e2 }

{Entry} < {LessThan}[Left]
";      
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

            rules.AddOrder(TokenDecl);
            rules.AddOrder(SymbolExp);
            

            var lexer = new Lexer();
            lexer.AddSkipToken(COMMENT, WHITESPACE);
            lexer.AddToken(STRING, REGEX, STAR, PLUS, QUESTION, EQUAL, LPAREN, RPAREN, LBRACE, RBRACE, 
                LBRACKET, RBRACKET, LESSTHAN, BAR, COMMA, ID);

            var parser = new Parser(rules);

            ASTNode result;
            parser.Parse(lexer.Lex(code), out result);
        }
    }
}
