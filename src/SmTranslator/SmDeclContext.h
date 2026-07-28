#pragma once
#include <memory>
#include <optional>
#include "Infra/Ref.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RDecl;
class RTypeArguments;
class SmTypeRes;
class SmDeclRes;

// 함수 Decl을 포함하는
class SmDeclContext 
{
public:
    // Resolve류 함수, 현재 범위에서 검색하고 못 찾으면 부모를 검색한다
    std::optional<SmTypeRes> ResolveTypeIdentifier(InRef<RName> name);
    std::optional<SmTypeRes> ResolveTypeIdentifierInHeader(InRef<RName> name);
    std::optional<SmDeclRes> ResolveIdentifier(InRef<RName> name);

public:
    virtual SmDeclContext* GetOuter() = 0;
    virtual RDecl* GetDecl() = 0;
    virtual RTypeArguments* GetTypeArgs() = 0;

private:
    // Resolve류 abstract 함수, ResolveTypeIdentifier와 ResolveIdentifier에서 쓰인다
    // 여기서 인자로 넘어가는 typeArgs는 derived의 typeArgs이다. base의 typeArgs는 derived typeArgs를 Apply해서 얻을 수 있다
    // class decl에서만 쓰이므로, 기본 구현을 만들어 놓고, class decl에서 override해서 쓰도록 한다
    virtual std::optional<SmTypeRes> ResolveInheritedTypeMember(InRef<RName> name);
    virtual std::optional<SmDeclRes> ResolveInheritedMember(InRef<RName> name);
};

} // namespace Citron