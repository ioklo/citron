using Gum.Collections;
using Pretune;

namespace Gum.Syntax
{
    public record VarDeclElement(string VarName, Exp? InitExp) : ISyntaxNode;

    public record VarDecl(TypeExp Type, ImmutableArray<VarDeclElement> Elems) : ISyntaxNode;
}