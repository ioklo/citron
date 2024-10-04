#include "pch.h"
#include "EnumTranslation.h"

#include <Infra/Ptr.h>

#include "SkeletonPhaseContext.h"
#include "MemberDeclPhaseContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void AddEnumElemMemberVar(const shared_ptr<REnumElemDecl>& rEnumElem, SEnumElemMemberVarDecl& sEnumElemMemberVar, SkeletonPhaseContext& context)
{
    auto rMemberVar = MakePtr<REnumElemMemberVarDecl>(rEnumElem, RNormalName(sEnumElemMemberVar.name));
    rEnumElem->AddMemberVar(rMemberVar);

    context.AddMemberDeclPhaseTask([type = sEnumElemMemberVar.type, rMemberVar, rEnumElem](MemberDeclPhaseContext& context) {
        auto declType = context.MakeType(type, rEnumElem);
        rMemberVar->InitDeclType(std::move(declType));
    });
}

void AddEnumElem(const shared_ptr<REnumDecl>& rEnum, SEnumElemDecl& sEnumElem, SkeletonPhaseContext& context)
{
    auto rEnumElem = MakePtr<REnumElemDecl>(rEnum, sEnumElem.name, sEnumElem.memberVars.size());

    for (auto& sMemberVar : sEnumElem.memberVars)
        AddEnumElemMemberVar(rEnumElem, *sMemberVar, context);

    rEnum->AddElem(std::move(rEnumElem));
}

std::shared_ptr<REnumDecl> InnerMakeEnum(RTypeDeclOuterPtr rOuter, SEnumDecl& sDecl, RAccessor accessor, SkeletonPhaseContext& context)
{
    auto typeParams = MakeTypeParams(sDecl.typeParams);
    auto rDecl = MakePtr<REnumDecl>(std::move(rOuter), accessor, RNormalName(sDecl.name), typeParams, sDecl.elements.size());
    
    for (auto& sElemDecl : sDecl.elements)
        AddEnumElem(rDecl, *sElemDecl, context);

    return rDecl;
}

}