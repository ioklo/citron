#pragma once

#include <vector>
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RFuncParameter.h"

namespace Citron {

enum class QInst_IntrinsicKind;
enum class MExp_CallIntrinsicKind;
enum class MInitExp_CallIntrinsicKind;
class RFactory;
struct QTranslationContexts;

struct QIntrinsicInfo
{
    QInst_IntrinsicKind kind;
    RFuncReturn funcRet;
    std::vector<RFuncParameter> funcParams;
};

QIntrinsicInfo* GetIntrinsicInfo(MExp_CallIntrinsicKind kind, RFactory* rFactory);
QIntrinsicInfo* GetIntrinsicInfo(MInitExp_CallIntrinsicKind kind, RFactory* rFactory);
QIntrinsicInfo* GetIntrinsicInfo(QInst_IntrinsicKind kind, RFactory* rFactory);

} // namespace Citron