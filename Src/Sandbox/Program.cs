using Gum.Translator.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Sandbox
{
    class Program
    {
        // Rules....

        // Rule = RuleName, Exp(root)
        //   - TokenRule
        //   - CompositeRule
        //   - CaptureRule
        // 
        // RuleSet (First -> CaptureRule)
        //// 

        //static void Print(ShiftStackItem stackItem)
        //{
        //    foreach(var trace in stackItem.Traces)
        //        Console.WriteLine(pos.ToString());

        //    Console.WriteLine("-------------------");
        //}

        // 
        //static void Traverse(Exp exp)
        //{
        //    ShiftStackItem posSet = ShiftStackItem.Create(exp);

        //    while(true)
        //    {
        //        Print(posSet);
        //        Console.ReadLine();
        //        posSet = posSet.Advance();
        //    }
        //}

        // 
        //static void Consume(Exp root, IEnumerable<TokenRule> tokenRules)
        //{
        //    ShiftStackItem posSet = ShiftStackItem.Create(root);
        //    Print(posSet);

        //    foreach (var token in tokenRules)
        //    {   
        //        Console.WriteLine(token.ID);
        //        Console.ReadLine();

        //        posSet = posSet.Consume(new TokenNode(token, string.Empty));
        //        posSet = posSet.Advance();
        //        Print(posSet);
        //    }
        //}

        
        

        

        static void Main(string[] args)
        {
            CalcTest test = new CalcTest();           
            

            //var A1Token = new StringTokenType("A1", "A1");
            //var A2Token = new StringTokenType("A2", "A2");
            //var WToken = new StringTokenType("W", "W");
            //var BToken = new StringTokenType("B", "B");
            //var XToken = new StringTokenType("X", "X");
            //var YToken = new StringTokenType("Y", "Y");
            //var DToken = new StringTokenType("D", "D");

            //var A1 = new TokenRule(A1Token);
            //var A2 = new TokenRule(A2Token);
            //var W = new TokenRule(WToken);
            //var B = new TokenRule(BToken);
            //var X = new TokenRule(XToken);
            //var Y = new TokenRule(YToken);
            //var D = new TokenRule(DToken);

            //// (A1 A2)? W+ B (X Y) D
            //var exp =
            //    new SeqExp(
            //        new OptionalExp(
            //            new SeqExp(new RuleExp(A1), new RuleExp(A2))),
            //        new PlusExp(new RuleExp(W)),
            //        new RuleExp(B),
            //        new SeqExp(new RuleExp(X), new RuleExp(Y)),
            //        new RuleExp(D));

            //Consume(exp, new[] { A1, A2, W, B, X, Y, D });
            //Consume(exp, new[] { W, W, W, B, X, Y, D });

            // Traverse(exp);
        }
    }
}
