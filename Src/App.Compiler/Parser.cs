using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.App.Compiler.AST;

namespace Gum.App.Compiler
{
    public class Parser
    {
        // 0x, 123 둘다 해석함
        public bool ParseInteger(Lexer lexer, out IntegerExp i)
        {
            i = null;

            if (lexer.Kind != Lexer.TokenKind.IntValue)
                return false;

            using (LexerScope scope = lexer.CreateScope())
            {
                int res;
                if (int.TryParse(lexer.Token, out res))
                {
                    // 성공했으면 다음 토큰으로 넘어감
                    lexer.NextToken();
                    i = new IntegerExp() { Value = res };
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
                case BinaryExpKind.And: return 1;
                case BinaryExpKind.Or: return 1;
            
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
        private bool ParsePostfixUnary(Lexer lexer, out UnaryExpKind unOp)
        {
            if (lexer.Kind == Lexer.TokenKind.PlusPlus)
            {
                unOp = UnaryExpKind.PostfixInc;
                lexer.NextToken();
                return true;
            }
            else if (lexer.Kind == Lexer.TokenKind.MinusMinus)
            {
                unOp = UnaryExpKind.PostfixDec;
                lexer.NextToken();
                return true;
            }
            else
            {
                unOp = UnaryExpKind.Invalid;
                return false;
            }
        }       
        

        // Operator를 제외한 나머지를 파싱할 수 있다
        private bool ParseOperand(Lexer lexer, out IExp exp)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                // () 괄호 처리
                if (lexer.Consume(Lexer.TokenKind.LParen))
                {
                    IExp insideExp;
                    if (!ParseExp(lexer, out insideExp))
                    {
                        exp = null;
                        return false;
                    }

                    if (lexer.Kind != Lexer.TokenKind.RParen)
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
                if (lexer.Consume(Lexer.TokenKind.Identifier, out id))
                {
                    // 1. 그냥 Variable일 수 있고,
                    // 2. Offset을 동반한 Variable일 수 있고,                    
                    
                    // TODO: Function as First class citizen
                    if (lexer.Kind == Lexer.TokenKind.Dot ||
                        lexer.Kind == Lexer.TokenKind.LBracket)
                    {
                        VariableExp ve = new VariableExp() { Name = id };
                        if (!ParseOffset(lexer, ve))
                        {
                            exp = null;
                            return false; // 복구 불가능.. 
                        }
                        exp = ve;
                    }
                    else
                    {
                        exp = new VariableExp() { Name = id };
                    }

                    scope.Accept();
                    return true;
                }


                if (lexer.Kind == Lexer.TokenKind.IntValue)
                {
                    exp = new IntegerExp() { Value = int.Parse(lexer.Token) };
                    lexer.NextToken();
                }
                else if (lexer.Kind == Lexer.TokenKind.StringValue)
                {
                    exp = new StringExp() { Value = lexer.Token.Substring(1, lexer.Token.Length - 2) };
                    lexer.NextToken();
                }
                else if (lexer.Kind == Lexer.TokenKind.TrueValue)
                {
                    exp = new BoolExp() { Value = true };
                    lexer.NextToken();
                }
                else if (lexer.Kind == Lexer.TokenKind.FalseValue)
                {
                    exp = new BoolExp() { Value = false };
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
        
        public bool ParseBinaryOperation(Lexer lexer, out BinaryExpKind binOp)
        {
            // 1. && ||           // 맨 나중에 계산
            // 2. == != 
            // 3. < <= > >=       // 
            // 3. + -
            // 4. * /             // 제일 먼저 계산

            switch(lexer.Kind)
            {
                case Lexer.TokenKind.AmperAmper: binOp = BinaryExpKind.And; break;
                case Lexer.TokenKind.BarBar:  binOp = BinaryExpKind.Or; break;

                case Lexer.TokenKind.EqualEqual: binOp = BinaryExpKind.Equal; break;
                case Lexer.TokenKind.NotEqual: binOp = BinaryExpKind.NotEqual; break;

                case Lexer.TokenKind.Less: binOp = BinaryExpKind.Less; break;
                case Lexer.TokenKind.LessEqual: binOp = BinaryExpKind.LessEqual; break;
                case Lexer.TokenKind.Greater: binOp = BinaryExpKind.Greater; break;
                case Lexer.TokenKind.GreaterEqual: binOp = BinaryExpKind.GreaterEqual; break;
                case Lexer.TokenKind.Plus: binOp = BinaryExpKind.Add; break;
                case Lexer.TokenKind.Minus: binOp = BinaryExpKind.Sub; break;
                case Lexer.TokenKind.Star: binOp = BinaryExpKind.Mul; break;
                case Lexer.TokenKind.Slash: binOp = BinaryExpKind.Div; break;
                default:
                    binOp = BinaryExpKind.Invalid;
                    return false;
            }
            
            lexer.NextToken();
            return true;
        }

        public bool ParseOffset(Lexer lexer, VariableExp v)
        {
            using (LexerScope scope = lexer.CreateScope())
            {                
                while (!lexer.End)
                {
                    if (lexer.Consume(Lexer.TokenKind.Dot))
                    {
                        string fieldName;
                        if (!lexer.Consume(Lexer.TokenKind.Identifier, out fieldName))
                            return false; // 복구 불가능

                        v.Offsets.Add(new StringOffset() { Field = fieldName });
                    }
                    else if (lexer.Consume(Lexer.TokenKind.LBracket))
                    {
                        string index;
                        if (!lexer.Consume(Lexer.TokenKind.IntValue, out index))
                            return false;

                        v.Offsets.Add(new IndexOffset() { Index = int.Parse(index) });
                    }
                    else break;
                }

                scope.Accept();
                return true;
            }
        }

        public bool ParseNewExp(Lexer lexer, out NewExp ne)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                string typeName;
                if (!lexer.Consume(Lexer.TokenKind.New) ||
                    !lexer.Consume(Lexer.TokenKind.Identifier, out typeName) ||
                    !lexer.Consume(Lexer.TokenKind.LParen))
                {
                    ne = null;
                    return false;
                }

                ne = new NewExp(typeName);

                // 그냥 빈 것
                if (lexer.Consume(Lexer.TokenKind.RParen))
                {
                    scope.Accept();
                    return true;
                }

                // 아니라면 적어도 하나는 있을 것이다
                do
                {
                    IExp exp;
                    if (!ParseExp(lexer, out exp))
                    {
                        ne = null;
                        return false;
                    }

                    ne.Args.Add(exp);
                } while (lexer.Consume(Lexer.TokenKind.Comma));

                if (!lexer.Consume(Lexer.TokenKind.RParen))
                {
                    ne = null;
                    return false;
                }

                scope.Accept();
                return true;
            }

        }

        public bool ParseCallExp(Lexer lexer, out CallExp ce)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                string funcName;
                if (!lexer.Consume(Lexer.TokenKind.Identifier, out funcName) ||
                    !lexer.Consume(Lexer.TokenKind.LParen))
                {
                    ce = null;
                    return false;
                }

                ce = new CallExp(funcName);
                
                // 그냥 빈 것
                if (lexer.Consume(Lexer.TokenKind.RParen))
                {
                    scope.Accept();
                    return true;
                }

                // 아니라면 적어도 하나는 있을 것이다
                do
                {
                    IExp exp;
                    if (!ParseExp(lexer, out exp))
                    {
                        ce = null;
                        return false;
                    }

                    ce.Args.Add(exp);
                } while (lexer.Consume(Lexer.TokenKind.Comma));

                if (!lexer.Consume(Lexer.TokenKind.RParen))
                {
                    ce = null;
                    return false;
                }
                
                scope.Accept();
                return true;
            }
        }   
     
        private void ArrangeOperations(Stack<IExp> operands, Stack<BinaryExpKind> ops, BinaryExpKind binOp, IExp operand)
        {
            while(ops.Count != 0 && BinOpLessEqual(binOp, ops.Peek()))
            {
                // merge
                IExp right = operands.Pop();
                IExp left = operands.Pop();
                BinaryExpKind op = ops.Pop();

                IExp binExp = new BinaryExp() { Operand1 = left, Operand2 = right, Operation = op};
                operands.Push(binExp);
            }

            ops.Push(binOp);
            operands.Push(operand);
            return;
        }

        bool ParsePreUnaryAndOperand(Lexer lexer, out IExp exp)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                UnaryExpKind prefixUnOp;
                ConsumePrefixUnary(lexer, out prefixUnOp);

                if (!ParseOperand(lexer, out exp))
                {
                    // expression은 operand로 시작해야 합니다.
                    exp = null;
                    return false;
                }

                if (prefixUnOp != UnaryExpKind.Invalid)
                    exp = new UnaryExp() { Operand = exp, Operation = prefixUnOp };

                scope.Accept();
                return true;
            }
        }
        
