#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>
#include <variant>

#include <Infra/Json.h>
#include <Infra/Unreachable.h>

namespace Citron {

using StmtSyntax = std::variant<
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
    class DirectiveStmtSyntax>;

SYNTAX_API JsonItem ToJson(StmtSyntax& stmt);

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
    class AsExpSyntax>;

SYNTAX_API JsonItem ToJson(ExpSyntax& exp);

using TypeExpSyntax = std::variant<
    class IdTypeExpSyntax,
    class MemberTypeExpSyntax,
    class NullableTypeExpSyntax,
    class LocalPtrTypeExpSyntax,
    class BoxPtrTypeExpSyntax,
    class LocalTypeExpSyntax>;

SYNTAX_API JsonItem ToJson(TypeExpSyntax& typeExp);

class IdTypeExpSyntax
{
    std::string name;
    std::vector<TypeExpSyntax> typeArgs;

public:
    IdTypeExpSyntax(std::string name, std::vector<TypeExpSyntax> typeArgs)
        : name(std::move(name)), typeArgs(std::move(typeArgs)) { }
    IdTypeExpSyntax(const IdTypeExpSyntax&) = delete;
    IdTypeExpSyntax(IdTypeExpSyntax&&) = default;

    IdTypeExpSyntax& operator=(const IdTypeExpSyntax& other) = delete;
    IdTypeExpSyntax& operator=(IdTypeExpSyntax&& other) = default;

    std::string& GetName() { return name; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class MemberTypeExpSyntax
{
    std::string name;
    std::vector<TypeExpSyntax> typeArgs;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API MemberTypeExpSyntax(TypeExpSyntax parentType, std::string name, std::vector<TypeExpSyntax> typeArgs);
    MemberTypeExpSyntax(const MemberTypeExpSyntax&) = delete;
    SYNTAX_API MemberTypeExpSyntax(MemberTypeExpSyntax&&) noexcept;
    SYNTAX_API ~MemberTypeExpSyntax();

    MemberTypeExpSyntax& operator=(const MemberTypeExpSyntax& other) = delete;
    SYNTAX_API MemberTypeExpSyntax& operator=(MemberTypeExpSyntax&& other) noexcept;

    SYNTAX_API TypeExpSyntax& GetParentType();
    std::string& GetName() { return name; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class NullableTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API NullableTypeExpSyntax(TypeExpSyntax innerType);
    NullableTypeExpSyntax(const NullableTypeExpSyntax&) = delete;
    SYNTAX_API NullableTypeExpSyntax(NullableTypeExpSyntax&&) noexcept;
    SYNTAX_API ~NullableTypeExpSyntax();

    NullableTypeExpSyntax& operator=(const NullableTypeExpSyntax& other) = delete;
    SYNTAX_API NullableTypeExpSyntax& operator=(NullableTypeExpSyntax&& other) noexcept;

    SYNTAX_API TypeExpSyntax& GetInnerType();

    SYNTAX_API JsonItem ToJson();
};

class LocalPtrTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API LocalPtrTypeExpSyntax(TypeExpSyntax innerType);
    LocalPtrTypeExpSyntax(const LocalPtrTypeExpSyntax&) = delete;
    SYNTAX_API LocalPtrTypeExpSyntax(LocalPtrTypeExpSyntax&&) noexcept;
    SYNTAX_API ~LocalPtrTypeExpSyntax();

    LocalPtrTypeExpSyntax& operator=(const LocalPtrTypeExpSyntax& other) = delete;
    SYNTAX_API LocalPtrTypeExpSyntax& operator=(LocalPtrTypeExpSyntax&& other) noexcept;

    SYNTAX_API TypeExpSyntax& GetInnerType();

    SYNTAX_API JsonItem ToJson();
};

class BoxPtrTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BoxPtrTypeExpSyntax(TypeExpSyntax innerType);
    BoxPtrTypeExpSyntax(const BoxPtrTypeExpSyntax&) = delete;
    SYNTAX_API BoxPtrTypeExpSyntax(BoxPtrTypeExpSyntax&&) noexcept;
    SYNTAX_API ~BoxPtrTypeExpSyntax();

    BoxPtrTypeExpSyntax& operator=(const BoxPtrTypeExpSyntax& other) = delete;
    SYNTAX_API BoxPtrTypeExpSyntax& operator=(BoxPtrTypeExpSyntax&& other) noexcept;

    SYNTAX_API TypeExpSyntax& GetInnerType();

    SYNTAX_API JsonItem ToJson();
};

class LocalTypeExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API LocalTypeExpSyntax(TypeExpSyntax innerType);
    LocalTypeExpSyntax(const LocalTypeExpSyntax&) = delete;
    SYNTAX_API LocalTypeExpSyntax(LocalTypeExpSyntax&&) noexcept;
    SYNTAX_API ~LocalTypeExpSyntax();

