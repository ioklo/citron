#include "pch.h"
#include "ImCallableAndSArgsToNExpTranslation.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Syntax/Syntax.h>
#include <Logging/Logger.h>

#include <IR0/NExp.h>
#include <IR0/NClassFuncDecl.h>

#include <IR0/NStructDecl.h>
#include <IR0/NStructFuncDecl.h>

#include <IR0/NEnumElemDecl.h>

#include "TranslationContext.h"
#include "ScopeContext.h"

#include "ImExp.h"
#include "ReExp.h"

#include "ImExpToReExpTranslation.h"
#include "ReExpToNLocTranslation.h"

#include "DesignatedErrorLogger.h"
#include "FuncMatching.h"

namespace Citron::SyntaxIR0Translator {

namespace {
// (IntermediateExp, Args) -> TranslationResult<IR0ExpResult>
class ImCallableAndSArgsToNExpTranslator : public ImExpVisitor
{
    SExpPtr sCallable;
    SArgumentsPtr sArgs;
    NExpPtr* result;

    TranslationContext& context;

    // S.ISyntaxNode nodeForCallExpErrorReport;
    // S.ISyntaxNode nodeForCallableErrorReport;

public:
    ImCallableAndSArgsToNExpTranslator(const SExpPtr& sCallable, const SArgumentsPtr& sArgs, NExpPtr* result, TranslationContext& context)
        : sCallable(sCallable), sArgs(sArgs), result(result), context(context)
    {
    }

private:
    // CallExp 분석에서 Callable이 Lambda, func<>로 계산되는 경우
    void HandleLoc(ImExp& imExp)
    {
        auto reExp = TranslateImExpToReExp(imExp, context);
        if (!reExp)
        {
            *result = nullptr;
            return;
        }

        auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable);
        auto callableLoc = TranslateReExpToNLoc(*reExp, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);

        if (!callableLoc)
        {
            *result = nullptr;
            return;
        }

        // TODO: Lambda말고 func<>도 있다
        auto callableType = context.GetType(*callableLoc);
        auto lambdaType = dynamic_cast<RType_Lambda*>(callableType.get());

        if (!lambdaType)
        {
            // FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable); 
            context.Log(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable); // sCallable
            *result = nullptr;
            return;
        }

        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
        // args는 params를 지원 할 수 있음

        // partially bound된 파라미터
        auto parameters = lambdaType->GetPartiallyBoundParameters();

        // 
        auto match = MatchArguments(lambdaType->outerTypeArgs, /*partialTypeArgs*/ {}, std::move(parameters), /*bVariadic*/false, sArgs);

        if (match)
        {
            *result = MakePtr<NExp_CallLambda>(lambdaType->decl, match->typeArgs, callableLoc, match->args);
        }
        else
        {
            context.Log(&Logger::Fatal_Parameter_MismatchBetweenParamCountAndArgCount);
            *result = nullptr;
        }
    }

public:
    void Visit(ImExp_Namespace& imExp) override
    {
        context.Log(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable);
        *result = nullptr;
    }

    void Visit(ImExp_GlobalFuncs& imExp) override
    {
        auto match = MatchFunc(imExp.items, sArgs, context);
        if (!match)
        {
            throw NotImplementedException();
        }

        *result = MakePtr<NExp_CallGlobalFunc>(match->funcDecl, match->typeArgs, match->args);
    }

    void Visit(ImExp_TypeVar& imExp) override
    {
        context.Log(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable);
        *result = nullptr;
    }

    void Visit(ImExp_Class& imExp) override
    {
        context.Log(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable);
        *result = nullptr;
    }

