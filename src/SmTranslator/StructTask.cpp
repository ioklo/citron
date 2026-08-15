#include "StructTask.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RStructCtorDecl.h"

#include "CommonTranslation.h"
#include "SmPhaseManager.h"
#include "BuildTypeHierarchyContext.h"
#include "BuildImplicitSymbolContext.h"
#include "Misc.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"


using namespace std;

namespace Citron {

StructTask::StructTask(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructDecl* syntax)
    : structDeclContext{structDeclContext.Take()}, rStructDecl{rStructDecl}, syntax{syntax}
{
}

void StructTask::Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructDecl* syntax, SmPhaseManager& phaseManager)
{
    shared_ptr<StructTask> task{new StructTask(move(structDeclContext), rStructDecl, syntax)};
    phaseManager.AddBuildTypeHierarchyTask(task);
    phaseManager.AddBuildImplicitSymbolTask(task);
}

expected<void, DiagPtr> StructTask::BuildTypeHierarchy(BuildTypeHierarchyContext& context)
{
    // NOTICE: struct, class의 base부분을 볼때는 base 전용 type lookup을 해야한다 
    // (type parameter는 검색이 되지만, {멤버 타입/base타입의 멤버타입}은 검색이 안되게) => SmTypeResolveScope_DeclHeader를 만들었다
    SmTypeResolveScope_DeclHeader scope{structDeclContext.get(), rStructDecl->GetTypeParams()};

    vector<RAppliedDecl<RTraitDecl>> rTraits;
    for (auto* sTrait : syntax->traits)
    {
        auto e_rTrait = context.MakeTrait(sTrait, scope);
        RETURN_ON_ERROR(e_rTrait);

        rTraits.push_back(move(*e_rTrait));
    }

    rStructDecl->InitTraits(move(rTraits));
    return {};
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
    rCtor->Init(RDeclKey::Ctor(rParameters), vector<RTypeParam*>{}, move(rParameters), /*bLastParameterVariadic*/false);

    rStructDecl->AddCtor(rCtor);
}

} // namespace Citron
