#include "QAbi_Citron_X64.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RFactory.h"
#include "QFuncInfo.h"

using namespace std;

namespace Citron {

QAbi_Citron_X64::QAbi_Citron_X64(const RFactoryPtr& rFactory)
    : rFactory{rFactory}
{
}

size_t QAbi_Citron_X64::GetTypeSize(RType* type)
{
    if (dynamic_cast<RType_Ptr*>(type))
        return 8; // 포인터는 64bit로 간주한다

    if (type == rFactory->MakeBoolType())
        return 1;

    if (type == rFactory->MakeIntType())
        return 4;

    if (type == rFactory->MakeStringType())
        return sizeof(string);

    throw NotImplementedException{};
}

QFuncInfo QAbi_Citron_X64::GetFuncInfo(MCallable& callable)
{
    size_t curArgIndex = 0;
    auto returnPassingMode = GetReturnPassingMode(rFuncDecl->GetFuncReturn(typeArgs), &curArgIndex);

    optional<size_t> thisIndex;
    switch (rFuncDecl->GetThisKind())
    {
    case RThisKind::None: thisIndex = nullopt; break;
    case RThisKind::Handle: thisIndex = curArgIndex++; break;
    case RThisKind::Ptr: thisIndex = curArgIndex++; break;
    }

    size_t explicitArgStartIndex = curArgIndex;
    vector<QParamPassingMode> paramPassingModes;
    size_t count = rFuncDecl->GetParamCount();
    paramPassingModes.reserve(count);
    for (size_t i = 0; i < count; i++)
    {
        auto param = rFuncDecl->GetFuncParam(typeArgs, i);
        if (param.IsRef())
            paramPassingModes.push_back(QParamPassingMode::Ref);
        else
        {
            switch (param.type->GetCopyStrategy())
            {
            case RCopyStrategy::Void:
                throw RuntimeFatalException{};

            case RCopyStrategy::Bitwise:
            {
                size_t typeSize = GetTypeSize(param.type);
                if (typeSize <= 8)
                    paramPassingModes.push_back(QParamPassingMode::Direct);
                else
                    paramPassingModes.push_back(QParamPassingMode::Indirect);
                break;
            }

            case RCopyStrategy::NonBitwise:
                paramPassingModes.push_back(QParamPassingMode::Indirect);
                break;
            }
        }
    }

    return QFuncInfo{
        .returnPassingMode = returnPassingMode,
        .thisIndex = thisIndex,
        .explicitArgStartIndex = explicitArgStartIndex,
        .paramPassingModes = move(paramPassingModes)
    };
}

QReturnPassingMode QAbi_Citron_X64::GetReturnPassingMode(RFuncReturn&& funcRet, size_t* outCurArgIndex)
{
    return visit([this, outCurArgIndex](auto& funcRet) -> QReturnPassingMode
    {
        using T = remove_cvref_t<decltype(funcRet)>;
        if constexpr (same_as<T, RFuncReturn_ForCtor>)
        {
            return QReturnPassingMode_Void{};
        }
        else if constexpr (same_as<T, RFuncReturn_Set>)
        {
            auto* rType = funcRet.type;
            switch (rType->GetCopyStrategy())
            {
            case RCopyStrategy::Void:
                return QReturnPassingMode_Void{};

            case RCopyStrategy::Bitwise:
            {
                // X64면 rType사이즈가 64bit보다 작으면 Direct, 크면 Indirect
                size_t typeSize = GetTypeSize(rType);
                if (typeSize <= 8)
                    return QReturnPassingMode_Direct{};
                else
                    return QReturnPassingMode_Indirect{(*outCurArgIndex)++};
            }

            case RCopyStrategy::NonBitwise:
                return QReturnPassingMode_Indirect{(*outCurArgIndex)++};
            }

            unreachable();
        }
        else if constexpr (same_as<T, RFuncReturn_NotSet>)
        {
            throw RuntimeFatalException{};
        }
    }, funcRet);
}

} // namespace Citron