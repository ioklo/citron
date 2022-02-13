using System;
using R = Citron.IR0;

namespace Citron.Analysis
{
    // "var"
    public class VarTypeValue : ITypeSymbol
    {
        public static readonly VarTypeValue Instance = new VarTypeValue();
        private VarTypeValue() { }

        public override ITypeSymbol Apply(TypeEnv typeEnv) { return this; }

        // var는 translation패스에서 추론되기 때문에 IR0에 없다
        public override R.Path GetRPath() { throw new InvalidOperationException();  }

        public override int GetTotalTypeParamCount()
            => throw new InvalidOperationException();

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();
    }
}
