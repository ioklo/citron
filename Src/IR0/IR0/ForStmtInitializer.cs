using Pretune;

namespace Citron.IR0
{
    public abstract record class ForStmtInitializer;
    
    public record class ExpForStmtInitializer(Exp Exp) : ForStmtInitializer;    
    public record class VarDeclForStmtInitializer(LocalVarDecl VarDecl) : ForStmtInitializer;
}