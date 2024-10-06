#include "RModuleDecl.h"

using namespace std;

namespace Citron {

RModuleDecl::RModuleDecl(string name)
    : name(std::move(name))
{
}

RDecl* RModuleDecl::GetOuter()
{
    return nullptr;
}

RIdentifier RModuleDecl::GetIdentifier()
{
    return RIdentifier { RNormalName(name), 0, {} };
}

RDecl* RModuleDecl::GetDecl()
{
    return this;
}

}