#pragma once

namespace Citron {

struct RIdentifier;
class RDecl;
class RType;
class RTypeArguments;
class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;
class RTypeParam;
class RTraitDecl;
struct RTypeDeclVisitor;

class RTypeDecl
{
public:
    virtual ~RTypeDecl() = default;
    
    virtual RDecl* RTypeDecl_GetDecl() = 0;
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

} // namespace Citron

#include "RTypeDeclVisitor.g.h"