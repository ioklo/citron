#include "CommonTranslation.h"

#include <ranges>

#include "Infra/Exceptions.h"
#include "RSymbol/RAccessor.h"
#include "RSymbol/RDecl.h"

#include "NSymbol/NFactory.h"
#include "NSymbol/NDecl.h"
#include "NSymbol/NTypeParamDecl.h"

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

vector<NTypeParamDecl*> MakeTypeParams(NDecl* outer, const vector<STypeParam>& sTypeParams, NFactory& nFactory)
{
    assert(outer);
    size_t baseIndex = outer->GetRDecl()->GetAllTypeParamCount();

    vector<NTypeParamDecl*> nTypeParams;
    size_t count = sTypeParams.size();
    nTypeParams.reserve(count);
    for (size_t i = 0; i < count; i++)
    {
        auto& sTypeParam = sTypeParams[i];
        auto* nTypeParam = nFactory.MakeNDecl<NTypeParamDecl>(outer, RName_Normal{sTypeParam.name}, baseIndex + i);
        nTypeParams.push_back(nTypeParam);
    }

    return nTypeParams;
}

}
