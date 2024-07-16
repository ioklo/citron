#pragma once
#include "MIdentifier.h"
#include "MNames.h"

namespace Citron {

template<typename TMFuncDecl>
class MFuncDeclContainerComponent
{
    std::vector<TMFuncDecl> funcs;
    std::unordered_map<MIdentifier, size_t> idMap;
    std::unordered_map<MName, std::vector<size_t>> nameMap;

public:
    void AddFunc(TMFuncDecl&& func) // consume func
    {
        size_t index = funcs.size();
        funcs.push_back(std::move(func));

        auto identifier = func.GetIdentifier();
        idMap.insert_or_assign(identifier, index);

        nameMap[identifier.MName].push_back(index);
    }

    TMFuncDecl* GetFunc(int index)
    {
        return &funcs[index];
    }

    std::vector<int> GetFuncs(MName& name, int minTypeParamCount)
    {
        std::vector<int> result;

        auto i = nameMap.find(name);
        if (i != nameMap.end()) return {};

        for(int index : i->second)
        {
            auto& item = funcs[index];
            if (minTypeParamCount <= item.GetTypeParamCount())
                result.push_back(index);
        }

        return result;
    }

    std::ptrdiff_t GetFunc(MIdentifier& identifier)
    {
        auto i = idMap.find(identifier);
        if (i != idMap.end()) return -1;

        return i->second;
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