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

    public class Parser : IExpVisitor<IEnumerable<ConsumerContext>, ConsumerContext>
    {
        RuleSet rules;
        HashSet<ASTStack> visited = new HashSet<ASTStack>();

        public Parser(RuleSet rules)
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
            foreach (var output in Consume(exps[index], input))
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
            if (input.Input == ASTStack.Empty)
                yield break;

            var peek = input.Input.Peek();

            // shift 불가능하면
            if (symbolExp.Symbol != peek.Symbol)
            {
                ASTStack output;
                Rule rule;

                if (!Reduce(input.Input, out output, out rule))
                    yield break;

                foreach (var output2 in Consume(symbolExp, new ConsumerContext(output, input.Captures, input.Rule)))
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



        public bool Parse(IEnumerable<TokenNode> inputs, out ASTNode result)
        {
            result = null;

            var inputStack = ASTStack.Empty;
            foreach (var tokenNode in inputs.Reverse())
                inputStack = inputStack.Push(tokenNode);                
            
            PrintStack(inputStack);

            ASTStack outputStack;
            Rule rule;
            while (Reduce(inputStack, out outputStack, out rule))
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
            while (!outputStack.IsEmpty)
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
    }
}
