#include "IrExpAndMemberNameTranslation.h"

#include <cassert>

#include "Logging/Diag.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "MIR/MFactory.h"
#include "MIR/MSharedExp.h"

#include "IrExp.h"
#include "Misc.h"
#include "TranslationContexts.h"


using namespace std;

namespace Citron{

expected<Result_GetClassVar, DiagPtr> GetClassVar(RType_Class* classType, const RName& name, RTypeArguments* typeArgsExceptOuter, bool bExpectedStatic)
{
    size_t typeArgsExceptOuterCount = typeArgsExceptOuter->GetCount();
    auto o_member = classType->GetMember(name, typeArgsExceptOuterCount);
    if (!o_member) return Error<Error_ResolveIdentifier_NotFound>();

    auto* classVarMember = get_if<RDeclRes_ClassVar>(&*o_member);
    if (!classVarMember) return Error<>();

    // static 성질이 다르면 에러    
    if (classVarMember->decl->IsStatic() != bExpectedStatic) return Error<>();

    // ClassVar이니까. classVarMember->typeArgs와 typeArgsExceptOuter를 합쳐서 쓰지 않고, classVarMember->typeArgs만 사용한다.
    assert(typeArgsExceptOuterCount == 0);
    return Result_GetClassVar{classVarMember->decl, classVarMember->typeArgs};
}

expected<Result_GetStructVar, DiagPtr> GetStructVar(RType_Struct* structType, const RName& name, RTypeArguments* typeArgsExceptOuter, bool bExpectedStatic)
{
    size_t typeArgsExceptOuterCount = typeArgsExceptOuter->GetCount();
    auto o_member = structType->GetMember(name, typeArgsExceptOuterCount);
    if (!o_member) return Error<Error_ResolveIdentifier_NotFound>();

    auto* structVarMember = get_if<RDeclRes_StructVar>(&*o_member);
    if (!structVarMember) return Error<>();

    // static 이면 에러
    if (structVarMember->decl->IsStatic() == bExpectedStatic) return Error<>();
    assert(typeArgsExceptOuterCount == 0);

    return Result_GetStructVar{structVarMember->decl, structVarMember->typeArgs};
}

} // namespace Citron