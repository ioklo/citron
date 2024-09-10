#include "RModuleDecl.h"

using namespace std;

namespace Citron {

RModuleDecl::RModuleDecl(string name)
    : name(std::move(name))
{
}

}