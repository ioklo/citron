using Gum.Collections;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface ITypeSymbolNode : ISymbolNode
    {
        new ITypeSymbolNode Apply(TypeEnv typeEnv);
        new ITypeDeclSymbolNode GetDeclSymbolNode();
    }

    public static class ITypeSymbolNodeExtensions
    {        
    }
}