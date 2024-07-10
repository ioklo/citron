#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>
#include <memory>
#include <variant>

#include <Infra/Json.h>
#include <Infra/Unreachable.h>

namespace Citron {
class ArgumentSyntax;

using StmtSyntax = std::variant<
    class CommandStmtSyntax,
    class VarDeclStmtSyntax,
    std::unique_ptr<class IfStmtSyntax>,
    std::unique_ptr<class IfTestStmtSyntax>,
    std::unique_ptr<class ForStmtSyntax>,
    class ContinueStmtSyntax,
    class BreakStmtSyntax,
    std::unique_ptr<class ReturnStmtSyntax>,
    class BlockStmtSyntax,
    class BlankStmtSyntax,
    std::unique_ptr<class ExpStmtSyntax>,
    class TaskStmtSyntax,
    class AwaitStmtSyntax,
    class AsyncStmtSyntax,
    std::unique_ptr<class ForeachStmtSyntax>,
    std::unique_ptr<class YieldStmtSyntax>,
    class DirectiveStmtSyntax>;

SYNTAX_API JsonItem ToJson(StmtSyntax& stmt);

using ExpSyntax = std::variant<
    class IdentifierExpSyntax,
    class StringExpSyntax,
    class IntLiteralExpSyntax,
    class BoolLiteralExpSyntax,
    class NullLiteralExpSyntax,
    std::unique_ptr<class BinaryOpExpSyntax>,
    std::unique_ptr<class UnaryOpExpSyntax>,
    std::unique_ptr<class CallExpSyntax>,
    std::unique_ptr<class LambdaExpSyntax>,
    std::unique_ptr<class IndexerExpSyntax>,
    std::unique_ptr<class MemberExpSyntax>,
    std::unique_ptr<class IndirectMemberExpSyntax>,
    class ListExpSyntax,
    class NewExpSyntax,
    std::unique_ptr<class BoxExpSyntax>,
    std::unique_ptr<class IsExpSyntax>,
    std::unique_ptr<class AsExpSyntax>>;

SYNTAX_API JsonItem ToJson(ExpSyntax& exp);

using TypeExpSyntax = std::variant<
    class IdTypeExpSyntax,
    std::unique_ptr<class MemberTypeExpSyntax>,
    std::unique_ptr<class NullableTypeExpSyntax>,
    std::unique_ptr<class LocalPtrTypeExpSyntax>,
    std::unique_ptr<class BoxPtrTypeExpSyntax>,
    std::unique_ptr<class LocalTypeExpSyntax>>;

SYNTAX_API JsonItem ToJson(TypeExpSyntax& typeExp);

class IdTypeExpSyntax
{
    std::string name;
    std::vector<TypeExpSyntax> typeArgs;

public:
    SYNTAX_API IdTypeExpSyntax(std::string name, std::vector<TypeExpSyntax> typeArgs);
    SYNTAX_API IdTypeExpSyntax(std::string name);
    IdTypeExpSyntax(const IdTypeExpSyntax&) = delete;
    SYNTAX_API IdTypeExpSyntax(IdTypeExpSyntax&&) noexcept;
    SYNTAX_API ~IdTypeExpSyntax();

    IdTypeExpSyntax& operator=(const IdTypeExpSyntax& other) = delete;
    SYNTAX_API IdTypeExpSyntax& operator=(IdTypeExpSyntax&& other) noexcept;