    LocalTypeExpSyntax& operator=(const LocalTypeExpSyntax& other) = delete;
    SYNTAX_API LocalTypeExpSyntax& operator=(LocalTypeExpSyntax&& other) noexcept;

    SYNTAX_API TypeExpSyntax& GetInnerType();

    SYNTAX_API JsonItem ToJson();
};

class LambdaExpParamSyntax
{
    std::optional<TypeExpSyntax> type;
    std::string name;
    bool hasOut;
    bool hasParams;

public:
    LambdaExpParamSyntax(std::optional<TypeExpSyntax> type, std::string name, bool hasOut, bool hasParams)
        : type(std::move(type)), name(std::move(name)), hasOut(hasOut), hasParams(hasParams) { }
    LambdaExpParamSyntax(const LambdaExpParamSyntax&) = delete;
    LambdaExpParamSyntax(LambdaExpParamSyntax&&) = default;

    LambdaExpParamSyntax& operator=(const LambdaExpParamSyntax& other) = delete;
    LambdaExpParamSyntax& operator=(LambdaExpParamSyntax&& other) = default;

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

class ArgumentSyntax
{
    bool bOut;
    bool bParams;

    struct Impl;
    std::unique_ptr<Impl> impl;

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
    SYNTAX_API ExpSyntax& GetExp();

    SYNTAX_API JsonItem ToJson();
};

class IdentifierExpSyntax
{
    std::string value;
    std::vector<TypeExpSyntax> typeArgs;

public:
    IdentifierExpSyntax(std::string value, std::vector<TypeExpSyntax> typeArgs)
        : value(std::move(value)), typeArgs(std::move(typeArgs)) { }
    IdentifierExpSyntax(const IdentifierExpSyntax&) = delete;
    IdentifierExpSyntax(IdentifierExpSyntax&&) = default;

    IdentifierExpSyntax& operator=(const IdentifierExpSyntax& other) = delete;
    IdentifierExpSyntax& operator=(IdentifierExpSyntax&& other) = default;

    std::string& GetValue() { return value; }
    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
};

using StringExpSyntaxElement = std::variant<
    class TextStringExpSyntaxElement,
    class ExpStringExpSyntaxElement>;

SYNTAX_API JsonItem ToJson(StringExpSyntaxElement& elem);

class TextStringExpSyntaxElement
{
    std::string text;

public:
    TextStringExpSyntaxElement(std::string text)
        : text(std::move(text)) { }
    TextStringExpSyntaxElement(const TextStringExpSyntaxElement&) = delete;
    TextStringExpSyntaxElement(TextStringExpSyntaxElement&&) = default;

    TextStringExpSyntaxElement& operator=(const TextStringExpSyntaxElement& other) = delete;
    TextStringExpSyntaxElement& operator=(TextStringExpSyntaxElement&& other) = default;

    std::string& GetText() { return text; }

    SYNTAX_API JsonItem ToJson();
};

class ExpStringExpSyntaxElement
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ExpStringExpSyntaxElement(ExpSyntax exp);
    ExpStringExpSyntaxElement(const ExpStringExpSyntaxElement&) = delete;
    SYNTAX_API ExpStringExpSyntaxElement(ExpStringExpSyntaxElement&&) noexcept;
    SYNTAX_API ~ExpStringExpSyntaxElement();