    void Visit(ImExp_ClassFuncs& imExp) override
    {
        auto match = MatchFunc(imExp.items, sArgs, context);
        if (!match)
        {
            throw NotImplementedException();
        }

        if (imExp.hasExplicitInstance) // x.F, C.F 등 인스턴스 부분이 명시적으로 정해졌다면
        {
            // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
            if (match->funcDecl->IsStatic() && imExp.explicitInstance != nullptr)
            {
                context.Log(&Logger::Fatal_ResolveIdentifier_CantGetStaticMemberThroughInstance);
                *result = nullptr;
                return;
            }

            // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
            if (!match->funcDecl->IsStatic() && imExp.explicitInstance == nullptr)
            {
                context.Log(&Logger::Fatal_ResolveIdentifier_CantGetInstanceMemberThroughType);
                *result = nullptr;
                return;
            }

            // ResolvedExp -> RExp
            NLocPtr instance;

            if (imExp.explicitInstance)
            {
                auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

                instance = TranslateReExpToNLoc(*imExp.explicitInstance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
                if (!instance)
                {
                    *result = nullptr;
                    return;
                }
            }

            *result = MakePtr<NExp_CallClassFunc>(std::move(match->funcDecl), std::move(match->typeArgs), std::move(instance), std::move(match->args));
        }
        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
        {
            if (match->funcDecl->IsStatic()) // 정적함수이면 인스턴스에 null
            {
                *result = MakePtr<NExp_CallClassFunc>(std::move(match->funcDecl), std::move(match->typeArgs), nullptr, std::move(match->args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                *result = MakePtr<NExp_CallClassFunc>(std::move(match->funcDecl), std::move(match->typeArgs), context.MakeThisLoc(), std::move(match->args));
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

    void Visit(ImExp_Struct& imExp) override
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
        std::vector<DeclWithOuterTypeArgs<RStructCtorDecl>> items;
        for (auto& ctor : imExp.structDecl->GetUnboundCtors())
        {
            items.emplace_back(ctor, imExp.typeArgs);
        }

        auto match = MatchFunc(items, sArgs, context);
        if (!match)
        {
            // 매치에 실패했습니다. 에러
            throw NotImplementedException();
            *result = nullptr;
            return;
        }

        *result = MakePtr<NExp_NewStruct>(match->funcDecl, std::move(match->typeArgs), std::move(match->args));
    }

    void Visit(ImExp_StructFuncs& imExp) override
    {
        auto match = MatchFunc(imExp.items, sArgs, context);
        if (!match)
        {
            // 매치에 실패했습니다.
            throw NotImplementedException();
            *result = nullptr;
            return;
        }

        // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
        if (imExp.hasExplicitInstance)
        {
            // static this 체크
            if (match->funcDecl->IsStatic() && imExp.explicitInstance)
            {
                context.Log(&Logger::Fatal_ResolveIdentifier_CantGetStaticMemberThroughInstance);
                *result = nullptr;
                return;
            }

            // 반대의 경우도 체크
            if (!match->funcDecl->IsStatic() && !imExp.explicitInstance)
            {
                context.Log(&Logger::Fatal_ResolveIdentifier_CantGetInstanceMemberThroughType);
                *result = nullptr;
                return;
            }

            NLocPtr instance;
            if (imExp.explicitInstance)
            {
                auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);
                instance = TranslateReExpToNLoc(*imExp.explicitInstance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
                if (!instance)
                {
                    *result = nullptr;
                    return;
                }
            }

            *result = MakePtr<NExp_CallStructFunc>(std::move(match->funcDecl), std::move(match->typeArgs), std::move(instance), std::move(match->args));
        }
        else
        {
            if (match->funcDecl->IsStatic()) // 정적함수이면 인스턴스에 null
            {
                *result = MakePtr<NExp_CallStructFunc>(std::move(match->funcDecl), std::move(match->typeArgs), nullptr, std::move(match->args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                *result = MakePtr<NExp_CallStructFunc>(std::move(match->funcDecl), std::move(match->typeArgs), context.MakeThisLoc(), std::move(match->args));
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

    void Visit(ImExp_Enum& imExp) override
    {
        context.Log(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable);
        *result = nullptr;
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        if (imExp.decl->IsStandalone())
        {
            context.Log(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable);
            *result = nullptr;
            return;
        }

        auto parameters = imExp.decl->GetUnboundCtorParams();

        // EnumElem은 variadic도, typeArgs도 지원하지 않는다
        // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
        // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에(?) 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
        auto match = MatchArguments(imExp.typeArgs, /*partialTypeArgsExceptOuter*/ {}, std::move(parameters), /*bVariadic*/ false, sArgs);

        if (!match)
        {
            context.Log(&Logger::Fatal_Parameter_MismatchBetweenParamCountAndArgCount);
            *result = nullptr;
            return;
        }

        *result = MakePtr<NExp_NewEnumElem>(imExp.decl, std::move(match->typeArgs), std::move(match->args));
    }

    void Visit(ImExp_ThisVar& imExp) override
    {
        context.Log(&Logger::Fatal_CallExp_CallableExpressionIsNotCallable);
        *result = nullptr;
    }

    void Visit(ImExp_LocalVar& imExp) override
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_LambdaVar& imExp) override
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_ClassVar& imExp) override
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_StructVar& imExp) override
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_EnumElemVar& imExp) override
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_ListIndexer& imExp) override
    {
        // l[0]
        return HandleLoc(imExp);
    }

    void Visit(ImExp_LocalDeref& imExp) override
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_BoxDeref& imExp) override
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_Else& imExp) override
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

NExpPtr TranslateImCallableAndSArgsToNExp(ImExp& imCallable, const SExpPtr& sCallable, const SArgumentsPtr& sArgs, TranslationContext& context)
{
    // 여기서 분석해야 할 것은 
    // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
    // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
    // 3. 잘 들어갔다면 리턴타입 -> 완료

    // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
    // 함수 이름을 먼저 찾는가
    // Argument 타입을 먼저 알아내야 하는가
    // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다

    NExpPtr nExp;
    ImCallableAndSArgsToNExpTranslator binder(sCallable, sArgs, &nExp, context);
    imCallable.Accept(binder);
    return nExp;
}

} // namespace Citron::SyntaxIR0Translator