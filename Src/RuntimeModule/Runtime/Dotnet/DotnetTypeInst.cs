using System;
using System.Reflection;

namespace Citron.Runtime.Dotnet
{
    class DotnetTypeInst : TypeInst
    {
        TypeInfo typeInfo;

        public DotnetTypeInst(TypeInfo typeInfo)
        {
            this.typeInfo = typeInfo;
        }

        public override ITypeSymbol GetTypeValue()
        {
            DotnetMisc.MakeTypeId(typeInfo.BaseType);

            throw new NotImplementedException();
        }

        public override Value MakeDefaultValue()
        {
            return new DotnetValue(null);
        }
    }
}