    ExpStringExpSyntaxElement& operator=(const ExpStringExpSyntaxElement& other) = delete;
    SYNTAX_API ExpStringExpSyntaxElement& operator=(ExpStringExpSyntaxElement&& other) noexcept;

    SYNTAX_API ExpSyntax& GetExp();

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
    IntLiteralExpSyntax(int value)
        : value(value) { }
    IntLiteralExpSyntax(const IntLiteralExpSyntax&) = delete;
    IntLiteralExpSyntax(IntLiteralExpSyntax&&) = default;

    IntLiteralExpSyntax& operator=(const IntLiteralExpSyntax& other) = delete;
    IntLiteralExpSyntax& operator=(IntLiteralExpSyntax&& other) = default;

    int& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

class BoolLiteralExpSyntax
{
    bool value;

public:
    BoolLiteralExpSyntax(bool value)
        : value(value) { }
    BoolLiteralExpSyntax(const BoolLiteralExpSyntax&) = delete;
    BoolLiteralExpSyntax(BoolLiteralExpSyntax&&) = default;

    BoolLiteralExpSyntax& operator=(const BoolLiteralExpSyntax& other) = delete;
    BoolLiteralExpSyntax& operator=(BoolLiteralExpSyntax&& other) = default;

    bool& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

class NullLiteralExpSyntax
{
public:
    NullLiteralExpSyntax() { }
    NullLiteralExpSyntax(const NullLiteralExpSyntax&) = delete;
    NullLiteralExpSyntax(NullLiteralExpSyntax&&) = default;

    NullLiteralExpSyntax& operator=(const NullLiteralExpSyntax& other) = delete;
    NullLiteralExpSyntax& operator=(NullLiteralExpSyntax&& other) = default;

    SYNTAX_API JsonItem ToJson();
};

class BinaryOpExpSyntax
{
    BinaryOpSyntaxKind kind;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BinaryOpExpSyntax(BinaryOpSyntaxKind kind, ExpSyntax operand0, ExpSyntax operand1);
    BinaryOpExpSyntax(const BinaryOpExpSyntax&) = delete;
    SYNTAX_API BinaryOpExpSyntax(BinaryOpExpSyntax&&) noexcept;
    SYNTAX_API ~BinaryOpExpSyntax();

    BinaryOpExpSyntax& operator=(const BinaryOpExpSyntax& other) = delete;
    SYNTAX_API BinaryOpExpSyntax& operator=(BinaryOpExpSyntax&& other) noexcept;

    BinaryOpSyntaxKind& GetKind() { return kind; }
    SYNTAX_API ExpSyntax& GetOperand0();
    SYNTAX_API ExpSyntax& GetOperand1();

    SYNTAX_API JsonItem ToJson();
};

class UnaryOpExpSyntax
{
    UnaryOpSyntaxKind kind;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API UnaryOpExpSyntax(UnaryOpSyntaxKind kind, ExpSyntax operand);
    UnaryOpExpSyntax(const UnaryOpExpSyntax&) = delete;
    SYNTAX_API UnaryOpExpSyntax(UnaryOpExpSyntax&&) noexcept;
    SYNTAX_API ~UnaryOpExpSyntax();

    UnaryOpExpSyntax& operator=(const UnaryOpExpSyntax& other) = delete;
    SYNTAX_API UnaryOpExpSyntax& operator=(UnaryOpExpSyntax&& other) noexcept;

    UnaryOpSyntaxKind& GetKind() { return kind; }
    SYNTAX_API ExpSyntax& GetOperand();

    SYNTAX_API JsonItem ToJson();
};

class CallExpSyntax
{
    std::vector<ArgumentSyntax> args;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API CallExpSyntax(ExpSyntax callable, std::vector<ArgumentSyntax> args);
    CallExpSyntax(const CallExpSyntax&) = delete;
    SYNTAX_API CallExpSyntax(CallExpSyntax&&) noexcept;
    SYNTAX_API ~CallExpSyntax();

