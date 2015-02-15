using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public interface IExpVisitor<Ret, Arg>
    {
        Ret Visit(StarExp exp, Arg a);
        Ret Visit(PlusExp exp, Arg a);
        Ret Visit(OptionalExp exp, Arg a);
        Ret Visit(SequenceExp exp, Arg a);
        Ret Visit(SymbolExp exp, Arg a);
    }

    public static class IExpVisitorHelper
    {
        static Arg Apply<Ret, Arg>(this IExpVisitor<Ret, Arg> Visitor, Exp e)
        {
            return default(Arg);
        }    
    }
}
