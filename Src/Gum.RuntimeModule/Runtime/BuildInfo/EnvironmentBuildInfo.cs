using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Runtime
{
    class EnvironmentBuildInfo : RuntimeModuleTypeBuildInfo.Class
    {
        public EnvironmentBuildInfo()
            : base(null, ModuleItemId.Make("Environment"), Enumerable.Empty<string>(), null, () => new ObjectValue(null))
        {
        }

        public override void Build(RuntimeModuleTypeBuilder builder)
        {
            var stringTypeValue = TypeValue.MakeNormal(RuntimeModule.StringId);

            builder.AddMemberVar(Name.MakeText("HomeDir"), false, stringTypeValue);
            builder.AddMemberVar(Name.MakeText("ScriptDir"), false, stringTypeValue);
        }
    }

    class EnvironmentObject : GumObject
    {
        Value homeDir;
        Value scriptDir;

        public string this[string varName]
        {
            get { return Environment.GetEnvironmentVariable(varName); }
            set { Environment.SetEnvironmentVariable(varName, value); }
        }

        public EnvironmentObject(Value homeDir, Value scriptDir)
        {
            this.homeDir = homeDir;
            this.scriptDir = scriptDir;
        }

        public override Value GetMemberValue(Name varName)
        {
            if (varName.Text == "HomeDir")
                return homeDir;

            if (varName.Text == "ScriptDir")
                return scriptDir;

            throw new InvalidOperationException();
        }
    }
}
