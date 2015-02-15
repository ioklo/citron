using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Translator.Parser;

namespace Gum.SandBox
{
    public class SelfParser
    {
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
// TokenDecl
ID = r""[_a-zA-Z][_a-zA-Z0-9]*""
STRING = r""\""([^\]|\"")*\""""
REGEX = r""r\""([^\]|\"")*\""""
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
    RBRACE}
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

        public SelfParser()
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
            
            
            
            // Gum.Translator.Parser.Parser.Accept2(rules,  
        }

        public void DefaultTest()
        {

            //parser.Parse(code);
        }
    }
}
