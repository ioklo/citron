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
            : base(null, new ItemId("Environment"), Enumerable.Empty<string>(), null, () => new ObjectValue(null))
        {
        }

        public override void Build(RuntimeModuleTypeBuilder builder)
        {
            var stringTypeValue = new TypeValue.Normal(RuntimeModule.StringId);

            builder.AddMemberVar("HomeDir", false, stringTypeValue);
            builder.AddMemberVar("ScriptDir", false, stringTypeValue);
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
