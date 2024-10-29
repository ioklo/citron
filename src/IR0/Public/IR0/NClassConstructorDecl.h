#pragma once
#include <vector>
#include <memory>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron
{

class NClassDecl;
struct RFuncParameter;

class NClassConstructorDecl 
    : public NFuncDecl
    , public NFuncDeclOuter
    , private NCommonFuncDeclComponent
{
public:
    std::weak_ptr<NClassDecl> _class;
    RAccessor accessor;
    std::vector<RFuncParameter> parameters;
    bool bTrivial;

public: 
    // from NDecl
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}