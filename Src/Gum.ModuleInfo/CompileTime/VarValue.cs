using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Gum.CompileTime
{
    public class VarValue
    {
        public ModuleItemId VarId { get; }
        public TypeArgumentList OuterTypeArgList { get; } // variable은 자체로 타입 인자가 없으므로, TypeValue, FuncValue랑 다르게 outer부터 시작한다

        public VarValue(ModuleItemId varId, TypeArgumentList outerTypeArgList)
        {
            VarId = varId;
            OuterTypeArgList = outerTypeArgList;
        }

        public override bool Equals(object? obj)
        {
            return obj is VarValue value &&
                   EqualityComparer<ModuleItemId>.Default.Equals(VarId, value.VarId) &&
                   EqualityComparer<TypeArgumentList>.Default.Equals(OuterTypeArgList, value.OuterTypeArgList);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VarId, OuterTypeArgList);
        }

        public static bool operator ==(VarValue? left, VarValue? right)
        {
            return EqualityComparer<VarValue?>.Default.Equals(left, right);
        }

        public static bool operator !=(VarValue? left, VarValue? right)
        {
            return !(left == right);
        }
    }
}
