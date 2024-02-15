#include "pch.h"

#include <Syntax/Syntaxes.g.h>
#include <Infra/Json.h>

using namespace std;

namespace Citron {

JsonItem ToJson(StmtSyntax& stmt)
{
    return std::visit([](auto&& stmt) { return stmt.ToJson(); }, stmt);
}

JsonItem ToJson(ExpSyntax& exp)
{
    return std::visit([](auto&& exp) { return exp.ToJson(); }, exp);
}

JsonItem ToJson(TypeExpSyntax& typeExp)
{
    return std::visit([](auto&& typeExp) { return typeExp.ToJson(); }, typeExp);
}

JsonItem IdTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IdTypeExpSyntax") },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

struct MemberTypeExpSyntax::Impl 
{
    TypeExpSyntax parentType;
};

MemberTypeExpSyntax::MemberTypeExpSyntax(TypeExpSyntax parentType, std::string name, std::vector<TypeExpSyntax> typeArgs)
    : name(std::move(name)), typeArgs(std::move(typeArgs)), impl(new Impl{ std::move(parentType) }) { }

MemberTypeExpSyntax::MemberTypeExpSyntax(MemberTypeExpSyntax&& other) noexcept = default;

MemberTypeExpSyntax::~MemberTypeExpSyntax() = default;

MemberTypeExpSyntax& MemberTypeExpSyntax::operator=(MemberTypeExpSyntax&& other) noexcept = default;

TypeExpSyntax& MemberTypeExpSyntax::GetParentType()
{
    return impl->parentType;
}

JsonItem MemberTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("MemberTypeExpSyntax") },
        { "parentType", Citron::ToJson(impl->parentType) },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

struct NullableTypeExpSyntax::Impl 
{
    TypeExpSyntax innerType;
};

NullableTypeExpSyntax::NullableTypeExpSyntax(TypeExpSyntax innerType)
    : impl(new Impl{ std::move(innerType) }) { }

NullableTypeExpSyntax::NullableTypeExpSyntax(NullableTypeExpSyntax&& other) noexcept = default;

NullableTypeExpSyntax::~NullableTypeExpSyntax() = default;

NullableTypeExpSyntax& NullableTypeExpSyntax::operator=(NullableTypeExpSyntax&& other) noexcept = default;

TypeExpSyntax& NullableTypeExpSyntax::GetInnerType()
{
    return impl->innerType;
}

JsonItem NullableTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("NullableTypeExpSyntax") },
        { "innerType", Citron::ToJson(impl->innerType) },
    };
}

struct LocalPtrTypeExpSyntax::Impl 
{
    TypeExpSyntax innerType;
};

LocalPtrTypeExpSyntax::LocalPtrTypeExpSyntax(TypeExpSyntax innerType)
    : impl(new Impl{ std::move(innerType) }) { }

LocalPtrTypeExpSyntax::LocalPtrTypeExpSyntax(LocalPtrTypeExpSyntax&& other) noexcept = default;

LocalPtrTypeExpSyntax::~LocalPtrTypeExpSyntax() = default;

LocalPtrTypeExpSyntax& LocalPtrTypeExpSyntax::operator=(LocalPtrTypeExpSyntax&& other) noexcept = default;

TypeExpSyntax& LocalPtrTypeExpSyntax::GetInnerType()
{
    return impl->innerType;
}

JsonItem LocalPtrTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("LocalPtrTypeExpSyntax") },
        { "innerType", Citron::ToJson(impl->innerType) },
    };
}

struct BoxPtrTypeExpSyntax::Impl 
{
    TypeExpSyntax innerType;
};

BoxPtrTypeExpSyntax::BoxPtrTypeExpSyntax(TypeExpSyntax innerType)
    : impl(new Impl{ std::move(innerType) }) { }

BoxPtrTypeExpSyntax::BoxPtrTypeExpSyntax(BoxPtrTypeExpSyntax&& other) noexcept = default;

BoxPtrTypeExpSyntax::~BoxPtrTypeExpSyntax() = default;

BoxPtrTypeExpSyntax& BoxPtrTypeExpSyntax::operator=(BoxPtrTypeExpSyntax&& other) noexcept = default;

TypeExpSyntax& BoxPtrTypeExpSyntax::GetInnerType()
{
    return impl->innerType;
}

JsonItem BoxPtrTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BoxPtrTypeExpSyntax") },
        { "innerType", Citron::ToJson(impl->innerType) },
    };
}

struct LocalTypeExpSyntax::Impl 
{
    TypeExpSyntax innerType;
};

LocalTypeExpSyntax::LocalTypeExpSyntax(TypeExpSyntax innerType)
    : impl(new Impl{ std::move(innerType) }) { }

LocalTypeExpSyntax::LocalTypeExpSyntax(LocalTypeExpSyntax&& other) noexcept = default;

LocalTypeExpSyntax::~LocalTypeExpSyntax() = default;

LocalTypeExpSyntax& LocalTypeExpSyntax::operator=(LocalTypeExpSyntax&& other) noexcept = default;

TypeExpSyntax& LocalTypeExpSyntax::GetInnerType()
{
    return impl->innerType;
}

