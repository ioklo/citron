#pragma once
#include "SyntaxConfig.h"

#include <optional>
#include <string>
#include <vector>
#include <variant>

#include "ArgumentSyntax.h"
#include "AccessModifierSyntax.h"
#include "TypeParamSyntax.h"
#include "FuncParamSyntax.h"
#include "StmtSyntaxes.h"


namespace Citron {

// forward declarations
using TypeDeclSyntax = std::variant<class ClassDeclSyntax, class StructDeclSyntax, class EnumDeclSyntax>;

// recursive, { typeDecl }
class ClassMemberTypeDeclSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ClassMemberTypeDeclSyntax(TypeDeclSyntax typeDecl);
    DECLARE_DEFAULTS(ClassMemberTypeDeclSyntax)

    SYNTAX_API TypeDeclSyntax& GetTypeDecl();
};

class ClassMemberFuncDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    bool bStatic;
    bool bSequence;
    TypeExpSyntax retType;
    std::u32string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;

public:
    ClassMemberFuncDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier, 
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
    bool IsSequence() { return bSequence;  }
    TypeExpSyntax& RetType() { return retType;  }
    std::u32string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }
};

class ClassConstructorDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::u32string name;
    std::vector<FuncParamSyntax> parameters;
    std::optional<std::vector<ArgumentSyntax>> baseArgs;
    std::vector<StmtSyntax> body;

public:
    ClassConstructorDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier,
        std::u32string name,
        std::vector<FuncParamSyntax> parameters,
        std::optional<std::vector<ArgumentSyntax>> baseArgs,
        std::vector<StmtSyntax> body)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , parameters(std::move(parameters))
        , baseArgs(std::move(baseArgs))
        , body(std::move(body))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier;  }
    std::u32string& GetName() { return name; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::optional<std::vector<ArgumentSyntax>>& GetBaseArgs() { return baseArgs; }
    std::vector<StmtSyntax>& GetBody() { return body; }
};

class ClassMemberVarDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    TypeExpSyntax varType;
    std::vector<std::u32string> varNames;

public:
    ClassMemberVarDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier,
        TypeExpSyntax varType,
        std::vector<std::u32string> varNames)
        : accessModifier(accessModifier)
        , varType(std::move(varType))
        , varNames(std::move(varNames))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    TypeExpSyntax& GetVarType() { return varType; }
    std::vector<std::u32string>& GetVarNames() { return varNames; }
};

using ClassMemberDeclSyntax = std::variant<ClassMemberTypeDeclSyntax, ClassConstructorDeclSyntax, ClassMemberFuncDeclSyntax, ClassMemberVarDeclSyntax>;

} // namespace Citron