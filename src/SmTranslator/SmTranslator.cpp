#include "SmTranslator.h"

#include <stdexcept>
#include <memory>
#include <cassert>
#include <variant>
#include <ranges>

#include "Infra/Unreachable.h"
#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "RSymbol/RNamespace.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "RSymbol/RModule.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTraitDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MFuncBody.h"
#include "MIR/MFactory.h"

#include "BuildNonTypeSymbolContext.h"
#include "SmFactory.h"
#include "SmScriptTranslation.h"
#include "SmPhaseManager.h"
#include "CommonTranslation.h"
#include "BinOpQueryService.h"
#include "SmDeclContext_Decl.h"
#include "SmDeclContext_ClassDecl.h"

using namespace std;
using namespace Citron;

namespace Citron {

expected<SmTranslationResult, DiagPtr> TranslateSyntax(
    string moduleName,
    const vector<SScript*>& scripts, // translation units
    const vector<EModule*>& referenceModules,
    InRef<LoggerPtr> logger,
    InRef<RFactoryPtr> rFactory,
    InRef<MFactoryPtr> mFactory)
{
    // TODO: NewRootNamespaceDecl이 아니라 RootNamespaceGroupDecl이어야 할것 같고, 모듈은 rootNamespaceDeclGroup을 가져야 할 것 같다

    // 모듈은 모든 translation unit에 대해 하나만 갖는다. symbol tree도 하나고, namespace도 하나다
    auto* rModule = (*rFactory)->MakeModule(std::move(moduleName));
    auto* rRootNamespace = (*rFactory)->MakeRootNamespaceDecl(rModule, *rFactory);
    rModule->InitRootNamespace(rRootNamespace);

    auto smFactory = MakePtr<SmFactory>();
    auto binOpQueryService = MakePtr<BinOpQueryService>(**rFactory);
    
    SmPhaseManager phaseManager{*logger, *rFactory, *mFactory, smFactory, binOpQueryService};

    // translation units
    for (auto* script : scripts)
        SmTranslateScript(script, rModule, rFactory, phaseManager);

    auto e_funcBodies = phaseManager.Run();
    RETURN_ON_ERROR(e_funcBodies);

    auto* mData = (*mFactory)->MakeMData(move(*e_funcBodies));
    return SmTranslationResult{rModule, mData};
}

} // namespace Citron
