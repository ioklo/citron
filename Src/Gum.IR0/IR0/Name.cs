using Pretune;

namespace Gum.IR0
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

    [ImplementIEquatable]
    public partial struct Name
    {
        public SpecialName Kind { get; }
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

        public override string ToString()
        {
            switch (Kind)
            {
                case SpecialName.Normal: return Text!;
                case SpecialName.AnonymousLambda: return $"$Labmda{Text!}";
                default: return $"${Kind}";
            }
        }

        public static implicit operator Name(string s) => new Name(SpecialName.Normal, s);
    }
}