#include "SmStructFuncTranslation.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "SmPhase4Context.h"
#include "SmPhase7Context.h"
#include "CommonTranslation.h"
#include "SmPhaseManager.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "ISmPhaseTask.h"
#include "ISmPhase4Task.h"
#include "ISmPhase7Task.h"

using namespace std;

namespace Citron {
namespace {

class StructFuncTask
    : public ISmPhaseTask
    , public ISmPhase4Task
    , public ISmPhase7Task
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructFuncDecl* sStructFunc;
    RFactoryPtr rFactory;

    RStructFuncDecl* rStructFunc;

public:
    StructFuncTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructFuncDecl* sStructFunc, InRef<RFactoryPtr> rFactory)
        : structDeclContext{*structDeclContext}, rStruct{rStruct}, sStructFunc{sStructFunc}, rFactory{*rFactory}, rStructFunc{nullptr}
    {
    }

    std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) override;
    std::expected<MFuncBody, DiagPtr> RunPhase7(SmPhase7Context& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructFunc(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructFuncDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    phaseManager.AddTask<StructFuncTask>(*structDeclContext, rStructDecl, syntax, *rFactory);
}

namespace {

expected<void, DiagPtr> StructFuncTask::RunPhase4(SmPhase4Context& context)
{
    auto accessor = MakeStructMemberAccessor(sStructFunc->accessModifier);

    rStructFunc = rFactory->MakeDecl<RStructFuncDecl>(
        rStruct, accessor, RName::Normal(sStructFunc->name), sStructFunc->bSequence);

    // typeParam을 만들땐 rStructFunc가 필요하다 (rStructFunc는 tree에 매달려있지 않은 상태라도 상관없다)
    auto typeParams = MakeTypeParams(rStruct->GetAllTypeParamCount(), rStructFunc, sStructFunc->typeParams, rFactory);

    // 만들어진 typeParam도 검색대상이다
    SmTypeResolveScope_DeclHeader scope{structDeclContext.get(), typeParams};
    auto e_funcRet = context.MakeFuncReturn(sStructFunc->funcRet, rStructFunc, typeParams, scope);
    RETURN_ON_ERROR(e_funcRet);

    auto e_parameters = context.MakeFuncParameters(sStructFunc->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);

    RThisKind thisKind = [this]() -> RThisKind {
        if (sStructFunc->bStatic) return RThisKind_Static{};
        auto* structType = rFactory->MakeStructType(RAppliedDecl<RStructDecl>{rStruct, rStruct->MakeOpenTypeArgs(*rFactory)});
        return RThisKind_Ref{structType};
        }();

    // init
    rStructFunc->Init(RDeclKey::Func(RName::Normal(sStructFunc->name), parameters), move(thisKind), move(*e_funcRet), move(typeParams), move(parameters), bLastParamVariadic);

    // AddFunc할땐 declKey가 필요하다. declKey는 FuncParamter가 필요하다
    rStruct->AddFunc(rStructFunc);
    return {};
}

expected<MFuncBody, DiagPtr> StructFuncTask::RunPhase7(SmPhase7Context& context)
{
    SmDeclContextPtr structFuncDeclContext = MakePtr<SmDeclContext_Decl<RStructFuncDecl>>(structDeclContext, rStructFunc, rStructFunc->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(structFuncDeclContext), rStructFunc, sStructFunc->bSequence, sStructFunc->body);
}

} // namespace
} // namespace Citron