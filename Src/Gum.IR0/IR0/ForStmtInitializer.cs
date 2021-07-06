using Gum.CompileTime;
using Pretune;

namespace Gum.IR0
{
    public abstract record ForStmtInitializer;
    
    public record ExpForStmtInitializer(Exp Exp) : ForStmtInitializer;    
    public record VarDeclForStmtInitializer(LocalVarDecl VarDecl) : ForStmtInitializer;
}