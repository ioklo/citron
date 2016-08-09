using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Translator.Text2AST.Parsing
{
    class MemberFuncDeclParser : Parser<MemberFuncDecl>
    {
        protected override MemberFuncDecl ParseInner(Lexer lexer)
        {
            var memberFuncModifiers = new List<MemberFuncModifier>();

            // function modifier는 순서를 보존해야 하지 않을까?

            MemberFuncModifier? memberFuncModifier = Parse<MemberFuncModifier?, MemberFuncModifierParser>(lexer);
            while (memberFuncModifier.HasValue)
            {
                // check duplicated modifier
                if (memberFuncModifiers.IndexOf(memberFuncModifier.Value) != -1)
                    throw new ParsingMemberFuncDeclDuplicatedModifierException();

                memberFuncModifiers.Add(memberFuncModifier.Value);
                memberFuncModifier = Parse<MemberFuncModifier?, MemberFuncModifierParser>(lexer);
            }

            // public void Func<T>(int a) { }
            TypeID returnType = Parse<TypeID, TypeIDParser>(lexer);
            if (returnType == null)
                throw new ParsingFailedException<MemberFuncDeclParser, TypeID>();

            string funcName;
            if(!lexer.Consume(TokenType.Identifier, out funcName))
                throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.Identifier);
            
            var typeParams = new List<string>();
            if (lexer.Consume(TokenType.Less))
            {
                string typeParam;
                if (!lexer.Consume(TokenType.Identifier, out typeParam))
                    throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.Identifier);

                typeParams.Add(typeParam);

                while (lexer.Consume(TokenType.Comma))
                {
                    if (!lexer.Consume(TokenType.Identifier, out typeParam))
                        throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.Identifier);

                    typeParams.Add(typeParam);
                }

                if (!lexer.Consume(TokenType.Greater, out typeParam))
                    throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.Greater);
            }

            var funcParams = new List<FuncParam>();
            if (!lexer.Consume(TokenType.LParen))
                throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.LParen);

            while( !lexer.Consume(TokenType.RParen) )
            {
                if (funcParams.Count != 0)
                {
                    if (!lexer.Consume(TokenType.Comma))
                        throw new ParsingTokenFailedException<MemberFuncDecl>(TokenType.Comma);
                }

                List<FuncParamModifier> funcParamModifiers = new List<FuncParamModifier>();
                FuncParamModifier? funcParamModifier = Parse<FuncParamModifier?, FuncParamModifierParser>(lexer);
                while (funcParamModifier.HasValue)
                {
                    funcParamModifiers.Add(funcParamModifier.Value);
                    funcParamModifier = Parse<FuncParamModifier?, FuncParamModifierParser>(lexer);
                }

                TypeID funcParamType = Parse<TypeID, TypeIDParser>(lexer);
                if (funcParamType == null)
                    throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.Identifier);

                string funcParamName;
                if (!lexer.Consume(TokenType.Identifier, out funcParamName))
                    throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.Identifier);

                funcParams.Add(new FuncParam(funcParamModifiers, funcParamType, funcParamName));
            } 

            if (!lexer.Consume(TokenType.LBrace))
                throw new ParsingTokenFailedException<MemberFuncDeclParser>(TokenType.LBrace);

            var funcStatements = new List<IStmtComponent>();
            while ( !lexer.Consume(TokenType.RBrace) )
            {
                IStmtComponent stmtComponent = Parse<IStmtComponent, StmtComponentParser>(lexer);
                funcStatements.Add(stmtComponent);
            }           

            return new MemberFuncDecl(memberFuncModifiers, typeParams, returnType, funcName, funcParams, funcStatements);
        }

    }
}