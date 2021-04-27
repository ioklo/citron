using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using static Gum.Infra.Misc;

namespace Gum.IR0
{
    // IR0에서 타입을 나타낼 때 쓰는 자료구조    
    public abstract class Type
    {
        // predefined named type
        // [System.Runtime]System.Boolean
        public static Type Bool = new StructType(Path.Make("System.Runtime", new NamespacePath("System"), "Boolean", default, ParamHash.None));
        public static Type Int = new StructType(Path.Make("System.Runtime", new NamespacePath("System"), "Int32", default, ParamHash.None));
        public static Type String = new ClassType(Path.Make("System.Runtime", new NamespacePath("System"), "String", default, ParamHash.None));
        
        // seq<> interface는 있다
        public static Type Seq(Type itemType)
        {   
            return new InterfaceType(Path.Make("System.Runtime", new NamespacePath("System"), "ISeq", Arr(itemType), ParamHash.None));
        }

        // list<> class System.List<>
        public static Type List(Type itemType)
        {
            return new ClassType(Path.Make("System.Runtime", new NamespacePath("System"), "List", Arr(itemType), ParamHash.None));
        }

        internal Type() { }
    }

    // outer가 될 수 있는 타입
    // class, struct, enum, interface    
    [AutoConstructor, ImplementIEquatable]
    public partial class ClassType : Type
    {
        Path path;
    }
    
    // int, 
    [AutoConstructor, ImplementIEquatable]
    public partial class StructType : Type
    {
        Path path;
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumType : Type
    {
        Path path;
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class InterfaceType : Type
    {
        Path path;
    }
    
    // from type alias, type parameter
    [AutoConstructor, ImplementIEquatable]
    public partial class TypeVar : Type
    {
        int depth;
        int index;
    }

    // Tuple<int i, string s>
    [AutoConstructor, ImplementIEquatable]
    public partial class TupleType : Type
    {
    }

    // box<int>
    [AutoConstructor, ImplementIEquatable]
    public partial class BoxType : Type
    {

    }

    // ref<string>
    [AutoConstructor, ImplementIEquatable]
    public partial class RefType : Type
    {
    }

    // TRef<T> => ref<int> (if T is int), string (if T is string)
    [AutoConstructor, ImplementIEquatable]
    public partial class GenericRefType : Type
    {
    }

    // Func<params array<int>> interface like type
    [AutoConstructor, ImplementIEquatable]
    public partial class FuncType : Type
    {
    }

    // void
    [ImplementIEquatable]
    public partial class VoidType : Type
    {
        public static readonly VoidType Instance = new VoidType();
        VoidType() { }
    }

    // nullable<>
    [AutoConstructor, ImplementIEquatable]
    public partial class NullableType : Type
    {
    }

    // seq T
    [AutoConstructor, ImplementIEquatable]
    public partial class AnonymousSeqType : Type
    {
        public Path Path { get; } // 돌려 쓰기        
    }

    // var l = () => { ... }; 에서 l 타입
    [AutoConstructor, ImplementIEquatable]
    public partial class AnonymousLambdaType : Type
    {
        public Path Path { get; }
    }
}
