export module Citron.DeclSymbols:FuncDeclSymbolComponent;

import Citron.Identifiers;
import Citron.Names;

import <vector>;
import <unordered_map>;

namespace Citron {

template<typename T>
concept FuncDeclSymbol = requires(T t) {
    t.GetTypeParamCount();
};

export template<typename TFuncDeclSymbol>
class FuncDeclSymbolComponent
{
    std::vector<TFuncDeclSymbol> funcs;
    std::unordered_map<Identifier, size_t> idMap;
    std::unordered_map<Name, std::vector<size_t>> nameMap;

public:
    void AddFunc(TFuncDeclSymbol&& declSymbol) // consume declSymbol
    {
        size_t index = funcs.size();
        funcs.push_back(std::move(declSymbol));

        auto identifier = declSymbol.GetIdentifier();
        idMap.insert_or_assign(identifier, index);

        nameMap[identifier.Name].push_back(index);
    }

    TFuncDeclSymbol* GetFunc(int index)
    {
        return &funcs[index];
    }

    std::vector<int> GetFuncs(Name& name, int minTypeParamCount)
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

    std::ptrdiff_t GetFunc(Identifier& identifier)
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