#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include <vector>
#include <string>
#include <memory>
#include <Infra/Json.h>

#include "SyntaxMacros.h"

namespace Citron {

// forward declaration
using TypeExpSyntax = std::variant<
    class IdTypeExpSyntax,
    class MemberTypeExpSyntax,
    class NullableTypeExpSyntax,
    class LocalPtrTypeExpSyntax,
    class BoxPtrTypeExpSyntax,
    class LocalTypeExpSyntax
>;

struct JsonObject;

class IdTypeExpSyntax
{
    std::u32string name;
    std::vector<TypeExpSyntax> typeArgs;

public:
    SYNTAX_API IdTypeExpSyntax(std::u32string name, std::vector<TypeExpSyntax> typeArgs = {});
    DECLARE_DEFAULTS(IdTypeExpSyntax)

    std::u32string& GetName() { return name; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

// recursive, Parent
class MemberTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API MemberTypeExpSyntax(TypeExpSyntax typeExp, std::u32string name, std::vector<TypeExpSyntax> typeArgs);
    DECLARE_DEFAULTS(MemberTypeExpSyntax)

    // assign operator는 필요하면 만들어보도록 하자.. 거의 안만들어도 되지 않을까 싶다
    SYNTAX_API TypeExpSyntax& GetParent();
    SYNTAX_API std::u32string& GetMemberName();
    SYNTAX_API std::vector<TypeExpSyntax>& GetTypeArgs();

    SYNTAX_API JsonItem ToJson();
};

// recursive, InnerTypeExp
class NullableTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API NullableTypeExpSyntax(TypeExpSyntax typeExp);
    DECLARE_DEFAULTS(NullableTypeExpSyntax)

    SYNTAX_API TypeExpSyntax& GetInnerTypeExp();

    SYNTAX_API JsonItem ToJson();
};

// recusrive, InnerTypeExp
class LocalPtrTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API LocalPtrTypeExpSyntax(TypeExpSyntax typeExp);
    DECLARE_DEFAULTS(LocalPtrTypeExpSyntax)

    SYNTAX_API TypeExpSyntax& GetInnerTypeExp();

    SYNTAX_API JsonItem ToJson();
};

// recursive, InnerTypeExp
class BoxPtrTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BoxPtrTypeExpSyntax(TypeExpSyntax typeExp);
    DECLARE_DEFAULTS(BoxPtrTypeExpSyntax)

    SYNTAX_API TypeExpSyntax& GetInnerTypeExp();

    SYNTAX_API JsonItem ToJson();
};

// local I, innerTypeExp가 interface인지는 여기서 확인하지 않는다
// recursive, InnerTypeExp
class LocalTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API LocalTypeExpSyntax(TypeExpSyntax typeExp);
    DECLARE_DEFAULTS(LocalTypeExpSyntax)

    SYNTAX_API TypeExpSyntax& GetInnerTypeExp();
    SYNTAX_API JsonItem ToJson();
};

SYNTAX_API JsonItem ToJson(TypeExpSyntax& typeExp);

} // namespace Citron
