using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        // namespace namespaceID { }
        private NamespaceDecl ParseNamespaceDecl()
        {
            // first start with 'namespace' keyword                
            ConsumeOrThrow(TokenType.Namespace);

            NamespaceID namespaceID = ParseNamespaceID();
            ConsumeOrThrow(TokenType.LBrace);

            var namespaceComponents = new List<INamespaceComponent>();
            while (!Consume(TokenType.RBrace))
            {
                NamespaceDecl namespaceDecl;
                if( RollbackIfFailed(out namespaceDecl, ParseNamespaceDecl))
                {
                    namespaceComponents.Add(namespaceDecl);
                    continue;
                }

                ClassDecl classDecl;
                if (RollbackIfFailed(out classDecl, ParseClassDecl))
                {
                    namespaceComponents.Add(classDecl);
                    continue;
                }

                //StructDecl structDecl = Parse<StructDecl, StructDeclParser>(lexer);
                //if (structDecl != null)
                //{
                //    namespaceComponents.Add(structDecl);
                //    continue;
                //}

                throw CreateException();
            }

            return new NamespaceDecl(namespaceID, namespaceComponents);
        }
        
    }
}