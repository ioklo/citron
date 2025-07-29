#pragma once

#include <vector>
#include <unordered_map>
#include <memory>
#include <optional>

#include "DeclWithOuterTypeArgs.h"

namespace Citron {

template<typename TNFuncDecl>
class NFuncDeclContainerComponent
{
    using TNFuncDeclPtr = std::shared_ptr<TNFuncDecl>;
    using RDeclType = typename TNFuncDecl::RDeclType;
    using RMemberType = typename TNFuncDecl::RMemberType;

    std::vector<TNFuncDeclPtr> funcs;
    std::unordered_map<RIdentifier, TNFuncDeclPtr> idMap;
    std::unordered_map<RName, std::vector<TNFuncDeclPtr>> nameMap;

public:
    void AddFunc(TNFuncDeclPtr&& func) // consume func
    {
        funcs.push_back(func);

        auto identifier = func->GetIdentifier();
        idMap.insert_or_assign(identifier, func);

        nameMap[identifier.name].push_back(std::move(func));
    }

    const TNFuncDeclPtr& GetFunc(RIdentifier& identifier)
    {
        return idMap[identifier];
    }

    std::optional<RMemberType> GetMemberFunc(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
    {
        std::vector<DeclWithOuterTypeArgs<RDeclType>> result;

        auto i = nameMap.find(name);
        if (i != nameMap.end()) return {};

        for (auto& func : i->second)
            if (explicitTypeParamsExceptOuterCount <= func->GetTypeParamCount())
                result.push_back(DeclWithOuterTypeArgs<RDeclType>(func, typeArgs));

        return RMemberType(std::move(result));

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