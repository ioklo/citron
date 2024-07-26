#pragma once

#include <memory>

namespace Citron
{

class RModuleDecl;
class RNamespaceDecl;
class RClass;
class RStruct;

class RTypeOuterVisitor
{
public:
    virtual ~RTypeOuterVisitor() { }
    virtual void Visit(RModuleDecl& outer) = 0;
    virtual void Visit(RNamespaceDecl& outer) = 0;
    virtual void Visit(RClass& outer) = 0;
    virtual void Visit(RStruct& outer) = 0;
};

class RTypeOuter
{
public:
    virtual ~RTypeOuter() { }
    virtual void Accept(RTypeOuterVisitor& visitor) = 0;
};

using RTypeOuterPtr = std::weak_ptr<RTypeOuter>;

}