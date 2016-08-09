using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;
using System.Linq;

namespace Gum.Evaluator
{
    partial class ASTEvaluator
    {
        class RuntimeException : Exception
        {
            public RuntimeException(string msg)
            {
            }
        }

        interface IFunction
        {
            IReadOnlyList<string> ParamNames { get; }
            IStmtComponent Body { get; }
            IType ReturnType { get; }
        }

        class Method : IFunction
        {
            public IReadOnlyList<string> ParamNames { get; private set; }
            public IStmtComponent Body { get; private set; }
            public IType ReturnType { get; private set; }

            public Method(IType returnType, IReadOnlyList<string> paramNames, IStmtComponent body)
            {
                ParamNames = paramNames.ToList();
                Body = body;
                ReturnType = returnType;
            }
        }

        interface IType
        {
            bool CanConvertTo(IType targetType);

            // 현재 타입의 인스턴스 멤버(함수와 변수), 정적 멤버(함수와 변수)를 검색합니다.. 
            // private인지 public인지 구분하지 않습니다..
            bool TryGetMemberType(out IType type, string memberName);
            bool TryGetMemberType(out IType type, string memberName, IReadOnlyList<IType> typeArgs, IReadOnlyList<IType> argTypes);
        }

        class FuncType : IType
        {
            public IType ReturnType { get; private set; }
            public IReadOnlyList<IType> ArgTypes { get; private set; }
        }
        
        // Exp를 실행한 결과물
        abstract class Value
        {
            public IType Type { get; private set; }
            public virtual void SetValue(Value value)
            {
                throw new RuntimeException("대입을 지원하지 않는 값에 대입하려고 했습니다");
            }
        }

        class RefValue : Value
        {
            public static RefValue NullValue = new RefValue(null);
            Value Value;

            public RefValue(Value value)
            {
                Value = value;
            }

            public override void SetValue(Value value)
            {
                Value = ((RefValue)value).Value;
            }
        }

        class IntegerValue : Value
        {
            public int Value { get; private set; }
            public IntegerValue(int i) { Value = i; }

            public override void SetValue(Value value)
            {
                Value = ((IntegerValue)value).Value;
            }
        }

        class BoolValue : Value
        {
            public bool Value { get; private set; }
            public BoolValue(bool b) { Value = b; }

            public override void SetValue(Value value)
            {
                Value = ((BoolValue)value).Value;
            }
        }

        class StringValue : Value
        {
            public string Value { get; private set; }
            public StringValue(string s) { Value = s; }
        }

        class LambdaValue : Value
        {
            public Method Method { get; private set; }
            public IReadOnlyCollection<IType> TypeArgs { get; private set; }

            public LambdaValue(Method method, IEnumerable<IType> typeArgs)
            {
                Method = method;
                TypeArgs = typeArgs.ToList();
            }
        }

        // 복합 Value 
        class CompositeValue : Value
        {
            public Value GetValue(string id, IEnumerable<IType> typeArgs)
            {
                throw new NotImplementedException();
            }

            // public IDictionary<string, Value> Fields { get; private set; }

        }

        // id -> Value
        class Env
        {
            List<Dictionary<string, Value>> stack = new List<Dictionary<string, Value>>();

            public Env()
            {
                Push();
            }
        
            public void Push()
            {
                stack.Add(new Dictionary<string, Value>());
            }

            public void Pop()
            {
                stack.RemoveAt(stack.Count - 1);
            }

            // 해당 env에 맞게 함수도 가져와야 합니다
            public bool TryGetValue(string id, out Value value)
            {
                for( int t = stack.Count - 1; 0 <= t ; t--)
                {
                    if (stack[t].TryGetValue(id, out value))
                        return true;
                }

                value = null;
                return false;
            }

            public void Add(string varName, Value value)
            {
                var peek = stack[stack.Count - 1];
                peek.Add(varName, value);
            }
        }

        class LoopCtrl
        {
            class Entry
            {
                public bool Continue = false;
                public bool Break = false;
            }

            Stack<Entry> stack = new Stack<Entry>();

            public LoopCtrl()
            {
                Push();
            }

            public bool Continue
            {
                get { return stack.Peek().Continue; }
                set
                {
                    var entry = stack.Peek();
                    entry.Continue = value;
                }
            }
            public bool Break
            {
                get { return stack.Peek().Break; }
                set
                {
                    var entry = stack.Peek();
                    entry.Break = value;
                }
            }

            public void Push()
            {
                stack.Push(new Entry());
            }

            public void Pop()
            {
                stack.Pop();
            }
        }

        class Frame
        {
            class Entry
            {
                public Env Env { get; private set; }
                public LoopCtrl LoopCtrl { get; private set; }
                public Value ReturnValue { get; set; }

                public Entry()
                {
                    Env = new Env();
                    LoopCtrl = new LoopCtrl();
                    ReturnValue = null;
                }
            }

            Stack<Entry> stack = new Stack<Entry>();

