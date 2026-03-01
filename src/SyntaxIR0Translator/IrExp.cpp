#include "IrExp.h"

#include <cassert>
#include "Infra/Ptr.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"

#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "MIR/MSharedExp.h"

#include "TranslationContexts.h"

using namespace std;

namespace Citron {

IrExp_Namespace::IrExp_Namespace(RNamespaceDecl* decl)
    : decl(decl)
{
}

IrExp_Class::IrExp_Class(RClassDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_Struct::IrExp_Struct(RStructDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_Exp::IrExp_Exp(MExp* exp)
    : exp{exp}
{

}

IrExp_Loc::IrExp_Loc(MLoc* loc)
    : loc{loc}
{
}

} // namespace Citron