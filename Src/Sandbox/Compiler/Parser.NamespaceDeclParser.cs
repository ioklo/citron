using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        class NamespaceDeclParser : Parser<NamespaceDecl>
        {
            // namespace namespaceID { }
            protected override NamespaceDecl ParseInner(Lexer lexer)
            {
                // first start with 'namespace' keyword                
                if (!lexer.Consume(TokenType.Namespace)) return null;

                NamespaceID namespaceID = Parse<NamespaceID, NamespaceIDParser>(lexer);
                if (namespaceID == null)
                    throw new ParsingFailedException<NamespaceDeclParser, NamespaceID>();

                if (!lexer.Consume(TokenType.LBrace))
                    throw new ParsingTokenFailedException<NamespaceDeclParser>(TokenType.LBrace);

                var namespaceComponents = new List<INamespaceComponent>();
                while (!lexer.Consume(TokenType.RBrace))
                {
                    NamespaceDecl namespaceDecl = Parse<NamespaceDecl, NamespaceDeclParser>(lexer);
                    if (namespaceDecl != null)
                    {
                        namespaceComponents.Add(namespaceDecl);
                        continue;
                    }

                    ClassDecl classDecl = Parse<ClassDecl, ClassDeclParser>(lexer);
                    if (classDecl != null)
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

                    throw new ParsingFailedException<NamespaceDeclParser, INamespaceComponent>();
                }

                return new NamespaceDecl(namespaceID, namespaceComponents);
            }
        }
    }
}