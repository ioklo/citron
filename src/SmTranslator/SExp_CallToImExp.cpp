#include "SExp_CallToImExp.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RCopyStrategy.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "MIR/MStmt.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"
#include "MIR/MCallable.h"
#include "MIR/MCatch.h"
#include "MIR/MFactory.h"
#include "SExpToImExp.h"
#include "ImExpToReExp.h"
#include "ImExp.h"
#include "FuncMatching.h"
#include "FuncContext.h"
#include "Misc.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "ImExpTranslations.h"

using namespace std;

namespace Citron {

namespace {

struct CallableTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;
    SArguments* sArgs;
    TranslationContexts& contexts;

    template<typename TMExp, typename... TArgs> requires derived_from<TMExp, MExp>
    ImExp* MakeImExp_ReExp_Exp(TArgs&&... args)
    {
        auto* exp = contexts.mFactory->MakeMExp<TMExp>(forward<TArgs>(args)...);
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_Exp{exp});
    }

    template<typename TMInitExp, typename... TArgs> requires derived_from<TMInitExp, MInitExp>
    ImExp* MakeImExp_ReExp_InitExp(TArgs&&... args)
    {
        auto* initExp = contexts.mFactory->MakeMInitExp<TMInitExp>(forward<TArgs>(args)...);
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_InitExp{initExp});
    }

    ResultType Call(RFuncDecl* rFuncDecl, RTypeArguments* typeArgs, MLoc* o_instance, vector<MArgument>&& args, std::optional<MCatch>&& o_catch)
    {
        auto* retType = rFuncDecl->GetReturnType(typeArgs);
        auto copyStrategy = retType->GetCopyStrategy();

        switch (copyStrategy)
        {
        case RCopyStrategy::Void:
        {
            // TODO: [41] try catch 구현
            auto* callStmt = contexts.mFactory->MakeMStmt<MStmt_Call>(MTopLevel_Call{MCallable{rFuncDecl, typeArgs, o_instance}, move(args), move(o_catch)});
            return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_StmtCall{callStmt});
        }

        case RCopyStrategy::Bitwise:
        {
            auto* callExp = contexts.mFactory->MakeMExp<MExp_Call>(MCallable{rFuncDecl, typeArgs, o_instance}, move(args), move(o_catch));
            return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_Exp{callExp});
        }

        case RCopyStrategy::NonBitwise:
        {
            auto* callInitExp = contexts.mFactory->MakeMInitExp<MInitExp_Call>(MCallable{rFuncDecl, typeArgs, o_instance}, move(args), move(o_catch));
            return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_InitExp{callInitExp});
        }
        }

        unreachable();
    }

    ResultType CallReExp(ReExp& reExp)
    {
        // TODO: loc이 아닐 경우에도 가능한지 확인
        auto* loc = get<ReExp_Loc>(reExp).mLoc;

        // TODO: Lambda말고 func<>도 있다
        auto* rCallableType = GetType(loc, &*contexts.rFactory);
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
        //auto e_o_match = MatchArguments(rLambdaType, rLambdaType->outerTypeArgs, /*memberTypeArgs*/{}, sArgs, contexts);
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

    ResultType Visit(ImExp* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    // ResultType Visit(ImExp_Namespace* imExp);
    ResultType Visit(ImExp_GlobalFuncs* imExp) 
    { 
        assert(!imExp->items.empty());

        auto e_match = MatchFunc<RGlobalFuncDecl>(imExp->items, imExp->memberTypeArgs, sArgs, contexts);
        RETURN_ON_ERROR_REFDECL(e_match, match);

        // TODO: [41] try catch 구현
        return Call(match.funcDecl, match.typeArgs, /*o_instance*/nullptr, move(match.args), /*o_catch*/nullopt);
    }

    // ResultType Visit(ImExp_TypeVar* imExp);
    // ResultType Visit(ImExp_Class* imExp);
    ResultType Visit(ImExp_ClassFuncs* imExp)
    { 
        assert(imExp->items.empty());

        auto e_match = MatchFunc<RClassFuncDecl>(imExp->items, imExp->memberTypeArgs, sArgs, contexts);
        RETURN_ON_ERROR_REFDECL(e_match, match);

        return visit([&match, this](auto& instanceKind) -> ResultType
        {
            using T = remove_cvref_t<decltype(instanceKind)>;

            if constexpr (same_as<T, ImExpInstanceKind_ExplicitStatic>)
            {
                // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
                if (!match.funcDecl->IsStatic())
                    return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();

                // TODO: [41] try catch 구현
                return Call(match.funcDecl, match.typeArgs, /*instance*/nullptr, move(match.args), /*o_catch*/nullopt);
            }
            else if constexpr (same_as<T, ImExpInstanceKind_ExplicitInstance>)
            {
                // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
                if (match.funcDecl->IsStatic())
                    return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();

                return Call(match.funcDecl, match.typeArgs, /*instance*/instanceKind.mInstLoc, move(match.args), /*o_catch*/nullopt);
            }
            else if constexpr (same_as<T, ImExpInstanceKind_Implicit>) // F 로 인스턴스를 명시적으로 정하지 않았다면 
            {
                if (match.funcDecl->IsStatic()) // 정적함수이면 인스턴스에 null
                {
                    // TODO: [41] try catch 구현
                    return Call(match.funcDecl, match.typeArgs, /*instance*/nullptr, move(match.args), /*o_catch*/nullopt);
                }
                else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
                {
                    // TODO: [41] try catch 구현
                    return Call(match.funcDecl, match.typeArgs, contexts.funcContext->MakeThisLoc(), move(match.args), /*o_catch*/nullopt);
                }
            }
            else static_assert(false);

        }, imExp->instanceKind);
    }

    ResultType Visit(ImExp_Struct* imExp) 
    { 
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
        std::vector<TDeclWithOuterTypeArgs<RStructCtorDecl>> items;
        for (auto* ctor : imExp->structDecl->GetUnboundCtors())
        {
            items.emplace_back(ctor, imExp->typeArgs);
        }

        auto e_match = MatchFunc<RStructCtorDecl>(items, /*memberTypeArgs*/contexts.rFactory->MakeEmptyTypeArguments(), sArgs, contexts);
        RETURN_ON_ERROR_REFDECL(e_match, match);

        auto* structType = contexts.rFactory->MakeStructType(imExp->structDecl, imExp->typeArgs);
        auto copyStrategy = structType->GetCopyStrategy();

        if (copyStrategy == RCopyStrategy::Bitwise) // MExp로 
        {
            return MakeImExp_ReExp_Exp<MExp_NewStruct>(match.funcDecl, match.typeArgs, move(match.args));
        }
        else if (copyStrategy == RCopyStrategy::NonBitwise)
        {
            auto ctorKind = match.funcDecl->GetKind();

            if (ctorKind == RStructCtorKind::Copy)
            {
                assert(match.args.size() == 1); // 한개이고
                auto& locArg = get<MArgument_Loc>(match.args[0]); // location이고

                return MakeImExp_ReExp_InitExp<MInitExp_StructCtor>(
                    MInitExp_StructCtorKind_Copy{
                        .structType = structType,
                        .src = MRead_Loc{.loc = locArg.loc}
                    });
            }
            else if (ctorKind == RStructCtorKind::Move)
            {
                assert(match.args.size() == 1); // 한개이고
                auto& moveArg = get<MArgument_Move>(match.args[0]); // move source고,

                return MakeImExp_ReExp_InitExp<MInitExp_StructCtor>(
                    MInitExp_StructCtorKind_Move{
                        .structType = structType,
                        .src = move(moveArg.src),
                    });
            }
            else if (ctorKind == RStructCtorKind::Memberwise)
            {
                throw NotImplementedException{}; // TODO: [27] enumElemDecl에 memberwise ctor 추가하기, memberwise ctor에서 직접 대입 처리
            }
            else
            {
                return MakeImExp_ReExp_InitExp<MInitExp_StructCtor>(
                   MInitExp_StructCtorKind_General{
                        .decl = match.funcDecl,
                        .typeArgs = match.typeArgs,
                        .args = move(match.args)
                   });
            }
        }
        else
        {
            unreachable();
        }
    }

    ResultType Visit(ImExp_StructFuncs* imExp)
    { 
        auto e_match = MatchFunc<RStructFuncDecl>(imExp->items, imExp->memberTypeArgs, sArgs, contexts);
        RETURN_ON_ERROR_REFDECL(e_match, match);

        return visit([&match, this](auto& instanceKind) -> ResultType
        {
            using T = remove_cvref_t<decltype(instanceKind)>;

            if constexpr (same_as<T, ImExpInstanceKind_ExplicitStatic>)
            {
                // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
                if (!match.funcDecl->IsStatic())
                    return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();

                // TODO: [41] try catch 구현
                return Call(match.funcDecl, match.typeArgs, /*o_instance*/nullptr, move(match.args), /*o_catch*/nullopt);
            }
            else if constexpr (same_as<T, ImExpInstanceKind_ExplicitInstance>)
            {
                // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
                if (match.funcDecl->IsStatic())
                    return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();
                
                return Call(match.funcDecl, match.typeArgs, /*o_instance*/instanceKind.mInstLoc, move(match.args), /*o_catch*/nullopt);
            }
            else if constexpr (same_as<T, ImExpInstanceKind_Implicit>) // F 로 인스턴스를 명시적으로 정하지 않았다면 
            {
                if (match.funcDecl->IsStatic()) // 정적함수이면 인스턴스에 null
                {   
                    // TODO: [41] try catch 구현
                    return Call(match.funcDecl, match.typeArgs, /*o_instance*/nullptr, move(match.args), /*o_catch*/nullopt);
                }
                else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
                {
                    // TODO: [41] try catch 구현
                    return Call(match.funcDecl, match.typeArgs, /*o_instance*/contexts.funcContext->MakeThisLoc(), move(match.args), /*o_catch*/nullopt);
                }
            }
            else static_assert(false);

        }, imExp->instanceKind);
    }

    // ResultType Visit(ImExp_Enum* imExp);

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
            auto* varDecl = enumElemDecl->GetUnboundVar(index);
            auto* declType = varDecl->GetUnboundDeclType()->Apply(typeArgs);

            return RFuncParameter{.kind = RFuncParameterKind::Init, .type = declType, .name = varDecl->GetIdentifier().name};
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
        auto o_match = MatchArguments(&input, imExp->typeArgs, /*memberTypeArgs*/contexts.rFactory->MakeEmptyTypeArguments(), sArgs, contexts);
        RETURN_ON_ERROR_REFDECL(o_match, match);

        auto* enumElemType = contexts.rFactory->MakeEnumElemType(imExp->decl, imExp->typeArgs);
        auto copyStrategy = enumElemType->GetCopyStrategy();

        switch (copyStrategy)
        {
        case RCopyStrategy::Void: throw RuntimeFatalException{};
        case RCopyStrategy::Bitwise:
            return MakeImExp_ReExp_Exp<MExp_NewEnumElem>(imExp->decl, match.typeArgs, move(match.args));

        case RCopyStrategy::NonBitwise:
            return MakeImExp_ReExp_InitExp<MInitExp_NewEnumElem>(imExp->decl, match.typeArgs, move(match.args));
        }

        unreachable();
    }

    ResultType Visit(ImExp_ClassVar* imExp) 
    {
        ReExp reExp = ReExp_Loc{TranslateImExp_ClassVarToMLoc_ClassVar(imExp, contexts)};
        return CallReExp(reExp);
    }

    ResultType Visit(ImExp_StructVar* imExp) 
    { 
        ReExp reExp = ReExp_Loc{TranslateImExp_StructVarToMLoc_StructVar(imExp, contexts)};
        return CallReExp(reExp);
    }

    ResultType Visit(ImExp_ReExp* imExp) 
    {
        return CallReExp(imExp->reExp);
    }
};


} // namespace

expected<ImExp*, DiagPtr> TranslateSExp_CallToImExp(SExp_Call* sExp, TranslationContexts& contexts)
{
    auto e_imCallable = TranslateSExpToImExp(sExp->callable, /*hintType*/nullptr, contexts);
    RETURN_ON_ERROR(e_imCallable);

    return Accept(CallableTranslator{sExp->args, contexts}, *e_imCallable);
}

} // namespace Citron