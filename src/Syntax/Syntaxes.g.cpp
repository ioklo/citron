#include "pch.h"

#include "Syntaxes.g.h"
#include <Infra/Json.h>

using namespace std;

namespace Citron {

namespace {
struct ToJsonVisitor {
    template<typename T>
    JsonItem operator()(std::shared_ptr<T>& t) { return t->ToJson(); }

    template<typename T>
    JsonItem operator()(T& t) { return t.ToJson(); }
};
}

SSyntax::SSyntax()
{ }
SSyntax::SSyntax(SSyntax&& other) noexcept = default;

SSyntax::~SSyntax() = default;

SSyntax& SSyntax::operator=(SSyntax&& other) noexcept = default;

JsonItem SSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SSyntax") },
    };
}

SArgument::SArgument(bool bOut, bool bParams, SExpPtr exp)
    : bOut(std::move(bOut)), bParams(std::move(bParams)), exp(std::move(exp)) { }

SArgument::SArgument(SArgument&& other) noexcept = default;

SArgument::~SArgument() = default;

SArgument& SArgument::operator=(SArgument&& other) noexcept = default;

JsonItem SArgument::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SArgument") },
        { "bOut", Citron::ToJson(bOut) },
        { "bParams", Citron::ToJson(bParams) },
        { "exp", Citron::ToJson(exp) },
    };
}

SLambdaExpParam::SLambdaExpParam(STypeExpPtr type, std::string name, bool hasOut, bool hasParams)
    : type(std::move(type)), name(std::move(name)), hasOut(std::move(hasOut)), hasParams(std::move(hasParams)) { }

SLambdaExpParam::SLambdaExpParam(SLambdaExpParam&& other) noexcept = default;

SLambdaExpParam::~SLambdaExpParam() = default;

SLambdaExpParam& SLambdaExpParam::operator=(SLambdaExpParam&& other) noexcept = default;

JsonItem SLambdaExpParam::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SLambdaExpParam") },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
        { "hasOut", Citron::ToJson(hasOut) },
        { "hasParams", Citron::ToJson(hasParams) },
    };
}

SVarDeclElement::SVarDeclElement(std::string varName, SExpPtr initExp)
    : varName(std::move(varName)), initExp(std::move(initExp)) { }

SVarDeclElement::SVarDeclElement(SVarDeclElement&& other) noexcept = default;

SVarDeclElement::~SVarDeclElement() = default;

SVarDeclElement& SVarDeclElement::operator=(SVarDeclElement&& other) noexcept = default;

JsonItem SVarDeclElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclElement") },
        { "varName", Citron::ToJson(varName) },
        { "initExp", Citron::ToJson(initExp) },
    };
}

SVarDecl::SVarDecl(STypeExpPtr type, std::vector<SVarDeclElement> elements)
    : type(std::move(type)), elements(std::move(elements)) { }

SVarDecl::SVarDecl(SVarDecl&& other) noexcept = default;

SVarDecl::~SVarDecl() = default;

SVarDecl& SVarDecl::operator=(SVarDecl&& other) noexcept = default;

JsonItem SVarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDecl") },
        { "type", Citron::ToJson(type) },
        { "elements", Citron::ToJson(elements) },
    };
}

STypeParam::STypeParam(std::string name)
    : name(std::move(name)) { }

STypeParam::STypeParam(STypeParam&& other) noexcept = default;

STypeParam::~STypeParam() = default;

STypeParam& STypeParam::operator=(STypeParam&& other) noexcept = default;

JsonItem STypeParam::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeParam") },
        { "name", Citron::ToJson(name) },
    };
}

SFuncParam::SFuncParam(bool hasOut, bool hasParams, STypeExpPtr type, std::string name)
    : hasOut(std::move(hasOut)), hasParams(std::move(hasParams)), type(std::move(type)), name(std::move(name)) { }

SFuncParam::SFuncParam(SFuncParam&& other) noexcept = default;

SFuncParam::~SFuncParam() = default;

SFuncParam& SFuncParam::operator=(SFuncParam&& other) noexcept = default;

JsonItem SFuncParam::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SFuncParam") },
        { "hasOut", Citron::ToJson(hasOut) },
        { "hasParams", Citron::ToJson(hasParams) },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

struct SStmtToJsonVisitor : public SStmtVisitor
{
    JsonItem result;
    void Visit(SStmt_Command& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_VarDecl& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_If& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_IfTest& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_For& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Continue& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Break& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Return& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Block& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Blank& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Exp& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Task& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Await& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Async& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Foreach& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Yield& stmt) override { result = stmt.ToJson(); }
    void Visit(SStmt_Directive& stmt) override { result = stmt.ToJson(); }
};

JsonItem ToJson(SStmtPtr& stmt)
{
    if (!stmt) return JsonNull();

    SStmtToJsonVisitor visitor;
    stmt->Accept(visitor);
    return visitor.result;
}
struct SExpToJsonVisitor : public SExpVisitor
{
    JsonItem result;
    void Visit(SExp_Identifier& exp) override { result = exp.ToJson(); }
    void Visit(SExp_String& exp) override { result = exp.ToJson(); }
    void Visit(SExp_IntLiteral& exp) override { result = exp.ToJson(); }
    void Visit(SExp_BoolLiteral& exp) override { result = exp.ToJson(); }
    void Visit(SExp_NullLiteral& exp) override { result = exp.ToJson(); }
    void Visit(SExp_BinaryOp& exp) override { result = exp.ToJson(); }
    void Visit(SExp_UnaryOp& exp) override { result = exp.ToJson(); }
    void Visit(SExp_Call& exp) override { result = exp.ToJson(); }
    void Visit(SExp_Lambda& exp) override { result = exp.ToJson(); }
    void Visit(SExp_Indexer& exp) override { result = exp.ToJson(); }
    void Visit(SExp_Member& exp) override { result = exp.ToJson(); }
    void Visit(SExp_IndirectMember& exp) override { result = exp.ToJson(); }
    void Visit(SExp_List& exp) override { result = exp.ToJson(); }
    void Visit(SExp_New& exp) override { result = exp.ToJson(); }
    void Visit(SExp_Box& exp) override { result = exp.ToJson(); }
    void Visit(SExp_Is& exp) override { result = exp.ToJson(); }
    void Visit(SExp_As& exp) override { result = exp.ToJson(); }
};

