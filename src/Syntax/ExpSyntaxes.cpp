#include "pch.h"
#include <Syntax/ExpSyntaxes.h>
#include <Syntax/ArgumentSyntax.h>
#include <Syntax/StringExpElementSyntaxes.h>
#include <Syntax/StmtSyntaxes.h>

using namespace std;

namespace Citron {

StringExpSyntax::StringExpSyntax(vector<StringExpElementSyntax> elements)
    : elements(std::move(elements)) { }

IMPLEMENT_DEFAULTS_DEFAULT(StringExpSyntax)

struct BinaryOpExpSyntax::Impl 
{
    BinaryOpKindSyntax kind;
    ExpSyntax operand0;
    ExpSyntax operand1;
};

BinaryOpExpSyntax::BinaryOpExpSyntax(BinaryOpKindSyntax kind, ExpSyntax operand0, ExpSyntax operand1)
    : impl(new Impl { kind, std::move(operand0), std::move(operand1) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(BinaryOpExpSyntax)

BinaryOpKindSyntax BinaryOpExpSyntax::GetKind()
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

struct UnaryOpExpSyntax::Impl
{
    UnaryOpKindSyntax kind;
    ExpSyntax operand;
};

UnaryOpExpSyntax::UnaryOpExpSyntax(UnaryOpKindSyntax kind, ExpSyntax operand)
    : impl(new Impl { kind, std::move(operand) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(UnaryOpExpSyntax)

UnaryOpKindSyntax UnaryOpExpSyntax::GetKind()
{
    return impl->kind;
}

ExpSyntax& UnaryOpExpSyntax::GetOperand()
{
    return impl->operand;
}

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

LambdaExpSyntax::LambdaExpSyntax(vector<LambdaExpParamSyntax> params, vector<StmtSyntax> body)
    : params(std::move(params)), body(std::move(body)) { }

IMPLEMENT_DEFAULTS_DEFAULT(LambdaExpSyntax)

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

ListExpSyntax::ListExpSyntax(vector<ExpSyntax> elems)
    : elems(std::move(elems))
{

}

IMPLEMENT_DEFAULTS_DEFAULT(ListExpSyntax)

NewExpSyntax::NewExpSyntax(TypeExpSyntax type, vector<ArgumentSyntax> args)
    : type(std::move(type)), args(std::move(args)) { }

IMPLEMENT_DEFAULTS_DEFAULT(NewExpSyntax)


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

} // Citron::Syntax