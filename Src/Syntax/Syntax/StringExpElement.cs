using Citron.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citron.Syntax
{
    public abstract record class StringExpElement : ISyntaxNode;    
    public record class TextStringExpElement(string Text) : StringExpElement;    
    public record class ExpStringExpElement(Exp Exp) : StringExpElement;
}
