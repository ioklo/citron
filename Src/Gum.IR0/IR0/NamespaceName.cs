using Pretune;

namespace Gum.IR0
{
    [ImplementIEquatable]
    public partial struct NamespaceName
    {
        public string Value { get; }
        public NamespaceName(string value) { Value = value; }

        public static implicit operator NamespaceName(string s) => new NamespaceName(s);
    }
}