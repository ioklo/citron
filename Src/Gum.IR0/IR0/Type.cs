using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    public abstract class OuterType
    {        
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class RootOuterType : OuterType 
    {
        ModuleName moduleName;
        NamespacePath namespacePath;
    }

    public class OuterType<TType> : OuterType { }    
    public class ClassOuterType : OuterType<ClassType> { }
    public class StructOuterType : OuterType<StructType> { }
    public class EnumOuterType : OuterType<EnumType> { }
    public class InterfaceOuterType : OuterType<InterfaceType> { }

    // IR0에서 타입을 나타낼 때 쓰는 자료구조    
    public abstract class Type
    {
        // predefined named type
        // [System.Runtime]System.Boolean
        public static Type Bool = new StructType(new RootOuterType("System.Runtime", new NamespacePath("System")), "Boolean", TypeContext.Empty);
        public static Type Int = new StructType(new RootOuterType("System.Runtime", new NamespacePath("System")), "Int32", TypeContext.Empty);
        public static Type String = new ClassType(new RootOuterType("System.Runtime", new NamespacePath("System")), "String", TypeContext.Empty);
        
        // seq<> interface는 있다
        public static Type Seq(Type itemType)
        {
            var typeContext = new TypeContextBuilder().Add(0, 0, itemType).Build();
            return new InterfaceType(new RootOuterType("System.Runtime", new NamespacePath("System")), "ISeq", typeContext);
        }

        // list<> class System.List<>
        public static Type List(Type itemType)
        {
            var typeContext = new TypeContextBuilder().Add(0, 0, itemType).Build();
            return new ClassType(new RootOuterType("System.Runtime", new NamespacePath("System")), "List", typeContext);
        }

        internal Type() { }
    }

    // outer가 될 수 있는 타입
    // class, struct, enum, interface    
    [AutoConstructor, ImplementIEquatable]
    public partial class ClassType : Type
    {
        OuterType outer;
        Name name;
        TypeContext typeContext;
    }
    
    // int, 
    [AutoConstructor, ImplementIEquatable]
    public partial class StructType : Type
    {
        OuterType outer;
        Name name;
        TypeContext typeContext;
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumType : Type
    {
        OuterType outer;
        Name name;
        TypeContext typeContext;
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class InterfaceType : Type
    {
        OuterType outer;
        Name name;
        TypeContext typeContext;
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
        public SeqDeclId SeqDeclId { get; }
        public TypeContext TypeContext { get; }
    }

    // var l = () => { ... }; 에서 l 타입
    [AutoConstructor, ImplementIEquatable]
    public partial class AnonymousLambdaType : Type
    {
        public LambdaDeclId LambdaDeclId { get; }
    }
}