        public bool ParseExp(Lexer lexer, out IExp exp)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                Stack<IExp> operands = new Stack<IExp>();
                Stack<BinaryExpKind> ops = new Stack<BinaryExpKind>();

                IExp operand;

                if (!ParsePreUnaryAndOperand(lexer, out operand))
                {
                    // expression은 operand로 시작해야 합니다.
                    exp = null;
                    return false;
                }

                operands.Push(operand);

                while (!lexer.End)
                {
                    UnaryExpKind postfixUnOp;

                    // Unary 는 가장 먼저 묶는다
                    if (ParsePostfixUnary(lexer, out postfixUnOp))
                    {
                        IExp unOperand = operands.Pop();
                        if (unOperand is UnaryExp)
                        {
                            throw new System.Exception("단항연산자는 중복될 수 없습니다");
                        }

                        IExp unExp = new UnaryExp() { Operand = unOperand, Operation = postfixUnOp };
                        operands.Push(unExp);
                        continue;
                    }

                    // BinaryExpKind을 가져온다
                    BinaryExpKind binOp;
                    if (!ParseBinaryOperation(lexer, out binOp))
                    {
                        // 더 이상 BinaryExpKind이 없을 경우 나머지 처리를 하고 정상종료를 한다.
                        break;
                    }

                    if (!ParsePreUnaryAndOperand(lexer, out operand))
                    {
                        // BinaryExpKind의 대상이 없습니다
                        exp = null;
                        return false;
                    }

                    ArrangeOperations(operands, ops, binOp, operand);
                }

