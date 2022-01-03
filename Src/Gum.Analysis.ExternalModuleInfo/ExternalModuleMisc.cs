using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    static class ExternalModuleMisc
    {
        public static ExternalModuleTypeDecl Make(M.TypeDecl typeDecl)
        {
            switch(typeDecl)
            {
                case M.StructDecl structDecl:
                    return new ExternalModuleStructDecl(structDecl);

                case M.EnumDecl enumInfo:
                    return new ExternalModuleEnumInfo(enumInfo);

                case M.EnumElem enumElemInfo:
                    return new ExternalModuleEnumElemInfo(enumElemInfo);

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}
