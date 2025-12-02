#include "CommonTranslation.h"

#include <ranges>

#include "Infra/Exceptions.h"
#include "RSymbol/RAccessor.h"

using namespace std;

namespace Citron {

RAccessor MakeGlobalMemberAccessor(optional<SAccessModifier> modifier)
{
    if (!modifier) return RAccessor::Private;

    switch (*modifier)
    {
    case SAccessModifier::Public: return RAccessor::Public;
    case SAccessModifier::Private: throw NotImplementedException{};
    case SAccessModifier::Protected: throw NotImplementedException{};
    }

    unreachable();
}

RAccessor MakeStructMemberAccessor(optional<SAccessModifier> accessModifier) // throws FatalException
{
    if (!accessModifier) return RAccessor::Public;

    switch (*accessModifier)
    {
    case SAccessModifier::Private: return RAccessor::Private;
    case SAccessModifier::Protected: throw NotImplementedException{};
    case SAccessModifier::Public: throw NotImplementedException{};
    }

    unreachable();
}

RAccessor MakeAccessor(optional<SAccessModifier> modifier, AccessorContext context)
{
    switch(context)
    {
    case AccessorContext::Global: return MakeGlobalMemberAccessor(modifier);
    case AccessorContext::InsideClass: throw NotImplementedException{};
    case AccessorContext::InsideStruct: return MakeStructMemberAccessor(modifier);
    }

    unreachable();
}

vector<string> MakeTypeParams(const vector<STypeParam>& sTypeParams)
{
    return sTypeParams
        | views::transform([](const STypeParam& typeParam) { return typeParam.name; })
        | ranges::to<std::vector>();
}

}
