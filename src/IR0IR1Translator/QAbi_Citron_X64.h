#pragma once
#include <memory>
#include "RSymbol/RFuncReturn.h"
#include "QAbi.h"
#include "QFuncInfo.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;

class QAbi_Citron_X64 : public QAbi
{
    RFactoryPtr rFactory;

public:
    QAbi_Citron_X64(const RFactoryPtr& rFactory);

    size_t GetTypeSize(RType* type) override;    
    QFuncInfo GetFuncInfo(RFuncDecl* rFuncDecl, RTypeArguments* typeArgs) override;
    QFuncInfo GetFuncInfo(QIntrinsicInfo* intrinsicInfo, RTypeArguments* typeArgs) override;

private:
    QReturnPassingMode GetReturnPassingMode(RFuncReturn funcRet, size_t* outCurArgIndex);
};

} // namespace Citron
