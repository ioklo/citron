#include "Syntaxes.g.h"

#include "Infra/Json.h"

using namespace std;

namespace Citron {

namespace {
struct ToJsonVisitor {
    template<typename T>
    JsonItem operator()(T* t) { return t->ToJson(); }

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

SArgument::SArgument(std::optional<SArgModifier> o_modifier, SExp* exp)
    : o_modifier(move(o_modifier)), exp(move(exp)) { }

SArgument::SArgument(SArgument&& other) noexcept = default;

SArgument::~SArgument() = default;

SArgument& SArgument::operator=(SArgument&& other) noexcept = default;

JsonItem SArgument::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SArgument") },
        { "o_modifier", Citron::ToJson(o_modifier) },
        { "exp", Citron::ToJson(exp) },
    };
}

SArguments::SArguments(std::vector<SArgument*> items)
    : items(move(items)) { }

SArguments::SArguments(SArguments&& other) noexcept = default;

SArguments::~SArguments() = default;

SArguments& SArguments::operator=(SArguments&& other) noexcept = default;

JsonItem SArguments::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SArguments") },
        { "items", Citron::ToJson(items) },
    };
}

SLambdaExpParam::SLambdaExpParam(std::optional<SParamModifier> o_paramModifier, STypeExp* type, std::string name)
    : o_paramModifier(move(o_paramModifier)), type(move(type)), name(move(name)) { }

SLambdaExpParam::SLambdaExpParam(SLambdaExpParam&& other) noexcept = default;

SLambdaExpParam::~SLambdaExpParam() = default;

SLambdaExpParam& SLambdaExpParam::operator=(SLambdaExpParam&& other) noexcept = default;

JsonItem SLambdaExpParam::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SLambdaExpParam") },
        { "o_paramModifier", Citron::ToJson(o_paramModifier) },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

JsonItem SVarDeclElementInit_Uninit::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclElementInit_Uninit") },
    };
}

JsonItem SVarDeclElementInit_Exp::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclElementInit_Exp") },
        { "exp", Citron::ToJson(exp) },
    };
}

JsonItem SVarDeclElementInit_Move::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclElementInit_Move") },
        { "exp", Citron::ToJson(exp) },
    };
}

JsonItem ToJson(SVarDeclElementInit& init)
{
    return std::visit(ToJsonVisitor(), init);
}

SVarDeclElement::SVarDeclElement(std::string varName, SVarDeclElementInit init)
    : varName(move(varName)), init(move(init)) { }

SVarDeclElement::SVarDeclElement(SVarDeclElement&& other) noexcept = default;

SVarDeclElement::~SVarDeclElement() = default;

SVarDeclElement& SVarDeclElement::operator=(SVarDeclElement&& other) noexcept = default;

JsonItem SVarDeclElement::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclElement") },
        { "varName", Citron::ToJson(varName) },
        { "init", Citron::ToJson(init) },
    };
}

SVarDecl::SVarDecl(SVarDeclType* type, std::vector<SVarDeclElement> elements)
    : type(move(type)), elements(move(elements)) { }

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
    : name(move(name)) { }

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

SFuncParam::SFuncParam(std::optional<SParamModifier> o_modifier, bool bRef, STypeExp* type, std::string name)
    : o_modifier(move(o_modifier)), bRef(move(bRef)), type(move(type)), name(move(name)) { }

SFuncParam::SFuncParam(SFuncParam&& other) noexcept = default;

SFuncParam::~SFuncParam() = default;

SFuncParam& SFuncParam::operator=(SFuncParam&& other) noexcept = default;

JsonItem SFuncParam::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SFuncParam") },
        { "o_modifier", Citron::ToJson(o_modifier) },
        { "bRef", Citron::ToJson(bRef) },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

JsonItem SFuncReturn_Normal::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SFuncReturn_Normal") },
        { "type", Citron::ToJson(type) },
    };
}

JsonItem SFuncReturn_Opaque::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SFuncReturn_Opaque") },
        { "type", Citron::ToJson(type) },
    };
}

JsonItem ToJson(SFuncReturn& funcRet)
{
    return std::visit(ToJsonVisitor(), funcRet);
}

struct SStmtToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(SStmt_Command* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_VarDecl* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_If* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_For* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_While* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Switch* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Continue* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Break* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Leave* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Return* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Block* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Blank* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Exp* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Task* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Await* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Async* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Foreach* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Yield* stmt) { return stmt->ToJson(); }
    ResultType Visit(SStmt_Directive* stmt) { return stmt->ToJson(); }
};

