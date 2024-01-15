#pragma once
#include "SyntaxConfig.h"

#include <optional>
#include <string>
#include <vector>

#include "AccessModifier.h"
#include "TypeParam.h"
#include "TypeExps.h"
#include "ClassMemberDecls.h"

namespace Citron::Syntax {

class ClassDecl
{
    std::optional<AccessModifier> accessModifier;
    std::string name;
    std::vector<TypeParam> typeParams;
    std::vector<TypeExp> baseTypes;
    std::vector<ClassMemberDecl> memberDecls;

public:

    ClassDecl(
        std::optional<AccessModifier> accessModifier,
        std::string name,
        std::vector<TypeParam> typeParams,
        std::vector<TypeExp> baseTypes,
        std::vector<ClassMemberDecl> memberDecls)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , typeParams(std::move(typeParams))
        , baseTypes(std::move(baseTypes))
        , memberDecls(std::move(memberDecls))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<TypeParam>& GetTypeParams() { return typeParams; }
    std::vector<TypeExp>& GetBaseTypes() { return baseTypes; }
    std::vector<ClassMemberDecl>& GetMemberDecls() { return memberDecls; }
};

}

