
namespace Gum.Syntax
{
    public abstract class TypeDecl : ISyntaxNode
    {
        public abstract string Name { get; }
        public abstract int TypeParamCount { get; }
    }
}