JsonItem ToJson(SStmt* stmt)
{
    if (!stmt) return JsonNull();

    SStmtToJsonVisitor visitor;
    return Accept(visitor, stmt);
}
struct SExpToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(SExp_Identifier* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_String* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_IntLiteral* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_BoolLiteral* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_NullLiteral* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_BinaryOp* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_UnaryOp* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_Call* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_Lambda* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_Indexer* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_Member* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_List* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_New* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_Shared* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_Is* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_As* exp) { return exp->ToJson(); }
    ResultType Visit(SExp_Inline* exp) { return exp->ToJson(); }
};

JsonItem ToJson(SExp* exp)
{
    if (!exp) return JsonNull();

    SExpToJsonVisitor visitor;
    return Accept(visitor, exp);
}
struct STypeExpToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(STypeExp_Id* typeExp) { return typeExp->ToJson(); }
    ResultType Visit(STypeExp_Member* typeExp) { return typeExp->ToJson(); }
    ResultType Visit(STypeExp_Nullable* typeExp) { return typeExp->ToJson(); }
    ResultType Visit(STypeExp_Shared* typeExp) { return typeExp->ToJson(); }
    ResultType Visit(STypeExp_Box* typeExp) { return typeExp->ToJson(); }
    ResultType Visit(STypeExp_Ptr* typeExp) { return typeExp->ToJson(); }
    ResultType Visit(STypeExp_Local* typeExp) { return typeExp->ToJson(); }
};

JsonItem ToJson(STypeExp* typeExp)
{
    if (!typeExp) return JsonNull();

    STypeExpToJsonVisitor visitor;
    return Accept(visitor, typeExp);
}
struct SStringExpElementToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(SStringExpElement_Text* elem) { return elem->ToJson(); }
    ResultType Visit(SStringExpElement_Exp* elem) { return elem->ToJson(); }
};

JsonItem ToJson(SStringExpElement* elem)
{
    if (!elem) return JsonNull();

    SStringExpElementToJsonVisitor visitor;
    return Accept(visitor, elem);
}
struct SLambdaExpBodyToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(SLambdaExpBody_Stmts* body) { return body->ToJson(); }
    ResultType Visit(SLambdaExpBody_Exp* body) { return body->ToJson(); }
};

JsonItem ToJson(SLambdaExpBody* body)
{
    if (!body) return JsonNull();

    SLambdaExpBodyToJsonVisitor visitor;
    return Accept(visitor, body);
}
struct SEmbeddableStmtToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(SEmbeddableStmt_Single* stmt) { return stmt->ToJson(); }
    ResultType Visit(SEmbeddableStmt_Block* stmt) { return stmt->ToJson(); }
};

JsonItem ToJson(SEmbeddableStmt* stmt)
{
    if (!stmt) return JsonNull();

    SEmbeddableStmtToJsonVisitor visitor;
    return Accept(visitor, stmt);
}
struct SForStmtInitializerToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(SForStmtInitializer_Exp* initializer) { return initializer->ToJson(); }
    ResultType Visit(SForStmtInitializer_VarDecl* initializer) { return initializer->ToJson(); }
};

JsonItem ToJson(SForStmtInitializer* initializer)
{
    if (!initializer) return JsonNull();

    SForStmtInitializerToJsonVisitor visitor;
    return Accept(visitor, initializer);
}
JsonItem ToJson(SClassMemberDecl& decl)
{
    return std::visit(ToJsonVisitor(), decl);
}

JsonItem ToJson(SStructMemberDecl& decl)
{
    return std::visit(ToJsonVisitor(), decl);
}

JsonItem ToJson(SNamespaceDeclElement& elem)
{
    return std::visit(ToJsonVisitor(), elem);
}

JsonItem ToJson(SScriptElement& elem)
{
    return std::visit(ToJsonVisitor(), elem);
}

struct SVarDeclTypeToJsonVisitor
{
    using ResultType = JsonItem;
    ResultType Visit(SVarDeclType_Var* type) { return type->ToJson(); }
    ResultType Visit(SVarDeclType_VarRef* type) { return type->ToJson(); }
    ResultType Visit(SVarDeclType_Ref* type) { return type->ToJson(); }
    ResultType Visit(SVarDeclType_Normal* type) { return type->ToJson(); }
};

