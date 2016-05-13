using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Translator.Text2AST.Parsing
{
    class VarDeclStmtParser : Parser<VarDeclStmt>
    {
        // Console.WriteLine(
        protected override VarDeclStmt ParseInner(Lexer lexer)
        {
            TypeID typeID = Parse<TypeID, TypeIDParser>(lexer);
            if (typeID == null) return null;

            var nameAndExps = new List<NameAndExp>();

            do
            {
                string name;
                if (!lexer.Consume(TokenType.Identifier, out name))
                    return null;

                IExpComponent exp = null;
                if ( lexer.Consume(TokenType.Equal))
                {
                    exp = Parse<IExpComponent, ExpComponentParser>(lexer);
                    if( exp == null )
                        throw new ParsingFailedException<IExpComponent, ExpComponentParser>();
                }
                                
                nameAndExps.Add(new NameAndExp(name, exp));

            } while (lexer.Consume(TokenType.Comma));

            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<VarDeclStmt>(TokenType.SemiColon);


            return new VarDeclStmt(typeID, nameAndExps);
        }
    }
}