JsonItem LocalTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("LocalTypeExpSyntax") },
        { "innerType", Citron::ToJson(impl->innerType) },
    };
}

JsonItem LambdaExpParamSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("LambdaExpParamSyntax") },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
        { "hasOut", Citron::ToJson(hasOut) },
        { "hasParams", Citron::ToJson(hasParams) },
    };
}

struct ArgumentSyntax::Impl 
{
    ExpSyntax exp;
};

ArgumentSyntax::ArgumentSyntax(bool bOut, bool bParams, ExpSyntax exp)
    : bOut(bOut), bParams(bParams), impl(new Impl{ std::move(exp) }) { }

ArgumentSyntax::ArgumentSyntax(ArgumentSyntax&& other) noexcept = default;

ArgumentSyntax::~ArgumentSyntax() = default;

ArgumentSyntax& ArgumentSyntax::operator=(ArgumentSyntax&& other) noexcept = default;

ExpSyntax& ArgumentSyntax::GetExp()
{
    return impl->exp;
}

JsonItem ArgumentSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ArgumentSyntax") },
        { "bOut", Citron::ToJson(bOut) },
        { "bParams", Citron::ToJson(bParams) },
        { "exp", Citron::ToJson(impl->exp) },
    };
}

JsonItem IdentifierExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IdentifierExpSyntax") },
        { "value", Citron::ToJson(value) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

JsonItem ToJson(StringExpSyntaxElement& elem)
{
    return std::visit([](auto&& elem) { return elem.ToJson(); }, elem);
}

JsonItem TextStringExpSyntaxElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("TextStringExpSyntaxElement") },
        { "text", Citron::ToJson(text) },
    };
}

struct ExpStringExpSyntaxElement::Impl 
{
    ExpSyntax exp;
};

ExpStringExpSyntaxElement::ExpStringExpSyntaxElement(ExpSyntax exp)
    : impl(new Impl{ std::move(exp) }) { }

ExpStringExpSyntaxElement::ExpStringExpSyntaxElement(ExpStringExpSyntaxElement&& other) noexcept = default;

ExpStringExpSyntaxElement::~ExpStringExpSyntaxElement() = default;

ExpStringExpSyntaxElement& ExpStringExpSyntaxElement::operator=(ExpStringExpSyntaxElement&& other) noexcept = default;

ExpSyntax& ExpStringExpSyntaxElement::GetExp()
{
    return impl->exp;
}

JsonItem ExpStringExpSyntaxElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpStringExpSyntaxElement") },
        { "exp", Citron::ToJson(impl->exp) },
    };
}

StringExpSyntax::StringExpSyntax(std::vector<StringExpSyntaxElement> elements)
    : elements(std::move(elements)) { }

StringExpSyntax::StringExpSyntax(StringExpSyntax&& other) noexcept = default;

StringExpSyntax::~StringExpSyntax() = default;

StringExpSyntax& StringExpSyntax::operator=(StringExpSyntax&& other) noexcept = default;

JsonItem StringExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("StringExpSyntax") },
        { "elements", Citron::ToJson(elements) },
    };
}

JsonItem IntLiteralExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IntLiteralExpSyntax") },
        { "value", Citron::ToJson(value) },
    };
}

JsonItem BoolLiteralExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BoolLiteralExpSyntax") },
        { "value", Citron::ToJson(value) },
    };
}

JsonItem NullLiteralExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("NullLiteralExpSyntax") },
    };
}

struct BinaryOpExpSyntax::Impl 
{
    ExpSyntax operand0;
    ExpSyntax operand1;
};

BinaryOpExpSyntax::BinaryOpExpSyntax(BinaryOpSyntaxKind kind, ExpSyntax operand0, ExpSyntax operand1)
    : kind(kind), impl(new Impl{ std::move(operand0), std::move(operand1) }) { }

BinaryOpExpSyntax::BinaryOpExpSyntax(BinaryOpExpSyntax&& other) noexcept = default;

BinaryOpExpSyntax::~BinaryOpExpSyntax() = default;

BinaryOpExpSyntax& BinaryOpExpSyntax::operator=(BinaryOpExpSyntax&& other) noexcept = default;

ExpSyntax& BinaryOpExpSyntax::GetOperand0()
{
    return impl->operand0;
}

ExpSyntax& BinaryOpExpSyntax::GetOperand1()
{
    return impl->operand1;
}

JsonItem BinaryOpExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BinaryOpExpSyntax") },
        { "kind", Citron::ToJson(kind) },
        { "operand0", Citron::ToJson(impl->operand0) },
        { "operand1", Citron::ToJson(impl->operand1) },
    };
}

struct UnaryOpExpSyntax::Impl 
{
    ExpSyntax operand;
};

UnaryOpExpSyntax::UnaryOpExpSyntax(UnaryOpSyntaxKind kind, ExpSyntax operand)
    : kind(kind), impl(new Impl{ std::move(operand) }) { }

UnaryOpExpSyntax::UnaryOpExpSyntax(UnaryOpExpSyntax&& other) noexcept = default;

UnaryOpExpSyntax::~UnaryOpExpSyntax() = default;

