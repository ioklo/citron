using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = Gum.Syntax;
using R = Gum.IR0;
using static Gum.IR0Translator.AnalyzeErrorCode;

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

                case LocIdentifierResult locResult:
                    if (locResult.Loc is R.TempLoc)
                        context.AddFatalError();

                    return new LocResult(locResult.Loc, locResult.TypeValue);

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

            //switch(exp)
            //{
            //    case S.IdentifierExp idExp: return AnalyzeIdentifierLoc(idExp);

            //}

        }
    }
}
