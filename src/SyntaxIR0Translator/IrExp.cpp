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

IrExp_TypeVar::IrExp_TypeVar(RType_TypeVar* type)
    : type(type)
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

IrExp_LocalValue::IrExp_LocalValue(MExp* exp)
    : exp{exp}
{

}

IrExp_SharedDeref::IrExp_SharedDeref(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* IrExp_SharedRef::GetTargetType()
{
    auto* sharedExpType = dynamic_cast<RType_Shared*>(sharedExp->GetType());
    assert(sharedExpType);

    return sharedExpType->innerType;
}

} // Citron::SyntaxIR0Translator