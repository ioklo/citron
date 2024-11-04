#pragma once
#include <memory>
#include "RTopLevelDeclOuter.h"

namespace Citron {

class NDecl;
class NModuleDecl;
class NNamespaceDecl;

class NTopLevelDeclOuterVisitor
{
public:
    virtual ~NTopLevelDeclOuterVisitor() { }
    virtual void Visit(NModuleDecl& outerDecl) = 0;
    virtual void Visit(NNamespaceDecl& outerDecl) = 0;
};

class NTopLevelDeclOuter : public RTopLevelDeclOuter
{
public:
    virtual ~NTopLevelDeclOuter() { }
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NTopLevelDeclOuterVisitor& visitor) = 0;
};

using NTopLevelDeclOuterWPtr = std::weak_ptr<NTopLevelDeclOuter>;

}