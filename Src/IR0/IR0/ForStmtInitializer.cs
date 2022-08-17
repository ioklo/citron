using Citron.Module;
using Pretune;

namespace Citron.IR0
{
    public abstract record ForStmtInitializer;
    
    public record ExpForStmtInitializer(Exp Exp) : ForStmtInitializer;    
    public record VarDeclForStmtInitializer(LocalVarDecl VarDecl) : ForStmtInitializer;
}