using System;
using Gum.Data.AbstractSyntax;
using System.Linq;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        private IDWithTypeArgs ParseIDWithTypeArgs()
        {
            string id;
            ConsumeOrThrow(TokenType.Identifier, out id);

            // typeArguments are optional
            if (!Consume(TokenType.Less))
                return new IDWithTypeArgs(id, new TypeID[] { });

            // comma-separated
            var typeIDs = new List<TypeID>();

            var typeID = ParseTypeID();
            typeIDs.Add(typeID);

            while (Consume(TokenType.Comma))
            {
                typeID = ParseTypeID();
                typeIDs.Add(typeID);
            }

            ConsumeOrThrow(TokenType.Greater);
            return new IDWithTypeArgs(id, typeIDs);
        }
    }
}