#pragma once

#include <variant>

namespace Citron {

class NDecl;
struct NFuncDeclOuterVisitor;

class NFuncDeclOuter
{
public:
    virtual ~NFuncDeclOuter() {}
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NFuncDeclOuterVisitor& visitor) = 0;
};

}

// for visitor
namespace Citron {
class NNamespaceDecl;
class NGlobalFuncDecl;
class NClassDecl;
class NClassCtorDecl;
class NClassFuncDecl;
class NStructDecl;
class NStructCtorDecl;
class NStructDtorDecl;
class NStructFuncDecl;
class NLambdaDecl;
}

#include "NFuncDeclOuterVisitor.g.h"