JsonItem ToJson(SExpPtr& exp)
{
    if (!exp) return JsonNull();

    SExpToJsonVisitor visitor;
    exp->Accept(visitor);
    return visitor.result;
}
struct STypeExpToJsonVisitor : public STypeExpVisitor
{
    JsonItem result;
    void Visit(STypeExp_Id& typeExp) override { result = typeExp.ToJson(); }
    void Visit(STypeExp_Member& typeExp) override { result = typeExp.ToJson(); }
    void Visit(STypeExp_Nullable& typeExp) override { result = typeExp.ToJson(); }
    void Visit(STypeExp_LocalPtr& typeExp) override { result = typeExp.ToJson(); }
    void Visit(STypeExp_BoxPtr& typeExp) override { result = typeExp.ToJson(); }
    void Visit(STypeExp_Local& typeExp) override { result = typeExp.ToJson(); }
};

JsonItem ToJson(STypeExpPtr& typeExp)
{
    if (!typeExp) return JsonNull();

    STypeExpToJsonVisitor visitor;
    typeExp->Accept(visitor);
    return visitor.result;
}
struct SStringExpElementToJsonVisitor : public SStringExpElementVisitor
{
    JsonItem result;
    void Visit(SStringExpElement_Text& elem) override { result = elem.ToJson(); }
    void Visit(SStringExpElement_Exp& elem) override { result = elem.ToJson(); }
};

JsonItem ToJson(SStringExpElementPtr& elem)
{
    if (!elem) return JsonNull();

    SStringExpElementToJsonVisitor visitor;
    elem->Accept(visitor);
    return visitor.result;
}
struct SLambdaExpBodyToJsonVisitor : public SLambdaExpBodyVisitor
{
    JsonItem result;
    void Visit(SLambdaExpBody_Stmts& body) override { result = body.ToJson(); }
    void Visit(SLambdaExpBody_Exp& body) override { result = body.ToJson(); }
};

JsonItem ToJson(SLambdaExpBodyPtr& body)
{
    if (!body) return JsonNull();

    SLambdaExpBodyToJsonVisitor visitor;
    body->Accept(visitor);
    return visitor.result;
}
struct SEmbeddableStmtToJsonVisitor : public SEmbeddableStmtVisitor
{
    JsonItem result;
    void Visit(SEmbeddableStmt_Single& stmt) override { result = stmt.ToJson(); }
    void Visit(SEmbeddableStmt_Block& stmt) override { result = stmt.ToJson(); }
};

JsonItem ToJson(SEmbeddableStmtPtr& stmt)
{
    if (!stmt) return JsonNull();

    SEmbeddableStmtToJsonVisitor visitor;
    stmt->Accept(visitor);
    return visitor.result;
}
struct SForStmtInitializerToJsonVisitor : public SForStmtInitializerVisitor
{
    JsonItem result;
    void Visit(SForStmtInitializer_Exp& initializer) override { result = initializer.ToJson(); }
    void Visit(SForStmtInitializer_VarDecl& initializer) override { result = initializer.ToJson(); }
};

JsonItem ToJson(SForStmtInitializerPtr& initializer)
{
    if (!initializer) return JsonNull();

    SForStmtInitializerToJsonVisitor visitor;
    initializer->Accept(visitor);
    return visitor.result;
}
struct SClassMemberDeclToJsonVisitor : public SClassMemberDeclVisitor
{
    JsonItem result;
    void Visit(SClassDecl& decl) override { result = decl.ToJson(); }
    void Visit(SStructDecl& decl) override { result = decl.ToJson(); }
    void Visit(SEnumDecl& decl) override { result = decl.ToJson(); }
    void Visit(SClassMemberFuncDecl& decl) override { result = decl.ToJson(); }
    void Visit(SClassConstructorDecl& decl) override { result = decl.ToJson(); }
    void Visit(SClassMemberVarDecl& decl) override { result = decl.ToJson(); }
};

JsonItem ToJson(SClassMemberDeclPtr& decl)
{
    if (!decl) return JsonNull();

    SClassMemberDeclToJsonVisitor visitor;
    decl->Accept(visitor);
    return visitor.result;
}
struct SStructMemberDeclToJsonVisitor : public SStructMemberDeclVisitor
{
    JsonItem result;
    void Visit(SClassDecl& decl) override { result = decl.ToJson(); }
    void Visit(SStructDecl& decl) override { result = decl.ToJson(); }
    void Visit(SEnumDecl& decl) override { result = decl.ToJson(); }
    void Visit(SStructMemberFuncDecl& decl) override { result = decl.ToJson(); }
    void Visit(SStructConstructorDecl& decl) override { result = decl.ToJson(); }
    void Visit(SStructMemberVarDecl& decl) override { result = decl.ToJson(); }
};

JsonItem ToJson(SStructMemberDeclPtr& decl)
{
    if (!decl) return JsonNull();

    SStructMemberDeclToJsonVisitor visitor;
    decl->Accept(visitor);
    return visitor.result;
}
struct SNamespaceDeclElementToJsonVisitor : public SNamespaceDeclElementVisitor
{
    JsonItem result;
    void Visit(SGlobalFuncDecl& elem) override { result = elem.ToJson(); }
    void Visit(SNamespaceDecl& elem) override { result = elem.ToJson(); }
    void Visit(SClassDecl& elem) override { result = elem.ToJson(); }
    void Visit(SStructDecl& elem) override { result = elem.ToJson(); }
    void Visit(SEnumDecl& elem) override { result = elem.ToJson(); }
};

