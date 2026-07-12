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

#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "RSymbol/RModule.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTraitDecl.h"
#include "MIR/MFuncBody.h"
#include "MIR/MFactory.h"

#include "BuildTypeDependentSymbolContext.h"
#include "SRTFactory.h"
#include "GlobalFuncTask.h"
#include "StructTask.h"
#include "StructFuncTask.h"
#include "StructCtorTask.h"
#include "StructDtorTask.h"
#include "StructVarTask.h"
#include "EnumElemVarTask.h"
#include "TraitFuncTask.h"
#include "PhaseManager.h"
#include "CommonTranslation.h"
#include "BinOpQueryService.h"

using namespace std;
using namespace Citron;

namespace Citron {

namespace {

class StructElemVisitor
{
    RStructDecl* rStruct;
    RFactoryPtr rFactory;
    PhaseManager& phaseManager;

public:
    StructElemVisitor(RStructDecl* rStruct, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
        : rStruct{rStruct}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {}

    void operator()(auto* decl) { Visit(decl); }

    void Visit(SClassDecl* decl);
    void Visit(SStructDecl* decl);
    void Visit(SEnumDecl* decl);
    void Visit(STraitDecl* decl);
    void Visit(SExtendDecl* decl);
    void Visit(SStructFuncDecl* decl);
    void Visit(SStructCtorDecl* decl);
    void Visit(SStructDtorDecl* decl);
    void Visit(SStructVarDecl* decl);
};

class ClassElemVisitor
{
    RClassDecl* rClass;
    RFactoryPtr rFactory;
    PhaseManager& phaseManager;

public:
    ClassElemVisitor(RClassDecl* outer, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
        : rClass{outer}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {}

    void operator()(auto* decl) { Visit(decl); }

    void Visit(SClassDecl* decl);
    void Visit(SStructDecl* decl);
    void Visit(SEnumDecl* decl);
    void Visit(STraitDecl* decl);
    void Visit(SExtendDecl* decl);
    void Visit(SClassFuncDecl* decl);
    void Visit(SClassCtorDecl* decl);
    void Visit(SClassVarDecl* decl);
};

// prepare task
class NamespaceElemVisitor
{
    RNamespaceDecl* curDecl;
    RFactoryPtr rFactory;
    PhaseManager& phaseManager;

public:
    NamespaceElemVisitor(RNamespaceDecl* curDecl, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
        : curDecl{curDecl}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* elem) { Visit(elem); }

    void Visit(SGlobalFuncDecl* elem);
    void Visit(SNamespaceDecl* elem);
    void Visit(SClassDecl* elem);
    void Visit(SStructDecl* elem);
    void Visit(SEnumDecl* elem);
    void Visit(STraitDecl* elem);
    void Visit(SExtendDecl* elem);
};

class ScriptElemVisitor
{
    RNamespaceDecl* rootNamespace;
    RFactoryPtr rFactory;
    PhaseManager& phaseManager;

public:
    ScriptElemVisitor(RNamespaceDecl* rootNamespace, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
        : rootNamespace{rootNamespace}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* elem) { Visit(elem); }