    CallExpSyntax& operator=(const CallExpSyntax& other) = delete;
    SYNTAX_API CallExpSyntax& operator=(CallExpSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetCallable();
    std::vector<ArgumentSyntax>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
};

class LambdaExpSyntax
{
    std::vector<LambdaExpParamSyntax> params;
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API LambdaExpSyntax(std::vector<LambdaExpParamSyntax> params, std::vector<StmtSyntax> body);
    LambdaExpSyntax(const LambdaExpSyntax&) = delete;
    SYNTAX_API LambdaExpSyntax(LambdaExpSyntax&&) noexcept;
    SYNTAX_API ~LambdaExpSyntax();

    LambdaExpSyntax& operator=(const LambdaExpSyntax& other) = delete;
    SYNTAX_API LambdaExpSyntax& operator=(LambdaExpSyntax&& other) noexcept;

    std::vector<LambdaExpParamSyntax>& GetParams() { return params; }
    std::vector<StmtSyntax>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
};

class IndexerExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IndexerExpSyntax(ExpSyntax obj, ExpSyntax index);
    IndexerExpSyntax(const IndexerExpSyntax&) = delete;
    SYNTAX_API IndexerExpSyntax(IndexerExpSyntax&&) noexcept;
    SYNTAX_API ~IndexerExpSyntax();

    IndexerExpSyntax& operator=(const IndexerExpSyntax& other) = delete;
    SYNTAX_API IndexerExpSyntax& operator=(IndexerExpSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetObject();
    SYNTAX_API ExpSyntax& GetIndex();

    SYNTAX_API JsonItem ToJson();
};

class MemberExpSyntax
{
    std::string memberName;
    std::vector<TypeExpSyntax> memberTypeArgs;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::string memberName, std::vector<TypeExpSyntax> memberTypeArgs);
    SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::string memberName);
    MemberExpSyntax(const MemberExpSyntax&) = delete;
    SYNTAX_API MemberExpSyntax(MemberExpSyntax&&) noexcept;
    SYNTAX_API ~MemberExpSyntax();

    MemberExpSyntax& operator=(const MemberExpSyntax& other) = delete;
    SYNTAX_API MemberExpSyntax& operator=(MemberExpSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetParent();
    std::string& GetMemberName() { return memberName; }
    std::vector<TypeExpSyntax>& GetMemberTypeArgs() { return memberTypeArgs; }

    SYNTAX_API JsonItem ToJson();
};

class ListExpSyntax
{
    std::vector<ExpSyntax> elems;

public:
    SYNTAX_API ListExpSyntax(std::vector<ExpSyntax> elems);
    ListExpSyntax(const ListExpSyntax&) = delete;
    SYNTAX_API ListExpSyntax(ListExpSyntax&&) noexcept;
    SYNTAX_API ~ListExpSyntax();

    ListExpSyntax& operator=(const ListExpSyntax& other) = delete;
    SYNTAX_API ListExpSyntax& operator=(ListExpSyntax&& other) noexcept;

    std::vector<ExpSyntax>& GetElems() { return elems; }

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

class BoxExpSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BoxExpSyntax(ExpSyntax innerExp);
    BoxExpSyntax(const BoxExpSyntax&) = delete;
    SYNTAX_API BoxExpSyntax(BoxExpSyntax&&) noexcept;
    SYNTAX_API ~BoxExpSyntax();

    BoxExpSyntax& operator=(const BoxExpSyntax& other) = delete;
    SYNTAX_API BoxExpSyntax& operator=(BoxExpSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetInnerExp();

    SYNTAX_API JsonItem ToJson();
};

class IsExpSyntax
{
    TypeExpSyntax type;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IsExpSyntax(ExpSyntax exp, TypeExpSyntax type);
    IsExpSyntax(const IsExpSyntax&) = delete;
    SYNTAX_API IsExpSyntax(IsExpSyntax&&) noexcept;
    SYNTAX_API ~IsExpSyntax();

