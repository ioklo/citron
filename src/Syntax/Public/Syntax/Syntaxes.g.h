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
class SCommandStmt;
class SVarDeclStmt;
class SIfStmt;
class SIfTestStmt;
class SForStmt;
class SContinueStmt;
class SBreakStmt;
class SReturnStmt;
class SBlockStmt;
class SBlankStmt;
class SExpStmt;
class STaskStmt;
class SAwaitStmt;
class SAsyncStmt;
class SForeachStmt;
class SYieldStmt;
class SDirectiveStmt;

class SExp;
class SIdentifierExp;
class SStringExp;
class SIntLiteralExp;
class SBoolLiteralExp;
class SNullLiteralExp;
class SBinaryOpExp;
class SUnaryOpExp;
class SCallExp;
class SLambdaExp;
class SIndexerExp;
class SMemberExp;
class SIndirectMemberExp;
class SListExp;
class SNewExp;
class SBoxExp;
class SIsExp;
class SAsExp;

class STypeExp;
class SIdTypeExp;
class SMemberTypeExp;
class SNullableTypeExp;
class SLocalPtrTypeExp;
class SBoxPtrTypeExp;
class SLocalTypeExp;

class SStringExpElement;
class STextStringExpElement;
class SExpStringExpElement;

class SLambdaExpBody;
class SStmtsLambdaExpBody;
class SExpLambdaExpBody;

class SEmbeddableStmt;
class SSingleEmbeddableStmt;
class SBlockEmbeddableStmt;

class SForStmtInitializer;
class SExpForStmtInitializer;
class SVarDeclForStmtInitializer;

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
    virtual void Visit(SCommandStmt& stmt) = 0;
    virtual void Visit(SVarDeclStmt& stmt) = 0;
    virtual void Visit(SIfStmt& stmt) = 0;
    virtual void Visit(SIfTestStmt& stmt) = 0;
    virtual void Visit(SForStmt& stmt) = 0;
    virtual void Visit(SContinueStmt& stmt) = 0;
    virtual void Visit(SBreakStmt& stmt) = 0;
    virtual void Visit(SReturnStmt& stmt) = 0;
    virtual void Visit(SBlockStmt& stmt) = 0;
    virtual void Visit(SBlankStmt& stmt) = 0;
    virtual void Visit(SExpStmt& stmt) = 0;
    virtual void Visit(STaskStmt& stmt) = 0;
    virtual void Visit(SAwaitStmt& stmt) = 0;
    virtual void Visit(SAsyncStmt& stmt) = 0;
    virtual void Visit(SForeachStmt& stmt) = 0;
    virtual void Visit(SYieldStmt& stmt) = 0;
    virtual void Visit(SDirectiveStmt& stmt) = 0;
};

