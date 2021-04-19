using Gum.CompileTime;
using Pretune;

namespace Gum.IR0
{
    public abstract class ForStmtInitializer 
    { 
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ExpForStmtInitializer : ForStmtInitializer
    {
        public Exp Exp { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class VarDeclForStmtInitializer : ForStmtInitializer
    {
        public LocalVarDecl VarDecl { get; }
    }
}