using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = Gum.Syntax;
using R = Gum.IR0;
using static Gum.IR0Translator.AnalyzeErrorCode;
using Gum.Infra;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [AutoConstructor, ImplementIEquatable]
        partial struct LocResult
        {
            public R.Loc Loc { get; }
            public TypeValue TypeValue { get; }
        }
        
        LocResult AnalyzeLoc(S.Exp exp)
        {
            var result = ResolveIdentifier(exp, ResolveHint.None);

            switch (result)
            {
                case NotFoundIdentifierResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);
                    break;

                case ErrorIdentifierResult errorResult:
                    HandleErrorIdentifierResult(exp, errorResult);
                    break;
                
                case ExpIdentifierResult expResult:
                    context.AddFatalError();
                    break;

                case LocIdentifierResult locResult:
                    return new LocResult(locResult.Loc, locResult.TypeValue);

                case InstanceFuncIdentifierResult:
                    context.AddFatalError();
                    break;

                case StaticFuncIdentifierResult:
                    context.AddFatalError();
                    break;

                case TypeIdentifierResult:
                    context.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, exp);
                    break;
                
                case EnumElemIdentifierResult _:
                    throw new UnreachableCodeException(); // 힌트 없이 EnumElem이 나올 수 없다
            }

            throw new UnreachableCodeException();
        }
    }
}
