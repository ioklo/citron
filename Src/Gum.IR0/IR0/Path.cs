using Gum.Collections;
using static Gum.Infra.Misc;
using Pretune;
using Gum.Infra;

namespace Gum.IR0
{
    // Path
    // Module, Namespace, Type, Func, Lambda, Sequence, Reserved(타입) 을 유일하게 가리킬 수 있는 구조
    public abstract partial record Path : IPure
    {
        public void EnsurePure() { }

        public abstract record Reserved : Path;
        public abstract record Normal : Path;

        // Reserved 
        public record TupleType(ImmutableArray<TypeAndName> Elems) : Reserved;
        public record TypeVarType(int Index) : Reserved;
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
        public record AnonymousLambdaType(Path.Nested Lambda) : Reserved;

        public record Root(ModuleName ModuleName) : Normal;
        public record Nested(Normal Outer, Name Name, ParamHash ParamHash, ImmutableArray<Path> TypeArgs) : Normal;
    }

    public abstract partial record Path
    {
        // Namespace
        public static readonly Normal System = Make("System.Runtime", "System", ParamHash.None, default);

        // Runtime Type
        public static readonly Path Bool = new Nested(System, "Boolean", ParamHash.None, default);
        public static readonly Path Int = new Nested(System, "Int32", ParamHash.None, default);
        public static readonly Path String = new Nested(System, "String", ParamHash.None, default);

        public static Normal Make(ModuleName moduleName, Name name, ParamHash paramHash, ImmutableArray<Path> typeArgs)
        {
            return new Nested(new Root(moduleName), name, paramHash, typeArgs);
        }        

        // seq<> interface는 있다
        public static Path Seq(Path itemType)
            => new Nested(System, "ISeq", new ParamHash(1, default), Arr(itemType));

        // list<> class System.List<>
        public static Path List(Path itemPath)
            => new Nested(System, "List", new ParamHash(1, default), Arr(itemPath));
    }    

    public static class PathExtensions
    {
        public static bool IsTypeInstOfList(this Path path)
        {
            if (path is Path.Nested nestedPath)
            {
                return nestedPath.Outer.Equals(Path.System) &&
                    nestedPath.Name.Equals(new Name.Normal("List")) &&
                    nestedPath.ParamHash.Equals(new ParamHash(1, default));
            }

            return false;
        }        
    }
}