#include "pch.h"

#include <Syntax/Syntaxes.g.h>
#include <Infra/Json.h>

using namespace std;

namespace Citron {

namespace {
struct ToJsonVisitor {
    template<typename T>
    JsonItem operator()(std::unique_ptr<T>& t) { return t->ToJson(); }

    template<typename T>
    JsonItem operator()(T& t) { return t.ToJson(); }
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
    void Visit(SCommandStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SVarDeclStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SIfStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SIfTestStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SForStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SContinueStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SBreakStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SReturnStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SBlockStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SBlankStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SExpStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(STaskStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SAwaitStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SAsyncStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SForeachStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SYieldStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SDirectiveStmt& stmt) override { result = stmt.ToJson(); }
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
    void Visit(SIdentifierExp& exp) override { result = exp.ToJson(); }
    void Visit(SStringExp& exp) override { result = exp.ToJson(); }
    void Visit(SIntLiteralExp& exp) override { result = exp.ToJson(); }
    void Visit(SBoolLiteralExp& exp) override { result = exp.ToJson(); }
    void Visit(SNullLiteralExp& exp) override { result = exp.ToJson(); }
    void Visit(SBinaryOpExp& exp) override { result = exp.ToJson(); }
    void Visit(SUnaryOpExp& exp) override { result = exp.ToJson(); }
    void Visit(SCallExp& exp) override { result = exp.ToJson(); }
    void Visit(SLambdaExp& exp) override { result = exp.ToJson(); }
    void Visit(SIndexerExp& exp) override { result = exp.ToJson(); }
    void Visit(SMemberExp& exp) override { result = exp.ToJson(); }
    void Visit(SIndirectMemberExp& exp) override { result = exp.ToJson(); }
    void Visit(SListExp& exp) override { result = exp.ToJson(); }
    void Visit(SNewExp& exp) override { result = exp.ToJson(); }
    void Visit(SBoxExp& exp) override { result = exp.ToJson(); }
    void Visit(SIsExp& exp) override { result = exp.ToJson(); }
    void Visit(SAsExp& exp) override { result = exp.ToJson(); }
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
    void Visit(SIdTypeExp& typeExp) override { result = typeExp.ToJson(); }
    void Visit(SMemberTypeExp& typeExp) override { result = typeExp.ToJson(); }
    void Visit(SNullableTypeExp& typeExp) override { result = typeExp.ToJson(); }
    void Visit(SLocalPtrTypeExp& typeExp) override { result = typeExp.ToJson(); }
    void Visit(SBoxPtrTypeExp& typeExp) override { result = typeExp.ToJson(); }
    void Visit(SLocalTypeExp& typeExp) override { result = typeExp.ToJson(); }
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
    void Visit(STextStringExpElement& elem) override { result = elem.ToJson(); }
    void Visit(SExpStringExpElement& elem) override { result = elem.ToJson(); }
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
    void Visit(SStmtsLambdaExpBody& body) override { result = body.ToJson(); }
    void Visit(SExpLambdaExpBody& body) override { result = body.ToJson(); }
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
    void Visit(SSingleEmbeddableStmt& stmt) override { result = stmt.ToJson(); }
    void Visit(SBlockEmbeddableStmt& stmt) override { result = stmt.ToJson(); }
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
    void Visit(SExpForStmtInitializer& initializer) override { result = initializer.ToJson(); }
    void Visit(SVarDeclForStmtInitializer& initializer) override { result = initializer.ToJson(); }
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
SIdentifierExp::SIdentifierExp(std::string value, std::vector<STypeExpPtr> typeArgs)
    : value(std::move(value)), typeArgs(std::move(typeArgs)) { }

SIdentifierExp::SIdentifierExp(SIdentifierExp&& other) noexcept = default;

SIdentifierExp::~SIdentifierExp() = default;

SIdentifierExp& SIdentifierExp::operator=(SIdentifierExp&& other) noexcept = default;

JsonItem SIdentifierExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIdentifierExp") },
        { "value", Citron::ToJson(value) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

SStringExp::SStringExp(std::vector<SStringExpElementPtr> elements)
    : elements(std::move(elements)) { }

SStringExp::SStringExp(SStringExp&& other) noexcept = default;

SStringExp::~SStringExp() = default;

SStringExp& SStringExp::operator=(SStringExp&& other) noexcept = default;

JsonItem SStringExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStringExp") },
        { "elements", Citron::ToJson(elements) },
    };
}

SIntLiteralExp::SIntLiteralExp(int value)
    : value(std::move(value)) { }

SIntLiteralExp::SIntLiteralExp(SIntLiteralExp&& other) noexcept = default;

SIntLiteralExp::~SIntLiteralExp() = default;

SIntLiteralExp& SIntLiteralExp::operator=(SIntLiteralExp&& other) noexcept = default;

JsonItem SIntLiteralExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIntLiteralExp") },
        { "value", Citron::ToJson(value) },
    };
}

SBoolLiteralExp::SBoolLiteralExp(bool value)
    : value(std::move(value)) { }

SBoolLiteralExp::SBoolLiteralExp(SBoolLiteralExp&& other) noexcept = default;

SBoolLiteralExp::~SBoolLiteralExp() = default;

SBoolLiteralExp& SBoolLiteralExp::operator=(SBoolLiteralExp&& other) noexcept = default;

JsonItem SBoolLiteralExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBoolLiteralExp") },
        { "value", Citron::ToJson(value) },
    };
}

