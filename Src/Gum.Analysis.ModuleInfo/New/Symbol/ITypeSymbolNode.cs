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
        public static M.NormalTypeId GetMTypeId(this ITypeSymbolNode node)
        {
            var outerNode = node.GetOuter();
            var typeArgs = node.GetTypeArgs();

            // ITypeSymbolNode는 언제나 어디 밑에 있다
            Debug.Assert(outerNode != null);

            var builder = ImmutableArray.CreateBuilder<M.TypeId>();
            foreach (var typeArg in typeArgs)
                builder.Add(typeArg.GetMTypeId());

            var declNode = node.GetDeclSymbolNode();
            var name = declNode.GetNodeName();

            Debug.Assert(typeArgs.Length == name.TypeParamCount);

            return outerNode.MakeChildTypeId(name.Name, builder.MoveToImmutable());
        }
    }
}