using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Syntax
{
    public abstract class StringExpElement : ISyntaxNode
    {
    }

    public class TextStringExpElement : StringExpElement
    {
        public string Text { get; }
        public TextStringExpElement(string text) { Text = text; }

        public override bool Equals(object? obj)
        {
            return obj is TextStringExpElement element &&
                   Text == element.Text;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text);
        }

        public static bool operator ==(TextStringExpElement? left, TextStringExpElement? right)
        {
            return EqualityComparer<TextStringExpElement?>.Default.Equals(left, right);
        }

        public static bool operator !=(TextStringExpElement? left, TextStringExpElement? right)
        {
            return !(left == right);
        }
    }

    public class ExpStringExpElement : StringExpElement
    {
        public Exp Exp { get; }
        public ExpStringExpElement(Exp exp) { Exp = exp; }

        public override bool Equals(object? obj)
        {
            return obj is ExpStringExpElement element &&
                   EqualityComparer<Exp>.Default.Equals(Exp, element.Exp);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Exp);
        }

        public static bool operator ==(ExpStringExpElement? left, ExpStringExpElement? right)
        {
            return EqualityComparer<ExpStringExpElement?>.Default.Equals(left, right);
        }

        public static bool operator !=(ExpStringExpElement? left, ExpStringExpElement? right)
        {
            return !(left == right);
        }
    }
}