JsonItem ToJson(SVarDeclType* type)
{
    if (!type) return JsonNull();

    SVarDeclTypeToJsonVisitor visitor;
    return Accept(visitor, type);
}
SExp_Identifier::SExp_Identifier(std::string value, std::vector<STypeExp*> typeArgs)
    : value(move(value)), typeArgs(move(typeArgs)) { }

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

SExp_String::SExp_String(std::vector<SStringExpElement*> elements)
    : elements(move(elements)) { }

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
    : value(move(value)) { }

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
    : value(move(value)) { }

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

SExp_List::SExp_List(std::vector<SExp*> elements)
    : elements(move(elements)) { }

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

SExp_New::SExp_New(STypeExp* type, SArguments* args)
    : type(move(type)), args(move(args)) { }

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

SExp_BinaryOp::SExp_BinaryOp(SBinaryOpKind kind, SExp* operand0, SExp* operand1)
    : kind(move(kind)), operand0(move(operand0)), operand1(move(operand1)) { }

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

SExp_UnaryOp::SExp_UnaryOp(SUnaryOpKind kind, SExp* operand)
    : kind(move(kind)), operand(move(operand)) { }

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

SExp_Call::SExp_Call(SExp* callable, SArguments* args)
    : callable(move(callable)), args(move(args)) { }

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

SExp_Lambda::SExp_Lambda(std::vector<SLambdaExpParam> params, SLambdaExpBody* body)
    : params(move(params)), body(move(body)) { }

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

SExp_Indexer::SExp_Indexer(SExp* obj, SExp* index)
    : obj(move(obj)), index(move(index)) { }

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

SExp_Member::SExp_Member(SExp* base, std::string memberName, std::vector<STypeExp*> memberTypeArgs)
    : base(move(base)), memberName(move(memberName)), memberTypeArgs(move(memberTypeArgs)) { }

SExp_Member::SExp_Member(SExp_Member&& other) noexcept = default;

SExp_Member::~SExp_Member() = default;

SExp_Member& SExp_Member::operator=(SExp_Member&& other) noexcept = default;

JsonItem SExp_Member::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Member") },
        { "base", Citron::ToJson(base) },
        { "memberName", Citron::ToJson(memberName) },
        { "memberTypeArgs", Citron::ToJson(memberTypeArgs) },
    };
}

SExp_Shared::SExp_Shared(SExp* innerExp)
    : innerExp(move(innerExp)) { }

SExp_Shared::SExp_Shared(SExp_Shared&& other) noexcept = default;

SExp_Shared::~SExp_Shared() = default;

SExp_Shared& SExp_Shared::operator=(SExp_Shared&& other) noexcept = default;

JsonItem SExp_Shared::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Shared") },
        { "innerExp", Citron::ToJson(innerExp) },
    };
}

SExp_Is::SExp_Is(SExp* exp, STypeExp* type, std::optional<std::string> o_bindName)
    : exp(move(exp)), type(move(type)), o_bindName(move(o_bindName)) { }

SExp_Is::SExp_Is(SExp_Is&& other) noexcept = default;

SExp_Is::~SExp_Is() = default;

SExp_Is& SExp_Is::operator=(SExp_Is&& other) noexcept = default;

JsonItem SExp_Is::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Is") },
        { "exp", Citron::ToJson(exp) },
        { "type", Citron::ToJson(type) },
        { "o_bindName", Citron::ToJson(o_bindName) },
    };
}

SExp_As::SExp_As(SExp* exp, STypeExp* type)
    : exp(move(exp)), type(move(type)) { }

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

SExp_Inline::SExp_Inline(std::vector<SStmt*> stmts, SExp* o_finalExp)
    : stmts(move(stmts)), o_finalExp(move(o_finalExp)) { }

SExp_Inline::SExp_Inline(SExp_Inline&& other) noexcept = default;

SExp_Inline::~SExp_Inline() = default;

SExp_Inline& SExp_Inline::operator=(SExp_Inline&& other) noexcept = default;

JsonItem SExp_Inline::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SExp_Inline") },
        { "stmts", Citron::ToJson(stmts) },
        { "o_finalExp", Citron::ToJson(o_finalExp) },
    };
}

STypeExp_Id::STypeExp_Id(std::string name, std::vector<STypeExp*> typeArgs)
    : name(move(name)), typeArgs(move(typeArgs)) { }

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

