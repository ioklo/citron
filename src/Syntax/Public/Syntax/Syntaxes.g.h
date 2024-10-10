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

class SStmt;
class SStmt_Command;
class SStmt_VarDecl;
class SStmt_If;
class SStmt_IfTest;
class SStmt_For;
class SStmt_Continue;
class SStmt_Break;
class SStmt_Return;
class SStmt_Block;
class SStmt_Blank;
class SStmt_Exp;
class SStmt_Task;
class SStmt_Await;
class SStmt_Async;
class SStmt_Foreach;
class SStmt_Yield;
class SStmt_Directive;

class SExp;
class SExp_Identifier;
class SExp_String;
class SExp_IntLiteral;
class SExp_BoolLiteral;
class SExp_NullLiteral;
class SExp_BinaryOp;
class SExp_UnaryOp;
class SExp_Call;
class SExp_Lambda;
class SExp_Indexer;
class SExp_Member;
class SExp_IndirectMember;
class SExp_List;
class SExp_New;
class SExp_Box;
class SExp_Is;
class SExp_As;

class STypeExp;
class STypeExp_Id;
class STypeExp_Member;
class STypeExp_Nullable;
class STypeExp_LocalPtr;
class STypeExp_BoxPtr;
class STypeExp_Local;

class SStringExpElement;
class SStringExpElement_Text;
class SStringExpElement_Exp;

class SLambdaExpBody;
class SLambdaExpBody_Stmts;
class SLambdaExpBody_Exp;

class SEmbeddableStmt;
class SEmbeddableStmt_Single;
class SEmbeddableStmt_Block;

class SForStmtInitializer;
class SForStmtInitializer_Exp;
class SForStmtInitializer_VarDecl;

class SClassMemberDecl;
class SClassMemberFuncDecl;
class SClassConstructorDecl;
class SClassMemberVarDecl;

class SStructMemberDecl;
class SStructMemberFuncDecl;
class SStructConstructorDecl;
class SStructMemberVarDecl;

class SNamespaceDeclElement;
class SScriptElement;

class SClassDecl;
class SStructDecl;
class SEnumDecl;
class SGlobalFuncDecl;
class SNamespaceDecl;
class SScript;

using SStmtPtr = std::shared_ptr<SStmt>;
using SExpPtr = std::shared_ptr<SExp>;
using STypeExpPtr = std::shared_ptr<STypeExp>;
using SStringExpElementPtr = std::shared_ptr<SStringExpElement>;
using SLambdaExpBodyPtr = std::shared_ptr<SLambdaExpBody>;
using SEmbeddableStmtPtr = std::shared_ptr<SEmbeddableStmt>;
using SForStmtInitializerPtr = std::shared_ptr<SForStmtInitializer>;
using SClassMemberDeclPtr = std::shared_ptr<SClassMemberDecl>;
using SStructMemberDeclPtr = std::shared_ptr<SStructMemberDecl>;
using SNamespaceDeclElementPtr = std::shared_ptr<SNamespaceDeclElement>;
using SScriptElementPtr = std::shared_ptr<SScriptElement>;
enum class SAccessModifier
{
    Public,
    Protected,
    Private,
};

inline JsonItem ToJson(SAccessModifier& arg)
{
    switch(arg)
    {
    case SAccessModifier::Public: return JsonString("Public");
    case SAccessModifier::Protected: return JsonString("Protected");
    case SAccessModifier::Private: return JsonString("Private");
    }
    unreachable();
}

enum class SBinaryOpKind
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

inline JsonItem ToJson(SBinaryOpKind& arg)
{
    switch(arg)
    {
    case SBinaryOpKind::Multiply: return JsonString("Multiply");
    case SBinaryOpKind::Divide: return JsonString("Divide");
    case SBinaryOpKind::Modulo: return JsonString("Modulo");
    case SBinaryOpKind::Add: return JsonString("Add");
    case SBinaryOpKind::Subtract: return JsonString("Subtract");
    case SBinaryOpKind::LessThan: return JsonString("LessThan");
    case SBinaryOpKind::GreaterThan: return JsonString("GreaterThan");
    case SBinaryOpKind::LessThanOrEqual: return JsonString("LessThanOrEqual");
    case SBinaryOpKind::GreaterThanOrEqual: return JsonString("GreaterThanOrEqual");
    case SBinaryOpKind::Equal: return JsonString("Equal");
    case SBinaryOpKind::NotEqual: return JsonString("NotEqual");
    case SBinaryOpKind::Assign: return JsonString("Assign");
    }
    unreachable();
}

enum class SUnaryOpKind
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

inline JsonItem ToJson(SUnaryOpKind& arg)
{
    switch(arg)
    {
    case SUnaryOpKind::PostfixInc: return JsonString("PostfixInc");
    case SUnaryOpKind::PostfixDec: return JsonString("PostfixDec");
    case SUnaryOpKind::Minus: return JsonString("Minus");
    case SUnaryOpKind::LogicalNot: return JsonString("LogicalNot");
    case SUnaryOpKind::PrefixInc: return JsonString("PrefixInc");
    case SUnaryOpKind::PrefixDec: return JsonString("PrefixDec");
    case SUnaryOpKind::Ref: return JsonString("Ref");
    case SUnaryOpKind::Deref: return JsonString("Deref");
    }
    unreachable();
}

class SSyntax
{
public:
    SYNTAX_API SSyntax();
    SSyntax(const SSyntax&) = delete;
    SYNTAX_API SSyntax(SSyntax&&) noexcept;
    SYNTAX_API ~SSyntax();