JsonItem ToJson(SNamespaceDeclElementPtr& elem)
{
    if (!elem) return JsonNull();

    SNamespaceDeclElementToJsonVisitor visitor;
    elem->Accept(visitor);
    return visitor.result;
}
struct SScriptElementToJsonVisitor : public SScriptElementVisitor
{
    JsonItem result;
    void Visit(SNamespaceDecl& elem) override { result = elem.ToJson(); }
    void Visit(SGlobalFuncDecl& elem) override { result = elem.ToJson(); }
    void Visit(SClassDecl& elem) override { result = elem.ToJson(); }
    void Visit(SStructDecl& elem) override { result = elem.ToJson(); }
    void Visit(SEnumDecl& elem) override { result = elem.ToJson(); }
};

JsonItem ToJson(SScriptElementPtr& elem)
{
    if (!elem) return JsonNull();

    SScriptElementToJsonVisitor visitor;
    elem->Accept(visitor);
    return visitor.result;
}
SExp_Identifier::SExp_Identifier(std::string value, std::vector<STypeExpPtr> typeArgs)
    : value(std::move(value)), typeArgs(std::move(typeArgs)) { }

SExp_Identifier::SExp_Identifier(SExp_Identifier&& other) noexcept = default;

SExp_Identifier::~SExp_Identifier() = default;

SExp_Identifier& SExp_Identifier::operator=(SExp_Identifier&& other) noexcept = default;

JsonItem SExp_Identifier::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Identifier") },
        { "value", Citron::ToJson(value) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

SExp_String::SExp_String(std::vector<SStringExpElementPtr> elements)
    : elements(std::move(elements)) { }

SExp_String::SExp_String(SExp_String&& other) noexcept = default;

SExp_String::~SExp_String() = default;

SExp_String& SExp_String::operator=(SExp_String&& other) noexcept = default;

JsonItem SExp_String::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_String") },
        { "elements", Citron::ToJson(elements) },
    };
}

SExp_IntLiteral::SExp_IntLiteral(int value)
    : value(std::move(value)) { }

SExp_IntLiteral::SExp_IntLiteral(SExp_IntLiteral&& other) noexcept = default;

SExp_IntLiteral::~SExp_IntLiteral() = default;

SExp_IntLiteral& SExp_IntLiteral::operator=(SExp_IntLiteral&& other) noexcept = default;

JsonItem SExp_IntLiteral::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_IntLiteral") },
        { "value", Citron::ToJson(value) },
    };
}

SExp_BoolLiteral::SExp_BoolLiteral(bool value)
    : value(std::move(value)) { }

SExp_BoolLiteral::SExp_BoolLiteral(SExp_BoolLiteral&& other) noexcept = default;

SExp_BoolLiteral::~SExp_BoolLiteral() = default;

SExp_BoolLiteral& SExp_BoolLiteral::operator=(SExp_BoolLiteral&& other) noexcept = default;

JsonItem SExp_BoolLiteral::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_BoolLiteral") },
        { "value", Citron::ToJson(value) },
    };
}

SExp_NullLiteral::SExp_NullLiteral()
{ }
SExp_NullLiteral::SExp_NullLiteral(SExp_NullLiteral&& other) noexcept = default;

SExp_NullLiteral::~SExp_NullLiteral() = default;

SExp_NullLiteral& SExp_NullLiteral::operator=(SExp_NullLiteral&& other) noexcept = default;

JsonItem SExp_NullLiteral::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_NullLiteral") },
    };
}

SExp_List::SExp_List(std::vector<SExpPtr> elements)
    : elements(std::move(elements)) { }

SExp_List::SExp_List(SExp_List&& other) noexcept = default;

SExp_List::~SExp_List() = default;

SExp_List& SExp_List::operator=(SExp_List&& other) noexcept = default;

JsonItem SExp_List::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_List") },
        { "elements", Citron::ToJson(elements) },
    };
}

SExp_New::SExp_New(STypeExpPtr type, std::vector<SArgument> args)
    : type(std::move(type)), args(std::move(args)) { }

SExp_New::SExp_New(SExp_New&& other) noexcept = default;

SExp_New::~SExp_New() = default;

SExp_New& SExp_New::operator=(SExp_New&& other) noexcept = default;

JsonItem SExp_New::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_New") },
        { "type", Citron::ToJson(type) },
        { "args", Citron::ToJson(args) },
    };
}

SExp_BinaryOp::SExp_BinaryOp(SBinaryOpKind kind, SExpPtr operand0, SExpPtr operand1)
    : kind(std::move(kind)), operand0(std::move(operand0)), operand1(std::move(operand1)) { }

SExp_BinaryOp::SExp_BinaryOp(SExp_BinaryOp&& other) noexcept = default;

SExp_BinaryOp::~SExp_BinaryOp() = default;

SExp_BinaryOp& SExp_BinaryOp::operator=(SExp_BinaryOp&& other) noexcept = default;

JsonItem SExp_BinaryOp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_BinaryOp") },
        { "kind", Citron::ToJson(kind) },
        { "operand0", Citron::ToJson(operand0) },
        { "operand1", Citron::ToJson(operand1) },
    };
}

SExp_UnaryOp::SExp_UnaryOp(SUnaryOpKind kind, SExpPtr operand)
    : kind(std::move(kind)), operand(std::move(operand)) { }

SExp_UnaryOp::SExp_UnaryOp(SExp_UnaryOp&& other) noexcept = default;

SExp_UnaryOp::~SExp_UnaryOp() = default;

SExp_UnaryOp& SExp_UnaryOp::operator=(SExp_UnaryOp&& other) noexcept = default;

JsonItem SExp_UnaryOp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_UnaryOp") },
        { "kind", Citron::ToJson(kind) },
        { "operand", Citron::ToJson(operand) },
    };
}

SExp_Call::SExp_Call(SExpPtr callable, std::vector<SArgument> args)
    : callable(std::move(callable)), args(std::move(args)) { }

SExp_Call::SExp_Call(SExp_Call&& other) noexcept = default;

SExp_Call::~SExp_Call() = default;

SExp_Call& SExp_Call::operator=(SExp_Call&& other) noexcept = default;

