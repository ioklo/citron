using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
#pragma warning disable CS0660, CS0661
    public abstract class TypeValue
    {
        // "var"
        public class Var : TypeValue
        {
            public static Var Instance { get; } = new Var();
            private Var() { }
        }

        // T
        public class TypeVar : TypeValue
        {
            public ModuleItemId ParentId { get; }
            public string Name { get; }

            internal TypeVar(ModuleItemId parentId, string name)
            {
                ParentId = parentId;
                Name = name;
            }

            public override bool Equals(object? obj)
            {
                return obj is TypeVar value &&
                       EqualityComparer<ModuleItemId>.Default.Equals(ParentId, value.ParentId) &&
                       Name == value.Name;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ParentId, Name);
            }
        }
        
        public class Normal : TypeValue
        {
            public ModuleItemId TypeId { get; }
            public TypeArgumentList TypeArgList { get; }

            internal Normal(ModuleItemId typeId, TypeArgumentList typeArgList)
            {
                this.TypeId = typeId;
                this.TypeArgList = typeArgList;
            }

            public override bool Equals(object? obj)
            {
                return obj is Normal value &&
                       EqualityComparer<ModuleItemId>.Default.Equals(TypeId, value.TypeId) &&
                       EqualityComparer<TypeArgumentList>.Default.Equals(TypeArgList, value.TypeArgList);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(TypeId, TypeArgList);
            }
        }

        // "void"
        public class Void : TypeValue
        {
            public static Void Instance { get; } = new Void();
            private Void() { }
        }

        // ArgTypeValues => RetValueTypes
        public class Func : TypeValue
        {
            public TypeValue Return { get; }
            public ImmutableArray<TypeValue> Params { get; }

            public Func(TypeValue ret, IEnumerable<TypeValue> parameters)
            {
                Return = ret;
                Params = parameters.ToImmutableArray();
            }

            public override bool Equals(object? obj)
            {
                return obj is Func value &&
                       EqualityComparer<TypeValue>.Default.Equals(Return, value.Return) &&
                       Enumerable.SequenceEqual(Params, value.Params);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Return, Params);
            }
        }

        public class EnumElem : TypeValue
        {
            public EnumElemInfo elemInfo { get; }
            public Normal EnumTypeValue { get; }
            public string Name { get; }

            public EnumElem(Normal enumTypeValue, string name)
            {
                EnumTypeValue = enumTypeValue;
                Name = name;
            }
        }

        public static Var MakeVar() => Var.Instance;
        public static TypeVar MakeTypeVar(ModuleItemId parentId, string name) => new TypeVar(parentId, name);
        public static Normal MakeNormal(ModuleItemId typeId, TypeArgumentList args) => new Normal(typeId, args);
        public static Normal MakeNormal(ModuleItemId typeId) => new Normal(typeId, TypeArgumentList.Empty);
        public static Void MakeVoid() => Void.Instance;
        public static Func MakeFunc(TypeValue ret, IEnumerable<TypeValue> parameters) => new Func(ret, parameters);
        public static EnumElem MakeEnumElem(Normal enumTypeValue, string name) => new EnumElem(enumTypeValue, name);

        // opeator
        public static bool operator ==(TypeValue? left, TypeValue? right)
        {
            return EqualityComparer<TypeValue?>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeValue? left, TypeValue? right)
        {
            return !(left == right);
        }

        
    }
    
#pragma warning restore CS0660, CS0661
}
