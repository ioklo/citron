using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST
{
    public class Parser
    {
        private IEnumerable<T> Empty<T>()
        {
            return Enumerable.Empty<T>();
        }
        private IDWithTypeArgs CreateSingleID(string name)
        {
            return new IDWithTypeArgs(name, Empty<IDWithTypeArgs>());
        }

        // 0x, 123 둘다 해석함
        // 이건 Lexer가 해줘야 할 것 같다.. TokenType이 Integer라면 int를 갖고 있도록..
        public bool ParseInteger(Lexer lexer, out IntegerExp i)
        {
            i = null;

            if (lexer.TokenType != TokenType.IntValue)
                return false;

            using (LexerScope scope = lexer.CreateScope())
            {
                int res;
                if (int.TryParse(lexer.TokenValue, out res))
                {
                    // 성공했으면 다음 토큰으로 넘어감
                    lexer.NextToken();
                    i = new IntegerExp(res);
                    scope.Accept();
                    return true;
                }

                return false;
            }
        }

        // exp1 = exp2 + exp2
        //      | exp2 - exp2
        // exp2 = exp3 * exp3
        //      | exp3 / exp3        

        // con = exp != exp
        //     
        // 2 + 3 == 7
        // 
        // 먼저 쪼개는것? => 계산 우선순위가 낮은걸 먼저 쪼갠다 (즉 나머지것을 다하고 계산한다)
        // 먼저 쪼개야 할 것
        // 앞에부터 하나씩 먹는다.
        // 
        // 1. 숫자 만나면 집어넣기
        // 2. 연산자 만나면.. 나보다 먼저 쪼갰어야 할 높은 연산자들 다 꺼내기

        // 2 + 3 + 4
      
        // 2 + (3 - 7) * 8 / 7 - 2
        // (2 + (((3-7)*8) / 7)) 2
        // -
        
        // as  = base 
        //     | as [+-] as
        //
        // md  = base 
        //     | md [*/] base
        //
        // par = '(' base ')' // 최 상위가 들어가야 하는데

        private int BinOpToPriority(BinaryExpKind binOp)
        {
            switch(binOp)
            {
                case BinaryExpKind.ConditionalAnd: return 1;
                case BinaryExpKind.ConditionalOr: return 1;
            
                case BinaryExpKind.Equal: return 2;
                case BinaryExpKind.NotEqual: return 2;

                case BinaryExpKind.Less: return 3;
                case BinaryExpKind.LessEqual: return 3;
                case BinaryExpKind.Greater: return 3;
                case BinaryExpKind.GreaterEqual: return 3;

                case BinaryExpKind.Add: return 4;
                case BinaryExpKind.Sub: return 4;

                case BinaryExpKind.Mul: return 5;
                case BinaryExpKind.Div: return 5;

                default:
                    Debug.Assert(false);
                    return -1;
            }
        }

        public bool BinOpLessEqual(BinaryExpKind b1, BinaryExpKind b2)
        {
            return BinOpToPriority(b1) <= BinOpToPriority(b2);
        }

        // ++, --
        private bool ParsePostfixUnary(Lexer lexer, out UnaryExpKind? unOp)
        {
            if (lexer.TokenType == TokenType.PlusPlus)
            {
                unOp = UnaryExpKind.PostfixInc;
                lexer.NextToken();
                return true;
            }
            else if (lexer.TokenType == TokenType.MinusMinus)
            {
                unOp = UnaryExpKind.PostfixDec;
                lexer.NextToken();
                return true;
            }
            else
            {
                unOp = null;
                return false;
            }
        }       
        

        // Operator를 제외한 나머지를 파싱할 수 있다
        private bool ParseOperand(Lexer lexer, out IExpComponent exp)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                // () 괄호 처리
                if (lexer.Consume(TokenType.LParen))
                {
                    IExpComponent insideExp;
                    if (!ParseExp(lexer, out insideExp))
                    {
                        exp = null;
                        return false;
                    }

                    if (lexer.TokenType != TokenType.RParen)
                    {
                        exp = null;
                        return false;
                    }

                    lexer.NextToken();  // RParen 소비
                    exp = insideExp;
                    scope.Accept();
                    return true;
                }

                // new 처리
                NewExp ne;
                if (ParseNewExp(lexer, out ne))
                {
                    exp = ne;
                    scope.Accept();
                    return true;
                }

                // call 처리
                CallExp ce;                
                if (ParseCallExp(lexer, out ce))
                {
                    exp = ce;
                    scope.Accept();
                    return true;
                }

                // identifier 전에 assignment인지 체크
                AssignExp ae;
                if (ParseAssign(lexer, out ae))
                {
                    exp = ae;
                    scope.Accept();
                    return true;
                }

                // identifier일 경우
                string id;
                if (lexer.Consume(TokenType.Identifier, out id))
                {
                    // 1. 그냥 Variable일 수 있고,
                    // 2. Offset을 동반한 Variable일 수 있고,                    
                    
                    // TODO: Function as First class citizen
                    exp = new IDExp(CreateSingleID(id));

                    scope.Accept();
                    return true;
                }


                if (lexer.TokenType == TokenType.IntValue)
                {
                    exp = new IntegerExp(int.Parse(lexer.TokenValue));
                    lexer.NextToken();
                }
                else if (lexer.TokenType == TokenType.StringValue)
                {
                    exp = new StringExp(lexer.TokenValue.Substring(1, lexer.TokenValue.Length - 2));
                    lexer.NextToken();
                }
                else if (lexer.TokenType == TokenType.TrueValue)
                {
                    exp = new BoolExp(true);
                    lexer.NextToken();
                }
                else if (lexer.TokenType == TokenType.FalseValue)
                {
                    exp = new BoolExp(false);
                    lexer.NextToken();
                }
                else
                {
                    // exp는 unary로 시작하거나, 괄호, 변수, 숫자, 값이 와야 합니다.
                    exp = null;
                    return false;
                }

                scope.Accept();
                return true;
            }
        }

        UnaryExpKind? ConvertUnaryOperation(TokenType tokenType)
        {
            switch(tokenType)
            {
                case TokenType.Plus: return UnaryExpKind.Plus;
                case TokenType.Minus: return UnaryExpKind.Minus;
                case TokenType.Exclamation: return UnaryExpKind.Not;
                case TokenType.Tilde: return UnaryExpKind.Neg;
            }

            return null;
        }

        UnaryExpKind? ConvertPreUnaryOperation(TokenType tokenType)
        {
            if (tokenType == TokenType.PlusPlus)
                return UnaryExpKind.PrefixInc;
            else if (tokenType == TokenType.MinusMinus)
                return UnaryExpKind.PrefixDec;

            return ConvertUnaryOperation(tokenType);
        }
        
        BinaryExpKind? ConvertBinaryOperation(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.AmperAmper: return BinaryExpKind.ConditionalAnd;
                case TokenType.BarBar: return BinaryExpKind.ConditionalOr;

                case TokenType.EqualEqual: return BinaryExpKind.Equal;
                case TokenType.NotEqual: return BinaryExpKind.NotEqual;

                case TokenType.Less: return BinaryExpKind.Less;
                case TokenType.LessEqual: return BinaryExpKind.LessEqual;
                case TokenType.Greater: return BinaryExpKind.Greater;
                case TokenType.GreaterEqual: return BinaryExpKind.GreaterEqual;
                case TokenType.Plus: return BinaryExpKind.Add;
                case TokenType.Minus: return BinaryExpKind.Sub;
                case TokenType.Star: return BinaryExpKind.Mul;
                case TokenType.Slash: return BinaryExpKind.Div;
                case TokenType.Percent: return BinaryExpKind.Mod;

                case TokenType.Equal: return BinaryExpKind.Assign;
                case TokenType.LessLess: return BinaryExpKind.ShiftLeft;
                case TokenType.GreaterGreater: return BinaryExpKind.ShiftRight;

            }

            return null;
        }
        
        public bool ParseBinaryOperation(Lexer lexer, out BinaryExpKind? binOp)
        {
            // 1. && ||           // 맨 나중에 계산
            // 2. == != 
            // 3. < <= > >=       // 
            // 3. + -
            // 4. * /             // 제일 먼저 계산

            switch(lexer.TokenType)
            {
                case TokenType.AmperAmper: binOp = BinaryExpKind.ConditionalAnd; break;
                case TokenType.BarBar:  binOp = BinaryExpKind.ConditionalOr; break;

                case TokenType.EqualEqual: binOp = BinaryExpKind.Equal; break;
                case TokenType.NotEqual: binOp = BinaryExpKind.NotEqual; break;

                case TokenType.Less: binOp = BinaryExpKind.Less; break;
                case TokenType.LessEqual: binOp = BinaryExpKind.LessEqual; break;
                case TokenType.Greater: binOp = BinaryExpKind.Greater; break;
                case TokenType.GreaterEqual: binOp = BinaryExpKind.GreaterEqual; break;
                case TokenType.Plus: binOp = BinaryExpKind.Add; break;
                case TokenType.Minus: binOp = BinaryExpKind.Sub; break;
                case TokenType.Star: binOp = BinaryExpKind.Mul; break;
                case TokenType.Slash: binOp = BinaryExpKind.Div; break;
                default:
                    binOp = null;
                    return false;
            }
            
            lexer.NextToken();
            return true;
        }

        //public bool ParseOffset(Lexer lexer, VariableExp v)
        //{
        //    using (LexerScope scope = lexer.CreateScope())
        //    {                
        //        while (!lexer.End)
        //        {
        //            if (lexer.Consume(TokenType.Dot))
        //            {
        //                string fieldName;
        //                if (!lexer.Consume(TokenType.Identifier, out fieldName))
        //                    return false; // 복구 불가능

        //                v.Offsets.Add(new StringOffset() { Field = fieldName });
        //            }
        //            else if (lexer.Consume(TokenType.LBracket))
        //            {
        //                string index;
        //                if (!lexer.Consume(TokenType.IntValue, out index))
        //                    return false;

        //                v.Offsets.Add(new IndexOffset() { Index = int.Parse(index) });
        //            }
        //            else break;
        //        }

        //        scope.Accept();
        //        return true;
        //    }
        //}

        public bool ParseNewExp(Lexer lexer, out NewExp ne)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                string typeName;
                if (!lexer.Consume(TokenType.New) ||
                    !lexer.Consume(TokenType.Identifier, out typeName) ||
                    !lexer.Consume(TokenType.LParen))
                {
                    ne = null;
                    return false;
                }

                // 그냥 빈 것
                if (lexer.Consume(TokenType.RParen))
                {
                    ne = new NewExp(CreateSingleID(typeName), Empty<IExpComponent>());
                    scope.Accept();
                    return true;
                }

                // 아니라면 적어도 하나는 있을 것이다
                var exps = new List<IExpComponent>();
                do
                {
                    IExpComponent exp;
                    if (!ParseExp(lexer, out exp))
                    {
                        ne = null;
                        return false;
                    }
                    exps.Add(exp);
                    
                } while (lexer.Consume(TokenType.Comma));

                if (!lexer.Consume(TokenType.RParen))
                {
                    ne = null;
                    return false;
                }

                ne = new NewExp(CreateSingleID(typeName), exps);
                scope.Accept();
                return true;
            }

        }

        public bool ParseCallExp(Lexer lexer, out CallExp ce)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                string funcName;
                if (!lexer.Consume(TokenType.Identifier, out funcName) ||
                    !lexer.Consume(TokenType.LParen))
                {
                    ce = null;
                    return false;
                }

                // 그냥 빈 것
                if (lexer.Consume(TokenType.RParen))
                {
                    scope.Accept();
                    ce = new CallExp(new IDExp(CreateSingleID(funcName)), Empty<IExpComponent>());
                    return true;
                }

                // 아니라면 적어도 하나는 있을 것이다
                var args = new List<IExpComponent>();
                do
                {
                    IExpComponent exp;
                    if (!ParseExp(lexer, out exp))
                    {
                        ce = null;
                        return false;
                    }

                    args.Add(exp);
                } while (lexer.Consume(TokenType.Comma));

                if (!lexer.Consume(TokenType.RParen))
                {
                    ce = null;
                    return false;
                }
                
                scope.Accept();
                ce = new CallExp(new IDExp(CreateSingleID(funcName)), args);
                return true;
            }
        }   
     
        private void ArrangeOperations(Stack<IExpComponent> operands, Stack<BinaryExpKind> ops, BinaryExpKind binOp, IExpComponent operand)
        {
            while(ops.Count != 0 && BinOpLessEqual(binOp, ops.Peek()))
            {
                // merge
                IExpComponent right = operands.Pop();
                IExpComponent left = operands.Pop();
                BinaryExpKind op = ops.Pop();

                IExpComponent binExp = new BinaryExp(op, left, right);
                operands.Push(binExp);
            }

            ops.Push(binOp);
            operands.Push(operand);
            return;
        }

        

        // 1. x.y, f(x), a[x], x++, x--, new, typeof default checked unchecked delegate?
        bool ParsePrimaryExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                if (lexer.Consume(TokenType.TrueValue))
                {
                    exp = new BoolExp(true);
                    lexerScope.Accept();
                    return true;
                }

                if (lexer.Consume(TokenType.FalseValue))
                {
                    exp = new BoolExp(false);
                    lexerScope.Accept();
                    return true;
                }

                // TODO: 일단 실험적으로 Identifier만 받습니다.
                string id;
                if (!lexer.Consume(TokenType.Identifier, out id)) return false;

                exp = new IDExp(CreateSingleID(id));
                lexerScope.Accept();
                return true;


                // 1. variable
                // 2. call
                // 3. array member
                // 4. postfix ++
                // 5. postfix --
                // 6. new 
            }
        }

        // 2. + - ! ~ ++x --x // (T)x
        bool ParseUnaryExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                if (ParsePrimaryExp(lexer, out exp))
                {
                    lexerScope.Accept();
                    return true;
                }

                TokenType tokenType;
                if (!lexer.ConsumeAny(out tokenType, TokenType.Plus, TokenType.Minus,
                    TokenType.Exclamation, TokenType.Tilde, TokenType.PlusPlus, TokenType.MinusMinus))
                    return false;

                UnaryExpKind? unaryExpKind = ConvertPreUnaryOperation(tokenType);
                if (!unaryExpKind.HasValue) return false;

                IExpComponent operandExp;
                if (!ParseUnaryExp(lexer, out operandExp)) return false;

                exp = new UnaryExp(unaryExpKind.Value, operandExp);
                lexerScope.Accept();
                return true;
            }
        }

        // 3. * / %
        // Unary
        // MultiplicativeExp (*/%) Unary
        bool ParseMultiplicativeExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseUnaryExp(lexer, out leftExp)) return false;

                TokenType tokenType;
                while (lexer.ConsumeAny(out tokenType,
                    TokenType.Star, TokenType.Slash, TokenType.Percent))
                {
                    var binExpKind = ConvertBinaryOperation(tokenType);
                    if (!binExpKind.HasValue) return false;

                    IExpComponent rightExp;
                    if (!ParseUnaryExp(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(binExpKind.Value, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 4. + - 
        bool ParseAdditiveExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseMultiplicativeExp(lexer, out leftExp)) return false;

                TokenType tokenType;
                while (lexer.ConsumeAny(out tokenType,
                    TokenType.Plus,
                    TokenType.Minus))
                {
                    var binExpKind = ConvertBinaryOperation(tokenType);
                    if (!binExpKind.HasValue) return false;

                    IExpComponent rightExp;
                    if (!ParseMultiplicativeExp(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(binExpKind.Value, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 5. << >>
        bool ParseShiftExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseAdditiveExp(lexer, out leftExp)) return false;

                TokenType tokenType;
                while (lexer.ConsumeAny(out tokenType,
                    TokenType.LessLess,
                    TokenType.GreaterGreater))
                {
                    var binExpKind = ConvertBinaryOperation(tokenType);
                    if (!binExpKind.HasValue) return false;

                    IExpComponent rightExp;
                    if (!ParseAdditiveExp(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(binExpKind.Value, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 6. Relational and type testing 
        // < > <= >=  // is as
        bool ParseRelationalAndTypeTestingExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseShiftExp(lexer, out leftExp)) return false;

                TokenType tokenType;
                while (lexer.ConsumeAny(out tokenType, 
                    TokenType.Less,
                    TokenType.Greater, 
                    TokenType.LessEqual,
                    TokenType.GreaterEqual))
                {
                    var binExpKind = ConvertBinaryOperation(tokenType);
                    if (!binExpKind.HasValue) return false;

                    IExpComponent rightExp;
                    if (!ParseShiftExp(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(binExpKind.Value, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 7. == !=
        bool ParseEqualityExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseRelationalAndTypeTestingExp(lexer, out leftExp)) return false;

                TokenType tokenType;
                while (lexer.ConsumeAny(out tokenType, TokenType.EqualEqual, TokenType.NotEqual))
                {
                    var binExpKind = ConvertBinaryOperation(tokenType);
                    if (!binExpKind.HasValue) return false;

                    IExpComponent rightExp;
                    if (!ParseRelationalAndTypeTestingExp(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(binExpKind.Value, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 8. &
        bool ParseLogicalAND(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseEqualityExp(lexer, out leftExp)) return false;

                while (lexer.Consume(TokenType.Amper))
                {
                    IExpComponent rightExp;
                    if (!ParseEqualityExp(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(BinaryExpKind.LogicalAnd, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 9. ^
        bool ParseLogicalXOR(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseLogicalAND(lexer, out leftExp)) return false;

                while (lexer.Consume(TokenType.Caret))
                {
                    IExpComponent rightExp;
                    if (!ParseLogicalAND(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(BinaryExpKind.LogicalXor, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }


        // 10. |
        bool ParseLogicalOR(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseLogicalXOR(lexer, out leftExp)) return false;

                while (lexer.Consume(TokenType.Bar))
                {
                    IExpComponent rightExp;
                    if (!ParseLogicalXOR(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(BinaryExpKind.LogicalOr, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 11. &&
        bool ParseConditionalAND(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseLogicalOR(lexer, out leftExp)) return false;

                while (lexer.Consume(TokenType.AmperAmper))
                {
                    IExpComponent rightExp;
                    if (!ParseLogicalOR(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(BinaryExpKind.ConditionalAnd, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 12. ||, left associativity
        bool ParseConditionalOR(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            using (var lexerScope = lexer.CreateScope())
            {
                IExpComponent leftExp;
                if (!ParseConditionalAND(lexer, out leftExp)) return false;

                while (lexer.Consume(TokenType.BarBar))
                {
                    IExpComponent rightExp;
                    if (!ParseConditionalAND(lexer, out rightExp)) return false;

                    leftExp = new BinaryExp(BinaryExpKind.ConditionalOr, leftExp, rightExp);
                }

                exp = leftExp;
                lexerScope.Accept();
                return true;
            }
        }

        // 

        // 13. ?? Null coalescing -> skip, right-associative
        // 14. ?: , right associative
        /*bool ParseConditionalExp(Lexer lexer, out IExpComponent exp)
        {
            exp = null;

            IExpComponent condExp, leftExp, rightExp;

            if (!ParseConditionalOR(lexer, out condExp)) return false;
            if (!lexer.Consume(TokenType.Question)) return false;
            if (!ParseExp(lexer, out leftExp)) return false;
            if (!lexer.Consume(TokenType.Colon)) return false;
            if (!ParseExp(lexer, out rightExp)) return false;

            // 끝까지 가서 먹어버리는 일 없게 하기..
            return true;
        }*/

        // 15. Assignment and (lambda expression '=>', not supported), right associative
        // = *= /= %= += -= <<= >>= &= ^= |= 
        bool ParseAssignmentExp(Lexer lexer, out IExpComponent exp)
        {
            TokenType[] assignmentTokenTypes = new[]
                {
                TokenType.Equal, TokenType.StarEqual, TokenType.SlashEqual, TokenType.PercentEqual,
                TokenType.PlusEqual, TokenType.MinusEqual, TokenType.LessLessEqual, TokenType.GreaterGreaterEqual,
                TokenType.AmperEqual, TokenType.CaretEqual, TokenType.BarEqual
            };

            using (var lexerScope = lexer.CreateScope())
            {
                exp = null;

                // Right Associativity의 경우 Stack을 사용합니다.. 안쓰면 어떻게 되나요
                // a = b = c = d;
                // Assign(a, Assign(b, Assign(c, d))))
                Stack<IExpComponent> operandExps = new Stack<IExpComponent>();
                Stack<BinaryExpKind> operators = new Stack<BinaryExpKind>();

                IExpComponent operandExp;
                if (!ParseConditionalOR(lexer, out operandExp)) return false;

                operandExps.Push(operandExp);

                TokenType tokenType;
                while (lexer.ConsumeAny(out tokenType, assignmentTokenTypes))
                {
                    var binOp = ConvertBinaryOperation(tokenType);
                    if (binOp == null) throw new NotImplementedException("Assignment Token을 BinaryExpKind로 변환하는데 실패했습니다");

                    operators.Push(binOp.Value);

                    if (!ParseConditionalOR(lexer, out operandExp)) return false;
                    operandExps.Push(operandExp);
                }

                while (operandExps.Count != 1)
                {
                    var rightExp = operandExps.Pop();
                    var leftExp = operandExps.Pop();
                    var binOp = operators.Pop();

                    operandExps.Push(new BinaryExp(binOp, leftExp, rightExp));
                }

                exp = operandExps.Pop();
                lexerScope.Accept();
                return true;
            }
        }

        public bool ParseExp(Lexer lexer, out IExpComponent exp)
        {
            return ParseAssignmentExp(lexer, out exp);
        }
        
        //public bool ParseExp(Lexer lexer, out IExpComponent exp)
        //{
        //    using (LexerScope scope = lexer.CreateScope())
        //    {
        //        Stack<IExpComponent> operands = new Stack<IExpComponent>();
        //        Stack<BinaryExpKind> ops = new Stack<BinaryExpKind>();

        //        IExpComponent operand;

        //        if (!ParsePreUnaryAndOperand(lexer, out operand))
        //        {
        //            // expression은 operand로 시작해야 합니다.
        //            exp = null;
        //            return false;
        //        }

        //        operands.Push(operand);

        //        while (!lexer.End)
        //        {
        //            UnaryExpKind? postfixUnOp;

        //            // Unary 는 가장 먼저 묶는다
        //            if (ParsePostfixUnary(lexer, out postfixUnOp))
        //            {
        //                IExpComponent unOperand = operands.Pop();
        //                if (unOperand is UnaryExp)
        //                {
        //                    throw new System.Exception("단항연산자는 중복될 수 없습니다");
        //                }

        //                IExpComponent unExp = new UnaryExp(postfixUnOp.Value, unOperand);
        //                operands.Push(unExp);
        //                continue;
        //            }

        //            // BinaryExpKind을 가져온다
        //            BinaryExpKind? binOp;
        //            if (!ParseBinaryOperation(lexer, out binOp))
        //            {
        //                // 더 이상 BinaryExpKind이 없을 경우 나머지 처리를 하고 정상종료를 한다.
        //                break;
        //            }

        //            if (!ParsePreUnaryAndOperand(lexer, out operand))
        //            {
        //                // BinaryExpKind의 대상이 없습니다
        //                exp = null;
        //                return false;
        //            }

        //            ArrangeOperations(operands, ops, binOp.Value, operand);
        //        }

        //        // 이제 남은 것들을 뒤에서 부터 묶는다.
        //        IExpComponent right = operands.Pop();
        //        Debug.Assert(operands.Count == ops.Count);
        //        while (ops.Count != 0)
        //        {
        //            BinaryExpKind op = ops.Pop();
        //            IExpComponent left = operands.Pop();

        //            right = new BinaryExp(op, left, right);
        //        }

        //        exp = right;
        //        scope.Accept();
        //        return true;
        //    }
        //}

        
        bool ParseVarDeclStmt(Lexer lexer, out VarDecl vd)
        {
            VarDecl varDecl;
            if (!ParseVarDecl(lexer, out varDecl))
            {
                vd = null;
                return false;
            }

            vd = varDecl;
            return true;
        }

        bool ParseVarDecl(Lexer lexer, out VarDecl vd)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                string typeName;

                if (!lexer.Consume(TokenType.Identifier, out typeName))
                {
                    vd = null;
                    return false;
                }

                var typeID = CreateSingleID(typeName);
                var nameAndExps = new List<NameAndExp>();

                do
                {
                    string name;
                    if (!lexer.Consume(TokenType.Identifier, out name))
                    {
                        vd = null;
                        return false;
                    }

                    IExpComponent exp;
                    ParseInitializer(lexer, out exp);

                    nameAndExps.Add(new NameAndExp(name, exp));

                } while (lexer.Consume(TokenType.Comma));

                if (!lexer.Consume(TokenType.SemiColon))
                {
                    vd = null;
                    return false;
                }

                vd = new VarDecl(typeID, nameAndExps);                
                scope.Accept();
                return true;
            }
        }        

        bool ParseInitializer(Lexer lexer, out IExpComponent exp)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                if (!lexer.Consume(TokenType.Equal) ||
                    !ParseExp(lexer, out exp))
                {
                    exp = null;
                    return false;                    
                }

                scope.Accept();
                return true;
            }
        }

        bool ParseFuncDecl(Lexer lexer, out FuncDecl fd)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                string typeName, name;
                if (!lexer.Consume(TokenType.Identifier, out typeName))
                {
                    fd = null;
                    return false;
                }

                if (!lexer.Consume(TokenType.Identifier, out name))
                {
                    name = typeName;
                    typeName = null;
                }

                var parameters = new List<FuncParam>();                
                
                // argument list 파싱
                // (
                if (!lexer.Consume(TokenType.LParen))
                {
                    fd = null;
                    return false;
                }

                // ) 찾기
                if (!lexer.Consume(TokenType.RParen))
                {
                    if (!lexer.Consume(TokenType.Identifier, out typeName) ||
                        !lexer.Consume(TokenType.Identifier, out name))
                    {
                        // 복구 불가능                    
                        fd = null;
                        return false;
                    }
                    
                    // TODO: Parameter Modifier 미구현 
                    parameters.Add(new FuncParam(Empty<FuncParamModifier>(), CreateSingleID(typeName), name));

                    while (lexer.Consume(TokenType.Comma))
                    {
                        if (!lexer.Consume(TokenType.Identifier, out typeName) ||
                            !lexer.Consume(TokenType.Identifier, out name))
                        {
                            // 복구 불가능                    
                            fd = null;
                            return false;
                        }

                        parameters.Add(new FuncParam(Empty<FuncParamModifier>(), CreateSingleID(typeName), name));
                    }

                    if (!lexer.Consume(TokenType.RParen))
                    {
                        // 복구 불가능, ")"이 빠졌습니다
                        fd = null;
                        return false;
                    }
                }

                if (!lexer.Consume(TokenType.LBrace))
                {
                    fd = null;
                    return false;
                }


                var stmts = new List<IStmtComponent>();
                while(!lexer.Consume(TokenType.RBrace))
                {
                    IStmtComponent stmt;
                    if (!ParseStatement(lexer, out stmt))
                    {
                        fd = null;
                        return false;
                    }

                    stmts.Add(stmt);
                }                

                fd = new FuncDecl(Empty<string>(), CreateSingleID(typeName), name, parameters, new BlockStmt(stmts));
                scope.Accept();
                return true;
            }
        }

        bool ParseBlock(Lexer lexer, out BlockStmt block)
        {
            var stmts = new List<IStmtComponent>();
            using (LexerScope scope = lexer.CreateScope())
            {
                if (lexer.TokenType != TokenType.LBrace)
                {
                    // Block이 아님
                    block = null;
                    return false;
                }

                lexer.NextToken();

                while (lexer.TokenType != TokenType.RBrace)
                {   
                    IStmtComponent stmt;

                    if (!ParseStatement(lexer, out stmt))
                    {   
                        block = null;
                        return false;
                    }

                    stmts.Add(stmt);
                }
                lexer.NextToken();
                
                block = new BlockStmt(stmts);
                scope.Accept();
                return true;
            }
        }

        bool ParseIfStatement(Lexer lexer, out IfStmt ifStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                // if ( 부분
                IExpComponent condExp;
                IStmtComponent thenStmt;

                if (!lexer.ConsumeSeq(TokenType.If, TokenType.LParen) ||
                    !ParseExp(lexer, out condExp) || 
                    !lexer.Consume(TokenType.RParen) ||
                    !ParseStatement(lexer, out thenStmt))
                {
                    ifStmt = null;
                    return false;
                }
                
                IStmtComponent elseStmt = null;
                if (lexer.Consume(TokenType.Else))
                {                    
                    if (!ParseStatement(lexer, out elseStmt))
                    {
                        ifStmt = null;
                        return false;
                    }
                }

                ifStmt = new IfStmt(condExp, thenStmt, elseStmt);
                scope.Accept();
                return true;
            }
        }

        bool ParseStatement(Lexer lexer, out IStmtComponent stmt)
        {
            // Statement 
            // 2. 변수 선언 Variable Declaration ;
            // - If            
            // - For, Do, While
            // - Return Exp;
            // - Expression-Statement;
            // - Block { }

            using (LexerScope scope = lexer.CreateScope())
            {
                // 2. Variable Declaration with ';'
                VarDecl varDecl;
                if (ParseVarDeclStmt(lexer, out varDecl))
                {                    
                    stmt = varDecl;
                    scope.Accept();
                    return true;
                }

                // Block 
                BlockStmt block;
                if (ParseBlock(lexer, out block))
                {
                    stmt = block;
                    scope.Accept();
                    return true;
                }

                ExpStmt expStmt;
                if (ParseExpStmt(lexer, out expStmt))
                {
                    stmt = expStmt;
                    scope.Accept();
                    return true;
                }

                IfStmt ifStmt;
                if (ParseIfStatement(lexer, out ifStmt))
                {
                    stmt = ifStmt;
                    scope.Accept();
                    return true;
                }

                ReturnStmt returnStmt;
                if (ParseReturnStmt(lexer, out returnStmt))
                {   
                    stmt = returnStmt;
                    scope.Accept();
                    return true;
                }

                ForStmt forStmt;
                if (ParseForStatement(lexer, out forStmt))
                {
                    stmt = forStmt;
                    scope.Accept();
                    return true;
                }

                ContinueStmt contStmt;
                if (ParseContinueStmt(lexer, out contStmt))
                {
                    stmt = contStmt;
                    scope.Accept();
                    return true;
                }

                BreakStmt breakStmt;
                if (ParseBreakStmt(lexer, out breakStmt))
                {
                    stmt = breakStmt;
                    scope.Accept();
                    return true;
                }

                WhileStmt whileStmt;
                if (ParseWhileStmt(lexer, out whileStmt))
                {
                    stmt = whileStmt;
                    scope.Accept();
                    return true;
                }

                DoWhileStmt doWhileStmt;
                if (ParseDoWhileStmt(lexer, out doWhileStmt))
                {
                    stmt = doWhileStmt;
                    scope.Accept();
                    return true;
                }

                stmt = null;
                return false;
            }
        }

        bool ParseExpStmt(Lexer lexer, out ExpStmt expStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                IExpComponent exp;
                // 빈 익스프레션도 받기 때문에 성공 여부가 중요하지 않다.
                ParseExp(lexer, out exp);
                
                if (!lexer.Consume(TokenType.SemiColon))
                {
                    expStmt = null;
                    return false;
                }
                
                expStmt = new ExpStmt(exp);
                scope.Accept();
                return true;                
            }
        }

        private bool ParseForStatement(Lexer lexer, out ForStmt forStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                if (!lexer.Consume(TokenType.For) ||
                    !lexer.Consume(TokenType.LParen))
                {
                    forStmt = null;
                    return false;
                }

                // 첫번째에 들어갈 수 있는 것들..
                // Nothing
                // Variable Declaration With Initial value
                // Expression
                IStmtComponent init = null;
                VarDecl vd;
                ExpStmt expStmt;
                if (ParseVarDeclStmt(lexer, out vd))
                {
                    init = vd;                        
                }
                else if (ParseExpStmt(lexer, out expStmt))
                {
                    init = expStmt;
                }
                else
                {
                    forStmt = null;
                    return false;
                }

                // 두번쨰에 들어갈 수 있는 것, IExp
                IExpComponent condExp = null;
                if (!lexer.Consume(TokenType.SemiColon))
                {                    
                    if (!ParseExp(lexer, out condExp) ||
                        !lexer.Consume(TokenType.SemiColon))
                    {
                        forStmt = null;
                        return false;
                    }
                }
                
                // 세번째에 들어갈 수 있는 것, IExp
                IExpComponent loopExp = null;
                if (!lexer.Consume(TokenType.RParen))
                {
                    if (!ParseExp(lexer, out loopExp) ||
                        !lexer.Consume(TokenType.RParen))
                    {
                        forStmt = null;
                        return false;
                    }
                }

                IStmtComponent body;
                if (!ParseStatement(lexer, out body))
                {
                    forStmt = null;
                    return false;
                }

                forStmt = new ForStmt(init, condExp, loopExp, body);
                scope.Accept();
                return true;
            }
        }

        private bool ParseWhileStmt(Lexer lexer, out WhileStmt whileStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                IExpComponent condExp;

                if (!lexer.Consume(TokenType.While) ||
                    !lexer.Consume(TokenType.LParen) ||
                    !ParseExp(lexer, out condExp) ||
                    !lexer.Consume(TokenType.RParen))
                {
                    whileStmt = null;
                    return false;
                }

                IStmtComponent body;
                if (!ParseStatement(lexer, out body))
                {
                    whileStmt = null;
                    return false;
                }

                whileStmt = new WhileStmt(condExp, body);
                scope.Accept();
                return true;
            }
        }

        private bool ParseDoWhileStmt(Lexer lexer, out DoWhileStmt doWhileStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {                
                IStmtComponent body;

                if (!lexer.Consume(TokenType.Do) ||
                    !ParseStatement(lexer, out body))
                {
                    doWhileStmt = null;
                    return false;
                }

                IExpComponent condExp;
                if (!lexer.Consume(TokenType.While) ||
                    !lexer.Consume(TokenType.LParen) ||
                    !ParseExp(lexer, out condExp) ||
                    !lexer.Consume(TokenType.RParen) ||
                    !lexer.Consume(TokenType.SemiColon))
                {
                    doWhileStmt = null;
                    return false;
                }

                doWhileStmt = new DoWhileStmt(body, condExp);
                scope.Accept();
                return true;
            }
        }

        bool ParseReturnStmt(Lexer lexer, out ReturnStmt returnStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                if (!lexer.Consume(TokenType.Return))
                {
                    returnStmt = null;
                    return false;
                }

                if (lexer.Consume(TokenType.SemiColon))
                {
                    returnStmt = new ReturnStmt(null);
                    scope.Accept();
                    return true;
                }

                IExpComponent exp = null;
                if (!ParseExp(lexer, out exp) || 
                    !lexer.Consume(TokenType.SemiColon))
                {
                    returnStmt = null;
                    return false;
                }

                returnStmt = new ReturnStmt(exp);
                scope.Accept();
                return true;
            }
        }

        bool ParseContinueStmt(Lexer lexer, out ContinueStmt continueStmt)
        {
            continueStmt = null;
            using (LexerScope scope = lexer.CreateScope())
            {
                if (lexer.Consume(TokenType.Continue) &&
                    lexer.Consume(TokenType.SemiColon))
                {
                    continueStmt = new ContinueStmt();
                    scope.Accept();
                    return true;
                }
            }
            return false;
        }

        bool ParseBreakStmt(Lexer lexer, out BreakStmt breakStmt)
        {
            breakStmt = null;
            using (LexerScope scope = lexer.CreateScope())
            {
                if (lexer.Consume(TokenType.Break) &&
                    lexer.Consume(TokenType.SemiColon))
                {
                    breakStmt = new BreakStmt();
                    scope.Accept();
                    return true;
                }
            }
            return false;
        }

        bool ParseAssign(Lexer lexer, out AssignExp aStmt)
        {
            throw new NotImplementedException();
            //aStmt = null;

            //using (LexerScope scope = lexer.CreateScope())
            //{
            //    VariableExp varExp;
            //    IExpComponent exp;
            //    Lexer.TokenKind tokenKind;                
                
            //    if (!ParseVariable(lexer, out varExp) ||
            //        !lexer.ConsumeAny(out tokenKind, TokenType.Equal,
            //            TokenType.PlusEqual,
            //            TokenType.MinusEqual,
            //            TokenType.StarEqual,
            //            TokenType.SlashEqual) ||
            //        !ParseExp(lexer, out exp))
            //    {                    
            //        return false;
            //    }

            //    switch(tokenKind)
            //    {
            //        case TokenType.Equal:
            //            aStmt = new AssignExp() { Left = varExp, Exp = exp };
            //            break;

            //        case TokenType.PlusEqual:
            //            aStmt = new AssignExp()
            //            {
            //                Left = varExp,
            //                Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Add }
            //            };
            //            break;

            //        case TokenType.MinusEqual:
            //            aStmt = new AssignExp()
            //            {
            //                Left = varExp,
            //                Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Add }
            //            };
            //            break;

            //        case TokenType.StarEqual:
            //            aStmt = new AssignExp()
            //            {
            //                Left = varExp,
            //                Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Mul }
            //            };
            //            break;

            //        case TokenType.SlashEqual:
            //            aStmt = new AssignExp()
            //            {
            //                Left = varExp,
            //                Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Div }
            //            };
            //            break;
            //    }
            //    scope.Accept();
            //    return true;
            //}            
        }

        bool ParseVariable(Lexer lexer, out IDExp idExp)
        {
            if (lexer.TokenType != TokenType.Identifier)
            {
                idExp = null;
                return false;
            }

            string id = lexer.TokenValue;
            using (LexerScope scope = lexer.CreateScope())
            {
                lexer.NextToken();

                IDExp ie = new IDExp(CreateSingleID(id));
                idExp = ie;
                scope.Accept();
                return true;
            }
        }

        //public bool ParseClassDecl(Lexer lexer, out ClassDecl classDecl)
        //{
        //    using (var scope = lexer.CreateScope())
        //    {
        //        string className;

        //        if (!lexer.Consume(TokenType.Class) ||
        //            !lexer.Consume(TokenType.Identifier, out className))
        //        {
        //            classDecl = null;
        //            return false;
        //        }

        //        // 상속 목록
        //        // : ID(, ID)*

        //        var baseTypes = new List<TypeIdentifier>();

        //        if (lexer.Consume(TokenType.Colon))
        //        {
        //            string baseTypeName;

        //            if (!lexer.Consume(TokenType.Identifier, out baseTypeName))
        //            {
        //                // 복구 불가능한 오류
        //                classDecl = null;
        //                return false;
        //            }

        //            baseTypes.Add(new TypeIdentifier(baseTypeName));
        //            while(lexer.Consume(TokenType.Comma))
        //            {
        //                if (!lexer.Consume(TokenType.Identifier, out baseTypeName))
        //                {
        //                    // 복구 불가능한 오류
        //                    classDecl = null;
        //                    return false;
        //                }
        //                baseTypes.Add(new TypeIdentifier(baseTypeName));
        //            }
        //        }

        //        if (!lexer.Consume(TokenType.LBrace))
        //        {
        //            classDecl = null;
        //            return false;
        //        }

        //        var funcDecls = new List<ClassFuncDecl>();
        //        var varDecls = new List<ClassVarDecl>();

        //        while (!lexer.Consume(TokenType.RBrace))
        //        {
        //            ClassFuncDecl cfd;
        //            if (ParseClassFuncDecl(lexer, out cfd))
        //            {
        //                funcDecls.Add(cfd);
        //                continue;
        //            }

        //            ClassVarDecl cvd;
        //            if (ParseClassVarDecl(lexer, out cvd))
        //            {
        //                varDecls.Add(cvd);
        //                continue;
        //            }
        //        }

        //        classDecl = new ClassDecl(new TypeIdentifier(className), funcDecls, varDecls, baseTypes);
        //        scope.Accept();
        //        return true;
        //    }
        //}

        //private bool ParseClassVarDecl(Lexer lexer, out ClassVarDecl cvd)
        //{
        //    // option, accessor (기본 private)
        //    using (var scope = lexer.CreateScope())
        //    {
        //        // option, accessor
        //        Lexer.TokenKind accessModifierToken;
        //        Gum.Core.AbstractSyntax.AccessModifier accessModifier = Gum.Core.AbstractSyntax.AccessModifier.None;
        //        if (!lexer.ConsumeAny(out accessModifierToken, TokenType.Public, TokenType.Private, TokenType.Protected))
        //            accessModifierToken = TokenType.Private;

        //        switch(accessModifierToken)
        //        {
        //            case TokenType.Public: accessModifier = Gum.Core.AbstractSyntax.AccessModifier.Public; break;
        //            case TokenType.Private: accessModifier = Gum.Core.AbstractSyntax.AccessModifier.Private; break;
        //            case TokenType.Protected: accessModifier = Gum.Core.AbstractSyntax.AccessModifier.Protected; break;
        //        }

        //        VarDecl varDecl;
        //        if (!ParseVarDecl(lexer, out varDecl))
        //        {
        //            cvd = null;
        //            return false;
        //        }

        //        cvd = new ClassVarDecl(varDecl.Type, varDecl.NameAndExps, accessModifier);
        //        scope.Accept();
        //        return true;
        //    }
        //}

        //private bool ParseClassFuncDecl(Lexer lexer, out ClassFuncDecl cfd)
        //{
        //    using (var scope = lexer.CreateScope())
        //    {
        //        // option, accessor
        //        Lexer.TokenKind accessorKind;
        //        if (!lexer.ConsumeAny(out accessorKind, TokenType.Public, TokenType.Private, TokenType.Protected))
        //            accessorKind = TokenType.Private;

        //        // virtual, override, new
        //        // new virtual
        //        Lexer.TokenKind inheritKind;
        //        if (!lexer.ConsumeAny(out inheritKind, TokenType.Virtual, TokenType.Override, TokenType.New))
        //            inheritKind = TokenType.New;

        //        FuncDecl funcDecl;
        //        if (!ParseFuncDecl(lexer, out funcDecl))
        //        {
        //            cfd = null;
        //            return false;
        //        }

        //        // 가능한 함수의 정의
        //        // 일반, virtual(이전 정의가 없을 때), override, sealed 
        //        // new virtual(이전 정의가 있을 때), new 일반(이전 정의가 있을 때)                

        //        cfd = new ClassFuncDecl(accessorKind, inheritKind, funcDecl,  );
        //        scope.Accept();
        //        return true;
        //    }            
        //}

        /* public bool ParseREPLStmt(string str, out IREPLStmt replStmt)
        {
            var lexer = new Lexer(str);
            lexer.NextToken();

            FuncDecl funcDecl;
            if (ParseFuncDecl(lexer, out funcDecl))
            {
                replStmt = funcDecl;
                return true;
            }

            ExpStmt expStmt;
            if (ParseExpStmt(lexer, out expStmt))
            {
                replStmt = expStmt;
                return true;
            }

            VarDecl varDecl;
            if (ParseVarDecl(lexer, out varDecl))
            {
                replStmt = varDecl;
                return true;
            }

            replStmt = null;
            return false;
        } */

        public bool ParseNamespaceIDs(Lexer lexer, out List<string> outNamespaceIDs)
        {
            using (var lexerScope = lexer.CreateScope())
            {
                outNamespaceIDs = null;
                var namespaceIDs = new List<string>();

                string namespaceID;

                // at least one namespace identifier                
                if (!lexer.Consume(TokenType.Identifier, out namespaceID)) return false;
                namespaceIDs.Add(namespaceID);

                while (lexer.Consume(TokenType.Dot))
                {
                    if (!lexer.Consume(TokenType.Identifier, out namespaceID)) return false;
                    namespaceIDs.Add(namespaceID);
                }

                outNamespaceIDs = namespaceIDs;
                lexerScope.Accept();
                return true;
            }
        }


        // using namespaceID.namespaceID;
        public bool ParseUsingDirective(Lexer lexer, out UsingDirective usingDirective)
        {         
            using (var lexerScope = lexer.CreateScope())
            {
                usingDirective = null;                

                // first start with 'using' keyword
                if (!lexer.Consume(TokenType.Using)) return false;
                
                List<string> namespaceIDs;
                if (!ParseNamespaceIDs(lexer, out namespaceIDs)) return false;

                if (!lexer.Consume(TokenType.SemiColon)) return false;

                usingDirective = new UsingDirective(namespaceIDs);
                lexerScope.Accept();
                return true;
            }
        }

        // ID<TypeArg0, TypeArg1, ... >
        public bool ParseIDWithTypeArgs(Lexer lexer, out IDWithTypeArgs outIDWithTypeArgs)
        {
            using (var lexerScope = lexer.CreateScope())
            {
                outIDWithTypeArgs = null;

                string id;
                if (!lexer.Consume(TokenType.Identifier, out id)) return false;

                // typeArguments are optional
                if (!lexer.Consume(TokenType.Less))
                {
                    outIDWithTypeArgs = new IDWithTypeArgs(id, Enumerable.Empty<IDWithTypeArgs>());
                    lexerScope.Accept();
                    return true;
                }

                // comma-separated
                var idWithTypeArgsList = new List<IDWithTypeArgs>();

                IDWithTypeArgs idWithTypeArgs;
                if (!ParseIDWithTypeArgs(lexer, out idWithTypeArgs)) return false;
                idWithTypeArgsList.Add(idWithTypeArgs);

                while(lexer.Consume(TokenType.Comma))
                {
                    if (!ParseIDWithTypeArgs(lexer, out idWithTypeArgs)) return false;
                    idWithTypeArgsList.Add(idWithTypeArgs);
                }

                if (!lexer.Consume(TokenType.Greater)) return false;

                outIDWithTypeArgs = new IDWithTypeArgs(id, idWithTypeArgsList);
                lexerScope.Accept();
                return true;
            }
        }

        public bool ParseMemberFuncModifier(Lexer lexer, out MemberFuncModifier? outMemberFuncModifier)
        {
            if (lexer.Consume(TokenType.Static))
            {
                outMemberFuncModifier = MemberFuncModifier.Static;
                return true;
            }
            else if (lexer.Consume(TokenType.New))
            {
                outMemberFuncModifier = MemberFuncModifier.New;
                return true;
            }
            else
            {
                outMemberFuncModifier = null;
                return false;
            }
        }
        
        public bool ParseMemberFuncDecl(Lexer lexer, out MemberFuncDecl outMemberFuncDecl)
        {
            using (var lexerScope = lexer.CreateScope())
            {
                outMemberFuncDecl = null;

                var memberFuncModifiers = new List<MemberFuncModifier>();
                AccessModifier? accessModifier = null;
                VirtualModifier? virtualModifier = null;

                while (true)
                {
                    MemberFuncModifier? memberFuncModifier;
                    if (ParseMemberFuncModifier(lexer, out memberFuncModifier))
                    {
                        // check duplicated modifier
                        if (memberFuncModifiers.IndexOf(memberFuncModifier.Value) != -1) return false;
                        
                        memberFuncModifiers.Add(memberFuncModifier.Value);
                        continue;
                    }

                    AccessModifier? parsedAccessModifier;
                    if( ParseAccessModifier(lexer, out parsedAccessModifier) )
                    {
                        if (accessModifier.HasValue) return false;
                        accessModifier = parsedAccessModifier;
                        continue;
                    }


                    VirtualModifier? parsedVirtualModifier;
                    if (ParseVirtualModifier(lexer, out virtualModifier))
                    {
                        if (virtualModifier.HasValue) return false;
                        virtualModifier = parsedVirtualModifier;

                        continue;
                    }

                    break;
                }                
                

                var typeParams = new List<string>();
                IDWithTypeArgs returnType;
                string funcName;
                var Parameters = new List<FuncParam>();
                BlockStmt bodyStmt;

                outMemberFuncDecl = new MemberFuncDecl(memberFuncModifiers, accessModifier, virtualModifier, typeParams, returnType, funcName, funcParams, bodyStmt);
                lexerScope.Accept();
                return true;
            }
        }

        // class ClassName : BaseType { }
        public bool ParseClassDecl(Lexer lexer, out ClassDecl outClassDecl)
        {
            using (var lexerScope = lexer.CreateScope())
            {
                outClassDecl = null;

                // typeVars, baseTypes, IMemberComponents                

                // first start with 'class' keyword
                if (!lexer.Consume(TokenType.Class)) return false;

                // then parse class name
                string className;
                if (!lexer.Consume(TokenType.Identifier, out className)) return false;

                // Parse typeVars, don't allow type-instantiation style, only accept identifier
                var typeVars = new List<string>();
                if( lexer.Consume(TokenType.Less) )
                {
                    string typeVar;
                    if (!lexer.Consume(TokenType.Identifier, out typeVar)) return false;
                    typeVars.Add(typeVar);

                    while ( lexer.Consume(TokenType.Comma) )
                    {
                        if (!lexer.Consume(TokenType.Identifier, out typeVar)) return false;
                        typeVars.Add(typeVar);
                    }

                    if (!lexer.Consume(TokenType.Greater)) return false;
                }

                // optional baseTypes
                var baseTypes = new List<IDWithTypeArgs>();
                if( lexer.Consume(TokenType.Colon))
                {
                    // comma-separated list
                    IDWithTypeArgs idWithTypeArgs;
                    if (!ParseIDWithTypeArgs(lexer, out idWithTypeArgs)) return false;
                    baseTypes.Add(idWithTypeArgs);

                    while(lexer.Consume(TokenType.Comma))
                    {
                        if (!ParseIDWithTypeArgs(lexer, out idWithTypeArgs)) return false;
                        baseTypes.Add(idWithTypeArgs);
                    }
                }

                if (!lexer.Consume(TokenType.LBrace)) return false;

                // IMemberComponent
                var memberComponents = new List<IMemberComponent>();
                while (!lexer.Consume(TokenType.RBrace))
                {
                    MemberFuncDecl memberFuncDecl;
                    if (ParseMemberFuncDecl(lexer, out memberFuncDecl))
                    {
                        memberComponents.Add(memberFuncDecl);
                        continue;
                    }

                    MemberVarDecl memberVarDecl;
                    if( ParseMemberVarDecl(lexer, out memberVarDecl))
                    {
                        memberComponents.Add(memberVarDecl);
                        continue;
                    }

                    return false;
                }

                outClassDecl = new ClassDecl(typeVars, className, baseTypes, memberComponents);
                lexerScope.Accept();
                return true;
            }
        }

        // namespace namespaceID.namespaceID { }
        public bool ParseNamespaceDecl(Lexer lexer, out NamespaceDecl outNamespaceDecl)
        {            
            using (var lexerScope = lexer.CreateScope())
            {
                outNamespaceDecl = null;

                // first start with 'namespace' keyword                
                if (!lexer.Consume(TokenType.Namespace)) return false;

                List<string> namespaceIDs;
                if (!ParseNamespaceIDs(lexer, out namespaceIDs)) return false;

                if (!lexer.Consume(TokenType.LBrace)) return false;

                var namespaceComponents = new List<INamespaceComponent>();
                while( !lexer.Consume(TokenType.RBrace))
                {
                    NamespaceDecl namespaceDecl;
                    if( ParseNamespaceDecl(lexer, out namespaceDecl) )
                    {
                        namespaceComponents.Add(namespaceDecl);
                        continue;
                    }

                    ClassDecl classDecl;
                    if( ParseClassDecl(lexer, out classDecl ))
                    {
                        namespaceComponents.Add(classDecl);
                        continue;
                    }

                    StructDecl structDecl;
                    if( ParseStructDecl(lexer, out structDecl ))
                    {
                        namespaceComponents.Add(structDecl);
                        continue;
                    }

                    VarDecl varDecl;
                    if( ParseVarDecl(lexer, out varDecl ))
                    {
                        namespaceComponents.Add(varDecl);
                        continue;
                    }

                    FuncDecl funcDecl;
                    if( ParseFuncDecl(lexer, out funcDecl ))
                    {
                        namespaceComponents.Add(funcDecl);
                        continue;
                    }
                }

                outNamespaceDecl = new NamespaceDecl(namespaceIDs, namespaceComponents);
                lexerScope.Accept();
                return true;
            }
        }
        
        public bool ParseFileUnit(Lexer lexer, out FileUnit fileUnit)
        {
            using (var lexerScope = lexer.CreateScope())
            {
                fileUnit = null;
                var fileUnitComponents = new List<IFileUnitComponent>();

                // 끝날때까지 
                while (!lexer.End)
                {
                    UsingDirective usingDirective;
                    if (ParseUsingDirective(lexer, out usingDirective))
                    {
                        fileUnitComponents.Add(usingDirective);
                        continue;
                    }

                    NamespaceDecl namespaceDecl;
                    if (ParseNamespaceDecl(lexer, out namespaceDecl))
                    {
                        fileUnitComponents.Add(namespaceDecl);
                        continue;
                    }

                    FuncDecl funcDecl;
                    if (ParseFuncDecl(lexer, out funcDecl))
                    {
                        fileUnitComponents.Add(funcDecl);
                        continue;
                    }

                    VarDecl varDecl;
                    if (ParseVarDecl(lexer, out varDecl))
                    {
                        fileUnitComponents.Add(varDecl);
                        continue;
                    }

                    return false;
                }

                lexerScope.Accept();
                fileUnit = new FileUnit(fileUnitComponents);
                return true;
            }
        }
        
        public bool ParseFileUnit(string str, out FileUnit fileUnit)
        {
            fileUnit = null;
            List<IFileUnitComponent> comps = new List<IFileUnitComponent>();

            var lexer = new Lexer(str);
            lexer.NextToken();

            while (!lexer.End)
            {
                // 프로그램은 변수 선언/ 함수 선언 두가지 경우

                // class definition인가
                //ClassDecl cd;
                //if (ParseClassDecl(lexer, out cd))
                //{
                //    res.Decls.Add(cd);
                //    continue;
                //}

                // Function인가?                 
                FuncDecl fd;
                if (ParseFuncDecl(lexer, out fd))
                {
                    comps.Add(fd);
                    continue;
                }

                // Variable Declaration인가?
                VarDecl vd;
                if (ParseVarDecl(lexer, out vd))
                {
                    comps.Add(vd);
                    continue;
                }
                
                return false;
            }

            fileUnit = new FileUnit(comps);
            return true;
        }        
    }
}
