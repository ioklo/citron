using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Immutable;

namespace Gum.Translator.Parser
{
    using ASTStack = ImmutableStack<ASTNode>;
    using ASTCapture = ImmutableList<Tuple<String, ASTNode>>;

    public class Parser
    {
        public struct ConsumerContext
        {
            public ASTStack Input { get; private set; }
            public ASTCapture Captures { get; private set; }
            public Rule Rule { get; private set; }
            public ConsumerContext(ASTStack input, ASTCapture captures, Rule rule)
                : this()
            { 
                Input = input;
                Captures = captures;
                Rule = rule;
            }
        }

        class Consumer : IExpVisitor<IEnumerable<ConsumerContext>, ConsumerContext>
        {
            RuleSet rules;
            HashSet<ASTStack> visited = new HashSet<ASTStack>();

            public Consumer(RuleSet rules)
            {
                this.rules = rules;

            }

            public IEnumerable<ConsumerContext> Consume(Exp exp, ConsumerContext input)
            {
                return exp.Visit(this, input);
            }

            public IEnumerable<ConsumerContext> Visit(StarExp starExp, ConsumerContext input)
            {
                foreach (var output in Consume(starExp.E, input))
                    foreach (var output2 in Consume(starExp, output))
                        yield return output2;

                yield return input;
            }

            public IEnumerable<ConsumerContext> Visit(PlusExp plusExp, ConsumerContext input)
            {
                foreach (var output in Consume(plusExp.E, input))
                {
                    foreach (var output2 in Consume(plusExp, output)) 
                        yield return output2;

                    yield return output;
                }
            }

            public IEnumerable<ConsumerContext> Visit(OptionalExp optionalExp, ConsumerContext input)
            {
                foreach (var output in Consume(optionalExp.E, input))
                    yield return output;

                yield return input;
            }

            private IEnumerable<ConsumerContext> VisitSeqExp(int index, IReadOnlyList<Exp> exps, ConsumerContext input)
            {
                foreach(var output in Consume(exps[index], input))
                {
                    if (index + 1 == exps.Count)
                    {
                        yield return output;
                        continue;
                    }

                    foreach (var output2 in VisitSeqExp(index + 1, exps, output))
                        yield return output2;
                }
            }

            public IEnumerable<ConsumerContext> Visit(SequenceExp exp, ConsumerContext input)
            {
                // 0검사 안함
                return VisitSeqExp(0, exp.Children, input);
            }

            public IEnumerable<ConsumerContext> Visit(SymbolExp symbolExp, ConsumerContext input)
            {
                if( input.Input == ASTStack.Empty) 
                    yield break;

                var peek = input.Input.Peek();

                // shift 불가능하면
                if( symbolExp.Symbol != peek.Symbol )
                {
                    ASTStack output;
                    Rule rule;

                    if( !Reduce(input.Input, out output, out rule) )
                        yield break;

                    foreach(var output2 in Consume(symbolExp, new ConsumerContext(output, input.Captures, input.Rule)))
                        yield return output2;

                    yield break;
                }
                else
                {
                    ASTStack output;
                    Rule rule;
                    Rule curRule = input.Rule;

                    if (Reduce(input.Input, out output, out rule))
                    {
                        int? compareResult = rules.Compare(curRule, rule);

                        // 두 룰의 순위를 정할 수 없다면 둘다 가 보기
                        if (!compareResult.HasValue)
                        {
                            foreach (var output2 in Consume(symbolExp, new ConsumerContext(output, input.Captures, input.Rule)))
                                yield return output2;
                                                        
                            // throw new Exception("Shift-Reduce conflict");
                        }
                        // 지금 rule보다 큰 rule을 적용시켰다면 
                        else if (compareResult < 0)
                        {
                            foreach (var output2 in Consume(symbolExp, new ConsumerContext(output, input.Captures, input.Rule)))
                                yield return output2;
                            yield break;
                        }
                        else if (compareResult == 0)
                        {
                            if (curRule.Assoc != rule.Assoc)
                                throw new Exception("비교하는 두 룰의 Associativity가 같지 않습니다");

                            if (curRule.Assoc == Associativity.None)
                                throw new Exception("Associativity가 없는 룰에서 shift-reduce conflict 가 났습니다");

                            // Right면 output을 씁니다
                            if (curRule.Assoc == Associativity.Right)
                            {
                                foreach (var output2 in Consume(symbolExp, new ConsumerContext(output, input.Captures, input.Rule)))
                                    yield return output2;
                                yield break;
                            }
                        }
                    }

                    // shift
                    var captures = input.Captures;
                    if (symbolExp.CaptureVar != null)
                    {
                        var capture = Tuple.Create(symbolExp.CaptureVar, peek);
                        captures = input.Captures.Add(capture);
                    }

                    yield return new ConsumerContext(input.Input.Pop(), captures, input.Rule);
                }
            }
            