SNullLiteralExp::SNullLiteralExp()
{ }
SNullLiteralExp::SNullLiteralExp(SNullLiteralExp&& other) noexcept = default;

SNullLiteralExp::~SNullLiteralExp() = default;

SNullLiteralExp& SNullLiteralExp::operator=(SNullLiteralExp&& other) noexcept = default;

JsonItem SNullLiteralExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SNullLiteralExp") },
    };
}

SListExp::SListExp(std::vector<SExpPtr> elements)
    : elements(std::move(elements)) { }

SListExp::SListExp(SListExp&& other) noexcept = default;

SListExp::~SListExp() = default;

SListExp& SListExp::operator=(SListExp&& other) noexcept = default;

JsonItem SListExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SListExp") },
        { "elements", Citron::ToJson(elements) },
    };
}

SNewExp::SNewExp(STypeExpPtr type, std::vector<SArgument> args)
    : type(std::move(type)), args(std::move(args)) { }

SNewExp::SNewExp(SNewExp&& other) noexcept = default;

SNewExp::~SNewExp() = default;

SNewExp& SNewExp::operator=(SNewExp&& other) noexcept = default;

JsonItem SNewExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SNewExp") },
        { "type", Citron::ToJson(type) },
        { "args", Citron::ToJson(args) },
    };
}

SBinaryOpExp::SBinaryOpExp(SBinaryOpKind kind, SExpPtr operand0, SExpPtr operand1)
    : kind(std::move(kind)), operand0(std::move(operand0)), operand1(std::move(operand1)) { }

SBinaryOpExp::SBinaryOpExp(SBinaryOpExp&& other) noexcept = default;

SBinaryOpExp::~SBinaryOpExp() = default;

SBinaryOpExp& SBinaryOpExp::operator=(SBinaryOpExp&& other) noexcept = default;

JsonItem SBinaryOpExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBinaryOpExp") },
        { "kind", Citron::ToJson(kind) },
        { "operand0", Citron::ToJson(operand0) },
        { "operand1", Citron::ToJson(operand1) },
    };
}

SUnaryOpExp::SUnaryOpExp(SUnaryOpKind kind, SExpPtr operand)
    : kind(std::move(kind)), operand(std::move(operand)) { }

SUnaryOpExp::SUnaryOpExp(SUnaryOpExp&& other) noexcept = default;

SUnaryOpExp::~SUnaryOpExp() = default;

SUnaryOpExp& SUnaryOpExp::operator=(SUnaryOpExp&& other) noexcept = default;

JsonItem SUnaryOpExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SUnaryOpExp") },
        { "kind", Citron::ToJson(kind) },
        { "operand", Citron::ToJson(operand) },
    };
}

SCallExp::SCallExp(SExpPtr callable, std::vector<SArgument> args)
    : callable(std::move(callable)), args(std::move(args)) { }

SCallExp::SCallExp(SCallExp&& other) noexcept = default;

SCallExp::~SCallExp() = default;

SCallExp& SCallExp::operator=(SCallExp&& other) noexcept = default;

JsonItem SCallExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SCallExp") },
        { "callable", Citron::ToJson(callable) },
        { "args", Citron::ToJson(args) },
    };
}

