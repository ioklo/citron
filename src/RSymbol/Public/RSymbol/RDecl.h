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
class RDeclKey;
class RName;

class RDecl
{
public:
    virtual ~RDecl() {}

public:
    RSYMBOL_API bool IsDescendantOf(RDecl* container);
    RSYMBOL_API bool CanAccess(RDecl* target);
    RSYMBOL_API RTypeArguments* MakeOpenTypeArgs(RFactory& factory);
    RSYMBOL_API size_t GetAllTypeParamCount();
    RSYMBOL_API RIdentifier GetIdentifier();

public:
    virtual RDeclKey& GetDeclKey() = 0;
    virtual RDecl* GetOuter() = 0;
    virtual RName* TryGetName() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual RTypeParam* GetTypeParam(size_t index) = 0;

    // Get류 함수, 현재 범위에서만 검색하고 리턴한다
    // typeArgs는 RDecl의 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 검색할 멤버가 추가로 가지고 있을 typeArgs이다
    // explicitTypeParamsExceptOuterCount는 확정적으로 알고 있는 typeArgs의 개수이다. 
    // 함수는 모든 typeArgs를 나열하지 않아도 type inference로 채울 수 있기 때문에,
    // explicitTypeParamsExceptOuterCount보다 더 많은 typeParams을 갖고 있어도 결과에 반영된다
    virtual RTypeParam* GetTypeParam(InRef<RName> name) = 0;
    virtual RTypeDecl* GetTypeMember(InRef<RName> name) = 0;
    virtual std::optional<RMember> GetMember(InRef<RName> name) = 0;

private:
    // 내부 구현용 virtual, RModule에서만 override한다
    RSYMBOL_API virtual void FillIdentifier(std::string& buffer);
};

} // namespace Citron
