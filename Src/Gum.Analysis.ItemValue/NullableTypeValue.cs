using System;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public class NullableTypeValue : TypeSymbol
    {
        ItemValueFactory factory;
        TypeSymbol innerTypeValue;

        public NullableTypeValue(ItemValueFactory factory, TypeSymbol innerTypeValue)
        {
            this.factory = factory;
            this.innerTypeValue = innerTypeValue;
        }

        public TypeSymbol GetInnerTypeValue()
        {
            return innerTypeValue;
        }

        // T? => int?
        public override TypeSymbol Apply(TypeEnv typeEnv)
        {
            var applied = innerTypeValue.Apply(typeEnv);
            return factory.MakeNullableTypeValue(applied);
        }

        // 
        public override R.Path GetRPath()
        {
            var innerPath = innerTypeValue.GetRPath();
            return new R.Path.NullableType(innerPath);
        }

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            throw new InvalidOperationException();
        }
    }
}
