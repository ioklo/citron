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
#include "ISmPhase4Task.h"
#include "ISmPhase7Task.h"

using namespace std;

namespace Citron {
namespace {

class StructDtorTask
    : public ISmPhase4Task
    , public ISmPhase7Task
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructDtorDecl* sStructDtor;
    RFactoryPtr rFactory;

    RStructDtorDecl* rStructDtor;

public:
    StructDtorTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRef<RFactoryPtr> rFactory);
    std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) override;
    std::expected<MFuncBody, DiagPtr> RunPhase7(SmPhase7Context& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructDtor(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    auto task = MakePtr<StructDtorTask>(structDeclContext, rStruct, sStructDtor, rFactory);
    phaseManager.AddPhase4Task(task);
    phaseManager.AddPhase7Task(task);
}

namespace {

StructDtorTask::StructDtorTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRef<RFactoryPtr> rFactory)
    : structDeclContext{*structDeclContext}, rStruct{rStruct}, sStructDtor{sStructDtor}, rFactory{*rFactory}, rStructDtor{nullptr}
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