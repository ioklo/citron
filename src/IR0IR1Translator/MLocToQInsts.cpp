#include "MLocToQInsts.h"

#include <variant>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RStructVarDecl.h"
#include "MIR/MLoc.h"

#include "MqBodyContext.h"
#include "MqTranslationContexts.h"
#include "MCreateToQInsts.h"
#include "MqEmitState.h"

using namespace std;

namespace Citron {

// MLoc이 가리키는 위치를 slot자체나, ptr를 돌려준다 (ptr이 들어간 slot을 리턴한다)
struct MLocQInstsTranslator
{
    using ResultType = expected<MqEmitState<QLocResult>, DiagPtr>;
    MqTranslationContexts& contexts;
    
    ResultType Visit(MLoc_Materialize* loc) 
    {
        RType* rType = GetType(loc->create, &*contexts.rFactory);
        size_t slotIndex = contexts.bodyContext.AddTemp(rType, "materialize");
        auto e_s_result = TranslateMCreateToQInsts(loc->create, MqCreateTarget_Slot{slotIndex}, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        return QLocResult_Slot{slotIndex};
    }

    ResultType Visit(MLoc_LocalVar* loc)
    {
        auto o_localInfo = contexts.bodyContext.GetLocalInfo(loc->name);
        assert(o_localInfo);

        auto& varInfo = get<MqLocalInfo_Var>(*o_localInfo);
        return QLocResult_Slot{varInfo.slotIndex};
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
                return QLocResult_Slot{localInfo.slotIndex};
            }
            else if constexpr (same_as<T, MqLocalInfo_RefPtr>)
            {
                return QLocResult_Ptr{localInfo.slotIndex};
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
            if constexpr (same_as<T, QLocResult_Slot>) // slot이면
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* rPtrType = contexts.bodyContext.GetPtrType();
                size_t destSlotIndex = contexts.bodyContext.AddTemp(rPtrType, "struct_field");
                contexts.bodyContext.EmitInst(QInst_FieldOf{QArg_Dest_Slot{destSlotIndex}, QArg_Addr_OfSlot{locResult.slotIndex}, loc->decl->GetIndex()});

                return QLocResult_Ptr{destSlotIndex};
            }
            else if constexpr(same_as<T, QLocResult_Ptr>)
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* ptrType = contexts.bodyContext.GetPtrType();
                size_t destSlotIndex = contexts.bodyContext.AddTemp(ptrType, "struct_field");
                contexts.bodyContext.EmitInst(QInst_FieldOf{QArg_Dest_Slot{destSlotIndex}, QArg_Addr_PtrSlot{locResult.slotIndex}, loc->decl->GetIndex()});

                return QLocResult_Ptr{destSlotIndex};
            }
            else static_assert(false);
            
        }, **e_s_instanceResult);
    }

    ResultType Visit(MLoc_ClassVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_EnumElemVar* loc) { throw NotImplementedException{}; }

    ResultType Visit(MLoc_This* loc) 
    {
        // 현재 컨텍스트에서, 첫번째 인자 slot 0번
        return QLocResult_Slot{0};
    }

    ResultType Visit(MLoc_PtrDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_SharedDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_NullableValue* loc) { throw NotImplementedException{}; }
};

expected<MqEmitState<QLocResult>, DiagPtr> TranslateMLocToQInsts(MLoc* loc, MqTranslationContexts& contexts)
{
    MLocQInstsTranslator translator{contexts};
    return Accept(translator, loc);
}

} // Citron