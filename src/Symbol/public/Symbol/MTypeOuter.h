#pragma once

namespace Citron
{

class MModuleDecl;
class MNamespaceDecl;
class MClass;
class MStruct;

class MTypeOuterVisitor
{
public:
    virtual ~MTypeOuterVisitor() { }
    virtual void Visit(MModuleDecl& outer) = 0;
    virtual void Visit(MNamespaceDecl& outer) = 0;
    virtual void Visit(MClass& outer) = 0;
    virtual void Visit(MStruct& outer) = 0;
};

class MTypeOuter
{
public:
    virtual ~MTypeOuter() { }
    virtual void Accept(MTypeOuterVisitor& visitor) = 0;
};

}