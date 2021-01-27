﻿using Gum.Misc;
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
    
    [AutoConstructor]
    public partial class ExternalType : NormalType, IEquatable<ExternalType?>
    {
        public ModuleName ModuleName { get; }
        public NamespacePath NamespacePath { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ExternalType);
        }

        public bool Equals(ExternalType? other)
        {
            return other != null &&
                   ModuleName.Equals(other.ModuleName) &&
                   NamespacePath.Equals(other.NamespacePath) &&
                   Name.Equals(other.Name) &&
                   TypeArgs.SequenceEqual(other.TypeArgs);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ModuleName);
            hashCode.Add(NamespacePath);
            hashCode.Add(Name);
            hashCode.AddSequence(TypeArgs);
            return hashCode.ToHashCode();
        }
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
