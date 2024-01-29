#pragma once
#include "SyntaxConfig.h"

#include <Infra/Json.h>
#include "StructMemberDeclSyntaxes.h"

namespace Citron {

class StructDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::u32string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<TypeExpSyntax> baseTypes;
    std::vector<StructMemberDeclSyntax> memberDecls;

public:
    StructDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier, 
        std::u32string name,
        std::vector<TypeParamSyntax> typeParams, 
        std::vector<TypeExpSyntax> baseTypes, 
        std::vector<StructMemberDeclSyntax> memberDecls)
        : accessModifier(accessModifier)
        , name(name)
        , typeParams(std::move(typeParams))
        , baseTypes(std::move(baseTypes))
        , memberDecls(std::move(memberDecls))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::u32string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<TypeExpSyntax>& GetBaseTypes() { return baseTypes; }
    std::vector<StructMemberDeclSyntax>& MemberDecls() { return memberDecls; }

    SYNTAX_API JsonItem ToJson();
};

} // namespace Citron