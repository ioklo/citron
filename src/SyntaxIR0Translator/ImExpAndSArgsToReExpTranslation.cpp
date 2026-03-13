#include "ImExpAndSArgsToReExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"
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
#include "MIR/MInitExp.h"
#include "MIR/MStmt.h"
#include "MIR/MFactory.h"

#include "TranslationContexts.h"
#include "ImExp.h"
#include "ReExp.h"
#include "DesignatedDiagnostic.h"
#include "ImExpToReExpTranslation.h"
#include "ReExpToMIRTranslation.h"
#include "FuncMatching.h"
#include "FuncContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

namespace {
// (IntermediateExp, Args) -> TranslationResult<IR0ExpResult>
struct ImCallableAndSArgsToReExpTranslator
{
    using ResultType = expected<ReExp, DiagPtr>;
    SArguments* sArgs;
    TranslationContexts& contexts;

private:
    template<typename TMExp, typename... TArgs> requires std::derived_from<TMExp, MExp>
    ResultType Exp(TArgs&&... args)
    {
        return ReExp_Exp{contexts.mFactory->MakeMExp<TMExp>(std::forward<TArgs>(args)...)};
    }

    template<typename TMInitExp, typename... TArgs> requires std::derived_from<TMInitExp, MInitExp>
    ResultType InitExp(TArgs&&... args)
    {
        return ReExp_InitExp{contexts.mFactory->MakeMInitExp<TMInitExp>(std::forward<TArgs>(args)...)};
    }

