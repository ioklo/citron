#pragma once

#include <memory>
#include "RIdentifier.h"
#include "RNames.h"

namespace Citron
{

class RModuleDecl;
class RNamespaceDecl;
class RGlobalFuncDecl;
class RStructDecl;
class RStructConstructorDecl;
class RStructMemberFuncDecl;
class RStructMemberVarDecl;
class RClassDecl;
class RClassConstructorDecl;
class RClassMemberFuncDecl;
class RClassMemberVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemMemberVarDecl;
class RLambdaDecl;
class RLambdaMemberVarDecl;
class RInterfaceDecl;

class RMember;
using RMemberPtr = std::shared_ptr<RMember>;

class RDeclId;
using RDeclIdPtr = std::shared_ptr<RDeclId>;

class RTypeFactory;
class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class RDeclVisitor
{
public:
    virtual ~RDeclVisitor() { }
    virtual void Visit(RModuleDecl& decl) = 0;
    virtual void Visit(RNamespaceDecl& decl) = 0;
    virtual void Visit(RGlobalFuncDecl& decl) = 0;
    virtual void Visit(RStructDecl& decl) = 0;
    virtual void Visit(RStructConstructorDecl& decl) = 0;
    virtual void Visit(RStructMemberFuncDecl& decl) = 0;
    virtual void Visit(RStructMemberVarDecl& decl) = 0;
    virtual void Visit(RClassDecl& decl) = 0;
    virtual void Visit(RClassConstructorDecl& decl) = 0;
    virtual void Visit(RClassMemberFuncDecl& decl) = 0;
    virtual void Visit(RClassMemberVarDecl& decl) = 0;
    virtual void Visit(REnumDecl& decl) = 0;
    virtual void Visit(REnumElemDecl& decl) = 0;
    virtual void Visit(REnumElemMemberVarDecl& decl) = 0;
    virtual void Visit(RLambdaDecl& decl) = 0;
    virtual void Visit(RLambdaMemberVarDecl& decl) = 0;
    virtual void Visit(RInterfaceDecl& decl) = 0;
};

class RDecl
{
public:
    virtual ~RDecl() { }
    virtual RIdentifier GetIdentifier() = 0;
    virtual RDecl* GetOuter() = 0;

    // typeArgs는 RDecl의 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 검색할 멤버가 추가로 가지고 있을 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 확정적으로 알고 있는 typeArgs의 개수이다. 
    // 함수는 모든 typeArgs를 나열하지 않아도 type inference로 채울 수 있기 때문에,
    // explicitTypeParamsExceptOuterCount보다 더 많은 typeParams을 갖고 있어도 결과에 반영된다
    virtual RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) = 0;
    virtual void Accept(RDeclVisitor& visitor) = 0;

    // once overridden by RModuleDecl
    virtual std::string GetModuleName();

private:
    // non virtual
    RDeclIdPtr GetDeclId(RTypeFactory& factory);
};


using RDeclPtr = std::shared_ptr<RDecl>;

}