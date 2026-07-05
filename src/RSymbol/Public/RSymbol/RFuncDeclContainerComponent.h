#pragma once

#include <vector>
#include <unordered_map>
#include <optional>
#include "Infra/Ref.h"
#include "DeclWithOuterTypeArgs.h"

namespace Citron {

template<typename TRFuncDecl>
class RFuncDeclContainerComponent
{
    using RDeclResType = typename TRFuncDecl::RDeclResType;

    std::vector<TRFuncDecl*> funcs;
    std::unordered_map<RIdentifier, TRFuncDecl*> idMap;
    std::unordered_map<RName, std::vector<TRFuncDecl*>> nameMap;

public:
    void AddFunc(TRFuncDecl* func) // consume func
    {
        funcs.push_back(func);

        auto identifier = func->GetIdentifier();
        idMap.insert_or_assign(identifier, func);

        nameMap[identifier.name].push_back(func);
    }

    TRFuncDecl* GetFunc(RIdentifier& identifier)
    {
        return idMap[identifier];
    }

    std::optional<RDeclResType> GetMemberFunc(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
    {
        std::vector<TDeclWithOuterTypeArgs<TRFuncDecl>> result;

        auto i = nameMap.find(*name);
        if (i == nameMap.end()) return {};

        for (auto& func : i->second)
            if (explicitTypeParamsExceptOuterCount <= func->GetTypeParamCount())
                result.push_back(TDeclWithOuterTypeArgs<TRFuncDecl>{func, typeArgs});

        return RDeclResType{std::move(result)};

    }

};

}