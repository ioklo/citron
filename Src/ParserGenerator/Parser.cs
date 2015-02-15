using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParserGenerator.AST;

namespace ParserGenerator
{
    public enum Token
    {
        Invalid,
        String, 
        Regex,
        Equal,
        Star,
        Bar,
        Plus,
        Question,
        LParen,
        RParen,
        LBrace,
        RBrace,
        LBracket,
        RBracket,
        Comma,
        ID,
    }

    class Parser
    {
        Lexer lexer;

        public Parser(string code)
        {
            lexer = new Lexer(code);
        }
        
        bool ParseTokenDecl(out TokenDeclNode tokenDeclNode)
        {
            tokenDeclNode = null;

            using (LexerScope scope = lexer.CreateScope("TokenDecl"))
            {
                if (scope == null) return false;

                string name;
                if (!lexer.Consume(Token.ID, out name)) return false;

                if (!lexer.Consume(Token.Equal)) return false;
                
                string str, regex;
                if (lexer.Consume(Token.String, out str))
                    tokenDeclNode = new TokenDeclNode(name, str, null);
                else if (lexer.Consume(Token.Regex, out regex))
                    tokenDeclNode = new TokenDeclNode(name, null, regex);
                else 
                    return false;

                scope.Accept();
                return true;
            }
        }

        // 그냥 손으로 짜는 
        bool ParseExp(out ExpNode expNode)
        {
            using(LexerScope scope = lexer.CreateScope("ParseExp"))
            {
                List<ExpNode> barExps = new List<ExpNode>();
                List<ExpNode> exps = new List<ExpNode>();

                while (!lexer.EOF)
                {
                    // 괄호가 있다면 괄호를 처리하고 집어넣기
                    ExpNode childNode;
                    if (ParseParenthesis(out childNode))
                    {
                        exps.Add(childNode);
                        continue;
                    }

                    // + * ? 처리
                    if (exps.Count > 0)
                    {
                        if( lexer.Consume(Token.Plus) )
                        {
                            exps[exps.Count - 1] = new OneOrMoreNode(exps[exps.Count - 1]);
                            continue;
                        }
                        else if (lexer.Consume(Token.Star))
                        {
                            exps[exps.Count - 1] = new ZeroOrMoreNode(exps[exps.Count - 1]);
                            continue;
                        }
                        else if (lexer.Consume(Token.Question))
                        {
                            exps[exps.Count - 1] = new OptionalNode(exps[exps.Count - 1]);
                            continue;
                        }
                        else if (lexer.Consume(Token.Bar))
                        {
                            if( exps.Count == 1 )
                                barExps.Add(exps[0]);
                            else 
                                barExps.Add(new ExpSequenceNode(exps));
                            exps.Clear();
                            continue;
                        }
                    }
                
                    // 추가
                    VariableNode varNode;
                    if (ParseVariable(out varNode)) 
                    {
                        exps.Add(varNode);
                        continue;
                    }

                    break;
                }

                // Bar가 나오고 나서 부터 exp가 하나도 없었다
                if (exps.Count == 0) 
                {
                    expNode = null;
                    return false;
                }

                if( barExps.Count == 0 )
                {
                    if (exps.Count == 1)
                        expNode = exps[0];
                    else
                        expNode = new ExpSequenceNode(exps);
                }
                else
                {                
                    if( exps.Count == 1 )
                        barExps.Add(exps[0]);
                    else 
                        barExps.Add(new ExpSequenceNode(exps));

                    expNode = new BarNode(barExps);
                }

                scope.Accept();
                return true;
            }

        }

        // Exp를 파싱하는데
        // 다음에 ?가 나오면 최대한 거기까지 파싱을 해야 한다
        // 즉, 1) Exp ?+*
        //     아니라면 단일 Exp

        //bool ParseExp2(out ExpNode expNode)
        //{
        //    ZeroOrMoreNode zeroOrMoreNode;
        //    if (ParseZeroOrMore(out zeroOrMoreNode))
        //    {
        //        expNode = zeroOrMoreNode;
        //        return true;
        //    }

        //    OneOrMoreNode oneOrMoreNode;
        //    if (ParseOneOrMore(out oneOrMoreNode))
        //    {
        //        expNode = oneOrMoreNode;
        //        return true;
        //    }

        //    OptionalNode optionalNode;
        //    if (ParseOptional(out optionalNode))
        //    {
        //        expNode = optionalNode;
        //        return true;
        //    }

        //    if (ParseParenthesis(out expNode))
        //    {
        //        return true;
        //    }                

        //    LookaheadExpNode lookaheadExpNode;
        //    if (ParseLookaheadExp(out lookaheadExpNode))
        //    {
        //        expNode = lookaheadExpNode;
        //        return true;
        //    }

