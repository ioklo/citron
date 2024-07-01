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

JsonItem ToJson(StmtSyntax& stmt)
{
    return std::visit(ToJsonVisitor(), stmt);
}

JsonItem ToJson(ExpSyntax& exp)
{
    return std::visit(ToJsonVisitor(), exp);
}

JsonItem ToJson(TypeExpSyntax& typeExp)
{
    return std::visit(ToJsonVisitor(), typeExp);
}

IdTypeExpSyntax::IdTypeExpSyntax(std::string name, std::vector<TypeExpSyntax> typeArgs)
    : name(std::move(name)), typeArgs(std::move(typeArgs)) { }

IdTypeExpSyntax::IdTypeExpSyntax(IdTypeExpSyntax&& other) noexcept = default;

IdTypeExpSyntax::~IdTypeExpSyntax() = default;

IdTypeExpSyntax& IdTypeExpSyntax::operator=(IdTypeExpSyntax&& other) noexcept = default;

JsonItem IdTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IdTypeExpSyntax") },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

MemberTypeExpSyntax::MemberTypeExpSyntax(TypeExpSyntax parentType, std::string name, std::vector<TypeExpSyntax> typeArgs)
    : parentType(std::move(parentType)), name(std::move(name)), typeArgs(std::move(typeArgs)) { }

MemberTypeExpSyntax::MemberTypeExpSyntax(MemberTypeExpSyntax&& other) noexcept = default;

MemberTypeExpSyntax::~MemberTypeExpSyntax() = default;

MemberTypeExpSyntax& MemberTypeExpSyntax::operator=(MemberTypeExpSyntax&& other) noexcept = default;

JsonItem MemberTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("MemberTypeExpSyntax") },
        { "parentType", Citron::ToJson(parentType) },
        { "name", Citron::ToJson(name) },
        { "typeArgs", Citron::ToJson(typeArgs) },
    };
}

NullableTypeExpSyntax::NullableTypeExpSyntax(TypeExpSyntax innerType)
    : innerType(std::move(innerType)) { }

NullableTypeExpSyntax::NullableTypeExpSyntax(NullableTypeExpSyntax&& other) noexcept = default;

NullableTypeExpSyntax::~NullableTypeExpSyntax() = default;

NullableTypeExpSyntax& NullableTypeExpSyntax::operator=(NullableTypeExpSyntax&& other) noexcept = default;

JsonItem NullableTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("NullableTypeExpSyntax") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

LocalPtrTypeExpSyntax::LocalPtrTypeExpSyntax(TypeExpSyntax innerType)
    : innerType(std::move(innerType)) { }

LocalPtrTypeExpSyntax::LocalPtrTypeExpSyntax(LocalPtrTypeExpSyntax&& other) noexcept = default;

LocalPtrTypeExpSyntax::~LocalPtrTypeExpSyntax() = default;

LocalPtrTypeExpSyntax& LocalPtrTypeExpSyntax::operator=(LocalPtrTypeExpSyntax&& other) noexcept = default;

JsonItem LocalPtrTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("LocalPtrTypeExpSyntax") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

BoxPtrTypeExpSyntax::BoxPtrTypeExpSyntax(TypeExpSyntax innerType)
    : innerType(std::move(innerType)) { }

BoxPtrTypeExpSyntax::BoxPtrTypeExpSyntax(BoxPtrTypeExpSyntax&& other) noexcept = default;

BoxPtrTypeExpSyntax::~BoxPtrTypeExpSyntax() = default;

BoxPtrTypeExpSyntax& BoxPtrTypeExpSyntax::operator=(BoxPtrTypeExpSyntax&& other) noexcept = default;

JsonItem BoxPtrTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BoxPtrTypeExpSyntax") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

LocalTypeExpSyntax::LocalTypeExpSyntax(TypeExpSyntax innerType)
    : innerType(std::move(innerType)) { }

LocalTypeExpSyntax::LocalTypeExpSyntax(LocalTypeExpSyntax&& other) noexcept = default;

LocalTypeExpSyntax::~LocalTypeExpSyntax() = default;

LocalTypeExpSyntax& LocalTypeExpSyntax::operator=(LocalTypeExpSyntax&& other) noexcept = default;

JsonItem LocalTypeExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("LocalTypeExpSyntax") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

LambdaExpParamSyntax::LambdaExpParamSyntax(std::optional<TypeExpSyntax> type, std::string name, bool hasOut, bool hasParams)
    : type(std::move(type)), name(std::move(name)), hasOut(std::move(hasOut)), hasParams(std::move(hasParams)) { }

