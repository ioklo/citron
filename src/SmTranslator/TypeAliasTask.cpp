#include "TypeAliasTask.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RDeclKey.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeAliasDecl.h"
#include "SmPhaseManager.h"
#include "SmTypeResolveScope.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"

using namespace std;

namespace Citron {

TypeAliasTask::TypeAliasTask(TakeRef<SmDeclContextPtr> outerDeclContext, STypeAliasDecl* sTypeAliasDecl, RTypeAliasDecl* rTypeAliasDecl, TakeRef<RFactoryPtr> rFactory)
    : outerDeclContext{outerDeclContext.Take()}, sTypeAliasDecl{sTypeAliasDecl}, rTypeAliasDecl{rTypeAliasDecl}, rFactory{rFactory.Take()}
{
}

void TypeAliasTask::Register(TakeRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, STypeAliasDecl* sTypeAliasDecl, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName name = RName::Normal(sTypeAliasDecl->name);
    auto* rTypeAliasDecl = (*rFactory)->MakeDecl<RTypeAliasDecl>(RDeclKey::Normal(name), outer, move(name));
    outer.AddType(rTypeAliasDecl);

    shared_ptr<TypeAliasTask> task{new TypeAliasTask{outerDeclContext.Take(), sTypeAliasDecl, rTypeAliasDecl, rFactory.Take()}};
    phaseManager.AddBuildTypeHierarchyTask(move(task));
}

expected<void, DiagPtr> TypeAliasTask::BuildTypeHierarchy(BuildTypeHierarchyContext& context)
{
    SmTypeTranslationContexts contexts{SmTypeResolveScope_DeclContext{outerDeclContext.get()}, rFactory.get()};
    auto e_targetType = TranslateSTypeExpToRType(sTypeAliasDecl->targetType, contexts);
    RETURN_ON_ERROR(e_targetType);

    rTypeAliasDecl->InitTargetType(*e_targetType);
    return {};
}

} // namespace Citron
