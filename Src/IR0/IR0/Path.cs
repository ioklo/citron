//using Citron.Collections;
//using static Citron.Infra.Misc;
//using Pretune;
//using Citron.Infra;
//using System.Diagnostics;
//using System.Linq;

//namespace Citron.IR0
//{
//    [AutoConstructor]
//    public partial struct TupleTypeElem
//    {
//        public Path Type { get; }
//        public Name Name { get; }
//    }

//    // Path
//    // Module, Namespace, Type, Func, Lambda, Sequence, Reserved(타입) 을 유일하게 가리킬 수 있는 구조
//    public abstract partial record Path
//    {
//        public abstract record Reserved : Path;
//        public abstract record Normal : Path;

//        // Reserved 
//        public record TupleType(ImmutableArray<TupleTypeElem> Elems) : Reserved;        
//        public record TypeVarType(int Index) : Reserved;
//        public record VoidType : Reserved
//        {
//            public static readonly Path Instance = new VoidType();
//            VoidType() { }
//        }

//        // Box<int>
//        public record BoxType(Path Type) : Reserved;
//        // TRef<T> => ref<int> (if T is int), string (if T is string)
//        public record GenericRefType(Path Type) : Reserved;

//        // Func<params array<int>> interface like type
//        public record FuncType : Reserved;
//        public record NullableType(Path Type) : Reserved;

//        [DebuggerDisplay("[{ModuleName}]")]
//        public record Root(ModuleName ModuleName) : Normal;

//        [DebuggerDisplay("{Outer}.{Name}`{ParamHash}<{TypeArgs}>")]
//        public record Nested(Normal Outer, Name Name, ParamHash ParamHash, ImmutableArray<Path> TypeArgs) : Normal;
//    }

//    public abstract partial record Path
//    {
//        // Namespace
//        public static readonly Normal System = Make("System.Runtime", new Name.Normal("System"), ParamHash.None, default);

//        // Runtime Type
//        public static readonly Path Bool = new Nested(System, new Name.Normal("Boolean"), ParamHash.None, default);
//        public static readonly Path Int = new Nested(System, new Name.Normal("Int32"), ParamHash.None, default);
//        public static readonly Path String = new Nested(System, new Name.Normal("String"), ParamHash.None, default);

//        public static Normal Make(ModuleName moduleName, Name name, ParamHash paramHash, ImmutableArray<Path> typeArgs)
//        {
//            return new Nested(new Root(moduleName), name, paramHash, typeArgs);
//        }        

//        // seq<> interface는 있다
//        public static Path Seq(Path itemType)
//            => new Nested(System, new Name.Normal("ISeq"), new ParamHash(1, default), Arr(itemType));

//        // list<> class System.List<>
//        public static Path List(Path itemPath)
//            => new Nested(System, new Name.Normal("List"), new ParamHash(1, default), Arr(itemPath));
//    }    

//    public static class PathExtensions
//    {
//        public static bool IsTypeInstOfList(this Path path)
//        {
//            if (path is Path.Nested nestedPath)
//            {
//                return nestedPath.Outer.Equals(Path.System) &&
//                    nestedPath.Name.Equals(new Name.Normal("List")) &&
//                    nestedPath.ParamHash.Equals(new ParamHash(1, default));
//            }

//            return false;
//        }

//        public static bool IsTypeInstOfListIter(this Path path)
//        {
//            if (path is Path.Nested nestedPath)
//            {
//                return IsTypeInstOfList(nestedPath.Outer) &&
//                    nestedPath.Name.Equals(new Name.Anonymous(0)) &&
//                    nestedPath.ParamHash.Equals(new ParamHash(0, default));
//            }

//            return false;
//        }

//        public static Path.Nested Child(this Path.Normal outer, Name name, ParamHash paramHash, ImmutableArray<Path> typeArgs)
//            => new Path.Nested(outer, name, paramHash, typeArgs);

//        public static Path.Nested Child(this Path.Normal outer, string name, ParamHash paramHash, ImmutableArray<Path> typeArgs)
//            => new Path.Nested(outer, new Name.Normal(name), paramHash, typeArgs);


//        public static Path.Nested Child(this Path.Normal outer, Name name)
//            => new Path.Nested(outer, name, ParamHash.None, default);

//        public static Path.Nested Child(this Path.Normal outer, string name)
//            => new Path.Nested(outer, new Name.Normal(name), ParamHash.None, default);


//        // no typeparams, all normal paramtypes
//        public static Path.Nested Child(this Path.Normal outer, Name name, params Path[] types)
//            => new Path.Nested(outer, name, new ParamHash(0, types.Select(type => new ParamHashEntry(ParamKind.Default, type)).ToImmutableArray()), default);

//        public static Path.Nested Child(this Path.Normal outer, string name, params Path[] types)
//            => new Path.Nested(outer, new Name.Normal(name), new ParamHash(0, types.Select(type => new ParamHashEntry(ParamKind.Default, type)).ToImmutableArray()), default);


//        // no typeparams version
//        public static Path.Nested Child(this Path.Normal outer, Name name, params ParamHashEntry[] entries)
//            => new Path.Nested(outer, name, new ParamHash(0, entries.ToImmutableArray()), default);

//        public static Path.Nested Child(this Path.Normal outer, string name, params ParamHashEntry[] entries)
//            => new Path.Nested(outer, new Name.Normal(name), new ParamHash(0, entries.ToImmutableArray()), default);
//    }
//}