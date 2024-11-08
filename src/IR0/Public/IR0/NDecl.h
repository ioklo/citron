#pragma once

#include <memory>
#include <optional>
#include "RIdentifier.h"
#include "RNames.h"
#include "RAccessor.h"
#include "RDecl.h"

namespace Citron
{

class NModule;
class NNamespaceDecl;
class NGlobalFuncDecl;
class NStructDecl;
class NStructConstructorDecl;
class NStructMemberFuncDecl;
class NStructMemberVarDecl;
class NClassDecl;
class NClassConstructorDecl;
class NClassMemberFuncDecl;
class NClassMemberVarDecl;
class NEnumDecl;
class NEnumElemDecl;
class NEnumElemMemberVarDecl;
class NLambdaDecl;
class NLambdaMemberVarDecl;
class NInterfaceDecl;

class RTypeFactory;

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

class NDeclVisitor
{
public:
    virtual ~NDeclVisitor() { }
    virtual void Visit(NNamespaceDecl& decl) = 0;
    virtual void Visit(NGlobalFuncDecl& decl) = 0;
    virtual void Visit(NStructDecl& decl) = 0;
    virtual void Visit(NStructConstructorDecl& decl) = 0;
    virtual void Visit(NStructMemberFuncDecl& decl) = 0;
    virtual void Visit(NStructMemberVarDecl& decl) = 0;
    virtual void Visit(NClassDecl& decl) = 0;
    virtual void Visit(NClassConstructorDecl& decl) = 0;
    virtual void Visit(NClassMemberFuncDecl& decl) = 0;
    virtual void Visit(NClassMemberVarDecl& decl) = 0;
    virtual void Visit(NEnumDecl& decl) = 0;
    virtual void Visit(NEnumElemDecl& decl) = 0;
    virtual void Visit(NEnumElemMemberVarDecl& decl) = 0;
    virtual void Visit(NLambdaDecl& decl) = 0;
    virtual void Visit(NLambdaMemberVarDecl& decl) = 0;
    virtual void Visit(NInterfaceDecl& decl) = 0;
};

class NDecl
{
public:
    virtual ~NDecl() { }
    
    virtual RDecl* GetRDecl() = 0;
    virtual NDecl* GetNOuter() = 0;
    virtual void Accept(NDeclVisitor& visitor) = 0;
};

using NDeclPtr = std::shared_ptr<NDecl>;

}