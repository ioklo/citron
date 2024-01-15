#include "pch.h"
#include "Syntax/Exps.h"
#include "Syntax/Argument.h"
#include "Syntax/StringExpElements.h"
#include "Syntax/Stmts.h"

using namespace std;

namespace Citron::Syntax {

StringExp::StringExp(vector<StringExpElement> elements)
    : elements(std::move(elements)) { }

IMPLEMENT_DEFAULTS_DEFAULT(StringExp)

struct BinaryOpExp::Impl 
{
    BinaryOpKind kind;
    Exp operand0;
    Exp operand1;
};

BinaryOpExp::BinaryOpExp(BinaryOpKind kind, Exp operand0, Exp operand1)
    : impl(new Impl { kind, std::move(operand0), std::move(operand1) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(BinaryOpExp)

BinaryOpKind BinaryOpExp::GetKind()
{
    return impl->kind;
}

Exp& BinaryOpExp::GetOperand0()
{
    return impl->operand0;
}

Exp& BinaryOpExp::GetOperand1()
{
    return impl->operand1;
}

struct UnaryOpExp::Impl
{
    UnaryOpKind kind;
    Exp operand;
};

UnaryOpExp::UnaryOpExp(UnaryOpKind kind, Exp operand)
    : impl(new Impl { kind, std::move(operand) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(UnaryOpExp)

UnaryOpKind UnaryOpExp::GetKind()
{
    return impl->kind;
}

Exp& UnaryOpExp::GetOperand()
{
    return impl->operand;
}

struct CallExp::Impl
{
    Exp callable;
    vector<Argument> args;
};

CallExp::CallExp(Exp callable, vector<Argument> args)
    : impl(new Impl { std::move(callable), std::move(args)})
{
}

IMPLEMENT_DEFAULTS_PIMPL(CallExp)

Exp& CallExp::GetCallable()
{
    return impl->callable;
}

vector<Argument>& CallExp::GetArgs()
{
    return impl->args;
}

LambdaExp::LambdaExp(vector<LambdaExpParam> params, vector<Stmt> body)
    : params(std::move(params)), body(std::move(body)) { }

IMPLEMENT_DEFAULTS_DEFAULT(LambdaExp)

struct IndexerExp::Impl
{
    Exp obj;
    Exp index;
};

IndexerExp::IndexerExp(Exp obj, Exp index)
    : impl(new Impl { std::move(obj), std::move(index) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IndexerExp)

Exp& IndexerExp::GetObject()
{
    return impl->obj;
}

Exp& IndexerExp::GetIndex()
{
    return impl->index;
}

struct MemberExp::Impl
{
    Exp parent;
    string memberName;
    vector<TypeExp> memberTypeArgs;
};

MemberExp::MemberExp(Exp parent, string memberName, vector<TypeExp> memberTypeArgs)
    : impl(new Impl { std::move(parent), std::move(memberName), std::move(memberTypeArgs) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(MemberExp)

Exp& MemberExp::GetParent()
{
    return impl->parent;
}

string& MemberExp::GetMemberName()
{
    return impl->memberName;
}

vector<TypeExp>& MemberExp::GetMemberTypeArgs()
{
    return impl->memberTypeArgs;
}

ListExp::ListExp(vector<Exp> elems)
    : elems(std::move(elems))
{

}

IMPLEMENT_DEFAULTS_DEFAULT(ListExp)

NewExp::NewExp(TypeExp type, vector<Argument> args)
    : type(std::move(type)), args(std::move(args)) { }

IMPLEMENT_DEFAULTS_DEFAULT(NewExp)


struct BoxExp::Impl
{
    Exp innerExp;
};

BoxExp::BoxExp(Exp innerExp)
    :impl(new Impl{ std::move(innerExp) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(BoxExp)

Exp& BoxExp::GetInnerExp()
{
    return impl->innerExp;
}

struct IsExp::Impl
{
    Exp exp;
    TypeExp type;
};

IsExp::IsExp(Exp exp, TypeExp type)
    : impl(new Impl { std::move(exp), std::move(type) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IsExp)

Exp& IsExp::GetExp()
{
    return impl->exp;
}

TypeExp& IsExp::Type()
{
    return impl->type;
}

struct AsExp::Impl
{
    Exp exp;
    TypeExp type;
};

AsExp::AsExp(Exp exp, TypeExp type)
    : impl(new Impl { std::move(exp), std::move(type) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(AsExp)

Exp& AsExp::GetExp()
{
    return impl->exp;
}

TypeExp& AsExp::Type()
{
    return impl->type;
}

} // Citron::Syntax