#pragma once


#include "RDecl.h"

namespace Citron {

class ETypeDecl;

struct RTypeDeclVisitor;

class RTypeDecl
{
public:
    virtual ~RTypeDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

class RETypeDecl : public RTypeDecl
{
    ETypeDecl* typeDecl;
};

} // namespace Citron

#include "RTypeDeclVisitor.g.h"