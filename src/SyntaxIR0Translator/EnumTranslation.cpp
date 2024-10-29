#include "pch.h"
#include "EnumTranslation.h"

#include <Infra/Ptr.h>
#include <IR0/NEnumElemMemberVarDecl.h>

#include "SkeletonPhaseContext.h"
#include "MemberDeclPhaseContext.h"


using namespace std;

namespace Citron::SyntaxIR0Translator {

void AddEnumElemMemberVar(const shared_ptr<NEnumElemDecl>& rEnumElem, SEnumElemMemberVarDecl& sEnumElemMemberVar, SkeletonPhaseContext& context)
{
    auto nMemberVar = MakePtr<NEnumElemMemberVarDecl>(rEnumElem, RName_Normal(sEnumElemMemberVar.name));
    rEnumElem->AddMemberVar(nMemberVar);

    context.AddMemberDeclPhaseTask([type = sEnumElemMemberVar.type, nMemberVar, rEnumElem](MemberDeclPhaseContext& context) {
        auto declType = context.MakeType(type, rEnumElem);
        nMemberVar->InitDeclType(std::move(declType));
    });
}

void AddEnumElem(const shared_ptr<NEnumDecl>& rEnum, SEnumElemDecl& sEnumElem, SkeletonPhaseContext& context)
{
    auto nEnumElem = MakePtr<NEnumElemDecl>(rEnum, sEnumElem.name, sEnumElem.memberVars.size());

    for (auto& sMemberVar : sEnumElem.memberVars)
        AddEnumElemMemberVar(nEnumElem, *sMemberVar, context);

    rEnum->AddElem(std::move(nEnumElem));
}

std::shared_ptr<NEnumDecl> InnerMakeEnum(NTypeDeclOuterWPtr nOuter, SEnumDecl& sDecl, RAccessor accessor, SkeletonPhaseContext& context)
{
    auto typeParams = MakeTypeParams(sDecl.typeParams);
    auto nDecl = MakePtr<NEnumDecl>(std::move(nOuter), accessor, RName_Normal(sDecl.name), typeParams, sDecl.elements.size());
    
    for (auto& sElemDecl : sDecl.elements)
        AddEnumElem(nDecl, *sElemDecl, context);

    return nDecl;
}

}