    IsExpSyntax& operator=(const IsExpSyntax& other) = delete;
    SYNTAX_API IsExpSyntax& operator=(IsExpSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetExp();
    TypeExpSyntax& GetType() { return type; }

    SYNTAX_API JsonItem ToJson();
};

class AsExpSyntax
{
    TypeExpSyntax type;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API AsExpSyntax(ExpSyntax exp, TypeExpSyntax type);
    AsExpSyntax(const AsExpSyntax&) = delete;
    SYNTAX_API AsExpSyntax(AsExpSyntax&&) noexcept;
    SYNTAX_API ~AsExpSyntax();

    AsExpSyntax& operator=(const AsExpSyntax& other) = delete;
    SYNTAX_API AsExpSyntax& operator=(AsExpSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetExp();
    TypeExpSyntax& GetType() { return type; }

    SYNTAX_API JsonItem ToJson();
};

using EmbeddableStmtSyntax = std::variant<
    class SingleEmbeddableStmtSyntax,
    class BlockEmbeddableStmtSyntax>;

SYNTAX_API JsonItem ToJson(EmbeddableStmtSyntax& embeddableStmt);

class SingleEmbeddableStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API SingleEmbeddableStmtSyntax(StmtSyntax stmt);
    SingleEmbeddableStmtSyntax(const SingleEmbeddableStmtSyntax&) = delete;
    SYNTAX_API SingleEmbeddableStmtSyntax(SingleEmbeddableStmtSyntax&&) noexcept;
    SYNTAX_API ~SingleEmbeddableStmtSyntax();

    SingleEmbeddableStmtSyntax& operator=(const SingleEmbeddableStmtSyntax& other) = delete;
    SYNTAX_API SingleEmbeddableStmtSyntax& operator=(SingleEmbeddableStmtSyntax&& other) noexcept;

    SYNTAX_API StmtSyntax& GetStmt();

    SYNTAX_API JsonItem ToJson();
};

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

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API VarDeclSyntaxElement(std::string varName, std::optional<ExpSyntax> initExp);
    VarDeclSyntaxElement(const VarDeclSyntaxElement&) = delete;
    SYNTAX_API VarDeclSyntaxElement(VarDeclSyntaxElement&&) noexcept;
    SYNTAX_API ~VarDeclSyntaxElement();

    VarDeclSyntaxElement& operator=(const VarDeclSyntaxElement& other) = delete;
    SYNTAX_API VarDeclSyntaxElement& operator=(VarDeclSyntaxElement&& other) noexcept;

    std::string& GetVarName() { return varName; }
    SYNTAX_API std::optional<ExpSyntax>& GetInitExp();

    SYNTAX_API JsonItem ToJson();
};

class VarDeclSyntax
{
    TypeExpSyntax type;
    std::vector<VarDeclSyntaxElement> elems;

public:
    VarDeclSyntax(TypeExpSyntax type, std::vector<VarDeclSyntaxElement> elems)
        : type(std::move(type)), elems(std::move(elems)) { }
    VarDeclSyntax(const VarDeclSyntax&) = delete;
    VarDeclSyntax(VarDeclSyntax&&) = default;

    VarDeclSyntax& operator=(const VarDeclSyntax& other) = delete;
    VarDeclSyntax& operator=(VarDeclSyntax&& other) = default;

    TypeExpSyntax& GetType() { return type; }
    std::vector<VarDeclSyntaxElement>& GetElems() { return elems; }

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
    VarDeclStmtSyntax(VarDeclSyntax varDecl)
        : varDecl(std::move(varDecl)) { }
    VarDeclStmtSyntax(const VarDeclStmtSyntax&) = delete;
    VarDeclStmtSyntax(VarDeclStmtSyntax&&) = default;

    VarDeclStmtSyntax& operator=(const VarDeclStmtSyntax& other) = delete;
    VarDeclStmtSyntax& operator=(VarDeclStmtSyntax&& other) = default;

    VarDeclSyntax& GetVarDecl() { return varDecl; }