LambdaExpParamSyntax::LambdaExpParamSyntax(LambdaExpParamSyntax&& other) noexcept = default;

LambdaExpParamSyntax::~LambdaExpParamSyntax() = default;

LambdaExpParamSyntax& LambdaExpParamSyntax::operator=(LambdaExpParamSyntax&& other) noexcept = default;

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

IdentifierExpSyntax::IdentifierExpSyntax(std::string value, std::vector<TypeExpSyntax> typeArgs)
    : value(std::move(value)), typeArgs(std::move(typeArgs)) { }

IdentifierExpSyntax::IdentifierExpSyntax(IdentifierExpSyntax&& other) noexcept = default;

IdentifierExpSyntax::~IdentifierExpSyntax() = default;

IdentifierExpSyntax& IdentifierExpSyntax::operator=(IdentifierExpSyntax&& other) noexcept = default;

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
    return std::visit(ToJsonVisitor(), elem);
}

TextStringExpSyntaxElement::TextStringExpSyntaxElement(std::string text)
    : text(std::move(text)) { }

TextStringExpSyntaxElement::TextStringExpSyntaxElement(TextStringExpSyntaxElement&& other) noexcept = default;

TextStringExpSyntaxElement::~TextStringExpSyntaxElement() = default;

TextStringExpSyntaxElement& TextStringExpSyntaxElement::operator=(TextStringExpSyntaxElement&& other) noexcept = default;

JsonItem TextStringExpSyntaxElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("TextStringExpSyntaxElement") },
        { "text", Citron::ToJson(text) },
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

IntLiteralExpSyntax::IntLiteralExpSyntax(int value)
    : value(std::move(value)) { }

IntLiteralExpSyntax::IntLiteralExpSyntax(IntLiteralExpSyntax&& other) noexcept = default;

IntLiteralExpSyntax::~IntLiteralExpSyntax() = default;

IntLiteralExpSyntax& IntLiteralExpSyntax::operator=(IntLiteralExpSyntax&& other) noexcept = default;

JsonItem IntLiteralExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IntLiteralExpSyntax") },
        { "value", Citron::ToJson(value) },
    };
}

BoolLiteralExpSyntax::BoolLiteralExpSyntax(bool value)
    : value(std::move(value)) { }

BoolLiteralExpSyntax::BoolLiteralExpSyntax(BoolLiteralExpSyntax&& other) noexcept = default;

BoolLiteralExpSyntax::~BoolLiteralExpSyntax() = default;

BoolLiteralExpSyntax& BoolLiteralExpSyntax::operator=(BoolLiteralExpSyntax&& other) noexcept = default;

JsonItem BoolLiteralExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BoolLiteralExpSyntax") },
        { "value", Citron::ToJson(value) },
    };
}

NullLiteralExpSyntax::NullLiteralExpSyntax()
{ }
NullLiteralExpSyntax::NullLiteralExpSyntax(NullLiteralExpSyntax&& other) noexcept = default;

NullLiteralExpSyntax::~NullLiteralExpSyntax() = default;

NullLiteralExpSyntax& NullLiteralExpSyntax::operator=(NullLiteralExpSyntax&& other) noexcept = default;

JsonItem NullLiteralExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("NullLiteralExpSyntax") },
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

BinaryOpExpSyntax::BinaryOpExpSyntax(BinaryOpSyntaxKind kind, ExpSyntax operand0, ExpSyntax operand1)
    : kind(std::move(kind)), operand0(std::move(operand0)), operand1(std::move(operand1)) { }

BinaryOpExpSyntax::BinaryOpExpSyntax(BinaryOpExpSyntax&& other) noexcept = default;

BinaryOpExpSyntax::~BinaryOpExpSyntax() = default;

BinaryOpExpSyntax& BinaryOpExpSyntax::operator=(BinaryOpExpSyntax&& other) noexcept = default;

JsonItem BinaryOpExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BinaryOpExpSyntax") },
        { "kind", Citron::ToJson(kind) },
        { "operand0", Citron::ToJson(operand0) },
        { "operand1", Citron::ToJson(operand1) },
    };
}

UnaryOpExpSyntax::UnaryOpExpSyntax(UnaryOpSyntaxKind kind, ExpSyntax operand)
    : kind(std::move(kind)), operand(std::move(operand)) { }

UnaryOpExpSyntax::UnaryOpExpSyntax(UnaryOpExpSyntax&& other) noexcept = default;

