using System;
using R = Gum.IR0;

namespace Gum.Analysis
{
    // "void"
    public class VoidTypeValue : TypeSymbol
    {
        public static readonly VoidTypeValue Instance = new VoidTypeValue();
        private VoidTypeValue() { }
        public override bool Equals(object? obj)
        {
            return obj == Instance;
        }

        public override TypeSymbol Apply(TypeEnv typeEnv)
        {
            return this;
        }

        public override R.Path GetRPath()
        {
            return R.Path.VoidType.Instance;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }
    }
}
