using Citron.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citron.Runtime
{
    class NativeTypeInst : TypeInst
    {
        // key
        ITypeSymbol typeValue;
        Func<Value> defaultValueFactory;

        public NativeTypeInst(ITypeSymbol typeValue, Func<Value> defaultValueFactory)
        {
            this.typeValue = typeValue;
            this.defaultValueFactory = defaultValueFactory;
        }

        public override Value MakeDefaultValue()
        {
            return defaultValueFactory();
        }

        public override ITypeSymbol GetTypeValue()
        {
            return typeValue;
        }

        public override bool Equals(object? obj)
        {
            return obj is NativeTypeInst inst &&
                   EqualityComparer<ITypeSymbol>.Default.Equals(typeValue, inst.typeValue);
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
