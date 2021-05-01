using Gum.Collections;
using static Gum.Infra.Misc;
using Pretune;

namespace Gum.IR0
{
    // Path
    public abstract partial record Path
    {
        public abstract record Reserved : Path;
        public abstract record Normal(Name Name, ParamHash ParamHash, ImmutableArray<Path> TypeArgs) : Path;

        // Reserved 
        public record TupleType : Reserved;
        public record TypeVarType(int Depth, int Index) : Reserved;
        public record VoidType : Reserved
        {
            public static readonly Path Instance = new VoidType();
            VoidType() { }
        }

        // Box<int>
        public record BoxType(Path Type) : Reserved;
        public record RefType(Path Type) : Reserved;
        // TRef<T> => ref<int> (if T is int), string (if T is string)
        public record GenericRefType(Path Type) : Reserved;

        // Func<params array<int>> interface like type
        public record FuncType : Reserved;
        public record NullableType(Path Type) : Reserved;
        public record AnonymousSeqType(Path SeqFunc) : Reserved;
        public record AnonymousLambdaType(Path.Normal Lambda) : Reserved;

        public record Root(ModuleName ModuleName, NamespacePath NamespacePath, Name Name, ParamHash ParamHash, ImmutableArray<Path> TypeArgs) 
            : Normal(Name, ParamHash, TypeArgs);

        public record Nested(Normal Outer, Name Name, ParamHash ParamHash, ImmutableArray<Path> TypeArgs)
            : Normal(Name, ParamHash, TypeArgs);
    }

    public abstract partial record Path
    {
        public static readonly Path Bool = Make("System.Runtime", new NamespacePath("System"), "Boolean", ParamHash.None, default);
        public static readonly Path Int = Make("System.Runtime", new NamespacePath("System"), "Int32", ParamHash.None, default);
        public static readonly Path String = Make("System.Runtime", new NamespacePath("System"), "String", ParamHash.None, default);

        public static Path Make(ModuleName moduleName, NamespacePath namespacePath, Name name, ParamHash paramHash, ImmutableArray<Path> typeArgs)
        {
            return new Root(moduleName, namespacePath, name, paramHash, typeArgs);
        }
       
        // seq<> interface는 있다
        public static Path Seq(Path itemType)
            => Make("System.Runtime", new NamespacePath("System"), "ISeq", new ParamHash(1, default), Arr(itemType));

        // list<> class System.List<>
        public static Path List(Path itemPath)
            => new Root("System.Runtime", new NamespacePath("System"), "List", new ParamHash(1, default), Arr(itemPath));
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
    }
}