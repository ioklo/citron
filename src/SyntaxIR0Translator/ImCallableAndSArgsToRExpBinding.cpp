#include "pch.h"
#include "ImCallableAndSArgsToRExpBinding.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Syntax/Syntax.h>
#include <Logging/Logger.h>

#include <IR0/RExp.h>
#include <IR0/RClassMemberFuncDecl.h>

#include <IR0/RStructDecl.h>
#include <IR0/RStructMemberFuncDecl.h>

#include <IR0/REnumElemDecl.h>

#include "ScopeContext.h"

#include "ImExp.h"
#include "ReExp.h"

#include "ImExpToReExpTranslation.h"
#include "ReExpToRLocTranslation.h"

#include "NotLocationErrorLogger.h"
#include "FuncMatching.h"

namespace Citron::SyntaxIR0Translator {

//interface IFuncs<TFuncDeclSymbol, TFuncSymbol>
//where TFuncDeclSymbol : IFuncDeclSymbol
//where TFuncSymbol : IFuncSymbol
//{
//    int GetCount();
//    TFuncDeclSymbol GetDecl(int i);
//
//    TypeEnv GetOuterTypeEnv(int i);
//    ImmutableArray<IType> GetPartialTypeArgs();
//    TFuncSymbol MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context);
//};

// (IntermediateExp, Args) -> TranslationResult<IR0ExpResult>
class ImCallableAndSArgsToRExpBinder : public ImExpVisitor
{
    SExpPtr sCallable;
    SArgumentsPtr sArgs;
    RExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

