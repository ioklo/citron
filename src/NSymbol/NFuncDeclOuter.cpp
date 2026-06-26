#include "NFuncDeclOuter.h"

#include "NNamespaceDecl.h"
#include "NGlobalFuncDecl.h"
#include "NClassDecl.h"
#include "NClassCtorDecl.h"
#include "NClassFuncDecl.h"
#include "NStructDecl.h"
#include "NStructCtorDecl.h"
#include "NStructDtorDecl.h"
#include "NStructFuncDecl.h"
#include "NLambdaDecl.h"

using namespace std;

namespace Citron {

NDecl* NFuncDeclOuter::GetNDecl()
{
    return visit([](auto* decl) -> NDecl* { return decl; }, v);
}

} // namespace Citron