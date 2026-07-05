#include "RTypeDeclOuter.h"
#include "RNamespaceDecl.h"
#include "RClassDecl.h"
#include "RStructDecl.h"

namespace Citron {

RDecl* RTypeDeclOuter::GetDecl()
{
    return std::visit([](auto&& arg) -> RDecl* { return arg.decl; }, v);
}
} // namespace Citron