UnaryOpExpSyntax& UnaryOpExpSyntax::operator=(UnaryOpExpSyntax&& other) noexcept = default;

ExpSyntax& UnaryOpExpSyntax::GetOperand()
{
    return impl->operand;
}

JsonItem UnaryOpExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("UnaryOpExpSyntax") },
        { "kind", Citron::ToJson(kind) },
        { "operand", Citron::ToJson(impl->operand) },
    };
}

struct CallExpSyntax::Impl 
{
    ExpSyntax callable;
};

CallExpSyntax::CallExpSyntax(ExpSyntax callable, std::vector<ArgumentSyntax> args)
    : args(std::move(args)), impl(new Impl{ std::move(callable) }) { }

CallExpSyntax::CallExpSyntax(CallExpSyntax&& other) noexcept = default;

CallExpSyntax::~CallExpSyntax() = default;

CallExpSyntax& CallExpSyntax::operator=(CallExpSyntax&& other) noexcept = default;

ExpSyntax& CallExpSyntax::GetCallable()
{
    return impl->callable;
}

JsonItem CallExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("CallExpSyntax") },
        { "callable", Citron::ToJson(impl->callable) },
        { "args", Citron::ToJson(args) },
    };
}

JsonItem ToJson(LambdaExpBodySyntax& body)
{
    return std::visit([](auto&& body) { return body.ToJson(); }, body);
}

StmtsLambdaExpBodySyntax::StmtsLambdaExpBodySyntax(std::vector<StmtSyntax> stmts)
    : stmts(std::move(stmts)) { }

StmtsLambdaExpBodySyntax::StmtsLambdaExpBodySyntax(StmtsLambdaExpBodySyntax&& other) noexcept = default;

StmtsLambdaExpBodySyntax::~StmtsLambdaExpBodySyntax() = default;

StmtsLambdaExpBodySyntax& StmtsLambdaExpBodySyntax::operator=(StmtsLambdaExpBodySyntax&& other) noexcept = default;

JsonItem StmtsLambdaExpBodySyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("StmtsLambdaExpBodySyntax") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

struct ExpLambdaExpBodySyntax::Impl 
{
    ExpSyntax exp;
};

ExpLambdaExpBodySyntax::ExpLambdaExpBodySyntax(ExpSyntax exp)
    : impl(new Impl{ std::move(exp) }) { }

ExpLambdaExpBodySyntax::ExpLambdaExpBodySyntax(ExpLambdaExpBodySyntax&& other) noexcept = default;

ExpLambdaExpBodySyntax::~ExpLambdaExpBodySyntax() = default;

ExpLambdaExpBodySyntax& ExpLambdaExpBodySyntax::operator=(ExpLambdaExpBodySyntax&& other) noexcept = default;

ExpSyntax& ExpLambdaExpBodySyntax::GetExp()
{
    return impl->exp;
}

JsonItem ExpLambdaExpBodySyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpLambdaExpBodySyntax") },
        { "exp", Citron::ToJson(impl->exp) },
    };
}

LambdaExpSyntax::LambdaExpSyntax(std::vector<LambdaExpParamSyntax> params, LambdaExpBodySyntax body)
    : params(std::move(params)), body(std::move(body)) { }

LambdaExpSyntax::LambdaExpSyntax(LambdaExpSyntax&& other) noexcept = default;

LambdaExpSyntax::~LambdaExpSyntax() = default;

LambdaExpSyntax& LambdaExpSyntax::operator=(LambdaExpSyntax&& other) noexcept = default;

JsonItem LambdaExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("LambdaExpSyntax") },
        { "params", Citron::ToJson(params) },
        { "body", Citron::ToJson(body) },
    };
}

struct IndexerExpSyntax::Impl 
{
    ExpSyntax obj;
    ExpSyntax index;
};

IndexerExpSyntax::IndexerExpSyntax(ExpSyntax obj, ExpSyntax index)
    : impl(new Impl{ std::move(obj), std::move(index) }) { }

IndexerExpSyntax::IndexerExpSyntax(IndexerExpSyntax&& other) noexcept = default;

IndexerExpSyntax::~IndexerExpSyntax() = default;

IndexerExpSyntax& IndexerExpSyntax::operator=(IndexerExpSyntax&& other) noexcept = default;

ExpSyntax& IndexerExpSyntax::GetObject()
{
    return impl->obj;
}

ExpSyntax& IndexerExpSyntax::GetIndex()
{
    return impl->index;
}

JsonItem IndexerExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IndexerExpSyntax") },
        { "obj", Citron::ToJson(impl->obj) },
        { "index", Citron::ToJson(impl->index) },
    };
}

struct MemberExpSyntax::Impl 
{
    ExpSyntax parent;
};

MemberExpSyntax::MemberExpSyntax(ExpSyntax parent, std::string memberName, std::vector<TypeExpSyntax> memberTypeArgs)
    : memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)), impl(new Impl{ std::move(parent) }) { }

MemberExpSyntax::MemberExpSyntax(MemberExpSyntax&& other) noexcept = default;

MemberExpSyntax::~MemberExpSyntax() = default;

MemberExpSyntax& MemberExpSyntax::operator=(MemberExpSyntax&& other) noexcept = default;

