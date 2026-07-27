#include "NFactory.h"
#include <deque>
#include <cassert>
#include <algorithm>

#include "RSymbol/RFactory.h"
#include "NStructInfo.h"
#include "NImplTraitFunc.h"

using namespace std;

namespace Citron {

struct NFactoryPrivateData
{
    std::deque<NStructInfo> structInfos;
    std::deque<NImplTraitFunc> implTraitFuncs;
};

NFactory::NFactory(RFactoryPtr& rFactory)
    : rFactory{rFactory}, privateData{std::make_unique<NFactoryPrivateData>()}
{
}

NFactory::~NFactory() = default;

NStructInfo* NFactory::MakeNStructInfo()
{
    auto& structInfo = privateData->structInfos.emplace_back();
    return &structInfo;
}

NImplTraitFunc* NFactory::MakeNImplTraitFunc()
{
    auto& implTraitFunc = privateData->implTraitFuncs.emplace_back();
    return &implTraitFunc;
}

} // namespace Citron