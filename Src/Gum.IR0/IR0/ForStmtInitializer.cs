using Gum.CompileTime;

namespace Gum.IR0
{
    public abstract class ForStmtInitializer 
    { 

    }

    public class ExpForStmtInitializer : ForStmtInitializer
    {
        public Exp Exp { get; }
        public TypeValue ExpType { get; }
        public ExpForStmtInitializer(Exp exp, TypeValue type) { Exp = exp; ExpType = expType; }
    }

    public class VarDeclForStmtInitializer : ForStmtInitializer
    {
        public LocalVarDecl VarDecl { get; }
        public VarDeclForStmtInitializer(LocalVarDecl varDecl) { VarDecl = varDecl; }
    }
}