
namespace Gum.Syntax
{
    public abstract class TypeDecl : ISyntaxNode
    {
        public string Name { get; }
        public abstract int TypeParamCount { get; }
        public TypeDecl(string name)
        {
            Name = name;
        }
    }
}