ExpSyntax& MemberExpSyntax::GetParent()
{
    return impl->parent;
}

JsonItem MemberExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("MemberExpSyntax") },
        { "parent", Citron::ToJson(impl->parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

struct IndirectMemberExpSyntax::Impl 
{
    ExpSyntax parent;
};

IndirectMemberExpSyntax::IndirectMemberExpSyntax(ExpSyntax parent, std::string memberName, std::vector<TypeExpSyntax> memberTypeArgs)
    : memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)), impl(new Impl{ std::move(parent) }) { }

IndirectMemberExpSyntax::IndirectMemberExpSyntax(IndirectMemberExpSyntax&& other) noexcept = default;

IndirectMemberExpSyntax::~IndirectMemberExpSyntax() = default;

IndirectMemberExpSyntax& IndirectMemberExpSyntax::operator=(IndirectMemberExpSyntax&& other) noexcept = default;

ExpSyntax& IndirectMemberExpSyntax::GetParent()
{
    return impl->parent;
}

JsonItem IndirectMemberExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IndirectMemberExpSyntax") },
        { "parent", Citron::ToJson(impl->parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

ListExpSyntax::ListExpSyntax(std::vector<ExpSyntax> elements)
    : elements(std::move(elements)) { }

ListExpSyntax::ListExpSyntax(ListExpSyntax&& other) noexcept = default;

ListExpSyntax::~ListExpSyntax() = default;

ListExpSyntax& ListExpSyntax::operator=(ListExpSyntax&& other) noexcept = default;

JsonItem ListExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ListExpSyntax") },
        { "elements", Citron::ToJson(elements) },
    };
}

NewExpSyntax::NewExpSyntax(TypeExpSyntax type, std::vector<ArgumentSyntax> args)
    : type(std::move(type)), args(std::move(args)) { }

NewExpSyntax::NewExpSyntax(NewExpSyntax&& other) noexcept = default;

NewExpSyntax::~NewExpSyntax() = default;

NewExpSyntax& NewExpSyntax::operator=(NewExpSyntax&& other) noexcept = default;

JsonItem NewExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("NewExpSyntax") },
        { "type", Citron::ToJson(type) },
        { "args", Citron::ToJson(args) },
    };
}

struct BoxExpSyntax::Impl 
{
    ExpSyntax innerExp;
};

BoxExpSyntax::BoxExpSyntax(ExpSyntax innerExp)
    : impl(new Impl{ std::move(innerExp) }) { }

BoxExpSyntax::BoxExpSyntax(BoxExpSyntax&& other) noexcept = default;

BoxExpSyntax::~BoxExpSyntax() = default;

BoxExpSyntax& BoxExpSyntax::operator=(BoxExpSyntax&& other) noexcept = default;

ExpSyntax& BoxExpSyntax::GetInnerExp()
{
    return impl->innerExp;
}

JsonItem BoxExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BoxExpSyntax") },
        { "innerExp", Citron::ToJson(impl->innerExp) },
    };
}

struct IsExpSyntax::Impl 
{
    ExpSyntax exp;
};

IsExpSyntax::IsExpSyntax(ExpSyntax exp, TypeExpSyntax type)
    : type(std::move(type)), impl(new Impl{ std::move(exp) }) { }

IsExpSyntax::IsExpSyntax(IsExpSyntax&& other) noexcept = default;

IsExpSyntax::~IsExpSyntax() = default;

IsExpSyntax& IsExpSyntax::operator=(IsExpSyntax&& other) noexcept = default;

ExpSyntax& IsExpSyntax::GetExp()
{
    return impl->exp;
}

JsonItem IsExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IsExpSyntax") },
        { "exp", Citron::ToJson(impl->exp) },
        { "type", Citron::ToJson(type) },
    };
}

struct AsExpSyntax::Impl 
{
    ExpSyntax exp;
};

AsExpSyntax::AsExpSyntax(ExpSyntax exp, TypeExpSyntax type)
    : type(std::move(type)), impl(new Impl{ std::move(exp) }) { }

AsExpSyntax::AsExpSyntax(AsExpSyntax&& other) noexcept = default;

AsExpSyntax::~AsExpSyntax() = default;

AsExpSyntax& AsExpSyntax::operator=(AsExpSyntax&& other) noexcept = default;

ExpSyntax& AsExpSyntax::GetExp()
{
    return impl->exp;
}

JsonItem AsExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("AsExpSyntax") },
        { "exp", Citron::ToJson(impl->exp) },
        { "type", Citron::ToJson(type) },
    };
}

JsonItem ToJson(EmbeddableStmtSyntax& embeddableStmt)
{
    return std::visit([](auto&& embeddableStmt) { return embeddableStmt.ToJson(); }, embeddableStmt);
}

struct SingleEmbeddableStmtSyntax::Impl 
{
    StmtSyntax stmt;
};

SingleEmbeddableStmtSyntax::SingleEmbeddableStmtSyntax(StmtSyntax stmt)
    : impl(new Impl{ std::move(stmt) }) { }

SingleEmbeddableStmtSyntax::SingleEmbeddableStmtSyntax(SingleEmbeddableStmtSyntax&& other) noexcept = default;

