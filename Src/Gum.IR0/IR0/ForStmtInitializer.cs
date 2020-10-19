using Gum.CompileTime;

namespace Gum.IR0
{
    public abstract class ForStmtInitializer 
    { 

    }

    public class ExpForStmtInitializer : ForStmtInitializer
    {
        public ExpInfo ExpInfo { get; }
        public ExpForStmtInitializer(ExpInfo expInfo) { ExpInfo = expInfo; }
    }

    public class VarDeclForStmtInitializer : ForStmtInitializer
    {
        public LocalVarDecl VarDecl { get; }
        public VarDeclForStmtInitializer(LocalVarDecl varDecl) { VarDecl = varDecl; }
    }
}