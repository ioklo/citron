module;
#include <memory>

export module Citron.MDecls:MFuncDecl;

namespace Citron
{

export class MGlobalFuncDecl;
export class MClassCtorDecl;
export class MClassFuncDecl;
export class MStructCtorDecl;
export class MStructFuncDecl;

export class MFuncDeclVisitor;

export class MFuncDecl
{
public:
    virtual ~MFuncDecl() {}
    virtual void Accept(MFuncDeclVisitor& visitor) = 0;
};

export using MFuncDeclPtr = std::shared_ptr<MFuncDecl>;

export class MFuncDeclVisitor
{
public:
    virtual ~MFuncDeclVisitor() {}
    virtual void Visit(MGlobalFuncDecl& func) = 0;
    virtual void Visit(MClassCtorDecl& func) = 0;
    virtual void Visit(MClassFuncDecl& func) = 0;
    virtual void Visit(MStructCtorDecl& func) = 0;
    virtual void Visit(MStructFuncDecl& func) = 0;
};

export class MFuncDeclOuter
{
public:
    ~MFuncDeclOuter() = default;
};

}
