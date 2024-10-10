#pragma once
#include <memory>

namespace Citron {

class RDecl;
class RModuleDecl;
class RNamespaceDecl;

class RTopLevelDeclOuterVisitor
{
public:
    virtual ~RTopLevelDeclOuterVisitor() { }
    virtual void Visit(RModuleDecl& outerDecl) = 0;
    virtual void Visit(RNamespaceDecl& outerDecl) = 0;
};

class RTopLevelDeclOuter
{
public:
    virtual ~RTopLevelDeclOuter() { }
    virtual RDecl* GetDecl() = 0;
    virtual void Accept(RTopLevelDeclOuterVisitor& visitor) = 0;
};

using RTopLevelDeclOuterPtr = std::weak_ptr<RTopLevelDeclOuter>;

}