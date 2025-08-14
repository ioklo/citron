#include "EnumTranslation.h"

#include "Infra/Ptr.h"
#include "IR0/RFactory.h"
#include "IR0/NEnumDecl.h"
#include "IR0/NEnumElemDecl.h"

#include "SkeletonPhaseContext.h"
#include "MemberDeclPhaseContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void AddEnumElemVar(NEnumElemDecl* rEnumElem, SEnumElemVarDecl* sEnumElemVar, SkeletonPhaseContext& context)
{
    auto* nEnumElemVar = context.MakeNDecl<NEnumElemVarDecl>(rEnumElem, sEnumElemVar->name);
    rEnumElem->AddVar(nEnumElemVar);

    context.AddMemberDeclPhaseTask([type = sEnumElemVar->type, nEnumElemVar, rEnumElem](MemberDeclPhaseContext& context) {
        auto declType = context.MakeType(type, rEnumElem);
        nEnumElemVar->InitDeclType(move(declType));
    });
}

void AddEnumElem(NEnumDecl* nEnum, SEnumElemDecl* sEnumElem, SkeletonPhaseContext& context)
{
    auto* nEnumElem = context.MakeNDecl<NEnumElemDecl>(nEnum, sEnumElem->name, sEnumElem->vars.size());

    for (auto* sEnumElemVar : sEnumElem->vars)
        AddEnumElemVar(nEnumElem, sEnumElemVar, context);

    nEnum->AddElem(move(nEnumElem));
}

NEnumDecl* InnerMakeEnum(NTypeDeclOuter* nOuter, SEnumDecl* sDecl, RAccessor accessor, SkeletonPhaseContext& context)
{
    auto typeParams = MakeTypeParams(sDecl->typeParams);
    auto* nDecl = context.MakeNDecl<NEnumDecl>(nOuter, accessor, RName_Normal(sDecl->name), typeParams, sDecl->elements.size());
    
    for (auto* sElemDecl : sDecl->elements)
        AddEnumElem(nDecl, sElemDecl, context);

    return nDecl;
}

}