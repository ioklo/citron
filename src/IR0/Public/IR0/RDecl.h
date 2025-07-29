#pragma once

#include "IR0Config.h"
#include <memory>
#include <optional>
#include <string>

#include "RMember.h"
#include "RNames.h"
#include "RAccessor.h"
#include "RIdentifier.h"

namespace Citron {

class MDecl;

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class RTypeFactory;

class RStructCtorDecl;
class RClassCtorDecl;
class RLambdaDecl;
class RInterfaceDecl;

class RDeclVisitor;

class RDecl
{
public:
    virtual ~RDecl() {}

public:
    IR0_API bool IsDescendantOf(RDecl* container);
    IR0_API bool CanAccess(RDecl* target);
    IR0_API size_t GetTypeParamCount();
    IR0_API size_t GetAllTypeParamCount();

    IR0_API RTypeArgumentsPtr MakeOpenTypeArgs(RTypeFactory& factory);

public:
    IR0_API virtual std::string GetModuleName(); // once overridden by NModuleDecl, NMModuleDecl
    virtual RDecl* GetROuter() = 0;
    virtual RAccessor GetAccessor() = 0;
    virtual RIdentifier GetIdentifier() = 0;

    // typeArgs는 RDecl의 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 검색할 멤버가 추가로 가지고 있을 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 확정적으로 알고 있는 typeArgs의 개수이다. 
    // 함수는 모든 typeArgs를 나열하지 않아도 type inference로 채울 수 있기 때문에,
    // explicitTypeParamsExceptOuterCount보다 더 많은 typeParams을 갖고 있어도 결과에 반영된다
    virtual std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) = 0;

    // 현재 관점에서 identifier를 찾는다. 못 찾을 경우 부모를 찾는다. 내부에서 GetMember를 쓸 수 있다
    virtual std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) = 0;

    virtual void Accept(RDeclVisitor& visitor) = 0;
};

class RDeclVisitor
{
public:
    virtual ~RDeclVisitor() {}
    virtual void Visit(RNamespaceDecl& decl) = 0;
    virtual void Visit(RGlobalFuncDecl& decl) = 0;
    virtual void Visit(RStructDecl& decl) = 0;
    virtual void Visit(RStructCtorDecl& decl) = 0;
    virtual void Visit(RStructFuncDecl& decl) = 0;
    virtual void Visit(RStructVarDecl& decl) = 0;
    virtual void Visit(RClassDecl& decl) = 0;
    virtual void Visit(RClassCtorDecl& decl) = 0;
    virtual void Visit(RClassFuncDecl& decl) = 0;
    virtual void Visit(RClassVarDecl& decl) = 0;
    virtual void Visit(REnumDecl& decl) = 0;
    virtual void Visit(REnumElemDecl& decl) = 0;
    virtual void Visit(REnumElemVarDecl& decl) = 0;
    virtual void Visit(RLambdaDecl& decl) = 0;
    virtual void Visit(RLambdaVarDecl& decl) = 0;
    virtual void Visit(RInterfaceDecl& decl) = 0;
};

class RMDecl : public RDecl
{
    std::shared_ptr<MDecl> decl;
};


} // namespace Citron
