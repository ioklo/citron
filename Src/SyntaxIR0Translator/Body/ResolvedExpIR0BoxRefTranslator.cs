using Citron.IR0;
using Citron.Symbol;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.Diagnostics;


namespace Citron.Analysis;

// 가장 최외각만 담당을 한다
struct ResolvedExpIR0BoxRefTranslator : IResolvedExpVisitor<TranslationResult<Exp>>
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    public static TranslationResult<Exp> Translate(ResolvedExp reExp, ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        var translator = new ResolvedExpIR0BoxRefTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
        return reExp.Accept<ResolvedExpIR0BoxRefTranslator, TranslationResult<Exp>>(ref translator);
    }

    static TranslationResult<Exp> Error()
    {
        return TranslationResult.Error<Exp>();
    }

    static TranslationResult<Exp> Valid(Exp exp)
    {
        return TranslationResult.Error<Exp>();
    }

    // box var& x = ce.x;
    // 1. ce.x가 box T& 타입인 경우, -> 그대로 사용
    // 2. ce.x가 local T& 타입인 경우, -> 에러
    // 3. ce.x가 일반 타입인 경우
    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
    {
        var expType = reExp.GetExpType();
        if (expType is BoxRefType)
        {
            return ResolvedExpIR0ExpTranslator.Translate(reExp, context, bDerefIfTypeIsRef: false, nodeForErrorReport);
        }
        else if (expType is LocalRefType)
        {
            context.AddFatalError(A3101_BoxRef_CantMakeBoxRefWithLocalRef, nodeForErrorReport);
            return Error();
        }
        else // 나머지 경우
        {
            if (reExp.Symbol.IsStatic()) // static의 경우 별도 조건없이 그냥 생성이 가능하다
            {
                // box의 holder 부분이 null이 된다
                return Valid(new StaticBoxRefExp(new ClassMemberLoc(Instance: null, reExp.Symbol), new BoxRefType(expType)));
            }
            else
            {
                if (reExp.HasExplicitInstance) // explicit instance가 있으면, ce.x
                {
                    // bWrapExpAsLoc: true, F().x 도 가능하게
                    Debug.Assert(reExp.ExplicitInstance != null);
                    var result = ResolvedInstanceExpIR0LocTranslator.Translate(reExp.ExplicitInstance, context, bWrapExpAsLoc: true, nodeForErrorReport, A3102_BoxRef_InstanceIsNotLocation);
                    if (!result.IsValid(out var locResult))
                        return Error();

                    return Valid(new ClassMemberVarBoxRefExp(locResult.Loc, reExp.Symbol));
                }
                else // x (this.x)
                {
                    return Valid(new ClassMemberVarBoxRefExp(new ThisLoc(), reExp.Symbol));
                }
            }
        }
    }

    // box var& x = se.x;  // o_o
    // 1. se.x가 box T& 타입인 경우, -> 그대로 사용
    // 2. se.x가 local T& 타입인 경우, -> 에러
    // 3. se.x가 일반 타입인 경우 -> ResolvedInstanceExpBoxRefTranslator를 써야 한다
    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
    {
        var expType = reExp.GetExpType();
        if (expType is BoxRefType)
        {
            return ResolvedExpIR0ExpTranslator.Translate(reExp, context, bDerefIfTypeIsRef: false, nodeForErrorReport);
        }
        else if (expType is LocalRefType)
        {
            context.AddFatalError(A3101_BoxRef_CantMakeBoxRefWithLocalRef, nodeForErrorReport);
            return Error();
        }
        else // 나머지 경우
        {
            if (reExp.Symbol.IsStatic()) // static의 경우 별도 조건없이 그냥 생성이 가능하다
            {
                // box의 holder 부분이 null이 된다
                return Valid(new StaticBoxRefExp(new StructMemberLoc(Instance: null, reExp.Symbol), new BoxRefType(expType)));
            }
            else
            {
                if (reExp.HasExplicitInstance) // explicit instance가 있으면, se.x
                {
                    Debug.Assert(reExp.ExplicitInstance != null);
                    return ResolvedInstanceExpStructMemberVarBinder.Bind(reExp.ExplicitInstance, context, nodeForErrorReport, reExp.Symbol);
                }
                else // x (this.x)
                {
                    // this는 S& 타입 이므로 에러
                    context.AddFatalError(A3101_BoxRef_CantMakeBoxRefWithLocalRef, nodeForErrorReport);
                    return Error();
                }
            }
        }
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitLocalVar(ResolvedExp.LocalVar reExp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitThis(ResolvedExp.ThisVar reExp)
    {
        throw new System.NotImplementedException();
    }
}