SLambdaExp::SLambdaExp(std::vector<SLambdaExpParam> params, SLambdaExpBodyPtr body)
    : params(std::move(params)), body(std::move(body)) { }

SLambdaExp::SLambdaExp(SLambdaExp&& other) noexcept = default;

SLambdaExp::~SLambdaExp() = default;

SLambdaExp& SLambdaExp::operator=(SLambdaExp&& other) noexcept = default;

JsonItem SLambdaExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SLambdaExp") },
        { "params", Citron::ToJson(params) },
        { "body", Citron::ToJson(body) },
    };
}

SIndexerExp::SIndexerExp(SExpPtr obj, SExpPtr index)
    : obj(std::move(obj)), index(std::move(index)) { }

SIndexerExp::SIndexerExp(SIndexerExp&& other) noexcept = default;

SIndexerExp::~SIndexerExp() = default;

SIndexerExp& SIndexerExp::operator=(SIndexerExp&& other) noexcept = default;

JsonItem SIndexerExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIndexerExp") },
        { "obj", Citron::ToJson(obj) },
        { "index", Citron::ToJson(index) },
    };
}

SMemberExp::SMemberExp(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs)
    : parent(std::move(parent)), memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)) { }

SMemberExp::SMemberExp(SMemberExp&& other) noexcept = default;

SMemberExp::~SMemberExp() = default;

SMemberExp& SMemberExp::operator=(SMemberExp&& other) noexcept = default;

JsonItem SMemberExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SMemberExp") },
        { "parent", Citron::ToJson(parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

SIndirectMemberExp::SIndirectMemberExp(SExpPtr parent, std::string memberName, std::vector<STypeExpPtr> memberTypeArgs)
    : parent(std::move(parent)), memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)) { }

SIndirectMemberExp::SIndirectMemberExp(SIndirectMemberExp&& other) noexcept = default;

SIndirectMemberExp::~SIndirectMemberExp() = default;

SIndirectMemberExp& SIndirectMemberExp::operator=(SIndirectMemberExp&& other) noexcept = default;

JsonItem SIndirectMemberExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIndirectMemberExp") },
        { "parent", Citron::ToJson(parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

SBoxExp::SBoxExp(SExpPtr innerExp)
    : innerExp(std::move(innerExp)) { }

SBoxExp::SBoxExp(SBoxExp&& other) noexcept = default;

SBoxExp::~SBoxExp() = default;

SBoxExp& SBoxExp::operator=(SBoxExp&& other) noexcept = default;

JsonItem SBoxExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBoxExp") },
        { "innerExp", Citron::ToJson(innerExp) },
    };
}

SIsExp::SIsExp(SExpPtr exp, STypeExpPtr type)
    : exp(std::move(exp)), type(std::move(type)) { }

SIsExp::SIsExp(SIsExp&& other) noexcept = default;

SIsExp::~SIsExp() = default;

SIsExp& SIsExp::operator=(SIsExp&& other) noexcept = default;

JsonItem SIsExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIsExp") },
        { "exp", Citron::ToJson(exp) },
        { "type", Citron::ToJson(type) },
    };
}

SAsExp::SAsExp(SExpPtr exp, STypeExpPtr type)
    : exp(std::move(exp)), type(std::move(type)) { }

SAsExp::SAsExp(SAsExp&& other) noexcept = default;

SAsExp::~SAsExp() = default;

SAsExp& SAsExp::operator=(SAsExp&& other) noexcept = default;

JsonItem SAsExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SAsExp") },
        { "exp", Citron::ToJson(exp) },
        { "type", Citron::ToJson(type) },
    };
}

SIdTypeExp::SIdTypeExp(std::string name, std::vector<STypeExpPtr> typeArgs)
    : name(std::move(name)), typeArgs(std::move(typeArgs)) { }

SIdTypeExp::SIdTypeExp(SIdTypeExp&& other) noexcept = default;

SIdTypeExp::~SIdTypeExp() = default;

SIdTypeExp& SIdTypeExp::operator=(SIdTypeExp&& other) noexcept = default;

JsonItem SIdTypeExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIdTypeExp") },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

SMemberTypeExp::SMemberTypeExp(STypeExpPtr parentType, std::string name, std::vector<STypeExpPtr> typeArgs)
    : parentType(std::move(parentType)), name(std::move(name)), typeArgs(std::move(typeArgs)) { }

