using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        class IDExpParser : Parser<IDExp>
        {
            protected override IDExp ParseInner(Lexer lexer)
            {
                string id;
                if (!lexer.Consume(TokenType.Identifier, out id))
                    return null;

                List<TypeID> typeArgs = new List<TypeID>();

                // func<typeid>
                if (lexer.Consume(TokenType.Less))
                {
                    // <TypeID, typeID2>
                    var typeID = Parser<TypeID, TypeIDParser>.Parse(lexer);
                    if (typeID == null)
                        throw new ParsingFailedException<TypeIDParser, TypeID>();

                    typeArgs.Add(typeID);

                    while (!lexer.Consume(TokenType.Greater))
                    {
                        typeID = Parser<TypeID, TypeIDParser>.Parse(lexer);
                        if (typeID == null)
                            throw new ParsingFailedException<TypeIDParser, TypeID>();

                        typeArgs.Add(typeID);
                    }
                }

                return new IDExp(id, typeArgs);
            }
        }
    }
}