    SYNTAX_API JsonItem ToJson();
};

class IfStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
    IfStmtSyntax(const IfStmtSyntax&) = delete;
    SYNTAX_API IfStmtSyntax(IfStmtSyntax&&) noexcept;
    SYNTAX_API ~IfStmtSyntax();

    IfStmtSyntax& operator=(const IfStmtSyntax& other) = delete;
    SYNTAX_API IfStmtSyntax& operator=(IfStmtSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetCond();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();
    SYNTAX_API std::optional<EmbeddableStmtSyntax>& GetElseBody();

    SYNTAX_API JsonItem ToJson();
};

class IfTestStmtSyntax
{
    TypeExpSyntax testType;
    std::string varName;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IfTestStmtSyntax(TypeExpSyntax testType, std::string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
    IfTestStmtSyntax(const IfTestStmtSyntax&) = delete;
    SYNTAX_API IfTestStmtSyntax(IfTestStmtSyntax&&) noexcept;
    SYNTAX_API ~IfTestStmtSyntax();

    IfTestStmtSyntax& operator=(const IfTestStmtSyntax& other) = delete;
    SYNTAX_API IfTestStmtSyntax& operator=(IfTestStmtSyntax&& other) noexcept;

    TypeExpSyntax& GetTestType() { return testType; }
    std::string& GetVarName() { return varName; }
    SYNTAX_API ExpSyntax& GetExp();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();
    SYNTAX_API std::optional<EmbeddableStmtSyntax>& GetElseBody();

    SYNTAX_API JsonItem ToJson();
};

using ForStmtInitializerSyntax = std::variant<
    class ExpForStmtInitializerSyntax,
    class VarDeclForStmtInitializerSyntax>;

SYNTAX_API JsonItem ToJson(ForStmtInitializerSyntax& forInit);

class ExpForStmtInitializerSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ExpForStmtInitializerSyntax(ExpSyntax exp);
    ExpForStmtInitializerSyntax(const ExpForStmtInitializerSyntax&) = delete;
    SYNTAX_API ExpForStmtInitializerSyntax(ExpForStmtInitializerSyntax&&) noexcept;
    SYNTAX_API ~ExpForStmtInitializerSyntax();

    ExpForStmtInitializerSyntax& operator=(const ExpForStmtInitializerSyntax& other) = delete;
    SYNTAX_API ExpForStmtInitializerSyntax& operator=(ExpForStmtInitializerSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetExp();

    SYNTAX_API JsonItem ToJson();
};

class VarDeclForStmtInitializerSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API VarDeclForStmtInitializerSyntax(VarDeclSyntax varDecl);
    VarDeclForStmtInitializerSyntax(const VarDeclForStmtInitializerSyntax&) = delete;
    SYNTAX_API VarDeclForStmtInitializerSyntax(VarDeclForStmtInitializerSyntax&&) noexcept;
    SYNTAX_API ~VarDeclForStmtInitializerSyntax();

    VarDeclForStmtInitializerSyntax& operator=(const VarDeclForStmtInitializerSyntax& other) = delete;
    SYNTAX_API VarDeclForStmtInitializerSyntax& operator=(VarDeclForStmtInitializerSyntax&& other) noexcept;

    SYNTAX_API VarDeclSyntax& GetVarDecl();

    SYNTAX_API JsonItem ToJson();
};

class ForStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, std::optional<ExpSyntax> cond, std::optional<ExpSyntax> cont, EmbeddableStmtSyntax body);
    ForStmtSyntax(const ForStmtSyntax&) = delete;
    SYNTAX_API ForStmtSyntax(ForStmtSyntax&&) noexcept;
    SYNTAX_API ~ForStmtSyntax();

    ForStmtSyntax& operator=(const ForStmtSyntax& other) = delete;
    SYNTAX_API ForStmtSyntax& operator=(ForStmtSyntax&& other) noexcept;

    SYNTAX_API std::optional<ForStmtInitializerSyntax>& GetInitializer();
    SYNTAX_API std::optional<ExpSyntax>& GetCond();
    SYNTAX_API std::optional<ExpSyntax>& GetCont();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();

