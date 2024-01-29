#include "pch.h"

#include <Infra/Json.h>

#include <Syntax/ExpSyntaxes.h>
#include <Syntax/ArgumentSyntax.h>
#include <Syntax/StringExpSyntaxElements.h>
#include <Syntax/StmtSyntaxes.h>

using namespace std;

namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(IdentifierExpSyntax)
    IMPLEMENT_JSON_MEMBER(value)
    IMPLEMENT_JSON_MEMBER(typeArgs)
END_IMPLEMENT_JSON_CLASS()

StringExpSyntax::StringExpSyntax(vector<StringExpSyntaxElement> elements)
    : elements(std::move(elements)) { }

StringExpSyntax::StringExpSyntax(u32string str)
    : elements({ TextStringExpSyntaxElement{str} })
{
}


IMPLEMENT_DEFAULTS_DEFAULT(StringExpSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(StringExpSyntax)
    IMPLEMENT_JSON_MEMBER(elements)        
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(IntLiteralExpSyntax)
    IMPLEMENT_JSON_MEMBER(value)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(BoolLiteralExpSyntax)
    IMPLEMENT_JSON_MEMBER(value)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(NullLiteralExpSyntax)
END_IMPLEMENT_JSON_CLASS()


struct BinaryOpExpSyntax::Impl 
{
    BinaryOpSyntaxKind kind;
    ExpSyntax operand0;
    ExpSyntax operand1;
};

BinaryOpExpSyntax::BinaryOpExpSyntax(BinaryOpSyntaxKind kind, ExpSyntax operand0, ExpSyntax operand1)
    : impl(new Impl { kind, std::move(operand0), std::move(operand1) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(BinaryOpExpSyntax)

BinaryOpSyntaxKind BinaryOpExpSyntax::GetKind()
{
    return impl->kind;
}

ExpSyntax& BinaryOpExpSyntax::GetOperand0()
{
    return impl->operand0;
}

ExpSyntax& BinaryOpExpSyntax::GetOperand1()
{
    return impl->operand1;
}

BEGIN_IMPLEMENT_JSON_CLASS(BinaryOpExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, kind)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, operand0)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, operand1)
END_IMPLEMENT_JSON_CLASS()

struct UnaryOpExpSyntax::Impl
{
    UnaryOpSyntaxKind kind;
    ExpSyntax operand;
};

UnaryOpExpSyntax::UnaryOpExpSyntax(UnaryOpSyntaxKind kind, ExpSyntax operand)
    : impl(new Impl { kind, std::move(operand) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(UnaryOpExpSyntax)

UnaryOpSyntaxKind UnaryOpExpSyntax::GetKind()
{
    return impl->kind;
}

ExpSyntax& UnaryOpExpSyntax::GetOperand()
{
    return impl->operand;
}

BEGIN_IMPLEMENT_JSON_CLASS(UnaryOpExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, kind)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, operand)
END_IMPLEMENT_JSON_CLASS()

struct CallExpSyntax::Impl
{
    ExpSyntax callable;
    vector<ArgumentSyntax> args;
};

CallExpSyntax::CallExpSyntax(ExpSyntax callable, vector<ArgumentSyntax> args)
    : impl(new Impl { std::move(callable), std::move(args)})
{
}

IMPLEMENT_DEFAULTS_PIMPL(CallExpSyntax)

ExpSyntax& CallExpSyntax::GetCallable()
{
    return impl->callable;
}

vector<ArgumentSyntax>& CallExpSyntax::GetArgs()
{
    return impl->args;
}

BEGIN_IMPLEMENT_JSON_CLASS(CallExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, callable)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, args)
END_IMPLEMENT_JSON_CLASS()


BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(LambdaExpParamSyntax, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, type)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, name)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, hasOut)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, hasParams)
END_IMPLEMENT_JSON_STRUCT_INLINE()

LambdaExpSyntax::LambdaExpSyntax(vector<LambdaExpParamSyntax> params, vector<StmtSyntax> body)
    : params(std::move(params)), body(std::move(body)) { }