    void Visit(SNamespaceDecl* elem);
    void Visit(SGlobalFuncDecl* elem);
    void Visit(SClassDecl* elem);
    void Visit(SStructDecl* elem);
    void Visit(SEnumDecl* elem);
    void Visit(STraitDecl* elem);
    void Visit(SExtendDecl* elem);
};

void VisitGlobalFunc(SGlobalFuncDecl* sGFuncDecl, RNamespaceDecl* outer, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{   
    GlobalFuncTask::Register(outer, sGFuncDecl, move(rFactory), phaseManager);
}

void VisitStruct(RTypeDeclOuter outer, SStructDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{   
    auto* rStructDecl = (*rFactory)->MakeDecl<RStructDecl>(outer, RName::Normal(syntax->name), *rFactory);

    auto typeParams = MakeTypeParams(rStructDecl, syntax->typeParams, *rFactory);
    rStructDecl->InitTypeParams(move(typeParams));
    
    outer.AddType(rStructDecl);

    StructTask::Register(rStructDecl, syntax, phaseManager);

    // child     
    for (auto& memberDecl : syntax->memberDecls)
    {
        visit(StructElemVisitor{rStructDecl, *rFactory, phaseManager}, memberDecl);
    }
}

void VisitEnum(RTypeDeclOuter outer, SEnumDecl* sEnum, InRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{   
    auto* rEnum = (*rFactory)->MakeDecl<REnumDecl>(outer, RName::Normal(sEnum->name), *rFactory);

    auto typeParams = MakeTypeParams(rEnum, sEnum->typeParams, *rFactory);
    rEnum->InitTypeParams(move(typeParams));
    
    outer.AddType(rEnum);

    // EnumElem
    for (auto* sEnumElem : sEnum->elements)
    {
        auto* rEnumElem = (*rFactory)->MakeDecl<REnumElemDecl>(rEnum, RName::Normal(sEnumElem->name), *rFactory);
        rEnum->AddElem(rEnumElem);

        // EnumElemVar
        for (auto* sEnumElemVar : sEnumElem->vars)
        {
            auto* rEnumElemVar = (*rFactory)->MakeDecl<REnumElemVarDecl>(rEnumElem, RName::Normal(sEnumElemVar->name));
            EnumElemVarTask::Register(rEnumElemVar, sEnumElemVar, phaseManager);
        }
    }
}

// TODO: [66] 2026-07-09, Trait, Extend 구현
void VisitTrait(RTypeDeclOuter outer, STraitDecl* sTrait, AccessorContext accessorContext, InRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    auto* rTrait = (*rFactory)->MakeDecl<RTraitDecl>(outer, RName::Normal(sTrait->name), *rFactory);

    for (auto& memberDecl : sTrait->memberDecls)
    {
        visit([rTrait, &phaseManager](auto* memberDecl) {
            using T = remove_cvref_t<decltype(memberDecl)>;

            if constexpr (same_as<T, STraitFuncDecl*>)
            {
                TraitFuncTask::Register(rTrait, memberDecl, phaseManager);
            }
            else static_assert(false);

        }, memberDecl);
    }


}

void VisitExtend()
{
    // TODO: [66] 2026-07-09, Trait, Extend 구현
    throw NotImplementedException{};
}

void StructElemVisitor::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void StructElemVisitor::Visit(SStructDecl* decl)
{
    auto accessor = MakeStructMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Struct outer{rStruct, accessor};
    VisitStruct(outer, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SEnumDecl* decl)
{
    auto accessor = MakeStructMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Struct outer{rStruct, accessor};
    VisitEnum(outer, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(STraitDecl* decl)
{
    // TODO: [66] 2026-07-09, Trait 구현
    throw NotImplementedException{};
    // VisitTrait();
}

void StructElemVisitor::Visit(SExtendDecl* decl)
{
    VisitExtend();
}

void StructElemVisitor::Visit(SStructFuncDecl* decl)
{   
    StructFuncTask::Register(rStruct, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructCtorDecl* decl)
{
    StructCtorTask::Register(rStruct, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructDtorDecl* decl)
{
    StructDtorTask::Register(rStruct, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructVarDecl* decl)
{   
    StructVarTask::Register(rStruct, decl, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void ClassElemVisitor::Visit(SStructDecl* decl)
{
    auto accessor = MakeClassMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Class outer{rClass, accessor};
    VisitStruct(outer, decl, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(SEnumDecl* decl)
{
    auto accessor = MakeClassMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Class outer{rClass, accessor};
    VisitEnum(outer, decl, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(STraitDecl* decl)
{
    // TODO: [66] 2026-07-09, Trait 구현
    // VisitTrait();
    throw NotImplementedException{};
}

void ClassElemVisitor::Visit(SExtendDecl* decl)
{    
    VisitExtend();
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

void NamespaceElemVisitor::Visit(SGlobalFuncDecl* elem)
{
    VisitGlobalFunc(elem, curDecl, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SNamespaceDecl* elem)
{
    RNamespaceDecl* curNamespace = curDecl;
    for (size_t i = 0, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        RNamespaceDecl* childNamespace = curNamespace->GetNamespace(RName::Normal(name));
        if (!childNamespace)
        {
            childNamespace = rFactory->MakeChildNamespaceDecl(curNamespace, name, rFactory);
            curNamespace->AddNamespace(childNamespace);
        }

        curNamespace = childNamespace;
    }

    for (auto& nsElem : elem->elements)
    {
        visit(NamespaceElemVisitor{curNamespace, rFactory, phaseManager}, nsElem);
    }
}

void NamespaceElemVisitor::Visit(SClassDecl* elem)
{
    throw NotImplementedException{};
}

void NamespaceElemVisitor::Visit(SStructDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{curDecl, accessor};

    VisitStruct(outer, elem, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SEnumDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{curDecl, accessor};
    VisitEnum(outer, elem, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(STraitDecl* decl)
{
    // TODO: [66] 2026-07-09, Trait 구현
    // VisitTrait();
}

void NamespaceElemVisitor::Visit(SExtendDecl* decl)
{
    VisitExtend();
}

void ScriptElemVisitor::Visit(SNamespaceDecl* elem)
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

    for (size_t i = 1, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        auto childNamespace = curNamespace->GetNamespace(RName::Normal(name));
        if (!childNamespace)
        {
            childNamespace = rFactory->MakeChildNamespaceDecl(curNamespace, name, rFactory);
            curNamespace->AddNamespace(childNamespace);
        }

        curNamespace = childNamespace;
    }

    for (auto& nsElem : elem->elements)
    {
        visit(NamespaceElemVisitor{curNamespace, rFactory, phaseManager}, nsElem);
    }
}

void ScriptElemVisitor::Visit(SGlobalFuncDecl* elem)
{
    VisitGlobalFunc(elem, rootNamespace, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SClassDecl* elem)
{
    throw NotImplementedException{};
}

void ScriptElemVisitor::Visit(SStructDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{rootNamespace, accessor};
    VisitStruct(outer, elem, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SEnumDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{rootNamespace, accessor};
    VisitEnum(outer, elem, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(STraitDecl* decl)
{
    // TODO: [66] 2026-07-09, Trait 구현
    // VisitTrait();
}

void ScriptElemVisitor::Visit(SExtendDecl* decl)
{
    VisitExtend();
}


} // unnamed namespace 

expected<SmTranslationResult, DiagPtr> TranslateSyntax(
    string moduleName,
    const vector<SScript*>& scripts, // translation units
    const vector<EModule*>& referenceModules,
    InRef<LoggerPtr> logger,
    InRef<RFactoryPtr> rFactory,
    InRef<MFactoryPtr> mFactory)
{
    // TODO: NewRootNamespaceDecl이 아니라 RootNamespaceGroupDecl이어야 할것 같고, 모듈은 rootNamespaceDeclGroup을 가져야 할 것 같다

    // RNamespaceDecl은 각 TranslationUnit별로 별개로 가지는데,
    auto* nModule = (*rFactory)->MakeModule(RName::Normal(moduleName));

    auto srtFactory = MakePtr<SRTFactory>();
    auto binOpQueryService = MakePtr<BinOpQueryService>(**rFactory);
    
    PhaseManager phaseManager{*logger, *rFactory, *mFactory, srtFactory, binOpQueryService};
    for (auto* script : scripts) // translation units
    {
        auto* rootNamespace = (*rFactory)->MakeRootNamespaceDecl(*rFactory);

        for (auto& elem : script->elements)
        {
            ScriptElemVisitor visitor{rootNamespace, *rFactory, phaseManager};
            visit(visitor, elem);
        }
    }

    auto e_funcBodies = phaseManager.Run();
    RETURN_ON_ERROR(e_funcBodies);

    auto* mData = (*mFactory)->MakeMData(move(*e_funcBodies));
    return SmTranslationResult{nModule, mData};

    // return {nModule, };

    //    var moduleDecl = new ModuleDeclSymbol(moduleName, bReference: false);
    //
    //    var skeletonPhaseContext = new BuildingSkeletonPhaseContext(); // refModules를 사용해야 한다
    //    var topLevelVisitor = new TopLevelVisitor_BuildingSkeletonPhase<ModuleDeclSymbol>(skeletonPhaseContext, moduleDecl);
    //
    //    foreach(var script in scripts)
    //    {
    //        foreach(var scriptElem in script.Elements)
    //        {
    //            switch (scriptElem)
    //            {
    //                case S.TypeDeclScriptElement typeDeclElem :
    //                    topLevelVisitor.VisitTypeDecl(typeDeclElem.TypeDecl);
    //                    break;
    //
    //                    case S.GlobalFuncDeclScriptElement globalFuncDeclElem :
    //                        topLevelVisitor.VisitGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
    //                        break;
    //
    //                        case S.NamespaceDeclScriptElement namespaceDeclElem :
    //                            // Discovering Namespaces
    //                            topLevelVisitor.VisitNamespaceDecl(namespaceDeclElem.NamespaceDecl);
    //                            break;
    //
    //                        default:
    //                            throw UnreachableException();
    //            }
    //        }
    //    }
    //
    //    var modulesBuilder = ImmutableArray.CreateBuilder<ModuleDeclSymbol>(refModuleDecls.Length + 1);
    //    modulesBuilder.Add(moduleDecl);
    //    modulesBuilder.AddRange(refModuleDecls.AsEnumerable());
    //    var moduleDecls = modulesBuilder.MoveToImmutable();
    //
    //    // 2. BuildingMemberDeclPhase
    //    var buildingMemberDeclPhaseContext = new BuildingMemberDeclPhaseContext(moduleDecls, factory);
    //    skeletonPhaseContext.BuildMemberDecl(buildingMemberDeclPhaseContext);
    //
    //    // 3. BuildingTrivialConstructorPhase
    //    buildingMemberDeclPhaseContext.BuildTrivialConstructor();
    //
    //    // 4. BuildingBodyPhase
    //    var globalContext = new GlobalContext(factory, moduleDecls, logger);
    //    var buildingBodyPhaseContext = new BuildingBodyPhaseContext(globalContext);
    //    if (!buildingMemberDeclPhaseContext.BuildBody(buildingBodyPhaseContext))
    //        return null;
    //
    //    var body = globalContext.GetBodies();
    //    return new R.Script(moduleDecl, body);
    //}
}

} // namespace Citron