JsonItem SExp_Call::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Call") },
        { "callable", Citron::ToJson(callable) },
        { "args", Citron::ToJson(args) },
    };
}

SExp_Lambda::SExp_Lambda(std::vector<SLambdaExpParam> params, SLambdaExpBodyPtr body)
    : params(std::move(params)), body(std::move(body)) { }

SExp_Lambda::SExp_Lambda(SExp_Lambda&& other) noexcept = default;

SExp_Lambda::~SExp_Lambda() = default;

SExp_Lambda& SExp_Lambda::operator=(SExp_Lambda&& other) noexcept = default;

JsonItem SExp_Lambda::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Lambda") },
        { "params", Citron::ToJson(params) },
        { "body", Citron::ToJson(body) },
    };
}

SExp_Indexer::SExp_Indexer(SExpPtr obj, SExpPtr index)
    : obj(std::move(obj)), index(std::move(index)) { }

SExp_Indexer::SExp_Indexer(SExp_Indexer&& other) noexcept = default;

SExp_Indexer::~SExp_Indexer() = default;

SExp_Indexer& SExp_Indexer::operator=(SExp_Indexer&& other) noexcept = default;

JsonItem SExp_Indexer::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Indexer") },
        { "obj", Citron::ToJson(obj) },
        { "index", Citron::ToJson(index) },
    };
}

SExp_Member::SExp_Member(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs)
    : parent(std::move(parent)), memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)) { }

SExp_Member::SExp_Member(SExp_Member&& other) noexcept = default;

SExp_Member::~SExp_Member() = default;

SExp_Member& SExp_Member::operator=(SExp_Member&& other) noexcept = default;

JsonItem SExp_Member::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Member") },
        { "parent", Citron::ToJson(parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

SExp_IndirectMember::SExp_IndirectMember(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs)
    : parent(std::move(parent)), memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)) { }

SExp_IndirectMember::SExp_IndirectMember(SExp_IndirectMember&& other) noexcept = default;

SExp_IndirectMember::~SExp_IndirectMember() = default;

SExp_IndirectMember& SExp_IndirectMember::operator=(SExp_IndirectMember&& other) noexcept = default;

JsonItem SExp_IndirectMember::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_IndirectMember") },
        { "parent", Citron::ToJson(parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

SExp_Box::SExp_Box(SExpPtr innerExp)
    : innerExp(std::move(innerExp)) { }

SExp_Box::SExp_Box(SExp_Box&& other) noexcept = default;

SExp_Box::~SExp_Box() = default;

SExp_Box& SExp_Box::operator=(SExp_Box&& other) noexcept = default;

JsonItem SExp_Box::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Box") },
        { "innerExp", Citron::ToJson(innerExp) },
    };
}

SExp_Is::SExp_Is(SExpPtr exp, STypeExpPtr type)
    : exp(std::move(exp)), type(std::move(type)) { }

SExp_Is::SExp_Is(SExp_Is&& other) noexcept = default;

SExp_Is::~SExp_Is() = default;

SExp_Is& SExp_Is::operator=(SExp_Is&& other) noexcept = default;

JsonItem SExp_Is::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Is") },
        { "exp", Citron::ToJson(exp) },
        { "type", Citron::ToJson(type) },
    };
}

SExp_As::SExp_As(SExpPtr exp, STypeExpPtr type)
    : exp(std::move(exp)), type(std::move(type)) { }

SExp_As::SExp_As(SExp_As&& other) noexcept = default;

SExp_As::~SExp_As() = default;

SExp_As& SExp_As::operator=(SExp_As&& other) noexcept = default;

JsonItem SExp_As::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_As") },
        { "exp", Citron::ToJson(exp) },
        { "type", Citron::ToJson(type) },
    };
}

STypeExp_Id::STypeExp_Id(std::string name, std::vector<STypeExpPtr> typeArgs)
    : name(std::move(name)), typeArgs(std::move(typeArgs)) { }

STypeExp_Id::STypeExp_Id(STypeExp_Id&& other) noexcept = default;

STypeExp_Id::~STypeExp_Id() = default;

STypeExp_Id& STypeExp_Id::operator=(STypeExp_Id&& other) noexcept = default;

JsonItem STypeExp_Id::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_Id") },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

STypeExp_Member::STypeExp_Member(STypeExpPtr parentType, std::string name, std::vector<STypeExpPtr> typeArgs)
    : parentType(std::move(parentType)), name(std::move(name)), typeArgs(std::move(typeArgs)) { }

STypeExp_Member::STypeExp_Member(STypeExp_Member&& other) noexcept = default;

STypeExp_Member::~STypeExp_Member() = default;

STypeExp_Member& STypeExp_Member::operator=(STypeExp_Member&& other) noexcept = default;

JsonItem STypeExp_Member::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_Member") },
        { "parentType", Citron::ToJson(parentType) },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

