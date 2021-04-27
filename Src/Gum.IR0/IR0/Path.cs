using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    public abstract record PathOuter;
    public record RootPathOuter(ModuleName ModuleName, NamespacePath NamespacePath) : FuncOuter;
    public record TypePathOuter : PathOuter;
    public record FuncPathOuter : PathOuter;

    [AutoConstructor]
    public struct Path
    {
        public PathOuter Outer { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }
        public string ParamHash { get; }
    }
}