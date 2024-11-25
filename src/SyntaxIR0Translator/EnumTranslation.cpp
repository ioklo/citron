#include "pch.h"
#include "EnumTranslation.h"

#include <Infra/Ptr.h>
#include <IR0/NEnumElemVarDecl.h>

#include "SkeletonPhaseContext.h"
#include "MemberDeclPhaseContext.h"


using namespace std;

namespace Citron::SyntaxIR0Translator {

void AddEnumElemVar(const shared_ptr<NEnumElemDecl>& rEnumElem, SEnumElemVarDecl& sEnumElemVar, SkeletonPhaseContext& context)
{
    auto nEnumElemVar = MakePtr<NEnumElemVarDecl>(rEnumElem, sEnumElemVar.name);
    rEnumElem->AddVar(nEnumElemVar);

    context.AddMemberDeclPhaseTask([type = sEnumElemVar.type, nEnumElemVar, rEnumElem](MemberDeclPhaseContext& context) {
        auto declType = context.MakeType(type, rEnumElem);
        nEnumElemVar->InitDeclType(std::move(declType));
    });
}

void AddEnumElem(const shared_ptr<NEnumDecl>& nEnum, SEnumElemDecl& sEnumElem, SkeletonPhaseContext& context)
{
    auto nEnumElem = MakePtr<NEnumElemDecl>(nEnum, sEnumElem.name, sEnumElem.vars.size());

    for (auto& sEnumElemVar : sEnumElem.vars)
        AddEnumElemVar(nEnumElem, *sEnumElemVar, context);

    nEnum->AddElem(std::move(nEnumElem));
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