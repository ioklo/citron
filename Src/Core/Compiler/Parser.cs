using Gum.Data.AbstractSyntax;
using System;
using System.Collections.Generic;

namespace Gum.Compiler
{ 
    public partial class Parser
    {
        List<Token> tokens;
        int curIndex;

        bool Ended { get { return tokens.Count == curIndex; } }

        private Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            curIndex = 0;
        }

        private bool RollbackIfFailed<T>(out T output, Func<T> func)
        {
            int origIndex = curIndex;

            output = func();
            if (output != null)
                return true;

            curIndex = origIndex;
            return false;
        }

        private bool Consume(TokenType tokenType)
        {
            if (tokens[curIndex].Type != tokenType)
                return false;

            curIndex++;
            return true;
        }

        private void ConsumeOrThrow(TokenType tokenType)
        {
            if (!Consume(tokenType))
                throw new NotImplementedException();
        }

        public bool Consume(TokenType tokenType, out string token)
        {
            if (tokens[curIndex].Type != tokenType)
            {
                token = null;
                return false;
            }

            token = tokens[curIndex].Value;
            curIndex++;
            return true;
        }

        private void ConsumeOrThrow(TokenType tokenType, out string token)
        {
            if (Consume(tokenType, out token))
                throw new NotImplementedException();
        }

        private bool ConsumeAny(out TokenType res, params TokenType[] tokenKinds)
        {
            foreach (var tokenType in tokenKinds)
            {
                if (tokens[curIndex].Type == tokenType)
                {
                    curIndex++;
                    res = tokenType;
                    return true;
                }
            }

            res = TokenType.Invalid;
            return false;
        }

        
        private Exception CreateException()
        {
            return new NotImplementedException();
        }

        private FileUnit ParseFileUnit()
        {
            var fileUnitComponents = new List<IFileUnitComponent>();

            // 끝날때까지 
            while (!Ended)
            {
                IStmtComponent stmtComp;
                if (RollbackIfFailed(out stmtComp, ParseStmt))
                {
                    fileUnitComponents.Add(stmtComp);
                    continue;
                }

                throw CreateException();
            }

            return new FileUnit(fileUnitComponents);
        }


        public static FileUnit ParseFileUnit(List<Token> tokens)
        {
            var parser = new Parser(tokens);
            return parser.ParseFileUnit();
        }
    }
}