SMemberTypeExp::SMemberTypeExp(SMemberTypeExp&& other) noexcept = default;

SMemberTypeExp::~SMemberTypeExp() = default;

SMemberTypeExp& SMemberTypeExp::operator=(SMemberTypeExp&& other) noexcept = default;

JsonItem SMemberTypeExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SMemberTypeExp") },
        { "parentType", Citron::ToJson(parentType) },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

SNullableTypeExp::SNullableTypeExp(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

SNullableTypeExp::SNullableTypeExp(SNullableTypeExp&& other) noexcept = default;

SNullableTypeExp::~SNullableTypeExp() = default;

SNullableTypeExp& SNullableTypeExp::operator=(SNullableTypeExp&& other) noexcept = default;

JsonItem SNullableTypeExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SNullableTypeExp") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

SLocalPtrTypeExp::SLocalPtrTypeExp(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

SLocalPtrTypeExp::SLocalPtrTypeExp(SLocalPtrTypeExp&& other) noexcept = default;

SLocalPtrTypeExp::~SLocalPtrTypeExp() = default;

SLocalPtrTypeExp& SLocalPtrTypeExp::operator=(SLocalPtrTypeExp&& other) noexcept = default;

JsonItem SLocalPtrTypeExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SLocalPtrTypeExp") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

SBoxPtrTypeExp::SBoxPtrTypeExp(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

SBoxPtrTypeExp::SBoxPtrTypeExp(SBoxPtrTypeExp&& other) noexcept = default;

SBoxPtrTypeExp::~SBoxPtrTypeExp() = default;

SBoxPtrTypeExp& SBoxPtrTypeExp::operator=(SBoxPtrTypeExp&& other) noexcept = default;

JsonItem SBoxPtrTypeExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBoxPtrTypeExp") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

SLocalTypeExp::SLocalTypeExp(STypeExpPtr innerType)
    : innerType(std::move(innerType)) { }

SLocalTypeExp::SLocalTypeExp(SLocalTypeExp&& other) noexcept = default;

SLocalTypeExp::~SLocalTypeExp() = default;

SLocalTypeExp& SLocalTypeExp::operator=(SLocalTypeExp&& other) noexcept = default;

JsonItem SLocalTypeExp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SLocalTypeExp") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

STextStringExpElement::STextStringExpElement(std::string text)
    : text(std::move(text)) { }

STextStringExpElement::STextStringExpElement(STextStringExpElement&& other) noexcept = default;

STextStringExpElement::~STextStringExpElement() = default;

STextStringExpElement& STextStringExpElement::operator=(STextStringExpElement&& other) noexcept = default;

JsonItem STextStringExpElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STextStringExpElement") },
        { "text", Citron::ToJson(text) },
    };
}

SExpStringExpElement::SExpStringExpElement(SExpPtr exp)
    : exp(std::move(exp)) { }

SExpStringExpElement::SExpStringExpElement(SExpStringExpElement&& other) noexcept = default;

SExpStringExpElement::~SExpStringExpElement() = default;

SExpStringExpElement& SExpStringExpElement::operator=(SExpStringExpElement&& other) noexcept = default;

JsonItem SExpStringExpElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExpStringExpElement") },
        { "exp", Citron::ToJson(exp) },
    };
}

SStmtsLambdaExpBody::SStmtsLambdaExpBody(std::vector<SStmtPtr> stmts)
    : stmts(std::move(stmts)) { }

SStmtsLambdaExpBody::SStmtsLambdaExpBody(SStmtsLambdaExpBody&& other) noexcept = default;

SStmtsLambdaExpBody::~SStmtsLambdaExpBody() = default;

SStmtsLambdaExpBody& SStmtsLambdaExpBody::operator=(SStmtsLambdaExpBody&& other) noexcept = default;

JsonItem SStmtsLambdaExpBody::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmtsLambdaExpBody") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

SExpLambdaExpBody::SExpLambdaExpBody(SExpPtr exp)
    : exp(std::move(exp)) { }

SExpLambdaExpBody::SExpLambdaExpBody(SExpLambdaExpBody&& other) noexcept = default;

SExpLambdaExpBody::~SExpLambdaExpBody() = default;

