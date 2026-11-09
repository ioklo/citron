#include "SmClassTranslation.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RDeclKey.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RImplTraitDeclOuter.h"
#include "CommonTranslation.h"
#include "SmDeclContext_ClassDecl.h"
#include "SmStructTranslation.h"
#include "SmEnumTranslation.h"
#include "SmTraitTranslation.h"
#include "SmImplTraitTranslation.h"

using namespace std;

namespace Citron {
namespace {

class SmClassElemTranslator
{
    SmDeclContextPtr classDeclContext;
    RClassDecl* rClass;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    SmClassElemTranslator(InRef<SmDeclContextPtr> classDeclContext, RClassDecl* outer, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : classDeclContext{*classDeclContext}, rClass{outer}, rFactory{*rFactory}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* decl) { Visit(decl); }

    void Visit(SClassDecl* decl);
    void Visit(SStructDecl* decl);
    void Visit(SEnumDecl* decl);
    void Visit(STraitDecl* decl);
    void Visit(SImplTraitDecl* decl);
    void Visit(STypeAliasDecl* elem);
    void Visit(SClassFuncDecl* decl);
    void Visit(SClassCtorDecl* decl);
    void Visit(SClassVarDecl* decl);
};

}

// namespace
} // namespace Citron



namespace Citron {

void SmTranslateClass(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SClassDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName name{RName::Normal(syntax->name)};
    auto* rClassDecl = (*rFactory)->MakeDecl<RClassDecl>(RDeclKey::Normal(name), outer, move(name), rFactory);

    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rClassDecl, syntax->typeParams, *rFactory);
    rClassDecl->InitTypeParams(move(typeParams));
    outer.AddType(rClassDecl);

    assert(false);

    // TODO: RDecl::MakeOpenTypeArgs는 전체적으로 typeArgs를 다시만드는데, 이 rDecl만큼만 만들게 할 수 있을 것이다
    // auto* openTypeArgs = rClassDecl->MakeOpenTypeArgs(**rFactory);
    // auto classDeclContext = MakePtr<SmDeclContext_ClassDecl>(*outerDeclContext, rClassDecl, openTypeArgs);
    // 
    //shared_ptr<SmClassTask> task{new SmClassTask(move(classDeclContext), rClassDecl, syntax)};
    //phaseManager.AddBuildTypeHierarchyTask(task);
    //phaseManager.AddBuildImplicitSymbolTask(task);

    //// child     
    //for (auto& memberDecl : syntax->memberDecls)
    //{
    //    visit(SmClassElemTranslator{classDeclContext, rStructDecl, *rFactory, phaseManager}, memberDecl);
    //}
}

namespace {

void SmClassElemTranslator::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void SmClassElemTranslator::Visit(SStructDecl* decl)
{
    auto accessor = MakeClassMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Class outer{rClass, accessor};
    SmTranslateStruct(classDeclContext, outer, decl, rFactory, phaseManager);
}

void SmClassElemTranslator::Visit(SEnumDecl* decl)
{
    auto accessor = MakeClassMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Class outer{rClass, accessor};
    SmTranslateEnum(classDeclContext, outer, decl, rFactory, phaseManager);
}

void SmClassElemTranslator::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Class outer{rClass, MakeClassMemberAccessor(decl->accessModifier)};
    SmTranslateTrait(classDeclContext, outer, decl, rFactory, phaseManager);
}

void SmClassElemTranslator::Visit(SImplTraitDecl* decl)
{
    SmTranslateImplTrait(decl, classDeclContext, RImplTraitDeclOuter{rClass}, rFactory, phaseManager);
}

void SmClassElemTranslator::Visit(STypeAliasDecl* elem)
{
    // TODO: [79] type alias 구현
    throw NotImplementedException{};
}

void SmClassElemTranslator::Visit(SClassFuncDecl* decl)
{
    throw NotImplementedException{};
}

void SmClassElemTranslator::Visit(SClassCtorDecl* decl)
{
    throw NotImplementedException{};
}

void SmClassElemTranslator::Visit(SClassVarDecl* decl)
{
    throw NotImplementedException{};
}

} // namespace
} // namespace Citron
