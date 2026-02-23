#include "ImCallableAndSArgsToMExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

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
#include "RSymbol/REnumElemVarDecl.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "TranslationContexts.h"
#include "ImExp.h"
#include "DesignatedDiagnostic.h"
#include "ImExpToReExpTranslation.h"
#include "ReExpToMLocTranslation.h"
#include "FuncMatching.h"
#include "FuncContext.h"


using namespace std;

namespace Citron {

namespace {
// (IntermediateExp, Args) -> TranslationResult<IR0ExpResult>
class ImCallableAndSArgsToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;

private:
    SExp* sCallable;
    SArguments* sArgs;

    TranslationContexts& contexts;

    // S.ISyntaxNode nodeForCallExpErrorReport;
    // S.ISyntaxNode nodeForCallableErrorReport;

public:
    ImCallableAndSArgsToMExpTranslator(SExp* sCallable, SArguments* sArgs, TranslationContexts& contexts)
        : sCallable{sCallable}, sArgs{sArgs}, contexts{contexts}
    {
    }

private:
    template<typename TMExp, typename... TArgs> requires std::derived_from<TMExp, MExp>
    ResultType Exp(TArgs&&... args)
    {
        return contexts.mFactory->MakeMExp<TMExp>(std::forward<TArgs>(args)...);
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
        auto e_reExp = TranslateImExpToReExp(imExp, contexts);
        if (!e_reExp)
            return Error(move(e_reExp));

        DesignatedDiagnostic<Error_CallExp_CallableExpressionIsNotCallable> designatedDiag;
        auto e_mCallable = TranslateReExpToMLoc(*e_reExp, /*bWrapExpAsLoc*/true, &designatedDiag, contexts);

        if (!e_mCallable)
            return Error(move(e_mCallable));

        // TODO: Lambda말고 func<>도 있다
        auto* rCallableType = (*e_mCallable)->GetType();
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

        throw NotImplementedException{};

        // 
        //auto e_o_match = MatchArguments(rLambdaType, rLambdaType->outerTypeArgs, /*partialTypeArgsExceptOuter*/{}, sArgs, contexts);
        //RETURN_ON_ERROR(e_o_match);

        //if (*e_o_match)
        //{
        //    auto& match = **e_o_match;
        //    return Exp<MExp_CallLambda>(rLambdaType->decl, match.typeArgs, *e_mCallable, match.args);
        //}
        //else
        //{
        //    return Error<Error_FuncMatch_MismatchBetweenParamCountAndArgCount>();
        //}
    }

public:
    ResultType Visit(ImExp_Namespace* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_GlobalFuncs* imExp)
    {
        auto e_o_Match = MatchFunc<RGlobalFuncDecl>(imExp->items, imExp->partialTypeArgsExceptOuter, sArgs, contexts);
        RETURN_ON_ERROR(e_o_Match);

        auto& oMatch = *e_o_Match;

        if (!oMatch)
        {
            throw NotImplementedException{};
        }

        auto& match = *oMatch;

        return Exp<MExp_CallGlobalFunc>(match.funcDecl, match.typeArgs, match.args);
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
        auto e_o_match = MatchFunc<RClassFuncDecl>(imExp->items, imExp->partialTypeArgsExceptOuter, sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        auto& oMatch = *e_o_match;
        if (!oMatch)
        {
            throw NotImplementedException{};
        }

        auto& match = *oMatch;

        if (imExp->hasExplicitInstance) // x.F, C.F 등 인스턴스 부분이 명시적으로 정해졌다면
        {
            // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
            if (match.funcDecl->GetThisKind() == RThisKind::None && imExp->explicitInstance != nullptr)
            {
                return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();
            }

            // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
            if (match.funcDecl->GetThisKind() == RThisKind::Handle && imExp->explicitInstance == nullptr)
            {
                return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
            }

            // ResolvedExp -> RExp
            MLoc* nInst = nullptr;
            if (imExp->explicitInstance)
            {
                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
                auto e_nLoc = TranslateReExpToMLoc(imExp->explicitInstance, /*bWrapExpAsLoc*/true, &designatedDiag, contexts);
                if (!e_nLoc) return Error(move(e_nLoc));

                nInst = *e_nLoc;
            }

            return Exp<MExp_CallClassFunc>(match.funcDecl, match.typeArgs, nInst, move(match.args));
        }
        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
        {
            if (match.funcDecl->GetThisKind() == RThisKind::None) // 정적함수이면 인스턴스에 null
            {
                return Exp<MExp_CallClassFunc>(match.funcDecl, match.typeArgs, nullptr, move(match.args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                return Exp<MExp_CallClassFunc>(match.funcDecl, match.typeArgs, contexts.funcContext->MakeThisLoc(), move(match.args));
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

        auto e_o_match = MatchFunc<RStructCtorDecl>(items, /*partialTypeArgsExceptOuter*/contexts.rFactory->MakeTypeArguments({}), sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        auto& oMatch = *e_o_match;
        if (!oMatch)
        {
            // 매치에 실패했습니다. 에러
            throw NotImplementedException{};
            // *result = nullptr;
            // return Error(MakePtr<>());
        }

        auto& match = *oMatch;
        return Exp<MExp_NewStruct>(match.funcDecl, match.typeArgs, move(match.args), contexts.rFactory);
    }

    ResultType Visit(ImExp_StructFuncs* imExp)
    {
        auto e_o_match = MatchFunc<RStructFuncDecl>(imExp->items, imExp->partialTypeArgsExceptOuter, sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        auto& oMatch = *e_o_match;
        if (!oMatch)
        {
            // 매치에 실패했습니다.
            throw NotImplementedException{};
            // return Error();
        }

        auto& match = *oMatch;
        // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
        if (imExp->hasExplicitInstance)
        {
            // static this 체크
            if (match.funcDecl->GetThisKind() == RThisKind::None && imExp->explicitInstance)
            {
                return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();
            }

            // 반대의 경우도 체크
            if (match.funcDecl->GetThisKind() == RThisKind::Ptr && !imExp->explicitInstance)
            {
                return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
            }

            MLoc* instance = nullptr;
            if (imExp->explicitInstance)
            {
                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
                auto e_instance = TranslateReExpToMLoc(imExp->explicitInstance, /*bWrapExpAsLoc*/true, &designatedDiag, contexts);
                if (!e_instance) return Error(move(e_instance));

                instance = *e_instance;
            }

            return Exp<MExp_CallStructFunc>(match.funcDecl, match.typeArgs, instance, move(match.args));
        }
        else
        {
            if (match.funcDecl->GetThisKind() == RThisKind::None) // 정적함수이면 인스턴스에 null
            {
                return Exp<MExp_CallStructFunc>(match.funcDecl, match.typeArgs, nullptr, move(match.args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                return Exp<MExp_CallStructFunc>(match.funcDecl, match.typeArgs, contexts.funcContext->MakeThisLoc(), move(match.args));
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

    class EnumElemMatchArgumentsInput : public IMatchArgumentsInput
    {
        REnumElemDecl* enumElemDecl;
        
    public:
        EnumElemMatchArgumentsInput(REnumElemDecl* enumElemDecl)
            : enumElemDecl{enumElemDecl}
        {   
        }

        // from IMatchArgumentsInput
        size_t GetTypeParamCount() override { return 0; }
        RTypeParamDecl* GetTypeParam(size_t index) override { return nullptr; }
        size_t GetFuncParamCount() override { return enumElemDecl->GetVarCount(); }
        RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) override
        {
            auto* varDecl = enumElemDecl->GetVarDecl(index);
            auto* declType = varDecl->GetDeclType(*typeArgs);
            
            return RFuncParameter{.kind = RFuncParameterKind::Init, .type = declType, .name = varDecl->GetIdentifier().name };
        }
    };

    ResultType Visit(ImExp_EnumElem* imExp)
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        if (imExp->decl->IsStandalone())
        {
            return Error<Error_CallExp_CallableExpressionIsNotCallable>();
        }

        // EnumElem은 variadic도, typeArgs도 지원하지 않는다
        // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
        // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에(?) 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
        EnumElemMatchArgumentsInput input{imExp->decl};
        auto e_o_match = MatchArguments(&input, imExp->typeArgs, /*partialTypeArgsExceptOuter*/contexts.rFactory->MakeTypeArguments({}), sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        if (!*e_o_match)
            return Error<Error_FuncMatch_NotFound>();

        auto& match = **e_o_match;
        return Exp<MExp_NewEnumElem>(imExp->decl, match.typeArgs, move(match.args), contexts.rFactory);
    }

    ResultType Visit(ImExp_ThisVar* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_LocalVar* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_LocalRef* imExp)
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

    ResultType Visit(ImExp_PtrDeref* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_SharedDeref* imExp)
    {
        return HandleLoc(imExp);
    }

    ResultType Visit(ImExp_Else* imExp)
    {
        return HandleLoc(imExp);
    }
};

} // namespace

expected<MExp*, DiagPtr> TranslateImCallableAndSArgsToMExp(ImExp* imCallable, SExp* sCallable, SArguments* sArgs, TranslationContexts& contexts)
{
    // 여기서 분석해야 할 것은 
    // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
    // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
    // 3. 잘 들어갔다면 리턴타입 -> 완료

    // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
    // 함수 이름을 먼저 찾는가
    // Argument 타입을 먼저 알아내야 하는가
    // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다
    ImCallableAndSArgsToMExpTranslator binder{sCallable, sArgs, contexts};
    return Accept(binder, imCallable);
}

} // namespace Citron