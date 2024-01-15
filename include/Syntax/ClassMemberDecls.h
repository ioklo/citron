#pragma once
#include "SyntaxConfig.h"

#include <optional>
#include <string>
#include <vector>
#include <variant>

#include "Argument.h"
#include "AccessModifier.h"
#include "TypeParam.h"
#include "FuncParam.h"
#include "Stmts.h"


namespace Citron::Syntax {

using TypeDecl = std::variant<class ClassDecl, class StructDecl, class EnumDecl>;

// recursive, { typeDecl }
class ClassMemberTypeDecl
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ClassMemberTypeDecl(TypeDecl typeDecl);
    DECLARE_DEFAULTS(ClassMemberTypeDecl)

    SYNTAX_API TypeDecl& GetTypeDecl();
};

class ClassMemberFuncDecl
{
    std::optional<AccessModifier> accessModifier;
    bool bStatic;
    bool bSequence;
    TypeExp retType;
    std::string name;
    std::vector<TypeParam> typeParams;
    std::vector<FuncParam> parameters;
    std::vector<Stmt> body;

public:
    ClassMemberFuncDecl(
        std::optional<AccessModifier> accessModifier, 
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
    bool IsSequence() { return bSequence;  }
    TypeExp& RetType() { return retType;  }
    std::string& GetName() { return name; }
    std::vector<TypeParam>& GetTypeParams() { return typeParams; }
    std::vector<FuncParam>& GetParameters() { return parameters; }
    std::vector<Stmt>& GetBody() { return body; }
};

class ClassConstructorDecl
{
    std::optional<AccessModifier> accessModifier;
    std::string name;
    std::vector<FuncParam> parameters;
    std::optional<std::vector<Argument>> baseArgs;
    std::vector<Stmt> body;

public:
    ClassConstructorDecl(
        std::optional<AccessModifier> accessModifier,
        std::string name,
        std::vector<FuncParam> parameters,
        std::optional<std::vector<Argument>> baseArgs,
        std::vector<Stmt> body)
        : accessModifier(accessModifier)
        , name(std::move(name))
        , parameters(std::move(parameters))
        , baseArgs(std::move(baseArgs))
        , body(std::move(body))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier;  }
    std::string& GetName() { return name; }
    std::vector<FuncParam>& GetParameters() { return parameters; }
    std::optional<std::vector<Argument>>& GetBaseArgs() { return baseArgs; }
    std::vector<Stmt>& GetBody() { return body; }
};

class ClassMemberVarDecl
{
    std::optional<AccessModifier> accessModifier;
    TypeExp varType;
    std::vector<std::string> varNames;

public:
    ClassMemberVarDecl(
        std::optional<AccessModifier> accessModifier,
        TypeExp varType,
        std::vector<std::string> varNames)
        : accessModifier(accessModifier)
        , varType(std::move(varType))
        , varNames(std::move(varNames))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier; }
    TypeExp& GetVarType() { return varType; }
    std::vector<std::string>& GetVarNames() { return varNames; }
};

using ClassMemberDecl = std::variant<ClassMemberTypeDecl, ClassConstructorDecl, ClassMemberFuncDecl, ClassMemberVarDecl>;

} // namespace Citron::Syntax