UnaryOpExpSyntax::~UnaryOpExpSyntax() = default;

UnaryOpExpSyntax& UnaryOpExpSyntax::operator=(UnaryOpExpSyntax&& other) noexcept = default;

JsonItem UnaryOpExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("UnaryOpExpSyntax") },
        { "kind", Citron::ToJson(kind) },
        { "operand", Citron::ToJson(operand) },
    };
}

CallExpSyntax::CallExpSyntax(ExpSyntax callable, std::vector<ArgumentSyntax> args)
    : callable(std::move(callable)), args(std::move(args)) { }

CallExpSyntax::CallExpSyntax(CallExpSyntax&& other) noexcept = default;

CallExpSyntax::~CallExpSyntax() = default;

CallExpSyntax& CallExpSyntax::operator=(CallExpSyntax&& other) noexcept = default;

JsonItem CallExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("CallExpSyntax") },
        { "callable", Citron::ToJson(callable) },
        { "args", Citron::ToJson(args) },
    };
}

JsonItem ToJson(LambdaExpBodySyntax& body)
{
    return std::visit(ToJsonVisitor(), body);
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

ExpLambdaExpBodySyntax::ExpLambdaExpBodySyntax(ExpSyntax exp)
    : exp(std::move(exp)) { }

ExpLambdaExpBodySyntax::ExpLambdaExpBodySyntax(ExpLambdaExpBodySyntax&& other) noexcept = default;

ExpLambdaExpBodySyntax::~ExpLambdaExpBodySyntax() = default;

ExpLambdaExpBodySyntax& ExpLambdaExpBodySyntax::operator=(ExpLambdaExpBodySyntax&& other) noexcept = default;

JsonItem ExpLambdaExpBodySyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpLambdaExpBodySyntax") },
        { "exp", Citron::ToJson(exp) },
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

IndexerExpSyntax::IndexerExpSyntax(ExpSyntax obj, ExpSyntax index)
    : obj(std::move(obj)), index(std::move(index)) { }

IndexerExpSyntax::IndexerExpSyntax(IndexerExpSyntax&& other) noexcept = default;

IndexerExpSyntax::~IndexerExpSyntax() = default;

IndexerExpSyntax& IndexerExpSyntax::operator=(IndexerExpSyntax&& other) noexcept = default;

JsonItem IndexerExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IndexerExpSyntax") },
        { "obj", Citron::ToJson(obj) },
        { "index", Citron::ToJson(index) },
    };
}

MemberExpSyntax::MemberExpSyntax(ExpSyntax parent, std::string memberName, std::vector<TypeExpSyntax> memberTypeArgs)
    : parent(std::move(parent)), memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)) { }

MemberExpSyntax::MemberExpSyntax(MemberExpSyntax&& other) noexcept = default;

MemberExpSyntax::~MemberExpSyntax() = default;

MemberExpSyntax& MemberExpSyntax::operator=(MemberExpSyntax&& other) noexcept = default;

JsonItem MemberExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("MemberExpSyntax") },
        { "parent", Citron::ToJson(parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

IndirectMemberExpSyntax::IndirectMemberExpSyntax(ExpSyntax parent, std::string memberName, std::vector<TypeExpSyntax> memberTypeArgs)
    : parent(std::move(parent)), memberName(std::move(memberName)), memberTypeArgs(std::move(memberTypeArgs)) { }

IndirectMemberExpSyntax::IndirectMemberExpSyntax(IndirectMemberExpSyntax&& other) noexcept = default;

IndirectMemberExpSyntax::~IndirectMemberExpSyntax() = default;

IndirectMemberExpSyntax& IndirectMemberExpSyntax::operator=(IndirectMemberExpSyntax&& other) noexcept = default;

JsonItem IndirectMemberExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IndirectMemberExpSyntax") },
        { "parent", Citron::ToJson(parent) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

BoxExpSyntax::BoxExpSyntax(ExpSyntax innerExp)
    : innerExp(std::move(innerExp)) { }

BoxExpSyntax::BoxExpSyntax(BoxExpSyntax&& other) noexcept = default;

BoxExpSyntax::~BoxExpSyntax() = default;

BoxExpSyntax& BoxExpSyntax::operator=(BoxExpSyntax&& other) noexcept = default;

JsonItem BoxExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BoxExpSyntax") },
        { "innerExp", Citron::ToJson(innerExp) },
    };
}

IsExpSyntax::IsExpSyntax(ExpSyntax exp, TypeExpSyntax type)
    : exp(std::move(exp)), type(std::move(type)) { }

IsExpSyntax::IsExpSyntax(IsExpSyntax&& other) noexcept = default;

IsExpSyntax::~IsExpSyntax() = default;

IsExpSyntax& IsExpSyntax::operator=(IsExpSyntax&& other) noexcept = default;

JsonItem IsExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IsExpSyntax") },
        { "exp", Citron::ToJson(exp) },
        { "type", Citron::ToJson(type) },
    };
}

