using Citron.Collections;
using Pretune;

namespace Citron.Syntax
{
    // var a = ref i; 도 있어서 refVarDecl, VarDecl나누지 말고 하나에서 다 처리한다
    public record class VarDeclElement(string VarName, Exp? InitExp) : ISyntaxNode;
    public record class VarDecl(TypeExp Type, ImmutableArray<VarDeclElement> Elems) : ISyntaxNode;
} 