#pragma once

class MModuleDecl;
class MNamespaceDecl;

namespace Citron
{

class MTopLevelOuterVisitor
{
public:
    virtual ~MTopLevelOuterVisitor() { }
    virtual void Visit(MModuleDecl& outer) = 0;
    virtual void Visit(MNamespaceDecl& outer) = 0;
};

class MTopLevelOuter
{
public:
    virtual ~MTopLevelOuter() { }
    virtual void Accept(MTopLevelOuterVisitor& visitor) = 0;
};

using MTopLevelOuterPtr = std::weak_ptr<MTopLevelOuter>;

}