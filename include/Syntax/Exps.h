#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>
#include <variant>

#include "TypeExps.h"
#include "UnaryOpKind.h"
#include "BinaryOpKind.h"

#include "SyntaxMacros.h"

namespace Citron::Syntax {

struct Argument;

// forward declarations
using StringExpElement = std::variant<struct TextStringExpElement, struct ExpStringExpElement>;

// NOTICE: 본판하고 선언이 맞아야 한다
using Stmt = std::variant <
    class CommandStmt,
    class VarDeclStmt,
    class IfStmt,
    class IfTestStmt,
    class ForStmt,

    class ContinueStmt,
    class BreakStmt,
    class ReturnStmt,
    class BlockStmt,
    class BlankStmt,
    class ExpStmt,

    class TaskStmt,
    class AwaitStmt,
    class AsyncStmt,
    class ForeachStmt,
    class YieldStmt,

    class DirectiveStmt
>;

using Exp = std::variant<
    class IdentifierExp,
    class StringExp,
    class IntLiteralExp,
    class BoolLiteralExp,
    class NullLiteralExp,
    class BinaryOpExp,
    class UnaryOpExp,
    class CallExp,
    class LambdaExp,
    class IndexerExp,
    class MemberExp,
    class ListExp,
    class NewExp,
    class BoxExp,
    class IsExp,
    class AsExp
>;

class IdentifierExp
{
    std::string value;
    std::vector<TypeExp> typeArgs;

public:
    IdentifierExp(std::string value, std::vector<TypeExp> typeArgs)
        : value(value), typeArgs(std::move(typeArgs)) { }

    std::string& GetValue() { return value; }
    std::vector<TypeExp>& GetTypeArgs() { return typeArgs; }
};

class StringExp
{
    std::vector<StringExpElement> elements;

public:
    SYNTAX_API StringExp(std::vector<StringExpElement> elements);
    DECLARE_DEFAULTS(StringExp)

    std::vector<StringExpElement>& GetElements() { return elements; }
};

class IntLiteralExp
{
    int value;

public:
    IntLiteralExp(int value)
        : value(value) { }

    int GetValue() { return value; }
};

class BoolLiteralExp
{
    bool value;

public:
    BoolLiteralExp(bool value)
        : value(value) { }

    bool GetValue() { return value; }
};

// null
class NullLiteralExp
{
};

// recursive, { operand0, operand1 }
class BinaryOpExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BinaryOpExp(BinaryOpKind kind, Exp operand0, Exp operand1);
    DECLARE_DEFAULTS(BinaryOpExp)

    SYNTAX_API BinaryOpKind GetKind();
    SYNTAX_API Exp& GetOperand0();
    SYNTAX_API Exp& GetOperand1();
};

class UnaryOpExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API UnaryOpExp(UnaryOpKind kind, Exp operand);
    DECLARE_DEFAULTS(UnaryOpExp)

    SYNTAX_API UnaryOpKind GetKind();
    SYNTAX_API Exp& GetOperand();
};

// recursive, { callable }
class CallExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API CallExp(Exp callable, std::vector<Argument> args);
    DECLARE_DEFAULTS(CallExp)
    
    SYNTAX_API Exp& GetCallable();
    SYNTAX_API std::vector<Argument>& GetArgs();
};

struct LambdaExpParam
{
    std::optional<TypeExp> type;
    std::string name;
    bool hasOut;
    bool hasParams;
};

class LambdaExp
{   
    std::vector<LambdaExpParam> params;
    std::vector<Stmt> body;

public:
    SYNTAX_API LambdaExp(std::vector<LambdaExpParam> params, std::vector<Stmt> body);
    DECLARE_DEFAULTS(LambdaExp)

    std::vector<LambdaExpParam>& GetParams() { return params; }
    std::vector<Stmt>& GetBody() { return body; }
};

// a[b]
// recursive, { object, index }
class IndexerExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IndexerExp(Exp obj, Exp index);
    DECLARE_DEFAULTS(IndexerExp)

    SYNTAX_API Exp& GetObject();
    SYNTAX_API Exp& GetIndex();
};

// recursive { parent }
class MemberExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API MemberExp(Exp parent, std::string memberName, std::vector<TypeExp> memberTypeArgs);
    DECLARE_DEFAULTS(MemberExp)

    SYNTAX_API Exp& GetParent();
    SYNTAX_API std::string& GetMemberName();
    SYNTAX_API std::vector<TypeExp>& GetMemberTypeArgs();
};

class ListExp
{
    std::vector<Exp> elems;

public:
    SYNTAX_API ListExp(std::vector<Exp> elems);
    DECLARE_DEFAULTS(ListExp)

    std::vector<Exp>& GetElems() { return elems; }
};

// new Type(2, 3, 4);
class NewExp
{
    TypeExp type;
    std::vector<Argument> args;

public:
    SYNTAX_API NewExp(TypeExp type, std::vector<Argument> args);
    DECLARE_DEFAULTS(NewExp); 

    TypeExp& GetType() { return type; }
    std::vector<Argument>& GetArgs() { return args; }
};

// box i
// recursive { innerExp }
class BoxExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API BoxExp(Exp innerExp);
    DECLARE_DEFAULTS(BoxExp)

    SYNTAX_API Exp& GetInnerExp();
};

// x is T
// recursive { exp }
class IsExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IsExp(Exp exp, TypeExp type);
    DECLARE_DEFAULTS(IsExp)

    SYNTAX_API Exp& GetExp();
    SYNTAX_API TypeExp& Type();
};

// x as T
class AsExp
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API AsExp(Exp exp, TypeExp type);
    DECLARE_DEFAULTS(AsExp)

    SYNTAX_API Exp& GetExp();
    SYNTAX_API TypeExp& Type();
};

} // namespace Citron::Syntax

