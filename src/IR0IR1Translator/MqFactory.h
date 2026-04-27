#pragma once

namespace Citron {

struct QIntrinsicInfo;

// Mir to Qir Translation Factory
class MqtFactory
{

public:
    QIntrinsicInfo* GetIntrinsicInfo(MExp_CallIntrinsicKind kind, RFactory* rFactory);
    QIntrinsicInfo* GetIntrinsicInfo(MInitExp_CallIntrinsicKind kind, RFactory* rFactory);
    QIntrinsicInfo* GetIntrinsicInfo(QInst_IntrinsicKind kind, RFactory* rFactory);
};

} // namespace Citron