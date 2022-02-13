namespace Citron.CompileTime
{
    public record NamespacePath(NamespacePath? Outer, Name Name);
}