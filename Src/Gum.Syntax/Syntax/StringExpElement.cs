using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Syntax
{
    public abstract record StringExpElement : ISyntaxNode;    
    public record TextStringExpElement(string Text) : StringExpElement;    
    public record ExpStringExpElement(Exp Exp) : StringExpElement;
}
