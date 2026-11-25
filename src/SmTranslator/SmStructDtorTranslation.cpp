#include "SmStructDtorTranslation.h"
#include "Infra/Ptr.h"

#include "Syntax/Syntaxes.g.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructDtorDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"

#include "SmPhaseManager.h"
#include "SmPhase7Context.h"
#include "SmDeclContext_Decl.h"
#include "ISmPhaseTask.h"
#include "ISmPhase4Task.h"
#include "ISmPhase7Task.h"

using namespace std;

namespace Citron {
namespace {

class StructDtorTask
    : public ISmPhaseTask
    , public ISmPhase4Task
    , public ISmPhase7Task
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructDtorDecl* sStructDtor;
    RFactoryPtr rFactory;

    RStructDtorDecl* rStructDtor;

public:
    StructDtorTask(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRefPtr<RFactory> rFactory);
    std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) override;
    std::expected<MFuncBody, DiagPtr> RunPhase7(SmPhase7Context& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructDtor(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager)
{
    phaseManager.AddTask<StructDtorTask>(structDeclContext, rStruct, sStructDtor, rFactory);
}

namespace {

StructDtorTask::StructDtorTask(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRefPtr<RFactory> rFactory)
    : structDeclContext{structDeclContext.Share()}, rStruct{rStruct}, sStructDtor{sStructDtor}, rFactory{rFactory.Share()}, rStructDtor{nullptr}
{
}

expected<void, DiagPtr> StructDtorTask::RunPhase4(SmPhase4Context& context)
{
    auto accessor = MakeStructMemberAccessor(sStructDtor->accessModifier);
    rStructDtor = rFactory->MakeDecl<RStructDtorDecl>(RDeclKey::Dtor(), rStruct, accessor);
    return {};
}

std::expected<MFuncBody, DiagPtr> StructDtorTask::RunPhase7(SmPhase7Context& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RStructDtorDecl>>(structDeclContext, rStructDtor, rStructDtor->MakeOpenTypeArgs(*rFactory));
    return context.Translate(declContext, rStructDtor, /*bSeqFunc*/false, sStructDtor->body);
}

} // namespace
} // namespace Citron