AsExpSyntax::AsExpSyntax(ExpSyntax exp, TypeExpSyntax type)
    : exp(std::move(exp)), type(std::move(type)) { }

AsExpSyntax::AsExpSyntax(AsExpSyntax&& other) noexcept = default;

AsExpSyntax::~AsExpSyntax() = default;

AsExpSyntax& AsExpSyntax::operator=(AsExpSyntax&& other) noexcept = default;

JsonItem AsExpSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("AsExpSyntax") },
        { "exp", Citron::ToJson(exp) },
        { "type", Citron::ToJson(type) },
    };
}

ArgumentSyntax::ArgumentSyntax(bool bOut, bool bParams, ExpSyntax exp)
    : bOut(std::move(bOut)), bParams(std::move(bParams)), exp(std::move(exp)) { }

ArgumentSyntax::ArgumentSyntax(ArgumentSyntax&& other) noexcept = default;

ArgumentSyntax::~ArgumentSyntax() = default;

ArgumentSyntax& ArgumentSyntax::operator=(ArgumentSyntax&& other) noexcept = default;

JsonItem ArgumentSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ArgumentSyntax") },
        { "bOut", Citron::ToJson(bOut) },
        { "bParams", Citron::ToJson(bParams) },
        { "exp", Citron::ToJson(exp) },
    };
}

ExpStringExpSyntaxElement::ExpStringExpSyntaxElement(ExpSyntax exp)
    : exp(std::move(exp)) { }

ExpStringExpSyntaxElement::ExpStringExpSyntaxElement(ExpStringExpSyntaxElement&& other) noexcept = default;

ExpStringExpSyntaxElement::~ExpStringExpSyntaxElement() = default;

ExpStringExpSyntaxElement& ExpStringExpSyntaxElement::operator=(ExpStringExpSyntaxElement&& other) noexcept = default;

JsonItem ExpStringExpSyntaxElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpStringExpSyntaxElement") },
        { "exp", Citron::ToJson(exp) },
    };
}

JsonItem ToJson(EmbeddableStmtSyntax& embeddableStmt)
{
    return std::visit(ToJsonVisitor(), embeddableStmt);
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

VarDeclSyntaxElement::VarDeclSyntaxElement(std::string varName, std::optional<ExpSyntax> initExp)
    : varName(std::move(varName)), initExp(std::move(initExp)) { }

VarDeclSyntaxElement::VarDeclSyntaxElement(VarDeclSyntaxElement&& other) noexcept = default;

VarDeclSyntaxElement::~VarDeclSyntaxElement() = default;

VarDeclSyntaxElement& VarDeclSyntaxElement::operator=(VarDeclSyntaxElement&& other) noexcept = default;

JsonItem VarDeclSyntaxElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("VarDeclSyntaxElement") },
        { "varName", Citron::ToJson(varName) },
        { "initExp", Citron::ToJson(initExp) },
    };
}

VarDeclSyntax::VarDeclSyntax(TypeExpSyntax type, std::vector<VarDeclSyntaxElement> elements)
    : type(std::move(type)), elements(std::move(elements)) { }

VarDeclSyntax::VarDeclSyntax(VarDeclSyntax&& other) noexcept = default;

VarDeclSyntax::~VarDeclSyntax() = default;

VarDeclSyntax& VarDeclSyntax::operator=(VarDeclSyntax&& other) noexcept = default;

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

VarDeclStmtSyntax::VarDeclStmtSyntax(VarDeclSyntax varDecl)
    : varDecl(std::move(varDecl)) { }

VarDeclStmtSyntax::VarDeclStmtSyntax(VarDeclStmtSyntax&& other) noexcept = default;

VarDeclStmtSyntax::~VarDeclStmtSyntax() = default;

VarDeclStmtSyntax& VarDeclStmtSyntax::operator=(VarDeclStmtSyntax&& other) noexcept = default;

JsonItem VarDeclStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("VarDeclStmtSyntax") },
        { "varDecl", Citron::ToJson(varDecl) },
    };
}

