#pragma once

#include <memory>

namespace Citron {

class MTopLevelDeclOuter;
class RTopLevelDeclOuterVisitor;
class RModuleDecl;
class RNamespaceDecl;

class RTopLevelDeclOuter
{
public:
    virtual ~RTopLevelDeclOuter() { }
    virtual void Accept(RTopLevelDeclOuterVisitor& visitor) = 0;
};

class RTopLevelDeclOuterVisitor
{
public:
    virtual ~RTopLevelDeclOuterVisitor() { }
    virtual void Visit(RModuleDecl& outerDecl) = 0;
    virtual void Visit(RNamespaceDecl& outerDecl) = 0;
};

class RMTopLevelDeclOuter : public RTopLevelDeclOuter
{
    std::shared_ptr<MTopLevelDeclOuter> outer;
};


} // namespace Citron