        //    expNode = null;
        //    return false;            
        //}

        bool ParseLookaheadExp(out LookaheadExpNode lookaheadExpNode)
        {
            VariableNode varNode;
            if (ParseVariable(out varNode))
            {
                lookaheadExpNode = varNode;
                return true;
            }

            BarNode barNode;
            if (ParseBar(out barNode))
            {
                lookaheadExpNode = barNode;
                return true;
            }

            lookaheadExpNode = null;
            return false;
        }

        bool ParseVariable(out VariableNode varNode)
        {
            string id;
            if (lexer.Consume(Token.ID, out id))
            {
                varNode = new VariableNode(id);
                return true;
            }

            varNode = null;
            return false;
        }

        bool ParseBar(out BarNode barNode)
        {
            barNode = null;
            return false;

            //using(var scope = lexer.CreateScope("Bar"))
            //{
            //    if (scope == null) return false;

            //    ExpNode e1, e2;

            //    // 무한루프 가능성
            //    if (!ParseExp(out e1)) return false;

            //    if (!lexer.Consume(Token.Bar)) return false;

            //    if (!ParseExp(out e2)) return false;

            //    barNode = new BarNode(e1, e2);
            //    scope.Accept();
            //    return true;
            //}
        }

        bool ParseZeroOrMore(out ZeroOrMoreNode zeroOrMoreNode)
        {
            zeroOrMoreNode = null;

            using(var scope = lexer.CreateScope("ZeroOrMore"))
            {
                if (scope == null) return false;

                ExpNode e;
                if (!ParseExp(out e)) return false;

                if (!lexer.Consume(Token.Star)) return false;

                zeroOrMoreNode = new ZeroOrMoreNode(e);
                scope.Accept();
                return true;
            }            
        }

        bool ParseParenthesis(out ExpNode expNode)
        {
            expNode = null;

            using (var scope = lexer.CreateScope("Parenthesis"))
            {
                if (scope == null) return false;

                if (!lexer.Consume(Token.LParen)) return false;

                ExpNode e;
                if (!ParseExp(out e)) return false;

                if (!lexer.Consume(Token.RParen)) return false;

                expNode = e;
                scope.Accept();
                return true;
            }            
        }

        bool ParseOptional(out OptionalNode optionalNode)
        {
            optionalNode = null;

            using (var scope = lexer.CreateScope("Optional"))
            {
                if (scope == null) return false;

                ExpNode e;
                if (!ParseExp(out e)) return false;

                if (!lexer.Consume(Token.Question)) return false;

                optionalNode = new OptionalNode(e);
                scope.Accept();
                return true;
            }
        }

        bool ParseOneOrMore(out OneOrMoreNode oneOrMoreNode)
        {
            oneOrMoreNode = null;

            using (var scope = lexer.CreateScope("OneOrMore"))
            {
                if (scope == null) return false;

                ExpNode e;
                if (!ParseExp(out e)) return false;

                if (!lexer.Consume(Token.Plus)) return false;

                oneOrMoreNode = new OneOrMoreNode(e);
                scope.Accept();
                return true;
            }
        }

        bool ParseGroupDecl(out GroupDeclNode groupDeclNode)
        {
            //GroupDecl(ID name, GroupDecl[] groupDecls, NodeDecl[] nodeDecls, LookaheadExp laExp)
            //{
            //    name LBRACE (groupDecls | nodeDecls)* RBRACE laExp?
            //}

            groupDeclNode = null;           

            using (var scope = lexer.CreateScope("GroupDecl"))
            {
                if (scope == null) return false;

                string name;
                var childGroupDecls = new List<GroupDeclNode>();
                var childNodeDecls = new List<NodeDeclNode>();

                if (!lexer.Consume(Token.ID, out name)) return false;
                if (!lexer.Consume(Token.LBrace)) return false;

                while (!lexer.Consume(Token.RBrace))
                {
                    GroupDeclNode childGroupDecl;
                    NodeDeclNode childNodeDecl;
                    if (ParseGroupDecl(out childGroupDecl))
                    {
                        childGroupDecls.Add(childGroupDecl);
                    }
                    else if (ParseNodeDecl(out childNodeDecl))
                    {
                        childNodeDecls.Add(childNodeDecl);
                    }
                    else return false;
                }

                // lookaheadexp 문제..
                LookaheadExpNode lookaheadExpNode;
                using (var lascope = lexer.CreateScope("GroupDeclLookahead"))
                {
                    if (ParseLookaheadExp(out lookaheadExpNode) && 
                        (lexer.Peek(Token.ID) || lexer.Peek(Token.RBrace) || lexer.EOF))
                        lascope.Accept();
                }

                groupDeclNode = new GroupDeclNode(name, childGroupDecls, childNodeDecls, lookaheadExpNode);
                scope.Accept();
                return true;
            }
        }

