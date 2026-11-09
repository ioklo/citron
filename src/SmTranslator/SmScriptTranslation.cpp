#include "SmScriptTranslation.h"
#include <memory>
#include <cassert>
#include "Infra/Ptr.h"
#include "Infra/Ref.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNamespace.h"
#include "SmNamespaceTranslation.h"
#include "SmGlobalFuncTranslation.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "RSymbol/RImplTraitDeclOuter.h"
#include "SmDeclContext_Decl.h"
#include "CommonTranslation.h"
#include "SmStructTranslation.h"
#include "SmEnumTranslation.h"
#include "SmTraitTranslation.h"
#include "SmImplTraitTranslation.h"

using namespace std;

namespace Citron {

class SNamespaceDecl;
class SGlobalFuncDecl;
class SClassDecl;
class SStructDecl;
class SEnumDecl;
class STraitDecl;
class SImplTraitDecl;

class RNamespace;
using RFactoryPtr = std::shared_ptr<class RFactory>;

using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

namespace {

class SmScriptElemTranslator
{
    SmDeclContextPtr rootDeclContext;
    RNamespace* rootNamespace;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    SmScriptElemTranslator(InRef<SmDeclContextPtr> rootDeclContext, RNamespace* rootNamespace, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : rootDeclContext{*rootDeclContext}, rootNamespace{rootNamespace}, rFactory{*rFactory}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* elem) { return Visit(elem); }

    void Visit(SNamespaceDecl* elem);
    void Visit(SGlobalFuncDecl* elem);
    void Visit(SClassDecl* elem);
    void Visit(SStructDecl* elem);
    void Visit(SEnumDecl* elem);
    void Visit(STraitDecl* elem);
    void Visit(SImplTraitDecl* elem);
    void Visit(STypeAliasDecl* elem);
};

} // namespace

} // namespace Citron

namespace Citron {

// TODO: 에러 처리
void SmTranslateScript(SScript* sScript, RModule* rModule, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    auto* emptyTypeArgs = (*rFactory)->MakeEmptyTypeArguments();
    auto* rootNamespace = (*rFactory)->MakeRootNamespaceDecl(rModule, *rFactory);
    SmDeclContextPtr rootDeclContext = MakePtr<SmDeclContext_Decl<RNamespace>>(SmDeclContextPtr{nullptr}, rootNamespace, emptyTypeArgs);

    for (auto& elem : sScript->elements)
    {
        SmScriptElemTranslator translator{rootDeclContext, rootNamespace, rFactory, phaseManager};
        visit(translator, elem);
    }
}

namespace {

void SmScriptElemTranslator::Visit(SNamespaceDecl* elem)
{
    // A.B.C가 있을 경우, 하위 네임스페이스를 찾는다. 없으면 만들어 나간다

    // 첫번째는 모듈에서 찾는다
    assert(1 <= elem->names.size());

    auto curNamespace = rootNamespace->GetNamespace(RName::Normal(elem->names[0]));
    if (!curNamespace)
    {
        curNamespace = rFactory->MakeChildNamespaceDecl(rootNamespace, elem->names[0], rFactory);
        rootNamespace->AddNamespace(curNamespace);
    }

    auto* emptyTypeArgs = rFactory->MakeEmptyTypeArguments();
    SmDeclContextPtr curDeclContext = MakePtr<SmDeclContext_Decl<RNamespace>>(SmDeclContextPtr{nullptr}, curNamespace, emptyTypeArgs);

    return SmTranslateNamespace(elem, curDeclContext, curNamespace, rFactory, phaseManager);
}

void SmScriptElemTranslator::Visit(SGlobalFuncDecl* elem)
{
    return SmTranslateGlobalFunc(rootDeclContext, elem, rootNamespace, rFactory, phaseManager);
}

void SmScriptElemTranslator::Visit(SClassDecl* elem)
{
    throw NotImplementedException{};
}

void SmScriptElemTranslator::Visit(SStructDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{rootNamespace, accessor};
    SmTranslateStruct(rootDeclContext, outer, elem, rFactory, phaseManager);
}

void SmScriptElemTranslator::Visit(SEnumDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{rootNamespace, accessor};
    SmTranslateEnum(rootDeclContext, outer, elem, rFactory, phaseManager);
}

void SmScriptElemTranslator::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Namespace outer{rootNamespace, MakeNamespaceMemberAccessor(decl->accessModifier)};
    SmTranslateTrait(rootDeclContext, outer, decl, rFactory, phaseManager);
}

void SmScriptElemTranslator::Visit(SImplTraitDecl* decl)
{
    SmTranslateImplTrait(decl, rootDeclContext, RImplTraitDeclOuter{rootNamespace}, rFactory, phaseManager);
}

void SmScriptElemTranslator::Visit(STypeAliasDecl* elem)
{
    // TODO: [79] type alias 구현
    throw NotImplementedException{};
}

} // namespace

} // namespace Citron