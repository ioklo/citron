using R = Gum.IR0;
using Gum.Infra;
using Pretune;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        abstract class ExpResult
        {
        }

        [AutoConstructor, ImplementIEquatable]
        class ExpExpResult : ExpResult
        {
            public R.Exp Exp { get; }
        }

        [AutoConstructor, ImplementIEquatable]
        class LocExpResult : ExpResult
        {
            public R.Loc Loc { get; }
        }

        struct ExpResult
        {
            R.Exp? exp;
            R.Loc? loc;

            public R.Exp ExpRaw => exp!;
            public R.Loc LocRaw => loc!;
            public TypeValue TypeValue { get; }

            public bool IsExp => exp != null;
            public bool IsLoc => loc != null;

            public R.Exp WrapExp()
            {
                if (exp != null)
                    return exp;

                else if (loc != null)
                    return new R.LoadExp(loc);

                else
                    throw new UnreachableCodeException();
            }

            public R.Loc WrapLoc()
            {
                if (loc != null)
                    return loc;

                else if (exp != null)
                    return new R.TempLoc(exp, TypeValue.GetRType());

                else
                    throw new UnreachableCodeException();
            }

            public ExpResult(R.Exp exp, TypeValue typeValue) { this.exp = exp; this.loc = null; this.TypeValue = typeValue; }
            public ExpResult(R.Loc loc, TypeValue typeValue) { this.exp = null; this.loc = loc; this.TypeValue = typeValue; }
        }        
    }
}
