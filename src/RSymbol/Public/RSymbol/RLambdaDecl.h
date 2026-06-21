#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class RType;
class RFactory;

class RLambdaDecl
    : public RDecl
    , public RFuncDecl
    , public RTypeDecl
{
public:
    virtual RType* GetReturnType(RTypeArguments* typeArgs) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RFuncDeclVisitor& visitor) final;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

// M버전이 없다

} // namespace Citron
