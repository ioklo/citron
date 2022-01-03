namespace Gum.CompileTime
{
    public record NamespacePath(NamespacePath? Outer, Name Name);
}