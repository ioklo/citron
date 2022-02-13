using Citron.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citron.Syntax
{
    public abstract record StringExpElement : ISyntaxNode;    
    public record TextStringExpElement(string Text) : StringExpElement;    
    public record ExpStringExpElement(Exp Exp) : StringExpElement;
}
