using Pretune;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    // AppliedType
    public abstract class Type
    {
    }

    public abstract class NormalType : Type
    {
    }

    // 모듈 내에 존재하는 타입
    [AutoConstructor, ImplementIEquatable]
    public partial class InternalType : NormalType
    {
        public NamespacePath NamespacePath { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ExternalType : NormalType
    {
        public ModuleName ModuleName { get; }
        public NamespacePath NamespacePath { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class MemberType : NormalType
    {
        public NormalType Outer { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }
    }

    // 로컬, 사용한 곳의 환경에 따라 가리키는 것이 달라진다
    [AutoConstructor, ImplementIEquatable]
    public partial class TypeVarType : Type
    {
        public int Depth { get; } 
        public int Index { get; }
        public string Name { get; }
    }

    public class VoidType : Type
    {
        public static VoidType Instance { get; } = new VoidType();
        private VoidType() { }
    }
}
