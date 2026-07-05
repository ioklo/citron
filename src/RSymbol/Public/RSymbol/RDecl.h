#pragma once
#include "RSymbolConfig.h"

#include <optional>
#include <span>
#include <string>
#include "Infra/Ref.h"
#include "RDeclRes.h"
#include "RNames.h"
#include "RAccessor.h"
#include "RIdentifier.h"

namespace Citron {

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
    // RSYMBOL_API bool CanAccess(RDecl* target);

    RSYMBOL_API RTypeArguments* MakeOpenTypeArgs(RFactory& factory);

public:
    virtual RDecl* GetOuter() = 0;
    virtual RIdentifier GetIdentifier() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual RTypeParamDecl* GetTypeParam(size_t index) = 0;
    virtual RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) = 0; // type-space search

    // typeArgs는 RDecl의 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 검색할 멤버가 추가로 가지고 있을 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 확정적으로 알고 있는 typeArgs의 개수이다. 
    // 함수는 모든 typeArgs를 나열하지 않아도 type inference로 채울 수 있기 때문에,
    // explicitTypeParamsExceptOuterCount보다 더 많은 typeParams을 갖고 있어도 결과에 반영된다
    virtual std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) = 0;

    // 현재 관점에서 identifier를 찾는다. 못 찾을 경우 부모를 찾는다. 내부에서 GetMember를 쓸 수 있다
    virtual std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) = 0;
};

} // namespace Citron
