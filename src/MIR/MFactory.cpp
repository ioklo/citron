#include "MFactory.h"
#include "MLoc.h"
#include "MExp.h"
#include "MSharedExp.h"
#include "MStmt.h"
#include "MData.h"

namespace Citron {

MFactory::MFactory() = default;
MFactory::~MFactory() = default;

MData* MFactory::MakeMData(std::vector<MFuncBody>&& funcBodies)
{
    auto data = std::make_unique<MData>(move(funcBodies));
    auto* pData = data.get();
    datas.push_back(move(data));
    return pData;
}

} // namespace Citron