export module Citron.NDecls:NTypeDeclContainerComponent;

import "IR0Config.h";

import <vector>;
import <unordered_map>;
import <optional>;

import Citron.RDecls;
import :NTypeDecl;

namespace Citron {

export class NTypeDeclContainerComponent
{
public:
    std::vector<NTypeDeclPtr> types;
    std::unordered_map<RIdentifier, NTypeDeclPtr> typeDict;

public:
    IR0_API NTypeDeclContainerComponent();

    IR0_API size_t GetTypeCount();
    IR0_API NTypeDeclPtr GetType(int index);
    IR0_API NTypeDeclPtr GetType(const RIdentifier& identifier);
    IR0_API void AddType(NTypeDeclPtr&& typeDecl);

    // internal
    std::optional<RMember> GetMemberType(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

}