            public Env Env {  get { return stack.Peek().Env; } }
            public LoopCtrl LoopCtrl {  get { return stack.Peek().LoopCtrl; } }
            public Value ReturnValue
            {
                get { return stack.Peek().ReturnValue; }
                set
                {
                    var peek = stack.Peek(); peek.ReturnValue = value;
                }
            }

            public Frame()
            {
                Push();
            }

            public void Push()
            {
                stack.Push(new Entry());
            }

            public void Pop()
            {
                stack.Pop();
            }
        }

        // Evaluator의 현재 상태
        class State
        {
            public Frame Frame { get; private set; }
            
            public State()
            {
                Frame = new Frame();
            }           

            public IType GetType(TypeID typeID)
            {
                throw new NotImplementedException();
            }

            internal Method GetConstructor(IType type, IEnumerable<IType> argTypes)
            {
                throw new NotImplementedException();
            }

            internal Method GetUnaryOperator(UnaryExpKind operation, IType operandType)
            {
                throw new NotImplementedException();
            }

            internal Method GetBinaryOperator(BinaryExpKind operation, IType operandType1, IType operandType2)
            {
                throw new NotImplementedException();
            }            
        }

        class ExpEvaluator : IExpComponentVisitorRet<Value, State>
        {
            static ExpEvaluator expEval = new ExpEvaluator();

            public static Value Eval(IExpComponent exp, State state)
            {
                return exp.VisitRet(expEval, state);
            }

            private IType GetStaticType(IExpComponent exp)
            {
                // TypeChecker에서 값을 갖고 옵니다
                throw new NotImplementedException();
            }
            
            private Value Invoke(Method method, IReadOnlyCollection<IExpComponent> args, State state)
            {
                return Invoke(method, new IType[] { }, args, state);
            }

            private Value Invoke(Method method, IReadOnlyCollection<IType> TypeArgs, IReadOnlyCollection<IExpComponent> args, State state)
            {
                var argValues = args.Select(arg => ExpEvaluator.Eval(arg, state)).ToList();
                state.Frame.Push();

                for (int t = 0; t < argValues.Count; t++)
                    state.Frame.Env.Add(method.ParamNames[t], argValues[t]);

                StmtEvaluator.Eval(method.Body, state);

                var returnValue = state.Frame.ReturnValue;
                state.Frame.Pop();

                return returnValue;
            }

            public Value VisitRet(MemberExp memberExp, State state)
            {
                var expValue = Eval(memberExp.Exp, state) as CompositeValue;

                // methods 와 member에 같은 값이 들어가지 못하도록 막아야 합니다
                return expValue.GetValue(memberExp.MemberName, memberExp.TypeArgs.Select(state.GetType));
            }
            
            public Value VisitRet(ArrayExp arrayExp, State state)
            {
                throw new NotImplementedException();
            }

            // state의 Environment에서 가리키는 값을 리턴합니다.
            public Value VisitRet(IDExp idExp, State state)
            {
                return state.GetValue(idExp.ID);
            }

            public Value VisitRet(StringExp stringExp, State state)
            {
                return new RefValue(new StringValue(stringExp.Value));
            }

            public Value VisitRet(IntegerExp integerExp, State state)
            {
                return new IntegerValue(integerExp.Value);
            }

            public Value VisitRet(BoolExp boolExp, State state)
            {
                return new BoolValue(boolExp.Value);
            }

            // value를 만들고, ref도 만들고.. 생성자도 실행하고, 그걸 돌려줍니다
            public Value VisitRet(NewExp newExp, State state)
            {
                IType type = state.GetType(newExp.TypeName);
                var argStaticTypes = newExp.Args.Select(GetStaticType);

                // type constructor를 검색해야 합니다
                Method method = state.GetConstructor(type, argStaticTypes);

                // type constructor를 invoke 하면 결과물로.. Value가 나옵니다
                return Invoke(method, newExp.Args, state);
            }

            public Value VisitRet(UnaryExp unaryExp, State state)
            {
                var operandStaticType = GetStaticType(unaryExp.Operand);
                var method = state.GetUnaryOperator(unaryExp.Operation, operandStaticType);

                return Invoke(method, new[] { unaryExp.Operand }, state);
            }

            public Value VisitRet(BinaryExp binaryExp, State state)
            {
                var operand1StaticType = GetStaticType(binaryExp.Operand1);
                var operand2StaticType = GetStaticType(binaryExp.Operand2);
                var method = state.GetBinaryOperator(binaryExp.Operation, operand1StaticType, operand2StaticType);

                return Invoke(method, new[] { binaryExp.Operand1, binaryExp.Operand2 }, state);
            }

            public Value VisitRet(CallExp callExp, State state)
            {
                var lambdaValue = Eval(callExp.FuncExp, state) as LambdaValue;

                return Invoke(lambdaValue.Method, lambdaValue.TypeArgs, callExp.Args, state);
            }

