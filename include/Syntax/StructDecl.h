#pragma once
#include "SyntaxConfig.h"

#include "StructMemberDecls.h"

namespace Citron::Syntax {

class StructDecl
{
    std::optional<AccessModifier> accessModifier;
    std::string name;
    std::vector<TypeParam> typeParams;
    std::vector<TypeExp> baseTypes;
    std::vector<StructMemberDecl> memberDecls;

public:
    StructDecl(
        std::optional<AccessModifier> accessModifier, 
        std::string name,
        std::vector<TypeParam> typeParams, 
        std::vector<TypeExp> baseTypes, 
        std::vector<StructMemberDecl> memberDecls)
        : accessModifier(accessModifier)
        , name(name)
        , typeParams(std::move(typeParams))
        , baseTypes(std::move(baseTypes))
        , memberDecls(std::move(memberDecls))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<TypeParam>& GetTypeParams() { return typeParams; }
    std::vector<TypeExp>& GetBaseTypes() { return baseTypes; }
    std::vector<StructMemberDecl>& MemberDecls() { return memberDecls; }

    
};

} // namespace Citron::Syntax