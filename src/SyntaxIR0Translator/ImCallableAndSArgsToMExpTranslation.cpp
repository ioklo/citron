#include "ImCallableAndSArgsToMExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "RSymbol/DeclWithOuterTypeArgs.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"

#include "TranslationContext.h"
#include "ScopeContext.h"

#include "ImExp.h"
#include "ReExp.h"

#include "ImExpToReExpTranslation.h"
#include "ReExpToMLocTranslation.h"

#include "FuncMatching.h"


using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {
// (IntermediateExp, Args) -> TranslationResult<IR0ExpResult>
class ImCallableAndSArgsToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;

private:
    SExp* sCallable;
    SArguments* sArgs;

    TranslationContext& context;

    // S.ISyntaxNode nodeForCallExpErrorReport;
    // S.ISyntaxNode nodeForCallableErrorReport;

public:
    ImCallableAndSArgsToMExpTranslator(SExp* sCallable, SArguments* sArgs, TranslationContext& context)
        : sCallable(sCallable), sArgs(sArgs), context(context)
    {
    }

private:
    template<typename TMExp, typename... TArgs> requires std::derived_from<TMExp, MExp>
    ResultType Exp(TArgs&&... args)
    {
        return context.MakeMExp<TMExp>(std::forward<TArgs>(args)...);
    }
    
    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

    // CallExp 분석에서 Callable이 Lambda, func<>로 계산되는 경우
    ResultType HandleLoc(ImExp* imExp)
    {
        auto eReExp = TranslateImExpToReExp(imExp, context);
        if (!eReExp)
            return Error(move(eReExp));

        DesignatedDiagnostic<Error_CallExp_CallableExpressionIsNotCallable> designatedDiag;
        auto eNCallable = TranslateReExpToMLoc(*eReExp, /*bWrapExpAsLoc*/ true, &designatedDiag, context);

        if (!eNCallable)
            return Error(move(eNCallable));

        // TODO: Lambda말고 func<>도 있다
        auto* rCallableType = context.GetType(*eNCallable);
        auto* rLambdaType = dynamic_cast<RType_Lambda*>(rCallableType);

        if (!rLambdaType)
        {
            // FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);             
            return Error<Error_CallExp_CallableExpressionIsNotCallable>(); // sCallable
        }

        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
        // args는 params를 지원 할 수 있음

        // partially bound된 파라미터
        auto rParams = rLambdaType->GetPartiallyBoundParameters();

        // 
        auto match = MatchArguments(rLambdaType->outerTypeArgs, /*partialTypeArgs*/ {}, move(rParams), /*bVariadic*/false, sArgs);

        if (match)
        {
            return Exp<MExp_CallLambda>(rLambdaType->decl, match->typeArgs, *eNCallable, match->args);
        }
        else
        {
            return Error<Error_Parameter_MismatchBetweenParamCountAndArgCount>();
        }
    }

public:
    ResultType Visit(ImExp_Namespace* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_GlobalFuncs* imExp)
    {
        auto match = MatchFunc(imExp->items, sArgs, context);
        if (!match)
        {
            throw NotImplementedException{};
        }

        return Exp<MExp_CallGlobalFunc>(match->funcDecl, match->typeArgs, match->args);
    }

    ResultType Visit(ImExp_TypeVar* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_Class* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_ClassFuncs* imExp)
    {
        auto match = MatchFunc(imExp->items, sArgs, context);
        if (!match)
        {
            throw NotImplementedException{};
        }

        if (imExp->hasExplicitInstance) // x.F, C.F 등 인스턴스 부분이 명시적으로 정해졌다면
        {
            // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
            if (match->funcDecl->IsStatic() && imExp->explicitInstance != nullptr)
            {
                return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();
            }

            // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
            if (!match->funcDecl->IsStatic() && imExp->explicitInstance == nullptr)
            {
                return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
            }

            // ResolvedExp -> RExp
            MLoc* nInst = nullptr;
            if (imExp->explicitInstance)
            {
                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
                auto eNLoc = TranslateReExpToMLoc(imExp->explicitInstance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
                if (!eNLoc) return Error(move(eNLoc));

                nInst = *eNLoc;
            }

            return Exp<MExp_CallClassFunc>(match->funcDecl, match->typeArgs, nInst, move(match->args));
        }
        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
        {
            if (match->funcDecl->IsStatic()) // 정적함수이면 인스턴스에 null
            {
                return Exp<MExp_CallClassFunc>(match->funcDecl, match->typeArgs, nullptr, move(match->args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                return Exp<MExp_CallClassFunc>(match->funcDecl, match->typeArgs, context.MakeThisLoc(), move(match->args));
            }
        }

        //if (func.IsSequence)
        //{
        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
        //    return new IntermediateExp.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
        //}
        //else
        //{
        //    return new IntermediateExp.Exp(new R.CallClassMemberFuncExp(func, funcs.Instance, args));
        //}
    }

    ResultType Visit(ImExp_Struct* imExp)
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
        std::vector<DeclWithOuterTypeArgs<RStructCtorDecl>> items;
        for (auto* ctor : imExp->structDecl->GetUnboundCtors())
        {
            items.emplace_back(ctor, imExp->typeArgs);
        }

        auto match = MatchFunc(items, sArgs, context);
        if (!match)
        {
            // 매치에 실패했습니다. 에러
            throw NotImplementedException{};
            // *result = nullptr;
            // return Error(MakePtr<>());
        }

        return Exp<MExp_NewStruct>(match->funcDecl, match->typeArgs, move(match->args));
    }

    ResultType Visit(ImExp_StructFuncs* imExp)
    {
        auto match = MatchFunc(imExp->items, sArgs, context);
        if (!match)
        {
            // 매치에 실패했습니다.
            throw NotImplementedException{};
            // return Error();
        }

        // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
        if (imExp->hasExplicitInstance)
        {
            // static this 체크
            if (match->funcDecl->IsStatic() && imExp->explicitInstance)
            {
                return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();
            }

            // 반대의 경우도 체크
            if (!match->funcDecl->IsStatic() && !imExp->explicitInstance)
            {
                return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
            }

            MLoc* instance = nullptr;
            if (imExp->explicitInstance)
            {
                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
                auto eInstance = TranslateReExpToMLoc(imExp->explicitInstance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
                if (!eInstance) return Error(move(eInstance));

                instance = *eInstance;
            }

            return Exp<MExp_CallStructFunc>(match->funcDecl, match->typeArgs, instance, move(match->args));
        }
        else
        {
            if (match->funcDecl->IsStatic()) // 정적함수이면 인스턴스에 null
            {
                return Exp<MExp_CallStructFunc>(match->funcDecl, match->typeArgs, nullptr, move(match->args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                return Exp<MExp_CallStructFunc>(match->funcDecl, match->typeArgs, context.MakeThisLoc(), move(match->args));
            }
        }

        //if (func.IsSequence)
        //{
        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
        //    return new IntermediateExp.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
        //}
        //else
        //{
        //    return new IntermediateExp.Exp(new R.CallStructMemberFuncExp(func, funcs.ExplicitInstance, args));
        //}
    }

    ResultType Visit(ImExp_Enum* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_EnumElem* imExp)
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        if (imExp->decl->IsStandalone())
        {
            return Error<Error_CallExp_CallableExpressionIsNotCallable>();
        }

        auto parameters = imExp->decl->GetUnboundCtorParams();

        // EnumElem은 variadic도, typeArgs도 지원하지 않는다
        // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
        // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에(?) 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
        auto match = MatchArguments(imExp->typeArgs, /*partialTypeArgsExceptOuter*/ {}, move(parameters), /*bVariadic*/ false, sArgs);

        if (!match)
        {
            return Error<Error_Parameter_MismatchBetweenParamCountAndArgCount>();
        }

        return Exp<MExp_NewEnumElem>(imExp->decl, match->typeArgs, move(match->args));
    }

    ResultType Visit(ImExp_ThisVar* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_LocalVar* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_LambdaVar* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_ClassVar* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_StructVar* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_EnumElemVar* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_ListIndexer* imExp)
    {
        // l[0]
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_LocalDeref* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_BoxDeref* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_Else* imExp)
    {
        return HandleLoc(imExp);
    }

    /*TranslationResult<IR0ExpResult> FatalCallable(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForCallableErrorReport);
        return TranslationResult.Error<IR0ExpResult>();
    }

    TranslationResult<IR0ExpResult> FatalCallExp(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForCallExpErrorReport);
        return TranslationResult.Error<IR0ExpResult>();
    }*/
};

} // namespace

expected<MExp*, DiagPtr> TranslateImCallableAndSArgsToMExp(ImExp* imCallable, SExp* sCallable, SArguments* sArgs, TranslationContext& context)
{
    // 여기서 분석해야 할 것은 
    // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
    // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
    // 3. 잘 들어갔다면 리턴타입 -> 완료

    // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
    // 함수 이름을 먼저 찾는가
    // Argument 타입을 먼저 알아내야 하는가
    // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다
    ImCallableAndSArgsToMExpTranslator binder{sCallable, sArgs, context};
    return Accept(binder, imCallable);
}

} // namespace Citron::SyntaxIR0Translator