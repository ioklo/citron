namespace Citron.Syntax
{
    public abstract record ForStmtInitializer : ISyntaxNode{ }
    public record ExpForStmtInitializer(Exp Exp) : ForStmtInitializer;
    public record VarDeclForStmtInitializer(VarDecl VarDecl) : ForStmtInitializer;
}