#include "RTypeToSmType.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RTypeParam.h"
#include "SmFactory.h"
#include "SmType.h"
#include "RAppliedDeclToSmAppliedDecl.h"

using namespace std;
namespace Citron {

struct RTypeToSmTypeTranslator
{
    using ResultType = SmType*;
    SmFactory* factory;

    template<typename TRType, typename TSmType>
    ResultType HandleWrapperType(TRType* rType)
    {
        auto innerType = TranslateRTypeToSmType(rType->innerType, factory);
        return factory->MakeSmType<TSmType>(innerType);
    }

    template<typename TRType, typename TSmType>
    ResultType HandleAppliedDeclType(TRType* rType)
    {
        auto appliedDecl = TranslateRAppliedDeclToSmAppliedDecl(rType->appliedDecl, factory);
        return factory->MakeSmType<TSmType>(move(appliedDecl));
    }
    
    ResultType Visit(RType_Nullable* rType) { return HandleWrapperType<RType_Nullable, SmType_Nullable>(rType); }
    ResultType Visit(RType_NullableInplace* rType) { return HandleWrapperType<RType_NullableInplace, SmType_NullableInplace>(rType); }

    ResultType Visit(RType_TypeVar* rType) 
    {
        return factory->MakeSmType<SmType_TypeVar>(rType->typeParam->GetGlobalIndex());
    }

    ResultType Visit(RType_Void* rType) 
    {
        return factory->MakeSmType<SmType_Void>();
    }

    ResultType Visit(RType_Primitive* rType) 
    {
        return factory->MakeSmType<SmType_Primitive>(rType->GetPrimitiveKind());
    }

    ResultType Visit(RType_Tuple* rType) 
    {
        // TODO: [44] Tuple 구현
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Func* rType) 
    {
        // TODO: [72] 2026-07-21, func<> 타입 구현
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Ptr* rType) { return HandleWrapperType<RType_Ptr, SmType_Ptr>(rType); }
    ResultType Visit(RType_Shared* rType) { return HandleWrapperType<RType_Shared, SmType_Shared>(rType); }
    ResultType Visit(RType_Box* rType) { return HandleWrapperType<RType_Box, SmType_Box>(rType); }

    ResultType Visit(RType_Class* rType) { return HandleAppliedDeclType<RType_Class, SmType_Class>(rType); }
    ResultType Visit(RType_Struct* rType) { return HandleAppliedDeclType<RType_Struct, SmType_Struct>(rType); }
    ResultType Visit(RType_Enum* rType) { return HandleAppliedDeclType<RType_Enum, SmType_Enum>(rType); }
    ResultType Visit(RType_EnumElem* rType) { return HandleAppliedDeclType<RType_EnumElem, SmType_EnumElem>(rType); }
    ResultType Visit(RType_Interface* rType) 
    {
        // TODO: [71] 2026-07-18, interface 구현
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Lambda* rType) 
    { 
        // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Opaque* rType)
    {
        auto appliedTrait = TranslateRAppliedDeclToSmAppliedDecl(rType->appliedTrait, factory);
        auto appliedOwnerFunc = TranslateRAppliedDeclToSmAppliedDecl(rType->appliedOwnerFunc, factory);

        return factory->MakeSmType<SmType_Opaque>(move(appliedTrait), move(appliedOwnerFunc));
    }
};

SmType* TranslateRTypeToSmType(RType* rType, SmFactory* factory)
{
    return Accept(RTypeToSmTypeTranslator{factory}, rType);
}

} // namespace Citron