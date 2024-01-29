#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>
#include <Infra/Json.h>

#include "AccessModifierSyntax.h"
#include "TypeExpSyntaxes.h"
#include "TypeParamSyntax.h"
#include "FuncParamSyntax.h"
#include "StmtSyntaxes.h"

namespace Citron {

class GlobalFuncDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    bool bSequence; // seq 함수인가        
    TypeExpSyntax retType;
    std::u32string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;


public:
    GlobalFuncDeclSyntax(
        std::optional<AccessModifierSyntax> accessModifier, 
        bool bSequence, 
        TypeExpSyntax retType, 
        std::u32string name, 
        std::vector<TypeParamSyntax> typeParams,
        std::vector<FuncParamSyntax> parameters, 
        std::vector<StmtSyntax> body)
        : accessModifier(accessModifier)
        , bSequence(bSequence)
        , retType(std::move(retType))
        , name(std::move(name))
        , typeParams(std::move(typeParams))
        , parameters(std::move(parameters))
        , body(std::move(body))
    {
    }

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    bool IsSequence() { return bSequence; } // seq 함수인가        
    TypeExpSyntax& GetRetType() { return retType; }
    std::u32string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }   

    SYNTAX_API JsonItem ToJson();
};


} // namespace Citron
