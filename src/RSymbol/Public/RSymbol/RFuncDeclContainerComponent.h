#pragma once
#include <vector>
#include <optional>
#include <unordered_map>
#include <optional>
#include "Infra/Ref.h"
#include "RMember.h"

namespace Citron {

class RMember;

template<typename TRFuncDecl, typename RMemberType>
class RFuncDeclContainerComponent
{
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

    std::optional<RMember> GetFuncs(InRef<RName> name)
    {
        std::vector<TRFuncDecl*> funcs;

        auto i = nameMap.find(*name);
        if (i == nameMap.end()) return std::nullopt;

        for (auto& func : i->second)
            funcs.push_back(func);

        return RMemberType{std::move(funcs)};
    }

    /*std::optional<RDeclResType> GetMemberFunc(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
    {
        std::vector<TDeclWithOuterTypeArgs<TRFuncDecl>> result;

        auto i = nameMap.find(*name);
        if (i == nameMap.end()) return {};

        for (auto& func : i->second)
            if (explicitTypeParamsExceptOuterCount <= func->RFuncDecl_GetDecl()->GetTypeParamCount())
                result.push_back(TDeclWithOuterTypeArgs<TRFuncDecl>{func, typeArgs});

        return RDeclResType{std::move(result)};

    }*/

};

}