SingleEmbeddableStmtSyntax::~SingleEmbeddableStmtSyntax() = default;

SingleEmbeddableStmtSyntax& SingleEmbeddableStmtSyntax::operator=(SingleEmbeddableStmtSyntax&& other) noexcept = default;

StmtSyntax& SingleEmbeddableStmtSyntax::GetStmt()
{
    return impl->stmt;
}

JsonItem SingleEmbeddableStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SingleEmbeddableStmtSyntax") },
        { "stmt", Citron::ToJson(impl->stmt) },
    };
}

BlockEmbeddableStmtSyntax::BlockEmbeddableStmtSyntax(std::vector<StmtSyntax> stmts)
    : stmts(std::move(stmts)) { }

BlockEmbeddableStmtSyntax::BlockEmbeddableStmtSyntax(BlockEmbeddableStmtSyntax&& other) noexcept = default;

BlockEmbeddableStmtSyntax::~BlockEmbeddableStmtSyntax() = default;

BlockEmbeddableStmtSyntax& BlockEmbeddableStmtSyntax::operator=(BlockEmbeddableStmtSyntax&& other) noexcept = default;

JsonItem BlockEmbeddableStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BlockEmbeddableStmtSyntax") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

struct VarDeclSyntaxElement::Impl 
{
    std::optional<ExpSyntax> initExp;
};

VarDeclSyntaxElement::VarDeclSyntaxElement(std::string varName, std::optional<ExpSyntax> initExp)
    : varName(std::move(varName)), impl(new Impl{ std::move(initExp) }) { }

VarDeclSyntaxElement::VarDeclSyntaxElement(VarDeclSyntaxElement&& other) noexcept = default;

VarDeclSyntaxElement::~VarDeclSyntaxElement() = default;

VarDeclSyntaxElement& VarDeclSyntaxElement::operator=(VarDeclSyntaxElement&& other) noexcept = default;

std::optional<ExpSyntax>& VarDeclSyntaxElement::GetInitExp()
{
    return impl->initExp;
}

JsonItem VarDeclSyntaxElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("VarDeclSyntaxElement") },
        { "varName", Citron::ToJson(varName) },
        { "initExp", Citron::ToJson(impl->initExp) },
    };
}

JsonItem VarDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("VarDeclSyntax") },
        { "type", Citron::ToJson(type) },
        { "elements", Citron::ToJson(elements) },
    };
}

CommandStmtSyntax::CommandStmtSyntax(std::vector<StringExpSyntax> commands)
    : commands(std::move(commands)) { }

CommandStmtSyntax::CommandStmtSyntax(CommandStmtSyntax&& other) noexcept = default;

CommandStmtSyntax::~CommandStmtSyntax() = default;

CommandStmtSyntax& CommandStmtSyntax::operator=(CommandStmtSyntax&& other) noexcept = default;

JsonItem CommandStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("CommandStmtSyntax") },
        { "commands", Citron::ToJson(commands) },
    };
}

JsonItem VarDeclStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("VarDeclStmtSyntax") },
        { "varDecl", Citron::ToJson(varDecl) },
    };
}

struct IfStmtSyntax::Impl 
{
    ExpSyntax cond;
    EmbeddableStmtSyntax body;
    std::optional<EmbeddableStmtSyntax> elseBody;
};

IfStmtSyntax::IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody)
    : impl(new Impl{ std::move(cond), std::move(body), std::move(elseBody) }) { }

IfStmtSyntax::IfStmtSyntax(IfStmtSyntax&& other) noexcept = default;

IfStmtSyntax::~IfStmtSyntax() = default;

IfStmtSyntax& IfStmtSyntax::operator=(IfStmtSyntax&& other) noexcept = default;

ExpSyntax& IfStmtSyntax::GetCond()
{
    return impl->cond;
}

EmbeddableStmtSyntax& IfStmtSyntax::GetBody()
{
    return impl->body;
}

std::optional<EmbeddableStmtSyntax>& IfStmtSyntax::GetElseBody()
{
    return impl->elseBody;
}

JsonItem IfStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IfStmtSyntax") },
        { "cond", Citron::ToJson(impl->cond) },
        { "body", Citron::ToJson(impl->body) },
        { "elseBody", Citron::ToJson(impl->elseBody) },
    };
}

struct IfTestStmtSyntax::Impl 
{
    ExpSyntax exp;
    EmbeddableStmtSyntax body;
    std::optional<EmbeddableStmtSyntax> elseBody;
};

IfTestStmtSyntax::IfTestStmtSyntax(TypeExpSyntax testType, std::string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody)
    : testType(std::move(testType)), varName(std::move(varName)), impl(new Impl{ std::move(exp), std::move(body), std::move(elseBody) }) { }

IfTestStmtSyntax::IfTestStmtSyntax(IfTestStmtSyntax&& other) noexcept = default;

IfTestStmtSyntax::~IfTestStmtSyntax() = default;

IfTestStmtSyntax& IfTestStmtSyntax::operator=(IfTestStmtSyntax&& other) noexcept = default;

ExpSyntax& IfTestStmtSyntax::GetExp()
{
    return impl->exp;
}

