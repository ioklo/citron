using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        private MemberFuncDecl ParseMemberFuncDecl()
        {
            var memberFuncModifiers = new List<MemberFuncModifier>();

            // function modifier는 순서를 보존해야 하지 않을까?

            MemberFuncModifier memberFuncModifier;
            while (RollbackIfFailed(out memberFuncModifier, ParseMemberFuncModifier))
            {
                // check duplicated modifier
                if (memberFuncModifiers.IndexOf(memberFuncModifier) != -1)
                    throw new ParsingMemberFuncDeclDuplicatedModifierException();

                memberFuncModifiers.Add(memberFuncModifier);
            }

            // public void Func<T>(int a) { }
            TypeID returnType = ParseTypeID();

            string funcName;
            ConsumeOrThrow(TokenType.Identifier, out funcName);

            var typeParams = new List<string>();
            if (Consume(TokenType.Less))
            {
                string typeParam;
                ConsumeOrThrow(TokenType.Identifier, out typeParam);
                typeParams.Add(typeParam);

                while (Consume(TokenType.Comma))
                {
                    ConsumeOrThrow(TokenType.Identifier, out typeParam);
                    typeParams.Add(typeParam);
                }

                ConsumeOrThrow(TokenType.Greater, out typeParam);
            }

            var funcParams = new List<FuncParam>();
            ConsumeOrThrow(TokenType.LParen);

            while (!Consume(TokenType.RParen))
            {
                if (funcParams.Count != 0)
                    ConsumeOrThrow(TokenType.Comma);

                List<FuncParamModifier> funcParamModifiers = new List<FuncParamModifier>();
                FuncParamModifier funcParamModifier;
                while (RollbackIfFailed(out funcParamModifier, ParseFuncParamModifier))
                {
                    funcParamModifiers.Add(funcParamModifier);
                }

                TypeID funcParamType = ParseTypeID();

                string funcParamName;
                ConsumeOrThrow(TokenType.Identifier, out funcParamName);
                funcParams.Add(new FuncParam(funcParamModifiers, funcParamType, funcParamName));
            }

            ConsumeOrThrow(TokenType.LBrace);

            var funcStatements = new List<IStmtComponent>();
            while (!Consume(TokenType.RBrace))
            {
                IStmtComponent stmtComponent = ParseStmt();
                funcStatements.Add(stmtComponent);
            }

            return new MemberFuncDecl(memberFuncModifiers, typeParams, returnType, funcName, funcParams, funcStatements);
        }
    }
}