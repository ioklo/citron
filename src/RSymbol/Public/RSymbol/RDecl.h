#pragma once
#include "RSymbolConfig.h"

#include <optional>
#include <span>
#include <string>

#include "RDeclRes.h"
#include "RNames.h"
#include "RAccessor.h"
#include "RIdentifier.h"

namespace Citron {

class EDecl;

class RTypeArguments;

class RFactory;

class RStructCtorDecl;
class RStructDtorDecl;
class RClassCtorDecl;
class RLambdaDecl;
class RInterfaceDecl;
class RTypeParamDecl;

class RDeclVisitor;

class RTypeDecl;

class RDecl
{
public:
    virtual ~RDecl() {}

public:
    RSYMBOL_API bool IsDescendantOf(RDecl* container);
    RSYMBOL_API bool CanAccess(RDecl* target);
    RSYMBOL_API size_t GetAllTypeParamCount();

    RSYMBOL_API RTypeArguments* MakeOpenTypeArgs(RFactory& factory);

public:
    RSYMBOL_API virtual std::string GetModuleName(); // once overridden by NModuleDecl, NMModuleDecl
    virtual RDecl* GetROuter() = 0;
    virtual RAccessor GetAccessor() = 0;
    virtual RIdentifier GetIdentifier() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual RTypeParamDecl* GetTypeParam(size_t index) = 0;

    virtual RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) = 0; // type-space search

    // typeArgs는 RDecl의 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 검색할 멤버가 추가로 가지고 있을 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 확정적으로 알고 있는 typeArgs의 개수이다. 
    // 함수는 모든 typeArgs를 나열하지 않아도 type inference로 채울 수 있기 때문에,
    // explicitTypeParamsExceptOuterCount보다 더 많은 typeParams을 갖고 있어도 결과에 반영된다
    virtual std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) = 0;

    // 현재 관점에서 identifier를 찾는다. 못 찾을 경우 부모를 찾는다. 내부에서 GetMember를 쓸 수 있다
    virtual std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) = 0;

    virtual void Accept(RDeclVisitor& visitor) = 0;
};

class RDeclVisitor
{
public:
    virtual ~RDeclVisitor() {}
    virtual void Visit(RNamespaceDecl* decl) = 0;
    virtual void Visit(RGlobalFuncDecl* decl) = 0;
    virtual void Visit(RStructDecl* decl) = 0;
    virtual void Visit(RStructCtorDecl* decl) = 0;
    virtual void Visit(RStructDtorDecl* decl) = 0;
    virtual void Visit(RStructFuncDecl* decl) = 0;
    virtual void Visit(RStructVarDecl* decl) = 0;
    virtual void Visit(RClassDecl* decl) = 0;
    virtual void Visit(RClassCtorDecl* decl) = 0;
    virtual void Visit(RClassFuncDecl* decl) = 0;
    virtual void Visit(RClassVarDecl* decl) = 0;
    virtual void Visit(REnumDecl* decl) = 0;
    virtual void Visit(REnumElemDecl* decl) = 0;
    virtual void Visit(REnumElemVarDecl* decl) = 0;
    virtual void Visit(RLambdaDecl* decl) = 0;
    virtual void Visit(RLambdaVarDecl* decl) = 0;
    virtual void Visit(RInterfaceDecl* decl) = 0;
    virtual void Visit(RTypeParamDecl* decl) = 0;
};

class REDecl : public RDecl
{
    EDecl* decl;
};


} // namespace Citron
