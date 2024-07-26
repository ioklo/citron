#pragma once

#include <memory>

namespace Citron
{

class RModuleDecl;
class RNamespaceDecl;

class RTopLevelOuterVisitor
{
public:
    virtual ~RTopLevelOuterVisitor() { }
    virtual void Visit(RModuleDecl& outer) = 0;
    virtual void Visit(RNamespaceDecl& outer) = 0;
};

class RTopLevelOuter
{
public:
    virtual ~RTopLevelOuter() { }
    virtual void Accept(RTopLevelOuterVisitor& visitor) = 0;
};

using RTopLevelOuterPtr = std::weak_ptr<RTopLevelOuter>;

}