EmbeddableStmtSyntax& IfTestStmtSyntax::GetBody()
{
    return impl->body;
}

std::optional<EmbeddableStmtSyntax>& IfTestStmtSyntax::GetElseBody()
{
    return impl->elseBody;
}

JsonItem IfTestStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IfTestStmtSyntax") },
        { "testType", Citron::ToJson(testType) },
        { "varName", Citron::ToJson(varName) },
        { "exp", Citron::ToJson(impl->exp) },
        { "body", Citron::ToJson(impl->body) },
        { "elseBody", Citron::ToJson(impl->elseBody) },
    };
}

JsonItem ToJson(ForStmtInitializerSyntax& forInit)
{
    return std::visit([](auto&& forInit) { return forInit.ToJson(); }, forInit);
}

struct ExpForStmtInitializerSyntax::Impl 
{
    ExpSyntax exp;
};

ExpForStmtInitializerSyntax::ExpForStmtInitializerSyntax(ExpSyntax exp)
    : impl(new Impl{ std::move(exp) }) { }

ExpForStmtInitializerSyntax::ExpForStmtInitializerSyntax(ExpForStmtInitializerSyntax&& other) noexcept = default;

ExpForStmtInitializerSyntax::~ExpForStmtInitializerSyntax() = default;

ExpForStmtInitializerSyntax& ExpForStmtInitializerSyntax::operator=(ExpForStmtInitializerSyntax&& other) noexcept = default;

ExpSyntax& ExpForStmtInitializerSyntax::GetExp()
{
    return impl->exp;
}

JsonItem ExpForStmtInitializerSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpForStmtInitializerSyntax") },
        { "exp", Citron::ToJson(impl->exp) },
    };
}

struct VarDeclForStmtInitializerSyntax::Impl 
{
    VarDeclSyntax varDecl;
};

VarDeclForStmtInitializerSyntax::VarDeclForStmtInitializerSyntax(VarDeclSyntax varDecl)
    : impl(new Impl{ std::move(varDecl) }) { }

VarDeclForStmtInitializerSyntax::VarDeclForStmtInitializerSyntax(VarDeclForStmtInitializerSyntax&& other) noexcept = default;

VarDeclForStmtInitializerSyntax::~VarDeclForStmtInitializerSyntax() = default;

VarDeclForStmtInitializerSyntax& VarDeclForStmtInitializerSyntax::operator=(VarDeclForStmtInitializerSyntax&& other) noexcept = default;

VarDeclSyntax& VarDeclForStmtInitializerSyntax::GetVarDecl()
{
    return impl->varDecl;
}

JsonItem VarDeclForStmtInitializerSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("VarDeclForStmtInitializerSyntax") },
        { "varDecl", Citron::ToJson(impl->varDecl) },
    };
}

struct ForStmtSyntax::Impl 
{
    std::optional<ForStmtInitializerSyntax> initializer;
    std::optional<ExpSyntax> cond;
    std::optional<ExpSyntax> cont;
    EmbeddableStmtSyntax body;
};

ForStmtSyntax::ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, std::optional<ExpSyntax> cond, std::optional<ExpSyntax> cont, EmbeddableStmtSyntax body)
    : impl(new Impl{ std::move(initializer), std::move(cond), std::move(cont), std::move(body) }) { }

ForStmtSyntax::ForStmtSyntax(ForStmtSyntax&& other) noexcept = default;

ForStmtSyntax::~ForStmtSyntax() = default;

ForStmtSyntax& ForStmtSyntax::operator=(ForStmtSyntax&& other) noexcept = default;

std::optional<ForStmtInitializerSyntax>& ForStmtSyntax::GetInitializer()
{
    return impl->initializer;
}

std::optional<ExpSyntax>& ForStmtSyntax::GetCond()
{
    return impl->cond;
}

std::optional<ExpSyntax>& ForStmtSyntax::GetCont()
{
    return impl->cont;
}

EmbeddableStmtSyntax& ForStmtSyntax::GetBody()
{
    return impl->body;
}

JsonItem ForStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ForStmtSyntax") },
        { "initializer", Citron::ToJson(impl->initializer) },
        { "cond", Citron::ToJson(impl->cond) },
        { "cont", Citron::ToJson(impl->cont) },
        { "body", Citron::ToJson(impl->body) },
    };
}

JsonItem ContinueStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ContinueStmtSyntax") },
    };
}

JsonItem BreakStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BreakStmtSyntax") },
    };
}

struct ReturnStmtSyntax::Impl 
{
    std::optional<ExpSyntax> value;
};

ReturnStmtSyntax::ReturnStmtSyntax(std::optional<ExpSyntax> value)
    : impl(new Impl{ std::move(value) }) { }

ReturnStmtSyntax::ReturnStmtSyntax(ReturnStmtSyntax&& other) noexcept = default;

ReturnStmtSyntax::~ReturnStmtSyntax() = default;

ReturnStmtSyntax& ReturnStmtSyntax::operator=(ReturnStmtSyntax&& other) noexcept = default;

std::optional<ExpSyntax>& ReturnStmtSyntax::GetValue()
{
    return impl->value;
}

