#include "StructTask.h"

#include "Infra/Exceptions.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RStructCtorDecl.h"

#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "BuildTypeHierarchyContext.h"
#include "BuildImplicitSymbolContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

StructTask::StructTask(RStructDecl* rStructDecl, SStructDecl* syntax)
    : rStructDecl{rStructDecl}, syntax{syntax}
{
}

void StructTask::Register(RStructDecl* rStructDecl, SStructDecl* syntax, PhaseManager& phaseManager)
{
    shared_ptr<StructTask> task{new StructTask(rStructDecl, syntax)};
    phaseManager.AddBuildTypeHierarchyTask(task);
    phaseManager.AddBuildImplicitSymbolTask(task);
}

void StructTask::BuildTypeHierarchy(BuildTypeHierarchyContext& context)
{
    // NOTICE: struct, class의 base부분을 볼때는 base 전용 type lookup을 해야한다 
    // (type parameter는 검색이 되지만, {멤버 타입/base타입의 멤버타입}은 검색이 안되게)

    // TODO: [66] 2026-07-09, Trait, Extend 구현

    //// 유일한 베이스 타입은 struct인데, 외부에서 선언된 struct일수도 있고, 조합 타입일 수도 있다 (사실 조합타입이 될 가능성은 거의 없어보인다)
    //RType_Struct* rBaseStruct = nullptr; // nullable

    //// 나머지는 interface들이다
    //vector<RType*> rInterfaces;

    //for (auto* sType : syntax->baseTypes)
    //{
    //    auto* rType = context.MakeType(sType, rStructDecl);
    //    auto rTypeKind = rType->GetTypeKind();

    //    if (auto* rStructType = dynamic_cast<RType_Struct*>(rType))
    //    {
    //        // 두개 이상의 struct를 상속받으려고 했다면, 에러 처리
    //        if (rBaseStruct != nullptr)
    //            throw NotImplementedException{};

    //        rBaseStruct = rStructType;
    //    }
    //    else if (rTypeKind == RTypeKind::Interface)
    //    {   
    //        rInterfaces.push_back(rType);
    //    }
    //    else
    //    {
    //        // 다른 타입은 struct의 basetype자리에 올 수 없습니다 에러 출력
    //        throw NotImplementedException{};
    //    }
    //}

    //rStructDecl->InitBaseTypes(rBaseStruct, move(rInterfaces));
}

void StructTask::BuildImplicitSymbol(BuildImplicitSymbolContext& context)
{
    SynthesizeMemberwiseCtor(context);
}

void StructTask::SynthesizeMemberwiseCtor(BuildImplicitSymbolContext& context)
{
    // memberwise constructor, 시그니처만 만든다 (resolve identifier용)
    vector<RFuncParameter> rParameters;
    
    for (auto* rVarDecl : rStructDecl->GetUnboundVars())
        rParameters.emplace_back(RFuncParameterKind::Init, rVarDecl->GetUnboundDeclType(), rVarDecl->GetName());

    auto* rCtor = context.MakeRDecl<RStructCtorDecl>(rStructDecl, RStructMemberAccessor::Public, RStructCtorKind::Memberwise);
    rCtor->InitFuncParameters(move(rParameters), /*bLastParameterVariadic*/false);
    rStructDecl->AddCtor(rCtor);
}

} // namespace Citron
