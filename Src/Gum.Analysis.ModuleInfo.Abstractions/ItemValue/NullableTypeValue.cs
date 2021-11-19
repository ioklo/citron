using System;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public class NullableTypeValue : TypeValue
    {
        ItemValueFactory factory;
        TypeValue innerTypeValue;

        public NullableTypeValue(ItemValueFactory factory, TypeValue innerTypeValue)
        {
            this.factory = factory;
            this.innerTypeValue = innerTypeValue;
        }

        public TypeValue GetInnerTypeValue()
        {
            return innerTypeValue;
        }

        // T? => int?
        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            var applied = innerTypeValue.Apply_TypeValue(typeEnv);
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
