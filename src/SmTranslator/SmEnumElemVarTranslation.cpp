#include "SmEnumElemVarTranslation.h"
#include "Infra/Expected.h"

#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "SmPhaseManager.h"
#include "CommonTranslation.h"
#include "SmPhase4Context.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"
#include "SmTypeResolveScope.h"
#include "ISmPhaseTask.h"
#include "ISmPhase4Task.h"

using namespace std;

namespace Citron {

using SmDeclContextPtr = shared_ptr<class SmDeclContext>;

namespace {

class SmEnumElemVarTranslator
    : public ISmPhaseTask
    , public ISmPhase4Task
{
    SmDeclContextPtr enumElemDeclContext;
    REnumElemVarDecl* rEnumElemVar;
    SEnumElemVarDecl* sEnumElemVar;
    RFactoryPtr rFactory;

public:
    SmEnumElemVarTranslator(InRefPtr<SmDeclContext> enumElemDeclContext, REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar, InRefPtr<RFactory> rFactory)
        : enumElemDeclContext{enumElemDeclContext.Share()}, rEnumElemVar{rEnumElemVar}, sEnumElemVar{sEnumElemVar}, rFactory{rFactory.Share()}
    {
    }

    std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateEnumElemVar(InRefPtr<SmDeclContext> enumElemDeclContext, SEnumElemVarDecl* sEnumElemVar, REnumElemDecl* rEnumElem, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager)
{
    RName elemVarName{RName::Normal(sEnumElemVar->name)};
    auto* rEnumElemVar = rFactory->MakeDecl<REnumElemVarDecl>(RDeclKey::Normal(elemVarName), rEnumElem, move(elemVarName));

    phaseManager.AddTask<SmEnumElemVarTranslator>(enumElemDeclContext, rEnumElemVar, sEnumElemVar, rFactory);
}

namespace {

expected<void, DiagPtr> SmEnumElemVarTranslator::RunPhase4(SmPhase4Context& context)
{
    SmTypeTranslationContexts typeTranslationContexts{SmTypeResolveScope_DeclContext{enumElemDeclContext.get()}, rFactory.get()};

    // enum 기준으로 타입을 만든다
    auto e_rDeclType = TranslateSTypeExpToRType(sEnumElemVar->type, typeTranslationContexts);
    RETURN_ON_ERROR(e_rDeclType);

    rEnumElemVar->InitDeclType(*e_rDeclType);
    rEnumElemVar->GetEnumElem()->AddVar(rEnumElemVar);
    return {};
}

} // namespace
} // namespace Citron