STypeExp_Member::STypeExp_Member(STypeExp* parentType, std::string name, std::vector<STypeExp*> typeArgs)
    : parentType(move(parentType)), name(move(name)), typeArgs(move(typeArgs)) { }

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

STypeExp_Nullable::STypeExp_Nullable(STypeExp* innerType)
    : innerType(move(innerType)) { }

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

STypeExp_Shared::STypeExp_Shared(STypeExp* innerType)
    : innerType(move(innerType)) { }

STypeExp_Shared::STypeExp_Shared(STypeExp_Shared&& other) noexcept = default;

STypeExp_Shared::~STypeExp_Shared() = default;

STypeExp_Shared& STypeExp_Shared::operator=(STypeExp_Shared&& other) noexcept = default;

JsonItem STypeExp_Shared::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_Shared") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

STypeExp_Box::STypeExp_Box(STypeExp* innerType)
    : innerType(move(innerType)) { }

STypeExp_Box::STypeExp_Box(STypeExp_Box&& other) noexcept = default;

STypeExp_Box::~STypeExp_Box() = default;

STypeExp_Box& STypeExp_Box::operator=(STypeExp_Box&& other) noexcept = default;

JsonItem STypeExp_Box::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_Box") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

STypeExp_Ptr::STypeExp_Ptr(STypeExp* innerType)
    : innerType(move(innerType)) { }

STypeExp_Ptr::STypeExp_Ptr(STypeExp_Ptr&& other) noexcept = default;

STypeExp_Ptr::~STypeExp_Ptr() = default;

STypeExp_Ptr& STypeExp_Ptr::operator=(STypeExp_Ptr&& other) noexcept = default;

JsonItem STypeExp_Ptr::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STypeExp_Ptr") },
        { "innerType", Citron::ToJson(innerType) },
    };
}

STypeExp_Local::STypeExp_Local(STypeExp* innerType)
    : innerType(move(innerType)) { }

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

SVarDeclType_Var::SVarDeclType_Var(SVarDeclType_VarKind kind)
    : kind(move(kind)) { }

SVarDeclType_Var::SVarDeclType_Var(SVarDeclType_Var&& other) noexcept = default;

SVarDeclType_Var::~SVarDeclType_Var() = default;

SVarDeclType_Var& SVarDeclType_Var::operator=(SVarDeclType_Var&& other) noexcept = default;

JsonItem SVarDeclType_Var::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclType_Var") },
        { "kind", Citron::ToJson(kind) },
    };
}

SVarDeclType_VarRef::SVarDeclType_VarRef()
{ }
SVarDeclType_VarRef::SVarDeclType_VarRef(SVarDeclType_VarRef&& other) noexcept = default;

SVarDeclType_VarRef::~SVarDeclType_VarRef() = default;

SVarDeclType_VarRef& SVarDeclType_VarRef::operator=(SVarDeclType_VarRef&& other) noexcept = default;

JsonItem SVarDeclType_VarRef::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclType_VarRef") },
    };
}

SVarDeclType_Ref::SVarDeclType_Ref(STypeExp* typeExp)
    : typeExp(move(typeExp)) { }

SVarDeclType_Ref::SVarDeclType_Ref(SVarDeclType_Ref&& other) noexcept = default;

SVarDeclType_Ref::~SVarDeclType_Ref() = default;

SVarDeclType_Ref& SVarDeclType_Ref::operator=(SVarDeclType_Ref&& other) noexcept = default;

JsonItem SVarDeclType_Ref::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclType_Ref") },
        { "typeExp", Citron::ToJson(typeExp) },
    };
}

SVarDeclType_Normal::SVarDeclType_Normal(STypeExp* typeExp)
    : typeExp(move(typeExp)) { }

SVarDeclType_Normal::SVarDeclType_Normal(SVarDeclType_Normal&& other) noexcept = default;

SVarDeclType_Normal::~SVarDeclType_Normal() = default;

SVarDeclType_Normal& SVarDeclType_Normal::operator=(SVarDeclType_Normal&& other) noexcept = default;

JsonItem SVarDeclType_Normal::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SVarDeclType_Normal") },
        { "typeExp", Citron::ToJson(typeExp) },
    };
}

SStringExpElement_Text::SStringExpElement_Text(std::string text)
    : text(move(text)) { }

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

SStringExpElement_Exp::SStringExpElement_Exp(SExp* exp)
    : exp(move(exp)) { }

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

