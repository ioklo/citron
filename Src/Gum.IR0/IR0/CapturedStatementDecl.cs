using Pretune;

namespace Gum.IR0
{
    [AutoConstructor]
    public partial class CapturedStatementDecl : Decl
    {
        public CapturedStatement CapturedStatement { get; }
    }
}