        bool ParseNodeDecl(out NodeDeclNode nodeDeclNode)
        {
            //NodeDecl(ID name, Arg[] args, Exp[] exps, LookaheadExp laExp)
            //{
            //    name LPAREN (args (COMMA args)*)? RPAREN LBRACE exps+ RBRACE laExp?
            //}

            nodeDeclNode = null;

            using (var scope = lexer.CreateScope("NodeDecl"))
            {
                if (scope == null) return false;

                string id;
                var argNodes = new List<ArgNode>();

                if (!lexer.Consume(Token.ID, out id)) return false;

                if (!lexer.Consume(Token.LParen)) return false;

                if (!lexer.Consume(Token.RParen))
                {
                    ArgNode argNode;
                    if (!ParseArg(out argNode)) return false;
                    argNodes.Add(argNode);

                    while (!lexer.Consume(Token.RParen))
                    {
                        if( !lexer.Consume(Token.Comma)) return false;

                        if (!ParseArg(out argNode)) return false;

                        argNodes.Add(argNode);
                    }
                }

                if (!lexer.Consume(Token.LBrace)) return false;

                ExpNode expNode;
                if (!ParseExp(out expNode))
                    return false;

                if (!lexer.Consume(Token.RBrace)) return false;

                // lookaheadexp 문제..
                LookaheadExpNode lookaheadExpNode;
                using (var lascope = lexer.CreateScope("NodeDeclLookahead"))
                {
                    if (ParseLookaheadExp(out lookaheadExpNode) &&
                        (lexer.Peek(Token.ID) || lexer.Peek(Token.RBrace) || lexer.EOF))
                        lascope.Accept();
                }

                nodeDeclNode = new NodeDeclNode(id, argNodes, expNode, lookaheadExpNode);
                scope.Accept();
                return true;
            }
        }

        bool ParseArg(out ArgNode argNode)
        {
            SimpleArgNode simpleArgNode;
            if (ParseSimpleArg(out simpleArgNode))
            {
                argNode = simpleArgNode;
                return true;
            }

            ArrayArgNode arrayArgNode;
            if (ParseArrayArg(out arrayArgNode))
            {
                argNode = arrayArgNode;
                return true;
            }

            argNode = null;
            return false;                
        }

        bool ParseSimpleArg(out SimpleArgNode simpleArgNode)
        {
            simpleArgNode = null;

            using (var scope = lexer.CreateScope("SimpleArg"))
            {
                if (scope == null) return false;

                string type, name;

                if (!lexer.Consume(Token.ID, out type))
                    return false;

                if (!lexer.Consume(Token.ID, out name))
                    return false;

                simpleArgNode = new SimpleArgNode(type, name);
                scope.Accept();
                return true;
            }
        }

        bool ParseArrayArg(out ArrayArgNode arrayArgNode)
        {
            arrayArgNode = null;

            using (var scope = lexer.CreateScope("ArrayArg"))
            {
                if (scope == null) return false;

                string type, name;

                if (!lexer.Consume(Token.ID, out type))
                    return false;

                if (!lexer.Consume(Token.LBracket) || !lexer.Consume(Token.RBracket)) return false;

                if (!lexer.Consume(Token.ID, out name))
                    return false;

                arrayArgNode = new ArrayArgNode(type, name);
                scope.Accept();
                return true;
            }
            
        }

        

        public bool ParseModule(out ModuleNode moduleNode)
        {
            //Module( TokenDecl[] tokenDecls, GroupDecl[] groupDecls)
            //{
            //    (tokenDecls | groupDecls)*
            //}

            moduleNode = null;
            var tokenDeclNodes = new List<TokenDeclNode>();
            var groupDeclNodes = new List<GroupDeclNode>();

            using (var scope = lexer.CreateScope("Module"))
            {
                if (scope == null) return false;

                while (!lexer.EOF)
                {
                    TokenDeclNode tokenDeclNode;
                    GroupDeclNode groupDeclNode;
                    if (ParseTokenDecl(out tokenDeclNode))
                    {
                        tokenDeclNodes.Add(tokenDeclNode);
                    }
                    else if (ParseGroupDecl(out groupDeclNode))
                    {
                        groupDeclNodes.Add(groupDeclNode);
                    }
                    else return false;
                }

                moduleNode = new ModuleNode(tokenDeclNodes, groupDeclNodes);
                scope.Accept();
                return true;
            }
            
        }
    }
}


