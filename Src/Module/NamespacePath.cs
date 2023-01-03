namespace Citron.Module
{
    public record class NamespacePath(NamespacePath? Outer, Name Name);
}