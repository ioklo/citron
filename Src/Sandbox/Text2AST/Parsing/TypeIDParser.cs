using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Translator.Text2AST.Parsing
{
    class TypeIDParser : Parser<TypeID>
    {
        protected override TypeID ParseInner(Lexer lexer)
        {
            var idWithTypeArgsList = new List<IDWithTypeArgs>();
            IDWithTypeArgs idWithTypeArgs = Parse<IDWithTypeArgs, IDWithTypeArgsParser>(lexer);

            // at least one namespace identifier
            if (idWithTypeArgs == null)
                return null;

            idWithTypeArgsList.Add(idWithTypeArgs);

            while (lexer.Consume(TokenType.Dot))
            {
                idWithTypeArgs = Parse<IDWithTypeArgs, IDWithTypeArgsParser>(lexer);
                if (idWithTypeArgs == null)
                    throw new ParsingFailedException<TypeIDParser, IDWithTypeArgs>();

                idWithTypeArgsList.Add(idWithTypeArgs);
            }

            return new TypeID(idWithTypeArgsList);
        }
    }
}