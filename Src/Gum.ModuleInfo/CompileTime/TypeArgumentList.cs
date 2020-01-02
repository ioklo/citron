using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.CompileTime
{
    public class TypeArgumentList
    {
        public TypeArgumentList? Outer { get; }
        public ImmutableArray<TypeValue> Args { get; }        

        public static TypeArgumentList Empty { get; } = new TypeArgumentList(null, Enumerable.Empty<TypeValue>());

        public static TypeArgumentList Make(params TypeValue[] typeArgs)
        {
            return new TypeArgumentList(null, typeArgs);
        }        

        public static TypeArgumentList Make(TypeValue[] typeArgs0, params TypeValue[][] typeArgList)
        {
            var curList = new TypeArgumentList(null, typeArgs0);

            foreach (var elem in typeArgList)
                curList = new TypeArgumentList(curList, elem);

            return curList;
        }

        public static TypeArgumentList Make(TypeArgumentList? outer, IEnumerable<TypeValue> typeArgs)
        {
            return new TypeArgumentList(outer, typeArgs);
        }

        public static TypeArgumentList Make(TypeArgumentList? outer, params TypeValue[] typeArgs)
        {
            return new TypeArgumentList(outer, typeArgs);
        }

        private TypeArgumentList(TypeArgumentList? outer, IEnumerable<TypeValue> args)
        {
            Outer = outer;
            Args = args.ToImmutableArray();
        }        

        public override bool Equals(object? obj)
        {
            return obj is TypeArgumentList list &&
                   EqualityComparer<TypeArgumentList?>.Default.Equals(Outer, list.Outer) &&
                   SeqEqComparer.Equals(Args, list.Args);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Outer, Args);
        }

        public static bool operator ==(TypeArgumentList? left, TypeArgumentList? right)
        {
            return EqualityComparer<TypeArgumentList?>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeArgumentList? left, TypeArgumentList? right)
        {
            return !(left == right);
        }

        public int GetTotalLength()
        {
            if (Outer != null)
                return Outer.GetTotalLength() + Args.Length;

            return Args.Length;
        }
    }    
}