    // S.ISyntaxNode nodeForCallExpErrorReport;
    // S.ISyntaxNode nodeForCallableErrorReport;

public:
    ImCallableAndSArgsToRExpBinder(const SExpPtr& sCallable, const SArgumentsPtr& sArgs, RExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : sCallable(sCallable), sArgs(sArgs), result(result), context(context), logger(logger), factory(factory)
    {
    }

private:
    // CallExp 분석에서 Callable이 Lambda, func<>로 계산되는 경우
    void HandleLoc(ImExp& imExp)
    {
        auto reExp = TranslateImExpToReExp(imExp, context, logger);
        if (!reExp)
        {
            *result = nullptr;
            return;
        }

        NotLocationErrorLogger notLocationErrorLogger(logger, &Logger::Fatal_CallableExpressionIsNotCallable);
        auto callableLoc = TranslateReExpToRLoc(*reExp, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);

        if (!callableLoc)
        {
            *result = nullptr;
            return;
        }

        // TODO: Lambda말고 func<>도 있다
        auto callableType = callableLoc->GetType(factory);
        auto lambdaType = dynamic_cast<RType_Lambda*>(callableType.get());

        if (!lambdaType)
        {
            // FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable); 
            logger->Fatal_CallableExpressionIsNotCallable(); // sCallable
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
            *result = MakePtr<RExp_CallLambda>(lambdaType->decl, match->typeArgs, callableLoc, match->args);
        }
        else
        {
            logger->Fatal_MismatchBetweenParamCountAndArgCount();
            *result = nullptr;
        }
    }

public:
    void Visit(ImExp_Namespace& imExp) override 
    {
        logger->Fatal_CallableExpressionIsNotCallable();
        *result = nullptr;
    }

    void Visit(ImExp_GlobalFuncs& imExp) override 
    {
        auto match = MatchFunc(imExp.items, sArgs, context);
        if (!match)
        {   
            throw NotImplementedException();
        }

        *result = MakePtr<RExp_CallGlobalFunc>(match->funcDecl, match->typeArgs, match->args);
    }

    void Visit(ImExp_TypeVar& imExp) override 
    {
        logger->Fatal_CallableExpressionIsNotCallable();
        *result = nullptr;
    }

    void Visit(ImExp_Class& imExp) override 
    {
        logger->Fatal_CallableExpressionIsNotCallable();
        *result = nullptr;
    }

    void Visit(ImExp_ClassMemberFuncs& imExp) override 
    {
        auto match = MatchFunc(imExp.items, sArgs, context);
        if (!match)
        {
            throw NotImplementedException();
        }

        if (imExp.hasExplicitInstance) // x.F, C.F 등 인스턴스 부분이 명시적으로 정해졌다면
        {
            // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
            if (match->funcDecl->bStatic && imExp.explicitInstance != nullptr)
            {
                logger->Fatal_CantGetStaticMemberThroughInstance();
                *result = nullptr;
                return;
            }

            // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
            if (!match->funcDecl->bStatic && imExp.explicitInstance == nullptr)
            {
                logger->Fatal_CantGetInstanceMemberThroughType();
                *result = nullptr;
                return;
            }

            // ResolvedExp -> RExp
            RLocPtr instance;

            if (imExp.explicitInstance)
            {
                NotLocationErrorLogger notLocationErrorLogger(logger, &Logger::Fatal_ExpressionIsNotLocation);

                instance = TranslateReExpToRLoc(*imExp.explicitInstance, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
                if (!instance)
                {
                    *result = nullptr;
                    return;
                }
            }

            *result = MakePtr<RExp_CallClassMemberFunc>(std::move(match->funcDecl), std::move(match->typeArgs), std::move(instance), std::move(match->args));
        }
        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
        {
            if (match->funcDecl->bStatic) // 정적함수이면 인스턴스에 null
            {
                *result = MakePtr<RExp_CallClassMemberFunc>(std::move(match->funcDecl), std::move(match->typeArgs), nullptr, std::move(match->args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                *result = MakePtr<RExp_CallClassMemberFunc>(std::move(match->funcDecl), std::move(match->typeArgs), context->MakeThisLoc(factory), std::move(match->args));
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
        std::vector<RDeclWithOuterTypeArgs<RStructConstructorDecl>> items;
        for(auto& constructorDecl : imExp.structDecl->GetConstructorDecls())
        {
            items.emplace_back(constructorDecl, imExp.typeArgs);
        }

        auto match = MatchFunc(items, sArgs, context);
        if (!match)
        {
            // 매치에 실패했습니다. 에러
            throw NotImplementedException(); 
            *result = nullptr;
            return;
        }

        *result = MakePtr<RExp_NewStruct>(match->funcDecl, std::move(match->typeArgs), std::move(match->args));
    }

    void Visit(ImExp_StructMemberFuncs& imExp) override 
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
            if (match->funcDecl->bStatic && imExp.explicitInstance)
            {
                logger->Fatal_CantGetStaticMemberThroughInstance();
                *result = nullptr;
                return;
            }

            // 반대의 경우도 체크
            if (!match->funcDecl->bStatic && !imExp.explicitInstance)
            {
                logger->Fatal_CantGetInstanceMemberThroughType();
                *result = nullptr;
                return;
            }

            RLocPtr instance;
            if (imExp.explicitInstance)
            {
                NotLocationErrorLogger notLocationErrorLogger(logger, &Logger::Fatal_ExpressionIsNotLocation);
                instance = TranslateReExpToRLoc(*imExp.explicitInstance, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
                if (!instance)
                {
                    *result = nullptr;
                    return;
                }
            }

            *result = MakePtr<RExp_CallStructMemberFunc>(std::move(match->funcDecl), std::move(match->typeArgs), std::move(instance), std::move(match->args));
        }
        else
        {
            if (match->funcDecl->bStatic) // 정적함수이면 인스턴스에 null
            {
                *result = MakePtr<RExp_CallStructMemberFunc>(std::move(match->funcDecl), std::move(match->typeArgs), nullptr, std::move(match->args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                *result = MakePtr<RExp_CallStructMemberFunc>(std::move(match->funcDecl), std::move(match->typeArgs), context->MakeThisLoc(factory), std::move(match->args));
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
        logger->Fatal_CallableExpressionIsNotCallable();
        *result = nullptr;
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        if (imExp.decl->IsStandalone())
        {
            logger->Fatal_CallableExpressionIsNotCallable();
            *result = nullptr;
            return;
        }


        auto parameters = imExp.decl->GetUnboundConstructorParams();

        // EnumElem은 variadic도, typeArgs도 지원하지 않는다
        // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
        // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에(?) 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
        auto match = MatchArguments(imExp.typeArgs, /*partialTypeArgsExceptOuter*/ {}, std::move(parameters), /*bVariadic*/ false, sArgs);

        if (!match)
        {
            logger->Fatal_MismatchBetweenParamCountAndArgCount();
            *result = nullptr;
            return;
        }

        *result = MakePtr<RExp_NewEnumElem>(imExp.decl, std::move(match->typeArgs), std::move(match->args));
    }

    void Visit(ImExp_ThisVar& imExp) override 
    {
        logger->Fatal_CallableExpressionIsNotCallable();
        *result = nullptr;
    }

    void Visit(ImExp_LocalVar& imExp) override 
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_LambdaMemberVar& imExp) override 
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_ClassMemberVar& imExp) override 
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_StructMemberVar& imExp) override 
    {
        return HandleLoc(imExp);
    }

    void Visit(ImExp_EnumElemMemberVar& imExp) override 
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

RExpPtr BindImCallableAndSArgsToRExp(ImExp& imCallable, const SExpPtr& sCallable, const SArgumentsPtr& sArgs, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    // 여기서 분석해야 할 것은 
    // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
    // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
    // 3. 잘 들어갔다면 리턴타입 -> 완료

    // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
    // 함수 이름을 먼저 찾는가
    // Argument 타입을 먼저 알아내야 하는가
    // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다

    RExpPtr rExp;
    ImCallableAndSArgsToRExpBinder binder(sCallable, sArgs, &rExp, context, logger, factory);
    imCallable.Accept(binder);
    return rExp;
}

} // namespace Citron::SyntaxIR0Translator