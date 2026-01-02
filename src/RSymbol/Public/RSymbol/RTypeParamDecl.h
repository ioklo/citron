#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class RTypeParamDecl 
    : public RDecl
    , public RTypeDecl
{
public:
    virtual ~RTypeParamDecl() = default;
    virtual size_t GetGlobalIndex() = 0;

public: // from RDecl
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }

public: // from RTypeDecl
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(this); }
};


} // namespace Citron