SExpLambdaExpBody& SExpLambdaExpBody::operator=(SExpLambdaExpBody&& other) noexcept = default;

JsonItem SExpLambdaExpBody::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExpLambdaExpBody") },
        { "exp", Citron::ToJson(exp) },
    };
}

SSingleEmbeddableStmt::SSingleEmbeddableStmt(SStmtPtr stmt)
    : stmt(std::move(stmt)) { }

SSingleEmbeddableStmt::SSingleEmbeddableStmt(SSingleEmbeddableStmt&& other) noexcept = default;

SSingleEmbeddableStmt::~SSingleEmbeddableStmt() = default;

SSingleEmbeddableStmt& SSingleEmbeddableStmt::operator=(SSingleEmbeddableStmt&& other) noexcept = default;

JsonItem SSingleEmbeddableStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SSingleEmbeddableStmt") },
        { "stmt", Citron::ToJson(stmt) },
    };
}

SBlockEmbeddableStmt::SBlockEmbeddableStmt(std::vector<SStmtPtr> stmts)
    : stmts(std::move(stmts)) { }

SBlockEmbeddableStmt::SBlockEmbeddableStmt(SBlockEmbeddableStmt&& other) noexcept = default;

SBlockEmbeddableStmt::~SBlockEmbeddableStmt() = default;

SBlockEmbeddableStmt& SBlockEmbeddableStmt::operator=(SBlockEmbeddableStmt&& other) noexcept = default;

JsonItem SBlockEmbeddableStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBlockEmbeddableStmt") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

SExpForStmtInitializer::SExpForStmtInitializer(SExpPtr exp)
    : exp(std::move(exp)) { }

SExpForStmtInitializer::SExpForStmtInitializer(SExpForStmtInitializer&& other) noexcept = default;

SExpForStmtInitializer::~SExpForStmtInitializer() = default;

SExpForStmtInitializer& SExpForStmtInitializer::operator=(SExpForStmtInitializer&& other) noexcept = default;

JsonItem SExpForStmtInitializer::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExpForStmtInitializer") },
        { "exp", Citron::ToJson(exp) },
    };
}

SVarDeclForStmtInitializer::SVarDeclForStmtInitializer(SVarDecl varDecl)
    : varDecl(std::move(varDecl)) { }

SVarDeclForStmtInitializer::SVarDeclForStmtInitializer(SVarDeclForStmtInitializer&& other) noexcept = default;

SVarDeclForStmtInitializer::~SVarDeclForStmtInitializer() = default;

SVarDeclForStmtInitializer& SVarDeclForStmtInitializer::operator=(SVarDeclForStmtInitializer&& other) noexcept = default;

JsonItem SVarDeclForStmtInitializer::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclForStmtInitializer") },
        { "varDecl", Citron::ToJson(varDecl) },
    };
}

SCommandStmt::SCommandStmt(std::vector<SStringExp> commands)
    : commands(std::move(commands)) { }

SCommandStmt::SCommandStmt(SCommandStmt&& other) noexcept = default;

SCommandStmt::~SCommandStmt() = default;

SCommandStmt& SCommandStmt::operator=(SCommandStmt&& other) noexcept = default;

JsonItem SCommandStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SCommandStmt") },
        { "commands", Citron::ToJson(commands) },
    };
}

SVarDeclStmt::SVarDeclStmt(SVarDecl varDecl)
    : varDecl(std::move(varDecl)) { }

SVarDeclStmt::SVarDeclStmt(SVarDeclStmt&& other) noexcept = default;

SVarDeclStmt::~SVarDeclStmt() = default;

SVarDeclStmt& SVarDeclStmt::operator=(SVarDeclStmt&& other) noexcept = default;

JsonItem SVarDeclStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclStmt") },
        { "varDecl", Citron::ToJson(varDecl) },
    };
}

SContinueStmt::SContinueStmt()
{ }
SContinueStmt::SContinueStmt(SContinueStmt&& other) noexcept = default;

SContinueStmt::~SContinueStmt() = default;

SContinueStmt& SContinueStmt::operator=(SContinueStmt&& other) noexcept = default;

JsonItem SContinueStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SContinueStmt") },
    };
}

SBreakStmt::SBreakStmt()
{ }
SBreakStmt::SBreakStmt(SBreakStmt&& other) noexcept = default;

SBreakStmt::~SBreakStmt() = default;