SLambdaExpBody_Stmts::SLambdaExpBody_Stmts(std::vector<SStmt*> stmts)
    : stmts(move(stmts)) { }

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

SLambdaExpBody_Exp::SLambdaExpBody_Exp(SExp* exp)
    : exp(move(exp)) { }

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

SEmbeddableStmt_Single::SEmbeddableStmt_Single(SStmt* stmt)
    : stmt(move(stmt)) { }

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

SEmbeddableStmt_Block::SEmbeddableStmt_Block(std::vector<SStmt*> stmts)
    : stmts(move(stmts)) { }

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

SForStmtInitializer_Exp::SForStmtInitializer_Exp(SExp* exp)
    : exp(move(exp)) { }

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
    : varDecl(move(varDecl)) { }

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

SStmt_Command::SStmt_Command(std::vector<SExp_String*> commands)
    : commands(move(commands)) { }

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
    : varDecl(move(varDecl)) { }

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

SStmt_Continue::SStmt_Continue(std::optional<std::string> o_label)
    : o_label(move(o_label)) { }

SStmt_Continue::SStmt_Continue(SStmt_Continue&& other) noexcept = default;

SStmt_Continue::~SStmt_Continue() = default;

SStmt_Continue& SStmt_Continue::operator=(SStmt_Continue&& other) noexcept = default;

JsonItem SStmt_Continue::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Continue") },
        { "o_label", Citron::ToJson(o_label) },
    };
}

SStmt_Break::SStmt_Break(std::optional<std::string> o_label)
    : o_label(move(o_label)) { }

SStmt_Break::SStmt_Break(SStmt_Break&& other) noexcept = default;

SStmt_Break::~SStmt_Break() = default;

SStmt_Break& SStmt_Break::operator=(SStmt_Break&& other) noexcept = default;

JsonItem SStmt_Break::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Break") },
        { "o_label", Citron::ToJson(o_label) },
    };
}

SStmt_Block::SStmt_Block(std::vector<SStmt*> stmts)
    : stmts(move(stmts)) { }

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

SStmt_Task::SStmt_Task(std::vector<SStmt*> body)
    : body(move(body)) { }

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

SStmt_Await::SStmt_Await(std::vector<SStmt*> body)
    : body(move(body)) { }

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

SStmt_Async::SStmt_Async(std::vector<SStmt*> body)
    : body(move(body)) { }

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

SStmt_Directive::SStmt_Directive(std::string name, std::vector<SExp*> args)
    : name(move(name)), args(move(args)) { }

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

SStmt_If::SStmt_If(SExp* cond, SEmbeddableStmt* body, SEmbeddableStmt* elseBody)
    : cond(move(cond)), body(move(body)), elseBody(move(elseBody)) { }

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

SStmt_For::SStmt_For(std::optional<std::string> o_label, SForStmtInitializer* initializer, SExp* cond, SExp* cont, SEmbeddableStmt* body)
    : o_label(move(o_label)), initializer(move(initializer)), cond(move(cond)), cont(move(cont)), body(move(body)) { }

SStmt_For::SStmt_For(SStmt_For&& other) noexcept = default;

SStmt_For::~SStmt_For() = default;

SStmt_For& SStmt_For::operator=(SStmt_For&& other) noexcept = default;

JsonItem SStmt_For::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_For") },
        { "o_label", Citron::ToJson(o_label) },
        { "initializer", Citron::ToJson(initializer) },
        { "cond", Citron::ToJson(cond) },
        { "cont", Citron::ToJson(cont) },
        { "body", Citron::ToJson(body) },
    };
}

SStmt_While::SStmt_While(std::optional<std::string> o_label, SExp* cond, SEmbeddableStmt* body)
    : o_label(move(o_label)), cond(move(cond)), body(move(body)) { }

SStmt_While::SStmt_While(SStmt_While&& other) noexcept = default;

SStmt_While::~SStmt_While() = default;

SStmt_While& SStmt_While::operator=(SStmt_While&& other) noexcept = default;

JsonItem SStmt_While::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_While") },
        { "o_label", Citron::ToJson(o_label) },
        { "cond", Citron::ToJson(cond) },
        { "body", Citron::ToJson(body) },
    };
}

SStmt_Switch::SStmt_Switch(std::optional<std::string> o_label, SExp* value)
    : o_label(move(o_label)), value(move(value)) { }

