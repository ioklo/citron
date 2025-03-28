export module Citron.MSymbol:MTypeDecl;

import :ForwardDecls;
import :MDecl;

namespace Citron {

// 같은 unit내에서 forward declaration
class MTypeDeclVisitor;

export class MTypeDecl
{
public:
    virtual ~MTypeDecl() {}
    virtual void Accept(MTypeDeclVisitor& visitor) = 0;
};

export class MTypeDeclVisitor
{
public:
    virtual ~MTypeDeclVisitor() {}
    virtual void Visit(MClassDecl& typeDecl) = 0;
    virtual void Visit(MStructDecl& typeDecl) = 0;
    virtual void Visit(MEnumDecl& typeDecl) = 0;
    virtual void Visit(MEnumElemDecl& typeDecl) = 0;
    virtual void Visit(MInterfaceDecl& typeDecl) = 0;
};

}