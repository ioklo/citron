using Gum.CompileTime;
using System.Reflection;

namespace Gum.Runtime.Dotnet
{
    class DotnetValue : Value
    {
        object? obj;

        public DotnetValue(object? obj)
        {
            this.obj = obj;
        }

        public Value GetMemberValue(Name varName)
        {
            var fieldInfo = obj!.GetType().GetField(varName.Text);

            return new DotnetValue(fieldInfo.GetValue(obj));
        }

        public TypeInst GetTypeInst()
        {
            return new DotnetTypeInst(obj!.GetType().GetTypeInfo());
        }

        public override Value MakeCopy()
        {
            return new DotnetValue(obj);
        }

        public override void SetValue(Value fromValue)
        {
            if (fromValue is DotnetValue dotnetFromValue)
            {
                this.obj = dotnetFromValue.obj;
            }
        }
    }
}
