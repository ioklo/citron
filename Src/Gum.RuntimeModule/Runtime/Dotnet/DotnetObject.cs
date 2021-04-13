using Gum.CompileTime;
using System;

namespace Gum.Runtime.Dotnet
{
    // 
    class DotnetObject : GumObject
    {
        TypeInst typeInst;        
        Object obj;

        public DotnetObject(TypeInst typeInst, Object obj)
        {
            this.typeInst = typeInst;
            this.obj = obj;
        }

        public override TypeInst GetTypeInst()
        {
            return typeInst;
        }

        public override Value GetMemberValue(Name varName)
        {
            var fieldInfo = obj.GetType().GetField(varName.Text!);
            return new DotnetValue(fieldInfo.GetValue(obj));
        }
    }
}
