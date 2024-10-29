#pragma once

#include <vector>
#include <unordered_map>

#include "RIdentifier.h"
#include "RNames.h"

namespace Citron {

template<typename TNFuncDecl>
class NFuncDeclContainerComponent
{
    using TNFuncDeclPtr = std::shared_ptr<TNFuncDecl>;

    std::vector<TNFuncDeclPtr> funcs;
    std::unordered_map<RIdentifier, TNFuncDeclPtr> idMap;
    std::unordered_map<RName, std::vector<TNFuncDeclPtr>> nameMap;

public:
    void AddFunc(TNFuncDeclPtr func) // consume func
    {
        funcs.push_back(func);

        auto identifier = func->GetIdentifier();
        idMap.insert_or_assign(identifier, func);

        nameMap[identifier.name].push_back(std::move(func));
    }
    
    std::vector<TNFuncDeclPtr> GetFuncs(const RName& name, int minTypeParamCount)
    {
        std::vector<TNFuncDeclPtr> result;

        auto i = nameMap.find(name);
        if (i != nameMap.end()) return {};
        
        for(auto& func : i->second)
            if (minTypeParamCount <= func.GetTypeParamCount())
                result.push_back(func);

        return result;
    }

    const TNFuncDeclPtr& GetFunc(RIdentifier& identifier)
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