#include "SmNamespaceTranslation.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RNamespace.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "RSymbol/RImplTraitDeclOuter.h"
#include "SmDeclContext_Decl.h"
#include "SmGlobalFuncTranslation.h"
#include "SmStructTranslation.h"
#include "SmEnumTranslation.h"
#include "SmTraitTranslation.h"
#include "SmImplTraitTranslation.h"
#include "CommonTranslation.h"


namespace Citron {

class SmNamespaceElemTranslator
{
    SmDeclContextPtr namespaceDeclContext;
    RNamespace* curNS;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    SmNamespaceElemTranslator(InRef<SmDeclContextPtr> namespaceDeclContext, RNamespace* curNS, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : namespaceDeclContext{*namespaceDeclContext}, curNS{curNS}, rFactory{*rFactory}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* elem) { return Visit(elem); }

    void Visit(SGlobalFuncDecl* elem);
    void Visit(SNamespaceDecl* elem);
    void Visit(SClassDecl* elem);
    void Visit(SStructDecl* elem);
    void Visit(SEnumDecl* elem);
    void Visit(STraitDecl* elem);
    void Visit(SImplTraitDecl* elem);
    void Visit(STypeAliasDecl* elem);
};

} // namespace Citron

// implementations
namespace Citron {

void SmTranslateNamespace(SNamespaceDecl* elem, InRef<SmDeclContextPtr> declContext, RNamespace* curNS, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    SmDeclContextPtr curDeclContext = *declContext;
    RNamespace* curNamespace = curNS;

    for (size_t i = 0, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        RNamespace* childNamespace = curNamespace->GetNamespace(RName::Normal(name));
        if (!childNamespace)
        {
            childNamespace = (*rFactory)->MakeChildNamespaceDecl(curNamespace, name, *rFactory);
            curNamespace->AddNamespace(childNamespace);
        }
        auto childDeclContext = MakePtr<SmDeclContext_Decl<RNamespace>>(curDeclContext, childNamespace, (*rFactory)->MakeEmptyTypeArguments());

        curNamespace = childNamespace;
        curDeclContext = childDeclContext;
    }

    for (auto& nsElem : elem->elements)
    {
        visit(SmNamespaceElemTranslator{curDeclContext, curNamespace, rFactory, phaseManager}, nsElem);
    }
}

void SmNamespaceElemTranslator::Visit(SGlobalFuncDecl* elem)
{
    SmTranslateGlobalFunc(namespaceDeclContext, elem, curNS, rFactory, phaseManager);
}

void SmNamespaceElemTranslator::Visit(SNamespaceDecl* elem)
{
    SmTranslateNamespace(elem, namespaceDeclContext, curNS, rFactory, phaseManager);
}

void SmNamespaceElemTranslator::Visit(SClassDecl* elem)
{
    throw NotImplementedException{};
}

void SmNamespaceElemTranslator::Visit(SStructDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{curNS, accessor};
    SmTranslateStruct(namespaceDeclContext, outer, elem, rFactory, phaseManager);
}

void SmNamespaceElemTranslator::Visit(SEnumDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{curNS, accessor};
    SmTranslateEnum(namespaceDeclContext, outer, elem, rFactory, phaseManager);
}

void SmNamespaceElemTranslator::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Namespace outer{curNS, MakeNamespaceMemberAccessor(decl->accessModifier)};
    SmTranslateTrait(namespaceDeclContext, outer, decl, rFactory, phaseManager);
}

void SmNamespaceElemTranslator::Visit(SImplTraitDecl* decl)
{
    SmTranslateImplTrait(decl, namespaceDeclContext, RImplTraitDeclOuter{curNS}, rFactory, phaseManager);
}

void SmNamespaceElemTranslator::Visit(STypeAliasDecl* elem)
{
    // TODO: [79] type alias 구현
    throw NotImplementedException{};
}

} // namespace Citron