SStmt_Switch::SStmt_Switch(SStmt_Switch&& other) noexcept = default;

SStmt_Switch::~SStmt_Switch() = default;

SStmt_Switch& SStmt_Switch::operator=(SStmt_Switch&& other) noexcept = default;

JsonItem SStmt_Switch::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Switch") },
        { "o_label", Citron::ToJson(o_label) },
        { "value", Citron::ToJson(value) },
    };
}

SStmt_Leave::SStmt_Leave(std::optional<std::string> o_label, SExp* value)
    : o_label(move(o_label)), value(move(value)) { }

SStmt_Leave::SStmt_Leave(SStmt_Leave&& other) noexcept = default;

SStmt_Leave::~SStmt_Leave() = default;

SStmt_Leave& SStmt_Leave::operator=(SStmt_Leave&& other) noexcept = default;

JsonItem SStmt_Leave::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStmt_Leave") },
        { "o_label", Citron::ToJson(o_label) },
        { "value", Citron::ToJson(value) },
    };
}

SStmt_Return::SStmt_Return(SExp* value)
    : value(move(value)) { }

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

SStmt_Exp::SStmt_Exp(SExp* exp)
    : exp(move(exp)) { }

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

SStmt_Foreach::SStmt_Foreach(STypeExp* type, std::string varName, SExp* enumerable, SEmbeddableStmt* body)
    : type(move(type)), varName(move(varName)), enumerable(move(enumerable)), body(move(body)) { }

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

SStmt_Yield::SStmt_Yield(SExp* value)
    : value(move(value)) { }

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

SGlobalFuncDecl::SGlobalFuncDecl(std::optional<SAccessModifier> accessModifier, bool bSequence, SFuncReturn funcRet, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmt*> body)
    : accessModifier(move(accessModifier)), bSequence(move(bSequence)), funcRet(move(funcRet)), name(move(name)), typeParams(move(typeParams)), parameters(move(parameters)), body(move(body)) { }

SGlobalFuncDecl::SGlobalFuncDecl(SGlobalFuncDecl&& other) noexcept = default;

SGlobalFuncDecl::~SGlobalFuncDecl() = default;

SGlobalFuncDecl& SGlobalFuncDecl::operator=(SGlobalFuncDecl&& other) noexcept = default;

JsonItem SGlobalFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SGlobalFuncDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "bSequence", Citron::ToJson(bSequence) },
        { "funcRet", Citron::ToJson(funcRet) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SClassDecl::SClassDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExp*> baseTypes, std::vector<SClassMemberDecl> memberDecls)
    : accessModifier(move(accessModifier)), name(move(name)), typeParams(move(typeParams)), baseTypes(move(baseTypes)), memberDecls(move(memberDecls)) { }

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

SClassFuncDecl::SClassFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, SFuncReturn funcRet, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmt*> body)
    : accessModifier(move(accessModifier)), bStatic(move(bStatic)), bSequence(move(bSequence)), funcRet(move(funcRet)), name(move(name)), typeParams(move(typeParams)), parameters(move(parameters)), body(move(body)) { }

SClassFuncDecl::SClassFuncDecl(SClassFuncDecl&& other) noexcept = default;

SClassFuncDecl::~SClassFuncDecl() = default;

SClassFuncDecl& SClassFuncDecl::operator=(SClassFuncDecl&& other) noexcept = default;

JsonItem SClassFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassFuncDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "bStatic", Citron::ToJson(bStatic) },
        { "bSequence", Citron::ToJson(bSequence) },
        { "funcRet", Citron::ToJson(funcRet) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SClassCtorDecl::SClassCtorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, SArguments* baseArgs, std::vector<SStmt*> body)
    : accessModifier(move(accessModifier)), parameters(move(parameters)), baseArgs(move(baseArgs)), body(move(body)) { }

SClassCtorDecl::SClassCtorDecl(SClassCtorDecl&& other) noexcept = default;

SClassCtorDecl::~SClassCtorDecl() = default;

SClassCtorDecl& SClassCtorDecl::operator=(SClassCtorDecl&& other) noexcept = default;

JsonItem SClassCtorDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassCtorDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "parameters", Citron::ToJson(parameters) },
        { "baseArgs", Citron::ToJson(baseArgs) },
        { "body", Citron::ToJson(body) },
    };
}