JsonItem ReturnStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ReturnStmtSyntax") },
        { "value", Citron::ToJson(impl->value) },
    };
}

BlockStmtSyntax::BlockStmtSyntax(std::vector<StmtSyntax> stmts)
    : stmts(std::move(stmts)) { }

BlockStmtSyntax::BlockStmtSyntax(BlockStmtSyntax&& other) noexcept = default;

BlockStmtSyntax::~BlockStmtSyntax() = default;

BlockStmtSyntax& BlockStmtSyntax::operator=(BlockStmtSyntax&& other) noexcept = default;

JsonItem BlockStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BlockStmtSyntax") },
        { "stmts", Citron::ToJson(stmts) },
    };
}

JsonItem BlankStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BlankStmtSyntax") },
    };
}

struct ExpStmtSyntax::Impl 
{
    ExpSyntax exp;
};

ExpStmtSyntax::ExpStmtSyntax(ExpSyntax exp)
    : impl(new Impl{ std::move(exp) }) { }

ExpStmtSyntax::ExpStmtSyntax(ExpStmtSyntax&& other) noexcept = default;

ExpStmtSyntax::~ExpStmtSyntax() = default;

ExpStmtSyntax& ExpStmtSyntax::operator=(ExpStmtSyntax&& other) noexcept = default;

ExpSyntax& ExpStmtSyntax::GetExp()
{
    return impl->exp;
}

JsonItem ExpStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpStmtSyntax") },
        { "exp", Citron::ToJson(impl->exp) },
    };
}

TaskStmtSyntax::TaskStmtSyntax(std::vector<StmtSyntax> body)
    : body(std::move(body)) { }

TaskStmtSyntax::TaskStmtSyntax(TaskStmtSyntax&& other) noexcept = default;

TaskStmtSyntax::~TaskStmtSyntax() = default;

TaskStmtSyntax& TaskStmtSyntax::operator=(TaskStmtSyntax&& other) noexcept = default;

JsonItem TaskStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("TaskStmtSyntax") },
        { "body", Citron::ToJson(body) },
    };
}

AwaitStmtSyntax::AwaitStmtSyntax(std::vector<StmtSyntax> body)
    : body(std::move(body)) { }

AwaitStmtSyntax::AwaitStmtSyntax(AwaitStmtSyntax&& other) noexcept = default;

AwaitStmtSyntax::~AwaitStmtSyntax() = default;

AwaitStmtSyntax& AwaitStmtSyntax::operator=(AwaitStmtSyntax&& other) noexcept = default;

JsonItem AwaitStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("AwaitStmtSyntax") },
        { "body", Citron::ToJson(body) },
    };
}

AsyncStmtSyntax::AsyncStmtSyntax(std::vector<StmtSyntax> body)
    : body(std::move(body)) { }

AsyncStmtSyntax::AsyncStmtSyntax(AsyncStmtSyntax&& other) noexcept = default;

AsyncStmtSyntax::~AsyncStmtSyntax() = default;

AsyncStmtSyntax& AsyncStmtSyntax::operator=(AsyncStmtSyntax&& other) noexcept = default;

JsonItem AsyncStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("AsyncStmtSyntax") },
        { "body", Citron::ToJson(body) },
    };
}

struct ForeachStmtSyntax::Impl 
{
    ExpSyntax enumerable;
    EmbeddableStmtSyntax body;
};

ForeachStmtSyntax::ForeachStmtSyntax(TypeExpSyntax type, std::string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body)
    : type(std::move(type)), varName(std::move(varName)), impl(new Impl{ std::move(enumerable), std::move(body) }) { }

ForeachStmtSyntax::ForeachStmtSyntax(ForeachStmtSyntax&& other) noexcept = default;

ForeachStmtSyntax::~ForeachStmtSyntax() = default;

ForeachStmtSyntax& ForeachStmtSyntax::operator=(ForeachStmtSyntax&& other) noexcept = default;

ExpSyntax& ForeachStmtSyntax::GetEnumerable()
{
    return impl->enumerable;
}

EmbeddableStmtSyntax& ForeachStmtSyntax::GetBody()
{
    return impl->body;
}

JsonItem ForeachStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ForeachStmtSyntax") },
        { "type", Citron::ToJson(type) },
        { "varName", Citron::ToJson(varName) },
        { "enumerable", Citron::ToJson(impl->enumerable) },
        { "body", Citron::ToJson(impl->body) },
    };
}

struct YieldStmtSyntax::Impl 
{
    ExpSyntax value;
};

YieldStmtSyntax::YieldStmtSyntax(ExpSyntax value)
    : impl(new Impl{ std::move(value) }) { }

YieldStmtSyntax::YieldStmtSyntax(YieldStmtSyntax&& other) noexcept = default;

YieldStmtSyntax::~YieldStmtSyntax() = default;

YieldStmtSyntax& YieldStmtSyntax::operator=(YieldStmtSyntax&& other) noexcept = default;

ExpSyntax& YieldStmtSyntax::GetValue()
{
    return impl->value;
}

JsonItem YieldStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("YieldStmtSyntax") },
        { "value", Citron::ToJson(impl->value) },
    };
}

