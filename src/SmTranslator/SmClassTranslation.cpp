#include "SmClassTranslation.h"

namespace Citron {
namespace {

class ClassElemVisitor
{
    SmDeclContextPtr classDeclContext;
    RClassDecl* rClass;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    ClassElemVisitor(InRef<SmDeclContextPtr> classDeclContext, RClassDecl* outer, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : classDeclContext{classDeclContext.Take()}, rClass{outer}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* decl) { Visit(decl); }

    void Visit(SClassDecl* decl);
    void Visit(SStructDecl* decl);
    void Visit(SEnumDecl* decl);
    void Visit(STraitDecl* decl);
    void Visit(SImplTraitDecl* decl);
    void Visit(SClassFuncDecl* decl);
    void Visit(SClassCtorDecl* decl);
    void Visit(SClassVarDecl* decl);
};

} // namespace
} // namespace Citron


namespace Citron {

void SmTranslateClass(SmDeclContextPtr classDeclContext, RClassDecl* rClass, RFactoryPtr rFactory, SmPhaseManager& phaseManager)
{
    // visit(ClassElemVisitor{classDeclContext},  );
}

namespace {

void ClassElemVisitor::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void ClassElemVisitor::Visit(SStructDecl* decl)
{
    auto accessor = MakeClassMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Class outer{rClass, accessor};
    VisitStruct(classDeclContext, outer, decl, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(SEnumDecl* decl)
{
    auto accessor = MakeClassMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Class outer{rClass, accessor};
    VisitEnum(classDeclContext, outer, decl, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Class outer{rClass, MakeClassMemberAccessor(decl->accessModifier)};
    VisitTrait(classDeclContext, outer, decl, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(SImplTraitDecl* decl)
{
    VisitImplTrait(decl, classDeclContext, RImplTraitDeclOuter{rClass}, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(SClassFuncDecl* decl)
{
    throw NotImplementedException{};
}

void ClassElemVisitor::Visit(SClassCtorDecl* decl)
{
    throw NotImplementedException{};
}

void ClassElemVisitor::Visit(SClassVarDecl* decl)
{
    throw NotImplementedException{};
}

} // namespace
} // namespace Citron