STypeExp_Nullable::STypeExp_Nullable(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

STypeExp_Nullable::STypeExp_Nullable(STypeExp_Nullable&& other) noexcept = default;

STypeExp_Nullable::~STypeExp_Nullable() = default;

STypeExp_Nullable& STypeExp_Nullable::operator=(STypeExp_Nullable&& other) noexcept = default;

JsonItem STypeExp_Nullable::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_Nullable") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

STypeExp_LocalPtr::STypeExp_LocalPtr(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

STypeExp_LocalPtr::STypeExp_LocalPtr(STypeExp_LocalPtr&& other) noexcept = default;

STypeExp_LocalPtr::~STypeExp_LocalPtr() = default;

STypeExp_LocalPtr& STypeExp_LocalPtr::operator=(STypeExp_LocalPtr&& other) noexcept = default;

JsonItem STypeExp_LocalPtr::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_LocalPtr") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

STypeExp_BoxPtr::STypeExp_BoxPtr(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

STypeExp_BoxPtr::STypeExp_BoxPtr(STypeExp_BoxPtr&& other) noexcept = default;

STypeExp_BoxPtr::~STypeExp_BoxPtr() = default;

STypeExp_BoxPtr& STypeExp_BoxPtr::operator=(STypeExp_BoxPtr&& other) noexcept = default;

JsonItem STypeExp_BoxPtr::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_BoxPtr") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

STypeExp_Local::STypeExp_Local(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

STypeExp_Local::STypeExp_Local(STypeExp_Local&& other) noexcept = default;

STypeExp_Local::~STypeExp_Local() = default;

STypeExp_Local& STypeExp_Local::operator=(STypeExp_Local&& other) noexcept = default;

JsonItem STypeExp_Local::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_Local") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

SStringExpElement_Text::SStringExpElement_Text(std::string text)
    : text(std::move(text)) { }

SStringExpElement_Text::SStringExpElement_Text(SStringExpElement_Text&& other) noexcept = default;

SStringExpElement_Text::~SStringExpElement_Text() = default;

SStringExpElement_Text& SStringExpElement_Text::operator=(SStringExpElement_Text&& other) noexcept = default;

JsonItem SStringExpElement_Text::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStringExpElement_Text") },
        { "text", Citron::ToJson(text) },
    };
}

SStringExpElement_Exp::SStringExpElement_Exp(SExpPtr exp)
    : exp(std::move(exp)) { }

SStringExpElement_Exp::SStringExpElement_Exp(SStringExpElement_Exp&& other) noexcept = default;

SStringExpElement_Exp::~SStringExpElement_Exp() = default;

SStringExpElement_Exp& SStringExpElement_Exp::operator=(SStringExpElement_Exp&& other) noexcept = default;

JsonItem SStringExpElement_Exp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStringExpElement_Exp") },
        { "exp", Citron::ToJson(exp) },
    };
}

SLambdaExpBody_Stmts::SLambdaExpBody_Stmts(std::vector<SStmtPtr> stmts)
    : stmts(std::move(stmts)) { }

SLambdaExpBody_Stmts::SLambdaExpBody_Stmts(SLambdaExpBody_Stmts&& other) noexcept = default;

SLambdaExpBody_Stmts::~SLambdaExpBody_Stmts() = default;

SLambdaExpBody_Stmts& SLambdaExpBody_Stmts::operator=(SLambdaExpBody_Stmts&& other) noexcept = default;

JsonItem SLambdaExpBody_Stmts::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SLambdaExpBody_Stmts") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

SLambdaExpBody_Exp::SLambdaExpBody_Exp(SExpPtr exp)
    : exp(std::move(exp)) { }

SLambdaExpBody_Exp::SLambdaExpBody_Exp(SLambdaExpBody_Exp&& other) noexcept = default;

SLambdaExpBody_Exp::~SLambdaExpBody_Exp() = default;

SLambdaExpBody_Exp& SLambdaExpBody_Exp::operator=(SLambdaExpBody_Exp&& other) noexcept = default;

JsonItem SLambdaExpBody_Exp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SLambdaExpBody_Exp") },
        { "exp", Citron::ToJson(exp) },
    };
}

SEmbeddableStmt_Single::SEmbeddableStmt_Single(SStmtPtr stmt)
    : stmt(std::move(stmt)) { }

SEmbeddableStmt_Single::SEmbeddableStmt_Single(SEmbeddableStmt_Single&& other) noexcept = default;

SEmbeddableStmt_Single::~SEmbeddableStmt_Single() = default;

SEmbeddableStmt_Single& SEmbeddableStmt_Single::operator=(SEmbeddableStmt_Single&& other) noexcept = default;

JsonItem SEmbeddableStmt_Single::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SEmbeddableStmt_Single") },
        { "stmt", Citron::ToJson(stmt) },
    };
}

SEmbeddableStmt_Block::SEmbeddableStmt_Block(std::vector<SStmtPtr> stmts)
    : stmts(std::move(stmts)) { }

SEmbeddableStmt_Block::SEmbeddableStmt_Block(SEmbeddableStmt_Block&& other) noexcept = default;

SEmbeddableStmt_Block::~SEmbeddableStmt_Block() = default;

SEmbeddableStmt_Block& SEmbeddableStmt_Block::operator=(SEmbeddableStmt_Block&& other) noexcept = default;

JsonItem SEmbeddableStmt_Block::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SEmbeddableStmt_Block") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

SForStmtInitializer_Exp::SForStmtInitializer_Exp(SExpPtr exp)
    : exp(std::move(exp)) { }

SForStmtInitializer_Exp::SForStmtInitializer_Exp(SForStmtInitializer_Exp&& other) noexcept = default;

SForStmtInitializer_Exp::~SForStmtInitializer_Exp() = default;

SForStmtInitializer_Exp& SForStmtInitializer_Exp::operator=(SForStmtInitializer_Exp&& other) noexcept = default;

JsonItem SForStmtInitializer_Exp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SForStmtInitializer_Exp") },
        { "exp", Citron::ToJson(exp) },
    };
}

SForStmtInitializer_VarDecl::SForStmtInitializer_VarDecl(SVarDecl varDecl)
    : varDecl(std::move(varDecl)) { }

SForStmtInitializer_VarDecl::SForStmtInitializer_VarDecl(SForStmtInitializer_VarDecl&& other) noexcept = default;

SForStmtInitializer_VarDecl::~SForStmtInitializer_VarDecl() = default;

SForStmtInitializer_VarDecl& SForStmtInitializer_VarDecl::operator=(SForStmtInitializer_VarDecl&& other) noexcept = default;

JsonItem SForStmtInitializer_VarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SForStmtInitializer_VarDecl") },
        { "varDecl", Citron::ToJson(varDecl) },
    };
}

SStmt_Command::SStmt_Command(std::vector<std::shared_ptr<SExp_String>> commands)
    : commands(std::move(commands)) { }

SStmt_Command::SStmt_Command(SStmt_Command&& other) noexcept = default;

SStmt_Command::~SStmt_Command() = default;

SStmt_Command& SStmt_Command::operator=(SStmt_Command&& other) noexcept = default;

JsonItem SStmt_Command::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Command") },
        { "commands", Citron::ToJson(commands) },
    };
}

