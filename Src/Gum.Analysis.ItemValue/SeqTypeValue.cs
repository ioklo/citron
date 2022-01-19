using System;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    // seq ref int F(ref int a, ref int b) { yield ref a; }
    [AutoConstructor]
    public partial class SeqTypeValue : ITypeSymbol
    {
        RItemFactory ritemFactory;
        R.Path.Nested seqFunc;
        public ITypeSymbol YieldType { get; }

        public override ITypeSymbol Apply(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Path GetRPath()
        {
            return seqFunc;
        }

        public override int GetTotalTypeParamCount()
        {
            throw new NotImplementedException();
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();
    }
}
