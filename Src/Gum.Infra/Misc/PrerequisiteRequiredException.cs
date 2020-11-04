using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Misc
{
    public enum Prerequisite
    {
        Class,
        Enum,
        TypeHint,
        IfTestClassStmt,
        IfTestEnumStmt,
        Generics,
        External,
        Static,
        Struct,
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
