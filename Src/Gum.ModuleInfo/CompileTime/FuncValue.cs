using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.CompileTime
{
    public class FuncValue
    {
        public ModuleItemId FuncId { get; }
        public TypeArgumentList TypeArgList { get; }

        public FuncValue(ModuleItemId funcId, TypeArgumentList typeArgList)
        {
            FuncId = funcId;
            TypeArgList = typeArgList;
        }

        public override bool Equals(object? obj)
        {
            return obj is FuncValue value &&
                   EqualityComparer<ModuleItemId>.Default.Equals(FuncId, value.FuncId) &&
                   EqualityComparer<TypeArgumentList>.Default.Equals(TypeArgList, value.TypeArgList);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FuncId, TypeArgList);
        }

        public static bool operator ==(FuncValue? left, FuncValue? right)
        {
            return EqualityComparer<FuncValue?>.Default.Equals(left, right);
        }

        public static bool operator !=(FuncValue? left, FuncValue? right)
        {
            return !(left == right);
        }
    }
}