SClassVarDecl::SClassVarDecl(std::optional<SAccessModifier> accessModifier, STypeExp* varType, std::vector<std::string> varNames)
    : accessModifier(move(accessModifier)), varType(move(varType)), varNames(move(varNames)) { }

SClassVarDecl::SClassVarDecl(SClassVarDecl&& other) noexcept = default;

SClassVarDecl::~SClassVarDecl() = default;

SClassVarDecl& SClassVarDecl::operator=(SClassVarDecl&& other) noexcept = default;

JsonItem SClassVarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SClassVarDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "varType", Citron::ToJson(varType) },
        { "varNames", Citron::ToJson(varNames) },
    };
}

SStructDecl::SStructDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STypeExp*> traits, std::vector<SStructMemberDecl> memberDecls)
    : accessModifier(move(accessModifier)), name(move(name)), typeParams(move(typeParams)), traits(move(traits)), memberDecls(move(memberDecls)) { }

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
        { "traits", Citron::ToJson(traits) },
        { "memberDecls", Citron::ToJson(memberDecls) },
    };
}

SStructFuncDecl::SStructFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, SFuncReturn funcRet, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmt*> body)
    : accessModifier(move(accessModifier)), bStatic(move(bStatic)), bSequence(move(bSequence)), funcRet(move(funcRet)), name(move(name)), typeParams(move(typeParams)), parameters(move(parameters)), body(move(body)) { }

SStructFuncDecl::SStructFuncDecl(SStructFuncDecl&& other) noexcept = default;

SStructFuncDecl::~SStructFuncDecl() = default;

SStructFuncDecl& SStructFuncDecl::operator=(SStructFuncDecl&& other) noexcept = default;

JsonItem SStructFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructFuncDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "bStatic", Citron::ToJson(bStatic) },
        { "bSequence", Citron::ToJson(bSequence) },
        { "funcRet", Citron::ToJson(funcRet) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SStructCtorDecl::SStructCtorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, std::vector<SStmt*> body)
    : accessModifier(move(accessModifier)), parameters(move(parameters)), body(move(body)) { }

SStructCtorDecl::SStructCtorDecl(SStructCtorDecl&& other) noexcept = default;

SStructCtorDecl::~SStructCtorDecl() = default;

SStructCtorDecl& SStructCtorDecl::operator=(SStructCtorDecl&& other) noexcept = default;

JsonItem SStructCtorDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructCtorDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

SStructDtorDecl::SStructDtorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SStmt*> body)
    : accessModifier(move(accessModifier)), body(move(body)) { }

SStructDtorDecl::SStructDtorDecl(SStructDtorDecl&& other) noexcept = default;

SStructDtorDecl::~SStructDtorDecl() = default;

SStructDtorDecl& SStructDtorDecl::operator=(SStructDtorDecl&& other) noexcept = default;

JsonItem SStructDtorDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructDtorDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "body", Citron::ToJson(body) },
    };
}

SStructVarDecl::SStructVarDecl(std::optional<SAccessModifier> accessModifier, STypeExp* varType, std::vector<std::string> varNames)
    : accessModifier(move(accessModifier)), varType(move(varType)), varNames(move(varNames)) { }

SStructVarDecl::SStructVarDecl(SStructVarDecl&& other) noexcept = default;

SStructVarDecl::~SStructVarDecl() = default;

SStructVarDecl& SStructVarDecl::operator=(SStructVarDecl&& other) noexcept = default;

JsonItem SStructVarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SStructVarDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "varType", Citron::ToJson(varType) },
        { "varNames", Citron::ToJson(varNames) },
    };
}

SEnumElemVarDecl::SEnumElemVarDecl(STypeExp* type, std::string name)
    : type(move(type)), name(move(name)) { }

SEnumElemVarDecl::SEnumElemVarDecl(SEnumElemVarDecl&& other) noexcept = default;

SEnumElemVarDecl::~SEnumElemVarDecl() = default;

SEnumElemVarDecl& SEnumElemVarDecl::operator=(SEnumElemVarDecl&& other) noexcept = default;

JsonItem SEnumElemVarDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SEnumElemVarDecl") },
        { "type", Citron::ToJson(type) },
        { "name", Citron::ToJson(name) },
    };
}

SEnumElemDecl::SEnumElemDecl(std::string name, std::vector<SEnumElemVarDecl*> vars)
    : name(move(name)), vars(move(vars)) { }

