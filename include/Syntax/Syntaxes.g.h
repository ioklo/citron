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
class SArgument;

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

using SStmtPtr = std::unique_ptr<SStmt>;
using SExpPtr = std::unique_ptr<SExp>;
using STypeExpPtr = std::unique_ptr<STypeExp>;
using SStringExpElementPtr = std::unique_ptr<SStringExpElement>;
using SLambdaExpBodyPtr = std::unique_ptr<SLambdaExpBody>;
using SEmbeddableStmtPtr = std::unique_ptr<SEmbeddableStmt>;
using SForStmtInitializerPtr = std::unique_ptr<SForStmtInitializer>;
using SClassMemberDeclPtr = std::unique_ptr<SClassMemberDecl>;
using SStructMemberDeclPtr = std::unique_ptr<SStructMemberDecl>;
using SNamespaceDeclElementPtr = std::unique_ptr<SNamespaceDeclElement>;
using SScriptElementPtr = std::unique_ptr<SScriptElement>;
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
    bool bOut;
    bool bParams;
    SExpPtr exp;

public:
    SYNTAX_API SArgument(bool bOut, bool bParams, SExpPtr exp);
    SYNTAX_API SArgument(SExpPtr exp);
    SArgument(const SArgument&) = delete;
    SYNTAX_API SArgument(SArgument&&) noexcept;
    SYNTAX_API ~SArgument();

    SArgument& operator=(const SArgument& other) = delete;
    SYNTAX_API SArgument& operator=(SArgument&& other) noexcept;

    bool& HasOut() { return bOut; }
    bool& GetParams() { return bParams; }
    SExpPtr& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
};

class SLambdaExpParam
{
    STypeExpPtr type;
    std::string name;
    bool hasOut;
    bool hasParams;

public:
    SYNTAX_API SLambdaExpParam(STypeExpPtr type, std::string name, bool hasOut, bool hasParams);
    SLambdaExpParam(const SLambdaExpParam&) = delete;
    SYNTAX_API SLambdaExpParam(SLambdaExpParam&&) noexcept;
    SYNTAX_API ~SLambdaExpParam();

    SLambdaExpParam& operator=(const SLambdaExpParam& other) = delete;
    SYNTAX_API SLambdaExpParam& operator=(SLambdaExpParam&& other) noexcept;

    STypeExpPtr& GetType() { return type; }
    std::string& GetName() { return name; }
    bool& HasOut() { return hasOut; }
    bool& HasParams() { return hasParams; }

    SYNTAX_API JsonItem ToJson();
};

class SVarDeclElement
{
    std::string varName;
    SExpPtr initExp;

public:
    SYNTAX_API SVarDeclElement(std::string varName, SExpPtr initExp);
    SVarDeclElement(const SVarDeclElement&) = delete;
    SYNTAX_API SVarDeclElement(SVarDeclElement&&) noexcept;
    SYNTAX_API ~SVarDeclElement();

    SVarDeclElement& operator=(const SVarDeclElement& other) = delete;
    SYNTAX_API SVarDeclElement& operator=(SVarDeclElement&& other) noexcept;

    std::string& GetVarName() { return varName; }
    SExpPtr& GetInitExp() { return initExp; }

    SYNTAX_API JsonItem ToJson();
};

class SVarDecl
{
    STypeExpPtr type;
    std::vector<SVarDeclElement> elements;

public:
    SYNTAX_API SVarDecl(STypeExpPtr type, std::vector<SVarDeclElement> elements);
    SVarDecl(const SVarDecl&) = delete;
    SYNTAX_API SVarDecl(SVarDecl&&) noexcept;
    SYNTAX_API ~SVarDecl();

    SVarDecl& operator=(const SVarDecl& other) = delete;
    SYNTAX_API SVarDecl& operator=(SVarDecl&& other) noexcept;

    STypeExpPtr& GetType() { return type; }
    std::vector<SVarDeclElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

class STypeParam
{
    std::string name;

public:
    SYNTAX_API STypeParam(std::string name);
    STypeParam(const STypeParam&) = delete;
    SYNTAX_API STypeParam(STypeParam&&) noexcept;
    SYNTAX_API ~STypeParam();

    STypeParam& operator=(const STypeParam& other) = delete;
    SYNTAX_API STypeParam& operator=(STypeParam&& other) noexcept;

    std::string& GetName() { return name; }

    SYNTAX_API JsonItem ToJson();
};

class SFuncParam
{
    bool hasOut;
    bool hasParams;
    STypeExpPtr type;
    std::string name;

public:
    SYNTAX_API SFuncParam(bool hasOut, bool hasParams, STypeExpPtr type, std::string name);
    SFuncParam(const SFuncParam&) = delete;
    SYNTAX_API SFuncParam(SFuncParam&&) noexcept;
    SYNTAX_API ~SFuncParam();

    SFuncParam& operator=(const SFuncParam& other) = delete;
    SYNTAX_API SFuncParam& operator=(SFuncParam&& other) noexcept;

    bool& HasOut() { return hasOut; }
    bool& HasParams() { return hasParams; }
    STypeExpPtr& GetType() { return type; }
    std::string& GetName() { return name; }

