export module Citron.MSymbol:MTypeDecl;

import :MDecl;

namespace Citron {

export class MClassDecl;
export class MStructDecl;
export class MEnumDecl;
export class MEnumElemDecl;
export class MInterfaceDecl;


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

export class MTypeDecl
{
public:
    virtual ~MTypeDecl() {}
    virtual void Accept(MTypeDeclVisitor& visitor) = 0;
};

}