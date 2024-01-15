#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include <vector>
#include <string>
#include <memory>

#include "SyntaxMacros.h"

namespace Citron::Syntax {

// forward declaration
using TypeExp = std::variant<
    class IdTypeExp,
    class MemberTypeExp,
    class NullableTypeExp,
    class LocalPtrTypeExp,
    class BoxPtrTypeExp,
    class LocalTypeExp
>;

class IdTypeExp
{
    std::string name;
    std::vector<TypeExp> typeArgs;

public:
    SYNTAX_API IdTypeExp(std::string name, std::vector<TypeExp> typeArgs);
    DECLARE_DEFAULTS(IdTypeExp)

    std::string& GetName() { return name; }
    std::vector<TypeExp>& GetTypeArgs() { return typeArgs; }
};

// recursive, Parent
class MemberTypeExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API MemberTypeExp(TypeExp typeExp, std::string name, std::vector<TypeExp> typeArgs);
    DECLARE_DEFAULTS(MemberTypeExp)

    // assign operator는 필요하면 만들어보도록 하자.. 거의 안만들어도 되지 않을까 싶다
    SYNTAX_API TypeExp& GetParent();
    SYNTAX_API std::string& GetMemberName();
    SYNTAX_API std::vector<TypeExp>& GetTypeArgs();
};

// recursive, InnerTypeExp
class NullableTypeExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API NullableTypeExp(TypeExp typeExp);
    DECLARE_DEFAULTS(NullableTypeExp)

    SYNTAX_API TypeExp& GetInnerTypeExp();
};

// recusrive, InnerTypeExp
class LocalPtrTypeExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API LocalPtrTypeExp(TypeExp typeExp);
    DECLARE_DEFAULTS(LocalPtrTypeExp)

    SYNTAX_API TypeExp& GetInnerTypeExp();
};

// recursive, InnerTypeExp
class BoxPtrTypeExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BoxPtrTypeExp(TypeExp typeExp);
    DECLARE_DEFAULTS(BoxPtrTypeExp)

    SYNTAX_API TypeExp& GetInnerTypeExp();
};

// local I, innerTypeExp가 interface인지는 여기서 확인하지 않는다
// recursive, InnerTypeExp
class LocalTypeExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API LocalTypeExp(TypeExp typeExp);
    DECLARE_DEFAULTS(LocalTypeExp)

    SYNTAX_API TypeExp& GetInnerTypeExp();
};

} // namespace Citron::Syntax
