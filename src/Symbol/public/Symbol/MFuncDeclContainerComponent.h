#pragma once
#include "MIdentifier.h"
#include "MNames.h"

namespace Citron {

template<typename TMFuncDecl>
class MFuncDeclContainerComponent
{
    using TMFuncDeclPtr = std::shared_ptr<TMFuncDecl>;

    std::vector<TMFuncDeclPtr> funcs;
    std::unordered_map<MIdentifier, TMFuncDeclPtr> idMap;
    std::unordered_map<MName, std::vector<TMFuncDeclPtr>> nameMap;

public:
    void AddFunc(TMFuncDeclPtr func) // consume func
    {
        funcs.push_back(func);

        auto identifier = func.GetIdentifier();
        idMap.insert_or_assign(identifier, func);

        nameMap[identifier.name].push_back(std::move(func));
    }
    
    std::vector<TMFuncDeclPtr> GetFuncs(const MName& name, int minTypeParamCount)
    {
        std::vector<TMFuncDeclPtr> result;

        auto i = nameMap.find(name);
        if (i != nameMap.end()) return {};
        
        for(auto& func : i->second)
            if (minTypeParamCount <= func.GetTypeParamCount())
                result.push_back(func);

        return result;
    }

    const TMFuncDeclPtr& GetFunc(MIdentifier& identifier)
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