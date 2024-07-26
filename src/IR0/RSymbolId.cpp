#include <memory>
#include <ranges>
#include <optional>

#include <Infra/Copy.h>

#include "RNames.h"
#include "RTypeId.h"

using namespace std;

namespace Citron {

namespace {

shared_ptr<RSymbolPath> nullPath;

}

RSymbolPath::RSymbolPath(
    std::shared_ptr<RSymbolPath>&& pOuter, 
    RName&& name, 
    std::vector<std::shared_ptr<RTypeId>>&& typeArgs, 
    std::vector<std::shared_ptr<RTypeId>>&& paramIds)
    : pOuter(std::move(pOuter)), name(std::move(name)), typeArgs(std::move(typeArgs)), paramIds(std::move(paramIds))
{
}

IMPLEMENT_DEFAULTS(RSymbolPath)

std::shared_ptr<RSymbolPath> RSymbolPath::Child(std::shared_ptr<RSymbolPath>&& sharedThis, RName&& name, std::vector<std::shared_ptr<RTypeId>>&& typeArgs, std::vector<std::shared_ptr<RTypeId>>&& paramIds)
{   
    return make_shared<RSymbolPath>(std::move(sharedThis), std::move(name), std::move(typeArgs), std::move(paramIds));
}

RSymbolId::RSymbolId(std::string&& moduleName, std::shared_ptr<RSymbolPath>&& pPath)
    : moduleName(std::move(moduleName)), pPath(std::move(pPath))
{
}

IMPLEMENT_DEFAULTS(RSymbolId)

RSymbolId RSymbolId::Copy() const
{
    return RSymbolId(std::string(moduleName), std::shared_ptr<RSymbolPath>(pPath));
}

RSymbolId RSymbolId::Child(RName name, std::vector<std::shared_ptr<RTypeId>>&& typeArgs, std::vector<std::shared_ptr<RTypeId>>&& paramIds)
{
    using Citron::Copy;

    if (pPath)
        return RSymbolId(std::string(moduleName), pPath->Child(std::shared_ptr<RSymbolPath>(pPath), std::move(name), std::move(typeArgs), std::move(paramIds)));
    else
        return RSymbolId(std::string(moduleName), make_shared<RSymbolPath>(std::shared_ptr<RSymbolPath>(), std::move(name), std::move(typeArgs), std::move(paramIds)));
}

 RSymbolId boolSymbolId = RSymbolId("System.Runtime", nullptr).Child(RNormalName("System"), {}, {}).Child(RNormalName("Bool"), {}, {});
 RSymbolId intSymbolId = RSymbolId("System.Runtime", nullptr).Child(RNormalName("System"), {}, {}).Child(RNormalName("Int32"), {}, {});
 RSymbolId stringSymbolId = RSymbolId("System.Runtime", nullptr).Child(RNormalName("System"), {}, {}).Child(RNormalName("String"), {}, {});

}