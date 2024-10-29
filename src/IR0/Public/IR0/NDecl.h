#pragma once

#include <memory>
#include "RIdentifier.h"
#include "RNames.h"
#include "RAccessor.h"
#include "RDecl.h"

namespace Citron
{

class NModuleDecl;
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

using RMemberPtr = std::shared_ptr<class RMember>;
using RDeclIdPtr = std::shared_ptr<class RDeclId>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

class NDeclVisitor
{
public:
    virtual ~NDeclVisitor() { }
    virtual void Visit(NModuleDecl& decl) = 0;
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
    virtual RAccessor GetAccessor() = 0;
    virtual RIdentifier GetIdentifier() = 0;
    virtual NDecl* GetOuter() = 0;

    // typeArgs는 RDecl의 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 검색할 멤버가 추가로 가지고 있을 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 확정적으로 알고 있는 typeArgs의 개수이다. 
    // 함수는 모든 typeArgs를 나열하지 않아도 type inference로 채울 수 있기 때문에,
    // explicitTypeParamsExceptOuterCount보다 더 많은 typeParams을 갖고 있어도 결과에 반영된다
    virtual RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) = 0;
    virtual std::string GetModuleName(); // once overridden by RModuleDecl    
    virtual void Accept(NDeclVisitor& visitor) = 0;

public:
    bool IsDescendantOf(NDecl* container);
    bool CanAccess(NDecl* target);

private:
    // non virtual
    RDeclIdPtr GetDeclId(RTypeFactory& factory);
};

using NDeclPtr = std::shared_ptr<NDecl>;

}