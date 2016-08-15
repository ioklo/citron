using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        private TypeID ParseTypeID()
        {
            var idWithTypeArgsList = new List<IDWithTypeArgs>();
            IDWithTypeArgs idWithTypeArgs = ParseIDWithTypeArgs();
            idWithTypeArgsList.Add(idWithTypeArgs);

            while (Consume(TokenType.Dot))
            {
                idWithTypeArgs = ParseIDWithTypeArgs();
                idWithTypeArgsList.Add(idWithTypeArgs);
            }

            return new TypeID(idWithTypeArgsList);
        }
    }
}