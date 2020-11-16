using System;
using System.Diagnostics;

namespace Gum.CompileTime
{
    public enum SpecialName
    {
        Normal,
        IndexerGet,
        IndexerSet,
        AnonymousLambda, // use Name member

        OpInc,
        OpDec,
    }

    public static class SpecialNames
    {
        public static Name IndexerGet { get; } = new Name(SpecialName.IndexerGet, null);
        public static Name IndexerSet { get; } = new Name(SpecialName.IndexerGet, null);
        public static Name OpInc { get; } = new Name(SpecialName.OpInc, null);
        public static Name OpDec { get; } = new Name(SpecialName.OpDec, null);
    }

    public struct Name
    {
        public SpecialName Kind { get;  }
        public string? Text { get; }

        public static Name MakeAnonymousLambda(string text)
        {
            return new Name(SpecialName.AnonymousLambda, text);
        }

        public static Name MakeText(string text)
        {
            return new Name(SpecialName.Normal, text);
        }

        internal Name(SpecialName kind, string? text)
        {
            Kind = kind;
            Text = text;
        }

        public override bool Equals(object? obj)
        {
            return obj is Name id &&
                   Kind == id.Kind &&
                   Text == id.Text;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Text);
        }

        public static bool operator ==(Name left, Name right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Name left, Name right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            switch(Kind)
            {
                case SpecialName.Normal: return Text!;
                case SpecialName.AnonymousLambda: return $"$Labmda{Text!}";
                default: return $"${Kind}";
            }
        }

        public static implicit operator Name(string s) => new Name(SpecialName.Normal, s);
    }
}
