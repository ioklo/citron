using System;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    // T: depth는 지역적이므로, 주어진 컨텍스트 안에서만 의미가 있다
    [AutoConstructor, ImplementIEquatable]
    public partial class TypeVarTypeValue : TypeValue
    {
        RItemFactory ritemFactory;

        public int Index { get; }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            var typeValue = typeEnv.GetValue(Index);
            if (typeValue != null)
                return typeValue;

            return this;
        }

        public override R.Path GetRPath() => new R.Path.TypeVarType(Index);

        public override int GetTotalTypeParamCount()
        {
            throw new InvalidOperationException();
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new NotImplementedException();        
    }
}
