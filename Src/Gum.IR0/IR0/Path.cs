using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    public abstract record PathOuter;
    public record RootPathOuter(ModuleName ModuleName, NamespacePath NamespacePath) : PathOuter;
    public record NestedPathOuter(Path outer) : PathOuter;

    [AutoConstructor]
    public partial struct Path
    {
        public static Path Make(ModuleName moduleName, NamespacePath namespacePath, Name name, ImmutableArray<Type> typeArgs, ParamHash paramHash)
        {
            return new Path(new RootPathOuter(moduleName, namespacePath), name, typeArgs, paramHash);
        }

        public PathOuter Outer { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }
        public ParamHash ParamHash { get; }
    }
}