using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        // class ClassName : BaseType { }
        private ClassDecl ParseClassDecl()
        {
            // typeVars, baseTypes, IMemberComponents                

            // first start with 'class' keyword
            ConsumeOrThrow(TokenType.Class);

            // then parse class name
            string className;
            ConsumeOrThrow(TokenType.Identifier, out className);

            // Parse typeVars, don't allow type-instantiation style, only accept identifier
            var typeVars = new List<string>();
            if (Consume(TokenType.Less))
            {
                string typeVar;
                ConsumeOrThrow(TokenType.Identifier, out typeVar);
                typeVars.Add(typeVar);

                while (Consume(TokenType.Comma))
                {
                    ConsumeOrThrow(TokenType.Identifier, out typeVar);
                    typeVars.Add(typeVar);
                }

                ConsumeOrThrow(TokenType.Greater);
            }

            // Parser<T>.Parse : Lexer -> T
            // new TokenParser<string>(TokenType).Parse : Lexer -> string

            // 많이 쓰이는 경우,
            // 1. discriminated union, 어느 한 가지중의 하나인 경우 (앞에 있을 수록 우선순위가 높아짐)
            // 2. sequence, 
            // 3. list: header, value, separator, footer의 반복
            // 4. 있어도 되고, 없어도 되고
            // 5. 파싱을 했을 떄, 그것이 어떤 value에 매칭이 되는지 'ㅁ'/, 파싱 실패시 

            // 이걸.. 다 만들고 해야 할지.. 아니면 다 만들기 전에 미리 만들어야 할지


            // 1 2 3의 복합
            // ClassDecl = 

            // TokenType.Less, TokenType.Identifier, TokenType.Comma, TokenType.Greater
            // TokenType.LParen, FuncParam, TokenType.Comma, TokenType.RParen
            // None, TypeID, TokenType.Comma, None
            // TokenType.LBrace, MemberComponentParser, None, TokenType.RBrace

            // List<C> ParseList<ParserH, C, ParserC, ParserS, ParserF>(ParserH headerParser, ParserC contentParser, ParserS separator, ParserF footerParser)


            // optional baseTypes
            var baseTypes = new List<TypeID>();
            if (Consume(TokenType.Colon))
            {
                // comma-separated list
                TypeID typeID = ParseTypeID();
                baseTypes.Add(typeID);

                while (Consume(TokenType.Comma))
                {
                    typeID = ParseTypeID();
                    baseTypes.Add(typeID);
                }
            }

            ConsumeOrThrow(TokenType.LBrace);

            // IMemberComponent
            var memberComponents = new List<IMemberComponent>();
            while (!Consume(TokenType.RBrace))
            {
                MemberFuncDecl memberFuncDecl;
                if (RollbackIfFailed(out memberFuncDecl, ParseMemberFuncDecl))
                {
                    memberComponents.Add(memberFuncDecl);
                    continue;
                }

                MemberVarDecl memberVarDecl;
                if( RollbackIfFailed(out memberVarDecl, ParseMemberVarDecl))
                {
                    memberComponents.Add(memberVarDecl);
                    continue;
                }

                throw CreateException();
            }

            return new ClassDecl(typeVars, className, baseTypes, memberComponents);
        }
        
    }
}