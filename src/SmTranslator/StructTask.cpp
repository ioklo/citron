#include "StructTask.h"

#include "Infra/Exceptions.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NStructDecl.h"

#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "ResolveTypeHierarchyContext.h"
#include "SynthesizeImplicitSymbolContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

StructTask::StructTask(NStructDecl* nStructDecl, SStructDecl* syntax, AccessorContext accessorContext)
    : nStructDecl{nStructDecl}, syntax{syntax}, accessorContext {accessorContext}
{
}

void StructTask::Register(NStructDecl* nStructDecl, SStructDecl* syntax, AccessorContext accessorContext, PhaseManager& phaseManager)
{
    shared_ptr<StructTask> task{new StructTask(nStructDecl, syntax, accessorContext)};
    phaseManager.AddResolveTypeHierarchyTask(task);
    phaseManager.AddSynthesizeImplicitSymbolTask(task);
}

void StructTask::ResolveTypeHierarchy(ResolveTypeHierarchyContext& context)
{
    // 유일한 베이스 타입은 struct인데, 외부에서 선언된 struct일수도 있고, 조합 타입일 수도 있다 (사실 조합타입이 될 가능성은 거의 없어보인다)
    RType_Struct* rBaseStruct = nullptr; // nullable

    // 나머지는 interface들이다
    vector<RType*> rInterfaces;

    for (auto* sType : syntax->baseTypes)
    {
        auto* rType = context.MakeType(sType, nStructDecl);
        auto rTypeKind = rType->GetTypeKind();

        if (auto* rStructType = dynamic_cast<RType_Struct*>(rType))
        {
            // 두개 이상의 struct를 상속받으려고 했다면, 에러 처리
            if (rBaseStruct != nullptr)
                throw NotImplementedException{};

            rBaseStruct = rStructType;
        }
        else if (rTypeKind == RTypeKind::Interface)
        {   
            rInterfaces.push_back(rType);
        }
        else
        {
            // 다른 타입은 struct의 basetype자리에 올 수 없습니다 에러 출력
            throw NotImplementedException{};
        }
    }

    nStructDecl->InitBaseTypes(rBaseStruct, move(rInterfaces));
}

void StructTask::SynthesizeImplicitSymbol(SynthesizeImplicitSymbolContext& context)
{
    SynthesizeMemberwiseCtor(context);
}

void GatherAllBaseVarDecls(vector<RFuncParameter>& params, RType_Struct* structType)
{
    // 먼저 base부터
    if (auto* baseType = structType->decl->GetUnboundBaseStruct())
        GatherAllBaseVarDecls(params, baseType);

    // 그리고 자기 자신
    for (auto* varDecl : structType->decl->GetRVars())
    {
        auto* declType = varDecl->GetDeclType(structType->typeArgs);
        RName_CtorParam name{params.size(), RNameToString(varDecl->GetIdentifier().name)};
        params.emplace_back(RFuncParameterKind::Init, declType, move(name));
    }
}

void StructTask::SynthesizeMemberwiseCtor(SynthesizeImplicitSymbolContext& context)
{
    // memberwise constructor, 시그니처만 만든다 (resolve identifier용)
    vector<RFuncParameter> rParameters;

    // 모든 base의 멤버 순회
    if (auto* baseStruct = nStructDecl->GetUnboundBaseStruct())
        GatherAllBaseVarDecls(rParameters, baseStruct);

    for (auto* nVarDecl : nStructDecl->GetVars())
        rParameters.emplace_back(RFuncParameterKind::Init, nVarDecl->GetUnboundDeclType(), RName_Normal{nVarDecl->name});

    auto* nCtor = context.MakeNDecl<NStructCtorDecl>(nStructDecl, RAccessor::Public, RStructCtorKind::Memberwise);
    nCtor->InitFuncParameters(move(rParameters), /*bLastParameterVariadic*/false);
    nStructDecl->AddCtor(nCtor);
}


} // namespace Citron