ContinueStmtSyntax::ContinueStmtSyntax()
{ }
ContinueStmtSyntax::ContinueStmtSyntax(ContinueStmtSyntax&& other) noexcept = default;

ContinueStmtSyntax::~ContinueStmtSyntax() = default;

ContinueStmtSyntax& ContinueStmtSyntax::operator=(ContinueStmtSyntax&& other) noexcept = default;

JsonItem ContinueStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ContinueStmtSyntax") },
    };
}

BreakStmtSyntax::BreakStmtSyntax()
{ }
BreakStmtSyntax::BreakStmtSyntax(BreakStmtSyntax&& other) noexcept = default;

BreakStmtSyntax::~BreakStmtSyntax() = default;

BreakStmtSyntax& BreakStmtSyntax::operator=(BreakStmtSyntax&& other) noexcept = default;

JsonItem BreakStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BreakStmtSyntax") },
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

BlankStmtSyntax::BlankStmtSyntax()
{ }
BlankStmtSyntax::BlankStmtSyntax(BlankStmtSyntax&& other) noexcept = default;

BlankStmtSyntax::~BlankStmtSyntax() = default;

BlankStmtSyntax& BlankStmtSyntax::operator=(BlankStmtSyntax&& other) noexcept = default;

JsonItem BlankStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("BlankStmtSyntax") },
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

SingleEmbeddableStmtSyntax::SingleEmbeddableStmtSyntax(StmtSyntax stmt)
    : stmt(std::move(stmt)) { }

SingleEmbeddableStmtSyntax::SingleEmbeddableStmtSyntax(SingleEmbeddableStmtSyntax&& other) noexcept = default;

SingleEmbeddableStmtSyntax::~SingleEmbeddableStmtSyntax() = default;

SingleEmbeddableStmtSyntax& SingleEmbeddableStmtSyntax::operator=(SingleEmbeddableStmtSyntax&& other) noexcept = default;

JsonItem SingleEmbeddableStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SingleEmbeddableStmtSyntax") },
        { "stmt", Citron::ToJson(stmt) },
    };
}

IfStmtSyntax::IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody)
    : cond(std::move(cond)), body(std::move(body)), elseBody(std::move(elseBody)) { }

IfStmtSyntax::IfStmtSyntax(IfStmtSyntax&& other) noexcept = default;

IfStmtSyntax::~IfStmtSyntax() = default;

IfStmtSyntax& IfStmtSyntax::operator=(IfStmtSyntax&& other) noexcept = default;

JsonItem IfStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IfStmtSyntax") },
        { "cond", Citron::ToJson(cond) },
        { "body", Citron::ToJson(body) },
        { "elseBody", Citron::ToJson(elseBody) },
    };
}

IfTestStmtSyntax::IfTestStmtSyntax(TypeExpSyntax testType, std::string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody)
    : testType(std::move(testType)), varName(std::move(varName)), exp(std::move(exp)), body(std::move(body)), elseBody(std::move(elseBody)) { }

IfTestStmtSyntax::IfTestStmtSyntax(IfTestStmtSyntax&& other) noexcept = default;

IfTestStmtSyntax::~IfTestStmtSyntax() = default;

IfTestStmtSyntax& IfTestStmtSyntax::operator=(IfTestStmtSyntax&& other) noexcept = default;

JsonItem IfTestStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("IfTestStmtSyntax") },
        { "testType", Citron::ToJson(testType) },
        { "varName", Citron::ToJson(varName) },
        { "exp", Citron::ToJson(exp) },
        { "body", Citron::ToJson(body) },
        { "elseBody", Citron::ToJson(elseBody) },
    };
}

JsonItem ToJson(ForStmtInitializerSyntax& forInit)
{
    return std::visit(ToJsonVisitor(), forInit);
}

ExpForStmtInitializerSyntax::ExpForStmtInitializerSyntax(ExpSyntax exp)
    : exp(std::move(exp)) { }

ExpForStmtInitializerSyntax::ExpForStmtInitializerSyntax(ExpForStmtInitializerSyntax&& other) noexcept = default;

ExpForStmtInitializerSyntax::~ExpForStmtInitializerSyntax() = default;

ExpForStmtInitializerSyntax& ExpForStmtInitializerSyntax::operator=(ExpForStmtInitializerSyntax&& other) noexcept = default;

JsonItem ExpForStmtInitializerSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpForStmtInitializerSyntax") },
        { "exp", Citron::ToJson(exp) },
    };
}

