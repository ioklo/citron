#include "RFuncDeclOuter.h"
#include "RNamespaceDecl.h"
#include "RGlobalFuncDecl.h"
#include "RClassDecl.h"
#include "RClassCtorDecl.h"
#include "RClassFuncDecl.h"
#include "RStructDecl.h"
#include "RStructCtorDecl.h"
#include "RStructDtorDecl.h"
#include "RStructFuncDecl.h"
#include "RLambdaDecl.h"

using namespace std;

namespace Citron {

RDecl* GetRDecl(RFuncDeclOuter& outer)
{
    return visit([](auto* outer) -> RDecl* { return outer; }, outer);
}

} // namespace Citron