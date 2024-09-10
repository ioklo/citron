#include "REnumDecl.h"

namespace Citron
{

REnumDecl::REnumDecl(RTypeDeclOuterPtr outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount)
    : outer(std::move(outer))
    , accessor(accessor)
    , name(std::move(name))
    , typeParams(std::move(typeParams))
{
    elems.reserve(elemCount);
}

void REnumDecl::AddElem(std::shared_ptr<REnumElemDecl> elem)
{
    elems.push_back(std::move(elem));
}

}