VarDeclForStmtInitializerSyntax::VarDeclForStmtInitializerSyntax(VarDeclSyntax varDecl)
    : varDecl(std::move(varDecl)) { }

VarDeclForStmtInitializerSyntax::VarDeclForStmtInitializerSyntax(VarDeclForStmtInitializerSyntax&& other) noexcept = default;

VarDeclForStmtInitializerSyntax::~VarDeclForStmtInitializerSyntax() = default;

VarDeclForStmtInitializerSyntax& VarDeclForStmtInitializerSyntax::operator=(VarDeclForStmtInitializerSyntax&& other) noexcept = default;

JsonItem VarDeclForStmtInitializerSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("VarDeclForStmtInitializerSyntax") },
        { "varDecl", Citron::ToJson(varDecl) },
    };
}

ForStmtSyntax::ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, std::optional<ExpSyntax> cond, std::optional<ExpSyntax> cont, EmbeddableStmtSyntax body)
    : initializer(std::move(initializer)), cond(std::move(cond)), cont(std::move(cont)), body(std::move(body)) { }

ForStmtSyntax::ForStmtSyntax(ForStmtSyntax&& other) noexcept = default;

ForStmtSyntax::~ForStmtSyntax() = default;

ForStmtSyntax& ForStmtSyntax::operator=(ForStmtSyntax&& other) noexcept = default;

JsonItem ForStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ForStmtSyntax") },
        { "initializer", Citron::ToJson(initializer) },
        { "cond", Citron::ToJson(cond) },
        { "cont", Citron::ToJson(cont) },
        { "body", Citron::ToJson(body) },
    };
}

ReturnStmtSyntax::ReturnStmtSyntax(std::optional<ExpSyntax> value)
    : value(std::move(value)) { }

ReturnStmtSyntax::ReturnStmtSyntax(ReturnStmtSyntax&& other) noexcept = default;

ReturnStmtSyntax::~ReturnStmtSyntax() = default;

ReturnStmtSyntax& ReturnStmtSyntax::operator=(ReturnStmtSyntax&& other) noexcept = default;

JsonItem ReturnStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ReturnStmtSyntax") },
        { "value", Citron::ToJson(value) },
    };
}

ExpStmtSyntax::ExpStmtSyntax(ExpSyntax exp)
    : exp(std::move(exp)) { }

ExpStmtSyntax::ExpStmtSyntax(ExpStmtSyntax&& other) noexcept = default;

ExpStmtSyntax::~ExpStmtSyntax() = default;

ExpStmtSyntax& ExpStmtSyntax::operator=(ExpStmtSyntax&& other) noexcept = default;

JsonItem ExpStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ExpStmtSyntax") },
        { "exp", Citron::ToJson(exp) },
    };
}

ForeachStmtSyntax::ForeachStmtSyntax(TypeExpSyntax type, std::string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body)
    : type(std::move(type)), varName(std::move(varName)), enumerable(std::move(enumerable)), body(std::move(body)) { }

ForeachStmtSyntax::ForeachStmtSyntax(ForeachStmtSyntax&& other) noexcept = default;

ForeachStmtSyntax::~ForeachStmtSyntax() = default;

ForeachStmtSyntax& ForeachStmtSyntax::operator=(ForeachStmtSyntax&& other) noexcept = default;

JsonItem ForeachStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("ForeachStmtSyntax") },
        { "type", Citron::ToJson(type) },
        { "varName", Citron::ToJson(varName) },
        { "enumerable", Citron::ToJson(enumerable) },
        { "body", Citron::ToJson(body) },
    };
}

YieldStmtSyntax::YieldStmtSyntax(ExpSyntax value)
    : value(std::move(value)) { }

YieldStmtSyntax::YieldStmtSyntax(YieldStmtSyntax&& other) noexcept = default;

YieldStmtSyntax::~YieldStmtSyntax() = default;

YieldStmtSyntax& YieldStmtSyntax::operator=(YieldStmtSyntax&& other) noexcept = default;

JsonItem YieldStmtSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("YieldStmtSyntax") },
        { "value", Citron::ToJson(value) },
    };
}

TypeParamSyntax::TypeParamSyntax(std::string name)
    : name(std::move(name)) { }

TypeParamSyntax::TypeParamSyntax(TypeParamSyntax&& other) noexcept = default;

TypeParamSyntax::~TypeParamSyntax() = default;

TypeParamSyntax& TypeParamSyntax::operator=(TypeParamSyntax&& other) noexcept = default;