    SYNTAX_API JsonItem ToJson();
};

class ContinueStmtSyntax
{
public:
    ContinueStmtSyntax() { }
    ContinueStmtSyntax(const ContinueStmtSyntax&) = delete;
    ContinueStmtSyntax(ContinueStmtSyntax&&) = default;

    ContinueStmtSyntax& operator=(const ContinueStmtSyntax& other) = delete;
    ContinueStmtSyntax& operator=(ContinueStmtSyntax&& other) = default;

    SYNTAX_API JsonItem ToJson();
};

class BreakStmtSyntax
{
public:
    BreakStmtSyntax() { }
    BreakStmtSyntax(const BreakStmtSyntax&) = delete;
    BreakStmtSyntax(BreakStmtSyntax&&) = default;

    BreakStmtSyntax& operator=(const BreakStmtSyntax& other) = delete;
    BreakStmtSyntax& operator=(BreakStmtSyntax&& other) = default;

    SYNTAX_API JsonItem ToJson();
};

class ReturnStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ReturnStmtSyntax(std::optional<ExpSyntax> value);
    ReturnStmtSyntax(const ReturnStmtSyntax&) = delete;
    SYNTAX_API ReturnStmtSyntax(ReturnStmtSyntax&&) noexcept;
    SYNTAX_API ~ReturnStmtSyntax();

    ReturnStmtSyntax& operator=(const ReturnStmtSyntax& other) = delete;
    SYNTAX_API ReturnStmtSyntax& operator=(ReturnStmtSyntax&& other) noexcept;

    SYNTAX_API std::optional<ExpSyntax>& GetValue();

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
    BlankStmtSyntax() { }
    BlankStmtSyntax(const BlankStmtSyntax&) = delete;
    BlankStmtSyntax(BlankStmtSyntax&&) = default;

    BlankStmtSyntax& operator=(const BlankStmtSyntax& other) = delete;
    BlankStmtSyntax& operator=(BlankStmtSyntax&& other) = default;

    SYNTAX_API JsonItem ToJson();
};

class ExpStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ExpStmtSyntax(ExpSyntax exp);
    ExpStmtSyntax(const ExpStmtSyntax&) = delete;
    SYNTAX_API ExpStmtSyntax(ExpStmtSyntax&&) noexcept;
    SYNTAX_API ~ExpStmtSyntax();

    ExpStmtSyntax& operator=(const ExpStmtSyntax& other) = delete;
    SYNTAX_API ExpStmtSyntax& operator=(ExpStmtSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetExp();

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

class ForeachStmtSyntax
{
    TypeExpSyntax type;
    std::string varName;

    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ForeachStmtSyntax(TypeExpSyntax type, std::string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body);
    ForeachStmtSyntax(const ForeachStmtSyntax&) = delete;
    SYNTAX_API ForeachStmtSyntax(ForeachStmtSyntax&&) noexcept;
    SYNTAX_API ~ForeachStmtSyntax();

    ForeachStmtSyntax& operator=(const ForeachStmtSyntax& other) = delete;
    SYNTAX_API ForeachStmtSyntax& operator=(ForeachStmtSyntax&& other) noexcept;

    TypeExpSyntax& GetType() { return type; }
    std::string& GetVarName() { return varName; }
    SYNTAX_API ExpSyntax& GetEnumerable();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();

    SYNTAX_API JsonItem ToJson();
};

class YieldStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API YieldStmtSyntax(ExpSyntax value);
    YieldStmtSyntax(const YieldStmtSyntax&) = delete;
    SYNTAX_API YieldStmtSyntax(YieldStmtSyntax&&) noexcept;
    SYNTAX_API ~YieldStmtSyntax();

    YieldStmtSyntax& operator=(const YieldStmtSyntax& other) = delete;
    SYNTAX_API YieldStmtSyntax& operator=(YieldStmtSyntax&& other) noexcept;

    SYNTAX_API ExpSyntax& GetValue();

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


}
