#pragma once

namespace Citron {

struct RIdentifier;
class RDecl;
class RType;
class RDeclRes;
class RTypeArguments;

class RTypeDecl
{
public:
    virtual ~RTypeDecl() = default;

    virtual RIdentifier GetIdentifier() = 0;
    virtual RDecl* GetDecl() = 0;
    virtual RType* GetOpenType() = 0;
    virtual RDeclRes ToRDeclRes(RTypeArguments* typeArgs) = 0;
};

} // namespace Citron