JsonItem TypeParamSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("TypeParamSyntax") },
        { "name", Citron::ToJson(name) },
    };
}

FuncParamSyntax::FuncParamSyntax(bool hasOut, bool hasParams, TypeExpSyntax type, std::string name)
    : hasOut(std::move(hasOut)), hasParams(std::move(hasParams)), type(std::move(type)), name(std::move(name)) { }

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

GlobalFuncDeclSyntax::GlobalFuncDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, bool bSequence, TypeExpSyntax retType, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body)
    : accessModifier(std::move(accessModifier)), bSequence(std::move(bSequence)), retType(std::move(retType)), name(std::move(name)), typeParams(std::move(typeParams)), parameters(std::move(parameters)), body(std::move(body)) { }

GlobalFuncDeclSyntax::GlobalFuncDeclSyntax(GlobalFuncDeclSyntax&& other) noexcept = default;

GlobalFuncDeclSyntax::~GlobalFuncDeclSyntax() = default;

GlobalFuncDeclSyntax& GlobalFuncDeclSyntax::operator=(GlobalFuncDeclSyntax&& other) noexcept = default;

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

ClassMemberFuncDeclSyntax::ClassMemberFuncDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, bool bStatic, bool bSequence, TypeExpSyntax retType, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body)
    : accessModifier(std::move(accessModifier)), bStatic(std::move(bStatic)), bSequence(std::move(bSequence)), retType(std::move(retType)), name(std::move(name)), typeParams(std::move(typeParams)), parameters(std::move(parameters)), body(std::move(body)) { }

ClassMemberFuncDeclSyntax::ClassMemberFuncDeclSyntax(ClassMemberFuncDeclSyntax&& other) noexcept = default;

ClassMemberFuncDeclSyntax::~ClassMemberFuncDeclSyntax() = default;

ClassMemberFuncDeclSyntax& ClassMemberFuncDeclSyntax::operator=(ClassMemberFuncDeclSyntax&& other) noexcept = default;

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

ClassConstructorDeclSyntax::ClassConstructorDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<FuncParamSyntax> parameters, std::optional<std::vector<ArgumentSyntax>> baseArgs, std::vector<StmtSyntax> body)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), parameters(std::move(parameters)), baseArgs(std::move(baseArgs)), body(std::move(body)) { }

ClassConstructorDeclSyntax::ClassConstructorDeclSyntax(ClassConstructorDeclSyntax&& other) noexcept = default;

ClassConstructorDeclSyntax::~ClassConstructorDeclSyntax() = default;

ClassConstructorDeclSyntax& ClassConstructorDeclSyntax::operator=(ClassConstructorDeclSyntax&& other) noexcept = default;

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

ClassMemberVarDeclSyntax::ClassMemberVarDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, TypeExpSyntax varType, std::vector<std::string> varNames)
    : accessModifier(std::move(accessModifier)), varType(std::move(varType)), varNames(std::move(varNames)) { }

ClassMemberVarDeclSyntax::ClassMemberVarDeclSyntax(ClassMemberVarDeclSyntax&& other) noexcept = default;

ClassMemberVarDeclSyntax::~ClassMemberVarDeclSyntax() = default;

ClassMemberVarDeclSyntax& ClassMemberVarDeclSyntax::operator=(ClassMemberVarDeclSyntax&& other) noexcept = default;

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
    return std::visit(ToJsonVisitor(), decl);
}

ClassDeclSyntax::ClassDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<TypeExpSyntax> baseTypes, std::vector<ClassMemberDeclSyntax> memberDecls)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), typeParams(std::move(typeParams)), baseTypes(std::move(baseTypes)), memberDecls(std::move(memberDecls)) { }

ClassDeclSyntax::ClassDeclSyntax(ClassDeclSyntax&& other) noexcept = default;

ClassDeclSyntax::~ClassDeclSyntax() = default;

ClassDeclSyntax& ClassDeclSyntax::operator=(ClassDeclSyntax&& other) noexcept = default;

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

StructMemberFuncDeclSyntax::StructMemberFuncDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, bool bStatic, bool bSequence, TypeExpSyntax retType, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body)
    : accessModifier(std::move(accessModifier)), bStatic(std::move(bStatic)), bSequence(std::move(bSequence)), retType(std::move(retType)), name(std::move(name)), typeParams(std::move(typeParams)), parameters(std::move(parameters)), body(std::move(body)) { }

StructMemberFuncDeclSyntax::StructMemberFuncDeclSyntax(StructMemberFuncDeclSyntax&& other) noexcept = default;

