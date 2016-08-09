using System;
using Gum.Data.AbstractSyntax;
using System.Linq;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        class IDWithTypeArgsParser : Parser<IDWithTypeArgs>
        {
            protected override IDWithTypeArgs ParseInner(Lexer lexer)
            {
                string id;
                if (!lexer.Consume(TokenType.Identifier, out id))
                    return null;

                // typeArguments are optional
                if (!lexer.Consume(TokenType.Less))
                    return new IDWithTypeArgs(id, new TypeID[] { });

                // comma-separated
                var typeIDs = new List<TypeID>();

                var typeID = Parse<TypeID, TypeIDParser>(lexer);
                if (typeID == null)
                    throw new ParsingFailedException<IDWithTypeArgsParser, IDWithTypeArgs>();
                typeIDs.Add(typeID);

                while (lexer.Consume(TokenType.Comma))
                {
                    typeID = Parse<TypeID, TypeIDParser>(lexer);
                    if (typeID == null)
                        throw new ParsingFailedException<IDWithTypeArgsParser, IDWithTypeArgs>();
                    typeIDs.Add(typeID);
                }

                if (!lexer.Consume(TokenType.Greater))
                    throw new ParsingTokenFailedException<IDWithTypeArgsParser>(TokenType.Greater);

                return new IDWithTypeArgs(id, typeIDs);
            }
        }
    }
}