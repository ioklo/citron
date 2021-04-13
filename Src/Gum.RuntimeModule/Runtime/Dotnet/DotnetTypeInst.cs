using Gum.CompileTime;
using System;
using System.Reflection;

namespace Gum.Runtime.Dotnet
{
    class DotnetTypeInst : TypeInst
    {
        TypeInfo typeInfo;

        public DotnetTypeInst(TypeInfo typeInfo)
        {
            this.typeInfo = typeInfo;
        }

        public override TypeValue GetTypeValue()
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
