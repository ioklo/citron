#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>

#include "AccessModifier.h"
#include "TypeExps.h"
#include "TypeParam.h"
#include "FuncParam.h"
#include "Stmts.h"

namespace Citron::Syntax {

class GlobalFuncDecl
{
    std::optional<AccessModifier> accessModifier;
    bool bSequence; // seq 함수인가        
    TypeExp retType;
    std::string name;
    std::vector<TypeParam> typeParams;
    std::vector<FuncParam> parameters;
    std::vector<Stmt> body;


public:
    GlobalFuncDecl(
        std::optional<AccessModifier> accessModifier, 
        bool bSequence, 
        TypeExp retType, 
        std::string name, 
        std::vector<TypeParam> typeParams,
        std::vector<FuncParam> parameters, 
        std::vector<Stmt> body)
        : accessModifier(accessModifier)
        , bSequence(bSequence)
        , retType(std::move(retType))
        , name(std::move(name))
        , typeParams(std::move(typeParams))
        , parameters(std::move(parameters))
        , body(std::move(body))
    {
    }

    std::optional<AccessModifier>& GetAccessModifier() { return accessModifier; }
    bool IsSequence() { return bSequence; } // seq 함수인가        
    TypeExp& GetRetType() { return retType; }
    std::string& GetName() { return name; }
    std::vector<TypeParam>& GetTypeParams() { return typeParams; }
    std::vector<FuncParam>& GetParameters() { return parameters; }
    std::vector<Stmt>& GetBody() { return body; }    
};


} // namespace Citron::Syntax
