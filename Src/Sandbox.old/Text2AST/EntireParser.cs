using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Data.AbstractSyntax;

namespace Gum.Translator.Text2AST
{
    public class EntireParser
    {
        // 행동이 추가될때마다 아래로 길어지는 구조.. 
        

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
        

        //public bool BinOpLessEqual(BinaryExpKind b1, BinaryExpKind b2)
        //{
        //    return BinOpToPriority(b1) <= BinOpToPriority(b2);
        //}

        
        
        // 13. ?? Null coalescing -> skip, right-associative
        // 14. ?: , right associative
        
        
        
        //bool ParseFuncDecl(Lexer lexer, out FuncDecl fd)
        //{
        //    using (LexerScope scope = lexer.CreateScope())
        //    {
        //        string typeName, name;
        //        if (!lexer.Consume(TokenType.Identifier, out typeName))
        //        {
        //            fd = null;
        //            return false;
        //        }

        //        if (!lexer.Consume(TokenType.Identifier, out name))
        //        {
        //            name = typeName;
        //            typeName = null;
        //        }

        //        var parameters = new List<FuncParam>();                
                
        //        // argument list 파싱
        //        // (
        //        if (!lexer.Consume(TokenType.LParen))
        //        {
        //            fd = null;
        //            return false;
        //        }

        //        // ) 찾기
        //        if (!lexer.Consume(TokenType.RParen))
        //        {
        //            if (!lexer.Consume(TokenType.Identifier, out typeName) ||
        //                !lexer.Consume(TokenType.Identifier, out name))
        //            {
        //                // 복구 불가능                    
        //                fd = null;
        //                return false;
        //            }
                    
        //            // TODO: Parameter Modifier 미구현 
        //            parameters.Add(new FuncParam(Empty<FuncParamModifier>(), CreateSingleID(typeName), name));

        //            while (lexer.Consume(TokenType.Comma))
        //            {
        //                if (!lexer.Consume(TokenType.Identifier, out typeName) ||
        //                    !lexer.Consume(TokenType.Identifier, out name))
        //                {
        //                    // 복구 불가능                    
        //                    fd = null;
        //                    return false;
        //                }

        //                parameters.Add(new FuncParam(Empty<FuncParamModifier>(), CreateSingleID(typeName), name));
        //            }

        //            if (!lexer.Consume(TokenType.RParen))
        //            {
        //                // 복구 불가능, ")"이 빠졌습니다
        //                fd = null;
        //                return false;
        //            }
        //        }

        //        if (!lexer.Consume(TokenType.LBrace))
        //        {
        //            fd = null;
        //            return false;
        //        }


        //        var stmts = new List<IStmtComponent>();
        //        while(!lexer.Consume(TokenType.RBrace))
        //        {
        //            IStmtComponent stmt;
        //            if (!ParseStatement(lexer, out stmt))
        //            {
        //                fd = null;
        //                return false;
        //            }

        //            stmts.Add(stmt);
        //        }                

        //        fd = new FuncDecl(Empty<string>(), CreateSingleID(typeName), name, parameters, new BlockStmt(stmts));
        //        scope.Accept();
        //        return true;
        //    }
        //}

        
        
        
        

        
        
        
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
        
    }
}