SBreakStmt& SBreakStmt::operator=(SBreakStmt&& other) noexcept = default;

JsonItem SBreakStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBreakStmt") },
    };
}

SBlockStmt::SBlockStmt(std::vector<SStmtPtr> stmts)
    : stmts(std::move(stmts)) { }

SBlockStmt::SBlockStmt(SBlockStmt&& other) noexcept = default;

SBlockStmt::~SBlockStmt() = default;

SBlockStmt& SBlockStmt::operator=(SBlockStmt&& other) noexcept = default;

JsonItem SBlockStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBlockStmt") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

SBlankStmt::SBlankStmt()
{ }
SBlankStmt::SBlankStmt(SBlankStmt&& other) noexcept = default;

SBlankStmt::~SBlankStmt() = default;

SBlankStmt& SBlankStmt::operator=(SBlankStmt&& other) noexcept = default;

JsonItem SBlankStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SBlankStmt") },
    };
}

STaskStmt::STaskStmt(std::vector<SStmtPtr> body)
    : body(std::move(body)) { }

STaskStmt::STaskStmt(STaskStmt&& other) noexcept = default;

STaskStmt::~STaskStmt() = default;

STaskStmt& STaskStmt::operator=(STaskStmt&& other) noexcept = default;

JsonItem STaskStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STaskStmt") },
        { "body", Citron::ToJson(body) },
    };
}

SAwaitStmt::SAwaitStmt(std::vector<SStmtPtr> body)
    : body(std::move(body)) { }

SAwaitStmt::SAwaitStmt(SAwaitStmt&& other) noexcept = default;

SAwaitStmt::~SAwaitStmt() = default;

SAwaitStmt& SAwaitStmt::operator=(SAwaitStmt&& other) noexcept = default;

JsonItem SAwaitStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SAwaitStmt") },
        { "body", Citron::ToJson(body) },
    };
}

SAsyncStmt::SAsyncStmt(std::vector<SStmtPtr> body)
    : body(std::move(body)) { }

SAsyncStmt::SAsyncStmt(SAsyncStmt&& other) noexcept = default;

SAsyncStmt::~SAsyncStmt() = default;

SAsyncStmt& SAsyncStmt::operator=(SAsyncStmt&& other) noexcept = default;

JsonItem SAsyncStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SAsyncStmt") },
        { "body", Citron::ToJson(body) },
    };
}

SDirectiveStmt::SDirectiveStmt(std::string name, std::vector<SExpPtr> args)
    : name(std::move(name)), args(std::move(args)) { }

SDirectiveStmt::SDirectiveStmt(SDirectiveStmt&& other) noexcept = default;

SDirectiveStmt::~SDirectiveStmt() = default;

SDirectiveStmt& SDirectiveStmt::operator=(SDirectiveStmt&& other) noexcept = default;

JsonItem SDirectiveStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SDirectiveStmt") },
        { "name", Citron::ToJson(name) },
        { "args", Citron::ToJson(args) },
    };
}

SIfStmt::SIfStmt(SExpPtr cond, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody)
    : cond(std::move(cond)), body(std::move(body)), elseBody(std::move(elseBody)) { }

SIfStmt::SIfStmt(SIfStmt&& other) noexcept = default;

SIfStmt::~SIfStmt() = default;

SIfStmt& SIfStmt::operator=(SIfStmt&& other) noexcept = default;

JsonItem SIfStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIfStmt") },
        { "cond", Citron::ToJson(cond) },
        { "body", Citron::ToJson(body) },
        { "elseBody", Citron::ToJson(elseBody) },
    };
}

SIfTestStmt::SIfTestStmt(STypeExpPtr testType, std::string varName, SExpPtr exp, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody)
    : testType(std::move(testType)), varName(std::move(varName)), exp(std::move(exp)), body(std::move(body)), elseBody(std::move(elseBody)) { }

SIfTestStmt::SIfTestStmt(SIfTestStmt&& other) noexcept = default;

SIfTestStmt::~SIfTestStmt() = default;

SIfTestStmt& SIfTestStmt::operator=(SIfTestStmt&& other) noexcept = default;

JsonItem SIfTestStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SIfTestStmt") },
        { "testType", Citron::ToJson(testType) },
        { "varName", Citron::ToJson(varName) },
        { "exp", Citron::ToJson(exp) },
        { "body", Citron::ToJson(body) },
        { "elseBody", Citron::ToJson(elseBody) },
    };
}

