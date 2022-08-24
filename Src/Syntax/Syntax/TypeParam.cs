using Pretune;

namespace Citron.Syntax
{
    // ISyntaxNode로 지정하기 위해서 클래스로
    [AutoConstructor, ImplementIEquatable]
    public partial class TypeParam : ISyntaxNode
    {
        public string Name { get; }
    }
}
