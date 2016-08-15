using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        private IDExp ParseIDExp()
        {
            string id;
            ConsumeOrThrow(TokenType.Identifier, out id);

            List<TypeID> typeArgs = new List<TypeID>();

            // func<typeid>
            if (Consume(TokenType.Less))
            {
                // <TypeID, typeID2>
                var typeID = ParseTypeID();
                typeArgs.Add(typeID);

                while (!Consume(TokenType.Greater))
                {
                    typeID = ParseTypeID();
                    typeArgs.Add(typeID);
                }
            }

            return new IDExp(id, typeArgs);
        }
    }
}