SEnumElemDecl::SEnumElemDecl(SEnumElemDecl&& other) noexcept = default;

SEnumElemDecl::~SEnumElemDecl() = default;

SEnumElemDecl& SEnumElemDecl::operator=(SEnumElemDecl&& other) noexcept = default;

JsonItem SEnumElemDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SEnumElemDecl") },
        { "name", Citron::ToJson(name) },
        { "vars", Citron::ToJson(vars) },
    };
}

SEnumDecl::SEnumDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<SEnumElemDecl*> elements)
    : accessModifier(move(accessModifier)), name(move(name)), typeParams(move(typeParams)), elements(move(elements)) { }

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

STraitFuncDecl::STraitFuncDecl(bool bStatic, SFuncReturn funcRet, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters)
    : bStatic(move(bStatic)), funcRet(move(funcRet)), name(move(name)), typeParams(move(typeParams)), parameters(move(parameters)) { }

STraitFuncDecl::STraitFuncDecl(STraitFuncDecl&& other) noexcept = default;

STraitFuncDecl::~STraitFuncDecl() = default;

STraitFuncDecl& STraitFuncDecl::operator=(STraitFuncDecl&& other) noexcept = default;

JsonItem STraitFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STraitFuncDecl") },
        { "bStatic", Citron::ToJson(bStatic) },
        { "funcRet", Citron::ToJson(funcRet) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
    };
}

JsonItem ToJson(STraitMemberDecl& memberDecl)
{
    return std::visit(ToJsonVisitor(), memberDecl);
}

STraitDecl::STraitDecl(std::optional<SAccessModifier> accessModifier, std::string name, std::vector<STypeParam> typeParams, std::vector<STraitMemberDecl> memberDecls)
    : accessModifier(move(accessModifier)), name(move(name)), typeParams(move(typeParams)), memberDecls(move(memberDecls)) { }

STraitDecl::STraitDecl(STraitDecl&& other) noexcept = default;

STraitDecl::~STraitDecl() = default;

STraitDecl& STraitDecl::operator=(STraitDecl&& other) noexcept = default;

JsonItem STraitDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("STraitDecl") },
        { "accessModifier", Citron::ToJson(accessModifier) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "memberDecls", Citron::ToJson(memberDecls) },
    };
}

SImplFuncDecl::SImplFuncDecl(bool bStatic, SFuncReturn funcReturn, std::string name, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmt*> body)
    : bStatic(move(bStatic)), funcReturn(move(funcReturn)), name(move(name)), typeParams(move(typeParams)), parameters(move(parameters)), body(move(body)) { }

SImplFuncDecl::SImplFuncDecl(SImplFuncDecl&& other) noexcept = default;

SImplFuncDecl::~SImplFuncDecl() = default;

SImplFuncDecl& SImplFuncDecl::operator=(SImplFuncDecl&& other) noexcept = default;

JsonItem SImplFuncDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SImplFuncDecl") },
        { "bStatic", Citron::ToJson(bStatic) },
        { "funcReturn", Citron::ToJson(funcReturn) },
        { "name", Citron::ToJson(name) },
        { "typeParams", Citron::ToJson(typeParams) },
        { "parameters", Citron::ToJson(parameters) },
        { "body", Citron::ToJson(body) },
    };
}

JsonItem ToJson(SImplMemberDecl& memberDecl)
{
    return std::visit(ToJsonVisitor(), memberDecl);
}

SImplDecl::SImplDecl(std::string name, STypeExp* trait, std::vector<SImplMemberDecl> memberDecls)
    : name(move(name)), trait(move(trait)), memberDecls(move(memberDecls)) { }

SImplDecl::SImplDecl(SImplDecl&& other) noexcept = default;

SImplDecl::~SImplDecl() = default;

SImplDecl& SImplDecl::operator=(SImplDecl&& other) noexcept = default;

JsonItem SImplDecl::ToJson()
{
    return JsonObject {
        { "$type", JsonString("SImplDecl") },
        { "name", Citron::ToJson(name) },
        { "trait", Citron::ToJson(trait) },
        { "memberDecls", Citron::ToJson(memberDecls) },
    };
}

SNamespaceDecl::SNamespaceDecl(std::vector<std::string> names, std::vector<SNamespaceDeclElement> elements)
    : names(move(names)), elements(move(elements)) { }

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

SScript::SScript(std::vector<SScriptElement> elements)
    : elements(move(elements)) { }

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
