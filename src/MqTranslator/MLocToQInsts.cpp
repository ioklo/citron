#include "MLocToQInsts.h"

#include <variant>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"

#include "MqBodyContext.h"
#include "MqTranslationContexts.h"
#include "MqEmitState.h"
#include "MqReadResult.h"
#include "MqCreateTarget.h"
#include "MExpToQInsts.h"
#include "MInitExpToQInsts.h"
#include "CommonQInstsTranslation.h"

using namespace std;

namespace Citron {

// MLoc이 가리키는 위치를 slot자체나, ptr를 돌려준다 (ptr이 들어간 slot을 리턴한다)
struct MLocQInstsTranslator
{
    using ResultType = expected<MqEmitState<MqLocResult>, DiagPtr>;
    MqTranslationContexts& contexts;
    
    ResultType Visit(MLoc_Materialize* loc) 
    {
        return Materialize(loc->create, contexts);
    }

    ResultType Visit(MLoc_LocalVar* loc)
    {
        auto o_localInfo = contexts.bodyContext.GetLocalInfo(loc->name);
        assert(o_localInfo);

        return visit([](auto& localInfo) -> ResultType
        {
            using T = remove_cvref_t<decltype(localInfo)>;
            if constexpr (same_as<T, MqLocalInfo_Var>)
                return MqLocResult_Slot{localInfo.slotIndex};
            else if constexpr (same_as<T, MqLocalInfo_RefAlias>)
                throw RuntimeFatalException{};
            else if constexpr (same_as<T, MqLocalInfo_RefPtr>)
                return MqLocResult_Ptr{localInfo.slotIndex};
        }, * o_localInfo);
    }

    ResultType Visit(MLoc_LocalRef* loc)
    {
        auto o_localInfo = contexts.bodyContext.GetLocalInfo(loc->name);
        assert(o_localInfo);

        return visit([](auto& localInfo) -> ResultType
        {
            using T = remove_cvref_t<decltype(localInfo)>;
            if constexpr (same_as<T, MqLocalInfo_RefAlias>)
            {
                return MqLocResult_Slot{localInfo.slotIndex};
            }
            else if constexpr (same_as<T, MqLocalInfo_RefPtr>)
            {
                return MqLocResult_Ptr{localInfo.slotIndex};
            }
            else if constexpr (same_as<T, MqLocalInfo_Var>)
            {   
                throw RuntimeFatalException{};
            }
            else static_assert(false);
        }, *o_localInfo);
    }

    ResultType Visit(MLoc_LambdaVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_ListIndexer* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_StructVar* loc) 
    {
        auto e_s_instanceResult = TranslateMLocToQInsts(loc->instance, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_instanceResult);

        return visit([this, loc](auto& locResult) -> ResultType {
            using T = remove_cvref_t<decltype(locResult)>;
            if constexpr (same_as<T, MqLocResult_Slot>) // slot이면
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* rPtrType = contexts.bodyContext.GetPtrType();
                size_t destSlotIndex = contexts.bodyContext.AddTemp(rPtrType, "struct_field");
                contexts.bodyContext.EmitInst(QInst_FieldOf{QArg_Dest_Slot{destSlotIndex}, QArg_Addr_OfSlot{locResult.slotIndex}, loc->decl->GetIndex()});

                return MqLocResult_Ptr{destSlotIndex};
            }
            else if constexpr(same_as<T, MqLocResult_Ptr>)
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* ptrType = contexts.bodyContext.GetPtrType();
                size_t destSlotIndex = contexts.bodyContext.AddTemp(ptrType, "struct_field");
                contexts.bodyContext.EmitInst(QInst_FieldOf{QArg_Dest_Slot{destSlotIndex}, QArg_Addr_PtrSlot{locResult.slotIndex}, loc->decl->GetIndex()});

                return MqLocResult_Ptr{destSlotIndex};
            }
            else static_assert(false);
            
        }, **e_s_instanceResult);
    }

    ResultType Visit(MLoc_ClassVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_EnumElemVar* loc) { throw NotImplementedException{}; }

    ResultType Visit(MLoc_This* loc) 
    {
        // 현재 컨텍스트에서, 첫번째 인자 slot 0번
        return MqLocResult_Slot{0};
    }

    ResultType Visit(MLoc_PtrDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_SharedDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_NullableValue* loc) { throw NotImplementedException{}; }
};

expected<MqEmitState<MqLocResult>, DiagPtr> TranslateMLocToQInsts(MLoc* loc, MqTranslationContexts& contexts)
{
    MLocQInstsTranslator translator{contexts};
    return Accept(translator, loc);
}

} // Citron