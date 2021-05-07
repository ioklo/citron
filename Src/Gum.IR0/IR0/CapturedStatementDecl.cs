using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class CapturedStatementDecl : Decl
    {
        public Name.Anonymous Name { get; }
        public CapturedStatement CapturedStatement { get; }
    }
}