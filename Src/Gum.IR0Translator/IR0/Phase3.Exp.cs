using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Phase3
    {
        StringExp VisitStringExp(S.StringExp exp)
        {
            var ir0StrExpElems = new List<StringExpElement>();
            foreach (var elem in exp.Elements)
            {
                var ir0StrExpElem = VisitStringExpElement(elem);
                ir0StrExpElems.Add(ir0StrExpElem);
            }

            return analyzer.AnalyzeStringExp(ir0StrExpElems);
        }

        StringExpElement VisitStringExpElement(S.StringExpElement elem)
        {
            throw new NotImplementedException();
        }
    }
}