SStmt_VarDecl::SStmt_VarDecl(SVarDecl varDecl)
    : varDecl(std::move(varDecl)) { }

SStmt_VarDecl::SStmt_VarDecl(SStmt_VarDecl&& other) noexcept = default;

SStmt_VarDecl::~SStmt_VarDecl() = default;

SStmt_VarDecl& SStmt_VarDecl::operator=(SStmt_VarDecl&& other) noexcept = default;

JsonItem SStmt_VarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_VarDecl") },
        { "varDecl", Citron::ToJson(varDecl) },
    };
}

SStmt_Continue::SStmt_Continue()
{ }
SStmt_Continue::SStmt_Continue(SStmt_Continue&& other) noexcept = default;

SStmt_Continue::~SStmt_Continue() = default;

SStmt_Continue& SStmt_Continue::operator=(SStmt_Continue&& other) noexcept = default;

JsonItem SStmt_Continue::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Continue") },
    };
}

SStmt_Break::SStmt_Break()
{ }
SStmt_Break::SStmt_Break(SStmt_Break&& other) noexcept = default;

SStmt_Break::~SStmt_Break() = default;

SStmt_Break& SStmt_Break::operator=(SStmt_Break&& other) noexcept = default;

JsonItem SStmt_Break::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Break") },
    };
}

SStmt_Block::SStmt_Block(std::vector<SStmtPtr> stmts)
    : stmts(std::move(stmts)) { }

SStmt_Block::SStmt_Block(SStmt_Block&& other) noexcept = default;

SStmt_Block::~SStmt_Block() = default;

SStmt_Block& SStmt_Block::operator=(SStmt_Block&& other) noexcept = default;

JsonItem SStmt_Block::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Block") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

SStmt_Blank::SStmt_Blank()
{ }
SStmt_Blank::SStmt_Blank(SStmt_Blank&& other) noexcept = default;

SStmt_Blank::~SStmt_Blank() = default;

SStmt_Blank& SStmt_Blank::operator=(SStmt_Blank&& other) noexcept = default;

JsonItem SStmt_Blank::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Blank") },
    };
}

SStmt_Task::SStmt_Task(std::vector<SStmtPtr> body)
    : body(std::move(body)) { }

SStmt_Task::SStmt_Task(SStmt_Task&& other) noexcept = default;

SStmt_Task::~SStmt_Task() = default;

SStmt_Task& SStmt_Task::operator=(SStmt_Task&& other) noexcept = default;

JsonItem SStmt_Task::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Task") },
        { "body", Citron::ToJson(body) },
    };
}

SStmt_Await::SStmt_Await(std::vector<SStmtPtr> body)
    : body(std::move(body)) { }

SStmt_Await::SStmt_Await(SStmt_Await&& other) noexcept = default;

SStmt_Await::~SStmt_Await() = default;

SStmt_Await& SStmt_Await::operator=(SStmt_Await&& other) noexcept = default;

JsonItem SStmt_Await::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Await") },
        { "body", Citron::ToJson(body) },
    };
}

SStmt_Async::SStmt_Async(std::vector<SStmtPtr> body)
    : body(std::move(body)) { }

SStmt_Async::SStmt_Async(SStmt_Async&& other) noexcept = default;

SStmt_Async::~SStmt_Async() = default;

SStmt_Async& SStmt_Async::operator=(SStmt_Async&& other) noexcept = default;

JsonItem SStmt_Async::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Async") },
        { "body", Citron::ToJson(body) },
    };
}

SStmt_Directive::SStmt_Directive(std::string name, std::vector<SExpPtr> args)
    : name(std::move(name)), args(std::move(args)) { }

SStmt_Directive::SStmt_Directive(SStmt_Directive&& other) noexcept = default;

SStmt_Directive::~SStmt_Directive() = default;

SStmt_Directive& SStmt_Directive::operator=(SStmt_Directive&& other) noexcept = default;

JsonItem SStmt_Directive::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Directive") },
        { "name", Citron::ToJson(name) },
        { "args", Citron::ToJson(args) },
    };
}

SStmt_If::SStmt_If(SExpPtr cond, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody)
    : cond(std::move(cond)), body(std::move(body)), elseBody(std::move(elseBody)) { }

SStmt_If::SStmt_If(SStmt_If&& other) noexcept = default;

SStmt_If::~SStmt_If() = default;

SStmt_If& SStmt_If::operator=(SStmt_If&& other) noexcept = default;

JsonItem SStmt_If::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_If") },
        { "cond", Citron::ToJson(cond) },
        { "body", Citron::ToJson(body) },
        { "elseBody", Citron::ToJson(elseBody) },
    };
}

SStmt_IfTest::SStmt_IfTest(STypeExpPtr testType, std::string varName, SExpPtr exp, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody)
    : testType(std::move(testType)), varName(std::move(varName)), exp(std::move(exp)), body(std::move(body)), elseBody(std::move(elseBody)) { }

SStmt_IfTest::SStmt_IfTest(SStmt_IfTest&& other) noexcept = default;

SStmt_IfTest::~SStmt_IfTest() = default;

SStmt_IfTest& SStmt_IfTest::operator=(SStmt_IfTest&& other) noexcept = default;

JsonItem SStmt_IfTest::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_IfTest") },
        { "testType", Citron::ToJson(testType) },
        { "varName", Citron::ToJson(varName) },
        { "exp", Citron::ToJson(exp) },
        { "body", Citron::ToJson(body) },
        { "elseBody", Citron::ToJson(elseBody) },
    };
}

SStmt_For::SStmt_For(SForStmtInitializerPtr initializer, SExpPtr cond, SExpPtr cont, SEmbeddableStmtPtr body)
    : initializer(std::move(initializer)), cond(std::move(cond)), cont(std::move(cont)), body(std::move(body)) { }

SStmt_For::SStmt_For(SStmt_For&& other) noexcept = default;

SStmt_For::~SStmt_For() = default;

SStmt_For& SStmt_For::operator=(SStmt_For&& other) noexcept = default;

