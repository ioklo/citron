#pragma once
#include "SyntaxConfig.h"

#include <optional>
#include <string>
#include <vector>

#include <Infra/Json.h>
#include "AccessModifierSyntax.h"
#include "TypeParamSyntax.h"
#include "TypeExpSyntaxes.h"
#include "ClassMemberDeclSyntaxes.h"

namespace Citron {

class ClassDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::u32string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<TypeExpSyntax> baseTypes;
    std::vector<ClassMemberDeclSyntax> memberDecls;

public:

    ClassDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier,
        std::u32string name,
        std::vector<TypeParamSyntax> typeParams,
        std::vector<TypeExpSyntax> baseTypes,
        std::vector<ClassMemberDeclSyntax> memberDecls)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , typeParams(std::move(typeParams))
        , baseTypes(std::move(baseTypes))
        , memberDecls(std::move(memberDecls))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::u32string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<TypeExpSyntax>& GetBaseTypes() { return baseTypes; }
    std::vector<ClassMemberDeclSyntax>& GetMemberDecls() { return memberDecls; }

    SYNTAX_API JsonItem ToJson();
};

}