                // 이제 남은 것들을 뒤에서 부터 묶는다.
                IExp right = operands.Pop();
                Debug.Assert(operands.Count == ops.Count);
                while (ops.Count != 0)
                {
                    BinaryExpKind op = ops.Pop();
                    IExp left = operands.Pop();

                    right = new BinaryExp() { Operand1 = left, Operation = op, Operand2 = right };
                }

                exp = right;
                scope.Accept();
                return true;
            }
        }

        bool ConsumePrefixUnary(Lexer lexer, out UnaryExpKind prefixUnOp)
        {
            switch(lexer.Kind)
            {
                case Lexer.TokenKind.Not:
                    prefixUnOp = UnaryExpKind.Not;
                    break;

                case Lexer.TokenKind.Minus:
                    prefixUnOp = UnaryExpKind.Neg;
                    break;

                case Lexer.TokenKind.PlusPlus:
                    prefixUnOp = UnaryExpKind.PrefixInc;
                    break;

                case Lexer.TokenKind.MinusMinus:
                    prefixUnOp = UnaryExpKind.PrefixDec;
                    break;

                default: 
                    prefixUnOp = UnaryExpKind.Invalid;
                    break;
            }

            if (prefixUnOp != UnaryExpKind.Invalid)
            {
                lexer.NextToken();
                return true;
            }

            return false;
        }

        bool ParseVarDeclStmt(Lexer lexer, out VarDeclStmt vd)
        {
            VarDecl varDecl;
            if (!ParseVarDecl(lexer, out varDecl))
            {
                vd = null;
                return false;
            }

            vd = new VarDeclStmt(varDecl);
            return true;
        }

        bool ParseVarDecl(Lexer lexer, out VarDecl vd)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                string typeName;

                if (!lexer.Consume(Lexer.TokenKind.Identifier, out typeName))
                {
                    vd = null;
                    return false;
                }

                vd = new VarDecl(typeName);

                do
                {
                    string name;
                    if (!lexer.Consume(Lexer.TokenKind.Identifier, out name))
                    {
                        vd = null;
                        return false;
                    }

                    IExp exp;
                    ParseInitializer(lexer, out exp);
                    
                    vd.NameAndExps.Add(new NameAndExp() { Name = name, Exp = exp });

                } while (lexer.Consume(Lexer.TokenKind.Comma));

                if (!lexer.Consume(Lexer.TokenKind.SemiColon))
                {
                    vd = null;
                    return false;
                }

                scope.Accept();
                return true;
            }
        }        

        bool ParseInitializer(Lexer lexer, out IExp exp)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                if (!lexer.Consume(Lexer.TokenKind.Equal) ||
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
                if (!lexer.Consume(Lexer.TokenKind.Identifier, out typeName))
                {
                    fd = null;
                    return false;
                }

                if (!lexer.Consume(Lexer.TokenKind.Identifier, out name))
                {
                    name = typeName;
                    typeName = null;
                }

                FuncDecl res = new FuncDecl(typeName, name);
                
                // argument list 파싱
                // (
                if (!lexer.Consume(Lexer.TokenKind.LParen))
                {
                    fd = null;
                    return false;
                }

                // ) 찾기
                if (!lexer.Consume(Lexer.TokenKind.RParen))
                {
                    if (!lexer.Consume(Lexer.TokenKind.Identifier, out typeName) ||
                        !lexer.Consume(Lexer.TokenKind.Identifier, out name))
                    {
                        // 복구 불가능                    
                        fd = null;
                        return false;
                    }
                    
                    res.Parameters.Add(new TypeAndName(typeName, name));

                    while (lexer.Consume(Lexer.TokenKind.Comma))
                    {
                        if (!lexer.Consume(Lexer.TokenKind.Identifier, out typeName) ||
                            !lexer.Consume(Lexer.TokenKind.Identifier, out name))
                        {
                            // 복구 불가능                    
                            fd = null;
                            return false;
                        }

                        res.Parameters.Add(new TypeAndName(typeName, name));
                    }

                    if (!lexer.Consume(Lexer.TokenKind.RParen))
                    {
                        // 복구 불가능, ")"이 빠졌습니다
                        fd = null;
                        return false;
                    }
                }

                if (lexer.Consume(Lexer.TokenKind.SemiColon))
                {
                    // extern 함수
                    res.Body = null;
                    fd = res;
                    scope.Accept();
                    return true;
                }

                // 함수 바디 불러오기..
                BlockStmt block;
                if (!ParseBlock(lexer, out block))
                {
                    fd = null;                    
                    return false;
                }

                res.Body = block;

                fd = res;
                scope.Accept();
                return true;
            }
        }

        bool ParseBlock(Lexer lexer, out BlockStmt block)
        {
            BlockStmt res = new BlockStmt();
            using (LexerScope scope = lexer.CreateScope())
            {
                if (lexer.Kind != Lexer.TokenKind.LBrace)
                {
                    // Block이 아님
                    block = null;
                    return false;
                }

                lexer.NextToken();

                while (lexer.Kind != Lexer.TokenKind.RBrace)
                {   
                    IStmt stmt;

                    if (!ParseStatement(lexer, out stmt))
                    {   
                        block = null;
                        return false;
                    }

                    res.Stmts.Add(stmt);
                }
                lexer.NextToken();
                
                block = res;
                scope.Accept();
                return true;
            }
        }

        bool ParseIfStatement(Lexer lexer, out IfStmt ifStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                // if ( 부분
                IExp condExp;
                IStmt thenStmt;

                if (!lexer.ConsumeSeq(Lexer.TokenKind.If, Lexer.TokenKind.LParen) ||
                    !ParseExp(lexer, out condExp) || 
                    !lexer.Consume(Lexer.TokenKind.RParen) ||
                    !ParseStatement(lexer, out thenStmt))
                {
                    ifStmt = null;
                    return false;
                }
                
                IStmt elseStmt = null;
                if (lexer.Consume(Lexer.TokenKind.Else))
                {                    
                    if (!ParseStatement(lexer, out elseStmt))
                    {
                        ifStmt = null;
                        return false;
                    }
                }

                ifStmt = new IfStmt() { CondExp = condExp, ThenStmt = thenStmt, ElseStmt = elseStmt };
                scope.Accept();
                return true;
            }
        }

        bool ParseStatement(Lexer lexer, out IStmt stmt)
        {
            // Statement 
            // 1. Null Statement (';' Only)
            // 2. 변수 선언 Variable Declaration ;
            // - If            
            // - For, Do, While
            // - Return Exp;
            // - Expression-Statement;
            // - Block { }

            using (LexerScope scope = lexer.CreateScope())
            {   
                // 1. Null Statement 
                if (lexer.Kind == Lexer.TokenKind.SemiColon)
                {
                    lexer.NextToken();
                    stmt = new NullStmt();
                    scope.Accept();
                    return true;
                }

                // 2. Variable Declaration with ';'
                VarDeclStmt vd;
                if (ParseVarDeclStmt(lexer, out vd))
                {
                    stmt = vd;
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
                IExp exp;
                // 빈 익스프레션도 받기 때문에 성공 여부가 중요하지 않다.
                ParseExp(lexer, out exp);
                
                if (!lexer.Consume(Lexer.TokenKind.SemiColon))
                {
                    expStmt = null;
                    return false;
                }
                
                expStmt = new ExpStmt() { Exp = exp };
                scope.Accept();
                return true;                
            }
        }

        private bool ParseForStatement(Lexer lexer, out ForStmt forStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                if (!lexer.Consume(Lexer.TokenKind.For) ||
                    !lexer.Consume(Lexer.TokenKind.LParen))
                {
                    forStmt = null;
                    return false;
                }

                // 첫번째에 들어갈 수 있는 것들..
                // Nothing
                // Variable Declaration With Initial value
                // Expression
                IStmt init = null;
                VarDeclStmt vd;
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
                IExp condExp = null;
                if (!lexer.Consume(Lexer.TokenKind.SemiColon))
                {                    
                    if (!ParseExp(lexer, out condExp) ||
                        !lexer.Consume(Lexer.TokenKind.SemiColon))
                    {
                        forStmt = null;
                        return false;
                    }
                }
                
                // 세번째에 들어갈 수 있는 것, IExp
                IExp loopExp = null;
                if (!lexer.Consume(Lexer.TokenKind.RParen))
                {
                    if (!ParseExp(lexer, out loopExp) ||
                        !lexer.Consume(Lexer.TokenKind.RParen))
                    {
                        forStmt = null;
                        return false;
                    }
                }

                IStmt body;
                if (!ParseStatement(lexer, out body))
                {
                    forStmt = null;
                    return false;
                }

                forStmt = new ForStmt() { Initializer = init, CondExp = condExp, LoopExp = loopExp, Body = body };
                scope.Accept();
                return true;
            }
        }

        private bool ParseWhileStmt(Lexer lexer, out WhileStmt whileStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                IExp condExp;

                if (!lexer.Consume(Lexer.TokenKind.While) ||
                    !lexer.Consume(Lexer.TokenKind.LParen) ||
                    !ParseExp(lexer, out condExp) ||
                    !lexer.Consume(Lexer.TokenKind.RParen))
                {
                    whileStmt = null;
                    return false;
                }

                IStmt body;
                if (!ParseStatement(lexer, out body))
                {
                    whileStmt = null;
                    return false;
                }

                whileStmt = new WhileStmt() { CondExp = condExp, Body = body };
                scope.Accept();
                return true;
            }
        }

        private bool ParseDoWhileStmt(Lexer lexer, out DoWhileStmt doWhileStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {                
                IStmt body;

                if (!lexer.Consume(Lexer.TokenKind.Do) ||
                    !ParseStatement(lexer, out body))
                {
                    doWhileStmt = null;
                    return false;
                }

                IExp condExp;
                if (!lexer.Consume(Lexer.TokenKind.While) ||
                    !lexer.Consume(Lexer.TokenKind.LParen) ||
                    !ParseExp(lexer, out condExp) ||
                    !lexer.Consume(Lexer.TokenKind.RParen) ||
                    !lexer.Consume(Lexer.TokenKind.SemiColon))
                {
                    doWhileStmt = null;
                    return false;
                }
                
                doWhileStmt = new DoWhileStmt() { Body = body, CondExp = condExp };
                scope.Accept();
                return true;
            }
        }

        bool ParseReturnStmt(Lexer lexer, out ReturnStmt returnStmt)
        {
            using (LexerScope scope = lexer.CreateScope())
            {
                if (!lexer.Consume(Lexer.TokenKind.Return))
                {
                    returnStmt = null;
                    return false;
                }

                if (lexer.Consume(Lexer.TokenKind.SemiColon))
                {
                    returnStmt = new ReturnStmt() { ReturnExp = null };
                    scope.Accept();
                    return true;
                }

                IExp exp = null;
                if (!ParseExp(lexer, out exp) || 
                    !lexer.Consume(Lexer.TokenKind.SemiColon))
                {
                    returnStmt = null;
                    return false;
                }

                returnStmt = new ReturnStmt() { ReturnExp = exp };
                scope.Accept();
                return true;
            }
        }

        bool ParseContinueStmt(Lexer lexer, out ContinueStmt continueStmt)
        {
            continueStmt = null;
            using (LexerScope scope = lexer.CreateScope())
            {
                if (lexer.Consume(Lexer.TokenKind.Continue) &&
                    lexer.Consume(Lexer.TokenKind.SemiColon))
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
                if (lexer.Consume(Lexer.TokenKind.Break) &&
                    lexer.Consume(Lexer.TokenKind.SemiColon))
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
            aStmt = null;

            using (LexerScope scope = lexer.CreateScope())
            {
                VariableExp varExp;
                IExp exp;
                Lexer.TokenKind tokenKind;                
                
                if (!ParseVariable(lexer, out varExp) ||
                    !lexer.ConsumeAny(out tokenKind, Lexer.TokenKind.Equal,
                        Lexer.TokenKind.PlusEqual,
                        Lexer.TokenKind.MinusEqual,
                        Lexer.TokenKind.StarEqual,
                        Lexer.TokenKind.SlashEqual) ||
                    !ParseExp(lexer, out exp))
                {                    
                    return false;
                }

                switch(tokenKind)
                {
                    case Lexer.TokenKind.Equal:
                        aStmt = new AssignExp() { Var = varExp, Exp = exp };
                        break;

                    case Lexer.TokenKind.PlusEqual:
                        aStmt = new AssignExp()
                        {
                            Var = varExp,
                            Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Add }
                        };
                        break;

                    case Lexer.TokenKind.MinusEqual:
                        aStmt = new AssignExp()
                        {
                            Var = varExp,
                            Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Add }
                        };
                        break;

                    case Lexer.TokenKind.StarEqual:
                        aStmt = new AssignExp()
                        {
                            Var = varExp,
                            Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Mul }
                        };
                        break;

                    case Lexer.TokenKind.SlashEqual:
                        aStmt = new AssignExp()
                        {
                            Var = varExp,
                            Exp = new BinaryExp() { Operand1 = varExp, Operand2 = exp, Operation = BinaryExpKind.Div }
                        };
                        break;
                }
                scope.Accept();
                return true;
            }            
        }

        bool ParseVariable(Lexer lexer, out VariableExp varExp)
        {
            if (lexer.Kind != Lexer.TokenKind.Identifier)
            {
                varExp = null;
                return false;
            }

            string id = lexer.Token;
            using (LexerScope scope = lexer.CreateScope())
            {
                lexer.NextToken();

                VariableExp ve = new VariableExp() { Name = id };
                if (!ParseOffset(lexer, ve))
                {
                    varExp = null;
                    return false;
                }

                varExp = ve;
                scope.Accept();
                return true;
            }
        }

        public bool ParseClassDecl(Lexer lexer, out ClassDecl classDecl)
        {
            using (var scope = lexer.CreateScope())
            {
                string className;

                if (!lexer.Consume(Lexer.TokenKind.Class) ||
                    !lexer.Consume(Lexer.TokenKind.Identifier, out className))
                {
                    classDecl = null;
                    return false;
                }

                classDecl = new ClassDecl(className);

                // 상속 목록
                // : ID(, ID)*

                if (lexer.Consume(Lexer.TokenKind.Colon))
                {
                    string baseTypeName;

                    if (!lexer.Consume(Lexer.TokenKind.Identifier, out baseTypeName))
                    {
                        // 복구 불가능한 오류
                        classDecl = null;
                        return false;
                    }

                    classDecl.BaseTypes.Add(baseTypeName);

                    while(lexer.Consume(Lexer.TokenKind.Comma))
                    {
                        if (!lexer.Consume(Lexer.TokenKind.Identifier, out baseTypeName))
                        {
                            // 복구 불가능한 오류
                            classDecl = null;
                            return false;
                        }

                        classDecl.BaseTypes.Add(baseTypeName);
                    }
                }

                if (!lexer.Consume(Lexer.TokenKind.LBrace))
                {
                    classDecl = null;
                    return false;
                }

                while (!lexer.Consume(Lexer.TokenKind.RBrace))
                {
                    ClassFuncDecl cfd;
                    if (ParseClassFuncDecl(lexer, out cfd))
                    {
                        classDecl.FuncDecls.Add(cfd);
                        continue;
                    }

                    ClassVarDecl cvd;
                    if (ParseClassVarDecl(lexer, out cvd))
                    {
                        classDecl.VarDecls.Add(cvd);
                        continue;
                    }
                }

                scope.Accept();
                return true;
            }
        }

        private bool ParseClassVarDecl(Lexer lexer, out ClassVarDecl cvd)
        {
            // option, accessor (기본 private)
            using (var scope = lexer.CreateScope())
            {
                // option, accessor
                Lexer.TokenKind accessorKind;
                if (!lexer.ConsumeAny(out accessorKind, Lexer.TokenKind.Public, Lexer.TokenKind.Private, Lexer.TokenKind.Protected))
                    accessorKind = Lexer.TokenKind.Private;

                VarDecl varDecl;
                if (!ParseVarDecl(lexer, out varDecl))
                {
                    cvd = null;
                    return false;
                }

                cvd = new ClassVarDecl(accessorKind, varDecl);
                scope.Accept();
                return true;
            }
        }

        private bool ParseClassFuncDecl(Lexer lexer, out ClassFuncDecl cfd)
        {
            using (var scope = lexer.CreateScope())
            {
                // option, accessor
                Lexer.TokenKind accessorKind;
                if (!lexer.ConsumeAny(out accessorKind, Lexer.TokenKind.Public, Lexer.TokenKind.Private, Lexer.TokenKind.Protected))
                    accessorKind = Lexer.TokenKind.Private;

                // virtual, override, new
                Lexer.TokenKind inheritKind;
                if (!lexer.ConsumeAny(out inheritKind, Lexer.TokenKind.Virtual, Lexer.TokenKind.Override, Lexer.TokenKind.New))
                    inheritKind = Lexer.TokenKind.New;

                FuncDecl funcDecl;
                if (!ParseFuncDecl(lexer, out funcDecl))
                {
                    cfd = null;
                    return false;
                }

                cfd = new ClassFuncDecl(accessorKind, inheritKind, funcDecl);
                scope.Accept();
                return true;
            }            
        }

        public bool ParseProgram(string str, out Program pgm)
        {
            pgm = null;
            Program res = new Program();

            var lexer = new Lexer(str);
            lexer.NextToken();

            while (!lexer.End)
            {
                // 프로그램은 변수 선언/ 함수 선언 두가지 경우

                // class definition인가
                ClassDecl cd;
                if (ParseClassDecl(lexer, out cd))
                {
                    res.Decls.Add(cd);
                    continue;
                }

                // Function인가?                 
                FuncDecl fd;
                if (ParseFuncDecl(lexer, out fd))
                {
                    res.Decls.Add(fd);
                    continue;
                }

                // Variable Declaration인가?
                VarDecl vd;
                if (ParseVarDecl(lexer, out vd))
                {
                    res.Decls.Add(vd);
                    continue;
                }
                
                return false;
            }

            pgm = res;
            return true;
        }        
    }
}
