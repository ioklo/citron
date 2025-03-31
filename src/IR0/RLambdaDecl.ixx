export module Citron.RDecls:RLambdaDecl;

import <memory>;

import :RDecl;
import :RFuncDecl;
import :RFuncDeclOuter;
import :RTypeDecl;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

export class RLambdaDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
    , public RTypeDecl
{
public:
    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
};

// M버전이 없다

} // namespace Citron
