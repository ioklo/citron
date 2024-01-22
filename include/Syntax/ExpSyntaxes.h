#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>
#include <variant>

#include "TypeExpSyntaxes.h"
#include "UnaryOpKindSyntax.h"
#include "BinaryOpKindSyntax.h"

#include "SyntaxMacros.h"

namespace Citron {

struct ArgumentSyntax;

// forward declarations
using StringExpElementSyntax = std::variant<struct TextStringExpElementSyntax, struct ExpStringExpElementSyntax>;

// NOTICE: 본판하고 선언이 맞아야 한다
using StmtSyntax = std::variant <
    class CommandStmtSyntax,
    class VarDeclStmtSyntax,
    class IfStmtSyntax,
    class IfTestStmtSyntax,
    class ForStmtSyntax,

    class ContinueStmtSyntax,
    class BreakStmtSyntax,
    class ReturnStmtSyntax,
    class BlockStmtSyntax,
    class BlankStmtSyntax,
    class ExpStmtSyntax,

    class TaskStmtSyntax,
    class AwaitStmtSyntax,
    class AsyncStmtSyntax,
    class ForeachStmtSyntax,
    class YieldStmtSyntax,

    class DirectiveStmtSyntax
>;

using ExpSyntax = std::variant<
    class IdentifierExpSyntax,
    class StringExpSyntax,
    class IntLiteralExpSyntax,
    class BoolLiteralExpSyntax,
    class NullLiteralExpSyntax,
    class BinaryOpExpSyntax,
    class UnaryOpExpSyntax,
    class CallExpSyntax,
    class LambdaExpSyntax,
    class IndexerExpSyntax,
    class MemberExpSyntax,
    class ListExpSyntax,
    class NewExpSyntax,
    class BoxExpSyntax,
    class IsExpSyntax,
    class AsExpSyntax
>;

class IdentifierExpSyntax
{
    std::u32string value;
    std::vector<TypeExpSyntax> typeArgs;

public:
    IdentifierExpSyntax(std::u32string value, std::vector<TypeExpSyntax> typeArgs)
        : value(value), typeArgs(std::move(typeArgs)) { }

    std::u32string& GetValue() { return value; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }
};

class StringExpSyntax
{
    std::vector<StringExpElementSyntax> elements;

public:
    SYNTAX_API StringExpSyntax(std::vector<StringExpElementSyntax> elements);
    DECLARE_DEFAULTS(StringExpSyntax)

    std::vector<StringExpElementSyntax>& GetElements() { return elements; }
};

class IntLiteralExpSyntax
{
    int value;

public:
    IntLiteralExpSyntax(int value)
        : value(value) { }

    int GetValue() { return value; }
};

class BoolLiteralExpSyntax
{
    bool value;

public:
    BoolLiteralExpSyntax(bool value)
        : value(value) { }

    bool GetValue() { return value; }
};

// null
class NullLiteralExpSyntax
{
};

// recursive, { operand0, operand1 }
class BinaryOpExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BinaryOpExpSyntax(BinaryOpKindSyntax kind, ExpSyntax operand0, ExpSyntax operand1);
    DECLARE_DEFAULTS(BinaryOpExpSyntax)

    SYNTAX_API BinaryOpKindSyntax GetKind();
    SYNTAX_API ExpSyntax& GetOperand0();
    SYNTAX_API ExpSyntax& GetOperand1();
};

class UnaryOpExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API UnaryOpExpSyntax(UnaryOpKindSyntax kind, ExpSyntax operand);
    DECLARE_DEFAULTS(UnaryOpExpSyntax)

    SYNTAX_API UnaryOpKindSyntax GetKind();
    SYNTAX_API ExpSyntax& GetOperand();
};

// recursive, { callable }
class CallExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API CallExpSyntax(ExpSyntax callable, std::vector<ArgumentSyntax> args);
    DECLARE_DEFAULTS(CallExpSyntax)
    
    SYNTAX_API ExpSyntax& GetCallable();
    SYNTAX_API std::vector<ArgumentSyntax>& GetArgs();
};

struct LambdaExpParamSyntax
{
    std::optional<TypeExpSyntax> type;
    std::u32string name;
    bool hasOut;
    bool hasParams;
};

class LambdaExpSyntax
{   
    std::vector<LambdaExpParamSyntax> params;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API LambdaExpSyntax(std::vector<LambdaExpParamSyntax> params, std::vector<StmtSyntax> body);
    DECLARE_DEFAULTS(LambdaExpSyntax)

    std::vector<LambdaExpParamSyntax>& GetParams() { return params; }
    std::vector<StmtSyntax>& GetBody() { return body; }
};

// a[b]
// recursive, { object, index }
class IndexerExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IndexerExpSyntax(ExpSyntax obj, ExpSyntax index);
    DECLARE_DEFAULTS(IndexerExpSyntax)

    SYNTAX_API ExpSyntax& GetObject();
    SYNTAX_API ExpSyntax& GetIndex();
};

// recursive { parent }
class MemberExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::u32string memberName, std::vector<TypeExpSyntax> memberTypeArgs);
    DECLARE_DEFAULTS(MemberExpSyntax)

    SYNTAX_API ExpSyntax& GetParent();
    SYNTAX_API std::u32string& GetMemberName();
    SYNTAX_API std::vector<TypeExpSyntax>& GetMemberTypeArgs();
};

class ListExpSyntax
{
    std::vector<ExpSyntax> elems;

public:
    SYNTAX_API ListExpSyntax(std::vector<ExpSyntax> elems);
    DECLARE_DEFAULTS(ListExpSyntax)

    std::vector<ExpSyntax>& GetElems() { return elems; }
};

// new Type(2, 3, 4);
class NewExpSyntax
{
    TypeExpSyntax type;
    std::vector<ArgumentSyntax> args;

public:
    SYNTAX_API NewExpSyntax(TypeExpSyntax type, std::vector<ArgumentSyntax> args);
    DECLARE_DEFAULTS(NewExpSyntax); 

    TypeExpSyntax& GetType() { return type; }
    std::vector<ArgumentSyntax>& GetArgs() { return args; }
};

// box i
// recursive { innerExp }
class BoxExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BoxExpSyntax(ExpSyntax innerExp);
    DECLARE_DEFAULTS(BoxExpSyntax)

    SYNTAX_API ExpSyntax& GetInnerExp();
};

// x is T
// recursive { exp }
class IsExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IsExpSyntax(ExpSyntax exp, TypeExpSyntax type);
    DECLARE_DEFAULTS(IsExpSyntax)

    SYNTAX_API ExpSyntax& GetExp();
    SYNTAX_API TypeExpSyntax& Type();
};

// x as T
class AsExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API AsExpSyntax(ExpSyntax exp, TypeExpSyntax type);
    DECLARE_DEFAULTS(AsExpSyntax)

    SYNTAX_API ExpSyntax& GetExp();
    SYNTAX_API TypeExpSyntax& Type();
};

} // namespace Citron

