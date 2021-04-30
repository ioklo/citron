using Gum.Collections;
using static Gum.Infra.Misc;
using Pretune;

namespace Gum.IR0
{
    // Path
    public abstract record Path;
    
    // Reserved 
    public record TuplePath : Path;
    public record TypeVar(int Depth, int Index) : Path;
    public record Void : Path
    {
        public static readonly Name Instance = new Void();
        Void() { }
    }



    public record RootPathOuter(ModuleName ModuleName, NamespacePath NamespacePath) : PathOuter;
    public record NestedPathOuter(Path outer) : PathOuter;

    // 모든 타입, 함수, 변수 등을 지칭 가능한 만능 식별자
    [AutoConstructor]
    public partial struct Path
    {
        public PathOuter Outer { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }
        public ParamHash ParamHash { get; }
    }

    // 구분을 위해 static을 따로 보관
    public partial struct Path
    {
        public static Path Make(ModuleName moduleName, NamespacePath namespacePath, Name name, ImmutableArray<Type> typeArgs, ParamHash paramHash)
        {
            return new Path(new RootPathOuter(moduleName, namespacePath), name, typeArgs, paramHash);
        }

        public static readonly Path Void = new Path(ReservedPathOuter.Instance, Name.Void.Instance, default, ParamHash.None);


        public static readonly Path Bool = Make("System.Runtime", new NamespacePath("System"), "Boolean", default, ParamHash.None);
        public static readonly Path Int = Make("System.Runtime", new NamespacePath("System"), "Int32", default, ParamHash.None);
        public static readonly Path String = Make("System.Runtime", new NamespacePath("System"), "String", default, ParamHash.None);

        // seq<> interface는 있다
        public static Path Seq(Type itemType)
            => Make("System.Runtime", new NamespacePath("System"), "ISeq", Arr(itemType), ParamHash.None);

        // list<> class System.List<>
        public static readonly PathOuter ListOuter = new RootPathOuter("System.Runtime", new NamespacePath("System"));
        public static Path List(Type itemType)
            => new Path(ListOuter, "List", Arr(itemType), ParamHash.None);
    }
}