JsonItem SStmt_For::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_For") },
        { "initializer", Citron::ToJson(initializer) },
        { "cond", Citron::ToJson(cond) },
        { "cont", Citron::ToJson(cont) },
        { "body", Citron::ToJson(body) },
    };
}

SStmt_Return::SStmt_Return(SExpPtr value)
    : value(std::move(value)) { }

SStmt_Return::SStmt_Return(SStmt_Return&& other) noexcept = default;

SStmt_Return::~SStmt_Return() = default;

SStmt_Return& SStmt_Return::operator=(SStmt_Return&& other) noexcept = default;

JsonItem SStmt_Return::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Return") },
        { "value", Citron::ToJson(value) },
    };
}

SStmt_Exp::SStmt_Exp(SExpPtr exp)
    : exp(std::move(exp)) { }

SStmt_Exp::SStmt_Exp(SStmt_Exp&& other) noexcept = default;

SStmt_Exp::~SStmt_Exp() = default;

SStmt_Exp& SStmt_Exp::operator=(SStmt_Exp&& other) noexcept = default;

JsonItem SStmt_Exp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Exp") },
        { "exp", Citron::ToJson(exp) },
    };
}

SStmt_Foreach::SStmt_Foreach(STypeExpPtr type, std::string varName, SExpPtr enumerable, SEmbeddableStmtPtr body)
    : type(std::move(type)), varName(std::move(varName)), enumerable(std::move(enumerable)), body(std::move(body)) { }

SStmt_Foreach::SStmt_Foreach(SStmt_Foreach&& other) noexcept = default;

SStmt_Foreach::~SStmt_Foreach() = default;

SStmt_Foreach& SStmt_Foreach::operator=(SStmt_Foreach&& other) noexcept = default;

JsonItem SStmt_Foreach::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Foreach") },
        { "type", Citron::ToJson(type) },
        { "varName", Citron::ToJson(varName) },
        { "enumerable", Citron::ToJson(enumerable) },
        { "body", Citron::ToJson(body) },
    };
}

SStmt_Yield::SStmt_Yield(SExpPtr value)
    : value(std::move(value)) { }

SStmt_Yield::SStmt_Yield(SStmt_Yield&& other) noexcept = default;

SStmt_Yield::~SStmt_Yield() = default;

SStmt_Yield& SStmt_Yield::operator=(SStmt_Yield&& other) noexcept = default;

JsonItem SStmt_Yield::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Yield") },
        { "value", Citron::ToJson(value) },
    };
}

SGlobalFuncDecl::SGlobalFuncDecl(std::optional<SAccessModifier> accessModifier, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body)
    : accessModifier(std::move(accessModifier)), bSequence(std::move(bSequence)), retType(std::move(retType)), name(std::move(name)), typeParams(std::move(typeParams)), parameters(std::move(parameters)), body(std::move(body)) { }

SGlobalFuncDecl::SGlobalFuncDecl(SGlobalFuncDecl&& other) noexcept = default;

SGlobalFuncDecl::~SGlobalFuncDecl() = default;

SGlobalFuncDecl& SGlobalFuncDecl::operator=(SGlobalFuncDecl&& other) noexcept = default;

JsonItem SGlobalFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SGlobalFuncDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "bSequence", Citron::ToJson(bSequence) },
        { "retType", Citron::ToJson(retType) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SClassDecl::SClassDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExpPtr> baseTypes, std::vector<SClassMemberDeclPtr> memberDecls)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), typeParams(std::move(typeParams)), baseTypes(std::move(baseTypes)), memberDecls(std::move(memberDecls)) { }

SClassDecl::SClassDecl(SClassDecl&& other) noexcept = default;

SClassDecl::~SClassDecl() = default;

SClassDecl& SClassDecl::operator=(SClassDecl&& other) noexcept = default;

JsonItem SClassDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "baseTypes", Citron::ToJson(baseTypes) },
        { "memberDecls", Citron::ToJson(memberDecls) },
    };
}

SClassMemberFuncDecl::SClassMemberFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body)
    : accessModifier(std::move(accessModifier)), bStatic(std::move(bStatic)), bSequence(std::move(bSequence)), retType(std::move(retType)), name(std::move(name)), typeParams(std::move(typeParams)), parameters(std::move(parameters)), body(std::move(body)) { }

SClassMemberFuncDecl::SClassMemberFuncDecl(SClassMemberFuncDecl&& other) noexcept = default;

SClassMemberFuncDecl::~SClassMemberFuncDecl() = default;

SClassMemberFuncDecl& SClassMemberFuncDecl::operator=(SClassMemberFuncDecl&& other) noexcept = default;

JsonItem SClassMemberFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassMemberFuncDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "bStatic", Citron::ToJson(bStatic) },
        { "bSequence", Citron::ToJson(bSequence) },
        { "retType", Citron::ToJson(retType) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SClassConstructorDecl::SClassConstructorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, std::optional<std::vector<SArgument>> baseArgs, std::vector<SStmtPtr> body)
    : accessModifier(std::move(accessModifier)), parameters(std::move(parameters)), baseArgs(std::move(baseArgs)), body(std::move(body)) { }

SClassConstructorDecl::SClassConstructorDecl(SClassConstructorDecl&& other) noexcept = default;

SClassConstructorDecl::~SClassConstructorDecl() = default;

SClassConstructorDecl& SClassConstructorDecl::operator=(SClassConstructorDecl&& other) noexcept = default;

JsonItem SClassConstructorDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassConstructorDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "parameters", Citron::ToJson(parameters) },
        { "baseArgs", Citron::ToJson(baseArgs) },
        { "body", Citron::ToJson(body) },
    };
}

SClassMemberVarDecl::SClassMemberVarDecl(std::optional<SAccessModifier> accessModifier, STypeExpPtr varType, std::vector<std::string> varNames)
    : accessModifier(std::move(accessModifier)), varType(std::move(varType)), varNames(std::move(varNames)) { }

SClassMemberVarDecl::SClassMemberVarDecl(SClassMemberVarDecl&& other) noexcept = default;

