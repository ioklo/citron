#include "MLocToQInsts.h"

#include <variant>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RStructVarDecl.h"
#include "MIR/MLoc.h"

#include "QBodyContext.h"
#include "QTranslationContexts.h"
#include "MCreateToQInsts.h"

using namespace std;

namespace Citron {

// MLoc이 가리키는 위치를 slot자체나, ptr를 돌려준다 (ptr이 들어간 slot을 리턴한다)
struct MLocQInstsTranslator
{
    using ResultType = expected<QLocResult, DiagPtr>;
    QTranslationContexts& contexts;
    
    ResultType Visit(MLoc_Materialize* loc) 
    {
        RType* rType = GetType(loc->create, &*contexts.rFactory);
        size_t slotIndex = contexts.bodyContext.NewSlot(rType);
        auto e_result = TranslateMCreateToQInsts(loc->create, slotIndex, contexts);
        RETURN_ON_ERROR(e_result);

        return QLocResult_Slot{slotIndex};
    }

    ResultType Visit(MLoc_LocalVar* loc)
    {
        auto o_localInfo = contexts.bodyContext.GetLocalInfo(loc->name);
        assert(o_localInfo);

        auto& varInfo = get<QLocalInfo_Var>(*o_localInfo);
        return QLocResult_Slot{varInfo.slotIndex};
    }

    ResultType Visit(MLoc_LocalRef* loc)
    {
        auto o_localInfo = contexts.bodyContext.GetLocalInfo(loc->name);
        assert(o_localInfo);

        return visit([](auto& localInfo) -> ResultType
        {
            using T = remove_cvref_t<decltype(localInfo)>;
            if constexpr (same_as<T, QLocalInfo_RefAlias>)
            {
                return QLocResult_Slot{localInfo.slotIndex};
            }
            else if constexpr (same_as<T, QLocalInfo_RefPtr>)
            {
                return QLocResult_Ptr{localInfo.slotIndex};
            }
            else if constexpr (same_as<T, QLocalInfo_Var>)
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
        auto e_instanceResult = TranslateMLocToQInsts(loc->instance, contexts);
        RETURN_ON_ERROR(e_instanceResult);

        return visit([this, loc](auto& locResult) -> ResultType {
            using T = remove_cvref_t<decltype(locResult)>;
            if constexpr (same_as<T, QLocResult_Slot>) // slot이면
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* rPtrType = contexts.bodyContext.GetPtrType();
                size_t instSlotIndex = contexts.bodyContext.NewSlot(rPtrType);
                contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{instSlotIndex}, QArg_Slot{locResult.slotIndex}});

                size_t destSlotIndex = contexts.bodyContext.NewSlot(rPtrType);
                contexts.bodyContext.EmitInst(QInst_FieldOf{QArg_Slot{destSlotIndex}, QArg_Slot{instSlotIndex}, loc->decl->GetIndex()});

                return QLocResult_Ptr{destSlotIndex};
            }
            else if constexpr(same_as<T, QLocResult_Ptr>)
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* ptrType = contexts.bodyContext.GetPtrType();

                size_t destSlotIndex = contexts.bodyContext.NewSlot(ptrType);
                contexts.bodyContext.EmitInst(QInst_FieldOf{QArg_Slot{destSlotIndex}, QArg_Slot{locResult.slotIndex}, loc->decl->GetIndex()});

                return QLocResult_Ptr{destSlotIndex};
            }
            else static_assert(false);
            
        }, *e_instanceResult);
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

expected<QLocResult, DiagPtr> TranslateMLocToQInsts(MLoc* loc, QTranslationContexts& contexts)
{
    MLocQInstsTranslator translator{contexts};
    return Accept(translator, loc);
}

} // Citron