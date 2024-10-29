#include "NModuleDecl.h"

using namespace std;

namespace Citron {

NModuleDecl::NModuleDecl(string name)
    : name(std::move(name))
{
}

NDecl* NModuleDecl::GetOuter()
{
    return nullptr;
}

RIdentifier NModuleDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

NDecl* NModuleDecl::GetDecl()
{
    return this;
}

}