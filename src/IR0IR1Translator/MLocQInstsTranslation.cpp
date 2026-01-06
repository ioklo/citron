#include "MLocQInstsTranslation.h"

#include <variant>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RStructVarDecl.h"
#include "MIR/MLoc.h"

#include "QBodyContext.h"

using namespace std;

namespace Citron {

// 메모리 주소를 value로 돌려주는 
class MLocQInstsTranslator
{
public:
    using ResultType = expected<QLocResult, DiagPtr>;
    QBodyContext& bodyContext;

public:
    MLocQInstsTranslator(QBodyContext& bodyContext)
        : bodyContext{bodyContext} {
    }
    
    ResultType Visit(MLoc_Temp* loc) { throw NotImplementedException{}; }

    ResultType Visit(MLoc_LocalVar* loc)
    {
        auto o_localInfo = bodyContext.GetLocalInfo(loc->name);
        assert(o_localInfo);

        auto& varInfo = get<QLocalInfo_Var>(*o_localInfo);
        return QLocResult_Slot{varInfo.slotIndex};
    }

    ResultType Visit(MLoc_LocalRef* loc)
    {
        auto o_localInfo = bodyContext.GetLocalInfo(loc->name);
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
                return QLocResult_PtrSlot{localInfo.slotIndex};
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
        auto e_instanceResult = TranslateMLocToQInsts(loc->instance, bodyContext);
        RETURN_ON_ERROR(e_instanceResult);

        return visit([this, loc](auto& locResult) -> ResultType {
            using T = remove_cvref_t<decltype(locResult)>;
            if constexpr (same_as<T, QLocResult_Slot>) // slot이면
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* qPtrType = bodyContext.GetPtrQType();
                size_t instSlotIndex = bodyContext.NewSlot(qPtrType);
                auto e_addrResult = bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{instSlotIndex}, QArg_Slot{locResult.slotIndex}});
                RETURN_ON_ERROR(e_addrResult);

                size_t destSlotIndex = bodyContext.NewSlot(qPtrType);
                auto e_fieldResult = bodyContext.EmitInst(QInst_FieldOf{QArg_Slot{destSlotIndex}, QArg_Slot{instSlotIndex}, loc->decl->GetIndex()});
                RETURN_ON_ERROR(e_fieldResult);

                return QLocResult_PtrSlot{destSlotIndex};
            }
            else if constexpr(same_as<T, QLocResult_PtrSlot>)
            {
                // slot의 addrof를 하나 한다 ptr 타입
                auto* qPtrType = bodyContext.GetPtrQType();

                size_t destSlotIndex = bodyContext.NewSlot(qPtrType);
                auto e_fieldResult = bodyContext.EmitInst(QInst_FieldOf{QArg_Slot{destSlotIndex}, QArg_Slot{locResult.slotIndex}, loc->decl->GetIndex()});
                RETURN_ON_ERROR(e_fieldResult);

                return QLocResult_PtrSlot{destSlotIndex};
            }
            else static_assert(false);

            
        }, *e_instanceResult);
    }

    ResultType Visit(MLoc_ClassVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_EnumElemVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_This* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_PtrDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_BoxDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_NullableValue* loc) { throw NotImplementedException{}; }
};

expected<QLocResult, DiagPtr> TranslateMLocToQInsts(MLoc* loc, QBodyContext& bodyContext)
{
    MLocQInstsTranslator translator{bodyContext};
    return Accept(translator, loc);
}

} // Citron