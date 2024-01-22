#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <optional>
#include "AccessModifierSyntax.h"
#include "TypeExpSyntaxes.h"
#include "TypeParamSyntax.h"

namespace Citron {

class EnumElemMemberVarDeclSyntax
{
    TypeExpSyntax type;
    std::u32string name;

public:
    EnumElemMemberVarDeclSyntax(TypeExpSyntax type, std::u32string name)
        : type(std::move(type)), name(std::move(name)) { }

    TypeExpSyntax& GetType() { return type; }
    std::u32string& GetName() { return name; }
};

class EnumElemDeclSyntax
{
    std::u32string name;
    std::vector<EnumElemMemberVarDeclSyntax> memberVars;

public:
    EnumElemDeclSyntax(std::u32string name, std::vector<EnumElemMemberVarDeclSyntax> memberVars)
        : name(std::move(name)), memberVars(std::move(memberVars)) { }

    std::u32string& GetName() { return name; }
    std::vector<EnumElemMemberVarDeclSyntax>& GetMemberVars() { return memberVars; }
};
    
class EnumDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::u32string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<EnumElemDeclSyntax> elems;

public:
    EnumDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier,
        std::u32string name,
        std::vector<TypeParamSyntax> typeParams,
        std::vector<EnumElemDeclSyntax> elems)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , typeParams(std::move(typeParams))
        , elems(std::move(elems))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::u32string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<EnumElemDeclSyntax>& GetElems() { return elems; }
};

} // namespace Citron
