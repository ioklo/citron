#include "NStructDtorDecl.h"

namespace Citron {

NStructDtorDecl::NStructDtorDecl(RAccessor accessor, NStructDecl* structDecl)
    : accessor{accessor}, structDecl{structDecl}, NCommonFuncDeclComponent{false, false, {}}
{
}

} // namespace Citron