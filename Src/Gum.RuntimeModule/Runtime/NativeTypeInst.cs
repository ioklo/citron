using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.Runtime
{
    class NativeTypeInst : TypeInst
    {
        // key
        TypeValue typeValue;
        Func<Value> defaultValueFactory;

        public NativeTypeInst(TypeValue typeValue, Func<Value> defaultValueFactory)
        {
            this.typeValue = typeValue;
            this.defaultValueFactory = defaultValueFactory;
        }

        public override Value MakeDefaultValue()
        {
            return defaultValueFactory();
        }

        public override TypeValue GetTypeValue()
        {
            return typeValue;
        }

        public override bool Equals(object? obj)
        {
            return obj is NativeTypeInst inst &&
                   EqualityComparer<TypeValue>.Default.Equals(typeValue, inst.typeValue);
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
