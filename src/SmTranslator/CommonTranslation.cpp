#include "CommonTranslation.h"

#include <ranges>

#include "Infra/Exceptions.h"
#include "RSymbol/RAccessor.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeParam.h"

using namespace std;

namespace Citron {

RNamespaceMemberAccessor MakeNamespaceMemberAccessor(optional<SAccessModifier> modifier)
{
    if (!modifier) return RNamespaceMemberAccessor::Private;

    switch (*modifier)
    {
    case SAccessModifier::Public: return RNamespaceMemberAccessor::Public;
    case SAccessModifier::Private: throw NotImplementedException{};
    case SAccessModifier::Protected: throw NotImplementedException{};
    }

    unreachable();
}

RStructMemberAccessor MakeStructMemberAccessor(optional<SAccessModifier> accessModifier) // throws FatalException
{
    if (!accessModifier) return RStructMemberAccessor::Public;

    switch (*accessModifier)
    {
    case SAccessModifier::Private: return RStructMemberAccessor::Private;
    case SAccessModifier::Protected: throw NotImplementedException{};
    case SAccessModifier::Public: throw NotImplementedException{};
    }

    unreachable();
}

// rDecl이 아직 tree에 매달려있지 않아도 되고, 대신 baseIndex를 따로 계산할것을 요구한다
vector<RTypeParam*> MakeTypeParams(size_t baseIndex, RDecl* rDecl, const vector<STypeParam>& sTypeParams, InRef<RFactoryPtr> rFactory)
{
    assert(rDecl);

    vector<RTypeParam*> nTypeParams;
    size_t count = sTypeParams.size();
    nTypeParams.reserve(count);
    for (size_t i = 0; i < count; i++)
    {
        auto& sTypeParam = sTypeParams[i];
        auto* nTypeParam = (*rFactory)->MakeTypeParam(rDecl, RName_Normal{sTypeParam.name}, baseIndex + i, *rFactory);
        nTypeParams.push_back(nTypeParam);
    }

    return nTypeParams;
}

}