    ResultType HandleLoc(MLoc* loc)
    {
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

    // CallExp 분석에서 Callable이 Lambda, func<>로 계산되는 경우
    ResultType HandleAsLoc(ImExp* imExp)
    {
        auto e_reExp = TranslateImExpToReExp(imExp, contexts);
        RETURN_ON_ERROR(e_reExp);

        DesignatedDiagnostic<Error_CallExp_CallableExpressionIsNotCallable> designatedDiag;
        auto e_mCallable = TranslateReExpToMLoc(*e_reExp, /*bMaterializeExp*/true, &designatedDiag, contexts);
        RETURN_ON_ERROR(e_mCallable);

        return HandleLoc(*e_mCallable);
    }

    ResultType Call(RCopyStrategy copyStrategy, MCallable&& call, vector<MArgument>&& args, std::optional<MCatch>&& o_catch)
    {
        switch (copyStrategy)
        {
        case RCopyStrategy::Void:
        {
            // TODO: [41] try catch 구현
            auto* callStmt = contexts.mFactory->MakeMStmt<MStmt_Call>(move(call), move(args), move(o_catch));
            return ReExp_StmtCall{callStmt};
        }

        case RCopyStrategy::Bitwise:
        {
            auto* callExp = contexts.mFactory->MakeMExp<MExp_Call>(move(call), move(args), move(o_catch));
            return ReExp_Exp{callExp};
        }

        case RCopyStrategy::NonBitwise:
        {
            auto* callInitExp = contexts.mFactory->MakeMInitExp<MInitExp_Call>(move(call), move(args), move(o_catch));
            return ReExp_InitExp{callInitExp};
        }
        }

        unreachable();
    }

public:
    ResultType Visit(ImExp_Namespace* imExp)
    {
        return Error<Error_CallExp_CallableExpressionIsNotCallable>();
    }

    ResultType Visit(ImExp_GlobalFuncs* imExp)
    {
        auto e_o_match = MatchFunc<RGlobalFuncDecl>(imExp->items, imExp->memberTypeArgs, sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        auto& o_match = *e_o_match;
        if (!o_match)
            throw NotImplementedException{}; // TODO: [39] SyntaxIR0Translator Eror 정리

        auto& match = *o_match;
        auto* retType = match.funcDecl->GetReturnType(match.typeArgs);

        // TODO: [41] try catch 구현
        return Call(retType->GetCopyStrategy(), MCallable_GlobalFunc{match.funcDecl, match.typeArgs}, move(match.args), /*o_catch*/nullopt);
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
        auto e_o_match = MatchFunc<RClassFuncDecl>(imExp->items, imExp->memberTypeArgs, sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        auto& o_match = *e_o_match;
        if (!o_match)
            throw NotImplementedException{}; // TODO: [39] SyntaxIR0Translator Eror 정리

        auto& match = *o_match;
        auto* retType = match.funcDecl->GetReturnType(match.typeArgs);
        auto copyStrategy = retType->GetCopyStrategy();

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

            // TODO: [41] try catch 구현
            return Call(copyStrategy, MCallable_ClassFunc{match.funcDecl, match.typeArgs, imExp->explicitInstance}, move(match.args), /*o_catch*/nullopt);
        }
        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
        {
            if (match.funcDecl->GetThisKind() == RThisKind::None) // 정적함수이면 인스턴스에 null
            {
                return Call(copyStrategy, MCallable_ClassFunc{match.funcDecl, match.typeArgs, /*instance*/nullptr}, move(match.args), /*o_catch*/nullopt);
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                return Call(copyStrategy, MCallable_ClassFunc{match.funcDecl, match.typeArgs, contexts.funcContext->MakeThisLoc()}, move(match.args), /*o_catch*/nullopt);
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

        auto e_o_match = MatchFunc<RStructCtorDecl>(items, /*memberTypeArgs*/contexts.rFactory->MakeTypeArguments({}), sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        auto& o_match = *e_o_match;
        if (!o_match)
            throw NotImplementedException{}; // TODO: [39] SyntaxIR0Translator Eror 정리

        auto& match = *o_match;
        auto* structType = contexts.rFactory->MakeStructType(imExp->structDecl, imExp->typeArgs);
        auto copyStrategy = structType->GetCopyStrategy();

        if (copyStrategy == RCopyStrategy::Bitwise) // MExp로 
        {
            return ReExp_Exp{contexts.mFactory->MakeMExp<MExp_NewStruct>(match.funcDecl, match.typeArgs, move(match.args))};
        }
        else if (copyStrategy == RCopyStrategy::NonBitwise)
        {
            auto ctorKind = match.funcDecl->GetKind();

            if (ctorKind == RStructCtorKind::Copy)
            {
                assert(match.args.size() == 1); // 한개이고
                auto& locArg = get<MArgument_Loc>(match.args[0]); // location이고

                return InitExp<MInitExp_StructCtor>(
                    MInitExp_StructCtorKind_Copy{
                        .structType = structType, 
                        .src = MRead_NBC{.loc = locArg.loc}
                    });
            }
            else if (ctorKind == RStructCtorKind::Move)
            {
                assert(match.args.size() == 1); // 한개이고
                auto& moveArg = get<MArgument_Move>(match.args[0]); // move source고,

                return InitExp<MInitExp_StructCtor>(
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
                return InitExp<MInitExp_StructCtor>(
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
        auto e_o_match = MatchFunc<RStructFuncDecl>(imExp->items, imExp->memberTypeArgs, sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        auto& oMatch = *e_o_match;
        if (!oMatch)
        {
            // 매치에 실패했습니다.
            throw NotImplementedException{};
            // return Error();
        }

        auto& match = *oMatch;
        auto* retType = match.funcDecl->GetReturnType(match.typeArgs);
        auto copyStrategy = retType->GetCopyStrategy();

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

            return Call(copyStrategy, MCallable_StructFunc{.decl = match.funcDecl, .typeArgs = match.typeArgs, .instance = imExp->explicitInstance}, move(match.args), /*o_catch*/nullopt);
        }
        else
        {
            if (match.funcDecl->GetThisKind() == RThisKind::None) // 정적함수이면 인스턴스에 null
            {
                return Call(copyStrategy, MCallable_StructFunc{.decl = match.funcDecl, .typeArgs = match.typeArgs, .instance = nullptr}, move(match.args), /*o_catch*/nullopt);
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                return Call(copyStrategy, MCallable_StructFunc{.decl = match.funcDecl, .typeArgs = match.typeArgs, .instance = contexts.funcContext->MakeThisLoc()}, move(match.args), /*o_catch*/nullopt);
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
            auto* declType = varDecl->GetDeclType(typeArgs);
            
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
        auto e_o_match = MatchArguments(&input, imExp->typeArgs, /*memberTypeArgs*/contexts.rFactory->MakeTypeArguments({}), sArgs, contexts);
        RETURN_ON_ERROR(e_o_match);

        if (!*e_o_match)
            return Error<Error_FuncMatch_NotFound>();

        auto& match = **e_o_match;
        auto* enumElemType = contexts.rFactory->MakeEnumElemType(imExp->decl, imExp->typeArgs);
        auto copyStrategy = enumElemType->GetCopyStrategy();

        switch (copyStrategy)
        {
        case RCopyStrategy::Void: throw RuntimeFatalException{};
        case RCopyStrategy::Bitwise:
            return Exp<MExp_NewEnumElem>(imExp->decl, match.typeArgs, move(match.args));

        case RCopyStrategy::NonBitwise:
            return InitExp<MInitExp_NewEnumElem>(imExp->decl, match.typeArgs, move(match.args));
        }

        unreachable();
    }

    ResultType Visit(ImExp_ClassVar* imExp)
    {
        return HandleAsLoc(imExp);
    }

    ResultType Visit(ImExp_StructVar* imExp)
    {
        return HandleAsLoc(imExp);
    }

    ResultType Visit(ImExp_ReExp* imExp)
    {
        static_assert(false);

        ResultType Visit(ImExp_Loc * imExp)
        {
            return HandleLoc(imExp->loc);
        }

        ResultType Visit(ImExp_Exp * imExp)
        {
            return HandleAsLoc(imExp);
        }
    }
};

} // namespace

expected<ReExp, DiagPtr> TranslateImExpAndSArgsToReExp(ImExp* imCallable, SArguments* sArgs, TranslationContexts& contexts)
{
    // 여기서 분석해야 할 것은 
    // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
    // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
    // 3. 잘 들어갔다면 리턴타입 -> 완료

    // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
    // 함수 이름을 먼저 찾는가
    // Argument 타입을 먼저 알아내야 하는가
    // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다
    ImCallableAndSArgsToReExpTranslator binder{sArgs, contexts};
    return Accept(binder, imCallable);
}

} // namespace Citron