class SStmt
{
public:
    virtual ~SStmt() { }
    virtual void Accept(SStmtVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStmtPtr& stmt);

class SExpVisitor
{
public:
    virtual ~SExpVisitor() { }
    virtual void Visit(SIdentifierExp& exp) = 0;
    virtual void Visit(SStringExp& exp) = 0;
    virtual void Visit(SIntLiteralExp& exp) = 0;
    virtual void Visit(SBoolLiteralExp& exp) = 0;
    virtual void Visit(SNullLiteralExp& exp) = 0;
    virtual void Visit(SBinaryOpExp& exp) = 0;
    virtual void Visit(SUnaryOpExp& exp) = 0;
    virtual void Visit(SCallExp& exp) = 0;
    virtual void Visit(SLambdaExp& exp) = 0;
    virtual void Visit(SIndexerExp& exp) = 0;
    virtual void Visit(SMemberExp& exp) = 0;
    virtual void Visit(SIndirectMemberExp& exp) = 0;
    virtual void Visit(SListExp& exp) = 0;
    virtual void Visit(SNewExp& exp) = 0;
    virtual void Visit(SBoxExp& exp) = 0;
    virtual void Visit(SIsExp& exp) = 0;
    virtual void Visit(SAsExp& exp) = 0;
};

class SExp
{
public:
    virtual ~SExp() { }
    virtual void Accept(SExpVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SExpPtr& exp);

class STypeExpVisitor
{
public:
    virtual ~STypeExpVisitor() { }
    virtual void Visit(SIdTypeExp& typeExp) = 0;
    virtual void Visit(SMemberTypeExp& typeExp) = 0;
    virtual void Visit(SNullableTypeExp& typeExp) = 0;
    virtual void Visit(SLocalPtrTypeExp& typeExp) = 0;
    virtual void Visit(SBoxPtrTypeExp& typeExp) = 0;
    virtual void Visit(SLocalTypeExp& typeExp) = 0;
};

class STypeExp
{
public:
    virtual ~STypeExp() { }
    virtual void Accept(STypeExpVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(STypeExpPtr& typeExp);

class SStringExpElementVisitor
{
public:
    virtual ~SStringExpElementVisitor() { }
    virtual void Visit(STextStringExpElement& elem) = 0;
    virtual void Visit(SExpStringExpElement& elem) = 0;
};

class SStringExpElement
{
public:
    virtual ~SStringExpElement() { }
    virtual void Accept(SStringExpElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStringExpElementPtr& elem);

class SLambdaExpBodyVisitor
{
public:
    virtual ~SLambdaExpBodyVisitor() { }
    virtual void Visit(SStmtsLambdaExpBody& body) = 0;
    virtual void Visit(SExpLambdaExpBody& body) = 0;
};

class SLambdaExpBody
{
public:
    virtual ~SLambdaExpBody() { }
    virtual void Accept(SLambdaExpBodyVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SLambdaExpBodyPtr& body);

class SEmbeddableStmtVisitor
{
public:
    virtual ~SEmbeddableStmtVisitor() { }
    virtual void Visit(SSingleEmbeddableStmt& stmt) = 0;
    virtual void Visit(SBlockEmbeddableStmt& stmt) = 0;
};

class SEmbeddableStmt
{
public:
    virtual ~SEmbeddableStmt() { }
    virtual void Accept(SEmbeddableStmtVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SEmbeddableStmtPtr& stmt);

class SForStmtInitializerVisitor
{
public:
    virtual ~SForStmtInitializerVisitor() { }
    virtual void Visit(SExpForStmtInitializer& initializer) = 0;
    virtual void Visit(SVarDeclForStmtInitializer& initializer) = 0;
};

class SForStmtInitializer
{
public:
    virtual ~SForStmtInitializer() { }
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

class SClassMemberDecl
{
public:
    virtual ~SClassMemberDecl() { }
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

class SStructMemberDecl
{
public:
    virtual ~SStructMemberDecl() { }
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

class SNamespaceDeclElement
{
public:
    virtual ~SNamespaceDeclElement() { }
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

class SScriptElement
{
public:
    virtual ~SScriptElement() { }
    virtual void Accept(SScriptElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SScriptElementPtr& elem);

class SIdentifierExp
    : public SExp
{
public:
    std::string value;
    std::vector<STypeExpPtr> typeArgs;

    SYNTAX_API SIdentifierExp(std::string value, std::vector<STypeExpPtr> typeArgs);
    SIdentifierExp(std::string value) : SIdentifierExp(std::move(value), {}) { }
    SIdentifierExp(const SIdentifierExp&) = delete;
    SYNTAX_API SIdentifierExp(SIdentifierExp&&) noexcept;
    SYNTAX_API virtual ~SIdentifierExp();

    SIdentifierExp& operator=(const SIdentifierExp& other) = delete;
    SYNTAX_API SIdentifierExp& operator=(SIdentifierExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SStringExp
    : public SExp
{
public:
    std::vector<SStringExpElementPtr> elements;

    SYNTAX_API SStringExp(std::vector<SStringExpElementPtr> elements);
    SYNTAX_API SStringExp(std::string str);
    SStringExp(const SStringExp&) = delete;
    SYNTAX_API SStringExp(SStringExp&&) noexcept;
    SYNTAX_API virtual ~SStringExp();

    SStringExp& operator=(const SStringExp& other) = delete;
    SYNTAX_API SStringExp& operator=(SStringExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIntLiteralExp
    : public SExp
{
public:
    int value;

    SYNTAX_API SIntLiteralExp(int value);
    SIntLiteralExp(const SIntLiteralExp&) = delete;
    SYNTAX_API SIntLiteralExp(SIntLiteralExp&&) noexcept;
    SYNTAX_API virtual ~SIntLiteralExp();

    SIntLiteralExp& operator=(const SIntLiteralExp& other) = delete;
    SYNTAX_API SIntLiteralExp& operator=(SIntLiteralExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBoolLiteralExp
    : public SExp
{
public:
    bool value;

    SYNTAX_API SBoolLiteralExp(bool value);
    SBoolLiteralExp(const SBoolLiteralExp&) = delete;
    SYNTAX_API SBoolLiteralExp(SBoolLiteralExp&&) noexcept;
    SYNTAX_API virtual ~SBoolLiteralExp();

    SBoolLiteralExp& operator=(const SBoolLiteralExp& other) = delete;
    SYNTAX_API SBoolLiteralExp& operator=(SBoolLiteralExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SNullLiteralExp
    : public SExp
{
public:
    SYNTAX_API SNullLiteralExp();
    SNullLiteralExp(const SNullLiteralExp&) = delete;
    SYNTAX_API SNullLiteralExp(SNullLiteralExp&&) noexcept;
    SYNTAX_API virtual ~SNullLiteralExp();

    SNullLiteralExp& operator=(const SNullLiteralExp& other) = delete;
    SYNTAX_API SNullLiteralExp& operator=(SNullLiteralExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SListExp
    : public SExp
{
public:
    std::vector<SExpPtr> elements;

    SYNTAX_API SListExp(std::vector<SExpPtr> elements);
    SListExp(const SListExp&) = delete;
    SYNTAX_API SListExp(SListExp&&) noexcept;
    SYNTAX_API virtual ~SListExp();

    SListExp& operator=(const SListExp& other) = delete;
    SYNTAX_API SListExp& operator=(SListExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SNewExp
    : public SExp
{
public:
    STypeExpPtr type;
    std::vector<SArgument> args;

    SYNTAX_API SNewExp(STypeExpPtr type, std::vector<SArgument> args);
    SNewExp(const SNewExp&) = delete;
    SYNTAX_API SNewExp(SNewExp&&) noexcept;
    SYNTAX_API virtual ~SNewExp();

    SNewExp& operator=(const SNewExp& other) = delete;
    SYNTAX_API SNewExp& operator=(SNewExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBinaryOpExp
    : public SExp
{
public:
    SBinaryOpKind kind;
    SExpPtr operand0;
    SExpPtr operand1;

    SYNTAX_API SBinaryOpExp(SBinaryOpKind kind, SExpPtr operand0, SExpPtr operand1);
    SBinaryOpExp(const SBinaryOpExp&) = delete;
    SYNTAX_API SBinaryOpExp(SBinaryOpExp&&) noexcept;
    SYNTAX_API virtual ~SBinaryOpExp();

    SBinaryOpExp& operator=(const SBinaryOpExp& other) = delete;
    SYNTAX_API SBinaryOpExp& operator=(SBinaryOpExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SUnaryOpExp
    : public SExp
{
public:
    SUnaryOpKind kind;
    SExpPtr operand;

    SYNTAX_API SUnaryOpExp(SUnaryOpKind kind, SExpPtr operand);
    SUnaryOpExp(const SUnaryOpExp&) = delete;
    SYNTAX_API SUnaryOpExp(SUnaryOpExp&&) noexcept;
    SYNTAX_API virtual ~SUnaryOpExp();

    SUnaryOpExp& operator=(const SUnaryOpExp& other) = delete;
    SYNTAX_API SUnaryOpExp& operator=(SUnaryOpExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SCallExp
    : public SExp
{
public:
    SExpPtr callable;
    std::vector<SArgument> args;

    SYNTAX_API SCallExp(SExpPtr callable, std::vector<SArgument> args);
    SCallExp(const SCallExp&) = delete;
    SYNTAX_API SCallExp(SCallExp&&) noexcept;
    SYNTAX_API virtual ~SCallExp();

    SCallExp& operator=(const SCallExp& other) = delete;
    SYNTAX_API SCallExp& operator=(SCallExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SLambdaExp
    : public SExp
{
public:
    std::vector<SLambdaExpParam> params;
    SLambdaExpBodyPtr body;

    SYNTAX_API SLambdaExp(std::vector<SLambdaExpParam> params, SLambdaExpBodyPtr body);
    SLambdaExp(const SLambdaExp&) = delete;
    SYNTAX_API SLambdaExp(SLambdaExp&&) noexcept;
    SYNTAX_API virtual ~SLambdaExp();

    SLambdaExp& operator=(const SLambdaExp& other) = delete;
    SYNTAX_API SLambdaExp& operator=(SLambdaExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIndexerExp
    : public SExp
{
public:
    SExpPtr obj;
    SExpPtr index;

    SYNTAX_API SIndexerExp(SExpPtr obj, SExpPtr index);
    SIndexerExp(const SIndexerExp&) = delete;
    SYNTAX_API SIndexerExp(SIndexerExp&&) noexcept;
    SYNTAX_API virtual ~SIndexerExp();

    SIndexerExp& operator=(const SIndexerExp& other) = delete;
    SYNTAX_API SIndexerExp& operator=(SIndexerExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SMemberExp
    : public SExp
{
public:
    SExpPtr parent;
    std::string memberName;
    std::vector<STypeExpPtr> memberTypeArgs;

    SYNTAX_API SMemberExp(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs);
    SYNTAX_API SMemberExp(SExpPtr parent, std::string memberName);
    SMemberExp(const SMemberExp&) = delete;
    SYNTAX_API SMemberExp(SMemberExp&&) noexcept;
    SYNTAX_API virtual ~SMemberExp();

    SMemberExp& operator=(const SMemberExp& other) = delete;
    SYNTAX_API SMemberExp& operator=(SMemberExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIndirectMemberExp
    : public SExp
{
public:
    SExpPtr parent;
    std::string memberName;
    std::vector<STypeExpPtr> memberTypeArgs;

    SYNTAX_API SIndirectMemberExp(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs);
    SYNTAX_API SIndirectMemberExp(SExpPtr parent, std::string memberName);
    SIndirectMemberExp(const SIndirectMemberExp&) = delete;
    SYNTAX_API SIndirectMemberExp(SIndirectMemberExp&&) noexcept;
    SYNTAX_API virtual ~SIndirectMemberExp();

    SIndirectMemberExp& operator=(const SIndirectMemberExp& other) = delete;
    SYNTAX_API SIndirectMemberExp& operator=(SIndirectMemberExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBoxExp
    : public SExp
{
public:
    SExpPtr innerExp;

    SYNTAX_API SBoxExp(SExpPtr innerExp);
    SBoxExp(const SBoxExp&) = delete;
    SYNTAX_API SBoxExp(SBoxExp&&) noexcept;
    SYNTAX_API virtual ~SBoxExp();

    SBoxExp& operator=(const SBoxExp& other) = delete;
    SYNTAX_API SBoxExp& operator=(SBoxExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIsExp
    : public SExp
{
public:
    SExpPtr exp;
    STypeExpPtr type;

    SYNTAX_API SIsExp(SExpPtr exp, STypeExpPtr type);
    SIsExp(const SIsExp&) = delete;
    SYNTAX_API SIsExp(SIsExp&&) noexcept;
    SYNTAX_API virtual ~SIsExp();

    SIsExp& operator=(const SIsExp& other) = delete;
    SYNTAX_API SIsExp& operator=(SIsExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SAsExp
    : public SExp
{
public:
    SExpPtr exp;
    STypeExpPtr type;

    SYNTAX_API SAsExp(SExpPtr exp, STypeExpPtr type);
    SAsExp(const SAsExp&) = delete;
    SYNTAX_API SAsExp(SAsExp&&) noexcept;
    SYNTAX_API virtual ~SAsExp();

    SAsExp& operator=(const SAsExp& other) = delete;
    SYNTAX_API SAsExp& operator=(SAsExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIdTypeExp
    : public STypeExp
{
public:
    std::string name;
    std::vector<STypeExpPtr> typeArgs;

    SYNTAX_API SIdTypeExp(std::string name, std::vector<STypeExpPtr> typeArgs);
    SYNTAX_API SIdTypeExp(std::string name);
    SIdTypeExp(const SIdTypeExp&) = delete;
    SYNTAX_API SIdTypeExp(SIdTypeExp&&) noexcept;
    SYNTAX_API virtual ~SIdTypeExp();

    SIdTypeExp& operator=(const SIdTypeExp& other) = delete;
    SYNTAX_API SIdTypeExp& operator=(SIdTypeExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SMemberTypeExp
    : public STypeExp
{
public:
    STypeExpPtr parentType;
    std::string name;
    std::vector<STypeExpPtr> typeArgs;

    SYNTAX_API SMemberTypeExp(STypeExpPtr parentType, std::string name, std::vector<STypeExpPtr> typeArgs);
    SMemberTypeExp(const SMemberTypeExp&) = delete;
    SYNTAX_API SMemberTypeExp(SMemberTypeExp&&) noexcept;
    SYNTAX_API virtual ~SMemberTypeExp();

    SMemberTypeExp& operator=(const SMemberTypeExp& other) = delete;
    SYNTAX_API SMemberTypeExp& operator=(SMemberTypeExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SNullableTypeExp
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API SNullableTypeExp(STypeExpPtr innerType);
    SNullableTypeExp(const SNullableTypeExp&) = delete;
    SYNTAX_API SNullableTypeExp(SNullableTypeExp&&) noexcept;
    SYNTAX_API virtual ~SNullableTypeExp();

    SNullableTypeExp& operator=(const SNullableTypeExp& other) = delete;
    SYNTAX_API SNullableTypeExp& operator=(SNullableTypeExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SLocalPtrTypeExp
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API SLocalPtrTypeExp(STypeExpPtr innerType);
    SLocalPtrTypeExp(const SLocalPtrTypeExp&) = delete;
    SYNTAX_API SLocalPtrTypeExp(SLocalPtrTypeExp&&) noexcept;
    SYNTAX_API virtual ~SLocalPtrTypeExp();

    SLocalPtrTypeExp& operator=(const SLocalPtrTypeExp& other) = delete;
    SYNTAX_API SLocalPtrTypeExp& operator=(SLocalPtrTypeExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBoxPtrTypeExp
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API SBoxPtrTypeExp(STypeExpPtr innerType);
    SBoxPtrTypeExp(const SBoxPtrTypeExp&) = delete;
    SYNTAX_API SBoxPtrTypeExp(SBoxPtrTypeExp&&) noexcept;
    SYNTAX_API virtual ~SBoxPtrTypeExp();

    SBoxPtrTypeExp& operator=(const SBoxPtrTypeExp& other) = delete;
    SYNTAX_API SBoxPtrTypeExp& operator=(SBoxPtrTypeExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SLocalTypeExp
    : public STypeExp
{
public:
    STypeExpPtr innerType;

    SYNTAX_API SLocalTypeExp(STypeExpPtr innerType);
    SLocalTypeExp(const SLocalTypeExp&) = delete;
    SYNTAX_API SLocalTypeExp(SLocalTypeExp&&) noexcept;
    SYNTAX_API virtual ~SLocalTypeExp();

    SLocalTypeExp& operator=(const SLocalTypeExp& other) = delete;
    SYNTAX_API SLocalTypeExp& operator=(SLocalTypeExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STextStringExpElement
    : public SStringExpElement
{
public:
    std::string text;

    SYNTAX_API STextStringExpElement(std::string text);
    STextStringExpElement(const STextStringExpElement&) = delete;
    SYNTAX_API STextStringExpElement(STextStringExpElement&&) noexcept;
    SYNTAX_API virtual ~STextStringExpElement();

    STextStringExpElement& operator=(const STextStringExpElement& other) = delete;
    SYNTAX_API STextStringExpElement& operator=(STextStringExpElement&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpStringExpElement
    : public SStringExpElement
{
public:
    SExpPtr exp;

    SYNTAX_API SExpStringExpElement(SExpPtr exp);
    SExpStringExpElement(const SExpStringExpElement&) = delete;
    SYNTAX_API SExpStringExpElement(SExpStringExpElement&&) noexcept;
    SYNTAX_API virtual ~SExpStringExpElement();

    SExpStringExpElement& operator=(const SExpStringExpElement& other) = delete;
    SYNTAX_API SExpStringExpElement& operator=(SExpStringExpElement&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmtsLambdaExpBody
    : public SLambdaExpBody
{
public:
    std::vector<SStmtPtr> stmts;

    SYNTAX_API SStmtsLambdaExpBody(std::vector<SStmtPtr> stmts);
    SStmtsLambdaExpBody(const SStmtsLambdaExpBody&) = delete;
    SYNTAX_API SStmtsLambdaExpBody(SStmtsLambdaExpBody&&) noexcept;
    SYNTAX_API virtual ~SStmtsLambdaExpBody();

    SStmtsLambdaExpBody& operator=(const SStmtsLambdaExpBody& other) = delete;
    SYNTAX_API SStmtsLambdaExpBody& operator=(SStmtsLambdaExpBody&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpLambdaExpBody
    : public SLambdaExpBody
{
public:
    SExpPtr exp;

    SYNTAX_API SExpLambdaExpBody(SExpPtr exp);
    SExpLambdaExpBody(const SExpLambdaExpBody&) = delete;
    SYNTAX_API SExpLambdaExpBody(SExpLambdaExpBody&&) noexcept;
    SYNTAX_API virtual ~SExpLambdaExpBody();

    SExpLambdaExpBody& operator=(const SExpLambdaExpBody& other) = delete;
    SYNTAX_API SExpLambdaExpBody& operator=(SExpLambdaExpBody&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(*this); }

};

class SSingleEmbeddableStmt
    : public SEmbeddableStmt
{
public:
    SStmtPtr stmt;

    SYNTAX_API SSingleEmbeddableStmt(SStmtPtr stmt);
    SSingleEmbeddableStmt(const SSingleEmbeddableStmt&) = delete;
    SYNTAX_API SSingleEmbeddableStmt(SSingleEmbeddableStmt&&) noexcept;
    SYNTAX_API virtual ~SSingleEmbeddableStmt();

    SSingleEmbeddableStmt& operator=(const SSingleEmbeddableStmt& other) = delete;
    SYNTAX_API SSingleEmbeddableStmt& operator=(SSingleEmbeddableStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SBlockEmbeddableStmt
    : public SEmbeddableStmt
{
public:
    std::vector<SStmtPtr> stmts;

    SYNTAX_API SBlockEmbeddableStmt(std::vector<SStmtPtr> stmts);
    SBlockEmbeddableStmt(const SBlockEmbeddableStmt&) = delete;
    SYNTAX_API SBlockEmbeddableStmt(SBlockEmbeddableStmt&&) noexcept;
    SYNTAX_API virtual ~SBlockEmbeddableStmt();

    SBlockEmbeddableStmt& operator=(const SBlockEmbeddableStmt& other) = delete;
    SYNTAX_API SBlockEmbeddableStmt& operator=(SBlockEmbeddableStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpForStmtInitializer
    : public SForStmtInitializer
{
public:
    SExpPtr exp;

    SYNTAX_API SExpForStmtInitializer(SExpPtr exp);
    SExpForStmtInitializer(const SExpForStmtInitializer&) = delete;
    SYNTAX_API SExpForStmtInitializer(SExpForStmtInitializer&&) noexcept;
    SYNTAX_API virtual ~SExpForStmtInitializer();

    SExpForStmtInitializer& operator=(const SExpForStmtInitializer& other) = delete;
    SYNTAX_API SExpForStmtInitializer& operator=(SExpForStmtInitializer&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(*this); }

};

class SVarDeclForStmtInitializer
    : public SForStmtInitializer
{
public:
    SVarDecl varDecl;

    SYNTAX_API SVarDeclForStmtInitializer(SVarDecl varDecl);
    SVarDeclForStmtInitializer(const SVarDeclForStmtInitializer&) = delete;
    SYNTAX_API SVarDeclForStmtInitializer(SVarDeclForStmtInitializer&&) noexcept;
    SYNTAX_API virtual ~SVarDeclForStmtInitializer();

    SVarDeclForStmtInitializer& operator=(const SVarDeclForStmtInitializer& other) = delete;
    SYNTAX_API SVarDeclForStmtInitializer& operator=(SVarDeclForStmtInitializer&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(*this); }

};

class SCommandStmt
    : public SStmt
{
public:
    std::vector<std::shared_ptr<SStringExp>> commands;

    SYNTAX_API SCommandStmt(std::vector<std::shared_ptr<SStringExp>> commands);
    SCommandStmt(const SCommandStmt&) = delete;
    SYNTAX_API SCommandStmt(SCommandStmt&&) noexcept;
    SYNTAX_API virtual ~SCommandStmt();

    SCommandStmt& operator=(const SCommandStmt& other) = delete;
    SYNTAX_API SCommandStmt& operator=(SCommandStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SVarDeclStmt
    : public SStmt
{
public:
    SVarDecl varDecl;

    SYNTAX_API SVarDeclStmt(SVarDecl varDecl);
    SVarDeclStmt(const SVarDeclStmt&) = delete;
    SYNTAX_API SVarDeclStmt(SVarDeclStmt&&) noexcept;
    SYNTAX_API virtual ~SVarDeclStmt();

    SVarDeclStmt& operator=(const SVarDeclStmt& other) = delete;
    SYNTAX_API SVarDeclStmt& operator=(SVarDeclStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SContinueStmt
    : public SStmt
{
public:
    SYNTAX_API SContinueStmt();
    SContinueStmt(const SContinueStmt&) = delete;
    SYNTAX_API SContinueStmt(SContinueStmt&&) noexcept;
    SYNTAX_API virtual ~SContinueStmt();

    SContinueStmt& operator=(const SContinueStmt& other) = delete;
    SYNTAX_API SContinueStmt& operator=(SContinueStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SBreakStmt
    : public SStmt
{
public:
    SYNTAX_API SBreakStmt();
    SBreakStmt(const SBreakStmt&) = delete;
    SYNTAX_API SBreakStmt(SBreakStmt&&) noexcept;
    SYNTAX_API virtual ~SBreakStmt();

    SBreakStmt& operator=(const SBreakStmt& other) = delete;
    SYNTAX_API SBreakStmt& operator=(SBreakStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SBlockStmt
    : public SStmt
{
public:
    std::vector<SStmtPtr> stmts;

    SYNTAX_API SBlockStmt(std::vector<SStmtPtr> stmts);
    SBlockStmt(const SBlockStmt&) = delete;
    SYNTAX_API SBlockStmt(SBlockStmt&&) noexcept;
    SYNTAX_API virtual ~SBlockStmt();

    SBlockStmt& operator=(const SBlockStmt& other) = delete;
    SYNTAX_API SBlockStmt& operator=(SBlockStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SBlankStmt
    : public SStmt
{
public:
    SYNTAX_API SBlankStmt();
    SBlankStmt(const SBlankStmt&) = delete;
    SYNTAX_API SBlankStmt(SBlankStmt&&) noexcept;
    SYNTAX_API virtual ~SBlankStmt();

    SBlankStmt& operator=(const SBlankStmt& other) = delete;
    SYNTAX_API SBlankStmt& operator=(SBlankStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class STaskStmt
    : public SStmt
{
public:
    std::vector<SStmtPtr> body;

    SYNTAX_API STaskStmt(std::vector<SStmtPtr> body);
    STaskStmt(const STaskStmt&) = delete;
    SYNTAX_API STaskStmt(STaskStmt&&) noexcept;
    SYNTAX_API virtual ~STaskStmt();

    STaskStmt& operator=(const STaskStmt& other) = delete;
    SYNTAX_API STaskStmt& operator=(STaskStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SAwaitStmt
    : public SStmt
{
public:
    std::vector<SStmtPtr> body;

    SYNTAX_API SAwaitStmt(std::vector<SStmtPtr> body);
    SAwaitStmt(const SAwaitStmt&) = delete;
    SYNTAX_API SAwaitStmt(SAwaitStmt&&) noexcept;
    SYNTAX_API virtual ~SAwaitStmt();

    SAwaitStmt& operator=(const SAwaitStmt& other) = delete;
    SYNTAX_API SAwaitStmt& operator=(SAwaitStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SAsyncStmt
    : public SStmt
{
public:
    std::vector<SStmtPtr> body;

    SYNTAX_API SAsyncStmt(std::vector<SStmtPtr> body);
    SAsyncStmt(const SAsyncStmt&) = delete;
    SYNTAX_API SAsyncStmt(SAsyncStmt&&) noexcept;
    SYNTAX_API virtual ~SAsyncStmt();

    SAsyncStmt& operator=(const SAsyncStmt& other) = delete;
    SYNTAX_API SAsyncStmt& operator=(SAsyncStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SDirectiveStmt
    : public SStmt
{
public:
    std::string name;
    std::vector<SExpPtr> args;

    SYNTAX_API SDirectiveStmt(std::string name, std::vector<SExpPtr> args);
    SDirectiveStmt(const SDirectiveStmt&) = delete;
    SYNTAX_API SDirectiveStmt(SDirectiveStmt&&) noexcept;
    SYNTAX_API virtual ~SDirectiveStmt();

    SDirectiveStmt& operator=(const SDirectiveStmt& other) = delete;
    SYNTAX_API SDirectiveStmt& operator=(SDirectiveStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SIfStmt
    : public SStmt
{
public:
    SExpPtr cond;
    SEmbeddableStmtPtr body;
    SEmbeddableStmtPtr elseBody;

    SYNTAX_API SIfStmt(SExpPtr cond, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
    SIfStmt(const SIfStmt&) = delete;
    SYNTAX_API SIfStmt(SIfStmt&&) noexcept;
    SYNTAX_API virtual ~SIfStmt();

    SIfStmt& operator=(const SIfStmt& other) = delete;
    SYNTAX_API SIfStmt& operator=(SIfStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SIfTestStmt
    : public SStmt
{
public:
    STypeExpPtr testType;
    std::string varName;
    SExpPtr exp;
    SEmbeddableStmtPtr body;
    SEmbeddableStmtPtr elseBody;

    SYNTAX_API SIfTestStmt(STypeExpPtr testType, std::string varName, SExpPtr exp, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
    SIfTestStmt(const SIfTestStmt&) = delete;
    SYNTAX_API SIfTestStmt(SIfTestStmt&&) noexcept;
    SYNTAX_API virtual ~SIfTestStmt();

    SIfTestStmt& operator=(const SIfTestStmt& other) = delete;
    SYNTAX_API SIfTestStmt& operator=(SIfTestStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SForStmt
    : public SStmt
{
public:
    SForStmtInitializerPtr initializer;
    SExpPtr cond;
    SExpPtr cont;
    SEmbeddableStmtPtr body;

    SYNTAX_API SForStmt(SForStmtInitializerPtr initializer, SExpPtr cond, SExpPtr cont, SEmbeddableStmtPtr body);
    SForStmt(const SForStmt&) = delete;
    SYNTAX_API SForStmt(SForStmt&&) noexcept;
    SYNTAX_API virtual ~SForStmt();

    SForStmt& operator=(const SForStmt& other) = delete;
    SYNTAX_API SForStmt& operator=(SForStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SReturnStmt
    : public SStmt
{
public:
    SExpPtr value;

    SYNTAX_API SReturnStmt(SExpPtr value);
    SReturnStmt(const SReturnStmt&) = delete;
    SYNTAX_API SReturnStmt(SReturnStmt&&) noexcept;
    SYNTAX_API virtual ~SReturnStmt();

    SReturnStmt& operator=(const SReturnStmt& other) = delete;
    SYNTAX_API SReturnStmt& operator=(SReturnStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpStmt
    : public SStmt
{
public:
    SExpPtr exp;

    SYNTAX_API SExpStmt(SExpPtr exp);
    SExpStmt(const SExpStmt&) = delete;
    SYNTAX_API SExpStmt(SExpStmt&&) noexcept;
    SYNTAX_API virtual ~SExpStmt();

    SExpStmt& operator=(const SExpStmt& other) = delete;
    SYNTAX_API SExpStmt& operator=(SExpStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SForeachStmt
    : public SStmt
{
public:
    STypeExpPtr type;
    std::string varName;
    SExpPtr enumerable;
    SEmbeddableStmtPtr body;

    SYNTAX_API SForeachStmt(STypeExpPtr type, std::string varName, SExpPtr enumerable, SEmbeddableStmtPtr body);
    SForeachStmt(const SForeachStmt&) = delete;
    SYNTAX_API SForeachStmt(SForeachStmt&&) noexcept;
    SYNTAX_API virtual ~SForeachStmt();

    SForeachStmt& operator=(const SForeachStmt& other) = delete;
    SYNTAX_API SForeachStmt& operator=(SForeachStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SYieldStmt
    : public SStmt
{
public:
    SExpPtr value;

    SYNTAX_API SYieldStmt(SExpPtr value);
    SYieldStmt(const SYieldStmt&) = delete;
    SYNTAX_API SYieldStmt(SYieldStmt&&) noexcept;
    SYNTAX_API virtual ~SYieldStmt();

    SYieldStmt& operator=(const SYieldStmt& other) = delete;
    SYNTAX_API SYieldStmt& operator=(SYieldStmt&& other) noexcept;

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