            // Reduce - Reduce conflict는 이미 해결됐다고 생각한다;
            public bool Reduce(ASTStack input, out ASTStack output, out Rule appliedRule)
            {
                output = null;
                appliedRule = null;

                if (input == ASTStack.Empty) return false;

                // visited 확인, 이건 cache와 다름.. reduce 안에서 같은 reduce가 일어날 일을 방지
                if (visited.Contains(input)) return false;
                visited.Add(input);

                try
                {
                    int inputCount = input.Count();
                    var reducedInputs = new List<ConsumerContext>();

                    foreach (var rule in rules.GetAvailableRules(input.Peek().Symbol))
                    {
                        // 같은 룰 안에서 인풋을 제일 많이 받아들인
                        int minInputCount = -1;
                        var minContexts = new List<ConsumerContext>();
                        var context = new ConsumerContext(input, ASTCapture.Empty, rule);
                        foreach (var newContext in rule.Exp.Visit(this, context))
                        {
                            int curInputCount = newContext.Input.Count();

                            if (minContexts.Count == 0)
                            {
                                minInputCount = curInputCount;
                                minContexts.Add(newContext);
                                continue;
                            }

                            if (curInputCount < minInputCount)
                            {
                                minInputCount = curInputCount;
                                minContexts.Clear();
                                minContexts.Add(newContext);
                                continue;
                            }

                            if (curInputCount == minInputCount)
                            {
                                minContexts.Add(newContext);
                                continue;
                            }
                        }

                        if (minContexts.Count == 0)
                            continue;

                        if (1 < minContexts.Count)
                            throw new Exception("같은 길이의 입력 시퀀스에 대해서 여러가지 표현이 가능합니다");

                        var curReducedInput = minContexts[0];

                        if (reducedInputs.Count == 0)
                        {
                            reducedInputs.Add(curReducedInput);
                            continue;
                        }

                        // compare 불가능한 것이 껴 있다면 바로
                        int? compareResult = rules.Compare(rule, reducedInputs[0].Rule);
                        if (!compareResult.HasValue)
                            throw new Exception("reduce-reduce conflict");

                        if (compareResult == 0)
                            reducedInputs.Add(curReducedInput);
                        else if (compareResult < 0)
                        {
                            reducedInputs.Clear();
                            reducedInputs.Add(curReducedInput);
                        }
                    }

                    if (reducedInputs.Count == 0) return false;
                    if (1 < reducedInputs.Count)
                        throw new Exception("Reduce-Reduce conflict");

                    var captureNode = new CaptureNode(reducedInputs[0].Rule);
                    foreach (var captureEntry in reducedInputs[0].Captures)
                        captureNode.Add(captureEntry.Item1, captureEntry.Item2);

                    output = reducedInputs[0].Input.Push(captureNode);
                    appliedRule = reducedInputs[0].Rule;
                    return true;
                }
                finally
                {
                    visited.Remove(input);
                }
            }
        }

        static public bool Accept2(RuleSet rules, TokenNode[] input, out ASTNode result )
        {
            result = null;

            var inputStack = ASTStack.Empty;
            for (int t = input.Length - 1; t >= 0; t--)
                inputStack = inputStack.Push(input[t]);

            var consumer = new Consumer(rules);

            PrintStack(inputStack);

            ASTStack outputStack;
            Rule rule;
            while (consumer.Reduce(inputStack, out outputStack, out rule))
            {
                PrintStack(outputStack);

                if (outputStack.Count() == 1)
                {
                    result = outputStack.Peek();
                    return true;
                }

                inputStack = outputStack;
            }

            return false;
        }

        private static void PrintStack(ASTStack outputStack)
        {
            bool bFirst = true;
            Console.Write("[");
            while(!outputStack.IsEmpty)
            {
                if (bFirst)
                {
                    Console.Write("{0}", outputStack.Peek().Symbol);
                    bFirst = false;
                }
                else
                {
                    Console.Write(", {0}", outputStack.Peek().Symbol);
                }
                outputStack = outputStack.Pop();
            }
            Console.WriteLine("]");
        }

        static public bool Accept(RuleSet rules, TokenNode[] input, out ASTNode result)
        {
            result = null;

            var inputStack = new Stack<ASTNode>();
            for (int t = input.Length - 1; t >= 0; t--)
                inputStack.Push(input[t]);

            // TraceSet과 ASTNode를 저장할 공간
            var shiftStack = new ShiftStack();

            // 시작은 curValue
            while (true)
            {
                var curValue = inputStack.Peek();

                // Trace 단위로 가져온다
                List<Trace> availableTraces = new List<Trace>();
                availableTraces.AddRange(shiftStack.GetAvailableTraces(curValue.Symbol));
                availableTraces.AddRange(rules.GetAvailableTraces(curValue.Symbol));

                // 
                List<Trace> shiftableRules, reducibleRules;
                if (!ClassifyRules(rules, availableTraces, out shiftableRules, out reducibleRules))
                    return false;

                // 1. shiftRules가 있다면 shift
                if (0 < shiftableRules.Count)
                {
                    shiftStack.Shift(shiftableRules, curValue);
                    inputStack.Pop();
                }

                // 2. reducibleRules가 1개만 있다면 그걸 선택합니다.
                else if (reducibleRules.Count == 1)
                {
                    ASTNode node;
                    if (!shiftStack.Reduce(reducibleRules[0], out node))
                        return false;

                    inputStack.Push(node);
                }
                // 3. 나머지는 불가능..
                else
                    return false;
            }

            // TODO: curValue 하나 남았는지 확인
            // result = shiftStack.Stack.Peek().Node;
            // return true;
        }

