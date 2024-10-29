#include "NEnumDecl.h"

namespace Citron
{

NEnumDecl::NEnumDecl(NTypeDeclOuterWPtr outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount)
    : outer(std::move(outer))
    , accessor(accessor)
    , name(std::move(name))
    , typeParams(std::move(typeParams))
{
    elems.reserve(elemCount);
}

void NEnumDecl::AddElem(std::shared_ptr<NEnumElemDecl> elem)
{
    elems.push_back(std::move(elem));
}

NDecl* NEnumDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NEnumDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

} // namespace Citron