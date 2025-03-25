export module Citron.MSymbol:MDecl;

import <memory>;

namespace Citron
{

export class MDeclVisitor;

export class MDecl
{
public:
    virtual ~MDecl() {}
    virtual void Accept(MDeclVisitor& visitor) = 0;
};

export using MDeclPtr = std::shared_ptr<MDecl>;

}