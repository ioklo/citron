#pragma once
#include "RSymbolConfig.h"

#include <optional>
#include <span>
#include <string>
#include "Infra/Ref.h"
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
class RTypeParam;

class RDeclVisitor;
class RMember;
class RTypeDecl;
class RTypeRes;
class RDeclRes;

class RDecl
{
public:
    virtual ~RDecl() {}

public:
    RSYMBOL_API bool IsDescendantOf(RDecl* container);
    RSYMBOL_API bool CanAccess(RDecl* target);
    RSYMBOL_API RTypeArguments* MakeOpenTypeArgs(RFactory& factory);
    RSYMBOL_API size_t GetAllTypeParamCount();

    // Resolve류 함수, 현재 범위에서 검색하고 못 찾으면 부모를 검색한다
    RSYMBOL_API std::optional<RTypeRes> ResolveTypeIdentifier(RTypeArguments* typeArgs, InRef<RName> name);
    RSYMBOL_API std::optional<RTypeRes> ResolveTypeIdentifierInHeader(RTypeArguments* typeArgs, InRef<RName> name);
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(RTypeArguments* typeArgs, InRef<RName> name);

public:
    virtual RDecl* GetOuter() = 0;
    virtual RIdentifier GetIdentifier() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual RTypeParam* GetTypeParam(size_t index) = 0;

    // Get류 함수, 현재 범위에서만 검색하고 리턴한다
    virtual RTypeParam* GetTypeParam(InRef<RName> name) = 0;
    virtual RTypeDecl* GetTypeMember(InRef<RName> name) = 0;
    virtual std::optional<RMember> GetMember(InRef<RName> name) = 0;

    // Resolve류 abstract 함수, ResolveTypeIdentifier와 ResolveIdentifier에서 쓰인다
    // 여기서 인자로 넘어가는 typeArgs는 derived의 typeArgs이다. base의 typeArgs는 derived typeArgs를 Apply해서 얻을 수 있다
    // class decl에서만 쓰이므로, 기본 구현을 만들어 놓고, class decl에서 override해서 쓰도록 한다
    RSYMBOL_API virtual std::optional<RTypeRes> ResolveInheritedTypeMember(RTypeArguments* typeArgs, InRef<RName> name);
    RSYMBOL_API virtual std::optional<RDeclRes> ResolveInheritedMember(RTypeArguments* typeArgs, InRef<RName> name);

    // virtual std::optional<RTypeRes> ResolveType(InRef<RName> name) = 0; // type-space search

    // typeArgs는 RDecl의 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 검색할 멤버가 추가로 가지고 있을 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 확정적으로 알고 있는 typeArgs의 개수이다. 
    // 함수는 모든 typeArgs를 나열하지 않아도 type inference로 채울 수 있기 때문에,
    // explicitTypeParamsExceptOuterCount보다 더 많은 typeParams을 갖고 있어도 결과에 반영된다
    // virtual std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) = 0;

    // 현재 관점에서 identifier를 찾는다. 못 찾을 경우 부모를 찾는다. 내부에서 ResolveMember를 쓸 수 있다
    // virtual std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) = 0;
};

} // namespace Citron
