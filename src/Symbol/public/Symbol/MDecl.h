#pragma once

#include <memory>

namespace Citron {

class MNamespaceDecl;
class MGlobalFuncDecl;
class MStructDecl;
class MStructCtorDecl;
class MStructFuncDecl;
class MStructVarDecl;
class MClassDecl;
class MClassCtorDecl;
class MClassFuncDecl;
class MClassVarDecl;
class MEnumDecl;
class MEnumElemDecl;
class MEnumElemVarDecl;
class MInterfaceDecl;

class MDeclVisitor;

class MDecl
{
public:
    virtual ~MDecl() {}
    virtual void Accept(MDeclVisitor& visitor) = 0;
};

using MDeclPtr = std::shared_ptr<MDecl>;

class MDeclVisitor
{
public:
    virtual ~MDeclVisitor() {}
    virtual void Visit(MNamespaceDecl& decl) = 0;
    virtual void Visit(MGlobalFuncDecl& decl) = 0;
    virtual void Visit(MStructDecl& decl) = 0;
    virtual void Visit(MStructCtorDecl& decl) = 0;
    virtual void Visit(MStructFuncDecl& decl) = 0;
    virtual void Visit(MStructVarDecl& decl) = 0;
    virtual void Visit(MClassDecl& decl) = 0;
    virtual void Visit(MClassCtorDecl& decl) = 0;
    virtual void Visit(MClassFuncDecl& decl) = 0;
    virtual void Visit(MClassVarDecl& decl) = 0;
    virtual void Visit(MEnumDecl& decl) = 0;
    virtual void Visit(MEnumElemDecl& decl) = 0;
    virtual void Visit(MEnumElemVarDecl& decl) = 0;
    virtual void Visit(MInterfaceDecl& decl) = 0;
};

}