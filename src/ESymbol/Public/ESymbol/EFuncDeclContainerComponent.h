#pragma once

#include <vector>
#include <unordered_map>
#include <variant>

#include "EIdentifier.h"

#include "ENames.h"

namespace Citron {

template<typename TEFuncDecl>
class EFuncDeclContainerComponent
{
    std::vector<TEFuncDecl*> funcs;
    std::unordered_map<EIdentifier, TEFuncDecl*> idMap;
    std::unordered_map<EName, std::vector<TEFuncDecl*>> nameMap;

public:
    void AddFunc(TEFuncDecl* func) // consume func
    {
        funcs.push_back(func);

        auto identifier = func->GetIdentifier();
        idMap.insert_or_assign(identifier, func);

        nameMap[identifier.name].push_back(std::move(func));
    }

    std::vector<TEFuncDecl*> GetFuncs(const EName& name, int minTypeParamCount)
    {
        std::vector<TEFuncDecl*> result;

        auto i = nameMap.find(name);
        if (i != nameMap.end()) return {};

        for (auto& func : i->second)
            if (minTypeParamCount <= func.GetTypeParamCount())
                result.push_back(func);

        return result;
    }

    TEFuncDecl* GetFunc(EIdentifier& identifier)
    {
        return idMap[identifier];
    }

    //    public IEnumerable<TFuncDeclSymbol> GetEnumerable()
    //    {
    //        return map.Values;
    //    }

    //    public IEnumerable<TFuncDeclSymbol> GetFuncs()
    //    {
    //        return map.Values;
    //    }

    //    bool ICyclicEqualityComparableStruct<FuncDeclSymbolComponent<TFuncDeclSymbol>>.CyclicEquals(ref FuncDeclSymbolComponent<TFuncDeclSymbol> other, ref CyclicEqualityCompareContext context)
    //    {
    //        if (!map.CyclicEqualsClassValue(other.map, ref context))
    //            return false;

    //        return true;
    //    }

    //    void ISerializable.DoSerialize(ref SerializeContext context)
    //    {
    //        context.SerializeDictRefKeyRefValue(nameof(map), map);
    //    }
    //}
};

}