SClassMemberVarDecl::~SClassMemberVarDecl() = default;

SClassMemberVarDecl& SClassMemberVarDecl::operator=(SClassMemberVarDecl&& other) noexcept = default;

JsonItem SClassMemberVarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassMemberVarDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "varType", Citron::ToJson(varType) },
        { "varNames", Citron::ToJson(varNames) },
    };
}

SStructDecl::SStructDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExpPtr> baseTypes, std::vector<SStructMemberDeclPtr> memberDecls)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), typeParams(std::move(typeParams)), baseTypes(std::move(baseTypes)), memberDecls(std::move(memberDecls)) { }

SStructDecl::SStructDecl(SStructDecl&& other) noexcept = default;

SStructDecl::~SStructDecl() = default;

SStructDecl& SStructDecl::operator=(SStructDecl&& other) noexcept = default;

JsonItem SStructDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "baseTypes", Citron::ToJson(baseTypes) },
        { "memberDecls", Citron::ToJson(memberDecls) },
    };
}

SStructMemberFuncDecl::SStructMemberFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExpPtr retType, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body)
    : accessModifier(std::move(accessModifier)), bStatic(std::move(bStatic)), bSequence(std::move(bSequence)), retType(std::move(retType)), name(std::move(name)), typeParams(std::move(typeParams)), parameters(std::move(parameters)), body(std::move(body)) { }

SStructMemberFuncDecl::SStructMemberFuncDecl(SStructMemberFuncDecl&& other) noexcept = default;

SStructMemberFuncDecl::~SStructMemberFuncDecl() = default;

SStructMemberFuncDecl& SStructMemberFuncDecl::operator=(SStructMemberFuncDecl&& other) noexcept = default;

JsonItem SStructMemberFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructMemberFuncDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "bStatic", Citron::ToJson(bStatic) },
        { "bSequence", Citron::ToJson(bSequence) },
        { "retType", Citron::ToJson(retType) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SStructConstructorDecl::SStructConstructorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body)
    : accessModifier(std::move(accessModifier)), parameters(std::move(parameters)), body(std::move(body)) { }

SStructConstructorDecl::SStructConstructorDecl(SStructConstructorDecl&& other) noexcept = default;

SStructConstructorDecl::~SStructConstructorDecl() = default;

SStructConstructorDecl& SStructConstructorDecl::operator=(SStructConstructorDecl&& other) noexcept = default;

JsonItem SStructConstructorDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructConstructorDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SStructMemberVarDecl::SStructMemberVarDecl(std::optional<SAccessModifier> accessModifier, STypeExpPtr varType, std::vector<std::string> varNames)
    : accessModifier(std::move(accessModifier)), varType(std::move(varType)), varNames(std::move(varNames)) { }

SStructMemberVarDecl::SStructMemberVarDecl(SStructMemberVarDecl&& other) noexcept = default;

SStructMemberVarDecl::~SStructMemberVarDecl() = default;

SStructMemberVarDecl& SStructMemberVarDecl::operator=(SStructMemberVarDecl&& other) noexcept = default;

JsonItem SStructMemberVarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructMemberVarDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "varType", Citron::ToJson(varType) },
        { "varNames", Citron::ToJson(varNames) },
    };
}

SEnumElemMemberVarDecl::SEnumElemMemberVarDecl(STypeExpPtr type, std::string name)
    : type(std::move(type)), name(std::move(name)) { }

SEnumElemMemberVarDecl::SEnumElemMemberVarDecl(SEnumElemMemberVarDecl&& other) noexcept = default;

SEnumElemMemberVarDecl::~SEnumElemMemberVarDecl() = default;

SEnumElemMemberVarDecl& SEnumElemMemberVarDecl::operator=(SEnumElemMemberVarDecl&& other) noexcept = default;

JsonItem SEnumElemMemberVarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SEnumElemMemberVarDecl") },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

SEnumElemDecl::SEnumElemDecl(std::string name, std::vector<std::shared_ptr<SEnumElemMemberVarDecl>> memberVars)
    : name(std::move(name)), memberVars(std::move(memberVars)) { }

SEnumElemDecl::SEnumElemDecl(SEnumElemDecl&& other) noexcept = default;

SEnumElemDecl::~SEnumElemDecl() = default;

SEnumElemDecl& SEnumElemDecl::operator=(SEnumElemDecl&& other) noexcept = default;

JsonItem SEnumElemDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SEnumElemDecl") },
        { "name", Citron::ToJson(name) },
        { "memberVars", Citron::ToJson(memberVars) },
    };
}

SEnumDecl::SEnumDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<std::shared_ptr<SEnumElemDecl>> elements)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), typeParams(std::move(typeParams)), elements(std::move(elements)) { }

SEnumDecl::SEnumDecl(SEnumDecl&& other) noexcept = default;

SEnumDecl::~SEnumDecl() = default;

SEnumDecl& SEnumDecl::operator=(SEnumDecl&& other) noexcept = default;

JsonItem SEnumDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SEnumDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "elements", Citron::ToJson(elements) },
    };
}

SNamespaceDecl::SNamespaceDecl(std::vector<std::string> names, std::vector<SNamespaceDeclElementPtr> elements)
    : names(std::move(names)), elements(std::move(elements)) { }

SNamespaceDecl::SNamespaceDecl(SNamespaceDecl&& other) noexcept = default;

SNamespaceDecl::~SNamespaceDecl() = default;

SNamespaceDecl& SNamespaceDecl::operator=(SNamespaceDecl&& other) noexcept = default;

JsonItem SNamespaceDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SNamespaceDecl") },
        { "names", Citron::ToJson(names) },
        { "elements", Citron::ToJson(elements) },
    };
}

SScript::SScript(std::vector<SScriptElementPtr> elements)
    : elements(std::move(elements)) { }

SScript::SScript(SScript&& other) noexcept = default;

SScript::~SScript() = default;

SScript& SScript::operator=(SScript&& other) noexcept = default;

JsonItem SScript::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SScript") },
        { "elements", Citron::ToJson(elements) },
    };
}

}
