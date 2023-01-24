using Citron.Collections;
using Pretune;

namespace Citron.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct VarDeclElemInitializer
    {
        public bool IsRef { get; }
        public Exp Exp { get; }
    }

    // var a = ref i; 도 있어서 refVarDecl, VarDecl나누지 말고 하나에서 다 처리한다
    public record class VarDeclElement(string VarName, VarDeclElemInitializer? Initializer) : ISyntaxNode;
    public record class VarDecl(bool IsRef, TypeExp Type, ImmutableArray<VarDeclElement> Elems) : ISyntaxNode;
} 