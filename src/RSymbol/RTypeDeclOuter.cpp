#include "RTypeDeclOuter.h"
#include "RNamespaceDecl.h"
#include "RClassDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RDecl* RTypeDeclOuter::GetDecl()
{
    return visit([](auto&& arg) -> RDecl* { return arg.decl; }, v);
}

void RTypeDeclOuter::AddType(RTypeDecl* typeDecl)
{
    visit([typeDecl](auto&& arg) { arg.decl->AddType(typeDecl); }, v);
}

} // namespace Citron