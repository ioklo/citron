using System;

namespace Gum.Analysis
{
    public class NullableTypeValue : ITypeSymbol
    {
        ItemValueFactory factory;
        ITypeSymbol innerTypeValue;

        public NullableTypeValue(ItemValueFactory factory, ITypeSymbol innerTypeValue)
        {
            this.factory = factory;
            this.innerTypeValue = innerTypeValue;
        }

        public ITypeSymbol GetInnerTypeValue()
        {
            return innerTypeValue;
        }

        // T? => int?
        public override ITypeSymbol Apply(TypeEnv typeEnv)
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