    SYNTAX_API JsonItem ToJson();
};

class SStmtVisitor
{
public:
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
    virtual void Accept(SStmtVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStmtPtr& stmt);

class SExpVisitor
{
public:
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
    virtual void Accept(SExpVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SExpPtr& exp);

class STypeExpVisitor
{
public:
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
    virtual void Accept(STypeExpVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(STypeExpPtr& typeExp);

class SStringExpElementVisitor
{
public:
    virtual void Visit(STextStringExpElement& elem) = 0;
    virtual void Visit(SExpStringExpElement& elem) = 0;
};

class SStringExpElement
{
public:
    virtual void Accept(SStringExpElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStringExpElementPtr& elem);

class SLambdaExpBodyVisitor
{
public:
    virtual void Visit(SStmtsLambdaExpBody& body) = 0;
    virtual void Visit(SExpLambdaExpBody& body) = 0;
};

class SLambdaExpBody
{
public:
    virtual void Accept(SLambdaExpBodyVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SLambdaExpBodyPtr& body);

class SEmbeddableStmtVisitor
{
public:
    virtual void Visit(SSingleEmbeddableStmt& stmt) = 0;
    virtual void Visit(SBlockEmbeddableStmt& stmt) = 0;
};

class SEmbeddableStmt
{
public:
    virtual void Accept(SEmbeddableStmtVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SEmbeddableStmtPtr& stmt);

class SForStmtInitializerVisitor
{
public:
    virtual void Visit(SExpForStmtInitializer& initializer) = 0;
    virtual void Visit(SVarDeclForStmtInitializer& initializer) = 0;
};

class SForStmtInitializer
{
public:
    virtual void Accept(SForStmtInitializerVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SForStmtInitializerPtr& initializer);

class SClassMemberDeclVisitor
{
public:
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
    virtual void Accept(SClassMemberDeclVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SClassMemberDeclPtr& decl);

class SStructMemberDeclVisitor
{
public:
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
    virtual void Accept(SStructMemberDeclVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SStructMemberDeclPtr& decl);

class SNamespaceDeclElementVisitor
{
public:
    virtual void Visit(SGlobalFuncDecl& elem) = 0;
    virtual void Visit(SNamespaceDecl& elem) = 0;
    virtual void Visit(SClassDecl& elem) = 0;
    virtual void Visit(SStructDecl& elem) = 0;
    virtual void Visit(SEnumDecl& elem) = 0;
};

class SNamespaceDeclElement
{
public:
    virtual void Accept(SNamespaceDeclElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SNamespaceDeclElementPtr& elem);

class SScriptElementVisitor
{
public:
    virtual void Visit(SNamespaceDecl& elem) = 0;
    virtual void Visit(SGlobalFuncDecl& elem) = 0;
    virtual void Visit(SClassDecl& elem) = 0;
    virtual void Visit(SStructDecl& elem) = 0;
    virtual void Visit(SEnumDecl& elem) = 0;
};

class SScriptElement
{
public:
    virtual void Accept(SScriptElementVisitor& visitor) = 0;
};

SYNTAX_API JsonItem ToJson(SScriptElementPtr& elem);

class SIdentifierExp
    : public SExp
{
    std::string value;
    std::vector<STypeExpPtr> typeArgs;

public:
    SYNTAX_API SIdentifierExp(std::string value, std::vector<STypeExpPtr> typeArgs);
    SIdentifierExp(std::string value) : SIdentifierExp(std::move(value), {}) { }
    SIdentifierExp(const SIdentifierExp&) = delete;
    SYNTAX_API SIdentifierExp(SIdentifierExp&&) noexcept;
    SYNTAX_API ~SIdentifierExp();

    SIdentifierExp& operator=(const SIdentifierExp& other) = delete;
    SYNTAX_API SIdentifierExp& operator=(SIdentifierExp&& other) noexcept;

    std::string& GetValue() { return value; }
    std::vector<STypeExpPtr>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SStringExp
    : public SExp
{
    std::vector<SStringExpElementPtr> elements;

public:
    SYNTAX_API SStringExp(std::vector<SStringExpElementPtr> elements);
    SYNTAX_API SStringExp(std::string str);
    SStringExp(const SStringExp&) = delete;
    SYNTAX_API SStringExp(SStringExp&&) noexcept;
    SYNTAX_API ~SStringExp();

    SStringExp& operator=(const SStringExp& other) = delete;
    SYNTAX_API SStringExp& operator=(SStringExp&& other) noexcept;

    std::vector<SStringExpElementPtr>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIntLiteralExp
    : public SExp
{
    int value;

public:
    SYNTAX_API SIntLiteralExp(int value);
    SIntLiteralExp(const SIntLiteralExp&) = delete;
    SYNTAX_API SIntLiteralExp(SIntLiteralExp&&) noexcept;
    SYNTAX_API ~SIntLiteralExp();

    SIntLiteralExp& operator=(const SIntLiteralExp& other) = delete;
    SYNTAX_API SIntLiteralExp& operator=(SIntLiteralExp&& other) noexcept;

    int& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBoolLiteralExp
    : public SExp
{
    bool value;

public:
    SYNTAX_API SBoolLiteralExp(bool value);
    SBoolLiteralExp(const SBoolLiteralExp&) = delete;
    SYNTAX_API SBoolLiteralExp(SBoolLiteralExp&&) noexcept;
    SYNTAX_API ~SBoolLiteralExp();

    SBoolLiteralExp& operator=(const SBoolLiteralExp& other) = delete;
    SYNTAX_API SBoolLiteralExp& operator=(SBoolLiteralExp&& other) noexcept;

    bool& GetValue() { return value; }

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
    SYNTAX_API ~SNullLiteralExp();

    SNullLiteralExp& operator=(const SNullLiteralExp& other) = delete;
    SYNTAX_API SNullLiteralExp& operator=(SNullLiteralExp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SListExp
    : public SExp
{
    std::vector<SExpPtr> elements;

public:
    SYNTAX_API SListExp(std::vector<SExpPtr> elements);
    SListExp(const SListExp&) = delete;
    SYNTAX_API SListExp(SListExp&&) noexcept;
    SYNTAX_API ~SListExp();

    SListExp& operator=(const SListExp& other) = delete;
    SYNTAX_API SListExp& operator=(SListExp&& other) noexcept;

    std::vector<SExpPtr>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SNewExp
    : public SExp
{
    STypeExpPtr type;
    std::vector<SArgument> args;

public:
    SYNTAX_API SNewExp(STypeExpPtr type, std::vector<SArgument> args);
    SNewExp(const SNewExp&) = delete;
    SYNTAX_API SNewExp(SNewExp&&) noexcept;
    SYNTAX_API ~SNewExp();

    SNewExp& operator=(const SNewExp& other) = delete;
    SYNTAX_API SNewExp& operator=(SNewExp&& other) noexcept;

    STypeExpPtr& GetType() { return type; }
    std::vector<SArgument>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBinaryOpExp
    : public SExp
{
    SBinaryOpKind kind;
    SExpPtr operand0;
    SExpPtr operand1;

public:
    SYNTAX_API SBinaryOpExp(SBinaryOpKind kind, SExpPtr operand0, SExpPtr operand1);
    SBinaryOpExp(const SBinaryOpExp&) = delete;
    SYNTAX_API SBinaryOpExp(SBinaryOpExp&&) noexcept;
    SYNTAX_API ~SBinaryOpExp();

    SBinaryOpExp& operator=(const SBinaryOpExp& other) = delete;
    SYNTAX_API SBinaryOpExp& operator=(SBinaryOpExp&& other) noexcept;

    SBinaryOpKind& GetKind() { return kind; }
    SExpPtr& GetOperand0() { return operand0; }
    SExpPtr& GetOperand1() { return operand1; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SUnaryOpExp
    : public SExp
{
    SUnaryOpKind kind;
    SExpPtr operand;

public:
    SYNTAX_API SUnaryOpExp(SUnaryOpKind kind, SExpPtr operand);
    SUnaryOpExp(const SUnaryOpExp&) = delete;
    SYNTAX_API SUnaryOpExp(SUnaryOpExp&&) noexcept;
    SYNTAX_API ~SUnaryOpExp();

    SUnaryOpExp& operator=(const SUnaryOpExp& other) = delete;
    SYNTAX_API SUnaryOpExp& operator=(SUnaryOpExp&& other) noexcept;

    SUnaryOpKind& GetKind() { return kind; }
    SExpPtr& GetOperand() { return operand; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SCallExp
    : public SExp
{
    SExpPtr callable;
    std::vector<SArgument> args;

public:
    SYNTAX_API SCallExp(SExpPtr callable, std::vector<SArgument> args);
    SCallExp(const SCallExp&) = delete;
    SYNTAX_API SCallExp(SCallExp&&) noexcept;
    SYNTAX_API ~SCallExp();

    SCallExp& operator=(const SCallExp& other) = delete;
    SYNTAX_API SCallExp& operator=(SCallExp&& other) noexcept;

    SExpPtr& GetCallable() { return callable; }
    std::vector<SArgument>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SLambdaExp
    : public SExp
{
    std::vector<SLambdaExpParam> params;
    SLambdaExpBodyPtr body;

public:
    SYNTAX_API SLambdaExp(std::vector<SLambdaExpParam> params, SLambdaExpBodyPtr body);
    SLambdaExp(const SLambdaExp&) = delete;
    SYNTAX_API SLambdaExp(SLambdaExp&&) noexcept;
    SYNTAX_API ~SLambdaExp();

    SLambdaExp& operator=(const SLambdaExp& other) = delete;
    SYNTAX_API SLambdaExp& operator=(SLambdaExp&& other) noexcept;

    std::vector<SLambdaExpParam>& GetParams() { return params; }
    SLambdaExpBodyPtr& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIndexerExp
    : public SExp
{
    SExpPtr obj;
    SExpPtr index;

public:
    SYNTAX_API SIndexerExp(SExpPtr obj, SExpPtr index);
    SIndexerExp(const SIndexerExp&) = delete;
    SYNTAX_API SIndexerExp(SIndexerExp&&) noexcept;
    SYNTAX_API ~SIndexerExp();

    SIndexerExp& operator=(const SIndexerExp& other) = delete;
    SYNTAX_API SIndexerExp& operator=(SIndexerExp&& other) noexcept;

    SExpPtr& GetObject() { return obj; }
    SExpPtr& GetIndex() { return index; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SMemberExp
    : public SExp
{
    SExpPtr parent;
    std::string memberName;
    std::vector<STypeExpPtr> memberTypeArgs;

public:
    SYNTAX_API SMemberExp(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs);
    SYNTAX_API SMemberExp(SExpPtr parent, std::string memberName);
    SMemberExp(const SMemberExp&) = delete;
    SYNTAX_API SMemberExp(SMemberExp&&) noexcept;
    SYNTAX_API ~SMemberExp();

    SMemberExp& operator=(const SMemberExp& other) = delete;
    SYNTAX_API SMemberExp& operator=(SMemberExp&& other) noexcept;

    SExpPtr& GetParent() { return parent; }
    std::string& GetMemberName() { return memberName; }
    std::vector<STypeExpPtr>& GetMemberTypeArgs() { return memberTypeArgs; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIndirectMemberExp
    : public SExp
{
    SExpPtr parent;
    std::string memberName;
    std::vector<STypeExpPtr> memberTypeArgs;

public:
    SYNTAX_API SIndirectMemberExp(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs);
    SYNTAX_API SIndirectMemberExp(SExpPtr parent, std::string memberName);
    SIndirectMemberExp(const SIndirectMemberExp&) = delete;
    SYNTAX_API SIndirectMemberExp(SIndirectMemberExp&&) noexcept;
    SYNTAX_API ~SIndirectMemberExp();

    SIndirectMemberExp& operator=(const SIndirectMemberExp& other) = delete;
    SYNTAX_API SIndirectMemberExp& operator=(SIndirectMemberExp&& other) noexcept;

    SExpPtr& GetParent() { return parent; }
    std::string& GetMemberName() { return memberName; }
    std::vector<STypeExpPtr>& GetMemberTypeArgs() { return memberTypeArgs; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBoxExp
    : public SExp
{
    SExpPtr innerExp;

public:
    SYNTAX_API SBoxExp(SExpPtr innerExp);
    SBoxExp(const SBoxExp&) = delete;
    SYNTAX_API SBoxExp(SBoxExp&&) noexcept;
    SYNTAX_API ~SBoxExp();

    SBoxExp& operator=(const SBoxExp& other) = delete;
    SYNTAX_API SBoxExp& operator=(SBoxExp&& other) noexcept;

    SExpPtr& GetInnerExp() { return innerExp; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIsExp
    : public SExp
{
    SExpPtr exp;
    STypeExpPtr type;

public:
    SYNTAX_API SIsExp(SExpPtr exp, STypeExpPtr type);
    SIsExp(const SIsExp&) = delete;
    SYNTAX_API SIsExp(SIsExp&&) noexcept;
    SYNTAX_API ~SIsExp();

    SIsExp& operator=(const SIsExp& other) = delete;
    SYNTAX_API SIsExp& operator=(SIsExp&& other) noexcept;

    SExpPtr& GetExp() { return exp; }
    STypeExpPtr& GetType() { return type; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SAsExp
    : public SExp
{
    SExpPtr exp;
    STypeExpPtr type;

public:
    SYNTAX_API SAsExp(SExpPtr exp, STypeExpPtr type);
    SAsExp(const SAsExp&) = delete;
    SYNTAX_API SAsExp(SAsExp&&) noexcept;
    SYNTAX_API ~SAsExp();

    SAsExp& operator=(const SAsExp& other) = delete;
    SYNTAX_API SAsExp& operator=(SAsExp&& other) noexcept;

    SExpPtr& GetExp() { return exp; }
    STypeExpPtr& GetType() { return type; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SIdTypeExp
    : public STypeExp
{
    std::string name;
    std::vector<STypeExpPtr> typeArgs;

public:
    SYNTAX_API SIdTypeExp(std::string name, std::vector<STypeExpPtr> typeArgs);
    SYNTAX_API SIdTypeExp(std::string name);
    SIdTypeExp(const SIdTypeExp&) = delete;
    SYNTAX_API SIdTypeExp(SIdTypeExp&&) noexcept;
    SYNTAX_API ~SIdTypeExp();

    SIdTypeExp& operator=(const SIdTypeExp& other) = delete;
    SYNTAX_API SIdTypeExp& operator=(SIdTypeExp&& other) noexcept;

    std::string& GetName() { return name; }
    std::vector<STypeExpPtr>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SMemberTypeExp
    : public STypeExp
{
    STypeExpPtr parentType;
    std::string name;
    std::vector<STypeExpPtr> typeArgs;

public:
    SYNTAX_API SMemberTypeExp(STypeExpPtr parentType, std::string name, std::vector<STypeExpPtr> typeArgs);
    SMemberTypeExp(const SMemberTypeExp&) = delete;
    SYNTAX_API SMemberTypeExp(SMemberTypeExp&&) noexcept;
    SYNTAX_API ~SMemberTypeExp();

    SMemberTypeExp& operator=(const SMemberTypeExp& other) = delete;
    SYNTAX_API SMemberTypeExp& operator=(SMemberTypeExp&& other) noexcept;

    STypeExpPtr& GetParentType() { return parentType; }
    std::string& GetName() { return name; }
    std::vector<STypeExpPtr>& GetTypeArgs() { return typeArgs; }

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SNullableTypeExp
    : public STypeExp
{
    STypeExpPtr innerType;

public:
    SYNTAX_API SNullableTypeExp(STypeExpPtr innerType);
    SNullableTypeExp(const SNullableTypeExp&) = delete;
    SYNTAX_API SNullableTypeExp(SNullableTypeExp&&) noexcept;
    SYNTAX_API ~SNullableTypeExp();

    SNullableTypeExp& operator=(const SNullableTypeExp& other) = delete;
    SYNTAX_API SNullableTypeExp& operator=(SNullableTypeExp&& other) noexcept;

    STypeExpPtr& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SLocalPtrTypeExp
    : public STypeExp
{
    STypeExpPtr innerType;

public:
    SYNTAX_API SLocalPtrTypeExp(STypeExpPtr innerType);
    SLocalPtrTypeExp(const SLocalPtrTypeExp&) = delete;
    SYNTAX_API SLocalPtrTypeExp(SLocalPtrTypeExp&&) noexcept;
    SYNTAX_API ~SLocalPtrTypeExp();

    SLocalPtrTypeExp& operator=(const SLocalPtrTypeExp& other) = delete;
    SYNTAX_API SLocalPtrTypeExp& operator=(SLocalPtrTypeExp&& other) noexcept;

    STypeExpPtr& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SBoxPtrTypeExp
    : public STypeExp
{
    STypeExpPtr innerType;

public:
    SYNTAX_API SBoxPtrTypeExp(STypeExpPtr innerType);
    SBoxPtrTypeExp(const SBoxPtrTypeExp&) = delete;
    SYNTAX_API SBoxPtrTypeExp(SBoxPtrTypeExp&&) noexcept;
    SYNTAX_API ~SBoxPtrTypeExp();

    SBoxPtrTypeExp& operator=(const SBoxPtrTypeExp& other) = delete;
    SYNTAX_API SBoxPtrTypeExp& operator=(SBoxPtrTypeExp&& other) noexcept;

    STypeExpPtr& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class SLocalTypeExp
    : public STypeExp
{
    STypeExpPtr innerType;

public:
    SYNTAX_API SLocalTypeExp(STypeExpPtr innerType);
    SLocalTypeExp(const SLocalTypeExp&) = delete;
    SYNTAX_API SLocalTypeExp(SLocalTypeExp&&) noexcept;
    SYNTAX_API ~SLocalTypeExp();

    SLocalTypeExp& operator=(const SLocalTypeExp& other) = delete;
    SYNTAX_API SLocalTypeExp& operator=(SLocalTypeExp&& other) noexcept;

    STypeExpPtr& GetInnerType() { return innerType; }

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(*this); }

};

class STextStringExpElement
    : public SStringExpElement
{
    std::string text;

public:
    SYNTAX_API STextStringExpElement(std::string text);
    STextStringExpElement(const STextStringExpElement&) = delete;
    SYNTAX_API STextStringExpElement(STextStringExpElement&&) noexcept;
    SYNTAX_API ~STextStringExpElement();

    STextStringExpElement& operator=(const STextStringExpElement& other) = delete;
    SYNTAX_API STextStringExpElement& operator=(STextStringExpElement&& other) noexcept;

    std::string& GetText() { return text; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpStringExpElement
    : public SStringExpElement
{
    SExpPtr exp;

public:
    SYNTAX_API SExpStringExpElement(SExpPtr exp);
    SExpStringExpElement(const SExpStringExpElement&) = delete;
    SYNTAX_API SExpStringExpElement(SExpStringExpElement&&) noexcept;
    SYNTAX_API ~SExpStringExpElement();

    SExpStringExpElement& operator=(const SExpStringExpElement& other) = delete;
    SYNTAX_API SExpStringExpElement& operator=(SExpStringExpElement&& other) noexcept;

    SExpPtr& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SStmtsLambdaExpBody
    : public SLambdaExpBody
{
    std::vector<SStmtPtr> stmts;

public:
    SYNTAX_API SStmtsLambdaExpBody(std::vector<SStmtPtr> stmts);
    SStmtsLambdaExpBody(const SStmtsLambdaExpBody&) = delete;
    SYNTAX_API SStmtsLambdaExpBody(SStmtsLambdaExpBody&&) noexcept;
    SYNTAX_API ~SStmtsLambdaExpBody();

    SStmtsLambdaExpBody& operator=(const SStmtsLambdaExpBody& other) = delete;
    SYNTAX_API SStmtsLambdaExpBody& operator=(SStmtsLambdaExpBody&& other) noexcept;

    std::vector<SStmtPtr>& GetStmts() { return stmts; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpLambdaExpBody
    : public SLambdaExpBody
{
    SExpPtr exp;

public:
    SYNTAX_API SExpLambdaExpBody(SExpPtr exp);
    SExpLambdaExpBody(const SExpLambdaExpBody&) = delete;
    SYNTAX_API SExpLambdaExpBody(SExpLambdaExpBody&&) noexcept;
    SYNTAX_API ~SExpLambdaExpBody();

    SExpLambdaExpBody& operator=(const SExpLambdaExpBody& other) = delete;
    SYNTAX_API SExpLambdaExpBody& operator=(SExpLambdaExpBody&& other) noexcept;

    SExpPtr& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(*this); }

};

class SSingleEmbeddableStmt
    : public SEmbeddableStmt
{
    SStmtPtr stmt;

public:
    SYNTAX_API SSingleEmbeddableStmt(SStmtPtr stmt);
    SSingleEmbeddableStmt(const SSingleEmbeddableStmt&) = delete;
    SYNTAX_API SSingleEmbeddableStmt(SSingleEmbeddableStmt&&) noexcept;
    SYNTAX_API ~SSingleEmbeddableStmt();

    SSingleEmbeddableStmt& operator=(const SSingleEmbeddableStmt& other) = delete;
    SYNTAX_API SSingleEmbeddableStmt& operator=(SSingleEmbeddableStmt&& other) noexcept;

    SStmtPtr& GetStmt() { return stmt; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SBlockEmbeddableStmt
    : public SEmbeddableStmt
{
    std::vector<SStmtPtr> stmts;

public:
    SYNTAX_API SBlockEmbeddableStmt(std::vector<SStmtPtr> stmts);
    SBlockEmbeddableStmt(const SBlockEmbeddableStmt&) = delete;
    SYNTAX_API SBlockEmbeddableStmt(SBlockEmbeddableStmt&&) noexcept;
    SYNTAX_API ~SBlockEmbeddableStmt();

    SBlockEmbeddableStmt& operator=(const SBlockEmbeddableStmt& other) = delete;
    SYNTAX_API SBlockEmbeddableStmt& operator=(SBlockEmbeddableStmt&& other) noexcept;

    std::vector<SStmtPtr>& GetStmts() { return stmts; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpForStmtInitializer
    : public SForStmtInitializer
{
    SExpPtr exp;

public:
    SYNTAX_API SExpForStmtInitializer(SExpPtr exp);
    SExpForStmtInitializer(const SExpForStmtInitializer&) = delete;
    SYNTAX_API SExpForStmtInitializer(SExpForStmtInitializer&&) noexcept;
    SYNTAX_API ~SExpForStmtInitializer();

    SExpForStmtInitializer& operator=(const SExpForStmtInitializer& other) = delete;
    SYNTAX_API SExpForStmtInitializer& operator=(SExpForStmtInitializer&& other) noexcept;

    SExpPtr& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(*this); }

};

class SVarDeclForStmtInitializer
    : public SForStmtInitializer
{
    SVarDecl varDecl;

public:
    SYNTAX_API SVarDeclForStmtInitializer(SVarDecl varDecl);
    SVarDeclForStmtInitializer(const SVarDeclForStmtInitializer&) = delete;
    SYNTAX_API SVarDeclForStmtInitializer(SVarDeclForStmtInitializer&&) noexcept;
    SYNTAX_API ~SVarDeclForStmtInitializer();

    SVarDeclForStmtInitializer& operator=(const SVarDeclForStmtInitializer& other) = delete;
    SYNTAX_API SVarDeclForStmtInitializer& operator=(SVarDeclForStmtInitializer&& other) noexcept;

    SVarDecl& GetVarDecl() { return varDecl; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(*this); }

};

class SCommandStmt
    : public SStmt
{
    std::vector<SStringExp> commands;

public:
    SYNTAX_API SCommandStmt(std::vector<SStringExp> commands);
    SCommandStmt(const SCommandStmt&) = delete;
    SYNTAX_API SCommandStmt(SCommandStmt&&) noexcept;
    SYNTAX_API ~SCommandStmt();

    SCommandStmt& operator=(const SCommandStmt& other) = delete;
    SYNTAX_API SCommandStmt& operator=(SCommandStmt&& other) noexcept;

    std::vector<SStringExp>& GetCommands() { return commands; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SVarDeclStmt
    : public SStmt
{
    SVarDecl varDecl;

public:
    SYNTAX_API SVarDeclStmt(SVarDecl varDecl);
    SVarDeclStmt(const SVarDeclStmt&) = delete;
    SYNTAX_API SVarDeclStmt(SVarDeclStmt&&) noexcept;
    SYNTAX_API ~SVarDeclStmt();

    SVarDeclStmt& operator=(const SVarDeclStmt& other) = delete;
    SYNTAX_API SVarDeclStmt& operator=(SVarDeclStmt&& other) noexcept;

    SVarDecl& GetVarDecl() { return varDecl; }

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
    SYNTAX_API ~SContinueStmt();

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
    SYNTAX_API ~SBreakStmt();

    SBreakStmt& operator=(const SBreakStmt& other) = delete;
    SYNTAX_API SBreakStmt& operator=(SBreakStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SBlockStmt
    : public SStmt
{
    std::vector<SStmtPtr> stmts;

public:
    SYNTAX_API SBlockStmt(std::vector<SStmtPtr> stmts);
    SBlockStmt(const SBlockStmt&) = delete;
    SYNTAX_API SBlockStmt(SBlockStmt&&) noexcept;
    SYNTAX_API ~SBlockStmt();

    SBlockStmt& operator=(const SBlockStmt& other) = delete;
    SYNTAX_API SBlockStmt& operator=(SBlockStmt&& other) noexcept;

    std::vector<SStmtPtr>& GetStmts() { return stmts; }

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
    SYNTAX_API ~SBlankStmt();

    SBlankStmt& operator=(const SBlankStmt& other) = delete;
    SYNTAX_API SBlankStmt& operator=(SBlankStmt&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class STaskStmt
    : public SStmt
{
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API STaskStmt(std::vector<SStmtPtr> body);
    STaskStmt(const STaskStmt&) = delete;
    SYNTAX_API STaskStmt(STaskStmt&&) noexcept;
    SYNTAX_API ~STaskStmt();

    STaskStmt& operator=(const STaskStmt& other) = delete;
    SYNTAX_API STaskStmt& operator=(STaskStmt&& other) noexcept;

    std::vector<SStmtPtr>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SAwaitStmt
    : public SStmt
{
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API SAwaitStmt(std::vector<SStmtPtr> body);
    SAwaitStmt(const SAwaitStmt&) = delete;
    SYNTAX_API SAwaitStmt(SAwaitStmt&&) noexcept;
    SYNTAX_API ~SAwaitStmt();

    SAwaitStmt& operator=(const SAwaitStmt& other) = delete;
    SYNTAX_API SAwaitStmt& operator=(SAwaitStmt&& other) noexcept;

    std::vector<SStmtPtr>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SAsyncStmt
    : public SStmt
{
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API SAsyncStmt(std::vector<SStmtPtr> body);
    SAsyncStmt(const SAsyncStmt&) = delete;
    SYNTAX_API SAsyncStmt(SAsyncStmt&&) noexcept;
    SYNTAX_API ~SAsyncStmt();

    SAsyncStmt& operator=(const SAsyncStmt& other) = delete;
    SYNTAX_API SAsyncStmt& operator=(SAsyncStmt&& other) noexcept;

    std::vector<SStmtPtr>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SDirectiveStmt
    : public SStmt
{
    std::string name;
    std::vector<SExpPtr> args;

public:
    SYNTAX_API SDirectiveStmt(std::string name, std::vector<SExpPtr> args);
    SDirectiveStmt(const SDirectiveStmt&) = delete;
    SYNTAX_API SDirectiveStmt(SDirectiveStmt&&) noexcept;
    SYNTAX_API ~SDirectiveStmt();

    SDirectiveStmt& operator=(const SDirectiveStmt& other) = delete;
    SYNTAX_API SDirectiveStmt& operator=(SDirectiveStmt&& other) noexcept;

    std::string& GetName() { return name; }
    std::vector<SExpPtr>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SIfStmt
    : public SStmt
{
    SExpPtr cond;
    SEmbeddableStmtPtr body;
    SEmbeddableStmtPtr elseBody;

public:
    SYNTAX_API SIfStmt(SExpPtr cond, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
    SIfStmt(const SIfStmt&) = delete;
    SYNTAX_API SIfStmt(SIfStmt&&) noexcept;
    SYNTAX_API ~SIfStmt();

    SIfStmt& operator=(const SIfStmt& other) = delete;
    SYNTAX_API SIfStmt& operator=(SIfStmt&& other) noexcept;

    SExpPtr& GetCond() { return cond; }
    SEmbeddableStmtPtr& GetBody() { return body; }
    SEmbeddableStmtPtr& GetElseBody() { return elseBody; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SIfTestStmt
    : public SStmt
{
    STypeExpPtr testType;
    std::string varName;
    SExpPtr exp;
    SEmbeddableStmtPtr body;
    SEmbeddableStmtPtr elseBody;

public:
    SYNTAX_API SIfTestStmt(STypeExpPtr testType, std::string varName, SExpPtr exp, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
    SIfTestStmt(const SIfTestStmt&) = delete;
    SYNTAX_API SIfTestStmt(SIfTestStmt&&) noexcept;
    SYNTAX_API ~SIfTestStmt();

    SIfTestStmt& operator=(const SIfTestStmt& other) = delete;
    SYNTAX_API SIfTestStmt& operator=(SIfTestStmt&& other) noexcept;

    STypeExpPtr& GetTestType() { return testType; }
    std::string& GetVarName() { return varName; }
    SExpPtr& GetExp() { return exp; }
    SEmbeddableStmtPtr& GetBody() { return body; }
    SEmbeddableStmtPtr& GetElseBody() { return elseBody; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SForStmt
    : public SStmt
{
    SForStmtInitializerPtr initializer;
    SExpPtr cond;
    SExpPtr cont;
    SEmbeddableStmtPtr body;

public:
    SYNTAX_API SForStmt(SForStmtInitializerPtr initializer, SExpPtr cond, SExpPtr cont, SEmbeddableStmtPtr body);
    SForStmt(const SForStmt&) = delete;
    SYNTAX_API SForStmt(SForStmt&&) noexcept;
    SYNTAX_API ~SForStmt();

    SForStmt& operator=(const SForStmt& other) = delete;
    SYNTAX_API SForStmt& operator=(SForStmt&& other) noexcept;

    SForStmtInitializerPtr& GetInitializer() { return initializer; }
    SExpPtr& GetCond() { return cond; }
    SExpPtr& GetCont() { return cont; }
    SEmbeddableStmtPtr& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SReturnStmt
    : public SStmt
{
    SExpPtr value;

public:
    SYNTAX_API SReturnStmt(SExpPtr value);
    SReturnStmt(const SReturnStmt&) = delete;
    SYNTAX_API SReturnStmt(SReturnStmt&&) noexcept;
    SYNTAX_API ~SReturnStmt();

    SReturnStmt& operator=(const SReturnStmt& other) = delete;
    SYNTAX_API SReturnStmt& operator=(SReturnStmt&& other) noexcept;

    SExpPtr& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SExpStmt
    : public SStmt
{
    SExpPtr exp;

public:
    SYNTAX_API SExpStmt(SExpPtr exp);
    SExpStmt(const SExpStmt&) = delete;
    SYNTAX_API SExpStmt(SExpStmt&&) noexcept;
    SYNTAX_API ~SExpStmt();

    SExpStmt& operator=(const SExpStmt& other) = delete;
    SYNTAX_API SExpStmt& operator=(SExpStmt&& other) noexcept;

    SExpPtr& GetExp() { return exp; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SForeachStmt
    : public SStmt
{
    STypeExpPtr type;
    std::string varName;
    SExpPtr enumerable;
    SEmbeddableStmtPtr body;

public:
    SYNTAX_API SForeachStmt(STypeExpPtr type, std::string varName, SExpPtr enumerable, SEmbeddableStmtPtr body);
    SForeachStmt(const SForeachStmt&) = delete;
    SYNTAX_API SForeachStmt(SForeachStmt&&) noexcept;
    SYNTAX_API ~SForeachStmt();

    SForeachStmt& operator=(const SForeachStmt& other) = delete;
    SYNTAX_API SForeachStmt& operator=(SForeachStmt&& other) noexcept;

    STypeExpPtr& GetType() { return type; }
    std::string& GetVarName() { return varName; }
    SExpPtr& GetEnumerable() { return enumerable; }
    SEmbeddableStmtPtr& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SYieldStmt
    : public SStmt
{
    SExpPtr value;

public:
    SYNTAX_API SYieldStmt(SExpPtr value);
    SYieldStmt(const SYieldStmt&) = delete;
    SYNTAX_API SYieldStmt(SYieldStmt&&) noexcept;
    SYNTAX_API ~SYieldStmt();

    SYieldStmt& operator=(const SYieldStmt& other) = delete;
    SYNTAX_API SYieldStmt& operator=(SYieldStmt&& other) noexcept;

    SExpPtr& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }

};

class SGlobalFuncDecl
    : public SNamespaceDeclElement
    , public SScriptElement
{
    std::optional<SAccessModifier> accessModifier;
    bool bSequence;
    STypeExpPtr retType;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API SGlobalFuncDecl(std::optional<SAccessModifier> accessModifier, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SGlobalFuncDecl(const SGlobalFuncDecl&) = delete;
    SYNTAX_API SGlobalFuncDecl(SGlobalFuncDecl&&) noexcept;
    SYNTAX_API ~SGlobalFuncDecl();

    SGlobalFuncDecl& operator=(const SGlobalFuncDecl& other) = delete;
    SYNTAX_API SGlobalFuncDecl& operator=(SGlobalFuncDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    bool& IsSequence() { return bSequence; }
    STypeExpPtr& GetRetType() { return retType; }
    std::string& GetName() { return name; }
    std::vector<STypeParam>& GetTypeParams() { return typeParams; }
    std::vector<SFuncParam>& GetParameters() { return parameters; }
    std::vector<SStmtPtr>& GetBody() { return body; }

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
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<STypeExpPtr> baseTypes;
    std::vector<SClassMemberDeclPtr> memberDecls;

public:
    SYNTAX_API SClassDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExpPtr> baseTypes, std::vector<SClassMemberDeclPtr> memberDecls);
    SClassDecl(const SClassDecl&) = delete;
    SYNTAX_API SClassDecl(SClassDecl&&) noexcept;
    SYNTAX_API ~SClassDecl();

    SClassDecl& operator=(const SClassDecl& other) = delete;
    SYNTAX_API SClassDecl& operator=(SClassDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<STypeParam>& GetTypeParams() { return typeParams; }
    std::vector<STypeExpPtr>& GetBaseTypes() { return baseTypes; }
    std::vector<SClassMemberDeclPtr>& GetMemberDecls() { return memberDecls; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SClassMemberFuncDecl
    : public SClassMemberDecl
{
    std::optional<SAccessModifier> accessModifier;
    bool bStatic;
    bool bSequence;
    STypeExpPtr retType;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API SClassMemberFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SClassMemberFuncDecl(const SClassMemberFuncDecl&) = delete;
    SYNTAX_API SClassMemberFuncDecl(SClassMemberFuncDecl&&) noexcept;
    SYNTAX_API ~SClassMemberFuncDecl();

    SClassMemberFuncDecl& operator=(const SClassMemberFuncDecl& other) = delete;
    SYNTAX_API SClassMemberFuncDecl& operator=(SClassMemberFuncDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    bool& IsStatic() { return bStatic; }
    bool& IsSequence() { return bSequence; }
    STypeExpPtr& GetRetType() { return retType; }
    std::string& GetName() { return name; }
    std::vector<STypeParam>& GetTypeParams() { return typeParams; }
    std::vector<SFuncParam>& GetParameters() { return parameters; }
    std::vector<SStmtPtr>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SClassConstructorDecl
    : public SClassMemberDecl
{
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<SFuncParam> parameters;
    std::optional<std::vector<SArgument>> baseArgs;
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API SClassConstructorDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<SFuncParam> parameters, std::optional<std::vector<SArgument>> baseArgs, std::vector<SStmtPtr> body);
    SClassConstructorDecl(const SClassConstructorDecl&) = delete;
    SYNTAX_API SClassConstructorDecl(SClassConstructorDecl&&) noexcept;
    SYNTAX_API ~SClassConstructorDecl();

    SClassConstructorDecl& operator=(const SClassConstructorDecl& other) = delete;
    SYNTAX_API SClassConstructorDecl& operator=(SClassConstructorDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<SFuncParam>& GetParameters() { return parameters; }
    std::optional<std::vector<SArgument>>& GetBaseArgs() { return baseArgs; }
    std::vector<SStmtPtr>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SClassMemberVarDecl
    : public SClassMemberDecl
{
    std::optional<SAccessModifier> accessModifier;
    STypeExpPtr varType;
    std::vector<std::string> varNames;

public:
    SYNTAX_API SClassMemberVarDecl(std::optional<SAccessModifier> accessModifier, STypeExpPtr varType, std::vector<std::string> varNames);
    SClassMemberVarDecl(const SClassMemberVarDecl&) = delete;
    SYNTAX_API SClassMemberVarDecl(SClassMemberVarDecl&&) noexcept;
    SYNTAX_API ~SClassMemberVarDecl();

    SClassMemberVarDecl& operator=(const SClassMemberVarDecl& other) = delete;
    SYNTAX_API SClassMemberVarDecl& operator=(SClassMemberVarDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    STypeExpPtr& GetVarType() { return varType; }
    std::vector<std::string>& GetVarNames() { return varNames; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<STypeExpPtr> baseTypes;
    std::vector<SStructMemberDeclPtr> memberDecls;

public:
    SYNTAX_API SStructDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExpPtr> baseTypes, std::vector<SStructMemberDeclPtr> memberDecls);
    SStructDecl(const SStructDecl&) = delete;
    SYNTAX_API SStructDecl(SStructDecl&&) noexcept;
    SYNTAX_API ~SStructDecl();

    SStructDecl& operator=(const SStructDecl& other) = delete;
    SYNTAX_API SStructDecl& operator=(SStructDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<STypeParam>& GetTypeParams() { return typeParams; }
    std::vector<STypeExpPtr>& GetBaseTypes() { return baseTypes; }
    std::vector<SStructMemberDeclPtr>& GetMemberDecls() { return memberDecls; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructMemberFuncDecl
    : public SStructMemberDecl
{
    std::optional<SAccessModifier> accessModifier;
    bool bStatic;
    bool bSequence;
    STypeExpPtr retType;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API SStructMemberFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SStructMemberFuncDecl(const SStructMemberFuncDecl&) = delete;
    SYNTAX_API SStructMemberFuncDecl(SStructMemberFuncDecl&&) noexcept;
    SYNTAX_API ~SStructMemberFuncDecl();

    SStructMemberFuncDecl& operator=(const SStructMemberFuncDecl& other) = delete;
    SYNTAX_API SStructMemberFuncDecl& operator=(SStructMemberFuncDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAcessModifier() { return accessModifier; }
    bool& IsStatic() { return bStatic; }
    bool& IsSequence() { return bSequence; }
    STypeExpPtr& GetRetType() { return retType; }
    std::string& GetName() { return name; }
    std::vector<STypeParam>& GetTypeParams() { return typeParams; }
    std::vector<SFuncParam>& GetParameters() { return parameters; }
    std::vector<SStmtPtr>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructConstructorDecl
    : public SStructMemberDecl
{
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<SFuncParam> parameters;
    std::vector<SStmtPtr> body;

public:
    SYNTAX_API SStructConstructorDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body);
    SStructConstructorDecl(const SStructConstructorDecl&) = delete;
    SYNTAX_API SStructConstructorDecl(SStructConstructorDecl&&) noexcept;
    SYNTAX_API ~SStructConstructorDecl();

    SStructConstructorDecl& operator=(const SStructConstructorDecl& other) = delete;
    SYNTAX_API SStructConstructorDecl& operator=(SStructConstructorDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<SFuncParam>& GetParameters() { return parameters; }
    std::vector<SStmtPtr>& GetBody() { return body; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SStructMemberVarDecl
    : public SStructMemberDecl
{
    std::optional<SAccessModifier> accessModifier;
    STypeExpPtr varType;
    std::vector<std::string> varNames;

public:
    SYNTAX_API SStructMemberVarDecl(std::optional<SAccessModifier> accessModifier, STypeExpPtr varType, std::vector<std::string> varNames);
    SStructMemberVarDecl(const SStructMemberVarDecl&) = delete;
    SYNTAX_API SStructMemberVarDecl(SStructMemberVarDecl&&) noexcept;
    SYNTAX_API ~SStructMemberVarDecl();

    SStructMemberVarDecl& operator=(const SStructMemberVarDecl& other) = delete;
    SYNTAX_API SStructMemberVarDecl& operator=(SStructMemberVarDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    STypeExpPtr& GetVarType() { return varType; }
    std::vector<std::string>& GetVarNames() { return varNames; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(*this); }

};

class SEnumElemMemberVarDecl
{
    STypeExpPtr type;
    std::string name;

public:
    SYNTAX_API SEnumElemMemberVarDecl(STypeExpPtr type, std::string name);
    SEnumElemMemberVarDecl(const SEnumElemMemberVarDecl&) = delete;
    SYNTAX_API SEnumElemMemberVarDecl(SEnumElemMemberVarDecl&&) noexcept;
    SYNTAX_API ~SEnumElemMemberVarDecl();

    SEnumElemMemberVarDecl& operator=(const SEnumElemMemberVarDecl& other) = delete;
    SYNTAX_API SEnumElemMemberVarDecl& operator=(SEnumElemMemberVarDecl&& other) noexcept;

    STypeExpPtr& GetType() { return type; }
    std::string& GetName() { return name; }

    SYNTAX_API JsonItem ToJson();
};

class SEnumElemDecl
{
    std::string name;
    std::vector<SEnumElemMemberVarDecl> memberVars;

public:
    SYNTAX_API SEnumElemDecl(std::string name, std::vector<SEnumElemMemberVarDecl> memberVars);
    SEnumElemDecl(const SEnumElemDecl&) = delete;
    SYNTAX_API SEnumElemDecl(SEnumElemDecl&&) noexcept;
    SYNTAX_API ~SEnumElemDecl();

    SEnumElemDecl& operator=(const SEnumElemDecl& other) = delete;
    SYNTAX_API SEnumElemDecl& operator=(SEnumElemDecl&& other) noexcept;

    std::string& GetName() { return name; }
    std::vector<SEnumElemMemberVarDecl>& GetMemberVars() { return memberVars; }

    SYNTAX_API JsonItem ToJson();
};

class SEnumDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
    std::optional<SAccessModifier> accessModifier;
    std::string name;
    std::vector<STypeParam> typeParams;
    std::vector<SEnumElemDecl> elements;

public:
    SYNTAX_API SEnumDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<SEnumElemDecl> elements);
    SEnumDecl(const SEnumDecl&) = delete;
    SYNTAX_API SEnumDecl(SEnumDecl&&) noexcept;
    SYNTAX_API ~SEnumDecl();

    SEnumDecl& operator=(const SEnumDecl& other) = delete;
    SYNTAX_API SEnumDecl& operator=(SEnumDecl&& other) noexcept;

    std::optional<SAccessModifier>& GetAccessModifier() { return accessModifier; }
    std::string& GetName() { return name; }
    std::vector<STypeParam>& GetTypeParams() { return typeParams; }
    std::vector<SEnumElemDecl>& GetElements() { return elements; }

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
    std::vector<std::string> names;
    std::vector<SNamespaceDeclElementPtr> elements;

public:
    SYNTAX_API SNamespaceDecl(std::vector<std::string> names, std::vector<SNamespaceDeclElementPtr> elements);
    SNamespaceDecl(const SNamespaceDecl&) = delete;
    SYNTAX_API SNamespaceDecl(SNamespaceDecl&&) noexcept;
    SYNTAX_API ~SNamespaceDecl();

    SNamespaceDecl& operator=(const SNamespaceDecl& other) = delete;
    SYNTAX_API SNamespaceDecl& operator=(SNamespaceDecl&& other) noexcept;

    std::vector<std::string>& GetNames() { return names; }
    std::vector<SNamespaceDeclElementPtr>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(*this); }

};

class SScript
{
    std::vector<SScriptElementPtr> elements;

public:
    SYNTAX_API SScript(std::vector<SScriptElementPtr> elements);
    SScript(const SScript&) = delete;
    SYNTAX_API SScript(SScript&&) noexcept;
    SYNTAX_API ~SScript();

    SScript& operator=(const SScript& other) = delete;
    SYNTAX_API SScript& operator=(SScript&& other) noexcept;

    std::vector<SScriptElementPtr>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};


}