    std::string& GetName() { return name; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class MemberTypeExpSyntax
{
    TypeExpSyntax parentType;
    std::string name;
    std::vector<TypeExpSyntax> typeArgs;

public:
    SYNTAX_API MemberTypeExpSyntax(TypeExpSyntax parentType, std::string name, std::vector<TypeExpSyntax> typeArgs);
    MemberTypeExpSyntax(const MemberTypeExpSyntax&) = delete;
    SYNTAX_API MemberTypeExpSyntax(MemberTypeExpSyntax&&) noexcept;
    SYNTAX_API ~MemberTypeExpSyntax();

    MemberTypeExpSyntax& operator=(const MemberTypeExpSyntax& other) = delete;
    SYNTAX_API MemberTypeExpSyntax& operator=(MemberTypeExpSyntax&& other) noexcept;

    TypeExpSyntax& GetParentType() { return parentType; }
    std::string& GetName() { return name; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class NullableTypeExpSyntax
{
    TypeExpSyntax innerType;

public:
    SYNTAX_API NullableTypeExpSyntax(TypeExpSyntax innerType);
    NullableTypeExpSyntax(const NullableTypeExpSyntax&) = delete;
    SYNTAX_API NullableTypeExpSyntax(NullableTypeExpSyntax&&) noexcept;
    SYNTAX_API ~NullableTypeExpSyntax();

    NullableTypeExpSyntax& operator=(const NullableTypeExpSyntax& other) = delete;
    SYNTAX_API NullableTypeExpSyntax& operator=(NullableTypeExpSyntax&& other) noexcept;

    TypeExpSyntax& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
};

class LocalPtrTypeExpSyntax
{
    TypeExpSyntax innerType;

public:
    SYNTAX_API LocalPtrTypeExpSyntax(TypeExpSyntax innerType);
    LocalPtrTypeExpSyntax(const LocalPtrTypeExpSyntax&) = delete;
    SYNTAX_API LocalPtrTypeExpSyntax(LocalPtrTypeExpSyntax&&) noexcept;
    SYNTAX_API ~LocalPtrTypeExpSyntax();

    LocalPtrTypeExpSyntax& operator=(const LocalPtrTypeExpSyntax& other) = delete;
    SYNTAX_API LocalPtrTypeExpSyntax& operator=(LocalPtrTypeExpSyntax&& other) noexcept;

    TypeExpSyntax& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
};

class BoxPtrTypeExpSyntax
{
    TypeExpSyntax innerType;

public:
    SYNTAX_API BoxPtrTypeExpSyntax(TypeExpSyntax innerType);
    BoxPtrTypeExpSyntax(const BoxPtrTypeExpSyntax&) = delete;
    SYNTAX_API BoxPtrTypeExpSyntax(BoxPtrTypeExpSyntax&&) noexcept;
    SYNTAX_API ~BoxPtrTypeExpSyntax();

    BoxPtrTypeExpSyntax& operator=(const BoxPtrTypeExpSyntax& other) = delete;
    SYNTAX_API BoxPtrTypeExpSyntax& operator=(BoxPtrTypeExpSyntax&& other) noexcept;

    TypeExpSyntax& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
};

class LocalTypeExpSyntax
{
    TypeExpSyntax innerType;

public:
    SYNTAX_API LocalTypeExpSyntax(TypeExpSyntax innerType);
    LocalTypeExpSyntax(const LocalTypeExpSyntax&) = delete;
    SYNTAX_API LocalTypeExpSyntax(LocalTypeExpSyntax&&) noexcept;
    SYNTAX_API ~LocalTypeExpSyntax();

    LocalTypeExpSyntax& operator=(const LocalTypeExpSyntax& other) = delete;
    SYNTAX_API LocalTypeExpSyntax& operator=(LocalTypeExpSyntax&& other) noexcept;

    TypeExpSyntax& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
};

class LambdaExpParamSyntax
{
    std::optional<TypeExpSyntax> type;
    std::string name;
    bool hasOut;
    bool hasParams;

public:
    SYNTAX_API LambdaExpParamSyntax(std::optional<TypeExpSyntax> type, std::string name, bool hasOut, bool hasParams);
    LambdaExpParamSyntax(const LambdaExpParamSyntax&) = delete;
    SYNTAX_API LambdaExpParamSyntax(LambdaExpParamSyntax&&) noexcept;
    SYNTAX_API ~LambdaExpParamSyntax();

    LambdaExpParamSyntax& operator=(const LambdaExpParamSyntax& other) = delete;
    SYNTAX_API LambdaExpParamSyntax& operator=(LambdaExpParamSyntax&& other) noexcept;

    std::optional<TypeExpSyntax>& GetType() { return type; }
    std::string& GetName() { return name; }
    bool& HasOut() { return hasOut; }
    bool& HasParams() { return hasParams; }

    SYNTAX_API JsonItem ToJson();
};

enum class AccessModifierSyntax
{
    Public,
    Protected,
    Private,
};

inline JsonItem ToJson(AccessModifierSyntax& arg)
{
    switch(arg)
    {
    case AccessModifierSyntax::Public: return JsonString("Public");
    case AccessModifierSyntax::Protected: return JsonString("Protected");
    case AccessModifierSyntax::Private: return JsonString("Private");
    }
    unreachable();
}

enum class BinaryOpSyntaxKind
{
    Multiply,
    Divide,
    Modulo,
    Add,
    Subtract,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    Equal,
    NotEqual,
    Assign,
};

inline JsonItem ToJson(BinaryOpSyntaxKind& arg)
{
    switch(arg)
    {
    case BinaryOpSyntaxKind::Multiply: return JsonString("Multiply");
    case BinaryOpSyntaxKind::Divide: return JsonString("Divide");
    case BinaryOpSyntaxKind::Modulo: return JsonString("Modulo");
    case BinaryOpSyntaxKind::Add: return JsonString("Add");
    case BinaryOpSyntaxKind::Subtract: return JsonString("Subtract");
    case BinaryOpSyntaxKind::LessThan: return JsonString("LessThan");
    case BinaryOpSyntaxKind::GreaterThan: return JsonString("GreaterThan");
    case BinaryOpSyntaxKind::LessThanOrEqual: return JsonString("LessThanOrEqual");
    case BinaryOpSyntaxKind::GreaterThanOrEqual: return JsonString("GreaterThanOrEqual");
    case BinaryOpSyntaxKind::Equal: return JsonString("Equal");
    case BinaryOpSyntaxKind::NotEqual: return JsonString("NotEqual");
    case BinaryOpSyntaxKind::Assign: return JsonString("Assign");
    }
    unreachable();
}

enum class UnaryOpSyntaxKind
{
    PostfixInc,
    PostfixDec,
    Minus,
    LogicalNot,
    PrefixInc,
    PrefixDec,
    Ref,
    Deref,
};

inline JsonItem ToJson(UnaryOpSyntaxKind& arg)
{
    switch(arg)
    {
    case UnaryOpSyntaxKind::PostfixInc: return JsonString("PostfixInc");
    case UnaryOpSyntaxKind::PostfixDec: return JsonString("PostfixDec");
    case UnaryOpSyntaxKind::Minus: return JsonString("Minus");
    case UnaryOpSyntaxKind::LogicalNot: return JsonString("LogicalNot");
    case UnaryOpSyntaxKind::PrefixInc: return JsonString("PrefixInc");
    case UnaryOpSyntaxKind::PrefixDec: return JsonString("PrefixDec");
    case UnaryOpSyntaxKind::Ref: return JsonString("Ref");
    case UnaryOpSyntaxKind::Deref: return JsonString("Deref");
    }
    unreachable();
}

class IdentifierExpSyntax
{
    std::string value;
    std::vector<TypeExpSyntax> typeArgs;

public:
    SYNTAX_API IdentifierExpSyntax(std::string value, std::vector<TypeExpSyntax> typeArgs);
    IdentifierExpSyntax(std::string value) : IdentifierExpSyntax(std::move(value), {}) { }
    IdentifierExpSyntax(const IdentifierExpSyntax&) = delete;
    SYNTAX_API IdentifierExpSyntax(IdentifierExpSyntax&&) noexcept;
    SYNTAX_API ~IdentifierExpSyntax();

    IdentifierExpSyntax& operator=(const IdentifierExpSyntax& other) = delete;
    SYNTAX_API IdentifierExpSyntax& operator=(IdentifierExpSyntax&& other) noexcept;

    std::string& GetValue() { return value; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

using StringExpSyntaxElement = std::variant<
    class TextStringExpSyntaxElement,
    std::unique_ptr<class ExpStringExpSyntaxElement>>;

SYNTAX_API JsonItem ToJson(StringExpSyntaxElement& elem);

class TextStringExpSyntaxElement
{
    std::string text;

public:
    SYNTAX_API TextStringExpSyntaxElement(std::string text);
    TextStringExpSyntaxElement(const TextStringExpSyntaxElement&) = delete;
    SYNTAX_API TextStringExpSyntaxElement(TextStringExpSyntaxElement&&) noexcept;
    SYNTAX_API ~TextStringExpSyntaxElement();

    TextStringExpSyntaxElement& operator=(const TextStringExpSyntaxElement& other) = delete;
    SYNTAX_API TextStringExpSyntaxElement& operator=(TextStringExpSyntaxElement&& other) noexcept;

    std::string& GetText() { return text; }

    SYNTAX_API JsonItem ToJson();
};

class StringExpSyntax
{
    std::vector<StringExpSyntaxElement> elements;

public:
    SYNTAX_API StringExpSyntax(std::vector<StringExpSyntaxElement> elements);
    SYNTAX_API StringExpSyntax(std::string str);
    StringExpSyntax(const StringExpSyntax&) = delete;
    SYNTAX_API StringExpSyntax(StringExpSyntax&&) noexcept;
    SYNTAX_API ~StringExpSyntax();

    StringExpSyntax& operator=(const StringExpSyntax& other) = delete;
    SYNTAX_API StringExpSyntax& operator=(StringExpSyntax&& other) noexcept;

    std::vector<StringExpSyntaxElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

class IntLiteralExpSyntax
{
    int value;

public:
    SYNTAX_API IntLiteralExpSyntax(int value);
    IntLiteralExpSyntax(const IntLiteralExpSyntax&) = delete;
    SYNTAX_API IntLiteralExpSyntax(IntLiteralExpSyntax&&) noexcept;
    SYNTAX_API ~IntLiteralExpSyntax();

    IntLiteralExpSyntax& operator=(const IntLiteralExpSyntax& other) = delete;
    SYNTAX_API IntLiteralExpSyntax& operator=(IntLiteralExpSyntax&& other) noexcept;

    int& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

class BoolLiteralExpSyntax
{
    bool value;

public:
    SYNTAX_API BoolLiteralExpSyntax(bool value);
    BoolLiteralExpSyntax(const BoolLiteralExpSyntax&) = delete;
    SYNTAX_API BoolLiteralExpSyntax(BoolLiteralExpSyntax&&) noexcept;
    SYNTAX_API ~BoolLiteralExpSyntax();

    BoolLiteralExpSyntax& operator=(const BoolLiteralExpSyntax& other) = delete;
    SYNTAX_API BoolLiteralExpSyntax& operator=(BoolLiteralExpSyntax&& other) noexcept;

    bool& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

class NullLiteralExpSyntax
{
public:
    SYNTAX_API NullLiteralExpSyntax();
    NullLiteralExpSyntax(const NullLiteralExpSyntax&) = delete;
    SYNTAX_API NullLiteralExpSyntax(NullLiteralExpSyntax&&) noexcept;
    SYNTAX_API ~NullLiteralExpSyntax();

    NullLiteralExpSyntax& operator=(const NullLiteralExpSyntax& other) = delete;
    SYNTAX_API NullLiteralExpSyntax& operator=(NullLiteralExpSyntax&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class ListExpSyntax
{
    std::vector<ExpSyntax> elements;

public:
    SYNTAX_API ListExpSyntax(std::vector<ExpSyntax> elements);
    ListExpSyntax(const ListExpSyntax&) = delete;
    SYNTAX_API ListExpSyntax(ListExpSyntax&&) noexcept;
    SYNTAX_API ~ListExpSyntax();

    ListExpSyntax& operator=(const ListExpSyntax& other) = delete;
    SYNTAX_API ListExpSyntax& operator=(ListExpSyntax&& other) noexcept;

    std::vector<ExpSyntax>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

class NewExpSyntax
{
    TypeExpSyntax type;
    std::vector<ArgumentSyntax> args;

public:
    SYNTAX_API NewExpSyntax(TypeExpSyntax type, std::vector<ArgumentSyntax> args);
    NewExpSyntax(const NewExpSyntax&) = delete;
    SYNTAX_API NewExpSyntax(NewExpSyntax&&) noexcept;
    SYNTAX_API ~NewExpSyntax();

    NewExpSyntax& operator=(const NewExpSyntax& other) = delete;
    SYNTAX_API NewExpSyntax& operator=(NewExpSyntax&& other) noexcept;

    TypeExpSyntax& GetType() { return type; }
    std::vector<ArgumentSyntax>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
};

class BinaryOpExpSyntax
{
    BinaryOpSyntaxKind kind;
    ExpSyntax operand0;
    ExpSyntax operand1;

public:
    SYNTAX_API BinaryOpExpSyntax(BinaryOpSyntaxKind kind, ExpSyntax operand0, ExpSyntax operand1);
    BinaryOpExpSyntax(const BinaryOpExpSyntax&) = delete;
    SYNTAX_API BinaryOpExpSyntax(BinaryOpExpSyntax&&) noexcept;
    SYNTAX_API ~BinaryOpExpSyntax();

    BinaryOpExpSyntax& operator=(const BinaryOpExpSyntax& other) = delete;
    SYNTAX_API BinaryOpExpSyntax& operator=(BinaryOpExpSyntax&& other) noexcept;

    BinaryOpSyntaxKind& GetKind() { return kind; }
    ExpSyntax& GetOperand0() { return operand0; }
    ExpSyntax& GetOperand1() { return operand1; }

    SYNTAX_API JsonItem ToJson();
};

class UnaryOpExpSyntax
{
    UnaryOpSyntaxKind kind;
    ExpSyntax operand;

public:
    SYNTAX_API UnaryOpExpSyntax(UnaryOpSyntaxKind kind, ExpSyntax operand);
    UnaryOpExpSyntax(const UnaryOpExpSyntax&) = delete;
    SYNTAX_API UnaryOpExpSyntax(UnaryOpExpSyntax&&) noexcept;
    SYNTAX_API ~UnaryOpExpSyntax();

    UnaryOpExpSyntax& operator=(const UnaryOpExpSyntax& other) = delete;
    SYNTAX_API UnaryOpExpSyntax& operator=(UnaryOpExpSyntax&& other) noexcept;

    UnaryOpSyntaxKind& GetKind() { return kind; }
    ExpSyntax& GetOperand() { return operand; }

    SYNTAX_API JsonItem ToJson();
};

class CallExpSyntax
{
    ExpSyntax callable;
    std::vector<ArgumentSyntax> args;

public:
    SYNTAX_API CallExpSyntax(ExpSyntax callable, std::vector<ArgumentSyntax> args);
    CallExpSyntax(const CallExpSyntax&) = delete;
    SYNTAX_API CallExpSyntax(CallExpSyntax&&) noexcept;
    SYNTAX_API ~CallExpSyntax();

    CallExpSyntax& operator=(const CallExpSyntax& other) = delete;
    SYNTAX_API CallExpSyntax& operator=(CallExpSyntax&& other) noexcept;

    ExpSyntax& GetCallable() { return callable; }
    std::vector<ArgumentSyntax>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
};

using LambdaExpBodySyntax = std::variant<
    class StmtsLambdaExpBodySyntax,
    std::unique_ptr<class ExpLambdaExpBodySyntax>>;

SYNTAX_API JsonItem ToJson(LambdaExpBodySyntax& body);

class StmtsLambdaExpBodySyntax
{
    std::vector<StmtSyntax> stmts;

public:
    SYNTAX_API StmtsLambdaExpBodySyntax(std::vector<StmtSyntax> stmts);
    StmtsLambdaExpBodySyntax(const StmtsLambdaExpBodySyntax&) = delete;
    SYNTAX_API StmtsLambdaExpBodySyntax(StmtsLambdaExpBodySyntax&&) noexcept;
    SYNTAX_API ~StmtsLambdaExpBodySyntax();

    StmtsLambdaExpBodySyntax& operator=(const StmtsLambdaExpBodySyntax& other) = delete;
    SYNTAX_API StmtsLambdaExpBodySyntax& operator=(StmtsLambdaExpBodySyntax&& other) noexcept;

    std::vector<StmtSyntax>& GetStmts() { return stmts; }

    SYNTAX_API JsonItem ToJson();
};

class ExpLambdaExpBodySyntax
{
    ExpSyntax exp;

public:
    SYNTAX_API ExpLambdaExpBodySyntax(ExpSyntax exp);
    ExpLambdaExpBodySyntax(const ExpLambdaExpBodySyntax&) = delete;
    SYNTAX_API ExpLambdaExpBodySyntax(ExpLambdaExpBodySyntax&&) noexcept;
    SYNTAX_API ~ExpLambdaExpBodySyntax();

    ExpLambdaExpBodySyntax& operator=(const ExpLambdaExpBodySyntax& other) = delete;
    SYNTAX_API ExpLambdaExpBodySyntax& operator=(ExpLambdaExpBodySyntax&& other) noexcept;

    ExpSyntax& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
};

class LambdaExpSyntax
{
    std::vector<LambdaExpParamSyntax> params;
    LambdaExpBodySyntax body;

public:
    SYNTAX_API LambdaExpSyntax(std::vector<LambdaExpParamSyntax> params, LambdaExpBodySyntax body);
    LambdaExpSyntax(const LambdaExpSyntax&) = delete;
    SYNTAX_API LambdaExpSyntax(LambdaExpSyntax&&) noexcept;
    SYNTAX_API ~LambdaExpSyntax();

    LambdaExpSyntax& operator=(const LambdaExpSyntax& other) = delete;
    SYNTAX_API LambdaExpSyntax& operator=(LambdaExpSyntax&& other) noexcept;

    std::vector<LambdaExpParamSyntax>& GetParams() { return params; }
    LambdaExpBodySyntax& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class IndexerExpSyntax
{
    ExpSyntax obj;
    ExpSyntax index;

public:
    SYNTAX_API IndexerExpSyntax(ExpSyntax obj, ExpSyntax index);
    IndexerExpSyntax(const IndexerExpSyntax&) = delete;
    SYNTAX_API IndexerExpSyntax(IndexerExpSyntax&&) noexcept;
    SYNTAX_API ~IndexerExpSyntax();

    IndexerExpSyntax& operator=(const IndexerExpSyntax& other) = delete;
    SYNTAX_API IndexerExpSyntax& operator=(IndexerExpSyntax&& other) noexcept;

    ExpSyntax& GetObject() { return obj; }
    ExpSyntax& GetIndex() { return index; }

    SYNTAX_API JsonItem ToJson();
};

class MemberExpSyntax
{
    ExpSyntax parent;
    std::string memberName;
    std::vector<TypeExpSyntax> memberTypeArgs;

public:
    SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::string memberName, std::vector<TypeExpSyntax> memberTypeArgs);
    SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::string memberName);
    MemberExpSyntax(const MemberExpSyntax&) = delete;
    SYNTAX_API MemberExpSyntax(MemberExpSyntax&&) noexcept;
    SYNTAX_API ~MemberExpSyntax();

    MemberExpSyntax& operator=(const MemberExpSyntax& other) = delete;
    SYNTAX_API MemberExpSyntax& operator=(MemberExpSyntax&& other) noexcept;

    ExpSyntax& GetParent() { return parent; }
    std::string& GetMemberName() { return memberName; }
    std::vector<TypeExpSyntax>& GetMemberTypeArgs() { return memberTypeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class IndirectMemberExpSyntax
{
    ExpSyntax parent;
    std::string memberName;
    std::vector<TypeExpSyntax> memberTypeArgs;

public:
    SYNTAX_API IndirectMemberExpSyntax(ExpSyntax parent, std::string memberName, std::vector<TypeExpSyntax> memberTypeArgs);
    SYNTAX_API IndirectMemberExpSyntax(ExpSyntax parent, std::string memberName);
    IndirectMemberExpSyntax(const IndirectMemberExpSyntax&) = delete;
    SYNTAX_API IndirectMemberExpSyntax(IndirectMemberExpSyntax&&) noexcept;
    SYNTAX_API ~IndirectMemberExpSyntax();

    IndirectMemberExpSyntax& operator=(const IndirectMemberExpSyntax& other) = delete;
    SYNTAX_API IndirectMemberExpSyntax& operator=(IndirectMemberExpSyntax&& other) noexcept;

    ExpSyntax& GetParent() { return parent; }
    std::string& GetMemberName() { return memberName; }
    std::vector<TypeExpSyntax>& GetMemberTypeArgs() { return memberTypeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class BoxExpSyntax
{
    ExpSyntax innerExp;

public:
    SYNTAX_API BoxExpSyntax(ExpSyntax innerExp);
    BoxExpSyntax(const BoxExpSyntax&) = delete;
    SYNTAX_API BoxExpSyntax(BoxExpSyntax&&) noexcept;
    SYNTAX_API ~BoxExpSyntax();

    BoxExpSyntax& operator=(const BoxExpSyntax& other) = delete;
    SYNTAX_API BoxExpSyntax& operator=(BoxExpSyntax&& other) noexcept;

    ExpSyntax& GetInnerExp() { return innerExp; }

    SYNTAX_API JsonItem ToJson();
};

class IsExpSyntax
{
    ExpSyntax exp;
    TypeExpSyntax type;

public:
    SYNTAX_API IsExpSyntax(ExpSyntax exp, TypeExpSyntax type);
    IsExpSyntax(const IsExpSyntax&) = delete;
    SYNTAX_API IsExpSyntax(IsExpSyntax&&) noexcept;
    SYNTAX_API ~IsExpSyntax();

    IsExpSyntax& operator=(const IsExpSyntax& other) = delete;
    SYNTAX_API IsExpSyntax& operator=(IsExpSyntax&& other) noexcept;

    ExpSyntax& GetExp() { return exp; }
    TypeExpSyntax& GetType() { return type; }

    SYNTAX_API JsonItem ToJson();
};

class AsExpSyntax
{
    ExpSyntax exp;
    TypeExpSyntax type;

public:
    SYNTAX_API AsExpSyntax(ExpSyntax exp, TypeExpSyntax type);
    AsExpSyntax(const AsExpSyntax&) = delete;
    SYNTAX_API AsExpSyntax(AsExpSyntax&&) noexcept;
    SYNTAX_API ~AsExpSyntax();

    AsExpSyntax& operator=(const AsExpSyntax& other) = delete;
    SYNTAX_API AsExpSyntax& operator=(AsExpSyntax&& other) noexcept;

    ExpSyntax& GetExp() { return exp; }
    TypeExpSyntax& GetType() { return type; }

    SYNTAX_API JsonItem ToJson();
};

class ArgumentSyntax
{
    bool bOut;
    bool bParams;
    ExpSyntax exp;

public:
    SYNTAX_API ArgumentSyntax(bool bOut, bool bParams, ExpSyntax exp);
    SYNTAX_API ArgumentSyntax(ExpSyntax exp);
    ArgumentSyntax(const ArgumentSyntax&) = delete;
    SYNTAX_API ArgumentSyntax(ArgumentSyntax&&) noexcept;
    SYNTAX_API ~ArgumentSyntax();

    ArgumentSyntax& operator=(const ArgumentSyntax& other) = delete;
    SYNTAX_API ArgumentSyntax& operator=(ArgumentSyntax&& other) noexcept;

    bool& HasOut() { return bOut; }
    bool& GetParams() { return bParams; }
    ExpSyntax& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
};

class ExpStringExpSyntaxElement
{
    ExpSyntax exp;

public:
    SYNTAX_API ExpStringExpSyntaxElement(ExpSyntax exp);
    ExpStringExpSyntaxElement(const ExpStringExpSyntaxElement&) = delete;
    SYNTAX_API ExpStringExpSyntaxElement(ExpStringExpSyntaxElement&&) noexcept;
    SYNTAX_API ~ExpStringExpSyntaxElement();

    ExpStringExpSyntaxElement& operator=(const ExpStringExpSyntaxElement& other) = delete;
    SYNTAX_API ExpStringExpSyntaxElement& operator=(ExpStringExpSyntaxElement&& other) noexcept;

    ExpSyntax& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
};

using EmbeddableStmtSyntax = std::variant<
    std::unique_ptr<class SingleEmbeddableStmtSyntax>,
    class BlockEmbeddableStmtSyntax>;

SYNTAX_API JsonItem ToJson(EmbeddableStmtSyntax& embeddableStmt);

class BlockEmbeddableStmtSyntax
{
    std::vector<StmtSyntax> stmts;

public:
    SYNTAX_API BlockEmbeddableStmtSyntax(std::vector<StmtSyntax> stmts);
    BlockEmbeddableStmtSyntax(const BlockEmbeddableStmtSyntax&) = delete;
    SYNTAX_API BlockEmbeddableStmtSyntax(BlockEmbeddableStmtSyntax&&) noexcept;
    SYNTAX_API ~BlockEmbeddableStmtSyntax();

    BlockEmbeddableStmtSyntax& operator=(const BlockEmbeddableStmtSyntax& other) = delete;
    SYNTAX_API BlockEmbeddableStmtSyntax& operator=(BlockEmbeddableStmtSyntax&& other) noexcept;

    std::vector<StmtSyntax>& GetStmts() { return stmts; }

    SYNTAX_API JsonItem ToJson();
};

class VarDeclSyntaxElement
{
    std::string varName;
    std::optional<ExpSyntax> initExp;

public:
    SYNTAX_API VarDeclSyntaxElement(std::string varName, std::optional<ExpSyntax> initExp);
    VarDeclSyntaxElement(const VarDeclSyntaxElement&) = delete;
    SYNTAX_API VarDeclSyntaxElement(VarDeclSyntaxElement&&) noexcept;
    SYNTAX_API ~VarDeclSyntaxElement();

    VarDeclSyntaxElement& operator=(const VarDeclSyntaxElement& other) = delete;
    SYNTAX_API VarDeclSyntaxElement& operator=(VarDeclSyntaxElement&& other) noexcept;

    std::string& GetVarName() { return varName; }
    std::optional<ExpSyntax>& GetInitExp() { return initExp; }

    SYNTAX_API JsonItem ToJson();
};

class VarDeclSyntax
{
    TypeExpSyntax type;
    std::vector<VarDeclSyntaxElement> elements;

public:
    SYNTAX_API VarDeclSyntax(TypeExpSyntax type, std::vector<VarDeclSyntaxElement> elements);
    VarDeclSyntax(const VarDeclSyntax&) = delete;
    SYNTAX_API VarDeclSyntax(VarDeclSyntax&&) noexcept;
    SYNTAX_API ~VarDeclSyntax();

    VarDeclSyntax& operator=(const VarDeclSyntax& other) = delete;
    SYNTAX_API VarDeclSyntax& operator=(VarDeclSyntax&& other) noexcept;

    TypeExpSyntax& GetType() { return type; }
    std::vector<VarDeclSyntaxElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

class CommandStmtSyntax
{
    std::vector<StringExpSyntax> commands;

public:
    SYNTAX_API CommandStmtSyntax(std::vector<StringExpSyntax> commands);
    CommandStmtSyntax(const CommandStmtSyntax&) = delete;
    SYNTAX_API CommandStmtSyntax(CommandStmtSyntax&&) noexcept;
    SYNTAX_API ~CommandStmtSyntax();

    CommandStmtSyntax& operator=(const CommandStmtSyntax& other) = delete;
    SYNTAX_API CommandStmtSyntax& operator=(CommandStmtSyntax&& other) noexcept;

    std::vector<StringExpSyntax>& GetCommands() { return commands; }

    SYNTAX_API JsonItem ToJson();
};

class VarDeclStmtSyntax
{
    VarDeclSyntax varDecl;

public:
    SYNTAX_API VarDeclStmtSyntax(VarDeclSyntax varDecl);
    VarDeclStmtSyntax(const VarDeclStmtSyntax&) = delete;
    SYNTAX_API VarDeclStmtSyntax(VarDeclStmtSyntax&&) noexcept;
    SYNTAX_API ~VarDeclStmtSyntax();

    VarDeclStmtSyntax& operator=(const VarDeclStmtSyntax& other) = delete;
    SYNTAX_API VarDeclStmtSyntax& operator=(VarDeclStmtSyntax&& other) noexcept;

    VarDeclSyntax& GetVarDecl() { return varDecl; }

    SYNTAX_API JsonItem ToJson();
};

class ContinueStmtSyntax
{
public:
    SYNTAX_API ContinueStmtSyntax();
    ContinueStmtSyntax(const ContinueStmtSyntax&) = delete;
    SYNTAX_API ContinueStmtSyntax(ContinueStmtSyntax&&) noexcept;
    SYNTAX_API ~ContinueStmtSyntax();

    ContinueStmtSyntax& operator=(const ContinueStmtSyntax& other) = delete;
    SYNTAX_API ContinueStmtSyntax& operator=(ContinueStmtSyntax&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class BreakStmtSyntax
{
public:
    SYNTAX_API BreakStmtSyntax();
    BreakStmtSyntax(const BreakStmtSyntax&) = delete;
    SYNTAX_API BreakStmtSyntax(BreakStmtSyntax&&) noexcept;
    SYNTAX_API ~BreakStmtSyntax();

    BreakStmtSyntax& operator=(const BreakStmtSyntax& other) = delete;
    SYNTAX_API BreakStmtSyntax& operator=(BreakStmtSyntax&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class BlockStmtSyntax
{
    std::vector<StmtSyntax> stmts;

public:
    SYNTAX_API BlockStmtSyntax(std::vector<StmtSyntax> stmts);
    BlockStmtSyntax(const BlockStmtSyntax&) = delete;
    SYNTAX_API BlockStmtSyntax(BlockStmtSyntax&&) noexcept;
    SYNTAX_API ~BlockStmtSyntax();

    BlockStmtSyntax& operator=(const BlockStmtSyntax& other) = delete;
    SYNTAX_API BlockStmtSyntax& operator=(BlockStmtSyntax&& other) noexcept;

    std::vector<StmtSyntax>& GetStmts() { return stmts; }

    SYNTAX_API JsonItem ToJson();
};

class BlankStmtSyntax
{
public:
    SYNTAX_API BlankStmtSyntax();
    BlankStmtSyntax(const BlankStmtSyntax&) = delete;
    SYNTAX_API BlankStmtSyntax(BlankStmtSyntax&&) noexcept;
    SYNTAX_API ~BlankStmtSyntax();

    BlankStmtSyntax& operator=(const BlankStmtSyntax& other) = delete;
    SYNTAX_API BlankStmtSyntax& operator=(BlankStmtSyntax&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class TaskStmtSyntax
{
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API TaskStmtSyntax(std::vector<StmtSyntax> body);
    TaskStmtSyntax(const TaskStmtSyntax&) = delete;
    SYNTAX_API TaskStmtSyntax(TaskStmtSyntax&&) noexcept;
    SYNTAX_API ~TaskStmtSyntax();

    TaskStmtSyntax& operator=(const TaskStmtSyntax& other) = delete;
    SYNTAX_API TaskStmtSyntax& operator=(TaskStmtSyntax&& other) noexcept;

    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class AwaitStmtSyntax
{
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API AwaitStmtSyntax(std::vector<StmtSyntax> body);
    AwaitStmtSyntax(const AwaitStmtSyntax&) = delete;
    SYNTAX_API AwaitStmtSyntax(AwaitStmtSyntax&&) noexcept;
    SYNTAX_API ~AwaitStmtSyntax();

    AwaitStmtSyntax& operator=(const AwaitStmtSyntax& other) = delete;
    SYNTAX_API AwaitStmtSyntax& operator=(AwaitStmtSyntax&& other) noexcept;

    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class AsyncStmtSyntax
{
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API AsyncStmtSyntax(std::vector<StmtSyntax> body);
    AsyncStmtSyntax(const AsyncStmtSyntax&) = delete;
    SYNTAX_API AsyncStmtSyntax(AsyncStmtSyntax&&) noexcept;
    SYNTAX_API ~AsyncStmtSyntax();

    AsyncStmtSyntax& operator=(const AsyncStmtSyntax& other) = delete;
    SYNTAX_API AsyncStmtSyntax& operator=(AsyncStmtSyntax&& other) noexcept;

    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class DirectiveStmtSyntax
{
    std::string name;
    std::vector<ExpSyntax> args;

public:
    SYNTAX_API DirectiveStmtSyntax(std::string name, std::vector<ExpSyntax> args);
    DirectiveStmtSyntax(const DirectiveStmtSyntax&) = delete;
    SYNTAX_API DirectiveStmtSyntax(DirectiveStmtSyntax&&) noexcept;
    SYNTAX_API ~DirectiveStmtSyntax();

    DirectiveStmtSyntax& operator=(const DirectiveStmtSyntax& other) = delete;
    SYNTAX_API DirectiveStmtSyntax& operator=(DirectiveStmtSyntax&& other) noexcept;

    std::string& GetName() { return name; }
    std::vector<ExpSyntax>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
};

class SingleEmbeddableStmtSyntax
{
    StmtSyntax stmt;

public:
    SYNTAX_API SingleEmbeddableStmtSyntax(StmtSyntax stmt);
    SingleEmbeddableStmtSyntax(const SingleEmbeddableStmtSyntax&) = delete;
    SYNTAX_API SingleEmbeddableStmtSyntax(SingleEmbeddableStmtSyntax&&) noexcept;
    SYNTAX_API ~SingleEmbeddableStmtSyntax();

    SingleEmbeddableStmtSyntax& operator=(const SingleEmbeddableStmtSyntax& other) = delete;
    SYNTAX_API SingleEmbeddableStmtSyntax& operator=(SingleEmbeddableStmtSyntax&& other) noexcept;

    StmtSyntax& GetStmt() { return stmt; }

    SYNTAX_API JsonItem ToJson();
};

class IfStmtSyntax
{
    ExpSyntax cond;
    EmbeddableStmtSyntax body;
    std::optional<EmbeddableStmtSyntax> elseBody;

public:
    SYNTAX_API IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
    IfStmtSyntax(const IfStmtSyntax&) = delete;
    SYNTAX_API IfStmtSyntax(IfStmtSyntax&&) noexcept;
    SYNTAX_API ~IfStmtSyntax();

    IfStmtSyntax& operator=(const IfStmtSyntax& other) = delete;
    SYNTAX_API IfStmtSyntax& operator=(IfStmtSyntax&& other) noexcept;

    ExpSyntax& GetCond() { return cond; }
    EmbeddableStmtSyntax& GetBody() { return body; }
    std::optional<EmbeddableStmtSyntax>& GetElseBody() { return elseBody; }

    SYNTAX_API JsonItem ToJson();
};

class IfTestStmtSyntax
{
    TypeExpSyntax testType;
    std::string varName;
    ExpSyntax exp;
    EmbeddableStmtSyntax body;
    std::optional<EmbeddableStmtSyntax> elseBody;

public:
    SYNTAX_API IfTestStmtSyntax(TypeExpSyntax testType, std::string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
    IfTestStmtSyntax(const IfTestStmtSyntax&) = delete;
    SYNTAX_API IfTestStmtSyntax(IfTestStmtSyntax&&) noexcept;
    SYNTAX_API ~IfTestStmtSyntax();

    IfTestStmtSyntax& operator=(const IfTestStmtSyntax& other) = delete;
    SYNTAX_API IfTestStmtSyntax& operator=(IfTestStmtSyntax&& other) noexcept;

    TypeExpSyntax& GetTestType() { return testType; }
    std::string& GetVarName() { return varName; }
    ExpSyntax& GetExp() { return exp; }
    EmbeddableStmtSyntax& GetBody() { return body; }
    std::optional<EmbeddableStmtSyntax>& GetElseBody() { return elseBody; }

    SYNTAX_API JsonItem ToJson();
};

using ForStmtInitializerSyntax = std::variant<
    std::unique_ptr<class ExpForStmtInitializerSyntax>,
    std::unique_ptr<class VarDeclForStmtInitializerSyntax>>;

SYNTAX_API JsonItem ToJson(ForStmtInitializerSyntax& forInit);

class ExpForStmtInitializerSyntax
{
    ExpSyntax exp;

public:
    SYNTAX_API ExpForStmtInitializerSyntax(ExpSyntax exp);
    ExpForStmtInitializerSyntax(const ExpForStmtInitializerSyntax&) = delete;
    SYNTAX_API ExpForStmtInitializerSyntax(ExpForStmtInitializerSyntax&&) noexcept;
    SYNTAX_API ~ExpForStmtInitializerSyntax();

    ExpForStmtInitializerSyntax& operator=(const ExpForStmtInitializerSyntax& other) = delete;
    SYNTAX_API ExpForStmtInitializerSyntax& operator=(ExpForStmtInitializerSyntax&& other) noexcept;

    ExpSyntax& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
};

class VarDeclForStmtInitializerSyntax
{
    VarDeclSyntax varDecl;

public:
    SYNTAX_API VarDeclForStmtInitializerSyntax(VarDeclSyntax varDecl);
    VarDeclForStmtInitializerSyntax(const VarDeclForStmtInitializerSyntax&) = delete;
    SYNTAX_API VarDeclForStmtInitializerSyntax(VarDeclForStmtInitializerSyntax&&) noexcept;
    SYNTAX_API ~VarDeclForStmtInitializerSyntax();

    VarDeclForStmtInitializerSyntax& operator=(const VarDeclForStmtInitializerSyntax& other) = delete;
    SYNTAX_API VarDeclForStmtInitializerSyntax& operator=(VarDeclForStmtInitializerSyntax&& other) noexcept;

    VarDeclSyntax& GetVarDecl() { return varDecl; }

    SYNTAX_API JsonItem ToJson();
};

class ForStmtSyntax
{
    std::optional<ForStmtInitializerSyntax> initializer;
    std::optional<ExpSyntax> cond;
    std::optional<ExpSyntax> cont;
    EmbeddableStmtSyntax body;

public:
    SYNTAX_API ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, std::optional<ExpSyntax> cond, std::optional<ExpSyntax> cont, EmbeddableStmtSyntax body);
    ForStmtSyntax(const ForStmtSyntax&) = delete;
    SYNTAX_API ForStmtSyntax(ForStmtSyntax&&) noexcept;
    SYNTAX_API ~ForStmtSyntax();

    ForStmtSyntax& operator=(const ForStmtSyntax& other) = delete;
    SYNTAX_API ForStmtSyntax& operator=(ForStmtSyntax&& other) noexcept;

    std::optional<ForStmtInitializerSyntax>& GetInitializer() { return initializer; }
    std::optional<ExpSyntax>& GetCond() { return cond; }
    std::optional<ExpSyntax>& GetCont() { return cont; }
    EmbeddableStmtSyntax& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class ReturnStmtSyntax
{
    std::optional<ExpSyntax> value;

public:
    SYNTAX_API ReturnStmtSyntax(std::optional<ExpSyntax> value);
    ReturnStmtSyntax(const ReturnStmtSyntax&) = delete;
    SYNTAX_API ReturnStmtSyntax(ReturnStmtSyntax&&) noexcept;
    SYNTAX_API ~ReturnStmtSyntax();

    ReturnStmtSyntax& operator=(const ReturnStmtSyntax& other) = delete;
    SYNTAX_API ReturnStmtSyntax& operator=(ReturnStmtSyntax&& other) noexcept;

    std::optional<ExpSyntax>& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

class ExpStmtSyntax
{
    ExpSyntax exp;

public:
    SYNTAX_API ExpStmtSyntax(ExpSyntax exp);
    ExpStmtSyntax(const ExpStmtSyntax&) = delete;
    SYNTAX_API ExpStmtSyntax(ExpStmtSyntax&&) noexcept;
    SYNTAX_API ~ExpStmtSyntax();

    ExpStmtSyntax& operator=(const ExpStmtSyntax& other) = delete;
    SYNTAX_API ExpStmtSyntax& operator=(ExpStmtSyntax&& other) noexcept;

    ExpSyntax& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
};

class ForeachStmtSyntax
{
    TypeExpSyntax type;
    std::string varName;
    ExpSyntax enumerable;
    EmbeddableStmtSyntax body;

public:
    SYNTAX_API ForeachStmtSyntax(TypeExpSyntax type, std::string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body);
    ForeachStmtSyntax(const ForeachStmtSyntax&) = delete;
    SYNTAX_API ForeachStmtSyntax(ForeachStmtSyntax&&) noexcept;
    SYNTAX_API ~ForeachStmtSyntax();

    ForeachStmtSyntax& operator=(const ForeachStmtSyntax& other) = delete;
    SYNTAX_API ForeachStmtSyntax& operator=(ForeachStmtSyntax&& other) noexcept;

    TypeExpSyntax& GetType() { return type; }
    std::string& GetVarName() { return varName; }
    ExpSyntax& GetEnumerable() { return enumerable; }
    EmbeddableStmtSyntax& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class YieldStmtSyntax
{
    ExpSyntax value;

public:
    SYNTAX_API YieldStmtSyntax(ExpSyntax value);
    YieldStmtSyntax(const YieldStmtSyntax&) = delete;
    SYNTAX_API YieldStmtSyntax(YieldStmtSyntax&&) noexcept;
    SYNTAX_API ~YieldStmtSyntax();

    YieldStmtSyntax& operator=(const YieldStmtSyntax& other) = delete;
    SYNTAX_API YieldStmtSyntax& operator=(YieldStmtSyntax&& other) noexcept;

    ExpSyntax& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

class TypeParamSyntax
{
    std::string name;

public:
    SYNTAX_API TypeParamSyntax(std::string name);
    TypeParamSyntax(const TypeParamSyntax&) = delete;
    SYNTAX_API TypeParamSyntax(TypeParamSyntax&&) noexcept;
    SYNTAX_API ~TypeParamSyntax();

    TypeParamSyntax& operator=(const TypeParamSyntax& other) = delete;
    SYNTAX_API TypeParamSyntax& operator=(TypeParamSyntax&& other) noexcept;

    std::string& GetName() { return name; }

    SYNTAX_API JsonItem ToJson();
};

class FuncParamSyntax
{
    bool hasOut;
    bool hasParams;
    TypeExpSyntax type;
    std::string name;

public:
    SYNTAX_API FuncParamSyntax(bool hasOut, bool hasParams, TypeExpSyntax type, std::string name);
    FuncParamSyntax(const FuncParamSyntax&) = delete;
    SYNTAX_API FuncParamSyntax(FuncParamSyntax&&) noexcept;
    SYNTAX_API ~FuncParamSyntax();

    FuncParamSyntax& operator=(const FuncParamSyntax& other) = delete;
    SYNTAX_API FuncParamSyntax& operator=(FuncParamSyntax&& other) noexcept;

    bool& HasOut() { return hasOut; }
    bool& HasParams() { return hasParams; }
    TypeExpSyntax& GetType() { return type; }
    std::string& GetName() { return name; }

    SYNTAX_API JsonItem ToJson();
};

class GlobalFuncDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    bool bSequence;
    TypeExpSyntax retType;
    std::string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API GlobalFuncDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, bool bSequence, TypeExpSyntax retType, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body);
    GlobalFuncDeclSyntax(const GlobalFuncDeclSyntax&) = delete;
    SYNTAX_API GlobalFuncDeclSyntax(GlobalFuncDeclSyntax&&) noexcept;
    SYNTAX_API ~GlobalFuncDeclSyntax();

    GlobalFuncDeclSyntax& operator=(const GlobalFuncDeclSyntax& other) = delete;
    SYNTAX_API GlobalFuncDeclSyntax& operator=(GlobalFuncDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    bool& IsSequence() { return bSequence; }
    TypeExpSyntax& GetRetType() { return retType; }
    std::string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class ClassMemberFuncDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    bool bStatic;
    bool bSequence;
    TypeExpSyntax retType;
    std::string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API ClassMemberFuncDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, bool bStatic, bool bSequence, TypeExpSyntax retType, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body);
    ClassMemberFuncDeclSyntax(const ClassMemberFuncDeclSyntax&) = delete;
    SYNTAX_API ClassMemberFuncDeclSyntax(ClassMemberFuncDeclSyntax&&) noexcept;
    SYNTAX_API ~ClassMemberFuncDeclSyntax();

    ClassMemberFuncDeclSyntax& operator=(const ClassMemberFuncDeclSyntax& other) = delete;
    SYNTAX_API ClassMemberFuncDeclSyntax& operator=(ClassMemberFuncDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    bool& IsStatic() { return bStatic; }
    bool& IsSequence() { return bSequence; }
    TypeExpSyntax& GetRetType() { return retType; }
    std::string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class ClassConstructorDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::string name;
    std::vector<FuncParamSyntax> parameters;
    std::optional<std::vector<ArgumentSyntax>> baseArgs;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API ClassConstructorDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<FuncParamSyntax> parameters, std::optional<std::vector<ArgumentSyntax>> baseArgs, std::vector<StmtSyntax> body);
    ClassConstructorDeclSyntax(const ClassConstructorDeclSyntax&) = delete;
    SYNTAX_API ClassConstructorDeclSyntax(ClassConstructorDeclSyntax&&) noexcept;
    SYNTAX_API ~ClassConstructorDeclSyntax();

    ClassConstructorDeclSyntax& operator=(const ClassConstructorDeclSyntax& other) = delete;
    SYNTAX_API ClassConstructorDeclSyntax& operator=(ClassConstructorDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::optional<std::vector<ArgumentSyntax>>& GetBaseArgs() { return baseArgs; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class ClassMemberVarDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    TypeExpSyntax varType;
    std::vector<std::string> varNames;

public:
    SYNTAX_API ClassMemberVarDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, TypeExpSyntax varType, std::vector<std::string> varNames);
    ClassMemberVarDeclSyntax(const ClassMemberVarDeclSyntax&) = delete;
    SYNTAX_API ClassMemberVarDeclSyntax(ClassMemberVarDeclSyntax&&) noexcept;
    SYNTAX_API ~ClassMemberVarDeclSyntax();

    ClassMemberVarDeclSyntax& operator=(const ClassMemberVarDeclSyntax& other) = delete;
    SYNTAX_API ClassMemberVarDeclSyntax& operator=(ClassMemberVarDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    TypeExpSyntax& GetVarType() { return varType; }
    std::vector<std::string>& GetVarNames() { return varNames; }

    SYNTAX_API JsonItem ToJson();
};

using ClassMemberDeclSyntax = std::variant<
    class ClassDeclSyntax,
    class StructDeclSyntax,
    class EnumDeclSyntax,
    ClassMemberFuncDeclSyntax,
    ClassConstructorDeclSyntax,
    ClassMemberVarDeclSyntax>;

SYNTAX_API JsonItem ToJson(ClassMemberDeclSyntax& decl);

class ClassDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<TypeExpSyntax> baseTypes;
    std::vector<ClassMemberDeclSyntax> memberDecls;

public:
    SYNTAX_API ClassDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<TypeExpSyntax> baseTypes, std::vector<ClassMemberDeclSyntax> memberDecls);
    ClassDeclSyntax(const ClassDeclSyntax&) = delete;
    SYNTAX_API ClassDeclSyntax(ClassDeclSyntax&&) noexcept;
    SYNTAX_API ~ClassDeclSyntax();

    ClassDeclSyntax& operator=(const ClassDeclSyntax& other) = delete;
    SYNTAX_API ClassDeclSyntax& operator=(ClassDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<TypeExpSyntax>& GetBaseTypes() { return baseTypes; }
    std::vector<ClassMemberDeclSyntax>& GetMemberDecls() { return memberDecls; }

    SYNTAX_API JsonItem ToJson();
};

class StructMemberFuncDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    bool bStatic;
    bool bSequence;
    TypeExpSyntax retType;
    std::string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API StructMemberFuncDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, bool bStatic, bool bSequence, TypeExpSyntax retType, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body);
    StructMemberFuncDeclSyntax(const StructMemberFuncDeclSyntax&) = delete;
    SYNTAX_API StructMemberFuncDeclSyntax(StructMemberFuncDeclSyntax&&) noexcept;
    SYNTAX_API ~StructMemberFuncDeclSyntax();

    StructMemberFuncDeclSyntax& operator=(const StructMemberFuncDeclSyntax& other) = delete;
    SYNTAX_API StructMemberFuncDeclSyntax& operator=(StructMemberFuncDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAcessModifier() { return accessModifier; }
    bool& IsStatic() { return bStatic; }
    bool& IsSequence() { return bSequence; }
    TypeExpSyntax& GetRetType() { return retType; }
    std::string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class StructConstructorDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::string name;
    std::vector<FuncParamSyntax> parameters;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API StructConstructorDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body);
    StructConstructorDeclSyntax(const StructConstructorDeclSyntax&) = delete;
    SYNTAX_API StructConstructorDeclSyntax(StructConstructorDeclSyntax&&) noexcept;
    SYNTAX_API ~StructConstructorDeclSyntax();

    StructConstructorDeclSyntax& operator=(const StructConstructorDeclSyntax& other) = delete;
    SYNTAX_API StructConstructorDeclSyntax& operator=(StructConstructorDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<FuncParamSyntax>& GetParameters() { return parameters; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class StructMemberVarDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    TypeExpSyntax varType;
    std::vector<std::string> varNames;

public:
    SYNTAX_API StructMemberVarDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, TypeExpSyntax varType, std::vector<std::string> varNames);
    StructMemberVarDeclSyntax(const StructMemberVarDeclSyntax&) = delete;
    SYNTAX_API StructMemberVarDeclSyntax(StructMemberVarDeclSyntax&&) noexcept;
    SYNTAX_API ~StructMemberVarDeclSyntax();

    StructMemberVarDeclSyntax& operator=(const StructMemberVarDeclSyntax& other) = delete;
    SYNTAX_API StructMemberVarDeclSyntax& operator=(StructMemberVarDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    TypeExpSyntax& GetVarType() { return varType; }
    std::vector<std::string>& GetVarNames() { return varNames; }

    SYNTAX_API JsonItem ToJson();
};

using StructMemberDeclSyntax = std::variant<
    class ClassDeclSyntax,
    class StructDeclSyntax,
    class EnumDeclSyntax,
    StructMemberFuncDeclSyntax,
    StructConstructorDeclSyntax,
    StructMemberVarDeclSyntax>;

SYNTAX_API JsonItem ToJson(StructMemberDeclSyntax& decl);

class StructDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<TypeExpSyntax> baseTypes;
    std::vector<StructMemberDeclSyntax> memberDecls;

public:
    SYNTAX_API StructDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<TypeExpSyntax> baseTypes, std::vector<StructMemberDeclSyntax> memberDecls);
    StructDeclSyntax(const StructDeclSyntax&) = delete;
    SYNTAX_API StructDeclSyntax(StructDeclSyntax&&) noexcept;
    SYNTAX_API ~StructDeclSyntax();

    StructDeclSyntax& operator=(const StructDeclSyntax& other) = delete;
    SYNTAX_API StructDeclSyntax& operator=(StructDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<TypeExpSyntax>& GetBaseTypes() { return baseTypes; }
    std::vector<StructMemberDeclSyntax>& GetMemberDecls() { return memberDecls; }

    SYNTAX_API JsonItem ToJson();
};

class EnumElemMemberVarDeclSyntax
{
    TypeExpSyntax type;
    std::string name;

public:
    SYNTAX_API EnumElemMemberVarDeclSyntax(TypeExpSyntax type, std::string name);
    EnumElemMemberVarDeclSyntax(const EnumElemMemberVarDeclSyntax&) = delete;
    SYNTAX_API EnumElemMemberVarDeclSyntax(EnumElemMemberVarDeclSyntax&&) noexcept;
    SYNTAX_API ~EnumElemMemberVarDeclSyntax();

    EnumElemMemberVarDeclSyntax& operator=(const EnumElemMemberVarDeclSyntax& other) = delete;
    SYNTAX_API EnumElemMemberVarDeclSyntax& operator=(EnumElemMemberVarDeclSyntax&& other) noexcept;

    TypeExpSyntax& GetType() { return type; }
    std::string& GetName() { return name; }

    SYNTAX_API JsonItem ToJson();
};

class EnumElemDeclSyntax
{
    std::string name;
    std::vector<EnumElemMemberVarDeclSyntax> memberVars;

public:
    SYNTAX_API EnumElemDeclSyntax(std::string name, std::vector<EnumElemMemberVarDeclSyntax> memberVars);
    EnumElemDeclSyntax(const EnumElemDeclSyntax&) = delete;
    SYNTAX_API EnumElemDeclSyntax(EnumElemDeclSyntax&&) noexcept;
    SYNTAX_API ~EnumElemDeclSyntax();

    EnumElemDeclSyntax& operator=(const EnumElemDeclSyntax& other) = delete;
    SYNTAX_API EnumElemDeclSyntax& operator=(EnumElemDeclSyntax&& other) noexcept;

    std::string& GetName() { return name; }
    std::vector<EnumElemMemberVarDeclSyntax>& GetMemberVars() { return memberVars; }

    SYNTAX_API JsonItem ToJson();
};

class EnumDeclSyntax
{
    std::optional<AccessModifierSyntax> accessModifier;
    std::string name;
    std::vector<TypeParamSyntax> typeParams;
    std::vector<EnumElemDeclSyntax> elements;

public:
    SYNTAX_API EnumDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<EnumElemDeclSyntax> elements);
    EnumDeclSyntax(const EnumDeclSyntax&) = delete;
    SYNTAX_API EnumDeclSyntax(EnumDeclSyntax&&) noexcept;
    SYNTAX_API ~EnumDeclSyntax();

    EnumDeclSyntax& operator=(const EnumDeclSyntax& other) = delete;
    SYNTAX_API EnumDeclSyntax& operator=(EnumDeclSyntax&& other) noexcept;

    std::optional<AccessModifierSyntax>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<TypeParamSyntax>& GetTypeParams() { return typeParams; }
    std::vector<EnumElemDeclSyntax>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

using NamespaceDeclSyntaxElement = std::variant<
    GlobalFuncDeclSyntax,
    class NamespaceDeclSyntax,
    ClassDeclSyntax,
    StructDeclSyntax,
    EnumDeclSyntax>;

SYNTAX_API JsonItem ToJson(NamespaceDeclSyntaxElement& elem);

class NamespaceDeclSyntax
{
    std::vector<std::string> names;
    std::vector<NamespaceDeclSyntaxElement> elements;

public:
    SYNTAX_API NamespaceDeclSyntax(std::vector<std::string> names, std::vector<NamespaceDeclSyntaxElement> elements);
    NamespaceDeclSyntax(const NamespaceDeclSyntax&) = delete;
    SYNTAX_API NamespaceDeclSyntax(NamespaceDeclSyntax&&) noexcept;
    SYNTAX_API ~NamespaceDeclSyntax();

    NamespaceDeclSyntax& operator=(const NamespaceDeclSyntax& other) = delete;
    SYNTAX_API NamespaceDeclSyntax& operator=(NamespaceDeclSyntax&& other) noexcept;

    std::vector<std::string>& GetNames() { return names; }
    std::vector<NamespaceDeclSyntaxElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

using ScriptSyntaxElement = std::variant<
    NamespaceDeclSyntax,
    GlobalFuncDeclSyntax,
    ClassDeclSyntax,
    StructDeclSyntax,
    EnumDeclSyntax>;

SYNTAX_API JsonItem ToJson(ScriptSyntaxElement& elem);

class ScriptSyntax
{
    std::vector<ScriptSyntaxElement> elements;

public:
    SYNTAX_API ScriptSyntax(std::vector<ScriptSyntaxElement> elements);
    ScriptSyntax(const ScriptSyntax&) = delete;
    SYNTAX_API ScriptSyntax(ScriptSyntax&&) noexcept;
    SYNTAX_API ~ScriptSyntax();

    ScriptSyntax& operator=(const ScriptSyntax& other) = delete;
    SYNTAX_API ScriptSyntax& operator=(ScriptSyntax&& other) noexcept;

    std::vector<ScriptSyntaxElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};


}