StructMemberFuncDeclSyntax::~StructMemberFuncDeclSyntax() = default;

StructMemberFuncDeclSyntax& StructMemberFuncDeclSyntax::operator=(StructMemberFuncDeclSyntax&& other) noexcept = default;

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

StructConstructorDeclSyntax::StructConstructorDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<FuncParamSyntax> parameters, std::vector<StmtSyntax> body)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), parameters(std::move(parameters)), body(std::move(body)) { }

StructConstructorDeclSyntax::StructConstructorDeclSyntax(StructConstructorDeclSyntax&& other) noexcept = default;

StructConstructorDeclSyntax::~StructConstructorDeclSyntax() = default;

StructConstructorDeclSyntax& StructConstructorDeclSyntax::operator=(StructConstructorDeclSyntax&& other) noexcept = default;

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

StructMemberVarDeclSyntax::StructMemberVarDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, TypeExpSyntax varType, std::vector<std::string> varNames)
    : accessModifier(std::move(accessModifier)), varType(std::move(varType)), varNames(std::move(varNames)) { }

StructMemberVarDeclSyntax::StructMemberVarDeclSyntax(StructMemberVarDeclSyntax&& other) noexcept = default;

StructMemberVarDeclSyntax::~StructMemberVarDeclSyntax() = default;

StructMemberVarDeclSyntax& StructMemberVarDeclSyntax::operator=(StructMemberVarDeclSyntax&& other) noexcept = default;

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
    return std::visit(ToJsonVisitor(), decl);
}

StructDeclSyntax::StructDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<TypeExpSyntax> baseTypes, std::vector<StructMemberDeclSyntax> memberDecls)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), typeParams(std::move(typeParams)), baseTypes(std::move(baseTypes)), memberDecls(std::move(memberDecls)) { }

StructDeclSyntax::StructDeclSyntax(StructDeclSyntax&& other) noexcept = default;

StructDeclSyntax::~StructDeclSyntax() = default;

StructDeclSyntax& StructDeclSyntax::operator=(StructDeclSyntax&& other) noexcept = default;

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

EnumElemMemberVarDeclSyntax::EnumElemMemberVarDeclSyntax(TypeExpSyntax type, std::string name)
    : type(std::move(type)), name(std::move(name)) { }

EnumElemMemberVarDeclSyntax::EnumElemMemberVarDeclSyntax(EnumElemMemberVarDeclSyntax&& other) noexcept = default;

EnumElemMemberVarDeclSyntax::~EnumElemMemberVarDeclSyntax() = default;

EnumElemMemberVarDeclSyntax& EnumElemMemberVarDeclSyntax::operator=(EnumElemMemberVarDeclSyntax&& other) noexcept = default;

JsonItem EnumElemMemberVarDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("EnumElemMemberVarDeclSyntax") },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

EnumElemDeclSyntax::EnumElemDeclSyntax(std::string name, std::vector<EnumElemMemberVarDeclSyntax> memberVars)
    : name(std::move(name)), memberVars(std::move(memberVars)) { }

EnumElemDeclSyntax::EnumElemDeclSyntax(EnumElemDeclSyntax&& other) noexcept = default;

EnumElemDeclSyntax::~EnumElemDeclSyntax() = default;

EnumElemDeclSyntax& EnumElemDeclSyntax::operator=(EnumElemDeclSyntax&& other) noexcept = default;

JsonItem EnumElemDeclSyntax::ToJson()
{
    return JsonObject {
        { "$type", JsonString("EnumElemDeclSyntax") },
        { "name", Citron::ToJson(name) },
        { "memberVars", Citron::ToJson(memberVars) },
    };
}

EnumDeclSyntax::EnumDeclSyntax(std::optional<AccessModifierSyntax> accessModifier, std::string name, std::vector<TypeParamSyntax> typeParams, std::vector<EnumElemDeclSyntax> elements)
    : accessModifier(std::move(accessModifier)), name(std::move(name)), typeParams(std::move(typeParams)), elements(std::move(elements)) { }

EnumDeclSyntax::EnumDeclSyntax(EnumDeclSyntax&& other) noexcept = default;

EnumDeclSyntax::~EnumDeclSyntax() = default;

EnumDeclSyntax& EnumDeclSyntax::operator=(EnumDeclSyntax&& other) noexcept = default;

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
    return std::visit(ToJsonVisitor(), elem);
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
    return std::visit(ToJsonVisitor(), elem);
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