SForStmt::SForStmt(SForStmtInitializerPtr initializer, SExpPtr cond, SExpPtr cont, SEmbeddableStmtPtr body)
    : initializer(std::move(initializer)), cond(std::move(cond)), cont(std::move(cont)), body(std::move(body)) { }

SForStmt::SForStmt(SForStmt&& other) noexcept = default;

SForStmt::~SForStmt() = default;

SForStmt& SForStmt::operator=(SForStmt&& other) noexcept = default;

JsonItem SForStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SForStmt") },
        { "initializer", Citron::ToJson(initializer) },
        { "cond", Citron::ToJson(cond) },
        { "cont", Citron::ToJson(cont) },
        { "body", Citron::ToJson(body) },
    };
}

SReturnStmt::SReturnStmt(SExpPtr value)
    : value(std::move(value)) { }

SReturnStmt::SReturnStmt(SReturnStmt&& other) noexcept = default;

SReturnStmt::~SReturnStmt() = default;

SReturnStmt& SReturnStmt::operator=(SReturnStmt&& other) noexcept = default;

JsonItem SReturnStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SReturnStmt") },
        { "value", Citron::ToJson(value) },
    };
}

SExpStmt::SExpStmt(SExpPtr exp)
    : exp(std::move(exp)) { }

SExpStmt::SExpStmt(SExpStmt&& other) noexcept = default;

SExpStmt::~SExpStmt() = default;

SExpStmt& SExpStmt::operator=(SExpStmt&& other) noexcept = default;

JsonItem SExpStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExpStmt") },
        { "exp", Citron::ToJson(exp) },
    };
}

SForeachStmt::SForeachStmt(STypeExpPtr type, std::string varName, SExpPtr enumerable, SEmbeddableStmtPtr body)
    : type(std::move(type)), varName(std::move(varName)), enumerable(std::move(enumerable)), body(std::move(body)) { }

SForeachStmt::SForeachStmt(SForeachStmt&& other) noexcept = default;

SForeachStmt::~SForeachStmt() = default;

SForeachStmt& SForeachStmt::operator=(SForeachStmt&& other) noexcept = default;

JsonItem SForeachStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SForeachStmt") },
        { "type", Citron::ToJson(type) },
        { "varName", Citron::ToJson(varName) },
        { "enumerable", Citron::ToJson(enumerable) },
        { "body", Citron::ToJson(body) },
    };
}

SYieldStmt::SYieldStmt(SExpPtr value)
    : value(std::move(value)) { }

SYieldStmt::SYieldStmt(SYieldStmt&& other) noexcept = default;

SYieldStmt::~SYieldStmt() = default;

SYieldStmt& SYieldStmt::operator=(SYieldStmt&& other) noexcept = default;

JsonItem SYieldStmt::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SYieldStmt") },
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

SClassConstructorDecl::SClassConstructorDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<SFuncParam> parameters, std::optional<std::vector<SArgument>> baseArgs, std::vector<SStmtPtr> body)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), parameters(std::move(parameters)), baseArgs(std::move(baseArgs)), body(std::move(body)) { }

SClassConstructorDecl::SClassConstructorDecl(SClassConstructorDecl&& other) noexcept = default;

SClassConstructorDecl::~SClassConstructorDecl() = default;

SClassConstructorDecl& SClassConstructorDecl::operator=(SClassConstructorDecl&& other) noexcept = default;

JsonItem SClassConstructorDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassConstructorDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
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

SStructConstructorDecl::SStructConstructorDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<SFuncParam> parameters, std::vector<SStmtPtr> body)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), parameters(std::move(parameters)), body(std::move(body)) { }

SStructConstructorDecl::SStructConstructorDecl(SStructConstructorDecl&& other) noexcept = default;

SStructConstructorDecl::~SStructConstructorDecl() = default;

SStructConstructorDecl& SStructConstructorDecl::operator=(SStructConstructorDecl&& other) noexcept = default;

JsonItem SStructConstructorDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructConstructorDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
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

SEnumElemDecl::SEnumElemDecl(std::string name, std::vector<SEnumElemMemberVarDecl> memberVars)
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

SEnumDecl::SEnumDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<SEnumElemDecl> elements)
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
