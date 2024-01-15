#pragma once
#include "SyntaxConfig.h"
#include <optional>
#include <string>
#include <vector>
#include <variant>

#include "AccessModifier.h"
#include "TypeParam.h"
#include "FuncParam.h"
#include "Stmts.h"

#include "SyntaxMacros.h"

namespace Citron::Syntax {

// forward declarations
using TypeDecl = std::variant<class ClassDecl, class StructDecl, class EnumDecl>;

// recursive, { typeDecl }
class StructMemberTypeDecl
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API StructMemberTypeDecl(TypeDecl typeDecl);
    DECLARE_DEFAULTS(StructMemberTypeDecl)

    SYNTAX_API TypeDecl& GetTypeDecl();
};

class StructMemberFuncDecl
{
    std::optional<AccessModifier> accessModifier;
    bool bStatic;
    bool bSequence; // seq 함수인가  
    TypeExp retType;
    std::string name;
    std::vector<TypeParam> typeParams;
    std::vector<FuncParam> parameters;
    std::vector<Stmt> body;

public:
    StructMemberFuncDecl(
        std::optional<AccessModifier> accessModifier,\
        bool bStatic, 
        bool bSequence, 
        TypeExp retType, 
        std::string name, 
        std::vector<TypeParam> typeParams, 
        std::vector<FuncParam> parameters, 
        std::vector<Stmt> body)
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

    std::optional<AccessModifier> GetAccessModifier() { return accessModifier; }
    bool IsStatic() { return bStatic; }
    bool IsSequence() { return bSequence; } // seq 함수인가  
    TypeExp& GetRetType() { return retType; }
    std::string& Name() { return name; }
    std::vector<TypeParam>& GetTypeParams() { return typeParams; }
    std::vector<FuncParam>& GetParameters() { return parameters; }
    std::vector<Stmt>& GetBody() { return body; }
};

class StructMemberVarDecl
{
    std::optional<AccessModifier> accessModifier;
    TypeExp varType;
    std::vector<std::string> varNames;

public:
    StructMemberVarDecl(
        std::optional<AccessModifier> accessModifier,
        TypeExp varType, 
        std::vector<std::string> varNames)
        : accessModifier(accessModifier), varType(std::move(varType)), varNames(std::move(varNames))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier; }
    TypeExp& GetVarType() { return varType; }
    std::vector<std::string>& GetVarNames() { return varNames; }
};

class StructConstructorDecl
{
    std::optional<AccessModifier> accessModifier;
    std::string name;
    std::vector<FuncParam> parameters;
    std::vector<Stmt> body;

public:
    StructConstructorDecl(
        std::optional<AccessModifier> accessModifier, 
        std::string name,
        std::vector<FuncParam> parameters, 
        std::vector<Stmt> body)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , parameters(std::move(parameters))
        , body(std::move(body))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<FuncParam>& GetParameters() { return parameters; }
    std::vector<Stmt>& GetBody() { return body; }
};

using StructMemberDecl = std::variant<StructMemberTypeDecl, StructMemberFuncDecl, StructConstructorDecl, StructMemberVarDecl>;

}
