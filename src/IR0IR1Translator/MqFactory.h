#pragma once

#include <memory>
#include <unordered_map>

#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "QIR/QInsts.h"
#include "MqIntrinsicInfo.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;

class MqFactory
{
    MqIntrinsicInfo intrinsicInfos[(size_t)QInst_IntrinsicKind::Max];
    MqIntrinsicInfo* mExpToMq[(size_t)MExp_CallIntrinsicKind::Max];
    MqIntrinsicInfo* mInitExpToMq[(size_t)MInitExp_CallIntrinsicKind::Max];

    RFactoryPtr rFactory;

public:
    MqFactory(const RFactoryPtr& rFactory);

private:
    void MakeIntrinsicInfo() noexcept;

public:
    MqIntrinsicInfo& GetIntrinsicInfo(QInst_IntrinsicKind kind) { return intrinsicInfos[(size_t)kind]; }
    MqIntrinsicInfo& GetIntrinsicInfo(MExp_CallIntrinsicKind kind) { return *mExpToMq[(size_t)kind]; }
    MqIntrinsicInfo& GetIntrinsicInfo(MInitExp_CallIntrinsicKind kind) { return *mInitExpToMq[(size_t)kind]; }
};

} // namespace Citron