            public Value VisitRet(AssignExp assignExp, State state)
            {
                // a = b
                var leftValue = Eval(assignExp.Left, state);
                var rightValue = Eval(assignExp.Right, state);

                // ref, int, bool, 
                leftValue.SetValue(rightValue);
                return leftValue;
            }
        }

        class ForInitEvaluator : IForInitComponentVisitor<State>
        {
            static ForInitEvaluator evaluator = new ForInitEvaluator();
            static public void Eval(IForInitComponent forInit, State state)
            {
                forInit.Visit(evaluator, state);
            }

            public void Visit(IExpComponent exp, State state)
            {
                ExpEvaluator.Eval(exp, state);
            }

            public void Visit(VarDecl varDecl, State state)
            {
                // TODO: VarDeclStmt와 같음.
                foreach (var nameAndExp in varDecl.NameAndExps)
                {
                    var value = ExpEvaluator.Eval(nameAndExp.Exp, state);
                    state.Frame.Env.Add(nameAndExp.VarName, value);
                }
            }
        }

        class StmtEvaluator : IStmtComponentVisitor<State>
        {
            static StmtEvaluator evaluator = new StmtEvaluator();
            public static void Eval(IStmtComponent stmt, State state)
            {
                stmt.Visit(evaluator, state);
            }

            public void Visit(BlockStmt blockStmt, State state)
            {
                state.Frame.Env.Push();
                foreach (var stmt in blockStmt.Stmts)
                {
                    Eval(stmt, state);
                    if (state.Frame.LoopCtrl.Continue || state.Frame.LoopCtrl.Break || state.Frame.ReturnValue != null ) break;
                }
                state.Frame.Env.Pop();
            }

            public void Visit(ContinueStmt continueStmt, State state)
            {
                state.Frame.LoopCtrl.Continue = true;
            }

            public void Visit(ExpStmt expStmt, State state)
            {
                ExpEvaluator.Eval(expStmt.Exp, state);
            }

            public void Visit(IfStmt ifStmt, State state)
            {
                var cond = ExpEvaluator.Eval(ifStmt.CondExp, state) as BoolValue;

                if (cond.Value)
                    StmtEvaluator.Eval(ifStmt.ThenStmt, state);
                else if(ifStmt.ElseStmt != null)
                    StmtEvaluator.Eval(ifStmt.ElseStmt, state);
            }

            public void Visit(WhileStmt whileStmt, State state)
            {
                state.Frame.Env.Push();
                state.Frame.LoopCtrl.Push();

                var cond = ExpEvaluator.Eval(whileStmt.CondExp, state) as BoolValue;
                while (cond.Value)
                {
                    StmtEvaluator.Eval(whileStmt.Body, state);
                    if (state.Frame.LoopCtrl.Break) break;

                    cond = ExpEvaluator.Eval(whileStmt.CondExp, state) as BoolValue;
                }

                state.Frame.LoopCtrl.Pop();
                state.Frame.Env.Pop();
            }

            public void Visit(ReturnStmt returnStmt, State state)
            {
                if (returnStmt.ReturnExp == null)
                    state.Frame.ReturnValue = RefValue.NullValue;
                else
                    state.Frame.ReturnValue = ExpEvaluator.Eval(returnStmt.ReturnExp, state);
            }

            public void Visit(ForStmt forStmt, State state)
            {
                state.Frame.Env.Push();
                state.Frame.LoopCtrl.Push();
                ForInitEvaluator.Eval(forStmt.Initializer, state);

                var cond = ExpEvaluator.Eval(forStmt.CondExp, state) as BoolValue;
                while (cond.Value)
                {
                    StmtEvaluator.Eval(forStmt.Body, state);
                    if (state.Frame.LoopCtrl.Break) break;

                    ExpEvaluator.Eval(forStmt.LoopExp, state);
                    cond = ExpEvaluator.Eval(forStmt.CondExp, state) as BoolValue;
                }

                state.Frame.LoopCtrl.Pop();
                state.Frame.Env.Pop();
            }

            public void Visit(DoWhileStmt doWhileStmt, State state)
            {
                state.Frame.Env.Push();
                state.Frame.LoopCtrl.Push();

                while (true)
                {
                    StmtEvaluator.Eval(doWhileStmt.Body, state);
                    if (state.Frame.LoopCtrl.Break) break;

                    var cond = ExpEvaluator.Eval(doWhileStmt.CondExp, state) as BoolValue;
                    if (!cond.Value) break;
                }

                state.Frame.LoopCtrl.Pop();
                state.Frame.Env.Pop();
            }

            public void Visit(BreakStmt breakStmt, State state)
            {
                state.Frame.LoopCtrl.Break = true;
            }

            public void Visit(BlankStmt blankStmt, State state)
            {
            }

            public void Visit(VarDeclStmt varDeclStmt, State state)
            {
                foreach (var nameAndExp in varDeclStmt.NameAndExps)
                {
                    var value = ExpEvaluator.Eval(nameAndExp.Exp, state);
                    state.Frame.Env.Add(nameAndExp.VarName, value);
                }
            }
        }
    }
}