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
#include "MIR/MFuncBody.h"
#include "MIR/MFactory.h"

#include "BuildNonTypeSymbolContext.h"
#include "SRTFactory.h"
#include "GlobalFuncTask.h"
#include "StructTask.h"
#include "StructFuncTask.h"
#include "StructCtorTask.h"
#include "StructDtorTask.h"
#include "StructVarTask.h"
#include "EnumElemVarTask.h"
#include "TraitFuncTask.h"
#include "ImplTask.h"
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
    void Visit(SImplDecl* decl);
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
    void Visit(SImplDecl* decl);
    void Visit(SClassFuncDecl* decl);
    void Visit(SClassCtorDecl* decl);
    void Visit(SClassVarDecl* decl);
};

// prepare task
class NamespaceElemVisitor
{
    RNamespace* curNS;
    RFactoryPtr rFactory;
    PhaseManager& phaseManager;

public:
    NamespaceElemVisitor(RNamespace* curNS, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
        : curNS{curNS}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* elem) { Visit(elem); }

    void Visit(SGlobalFuncDecl* elem);
    void Visit(SNamespaceDecl* elem);
    void Visit(SClassDecl* elem);
    void Visit(SStructDecl* elem);
    void Visit(SEnumDecl* elem);
    void Visit(STraitDecl* elem);
    void Visit(SImplDecl* elem);
};

class ScriptElemVisitor
{
    RNamespace* rootNamespace;
    RFactoryPtr rFactory;
    PhaseManager& phaseManager;

public:
    ScriptElemVisitor(RNamespace* rootNamespace, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
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
    void Visit(SImplDecl* elem);
};

void VisitGlobalFunc(SGlobalFuncDecl* sGFuncDecl, RNamespace* outer, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{   
    GlobalFuncTask::Register(outer, sGFuncDecl, move(rFactory), phaseManager);
}

void VisitStruct(RTypeDeclOuter outer, SStructDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{   
    RName name{RName::Normal(syntax->name)};
    auto* rStructDecl = (*rFactory)->MakeDecl<RStructDecl>(RDeclKey::Normal(name), outer, move(name), *rFactory);

    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rStructDecl, syntax->typeParams, *rFactory);
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
    RName enumName{RName::Normal(sEnum->name)};

    auto* rEnum = (*rFactory)->MakeDecl<REnumDecl>(RDeclKey::Normal(enumName), outer, move(enumName), *rFactory);
    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rEnum, sEnum->typeParams, *rFactory);
    rEnum->InitTypeParams(move(typeParams));
    outer.AddType(rEnum);

    // EnumElem
    for (auto* sEnumElem : sEnum->elements)
    {
        RName elemName{RName::Normal(sEnumElem->name)};
        auto* rEnumElem = (*rFactory)->MakeDecl<REnumElemDecl>(RDeclKey::Normal(elemName), rEnum, move(elemName), *rFactory);
        rEnum->AddElem(rEnumElem);

        // EnumElemVar
        for (auto* sEnumElemVar : sEnumElem->vars)
        {
            RName elemVarName{RName::Normal(sEnumElemVar->name)};
            auto* rEnumElemVar = (*rFactory)->MakeDecl<REnumElemVarDecl>(RDeclKey::Normal(elemVarName), rEnumElem, move(elemVarName), *rFactory);
            EnumElemVarTask::Register(rEnumElemVar, sEnumElemVar, phaseManager);
        }
    }
}

// TODO: [66] 2026-07-09, Trait, Extend 구현
void VisitTrait(RTypeDeclOuter outer, STraitDecl* sTrait, InRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    RName traitName{RName::Normal(sTrait->name)};
    auto* rTraitDecl = (*rFactory)->MakeDecl<RTraitDecl>(RDeclKey::Normal(traitName), outer, move(traitName), *rFactory);

    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rTraitDecl, sTrait->typeParams, *rFactory);
    rTraitDecl->InitTypeParams(move(typeParams));

    outer.AddType(rTraitDecl);

    for (auto& memberDecl : sTrait->memberDecls)
    {
        visit([rTraitDecl, &rFactory, &phaseManager](auto* memberDecl) {
            using T = remove_cvref_t<decltype(memberDecl)>;

            if constexpr (same_as<T, STraitFuncDecl*>)
            {
                TraitFuncTask::Register(rTraitDecl, memberDecl, *rFactory, phaseManager);
            }
            else static_assert(false);

        }, memberDecl);
    }
}

// Impl을 만드려고 하면은, trait, struct 등이 살아있어야 한다.
void VisitImpl(SImplDecl* decl, RDecl* outer, PhaseManager& phaseManager)
{
    ImplTask::Register(decl, outer, phaseManager);
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
    RTypeDeclOuter_Struct outer{rStruct, MakeStructMemberAccessor(decl->accessModifier)};
    VisitTrait(outer, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SImplDecl* decl)
{   
    VisitImpl(decl, rStruct, phaseManager);
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
    RTypeDeclOuter_Class outer{rClass, MakeClassMemberAccessor(decl->accessModifier)};
    VisitTrait(outer, decl, rFactory, phaseManager);
}

void ClassElemVisitor::Visit(SImplDecl* decl)
{    
    VisitImpl(decl, rClass, phaseManager);
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
    VisitGlobalFunc(elem, curNS, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SNamespaceDecl* elem)
{
    RNamespace* curNamespace = curNS;
    for (size_t i = 0, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        RNamespace* childNamespace = curNamespace->GetNamespace(RName::Normal(name));
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
    RTypeDeclOuter_Namespace outer{curNS, accessor};

    VisitStruct(outer, elem, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SEnumDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{curNS, accessor};
    VisitEnum(outer, elem, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Namespace outer{curNS, MakeNamespaceMemberAccessor(decl->accessModifier)};
    VisitTrait(outer, decl, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SImplDecl* decl)
{
    VisitImpl(decl, curNS, phaseManager);
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
    RTypeDeclOuter_Namespace outer{rootNamespace, MakeNamespaceMemberAccessor(decl->accessModifier)};
    VisitTrait(outer, decl, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SImplDecl* decl)
{
    VisitImpl(decl, rootNamespace, phaseManager);
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


    // 모듈은 모든 translation unit에 대해 하나만 갖는다. symbol tree도 하나고, namespace도 하나다
    auto* rModule = (*rFactory)->MakeModule(std::move(moduleName));
    auto* rRootNamespace = (*rFactory)->MakeRootNamespaceDecl(rModule, *rFactory);
    rModule->InitRootNamespace(rRootNamespace);

    auto srtFactory = MakePtr<SRTFactory>();
    auto binOpQueryService = MakePtr<BinOpQueryService>(**rFactory);
    
    PhaseManager phaseManager{*logger, *rFactory, *mFactory, srtFactory, binOpQueryService};
    for (auto* script : scripts) // translation units
    {
        auto* rootNamespace = (*rFactory)->MakeRootNamespaceDecl(rModule, *rFactory);

        for (auto& elem : script->elements)
        {
            ScriptElemVisitor visitor{rootNamespace, *rFactory, phaseManager};
            visit(visitor, elem);
        }
    }

    auto e_funcBodies = phaseManager.Run();
    RETURN_ON_ERROR(e_funcBodies);

    auto* mData = (*mFactory)->MakeMData(move(*e_funcBodies));
    return SmTranslationResult{rModule, mData};

    // return {rModule, };

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
