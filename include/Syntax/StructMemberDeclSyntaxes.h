#pragma once
#include "SyntaxConfig.h"
#include <optional>
#include <string>
#include <vector>
#include <variant>

#include "AccessModifierSyntax.h"
#include "TypeParamSyntax.h"
#include "FuncParamSyntax.h"
#include "StmtSyntaxes.h"

#include "SyntaxMacros.h"

namespace Citron {

// forward declarations
using TypeDeclSyntax = std::variant<class ClassDeclSyntax, class StructDeclSyntax, class EnumDeclSyntax>;

// recursive, { typeDecl }
class StructMemberTypeDeclSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API StructMemberTypeDeclSyntax(TypeDeclSyntax typeDecl);
    DECLARE_DEFAULTS(StructMemberTypeDeclSyntax)

    SYNTAX_API TypeDeclSyntax& GetTypeDecl();

    SYNTAX_API JsonItem ToJson();
};

class StructMemberFuncDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    bool bStatic;
    bool bSequence; // seq 함수인가  
    TypeExpSyntax retType;
    std::u32string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;

public:
    StructMemberFuncDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier,\
        bool bStatic, 
        bool bSequence, 
        TypeExpSyntax retType, 
        std::u32string name, 
        std::vector<TypeParamSyntax> typeParams, 
        std::vector<FuncParamSyntax> parameters, 
        std::vector<StmtSyntax> body)
        : accessModifier(accessModifier)
        , bStatic(bStatic)
        , bSequence(bSequence)
        , retType(std::move(retType))
        , name(std::move(name))
        , typeParams(std::move(typeParams))
        , parameters(std::move(parameters))
        , body(std::move(body))
    {
    }

    std::optional<AccessModifierSyntax> GetAccessModifier() { return accessModifier; }
    bool IsStatic() { return bStatic; }
    bool IsSequence() { return bSequence; } // seq 함수인가  
    TypeExpSyntax& GetRetType() { return retType; }
    std::u32string& Name() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class StructMemberVarDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    TypeExpSyntax varType;
    std::vector<std::u32string> varNames;

public:
    StructMemberVarDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier,
        TypeExpSyntax varType, 
        std::vector<std::u32string> varNames)
        : accessModifier(accessModifier), varType(std::move(varType)), varNames(std::move(varNames))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    TypeExpSyntax& GetVarType() { return varType; }
    std::vector<std::u32string>& GetVarNames() { return varNames; }

    SYNTAX_API JsonItem ToJson();
};

class StructConstructorDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::u32string name;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;

public:
    StructConstructorDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier, 
        std::u32string name,
        std::vector<FuncParamSyntax> parameters, 
        std::vector<StmtSyntax> body)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , parameters(std::move(parameters))
        , body(std::move(body))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::u32string& GetName() { return name; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

using StructMemberDeclSyntax = std::variant<StructMemberTypeDeclSyntax, StructMemberFuncDeclSyntax, StructConstructorDeclSyntax, StructMemberVarDeclSyntax>;
SYNTAX_API JsonItem ToJson(StructMemberDeclSyntax&);

}
