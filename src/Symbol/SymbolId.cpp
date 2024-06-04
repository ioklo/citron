module Citron.Identifiers:SymbolId;

import <memory>;
import <ranges>;
import <optional>;

import Citron.Names;
import Citron.Copy;
import :TypeIds;


using namespace std;

namespace Citron {

namespace {

shared_ptr<SymbolPath> nullPath;

}

SymbolPath::SymbolPath(std::shared_ptr<SymbolPath>&& pOuter, Name&& name, std::vector<TypeId>&& typeArgs, std::vector<TypeId>&& paramIds)
    : pOuter(std::move(pOuter)), name(std::move(name)), typeArgs(std::move(typeArgs)), paramIds(std::move(paramIds))
{
}

std::shared_ptr<SymbolPath> SymbolPath::Child(std::shared_ptr<SymbolPath>&& sharedThis, Name&& name, std::vector<TypeId>&& typeArgs, std::vector<TypeId>&& paramIds)
{   
    return make_shared<SymbolPath>(std::move(sharedThis), std::move(name), std::move(typeArgs), std::move(paramIds));
}

SymbolId::SymbolId(std::string&& moduleName, std::shared_ptr<SymbolPath>&& pPath)
    : moduleName(std::move(moduleName)), pPath(std::move(pPath))
{
}

SymbolId SymbolId::Copy() const
{
    return SymbolId(std::string(moduleName), std::shared_ptr<SymbolPath>(pPath));
}

SymbolId SymbolId::Child(Name name, std::vector<TypeId>&& typeArgs, std::vector<TypeId>&& paramIds)
{
    using Citron::Copy;

    if (pPath)
        return SymbolId(std::string(moduleName), pPath->Child(std::shared_ptr<SymbolPath>(pPath), std::move(name), std::move(typeArgs), std::move(paramIds)));
    else
        return SymbolId(std::string(moduleName), make_shared<SymbolPath>(std::shared_ptr<SymbolPath>(), std::move(name), std::move(typeArgs), std::move(paramIds)));
}

 SymbolId boolSymbolId = SymbolId("System.Runtime", nullptr).Child(NormalName("System"), {}, {}).Child(NormalName("Bool"), {}, {});
 SymbolId intSymbolId = SymbolId("System.Runtime", nullptr).Child(NormalName("System"), {}, {}).Child(NormalName("Int32"), {}, {});
 SymbolId stringSymbolId = SymbolId("System.Runtime", nullptr).Child(NormalName("System"), {}, {}).Child(NormalName("String"), {}, {});

}