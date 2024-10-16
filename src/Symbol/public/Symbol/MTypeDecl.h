#pragma once

namespace Citron {

class MClassDecl;
class MStructDecl;
class MEnumDecl;
class MEnumElemDecl;
class MInterfaceDecl;

class MTypeDeclVisitor
{
public:
    virtual ~MTypeDeclVisitor() { }
    virtual void Visit(MClassDecl& typeDecl) = 0;
    virtual void Visit(MStructDecl& typeDecl) = 0;
    virtual void Visit(MEnumDecl& typeDecl) = 0;
    virtual void Visit(MEnumElemDecl& typeDecl) = 0;
    virtual void Visit(MInterfaceDecl& typeDecl) = 0;
};

class MTypeDecl
{
public:
    virtual ~MTypeDecl() { }
    virtual void Accept(MTypeDeclVisitor& visitor) = 0;
};

}