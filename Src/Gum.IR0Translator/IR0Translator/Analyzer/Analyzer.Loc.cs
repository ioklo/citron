using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = Gum.Syntax;
using R = Gum.IR0;

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

        LocResult AnalyzeIdentifierLoc(S.IdentifierExp idExp)
        {
            var result = ResolveIdentifierIdExp(idExp, resolveHint);

            switch (result)
            {
                case NotFoundIdentifierResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, idExp);
                    break;

                case ErrorIdentifierResult errorResult:
                    HandleErrorIdentifierResult(idExp, errorResult);
                    break;

                case ExpIdentifierResult expResult:
                    context.AddLambdaCapture(expResult.LambdaCapture);
                    return new LocResult(expResult.Exp, expResult.TypeValue);

                case InstanceFuncIdentifierResult:
                    throw new NotImplementedException();

                case StaticFuncIdentifierResult:
                    throw new NotImplementedException();

                case TypeIdentifierResult:
                    context.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, idExp);
                    break;

                case EnumElemIdentifierResult enumElemResult:
                    if (enumElemResult.IsStandalone)      // 인자 없이 있는 것
                    {
                        throw new NotImplementedException();
                        // return new ExpResult(new NewEnumExp(enumElemResult.Name, Array.Empty<NewEnumExp.Elem>()), enumElem.EnumTypeValue);
                    }
                    else
                    {
                        // TODO: Func일때 감싸기
                        throw new NotImplementedException();
                    }
            }

            throw new UnreachableCodeException();

        }
        
        LocResult AnalyzeLoc(S.Exp exp)
        {
            switch(exp)
            {
                case S.IdentifierExp idExp: return AnalyzeIdentifierLoc(idExp);

            }

        }
    }
}
