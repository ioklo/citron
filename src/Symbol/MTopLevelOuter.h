#pragma once

class MModuleDecl;
class MNamespaceDecl;

namespace Citron
{

class MTopLevelOuterVisitor
{
public:
    virtual void Visit(MModuleDecl& outer) = 0;
    virtual void Visit(MNamespaceDecl& outer) = 0;
};

class MTopLevelOuter
{
public:
    virtual void Accept(MTopLevelOuterVisitor& visitor) = 0;
};

}