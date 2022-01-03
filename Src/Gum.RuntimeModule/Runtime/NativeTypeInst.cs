using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Runtime
{
    class NativeTypeInst : TypeInst
    {
        // key
        TypeSymbol typeValue;
        Func<Value> defaultValueFactory;

        public NativeTypeInst(TypeSymbol typeValue, Func<Value> defaultValueFactory)
        {
            this.typeValue = typeValue;
            this.defaultValueFactory = defaultValueFactory;
        }

        public override Value MakeDefaultValue()
        {
            return defaultValueFactory();
        }

        public override TypeSymbol GetTypeValue()
        {
            return typeValue;
        }

        public override bool Equals(object? obj)
        {
            return obj is NativeTypeInst inst &&
                   EqualityComparer<TypeSymbol>.Default.Equals(typeValue, inst.typeValue);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(typeValue);
        }

        public static bool operator ==(NativeTypeInst? left, NativeTypeInst? right)
        {
            return EqualityComparer<NativeTypeInst?>.Default.Equals(left, right);
        }

        public static bool operator !=(NativeTypeInst? left, NativeTypeInst? right)
        {
            return !(left == right);
        }
    }
}
