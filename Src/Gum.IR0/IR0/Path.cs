using Gum.Collections;
using static Gum.Infra.Misc;
using Pretune;

namespace Gum.IR0
{
    // Path
    public abstract partial record Path
    {
        public abstract record Reserved : Path;
        public abstract record Normal : Path;

        // Reserved 
        public record Tuple : Reserved;
        public record TypeVar(int Depth, int Index) : Reserved;
        public record Void : Reserved
        {
            public static readonly Path Instance = new Void();
            Void() { }
        }

        // Box<int>
        public record Box(Path Type) : Reserved;
        public record Ref(Path Type) : Reserved;
        // TRef<T> => ref<int> (if T is int), string (if T is string)
        public record GenericRef(Path Type) : Reserved;

        // Func<params array<int>> interface like type
        public record Func : Reserved;
        public record Nullable(Path Type) : Reserved;

        public record Root(ModuleName ModuleName, NamespacePath NamespacePath, Name Name, ParamHash ParamHash, ImmutableArray<Path> TypeArgs) : Normal;
        public record Nested(Normal outer, Name Name, ParamHash ParamHash, ImmutableArray<Path> TypeArgs) : Normal;
    }

    public abstract partial record Path
    {
        public static readonly Path Bool = Make("System.Runtime", new NamespacePath("System"), "Boolean", default, ParamHash.None);
        public static readonly Path Int = Make("System.Runtime", new NamespacePath("System"), "Int32", default, ParamHash.None);
        public static readonly Path String = Make("System.Runtime", new NamespacePath("System"), "String", default, ParamHash.None);

        public static Path Make(ModuleName moduleName, NamespacePath namespacePath, Name name, ImmutableArray<Path> typeArgs, ParamHash paramHash)
        {
            return new Root(moduleName, namespacePath, name, typeArgs, paramHash);
        }
       
        // seq<> interface는 있다
        public static Path Seq(Path itemType)
            => Make("System.Runtime", new NamespacePath("System"), "ISeq", Arr(itemType), ParamHash.None);

        // list<> class System.List<>
        public static Path List(Path itemPath)
            => new Root("System.Runtime", new NamespacePath("System"), "List", Arr(itemPath), ParamHash.None);
    }    

    public static class PathExtensions
    {
        public static bool IsList(this Path path)
        {
            if (path is Path.Root rootPath)
                return rootPath.ModuleName.Equals(new ModuleName("System.Runtime")) &&
                    rootPath.NamespacePath.Equals(new NamespacePath("System")) &&
                    rootPath.Name.Equals(new Name.Normal("List"));

            return false;
        }

        public static bool IsLambda(this Path path)
        {
            if (path is Path.Root rootPath && rootPath.Name is Name.AnonymousLambda)
                return true;

            if (path is Path.Nested nestedPath && nestedPath.Name is Name.AnonymousLambda)
                return true;

            return false;
        }
    }
}