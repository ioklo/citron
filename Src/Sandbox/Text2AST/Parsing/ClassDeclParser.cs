using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Translator.Text2AST.Parsing
{
    class ClassDeclParser : Parser<ClassDecl>
    {
        // class ClassName : BaseType { }
        protected override ClassDecl ParseInner(Lexer lexer)
        {
            // typeVars, baseTypes, IMemberComponents                

            // first start with 'class' keyword
            if (!lexer.Consume(TokenType.Class)) return null;

            // then parse class name
            string className;
            if (!lexer.Consume(TokenType.Identifier, out className))
                throw new ParsingTokenFailedException<ClassDecl>(TokenType.Identifier);

            // Parse typeVars, don't allow type-instantiation style, only accept identifier
            var typeVars = new List<string>();
            if (lexer.Consume(TokenType.Less))
            {
                string typeVar;
                if (!lexer.Consume(TokenType.Identifier, out typeVar))
                    throw new ParsingTokenFailedException<ClassDeclParser>(TokenType.Identifier);

                typeVars.Add(typeVar);

                while (lexer.Consume(TokenType.Comma))
                {
                    if (!lexer.Consume(TokenType.Identifier, out typeVar))
                        throw new ParsingTokenFailedException<ClassDeclParser>(TokenType.Identifier);

                    typeVars.Add(typeVar);
                }

                if (!lexer.Consume(TokenType.Greater))
                    throw new ParsingTokenFailedException<ClassDeclParser>(TokenType.Greater);
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
            if (lexer.Consume(TokenType.Colon))
            {
                // comma-separated list
                TypeID typeID = Parse<TypeID, TypeIDParser>(lexer);
                if (typeID != null)
                    throw new ParsingFailedException<ClassDeclParser, TypeID>();

                baseTypes.Add(typeID);

                while (lexer.Consume(TokenType.Comma))
                {
                    typeID = Parse<TypeID, TypeIDParser>(lexer);

                    if (typeID != null)
                        throw new ParsingFailedException<ClassDeclParser, TypeID>();

                    baseTypes.Add(typeID);
                }
            }

            if (!lexer.Consume(TokenType.LBrace))
                throw new ParsingTokenFailedException<ClassDeclParser>(TokenType.LBrace);

            // IMemberComponent
            var memberComponents = new List<IMemberComponent>();
            while (!lexer.Consume(TokenType.RBrace))
            {
                MemberFuncDecl memberFuncDecl = Parse<MemberFuncDecl, MemberFuncDeclParser>(lexer);
                if ( memberFuncDecl != null )
                {
                    memberComponents.Add(memberFuncDecl);
                    continue;
                }

                MemberVarDecl memberVarDecl = Parse<MemberVarDecl, MemberVarDeclParser>(lexer);
                if (memberVarDecl != null)
                {
                    memberComponents.Add(memberVarDecl);
                    continue;
                }

                throw new ParsingFailedException<ClassDeclParser, IMemberComponent>();
            }

            return new ClassDecl(typeVars, className, baseTypes, memberComponents);
        }
    }
}