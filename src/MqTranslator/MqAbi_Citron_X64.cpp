#include "MqAbi_Citron_X64.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RFactory.h"
#include "MqFuncInfo.h"
#include "CommonQInstsTranslation.h"
#include "MqIntrinsicInfo.h"

using namespace std;

namespace Citron {

MqAbi_Citron_X64::MqAbi_Citron_X64(TakeRef<RFactoryPtr> rFactory)
    : rFactory{rFactory.Take()}
{
}

size_t MqAbi_Citron_X64::GetTypeSize(RType* type)
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

MqFuncInfo MqAbi_Citron_X64::GetFuncInfo(RFuncDecl* rFuncDecl, RTypeArguments* typeArgs)
{
    size_t curArgIndex = 0;

    auto returnPassingMode = GetReturnPassingMode(rFuncDecl->GetFuncReturn(typeArgs), &curArgIndex);

    auto thisPassingMode = rFuncDecl->GetThisKind().Visit([&curArgIndex](auto&& thisKind) -> MqThisPassingMode {
        using T = remove_cvref_t<decltype(thisKind)>;
        if constexpr (same_as<T, RThisKind_Static>)
            return MqThisPassingMode_None{};
        else if constexpr (same_as<T, RThisKind_Handle>)
            return MqThisPassingMode_Handle{curArgIndex++};
        else if constexpr (same_as<T, RThisKind_Ref>)
            return MqThisPassingMode_Ptr{curArgIndex++};
        else static_assert(false);
    });

    size_t explicitArgStartIndex = curArgIndex;
    vector<MqParamPassingMode> paramPassingModes;
    size_t count = rFuncDecl->GetParamCount();
    paramPassingModes.reserve(count);
    for (size_t i = 0; i < count; i++)
    {
        auto param = rFuncDecl->GetFuncParam(typeArgs, i);
        if (param.IsRef())
            paramPassingModes.push_back(MqParamPassingMode::Ref);
        else
        {
            switch (param.type->GetCopyStrategy())
            {
            case RCopyStrategy::Void:
                throw RuntimeFatalException{}; // 파라미터에는 void가 올 수 없다ㄴ

            case RCopyStrategy::Bitwise:
            {
                size_t typeSize = GetTypeSize(param.type);
                if (typeSize <= 8)
                    paramPassingModes.push_back(MqParamPassingMode::Direct);
                else
                    paramPassingModes.push_back(MqParamPassingMode::Indirect);
                break;
            }

            case RCopyStrategy::NonBitwise:
                paramPassingModes.push_back(MqParamPassingMode::Indirect);
                break;
            }
        }
    }

    return MqFuncInfo{
        .returnPassingMode = returnPassingMode,
        .thisPassingMode = thisPassingMode,
        .explicitArgStartIndex = explicitArgStartIndex,
        .paramPassingModes = move(paramPassingModes)
    };
}

MqFuncInfo MqAbi_Citron_X64::GetFuncInfo(MqIntrinsicInfo& intrinsicInfo, RTypeArguments* typeArgs)
{
    size_t curArgIndex = 0;
    auto returnPassingMode = GetReturnPassingMode(intrinsicInfo.funcRet, &curArgIndex);

    size_t explicitArgStartIndex = curArgIndex;
    vector<MqParamPassingMode> paramPassingModes;
    size_t count = intrinsicInfo.funcParams.size();
    paramPassingModes.reserve(count);
    for (size_t i = 0; i < count; i++)
    {
        auto appliedFuncParam = intrinsicInfo.funcParams[i].Apply(typeArgs); // TODO: intrinsic에 typeArgs 적용.

        if (appliedFuncParam.IsRef())
            paramPassingModes.push_back(MqParamPassingMode::Ref);
        else
        {
            switch (appliedFuncParam.type->GetCopyStrategy())
            {
            case RCopyStrategy::Void:
                throw RuntimeFatalException{}; // 파라미터에는 void가 올 수 없다

            case RCopyStrategy::Bitwise:
            {
                size_t typeSize = GetTypeSize(appliedFuncParam.type);
                if (typeSize <= 8)
                    paramPassingModes.push_back(MqParamPassingMode::Direct);
                else
                    paramPassingModes.push_back(MqParamPassingMode::Indirect);
                break;
            }

            case RCopyStrategy::NonBitwise:
                paramPassingModes.push_back(MqParamPassingMode::Indirect);
                break;
            }
        }
    }

    return MqFuncInfo{
        .returnPassingMode = returnPassingMode,
        .thisPassingMode = MqThisPassingMode_None{},
        .explicitArgStartIndex = explicitArgStartIndex,
        .paramPassingModes = move(paramPassingModes)
    };
}

MqReturnPassingMode MqAbi_Citron_X64::GetReturnPassingMode(RFuncReturn funcRet, size_t* outCurArgIndex)
{
    return funcRet.Visit([this, outCurArgIndex](auto& funcRet) -> MqReturnPassingMode
    {
        using T = remove_cvref_t<decltype(funcRet)>;
        if constexpr (same_as<T, RFuncReturn_None>)
        {
            return MqReturnPassingMode_Void{};
        }
        else if constexpr (same_as<T, RFuncReturn_Normal>)
        {
            auto* rType = funcRet.type;
            switch (rType->GetCopyStrategy())
            {
            case RCopyStrategy::Void:
                return MqReturnPassingMode_Void{};

            case RCopyStrategy::Bitwise:
            {
                // X64면 rType사이즈가 64bit보다 작으면 Direct, 크면 Indirect
                size_t typeSize = GetTypeSize(rType);
                if (typeSize <= 8)
                    return MqReturnPassingMode_Direct{};
                else
                    return MqReturnPassingMode_Indirect{(*outCurArgIndex)++};
            }

            case RCopyStrategy::NonBitwise:
                return MqReturnPassingMode_Indirect{(*outCurArgIndex)++};
            }

            unreachable();
        }
        else if constexpr (same_as<T, RFuncReturn_NotSet>)
        {
            throw RuntimeFatalException{};
        }
    });
}

} // namespace Citron