        private static bool ClassifyRules(RuleSet rules, List<Trace> traces, out List<Trace> shiftableTraces, out List<Trace> reducibleTraces)
        {
            shiftableTraces = new List<Trace>(); // has same priorities
            reducibleTraces = new List<Trace>();

            foreach (var trace in traces)
            {
                if (trace.IsShiftable)
                {
                    if (reducibleTraces.Count == 0)
                        shiftableTraces.Add(trace);
                    else
                    {
                        int? result = rules.Compare(reducibleTraces[0].Position.Rule, trace.Position.Rule);
                        if( !result.HasValue ) 
                        {
                            shiftableTraces = null;
                            reducibleTraces = null;
                            return false;
                        }

                        // reducibleTrace보다 크다면
                        if (result < 0) shiftableTraces.Add(trace);
                    }
                }

                else if (trace.IsReducible)
                {
                    if (reducibleTraces.Count == 0)
                        reducibleTraces.Add(trace);

                    else 
                    {
                        int? result = rules.Compare(reducibleTraces[0].Position.Rule, trace.Position.Rule);

                        if(result.HasValue && result < 0)
                        {
                            reducibleTraces.Clear();
                            reducibleTraces.Add(trace);

                            var newShiftableTraces = new List<Trace>();
                            foreach (var shiftableTrace in shiftableTraces)
                            {
                                result = rules.Compare(trace.Position.Rule, shiftableTrace.Position.Rule);
                                if (!result.HasValue)
                                {
                                    shiftableTraces = null;
                                    reducibleTraces = null;
                                    return false;
                                }

                                if (result < 0)
                                    newShiftableTraces.Add(shiftableTrace);
                            }
                            shiftableTraces = newShiftableTraces;
                        }
                    }
                }
                else
                {
                    shiftableTraces = null;
                    reducibleTraces = null;
                    return false;
                }
            }

            return true;
        }

    }



    //public class Parser
    //{
    //    List<TokenRule> tokenRules = new List<TokenRule>();
    //    List<Rule> rules = new List<Rule>();

    //    public void AddRules(params Rule[] rules)
    //    {
    //        tokenRules.AddRange(from rule in rules where (rule is TokenRule) select rule as TokenRule);
    //        this.rules.AddRange(rules);
    //    }

        // string -> string
        public string PreprocessComment(string input)
        {
            string ret = Regex.Replace(input, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            return Regex.Replace(ret, @"//[^\n]*\n", " ");
        }

        private int FindNextTokenStart(string input, int start)
        {
            while (start < input.Length)
            {
                if (!char.IsWhiteSpace(input[start])) break;
                start++;
            }

            return start;
        }

        // 현재 delimeter = \s 
        public IEnumerable<Token> Tokenizer(string input)
        {            
            int cur = FindNextTokenStart(input, 0);            

            while( cur < input.Length )
            {
                // rule 중에서 만족하는 input을 찾는다
                bool bFound = false;
                foreach(var tokenRule in tokenRules)
                {
                    int next;
                    if( tokenRule.Accept(input, cur, out next) )
                    {
                        yield return new Token(tokenRule, input.Substring(cur, next - cur));
                        cur = FindNextTokenStart(input, next);
                        bFound = true;
                        break;
                    }
                }

                if (!bFound) throw new Exception();
            }
        }

    //    public AST Parse(string input, Rule startRule)
    //    {
    //        var preprocessedInput = PreprocessComment(input);

    //        // 현재 State 머신

    //        // Shift, Reduce // 
    //        // Shift의 결과는 여러개로 나눠질 수 있다.

    //        // Reduce 

    //        // Some State, Input 

    //        // Nondeterministic State machine 

    //        // State = Rule * Position
    //        // Stack<State Set/ State Set>            
    //        Stack<StateSet> stack = InitiailzeStack(startRule);
            
    //        foreach(Token token in Tokenizer(preprocessedInput))
    //        {
    //            Progress(stack, token);
    //        }
            
    //        return null;            
    //        // Token
    //    }

    //    private Stack<StateSet> InitiailzeStack(Rule startRule)
    //    {            
    //        // State = Rule * Location       
    //        // 
    //        // GetNextStateSet(State, TokenRule) = StateSet
    //        // State.GetNextStates(TokenRule)
    //        // A B ( C D ) *
    //        // A? B C D
            


    //        // State는 .. SequenceRule만 가져야 합니다
    //        var stack = new Stack<StateSet>();

    //        // startRule에서부터 출발하는 모든 것들
    //        startRule.GetStateSetAtFirst();  

    //        return stack;
    //    }

    //    private void Progress(Stack<StateSet> stack, Token token)
    //    {
    //        // Rule = E

    //        // S = 
    //        // E = . E + 

    //    }
    //}
}