DirectiveStmtSyntax::DirectiveStmtSyntax(std::string name, std::vector<ExpSyntax> args)
    : name(std::move(name)), args(std::move(args)) { }

DirectiveStmtSyntax::DirectiveStmtSyntax(DirectiveStmtSyntax&& other) noexcept = default;

DirectiveStmtSyntax::~DirectiveStmtSyntax() = default;

DirectiveStmtSyntax& DirectiveStmtSyntax::operator=(DirectiveStmtSyntax&& other) noexcept = default;

JsonItem DirectiveStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("DirectiveStmtSyntax") },
        { "name", Citron::ToJson(name) },
        { "args", Citron::ToJson(args) },
    };
}

JsonItem TypeParamSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("TypeParamSyntax") },
        { "name", Citron::ToJson(name) },
    };
}

FuncParamSyntax::FuncParamSyntax(bool hasOut, bool hasParams, TypeExpSyntax type, std::string name)
    : hasOut(hasOut), hasParams(hasParams), type(std::move(type)), name(std::move(name)) { }

FuncParamSyntax::FuncParamSyntax(FuncParamSyntax&& other) noexcept = default;

FuncParamSyntax::~FuncParamSyntax() = default;

FuncParamSyntax& FuncParamSyntax::operator=(FuncParamSyntax&& other) noexcept = default;

JsonItem FuncParamSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("FuncParamSyntax") },
        { "hasOut", Citron::ToJson(hasOut) },
        { "hasParams", Citron::ToJson(hasParams) },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

JsonItem GlobalFuncDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("GlobalFuncDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "bSequence", Citron::ToJson(bSequence) },
        { "retType", Citron::ToJson(retType) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

JsonItem ClassMemberFuncDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ClassMemberFuncDeclSyntax") },
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

JsonItem ClassConstructorDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ClassConstructorDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "parameters", Citron::ToJson(parameters) },
        { "baseArgs", Citron::ToJson(baseArgs) },
        { "body", Citron::ToJson(body) },
    };
}

JsonItem ClassMemberVarDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ClassMemberVarDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "varType", Citron::ToJson(varType) },
        { "varNames", Citron::ToJson(varNames) },
    };
}

JsonItem ToJson(ClassMemberDeclSyntax& decl)
{
    return std::visit([](auto&& decl) { return decl.ToJson(); }, decl);
}

JsonItem ClassDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ClassDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "baseTypes", Citron::ToJson(baseTypes) },
        { "memberDecls", Citron::ToJson(memberDecls) },
    };
}

JsonItem StructMemberFuncDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("StructMemberFuncDeclSyntax") },
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

JsonItem StructConstructorDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("StructConstructorDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

JsonItem StructMemberVarDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("StructMemberVarDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "varType", Citron::ToJson(varType) },
        { "varNames", Citron::ToJson(varNames) },
    };
}

JsonItem ToJson(StructMemberDeclSyntax& decl)
{
    return std::visit([](auto&& decl) { return decl.ToJson(); }, decl);
}

JsonItem StructDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("StructDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "baseTypes", Citron::ToJson(baseTypes) },
        { "memberDecls", Citron::ToJson(memberDecls) },
    };
}

JsonItem EnumElemMemberVarDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("EnumElemMemberVarDeclSyntax") },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

JsonItem EnumElemDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("EnumElemDeclSyntax") },
        { "name", Citron::ToJson(name) },
        { "memberVars", Citron::ToJson(memberVars) },
    };
}

JsonItem EnumDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("EnumDeclSyntax") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "elements", Citron::ToJson(elements) },
    };
}

JsonItem ToJson(NamespaceDeclSyntaxElement& elem)
{
    return std::visit([](auto&& elem) { return elem.ToJson(); }, elem);
}

NamespaceDeclSyntax::NamespaceDeclSyntax(std::vector<std::string> names, std::vector<NamespaceDeclSyntaxElement> elements)
    : names(std::move(names)), elements(std::move(elements)) { }

NamespaceDeclSyntax::NamespaceDeclSyntax(NamespaceDeclSyntax&& other) noexcept = default;

NamespaceDeclSyntax::~NamespaceDeclSyntax() = default;

NamespaceDeclSyntax& NamespaceDeclSyntax::operator=(NamespaceDeclSyntax&& other) noexcept = default;

JsonItem NamespaceDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("NamespaceDeclSyntax") },
        { "names", Citron::ToJson(names) },
        { "elements", Citron::ToJson(elements) },
    };
}

JsonItem ToJson(ScriptSyntaxElement& elem)
{
    return std::visit([](auto&& elem) { return elem.ToJson(); }, elem);
}

ScriptSyntax::ScriptSyntax(std::vector<ScriptSyntaxElement> elements)
    : elements(std::move(elements)) { }

ScriptSyntax::ScriptSyntax(ScriptSyntax&& other) noexcept = default;

ScriptSyntax::~ScriptSyntax() = default;

ScriptSyntax& ScriptSyntax::operator=(ScriptSyntax&& other) noexcept = default;

JsonItem ScriptSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ScriptSyntax") },
        { "elements", Citron::ToJson(elements) },
    };
}

}
