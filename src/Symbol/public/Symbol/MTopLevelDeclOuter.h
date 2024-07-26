#pragma once
// circular dependency 때문에, import 하지는 못한다
// #include "ModuleDeclSymbol.h"

namespace Citron {

class MModuleDecl;
class MNamespaceDecl;

class MTopLevelDeclOuterVisitor
{
public:
    virtual ~MTopLevelDeclOuterVisitor() { }
    virtual void Visit(MModuleDecl& outerDecl) = 0;
    virtual void Visit(MNamespaceDecl& outerDecl) = 0;
};

class MTopLevelDeclOuter
{
public:
    virtual ~MTopLevelDeclOuter() { }
    virtual void Accept(MTopLevelDeclOuterVisitor& visitor) = 0;
};

using MTopLevelDeclOuterPtr = std::weak_ptr<MTopLevelDeclOuter>;

}