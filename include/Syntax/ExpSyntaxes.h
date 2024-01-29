#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>
#include <variant>

#include "TypeExpSyntaxes.h"
#include "UnaryOpSyntaxKind.h"
#include "BinaryOpSyntaxKind.h"

#include <Infra/Json.h>

#include "SyntaxMacros.h"

namespace Citron {

struct ArgumentSyntax;

// forward declarations
using StringExpSyntaxElement = std::variant<struct TextStringExpSyntaxElement, struct ExpStringExpSyntaxElement>;

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

class StringWriter;

class IdentifierExpSyntax
{
    std::u32string value;
    std::vector<TypeExpSyntax> typeArgs;

public:
    IdentifierExpSyntax(std::u32string value, std::vector<TypeExpSyntax> typeArgs = {})
        : value(value), typeArgs(std::move(typeArgs)) { }

    std::u32string& GetValue() { return value; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class StringExpSyntax
{
    std::vector<StringExpSyntaxElement> elements;

public:
    SYNTAX_API StringExpSyntax(std::vector<StringExpSyntaxElement> elements);
    SYNTAX_API StringExpSyntax(std::u32string str);
    DECLARE_DEFAULTS(StringExpSyntax)

    std::vector<StringExpSyntaxElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

class IntLiteralExpSyntax
{
    int value;

public:
    IntLiteralExpSyntax(int value)
        : value(value) { }

    int GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

class BoolLiteralExpSyntax
{
    bool value;

public:
    BoolLiteralExpSyntax(bool value)
        : value(value) { }

    bool GetValue() { return value; }
    
    SYNTAX_API JsonItem ToJson();
};

// null
class NullLiteralExpSyntax
{
public:
    SYNTAX_API JsonItem ToJson();
};

// recursive, { operand0, operand1 }
class BinaryOpExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BinaryOpExpSyntax(BinaryOpSyntaxKind kind, ExpSyntax operand0, ExpSyntax operand1);
    DECLARE_DEFAULTS(BinaryOpExpSyntax)

    SYNTAX_API BinaryOpSyntaxKind GetKind();
    SYNTAX_API ExpSyntax& GetOperand0();
    SYNTAX_API ExpSyntax& GetOperand1();

    SYNTAX_API JsonItem ToJson();

    
};

class UnaryOpExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API UnaryOpExpSyntax(UnaryOpSyntaxKind kind, ExpSyntax operand);
    DECLARE_DEFAULTS(UnaryOpExpSyntax)

    SYNTAX_API UnaryOpSyntaxKind GetKind();
    SYNTAX_API ExpSyntax& GetOperand();

    SYNTAX_API JsonItem ToJson();
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

    SYNTAX_API JsonItem ToJson();
};

struct LambdaExpParamSyntax
{
    std::optional<TypeExpSyntax> type;
    std::u32string name;
    bool hasOut;
    bool hasParams;
};

SYNTAX_API JsonItem ToJson(LambdaExpParamSyntax& syntax);

class LambdaExpSyntax
{   
    std::vector<LambdaExpParamSyntax> params;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API LambdaExpSyntax(std::vector<LambdaExpParamSyntax> params, std::vector<StmtSyntax> body);
    DECLARE_DEFAULTS(LambdaExpSyntax)

    std::vector<LambdaExpParamSyntax>& GetParams() { return params; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
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

    SYNTAX_API JsonItem ToJson();
};

// recursive { parent }
class MemberExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::u32string memberName, std::vector<TypeExpSyntax> memberTypeArgs = {});
    DECLARE_DEFAULTS(MemberExpSyntax)

    SYNTAX_API ExpSyntax& GetParent();
    SYNTAX_API std::u32string& GetMemberName();
    SYNTAX_API std::vector<TypeExpSyntax>& GetMemberTypeArgs();

    SYNTAX_API JsonItem ToJson();
};

class ListExpSyntax
{
    std::vector<ExpSyntax> elems;

public:
    SYNTAX_API ListExpSyntax(std::vector<ExpSyntax> elems);
    DECLARE_DEFAULTS(ListExpSyntax)

    std::vector<ExpSyntax>& GetElems() { return elems; }

    SYNTAX_API JsonItem ToJson();
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

    SYNTAX_API JsonItem ToJson();
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

    SYNTAX_API JsonItem ToJson();
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

    SYNTAX_API JsonItem ToJson();
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

    SYNTAX_API JsonItem ToJson();
};

SYNTAX_API JsonItem ToJson(ExpSyntax& exp);

} // namespace Citron

