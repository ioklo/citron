using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Infra
{
    public enum Prerequisite
    {
        Class,
        Struct,

        Enum,
        TypeHint,
        IfTestClassStmt,
        IfTestEnumStmt,
        Generics,
        External,
        Static,
        Interface,
    }

    public class PrerequisiteRequiredException : Exception
    {        
        public override string Message { get; }            

        public PrerequisiteRequiredException(params Prerequisite[] prerequisites)
        {
            Message = $"{string.Join(',', prerequisites)} 구현이 필요합니다";
        }
    }
}
