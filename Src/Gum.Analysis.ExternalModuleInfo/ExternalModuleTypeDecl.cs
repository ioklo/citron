using Gum.CompileTime;

namespace Gum.Analysis
{
    public abstract class ExternalModuleTypeDecl : IModuleTypeDecl
    {
        public abstract TypeName GetName();
    }
}