#include <memory>
#include <ranges>
#include <optional>

#include "Copy.h"

#include "MNames.h"
#include "MTypeIds.h"

using namespace std;

namespace Citron {

namespace {

shared_ptr<MSymbolPath> nullPath;

}

MSymbolPath::MSymbolPath(std::shared_ptr<MSymbolPath>&& pOuter, MName&& name, std::vector<MTypeId>&& typeArgs, std::vector<MTypeId>&& paramIds)
    : pOuter(std::move(pOuter)), name(std::move(name)), typeArgs(std::move(typeArgs)), paramIds(std::move(paramIds))
{
}

IMPLEMENT_DEFAULTS(MSymbolPath)

std::shared_ptr<MSymbolPath> MSymbolPath::Child(std::shared_ptr<MSymbolPath>&& sharedThis, MName&& name, std::vector<MTypeId>&& typeArgs, std::vector<MTypeId>&& paramIds)
{   
    return make_shared<MSymbolPath>(std::move(sharedThis), std::move(name), std::move(typeArgs), std::move(paramIds));
}

MSymbolId::MSymbolId(std::string&& moduleName, std::shared_ptr<MSymbolPath>&& pPath)
    : moduleName(std::move(moduleName)), pPath(std::move(pPath))
{
}

IMPLEMENT_DEFAULTS(MSymbolId)

MSymbolId MSymbolId::Copy() const
{
    return MSymbolId(std::string(moduleName), std::shared_ptr<MSymbolPath>(pPath));
}

MSymbolId MSymbolId::Child(MName name, std::vector<MTypeId>&& typeArgs, std::vector<MTypeId>&& paramIds)
{
    using Citron::Copy;

    if (pPath)
        return MSymbolId(std::string(moduleName), pPath->Child(std::shared_ptr<MSymbolPath>(pPath), std::move(name), std::move(typeArgs), std::move(paramIds)));
    else
        return MSymbolId(std::string(moduleName), make_shared<MSymbolPath>(std::shared_ptr<MSymbolPath>(), std::move(name), std::move(typeArgs), std::move(paramIds)));
}

 MSymbolId boolSymbolId = MSymbolId("System.Runtime", nullptr).Child(MNormalName("System"), {}, {}).Child(MNormalName("Bool"), {}, {});
 MSymbolId intSymbolId = MSymbolId("System.Runtime", nullptr).Child(MNormalName("System"), {}, {}).Child(MNormalName("Int32"), {}, {});
 MSymbolId stringSymbolId = MSymbolId("System.Runtime", nullptr).Child(MNormalName("System"), {}, {}).Child(MNormalName("String"), {}, {});

}