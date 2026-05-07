#include "MInitExpToQInsts.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"
#include "MIR/MCallable.h"
#include "QIR/QInsts.h"
#include "MLocToQInsts.h"
#include "MStmtToQInsts.h"
#include "MqBodyContext.h"
#include "MqTranslationContexts.h"
#include "MReadToQInsts.h"
#include "CommonQInstsTranslation.h"
#include "MqEmitState.h"
#include "MqAbi.h"
#include "MqFactory.h"

using namespace std;

namespace Citron {

struct MInitExpQInstsTranslator
{
    using ResultType = expected<MqEmitState<void>, DiagPtr>;
    MqCreateTarget createTarget;
    MqTranslationContexts& contexts;

    ResultType Visit(MInitExp* initExp)
    {
        throw NotImplementedException{};
    }

    // ResultType Visit(MInitExp_Shared* mInitExp) { }
    // ResultType Visit(MInitExp_SharedRef* mInitExp) { }
    // ResultType Visit(MInitExp_Stmt* mInitExp) { }
    ResultType Visit(MInitExp_String* mInitExp) 
    { 
        return TranslateMInitExp_StringToQInsts(mInitExp, createTarget, contexts);
    }
    // ResultType Visit(MInitExp_List* mInitExp) { }
    ResultType Visit(MInitExp_CallIntrinsic* mInitExp) 
    { 
        auto& intrinsicInfo = contexts.mqFactory->GetIntrinsicInfo(mInitExp->kind);
        auto e_s_o_retLocResult = HandleIntrinsicCall(intrinsicInfo, createTarget, mInitExp->typeArgs, mInitExp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);
        return MqEmitState_Ready{};
    }
    // ResultType Visit(MInitExp_NewClass* mInitExp) { }
    ResultType Visit(MInitExp_StructCtor* mInitExp) 
    {
        // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
        auto* type = GetType(mInitExp, &*contexts.rFactory);
        if (type != contexts.bodyContext.GetStringType())
            throw NotImplementedException{};

        return visit([this](auto& kind) -> ResultType {
            using U = remove_cvref_t<decltype(kind)>;
            if constexpr (same_as<U, MInitExp_StructCtorKind_Copy>)
            {
                auto e_s_srcResult = TranslateMLocToQInsts(kind.src.loc, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_srcResult);

                if (auto o_createTargetArg = MakeAddrCallArg(createTarget, contexts))
                {
                    auto srcArg = MakeAddrCallArg(**e_s_srcResult, contexts);
                    contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::CopyCtor_Void_StringRef_StringInRef, nullopt, 
                        {*o_createTargetArg, srcArg});
                }

                return MqEmitState_Ready{};
            }
            else if constexpr (same_as<U, MInitExp_StructCtorKind_Move>)
            {
                // TODO: [30] move구현
                throw NotImplementedException{};
            }
            else if constexpr (same_as<U, MInitExp_StructCtorKind_General>)
            {
                // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
                throw NotImplementedException{};
            }
            else static_assert(false);
        }, mInitExp->kind);
    }

    ResultType Visit(MInitExp_Call* mInitExp)
    { 
        auto e_s_o_retLocResult = HandleCall(mInitExp->callable.decl, mInitExp->callable.typeArgs, createTarget, mInitExp->callable.o_instance, mInitExp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);

        return MqEmitState_Ready{};
    }

    // ResultType Visit(MInitExp_NewEnumElem* mInitExp) { }
    // ResultType Visit(MInitExp_Nullable* mInitExp) { }
    // ResultType Visit(MInitExp_NullableNullLiteral* mInitExp) { }
    // ResultType Visit(MInitExp_NullableInplaceNullLiteral* mInitExp) { }
    // ResultType Visit(MInitExp_Cast* mInitExp) { }
    // ResultType Visit(MInitExp_Lambda* mInitExp) { }
    ResultType Visit(MInitExp_InlineBlock* mInitExp) 
    { 
        return HandleInlineBlock(mInitExp->scope, createTarget, contexts);
    }
    // ResultType Visit(MInitExp_As* mInitExp) { }
};

expected<MqEmitState<void>, DiagPtr> TranslateMInitExpToQInsts(MInitExp* mInitExp, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    return Accept(MInitExpQInstsTranslator{createTarget, contexts}, mInitExp);
}

} // namespace Citron