#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <optional>
#include "AccessModifier.h"
#include "TypeExps.h"
#include "TypeParam.h"

namespace Citron::Syntax {

class EnumElemMemberVarDecl
{
    TypeExp type;
    std::string name;

public:
    EnumElemMemberVarDecl(TypeExp type, std::string name)
        : type(std::move(type)), name(std::move(name)) { }

    TypeExp& GetType() { return type; }
    std::string& GetName() { return name; }
};

class EnumElemDecl
{
    std::string name;
    std::vector<EnumElemMemberVarDecl> memberVars;

public:
    EnumElemDecl(std::string name, std::vector<EnumElemMemberVarDecl> memberVars)
        : name(std::move(name)), memberVars(std::move(memberVars)) { }

    std::string& GetName() { return name; }
    std::vector<EnumElemMemberVarDecl>& GetMemberVars() { return memberVars; }
};
    
class EnumDecl
{
    std::optional<AccessModifier> accessModifier;
    std::string name;
    std::vector<TypeParam> typeParams;
    std::vector<EnumElemDecl> elems;

public:
    EnumDecl(
        std::optional<AccessModifier> accessModifier,
        std::string name,
        std::vector<TypeParam> typeParams,
        std::vector<EnumElemDecl> elems)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , typeParams(std::move(typeParams))
        , elems(std::move(elems))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<TypeParam>& GetTypeParams() { return typeParams; }
    std::vector<EnumElemDecl>& GetElems() { return elems; }
};

} // namespace Citron::Syntax
