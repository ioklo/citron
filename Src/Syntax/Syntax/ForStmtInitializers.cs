namespace Citron.Syntax
{
    public abstract record class ForStmtInitializer : ISyntaxNode{ }
    public record class ExpForStmtInitializer(Exp Exp) : ForStmtInitializer;
    public record class VarDeclForStmtInitializer(VarDecl VarDecl) : ForStmtInitializer;
}