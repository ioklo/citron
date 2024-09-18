#pragma once

#include <memory>

namespace Citron
{

class MModuleDecl;
class MNamespaceDecl;
class MGlobalFuncDecl;
class MStructDecl;
class MStructConstructorDecl;
class MStructMemberFuncDecl;
class MStructMemberVarDecl;
class MClassDecl;
class MClassConstructorDecl;
class MClassMemberFuncDecl;
class MClassMemberVarDecl;
class MEnumDecl;
class MEnumElemDecl;
class MEnumElemMemberVarDecl;
class MInterfaceDecl;

class MDeclVisitor
{
public:
    virtual ~MDeclVisitor() { }
    virtual void Visit(MModuleDecl& decl) = 0;
    virtual void Visit(MNamespaceDecl& decl) = 0;
    virtual void Visit(MGlobalFuncDecl& decl) = 0;
    virtual void Visit(MStructDecl& decl) = 0;
    virtual void Visit(MStructConstructorDecl& decl) = 0;
    virtual void Visit(MStructMemberFuncDecl& decl) = 0;
    virtual void Visit(MStructMemberVarDecl& decl) = 0;
    virtual void Visit(MClassDecl& decl) = 0;
    virtual void Visit(MClassConstructorDecl& decl) = 0;
    virtual void Visit(MClassMemberFuncDecl& decl) = 0;
    virtual void Visit(MClassMemberVarDecl& decl) = 0;
    virtual void Visit(MEnumDecl& decl) = 0;
    virtual void Visit(MEnumElemDecl& decl) = 0;
    virtual void Visit(MEnumElemMemberVarDecl& decl) = 0;
    virtual void Visit(MInterfaceDecl& decl) = 0;
};

class MDecl
{
public:
    virtual ~MDecl() { }
    virtual void Accept(MDeclVisitor& visitor) = 0;
};

using MDeclPtr = std::shared_ptr<MDecl>;

}