IMPLEMENT_DEFAULTS_DEFAULT(LambdaExpSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(LambdaExpSyntax)
    IMPLEMENT_JSON_MEMBER(params)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

struct IndexerExpSyntax::Impl
{
    ExpSyntax obj;
    ExpSyntax index;
};

IndexerExpSyntax::IndexerExpSyntax(ExpSyntax obj, ExpSyntax index)
    : impl(new Impl { std::move(obj), std::move(index) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IndexerExpSyntax)

ExpSyntax& IndexerExpSyntax::GetObject()
{
    return impl->obj;
}

ExpSyntax& IndexerExpSyntax::GetIndex()
{
    return impl->index;
}

BEGIN_IMPLEMENT_JSON_CLASS(IndexerExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, obj)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, index)
END_IMPLEMENT_JSON_CLASS()

struct MemberExpSyntax::Impl
{
    ExpSyntax parent;
    u32string memberName;
    vector<TypeExpSyntax> memberTypeArgs;
};

MemberExpSyntax::MemberExpSyntax(ExpSyntax parent, u32string memberName, vector<TypeExpSyntax> memberTypeArgs)
    : impl(new Impl { std::move(parent), std::move(memberName), std::move(memberTypeArgs) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(MemberExpSyntax)

ExpSyntax& MemberExpSyntax::GetParent()
{
    return impl->parent;
}

u32string& MemberExpSyntax::GetMemberName()
{
    return impl->memberName;
}

vector<TypeExpSyntax>& MemberExpSyntax::GetMemberTypeArgs()
{
    return impl->memberTypeArgs;
}

BEGIN_IMPLEMENT_JSON_CLASS(MemberExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, parent)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, memberName)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, memberTypeArgs)
END_IMPLEMENT_JSON_CLASS()

ListExpSyntax::ListExpSyntax(vector<ExpSyntax> elems)
    : elems(std::move(elems))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(ListExpSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(ListExpSyntax)
    IMPLEMENT_JSON_MEMBER(elems)
END_IMPLEMENT_JSON_CLASS()

NewExpSyntax::NewExpSyntax(TypeExpSyntax type, vector<ArgumentSyntax> args)
    : type(std::move(type)), args(std::move(args)) { }

IMPLEMENT_DEFAULTS_DEFAULT(NewExpSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(NewExpSyntax)
    IMPLEMENT_JSON_MEMBER(type)
    IMPLEMENT_JSON_MEMBER(args)
END_IMPLEMENT_JSON_CLASS()

struct BoxExpSyntax::Impl
{
    ExpSyntax innerExp;
};

BoxExpSyntax::BoxExpSyntax(ExpSyntax innerExp)
    :impl(new Impl{ std::move(innerExp) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(BoxExpSyntax)

ExpSyntax& BoxExpSyntax::GetInnerExp()
{
    return impl->innerExp;
}

BEGIN_IMPLEMENT_JSON_CLASS(BoxExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, innerExp)
END_IMPLEMENT_JSON_CLASS()

struct IsExpSyntax::Impl
{
    ExpSyntax exp;
    TypeExpSyntax type;
};

IsExpSyntax::IsExpSyntax(ExpSyntax exp, TypeExpSyntax type)
    : impl(new Impl { std::move(exp), std::move(type) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IsExpSyntax)

ExpSyntax& IsExpSyntax::GetExp()
{
    return impl->exp;
}

TypeExpSyntax& IsExpSyntax::Type()
{
    return impl->type;
}

BEGIN_IMPLEMENT_JSON_CLASS(IsExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, exp)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, type)
END_IMPLEMENT_JSON_CLASS()

struct AsExpSyntax::Impl
{
    ExpSyntax exp;
    TypeExpSyntax type;
};

AsExpSyntax::AsExpSyntax(ExpSyntax exp, TypeExpSyntax type)
    : impl(new Impl { std::move(exp), std::move(type) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(AsExpSyntax)

ExpSyntax& AsExpSyntax::GetExp()
{
    return impl->exp;
}

TypeExpSyntax& AsExpSyntax::Type()
{
    return impl->type;
}

BEGIN_IMPLEMENT_JSON_CLASS(AsExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, exp)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, type)
END_IMPLEMENT_JSON_CLASS()

JsonItem ToJson(ExpSyntax& exp)
{
    return std::visit([](auto&& exp) { return exp.ToJson(); }, exp);
}


} // Citron::Syntax