    SSyntax& operator=(const SSyntax& other) = delete;
    SYNTAX_API SSyntax& operator=(SSyntax&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SArgument
{
public:
    bool bOut;
    bool bParams;
    SExpPtr exp;

    SYNTAX_API SArgument(bool bOut, bool bParams, SExpPtr exp);
    SYNTAX_API SArgument(SExpPtr exp);
    SArgument(const SArgument&) = delete;
    SYNTAX_API SArgument(SArgument&&) noexcept;
    SYNTAX_API ~SArgument();

    SArgument& operator=(const SArgument& other) = delete;
    SYNTAX_API SArgument& operator=(SArgument&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SLambdaExpParam
{
public:
    STypeExpPtr type;
    std::string name;
    bool hasOut;
    bool hasParams;

    SYNTAX_API SLambdaExpParam(STypeExpPtr type, std::string name, bool hasOut, bool hasParams);
    SLambdaExpParam(const SLambdaExpParam&) = delete;
    SYNTAX_API SLambdaExpParam(SLambdaExpParam&&) noexcept;
    SYNTAX_API ~SLambdaExpParam();

    SLambdaExpParam& operator=(const SLambdaExpParam& other) = delete;
    SYNTAX_API SLambdaExpParam& operator=(SLambdaExpParam&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SVarDeclElement
{
public:
    std::string varName;
    SExpPtr initExp;

    SYNTAX_API SVarDeclElement(std::string varName, SExpPtr initExp);
    SVarDeclElement(const SVarDeclElement&) = delete;
    SYNTAX_API SVarDeclElement(SVarDeclElement&&) noexcept;
    SYNTAX_API ~SVarDeclElement();

    SVarDeclElement& operator=(const SVarDeclElement& other) = delete;
    SYNTAX_API SVarDeclElement& operator=(SVarDeclElement&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SVarDecl
{
public:
    STypeExpPtr type;
    std::vector<SVarDeclElement> elements;

    SYNTAX_API SVarDecl(STypeExpPtr type, std::vector<SVarDeclElement> elements);
    SVarDecl(const SVarDecl&) = delete;
    SYNTAX_API SVarDecl(SVarDecl&&) noexcept;
    SYNTAX_API ~SVarDecl();

    SVarDecl& operator=(const SVarDecl& other) = delete;
    SYNTAX_API SVarDecl& operator=(SVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class STypeParam
{
public:
    std::string name;

    SYNTAX_API STypeParam(std::string name);
    STypeParam(const STypeParam&) = delete;
    SYNTAX_API STypeParam(STypeParam&&) noexcept;
    SYNTAX_API ~STypeParam();

    STypeParam& operator=(const STypeParam& other) = delete;
    SYNTAX_API STypeParam& operator=(STypeParam&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SFuncParam
{
public:
    bool hasOut;
    bool hasParams;
    STypeExpPtr type;
    std::string name;

    SYNTAX_API SFuncParam(bool hasOut, bool hasParams, STypeExpPtr type, std::string name);
    SFuncParam(const SFuncParam&) = delete;
    SYNTAX_API SFuncParam(SFuncParam&&) noexcept;
    SYNTAX_API ~SFuncParam();

    SFuncParam& operator=(const SFuncParam& other) = delete;
    SYNTAX_API SFuncParam& operator=(SFuncParam&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SStmtVisitor
{
public:
    virtual ~SStmtVisitor() { }
    virtual void Visit(SStmt_Command& stmt) = 0;
    virtual void Visit(SStmt_VarDecl& stmt) = 0;
    virtual void Visit(SStmt_If& stmt) = 0;
    virtual void Visit(SStmt_IfTest& stmt) = 0;
    virtual void Visit(SStmt_For& stmt) = 0;
    virtual void Visit(SStmt_Continue& stmt) = 0;
    virtual void Visit(SStmt_Break& stmt) = 0;
    virtual void Visit(SStmt_Return& stmt) = 0;
    virtual void Visit(SStmt_Block& stmt) = 0;
    virtual void Visit(SStmt_Blank& stmt) = 0;
    virtual void Visit(SStmt_Exp& stmt) = 0;
    virtual void Visit(SStmt_Task& stmt) = 0;
    virtual void Visit(SStmt_Await& stmt) = 0;
    virtual void Visit(SStmt_Async& stmt) = 0;
    virtual void Visit(SStmt_Foreach& stmt) = 0;
    virtual void Visit(SStmt_Yield& stmt) = 0;
    virtual void Visit(SStmt_Directive& stmt) = 0;
};

class SStmt : public SSyntax
{
public:
    SStmt() = default;
    SStmt(const SStmt&) = delete;
    SStmt(SStmt&&) = default;
    virtual ~SStmt() { }
    SStmt& operator=(const SStmt& other) = delete;
    SStmt& operator=(SStmt&& other) noexcept = default;
    virtual void Accept(SStmtVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStmtPtr& stmt);

class SExpVisitor
{
public:
    virtual ~SExpVisitor() { }
    virtual void Visit(SExp_Identifier& exp) = 0;
    virtual void Visit(SExp_String& exp) = 0;
    virtual void Visit(SExp_IntLiteral& exp) = 0;
    virtual void Visit(SExp_BoolLiteral& exp) = 0;
    virtual void Visit(SExp_NullLiteral& exp) = 0;
    virtual void Visit(SExp_BinaryOp& exp) = 0;
    virtual void Visit(SExp_UnaryOp& exp) = 0;
    virtual void Visit(SExp_Call& exp) = 0;
    virtual void Visit(SExp_Lambda& exp) = 0;
    virtual void Visit(SExp_Indexer& exp) = 0;
    virtual void Visit(SExp_Member& exp) = 0;
    virtual void Visit(SExp_IndirectMember& exp) = 0;
    virtual void Visit(SExp_List& exp) = 0;
    virtual void Visit(SExp_New& exp) = 0;
    virtual void Visit(SExp_Box& exp) = 0;
    virtual void Visit(SExp_Is& exp) = 0;
    virtual void Visit(SExp_As& exp) = 0;
};

class SExp : public SSyntax
{
public:
    SExp() = default;
    SExp(const SExp&) = delete;
    SExp(SExp&&) = default;
    virtual ~SExp() { }
    SExp& operator=(const SExp& other) = delete;
    SExp& operator=(SExp&& other) noexcept = default;
    virtual void Accept(SExpVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SExpPtr& exp);

class STypeExpVisitor
{
public:
    virtual ~STypeExpVisitor() { }
    virtual void Visit(STypeExp_Id& typeExp) = 0;
    virtual void Visit(STypeExp_Member& typeExp) = 0;
    virtual void Visit(STypeExp_Nullable& typeExp) = 0;
    virtual void Visit(STypeExp_LocalPtr& typeExp) = 0;
    virtual void Visit(STypeExp_BoxPtr& typeExp) = 0;
    virtual void Visit(STypeExp_Local& typeExp) = 0;
};

class STypeExp : public SSyntax
{
public:
    STypeExp() = default;
    STypeExp(const STypeExp&) = delete;
    STypeExp(STypeExp&&) = default;
    virtual ~STypeExp() { }
    STypeExp& operator=(const STypeExp& other) = delete;
    STypeExp& operator=(STypeExp&& other) noexcept = default;
    virtual void Accept(STypeExpVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(STypeExpPtr& typeExp);

class SStringExpElementVisitor
{
public:
    virtual ~SStringExpElementVisitor() { }
    virtual void Visit(SStringExpElement_Text& elem) = 0;
    virtual void Visit(SStringExpElement_Exp& elem) = 0;
};

class SStringExpElement : public SSyntax
{
public:
    SStringExpElement() = default;
    SStringExpElement(const SStringExpElement&) = delete;
    SStringExpElement(SStringExpElement&&) = default;
    virtual ~SStringExpElement() { }
    SStringExpElement& operator=(const SStringExpElement& other) = delete;
    SStringExpElement& operator=(SStringExpElement&& other) noexcept = default;
    virtual void Accept(SStringExpElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStringExpElementPtr& elem);

class SLambdaExpBodyVisitor
{
public:
    virtual ~SLambdaExpBodyVisitor() { }
    virtual void Visit(SLambdaExpBody_Stmts& body) = 0;
    virtual void Visit(SLambdaExpBody_Exp& body) = 0;
};

class SLambdaExpBody : public SSyntax
{
public:
    SLambdaExpBody() = default;
    SLambdaExpBody(const SLambdaExpBody&) = delete;
    SLambdaExpBody(SLambdaExpBody&&) = default;
    virtual ~SLambdaExpBody() { }
    SLambdaExpBody& operator=(const SLambdaExpBody& other) = delete;
    SLambdaExpBody& operator=(SLambdaExpBody&& other) noexcept = default;
    virtual void Accept(SLambdaExpBodyVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SLambdaExpBodyPtr& body);

class SEmbeddableStmtVisitor
{
public:
    virtual ~SEmbeddableStmtVisitor() { }
    virtual void Visit(SEmbeddableStmt_Single& stmt) = 0;
    virtual void Visit(SEmbeddableStmt_Block& stmt) = 0;
};

class SEmbeddableStmt : public SSyntax
{
public:
    SEmbeddableStmt() = default;
    SEmbeddableStmt(const SEmbeddableStmt&) = delete;
    SEmbeddableStmt(SEmbeddableStmt&&) = default;
    virtual ~SEmbeddableStmt() { }
    SEmbeddableStmt& operator=(const SEmbeddableStmt& other) = delete;
    SEmbeddableStmt& operator=(SEmbeddableStmt&& other) noexcept = default;
    virtual void Accept(SEmbeddableStmtVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SEmbeddableStmtPtr& stmt);

class SForStmtInitializerVisitor
{
public:
    virtual ~SForStmtInitializerVisitor() { }
    virtual void Visit(SForStmtInitializer_Exp& initializer) = 0;
    virtual void Visit(SForStmtInitializer_VarDecl& initializer) = 0;
};

class SForStmtInitializer : public SSyntax
{
public:
    SForStmtInitializer() = default;
    SForStmtInitializer(const SForStmtInitializer&) = delete;
    SForStmtInitializer(SForStmtInitializer&&) = default;
    virtual ~SForStmtInitializer() { }
    SForStmtInitializer& operator=(const SForStmtInitializer& other) = delete;
    SForStmtInitializer& operator=(SForStmtInitializer&& other) noexcept = default;
    virtual void Accept(SForStmtInitializerVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SForStmtInitializerPtr& initializer);

class SClassMemberDeclVisitor
{
public:
    virtual ~SClassMemberDeclVisitor() { }
    virtual void Visit(SClassDecl& decl) = 0;
    virtual void Visit(SStructDecl& decl) = 0;
    virtual void Visit(SEnumDecl& decl) = 0;
    virtual void Visit(SClassMemberFuncDecl& decl) = 0;
    virtual void Visit(SClassConstructorDecl& decl) = 0;
    virtual void Visit(SClassMemberVarDecl& decl) = 0;
};

class SClassMemberDecl : public SSyntax
{
public:
    SClassMemberDecl() = default;
    SClassMemberDecl(const SClassMemberDecl&) = delete;
    SClassMemberDecl(SClassMemberDecl&&) = default;
    virtual ~SClassMemberDecl() { }
    SClassMemberDecl& operator=(const SClassMemberDecl& other) = delete;
    SClassMemberDecl& operator=(SClassMemberDecl&& other) noexcept = default;
    virtual void Accept(SClassMemberDeclVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SClassMemberDeclPtr& decl);

class SStructMemberDeclVisitor
{
public:
    virtual ~SStructMemberDeclVisitor() { }
    virtual void Visit(SClassDecl& decl) = 0;
    virtual void Visit(SStructDecl& decl) = 0;
    virtual void Visit(SEnumDecl& decl) = 0;
    virtual void Visit(SStructMemberFuncDecl& decl) = 0;
    virtual void Visit(SStructConstructorDecl& decl) = 0;
    virtual void Visit(SStructMemberVarDecl& decl) = 0;
};

class SStructMemberDecl : public SSyntax
{
public:
    SStructMemberDecl() = default;
    SStructMemberDecl(const SStructMemberDecl&) = delete;
    SStructMemberDecl(SStructMemberDecl&&) = default;
    virtual ~SStructMemberDecl() { }
    SStructMemberDecl& operator=(const SStructMemberDecl& other) = delete;
    SStructMemberDecl& operator=(SStructMemberDecl&& other) noexcept = default;
    virtual void Accept(SStructMemberDeclVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStructMemberDeclPtr& decl);

class SNamespaceDeclElementVisitor
{
public:
    virtual ~SNamespaceDeclElementVisitor() { }
    virtual void Visit(SGlobalFuncDecl& elem) = 0;
    virtual void Visit(SNamespaceDecl& elem) = 0;
    virtual void Visit(SClassDecl& elem) = 0;
    virtual void Visit(SStructDecl& elem) = 0;
    virtual void Visit(SEnumDecl& elem) = 0;
};

class SNamespaceDeclElement : public SSyntax
{
public:
    SNamespaceDeclElement() = default;
    SNamespaceDeclElement(const SNamespaceDeclElement&) = delete;
    SNamespaceDeclElement(SNamespaceDeclElement&&) = default;
    virtual ~SNamespaceDeclElement() { }
    SNamespaceDeclElement& operator=(const SNamespaceDeclElement& other) = delete;
    SNamespaceDeclElement& operator=(SNamespaceDeclElement&& other) noexcept = default;
    virtual void Accept(SNamespaceDeclElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SNamespaceDeclElementPtr& elem);

class SScriptElementVisitor
{
public:
    virtual ~SScriptElementVisitor() { }
    virtual void Visit(SNamespaceDecl& elem) = 0;
    virtual void Visit(SGlobalFuncDecl& elem) = 0;
    virtual void Visit(SClassDecl& elem) = 0;
    virtual void Visit(SStructDecl& elem) = 0;
    virtual void Visit(SEnumDecl& elem) = 0;
};

class SScriptElement : public SSyntax
{
public:
    SScriptElement() = default;
    SScriptElement(const SScriptElement&) = delete;
    SScriptElement(SScriptElement&&) = default;
    virtual ~SScriptElement() { }
    SScriptElement& operator=(const SScriptElement& other) = delete;
    SScriptElement& operator=(SScriptElement&& other) noexcept = default;
    virtual void Accept(SScriptElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SScriptElementPtr& elem);

class SExp_Identifier
    : public SExp
{
public:
    std::string value;
    std::vector<STypeExpPtr> typeArgs;

    SYNTAX_API SExp_Identifier(std::string value, std::vector<STypeExpPtr> typeArgs);
    SExp_Identifier(std::string value) : SExp_Identifier(std::move(value), {}) { }
    SExp_Identifier(const SExp_Identifier&) = delete;
    SYNTAX_API SExp_Identifier(SExp_Identifier&&) noexcept;
    SYNTAX_API virtual ~SExp_Identifier();

    SExp_Identifier& operator=(const SExp_Identifier& other) = delete;
    SYNTAX_API SExp_Identifier& operator=(SExp_Identifier&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_String
    : public SExp
{
public:
    std::vector<SStringExpElementPtr> elements;

    SYNTAX_API SExp_String(std::vector<SStringExpElementPtr> elements);
    SYNTAX_API SExp_String(std::string str);
    SExp_String(const SExp_String&) = delete;
    SYNTAX_API SExp_String(SExp_String&&) noexcept;
    SYNTAX_API virtual ~SExp_String();

    SExp_String& operator=(const SExp_String& other) = delete;
    SYNTAX_API SExp_String& operator=(SExp_String&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_IntLiteral
    : public SExp
{
public:
    int value;

    SYNTAX_API SExp_IntLiteral(int value);
    SExp_IntLiteral(const SExp_IntLiteral&) = delete;
    SYNTAX_API SExp_IntLiteral(SExp_IntLiteral&&) noexcept;
    SYNTAX_API virtual ~SExp_IntLiteral();

    SExp_IntLiteral& operator=(const SExp_IntLiteral& other) = delete;
    SYNTAX_API SExp_IntLiteral& operator=(SExp_IntLiteral&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_BoolLiteral
    : public SExp
{
public:
    bool value;

    SYNTAX_API SExp_BoolLiteral(bool value);
    SExp_BoolLiteral(const SExp_BoolLiteral&) = delete;
    SYNTAX_API SExp_BoolLiteral(SExp_BoolLiteral&&) noexcept;
    SYNTAX_API virtual ~SExp_BoolLiteral();

    SExp_BoolLiteral& operator=(const SExp_BoolLiteral& other) = delete;
    SYNTAX_API SExp_BoolLiteral& operator=(SExp_BoolLiteral&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_NullLiteral
    : public SExp
{
public:
    SYNTAX_API SExp_NullLiteral();
    SExp_NullLiteral(const SExp_NullLiteral&) = delete;
    SYNTAX_API SExp_NullLiteral(SExp_NullLiteral&&) noexcept;
    SYNTAX_API virtual ~SExp_NullLiteral();

    SExp_NullLiteral& operator=(const SExp_NullLiteral& other) = delete;
    SYNTAX_API SExp_NullLiteral& operator=(SExp_NullLiteral&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_List
    : public SExp
{
public:
    std::vector<SExpPtr> elements;

    SYNTAX_API SExp_List(std::vector<SExpPtr> elements);
    SExp_List(const SExp_List&) = delete;
    SYNTAX_API SExp_List(SExp_List&&) noexcept;
    SYNTAX_API virtual ~SExp_List();

    SExp_List& operator=(const SExp_List& other) = delete;
    SYNTAX_API SExp_List& operator=(SExp_List&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_New
    : public SExp
{
public:
    STypeExpPtr type;
    std::vector<SArgument> args;

    SYNTAX_API SExp_New(STypeExpPtr type, std::vector<SArgument> args);
    SExp_New(const SExp_New&) = delete;
    SYNTAX_API SExp_New(SExp_New&&) noexcept;
    SYNTAX_API virtual ~SExp_New();

    SExp_New& operator=(const SExp_New& other) = delete;
    SYNTAX_API SExp_New& operator=(SExp_New&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_BinaryOp
    : public SExp
{
public:
    SBinaryOpKind kind;
    SExpPtr operand0;
    SExpPtr operand1;

    SYNTAX_API SExp_BinaryOp(SBinaryOpKind kind, SExpPtr operand0, SExpPtr operand1);
    SExp_BinaryOp(const SExp_BinaryOp&) = delete;
    SYNTAX_API SExp_BinaryOp(SExp_BinaryOp&&) noexcept;
    SYNTAX_API virtual ~SExp_BinaryOp();

    SExp_BinaryOp& operator=(const SExp_BinaryOp& other) = delete;
    SYNTAX_API SExp_BinaryOp& operator=(SExp_BinaryOp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_UnaryOp
    : public SExp
{
public:
    SUnaryOpKind kind;
    SExpPtr operand;

    SYNTAX_API SExp_UnaryOp(SUnaryOpKind kind, SExpPtr operand);
    SExp_UnaryOp(const SExp_UnaryOp&) = delete;
    SYNTAX_API SExp_UnaryOp(SExp_UnaryOp&&) noexcept;
    SYNTAX_API virtual ~SExp_UnaryOp();

    SExp_UnaryOp& operator=(const SExp_UnaryOp& other) = delete;
    SYNTAX_API SExp_UnaryOp& operator=(SExp_UnaryOp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_Call
    : public SExp
{
public:
    SExpPtr callable;
    std::vector<SArgument> args;

    SYNTAX_API SExp_Call(SExpPtr callable, std::vector<SArgument> args);
    SExp_Call(const SExp_Call&) = delete;
    SYNTAX_API SExp_Call(SExp_Call&&) noexcept;
    SYNTAX_API virtual ~SExp_Call();

    SExp_Call& operator=(const SExp_Call& other) = delete;
    SYNTAX_API SExp_Call& operator=(SExp_Call&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_Lambda
    : public SExp
{
public:
    std::vector<SLambdaExpParam> params;
    SLambdaExpBodyPtr body;

    SYNTAX_API SExp_Lambda(std::vector<SLambdaExpParam> params, SLambdaExpBodyPtr body);
    SExp_Lambda(const SExp_Lambda&) = delete;
    SYNTAX_API SExp_Lambda(SExp_Lambda&&) noexcept;
    SYNTAX_API virtual ~SExp_Lambda();

    SExp_Lambda& operator=(const SExp_Lambda& other) = delete;
    SYNTAX_API SExp_Lambda& operator=(SExp_Lambda&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_Indexer
    : public SExp
{
public:
    SExpPtr obj;
    SExpPtr index;

    SYNTAX_API SExp_Indexer(SExpPtr obj, SExpPtr index);
    SExp_Indexer(const SExp_Indexer&) = delete;
    SYNTAX_API SExp_Indexer(SExp_Indexer&&) noexcept;
    SYNTAX_API virtual ~SExp_Indexer();

    SExp_Indexer& operator=(const SExp_Indexer& other) = delete;
    SYNTAX_API SExp_Indexer& operator=(SExp_Indexer&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_Member
    : public SExp
{
public:
    SExpPtr parent;
    std::string memberName;
    std::vector<STypeExpPtr> memberTypeArgs;

    SYNTAX_API SExp_Member(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs);
    SYNTAX_API SExp_Member(SExpPtr parent, std::string memberName);
    SExp_Member(const SExp_Member&) = delete;
    SYNTAX_API SExp_Member(SExp_Member&&) noexcept;
    SYNTAX_API virtual ~SExp_Member();

    SExp_Member& operator=(const SExp_Member& other) = delete;
    SYNTAX_API SExp_Member& operator=(SExp_Member&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_IndirectMember
    : public SExp
{
public:
    SExpPtr parent;
    std::string memberName;
    std::vector<STypeExpPtr> memberTypeArgs;

    SYNTAX_API SExp_IndirectMember(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs);
    SYNTAX_API SExp_IndirectMember(SExpPtr parent, std::string memberName);
    SExp_IndirectMember(const SExp_IndirectMember&) = delete;
    SYNTAX_API SExp_IndirectMember(SExp_IndirectMember&&) noexcept;
    SYNTAX_API virtual ~SExp_IndirectMember();

    SExp_IndirectMember& operator=(const SExp_IndirectMember& other) = delete;
    SYNTAX_API SExp_IndirectMember& operator=(SExp_IndirectMember&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_Box
    : public SExp
{
public:
    SExpPtr innerExp;

    SYNTAX_API SExp_Box(SExpPtr innerExp);
    SExp_Box(const SExp_Box&) = delete;
    SYNTAX_API SExp_Box(SExp_Box&&) noexcept;
    SYNTAX_API virtual ~SExp_Box();

    SExp_Box& operator=(const SExp_Box& other) = delete;
    SYNTAX_API SExp_Box& operator=(SExp_Box&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_Is
    : public SExp
{
public:
    SExpPtr exp;
    STypeExpPtr type;

    SYNTAX_API SExp_Is(SExpPtr exp, STypeExpPtr type);
    SExp_Is(const SExp_Is&) = delete;
    SYNTAX_API SExp_Is(SExp_Is&&) noexcept;
    SYNTAX_API virtual ~SExp_Is();

    SExp_Is& operator=(const SExp_Is& other) = delete;
    SYNTAX_API SExp_Is& operator=(SExp_Is&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SExp_As
    : public SExp
{
public:
    SExpPtr exp;
    STypeExpPtr type;

    SYNTAX_API SExp_As(SExpPtr exp, STypeExpPtr type);
    SExp_As(const SExp_As&) = delete;
    SYNTAX_API SExp_As(SExp_As&&) noexcept;
    SYNTAX_API virtual ~SExp_As();

    SExp_As& operator=(const SExp_As& other) = delete;
    SYNTAX_API SExp_As& operator=(SExp_As&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STypeExp_Id
    : public STypeExp
{
public:
    std::string name;
    std::vector<STypeExpPtr> typeArgs;

    SYNTAX_API STypeExp_Id(std::string name, std::vector<STypeExpPtr> typeArgs);
    SYNTAX_API STypeExp_Id(std::string name);
    STypeExp_Id(const STypeExp_Id&) = delete;
    SYNTAX_API STypeExp_Id(STypeExp_Id&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Id();

    STypeExp_Id& operator=(const STypeExp_Id& other) = delete;
    SYNTAX_API STypeExp_Id& operator=(STypeExp_Id&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STypeExp_Member
    : public STypeExp
{
public:
    STypeExpPtr parentType;
    std::string name;
    std::vector<STypeExpPtr> typeArgs;

    SYNTAX_API STypeExp_Member(STypeExpPtr parentType, std::string name, std::vector<STypeExpPtr> typeArgs);
    STypeExp_Member(const STypeExp_Member&) = delete;
    SYNTAX_API STypeExp_Member(STypeExp_Member&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Member();

    STypeExp_Member& operator=(const STypeExp_Member& other) = delete;
    SYNTAX_API STypeExp_Member& operator=(STypeExp_Member&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STypeExp_Nullable
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API STypeExp_Nullable(STypeExpPtr innerType);
    STypeExp_Nullable(const STypeExp_Nullable&) = delete;
    SYNTAX_API STypeExp_Nullable(STypeExp_Nullable&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Nullable();

    STypeExp_Nullable& operator=(const STypeExp_Nullable& other) = delete;
    SYNTAX_API STypeExp_Nullable& operator=(STypeExp_Nullable&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STypeExp_LocalPtr
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API STypeExp_LocalPtr(STypeExpPtr innerType);
    STypeExp_LocalPtr(const STypeExp_LocalPtr&) = delete;
    SYNTAX_API STypeExp_LocalPtr(STypeExp_LocalPtr&&) noexcept;
    SYNTAX_API virtual ~STypeExp_LocalPtr();

    STypeExp_LocalPtr& operator=(const STypeExp_LocalPtr& other) = delete;
    SYNTAX_API STypeExp_LocalPtr& operator=(STypeExp_LocalPtr&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STypeExp_BoxPtr
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API STypeExp_BoxPtr(STypeExpPtr innerType);
    STypeExp_BoxPtr(const STypeExp_BoxPtr&) = delete;
    SYNTAX_API STypeExp_BoxPtr(STypeExp_BoxPtr&&) noexcept;
    SYNTAX_API virtual ~STypeExp_BoxPtr();

    STypeExp_BoxPtr& operator=(const STypeExp_BoxPtr& other) = delete;
    SYNTAX_API STypeExp_BoxPtr& operator=(STypeExp_BoxPtr&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STypeExp_Local
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API STypeExp_Local(STypeExpPtr innerType);
    STypeExp_Local(const STypeExp_Local&) = delete;
    SYNTAX_API STypeExp_Local(STypeExp_Local&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Local();

    STypeExp_Local& operator=(const STypeExp_Local& other) = delete;
    SYNTAX_API STypeExp_Local& operator=(STypeExp_Local&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SStringExpElement_Text
    : public SStringExpElement
{
public:
    std::string text;

    SYNTAX_API SStringExpElement_Text(std::string text);
    SStringExpElement_Text(const SStringExpElement_Text&) = delete;
    SYNTAX_API SStringExpElement_Text(SStringExpElement_Text&&) noexcept;
    SYNTAX_API virtual ~SStringExpElement_Text();

    SStringExpElement_Text& operator=(const SStringExpElement_Text& other) = delete;
    SYNTAX_API SStringExpElement_Text& operator=(SStringExpElement_Text&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SStringExpElement_Exp
    : public SStringExpElement
{
public:
    SExpPtr exp;

    SYNTAX_API SStringExpElement_Exp(SExpPtr exp);
    SStringExpElement_Exp(const SStringExpElement_Exp&) = delete;
    SYNTAX_API SStringExpElement_Exp(SStringExpElement_Exp&&) noexcept;
    SYNTAX_API virtual ~SStringExpElement_Exp();

    SStringExpElement_Exp& operator=(const SStringExpElement_Exp& other) = delete;
    SYNTAX_API SStringExpElement_Exp& operator=(SStringExpElement_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SLambdaExpBody_Stmts
    : public SLambdaExpBody
{
public:
    std::vector<SStmtPtr> stmts;

    SYNTAX_API SLambdaExpBody_Stmts(std::vector<SStmtPtr> stmts);
    SLambdaExpBody_Stmts(const SLambdaExpBody_Stmts&) = delete;
    SYNTAX_API SLambdaExpBody_Stmts(SLambdaExpBody_Stmts&&) noexcept;
    SYNTAX_API virtual ~SLambdaExpBody_Stmts();

    SLambdaExpBody_Stmts& operator=(const SLambdaExpBody_Stmts& other) = delete;
    SYNTAX_API SLambdaExpBody_Stmts& operator=(SLambdaExpBody_Stmts&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(*this); }

};

class SLambdaExpBody_Exp
    : public SLambdaExpBody
{
public:
    SExpPtr exp;

    SYNTAX_API SLambdaExpBody_Exp(SExpPtr exp);
    SLambdaExpBody_Exp(const SLambdaExpBody_Exp&) = delete;
    SYNTAX_API SLambdaExpBody_Exp(SLambdaExpBody_Exp&&) noexcept;
    SYNTAX_API virtual ~SLambdaExpBody_Exp();

    SLambdaExpBody_Exp& operator=(const SLambdaExpBody_Exp& other) = delete;
    SYNTAX_API SLambdaExpBody_Exp& operator=(SLambdaExpBody_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(*this); }

};

class SEmbeddableStmt_Single
    : public SEmbeddableStmt
{
public:
    SStmtPtr stmt;

    SYNTAX_API SEmbeddableStmt_Single(SStmtPtr stmt);
    SEmbeddableStmt_Single(const SEmbeddableStmt_Single&) = delete;
    SYNTAX_API SEmbeddableStmt_Single(SEmbeddableStmt_Single&&) noexcept;
    SYNTAX_API virtual ~SEmbeddableStmt_Single();

    SEmbeddableStmt_Single& operator=(const SEmbeddableStmt_Single& other) = delete;
    SYNTAX_API SEmbeddableStmt_Single& operator=(SEmbeddableStmt_Single&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SEmbeddableStmt_Block
    : public SEmbeddableStmt
{
public:
    std::vector<SStmtPtr> stmts;

    SYNTAX_API SEmbeddableStmt_Block(std::vector<SStmtPtr> stmts);
    SEmbeddableStmt_Block(const SEmbeddableStmt_Block&) = delete;
    SYNTAX_API SEmbeddableStmt_Block(SEmbeddableStmt_Block&&) noexcept;
    SYNTAX_API virtual ~SEmbeddableStmt_Block();

    SEmbeddableStmt_Block& operator=(const SEmbeddableStmt_Block& other) = delete;
    SYNTAX_API SEmbeddableStmt_Block& operator=(SEmbeddableStmt_Block&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SForStmtInitializer_Exp
    : public SForStmtInitializer
{
public:
    SExpPtr exp;

    SYNTAX_API SForStmtInitializer_Exp(SExpPtr exp);
    SForStmtInitializer_Exp(const SForStmtInitializer_Exp&) = delete;
    SYNTAX_API SForStmtInitializer_Exp(SForStmtInitializer_Exp&&) noexcept;
    SYNTAX_API virtual ~SForStmtInitializer_Exp();

    SForStmtInitializer_Exp& operator=(const SForStmtInitializer_Exp& other) = delete;
    SYNTAX_API SForStmtInitializer_Exp& operator=(SForStmtInitializer_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(*this); }

};

class SForStmtInitializer_VarDecl
    : public SForStmtInitializer
{
public:
    SVarDecl varDecl;

    SYNTAX_API SForStmtInitializer_VarDecl(SVarDecl varDecl);
    SForStmtInitializer_VarDecl(const SForStmtInitializer_VarDecl&) = delete;
    SYNTAX_API SForStmtInitializer_VarDecl(SForStmtInitializer_VarDecl&&) noexcept;
    SYNTAX_API virtual ~SForStmtInitializer_VarDecl();

    SForStmtInitializer_VarDecl& operator=(const SForStmtInitializer_VarDecl& other) = delete;
    SYNTAX_API SForStmtInitializer_VarDecl& operator=(SForStmtInitializer_VarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Command
    : public SStmt
{
public:
    std::vector<std::shared_ptr<SExp_String>> commands;

    SYNTAX_API SStmt_Command(std::vector<std::shared_ptr<SExp_String>> commands);
    SStmt_Command(const SStmt_Command&) = delete;
    SYNTAX_API SStmt_Command(SStmt_Command&&) noexcept;
    SYNTAX_API virtual ~SStmt_Command();

    SStmt_Command& operator=(const SStmt_Command& other) = delete;
    SYNTAX_API SStmt_Command& operator=(SStmt_Command&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_VarDecl
    : public SStmt
{
public:
    SVarDecl varDecl;

    SYNTAX_API SStmt_VarDecl(SVarDecl varDecl);
    SStmt_VarDecl(const SStmt_VarDecl&) = delete;
    SYNTAX_API SStmt_VarDecl(SStmt_VarDecl&&) noexcept;
    SYNTAX_API virtual ~SStmt_VarDecl();

    SStmt_VarDecl& operator=(const SStmt_VarDecl& other) = delete;
    SYNTAX_API SStmt_VarDecl& operator=(SStmt_VarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Continue
    : public SStmt
{
public:
    SYNTAX_API SStmt_Continue();
    SStmt_Continue(const SStmt_Continue&) = delete;
    SYNTAX_API SStmt_Continue(SStmt_Continue&&) noexcept;
    SYNTAX_API virtual ~SStmt_Continue();

    SStmt_Continue& operator=(const SStmt_Continue& other) = delete;
    SYNTAX_API SStmt_Continue& operator=(SStmt_Continue&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Break
    : public SStmt
{
public:
    SYNTAX_API SStmt_Break();
    SStmt_Break(const SStmt_Break&) = delete;
    SYNTAX_API SStmt_Break(SStmt_Break&&) noexcept;
    SYNTAX_API virtual ~SStmt_Break();

    SStmt_Break& operator=(const SStmt_Break& other) = delete;
    SYNTAX_API SStmt_Break& operator=(SStmt_Break&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Block
    : public SStmt
{
public:
    std::vector<SStmtPtr> stmts;

    SYNTAX_API SStmt_Block(std::vector<SStmtPtr> stmts);
    SStmt_Block(const SStmt_Block&) = delete;
    SYNTAX_API SStmt_Block(SStmt_Block&&) noexcept;
    SYNTAX_API virtual ~SStmt_Block();

    SStmt_Block& operator=(const SStmt_Block& other) = delete;
    SYNTAX_API SStmt_Block& operator=(SStmt_Block&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Blank
    : public SStmt
{
public:
    SYNTAX_API SStmt_Blank();
    SStmt_Blank(const SStmt_Blank&) = delete;
    SYNTAX_API SStmt_Blank(SStmt_Blank&&) noexcept;
    SYNTAX_API virtual ~SStmt_Blank();

    SStmt_Blank& operator=(const SStmt_Blank& other) = delete;
    SYNTAX_API SStmt_Blank& operator=(SStmt_Blank&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Task
    : public SStmt
{
public:
    std::vector<SStmtPtr> body;

    SYNTAX_API SStmt_Task(std::vector<SStmtPtr> body);
    SStmt_Task(const SStmt_Task&) = delete;
    SYNTAX_API SStmt_Task(SStmt_Task&&) noexcept;
    SYNTAX_API virtual ~SStmt_Task();

    SStmt_Task& operator=(const SStmt_Task& other) = delete;
    SYNTAX_API SStmt_Task& operator=(SStmt_Task&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Await
    : public SStmt
{
public:
    std::vector<SStmtPtr> body;

    SYNTAX_API SStmt_Await(std::vector<SStmtPtr> body);
    SStmt_Await(const SStmt_Await&) = delete;
    SYNTAX_API SStmt_Await(SStmt_Await&&) noexcept;
    SYNTAX_API virtual ~SStmt_Await();

    SStmt_Await& operator=(const SStmt_Await& other) = delete;
    SYNTAX_API SStmt_Await& operator=(SStmt_Await&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Async
    : public SStmt
{
public:
    std::vector<SStmtPtr> body;

    SYNTAX_API SStmt_Async(std::vector<SStmtPtr> body);
    SStmt_Async(const SStmt_Async&) = delete;
    SYNTAX_API SStmt_Async(SStmt_Async&&) noexcept;
    SYNTAX_API virtual ~SStmt_Async();

    SStmt_Async& operator=(const SStmt_Async& other) = delete;
    SYNTAX_API SStmt_Async& operator=(SStmt_Async&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Directive
    : public SStmt
{
public:
    std::string name;
    std::vector<SExpPtr> args;

    SYNTAX_API SStmt_Directive(std::string name, std::vector<SExpPtr> args);
    SStmt_Directive(const SStmt_Directive&) = delete;
    SYNTAX_API SStmt_Directive(SStmt_Directive&&) noexcept;
    SYNTAX_API virtual ~SStmt_Directive();

    SStmt_Directive& operator=(const SStmt_Directive& other) = delete;
    SYNTAX_API SStmt_Directive& operator=(SStmt_Directive&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_If
    : public SStmt
{
public:
    SExpPtr cond;
    SEmbeddableStmtPtr body;
    SEmbeddableStmtPtr elseBody;

    SYNTAX_API SStmt_If(SExpPtr cond, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
    SStmt_If(const SStmt_If&) = delete;
    SYNTAX_API SStmt_If(SStmt_If&&) noexcept;
    SYNTAX_API virtual ~SStmt_If();

    SStmt_If& operator=(const SStmt_If& other) = delete;
    SYNTAX_API SStmt_If& operator=(SStmt_If&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_IfTest
    : public SStmt
{
public:
    STypeExpPtr testType;
    std::string varName;
    SExpPtr exp;
    SEmbeddableStmtPtr body;
    SEmbeddableStmtPtr elseBody;

    SYNTAX_API SStmt_IfTest(STypeExpPtr testType, std::string varName, SExpPtr exp, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
    SStmt_IfTest(const SStmt_IfTest&) = delete;
    SYNTAX_API SStmt_IfTest(SStmt_IfTest&&) noexcept;
    SYNTAX_API virtual ~SStmt_IfTest();

    SStmt_IfTest& operator=(const SStmt_IfTest& other) = delete;
    SYNTAX_API SStmt_IfTest& operator=(SStmt_IfTest&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_For
    : public SStmt
{
public:
    SForStmtInitializerPtr initializer;
    SExpPtr cond;
    SExpPtr cont;
    SEmbeddableStmtPtr body;

    SYNTAX_API SStmt_For(SForStmtInitializerPtr initializer, SExpPtr cond, SExpPtr cont, SEmbeddableStmtPtr body);
    SStmt_For(const SStmt_For&) = delete;
    SYNTAX_API SStmt_For(SStmt_For&&) noexcept;
    SYNTAX_API virtual ~SStmt_For();

    SStmt_For& operator=(const SStmt_For& other) = delete;
    SYNTAX_API SStmt_For& operator=(SStmt_For&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Return
    : public SStmt
{
public:
    SExpPtr value;

    SYNTAX_API SStmt_Return(SExpPtr value);
    SStmt_Return(const SStmt_Return&) = delete;
    SYNTAX_API SStmt_Return(SStmt_Return&&) noexcept;
    SYNTAX_API virtual ~SStmt_Return();

    SStmt_Return& operator=(const SStmt_Return& other) = delete;
    SYNTAX_API SStmt_Return& operator=(SStmt_Return&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Exp
    : public SStmt
{
public:
    SExpPtr exp;

    SYNTAX_API SStmt_Exp(SExpPtr exp);
    SStmt_Exp(const SStmt_Exp&) = delete;
    SYNTAX_API SStmt_Exp(SStmt_Exp&&) noexcept;
    SYNTAX_API virtual ~SStmt_Exp();

    SStmt_Exp& operator=(const SStmt_Exp& other) = delete;
    SYNTAX_API SStmt_Exp& operator=(SStmt_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Foreach
    : public SStmt
{
public:
    STypeExpPtr type;
    std::string varName;
    SExpPtr enumerable;
    SEmbeddableStmtPtr body;

    SYNTAX_API SStmt_Foreach(STypeExpPtr type, std::string varName, SExpPtr enumerable, SEmbeddableStmtPtr body);
    SStmt_Foreach(const SStmt_Foreach&) = delete;
    SYNTAX_API SStmt_Foreach(SStmt_Foreach&&) noexcept;
    SYNTAX_API virtual ~SStmt_Foreach();

    SStmt_Foreach& operator=(const SStmt_Foreach& other) = delete;
    SYNTAX_API SStmt_Foreach& operator=(SStmt_Foreach&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmt_Yield
    : public SStmt
{
public:
    SExpPtr value;

    SYNTAX_API SStmt_Yield(SExpPtr value);
    SStmt_Yield(const SStmt_Yield&) = delete;
    SYNTAX_API SStmt_Yield(SStmt_Yield&&) noexcept;
    SYNTAX_API virtual ~SStmt_Yield();

    SStmt_Yield& operator=(const SStmt_Yield& other) = delete;
    SYNTAX_API SStmt_Yield& operator=(SStmt_Yield&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SGlobalFuncDecl
    : public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    bool bSequence;
    STypeExpPtr retType;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

    SYNTAX_API SGlobalFuncDecl(std::optional<SAccessModifier> accessModifier, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SGlobalFuncDecl(const SGlobalFuncDecl&) = delete;
    SYNTAX_API SGlobalFuncDecl(SGlobalFuncDecl&&) noexcept;
    SYNTAX_API virtual ~SGlobalFuncDecl();

    SGlobalFuncDecl& operator=(const SGlobalFuncDecl& other) = delete;
    SYNTAX_API SGlobalFuncDecl& operator=(SGlobalFuncDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SClassDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<STypeExpPtr> baseTypes;
    std::vector<SClassMemberDeclPtr> memberDecls;

    SYNTAX_API SClassDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExpPtr> baseTypes, std::vector<SClassMemberDeclPtr> memberDecls);
    SClassDecl(const SClassDecl&) = delete;
    SYNTAX_API SClassDecl(SClassDecl&&) noexcept;
    SYNTAX_API virtual ~SClassDecl();

    SClassDecl& operator=(const SClassDecl& other) = delete;
    SYNTAX_API SClassDecl& operator=(SClassDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SClassMemberFuncDecl
    : public SClassMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    bool bStatic;
    bool bSequence;
    STypeExpPtr retType;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

    SYNTAX_API SClassMemberFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SClassMemberFuncDecl(const SClassMemberFuncDecl&) = delete;
    SYNTAX_API SClassMemberFuncDecl(SClassMemberFuncDecl&&) noexcept;
    SYNTAX_API virtual ~SClassMemberFuncDecl();

    SClassMemberFuncDecl& operator=(const SClassMemberFuncDecl& other) = delete;
    SYNTAX_API SClassMemberFuncDecl& operator=(SClassMemberFuncDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SClassConstructorDecl
    : public SClassMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::vector<SFuncParam> parameters;
    std::optional<std::vector<SArgument>> baseArgs;
    std::vector<SStmtPtr> body;

    SYNTAX_API SClassConstructorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, std::optional<std::vector<SArgument>> baseArgs, std::vector<SStmtPtr> body);
    SClassConstructorDecl(const SClassConstructorDecl&) = delete;
    SYNTAX_API SClassConstructorDecl(SClassConstructorDecl&&) noexcept;
    SYNTAX_API virtual ~SClassConstructorDecl();

    SClassConstructorDecl& operator=(const SClassConstructorDecl& other) = delete;
    SYNTAX_API SClassConstructorDecl& operator=(SClassConstructorDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SClassMemberVarDecl
    : public SClassMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    STypeExpPtr varType;
    std::vector<std::string> varNames;

    SYNTAX_API SClassMemberVarDecl(std::optional<SAccessModifier> accessModifier, STypeExpPtr varType, std::vector<std::string> varNames);
    SClassMemberVarDecl(const SClassMemberVarDecl&) = delete;
    SYNTAX_API SClassMemberVarDecl(SClassMemberVarDecl&&) noexcept;
    SYNTAX_API virtual ~SClassMemberVarDecl();

    SClassMemberVarDecl& operator=(const SClassMemberVarDecl& other) = delete;
    SYNTAX_API SClassMemberVarDecl& operator=(SClassMemberVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<STypeExpPtr> baseTypes;
    std::vector<SStructMemberDeclPtr> memberDecls;

    SYNTAX_API SStructDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExpPtr> baseTypes, std::vector<SStructMemberDeclPtr> memberDecls);
    SStructDecl(const SStructDecl&) = delete;
    SYNTAX_API SStructDecl(SStructDecl&&) noexcept;
    SYNTAX_API virtual ~SStructDecl();

    SStructDecl& operator=(const SStructDecl& other) = delete;
    SYNTAX_API SStructDecl& operator=(SStructDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructMemberFuncDecl
    : public SStructMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    bool bStatic;
    bool bSequence;
    STypeExpPtr retType;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

    SYNTAX_API SStructMemberFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SStructMemberFuncDecl(const SStructMemberFuncDecl&) = delete;
    SYNTAX_API SStructMemberFuncDecl(SStructMemberFuncDecl&&) noexcept;
    SYNTAX_API virtual ~SStructMemberFuncDecl();

    SStructMemberFuncDecl& operator=(const SStructMemberFuncDecl& other) = delete;
    SYNTAX_API SStructMemberFuncDecl& operator=(SStructMemberFuncDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructConstructorDecl
    : public SStructMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

    SYNTAX_API SStructConstructorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SStructConstructorDecl(const SStructConstructorDecl&) = delete;
    SYNTAX_API SStructConstructorDecl(SStructConstructorDecl&&) noexcept;
    SYNTAX_API virtual ~SStructConstructorDecl();

    SStructConstructorDecl& operator=(const SStructConstructorDecl& other) = delete;
    SYNTAX_API SStructConstructorDecl& operator=(SStructConstructorDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructMemberVarDecl
    : public SStructMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    STypeExpPtr varType;
    std::vector<std::string> varNames;

    SYNTAX_API SStructMemberVarDecl(std::optional<SAccessModifier> accessModifier, STypeExpPtr varType, std::vector<std::string> varNames);
    SStructMemberVarDecl(const SStructMemberVarDecl&) = delete;
    SYNTAX_API SStructMemberVarDecl(SStructMemberVarDecl&&) noexcept;
    SYNTAX_API virtual ~SStructMemberVarDecl();

    SStructMemberVarDecl& operator=(const SStructMemberVarDecl& other) = delete;
    SYNTAX_API SStructMemberVarDecl& operator=(SStructMemberVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SEnumElemMemberVarDecl
{
public:
    STypeExpPtr type;
    std::string name;

    SYNTAX_API SEnumElemMemberVarDecl(STypeExpPtr type, std::string name);
    SEnumElemMemberVarDecl(const SEnumElemMemberVarDecl&) = delete;
    SYNTAX_API SEnumElemMemberVarDecl(SEnumElemMemberVarDecl&&) noexcept;
    SYNTAX_API ~SEnumElemMemberVarDecl();

    SEnumElemMemberVarDecl& operator=(const SEnumElemMemberVarDecl& other) = delete;
    SYNTAX_API SEnumElemMemberVarDecl& operator=(SEnumElemMemberVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SEnumElemDecl
{
public:
    std::string name;
    std::vector<std::shared_ptr<SEnumElemMemberVarDecl>> memberVars;

    SYNTAX_API SEnumElemDecl(std::string name, std::vector<std::shared_ptr<SEnumElemMemberVarDecl>> memberVars);
    SEnumElemDecl(const SEnumElemDecl&) = delete;
    SYNTAX_API SEnumElemDecl(SEnumElemDecl&&) noexcept;
    SYNTAX_API ~SEnumElemDecl();

    SEnumElemDecl& operator=(const SEnumElemDecl& other) = delete;
    SYNTAX_API SEnumElemDecl& operator=(SEnumElemDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SEnumDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<std::shared_ptr<SEnumElemDecl>> elements;

    SYNTAX_API SEnumDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<std::shared_ptr<SEnumElemDecl>> elements);
    SEnumDecl(const SEnumDecl&) = delete;
    SYNTAX_API SEnumDecl(SEnumDecl&&) noexcept;
    SYNTAX_API virtual ~SEnumDecl();

    SEnumDecl& operator=(const SEnumDecl& other) = delete;
    SYNTAX_API SEnumDecl& operator=(SEnumDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SNamespaceDecl
    : public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::vector<std::string> names;
    std::vector<SNamespaceDeclElementPtr> elements;

    SYNTAX_API SNamespaceDecl(std::vector<std::string> names, std::vector<SNamespaceDeclElementPtr> elements);
    SNamespaceDecl(const SNamespaceDecl&) = delete;
    SYNTAX_API SNamespaceDecl(SNamespaceDecl&&) noexcept;
    SYNTAX_API virtual ~SNamespaceDecl();

    SNamespaceDecl& operator=(const SNamespaceDecl& other) = delete;
    SYNTAX_API SNamespaceDecl& operator=(SNamespaceDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SScript
{
public:
    std::vector<SScriptElementPtr> elements;

    SYNTAX_API SScript(std::vector<SScriptElementPtr> elements);
    SScript(const SScript&) = delete;
    SYNTAX_API SScript(SScript&&) noexcept;
    SYNTAX_API ~SScript();

    SScript& operator=(const SScript& other) = delete;
    SYNTAX_API SScript& operator=(SScript&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};


}
