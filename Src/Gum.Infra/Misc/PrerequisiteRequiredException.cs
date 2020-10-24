using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Misc
{
    public class PrerequisiteRequiredException : Exception
    {        
        public override string Message { get; }            

        public PrerequisiteRequiredException(string prerequisite)
        {
            Message = $"{prerequisite}이 필요해서 아직 구현하지 않았습니다";
        }
    }
}
