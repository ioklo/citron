using Gum.Translator.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Sandbox
{
    class CalcTest
    {
        // Rule(Add) E -> E + E
        // Symbol(E, DIGIT) E
        // Symbol: Terminal(Token), NonTerminal(Exp)        
        // Terminal - RegexToken, StringToken

        RegexTokenType DIGIT = new RegexTokenType("DIGIT", "[1-9][0-9]*");
        StringTokenType PLUS = new StringTokenType("PLUS", "+");
        StringTokenType MINUS = new StringTokenType("MINUS", "-");
        StringTokenType STAR = new StringTokenType("STAR", "*");
        StringTokenType SLASH = new StringTokenType("SLASH", "/");
        StringTokenType LPAREN = new StringTokenType("LPAREN", "(");
        StringTokenType RPAREN = new StringTokenType("RPAREN", ")");

        RuleSet ruleSet;
        TokenNode[] tokens;

        // Terminal
        // CompositeRule -> NonTerminal
        // TokenRule -> Terminal
        // CaptureRule -> Rule
        // RuleExp -> SymbolExp

        public CalcTest()
        {
            var Exp = new NonTerminal("Exp");

            var Paren = new Rule("Paren", Exp, new SequenceExp(new SymbolExp(LPAREN), new SymbolExp(Exp, "e"), new SymbolExp(RPAREN)));
            var Add = new Rule("Add", Exp, new SequenceExp(new SymbolExp(Exp, "e1"), new SymbolExp(PLUS), new SymbolExp(Exp, "e2")), Associativity.Left);
            var Sub = new Rule("Sub", Exp, new SequenceExp(new SymbolExp(Exp, "e1"), new SymbolExp(MINUS), new SymbolExp(Exp, "e2")), Associativity.Left);
            var Mul = new Rule("Mul", Exp, new SequenceExp(new SymbolExp(Exp, "e1"), new SymbolExp(STAR), new SymbolExp(Exp, "e2")), Associativity.Left);
            var Div = new Rule("Div", Exp, new SequenceExp(new SymbolExp(Exp, "e1"), new SymbolExp(SLASH), new SymbolExp(Exp, "e2")), Associativity.Left);
            var Digit = new Rule("Digit", Exp, new SymbolExp(DIGIT, "d"));
            
            ruleSet = new RuleSet(Paren, Add, Sub, Mul, Div, Digit);

            ruleSet.AddOrder(Add, Sub);
            ruleSet.AddOrder(Mul, Div);

            tokens = new[] { 
                new TokenNode(DIGIT, "1"), 
                new TokenNode(PLUS, "+"), 
                new TokenNode(DIGIT, "2"), 
                new TokenNode(PLUS, "+"), 
                new TokenNode(DIGIT, "3") };

            var tokens2 = new[] { 
                new TokenNode(LPAREN, "("),
                new TokenNode(DIGIT, "1"), 
                new TokenNode(PLUS, "+"), 
                new TokenNode(DIGIT, "2"), 
                new TokenNode(RPAREN, ")"),
                new TokenNode(STAR, "*"), 
                new TokenNode(DIGIT, "3") };

            ASTNode result;
            Parser.Accept2(ruleSet, tokens, out result);

            Console.WriteLine(result.ToString(""));
        }
    }
}
