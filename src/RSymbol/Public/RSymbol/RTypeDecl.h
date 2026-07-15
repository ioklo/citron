#pragma once

namespace Citron {

struct RIdentifier;
class RDecl;
class RType;
class RDeclRes;
class RTypeArguments;
class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;
class RTypeParamDecl;
class RTraitDecl;
struct RTypeDeclVisitor;

class RTypeDecl
{
public:
    virtual ~RTypeDecl() = default;
    
    virtual RDecl* RTypeDecl_GetDecl() = 0;
    virtual RType* GetOpenType() = 0;
    virtual RDeclRes ToRDeclRes(RTypeArguments* typeArgs) = 0;
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

} // namespace Citron

#include "RTypeDeclVisitor.g.h"