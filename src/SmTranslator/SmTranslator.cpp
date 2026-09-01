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
#include "RSymbol/RTypeAliasDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MFuncBody.h"
#include "MIR/MFactory.h"

#include "BuildNonTypeSymbolContext.h"
#include "SmFactory.h"
#include "GlobalFuncTask.h"
#include "StructTask.h"
#include "StructFuncTask.h"
#include "StructCtorTask.h"
#include "StructDtorTask.h"
#include "StructVarTask.h"
#include "EnumElemVarTask.h"
#include "TraitFuncTask.h"
#include "ImplTraitTask.h"
#include "SmPhaseManager.h"
#include "CommonTranslation.h"
#include "BinOpQueryService.h"
#include "SmDeclContext_Decl.h"
#include "SmDeclContext_ClassDecl.h"
#include "SmTraitTypeTask.h"
#include "TypeAliasTask.h"

using namespace std;
using namespace Citron;

namespace Citron {

namespace {

class StructElemVisitor
{
    SmDeclContextPtr structDeclContext; // rStruct의 declContext
    RStructDecl* rStruct;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    StructElemVisitor(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : structDeclContext{structDeclContext.Take()}, rStruct{rStruct}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {}

    void operator()(auto* decl) { Visit(decl); }

    void Visit(SClassDecl* decl);
    void Visit(SStructDecl* decl);
    void Visit(SEnumDecl* decl);
    void Visit(STraitDecl* decl);
    void Visit(SImplTraitDecl* decl);
    void Visit(STypeAliasDecl* decl);
    void Visit(SStructFuncDecl* decl);
    void Visit(SStructCtorDecl* decl);
    void Visit(SStructDtorDecl* decl);
    void Visit(SStructVarDecl* decl);
};

class ClassElemVisitor
{
    SmDeclContextPtr classDeclContext;
    RClassDecl* rClass;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    ClassElemVisitor(TakeRef<SmDeclContextPtr> classDeclContext, RClassDecl* outer, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : classDeclContext{classDeclContext.Take()}, rClass{outer}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {}

    void operator()(auto* decl) { Visit(decl); }

    void Visit(SClassDecl* decl);
    void Visit(SStructDecl* decl);
    void Visit(SEnumDecl* decl);
    void Visit(STraitDecl* decl);
    void Visit(SImplTraitDecl* decl);
    void Visit(STypeAliasDecl* decl);
    void Visit(SClassFuncDecl* decl);
    void Visit(SClassCtorDecl* decl);
    void Visit(SClassVarDecl* decl);
};

// prepare task
class NamespaceElemVisitor
{
    SmDeclContextPtr namespaceDeclContext;
    RNamespace* curNS;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    NamespaceElemVisitor(TakeRef<SmDeclContextPtr> namespaceDeclContext, RNamespace* curNS, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : namespaceDeclContext{namespaceDeclContext.Take()}, curNS{curNS}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* elem) { Visit(elem); }

    void Visit(SGlobalFuncDecl* elem);
    void Visit(SNamespaceDecl* elem);
    void Visit(SClassDecl* elem);
    void Visit(SStructDecl* elem);
    void Visit(SEnumDecl* elem);
    void Visit(STraitDecl* elem);
    void Visit(SImplTraitDecl* elem);
    void Visit(STypeAliasDecl* elem);
};

class ScriptElemVisitor
{
    SmDeclContextPtr rootDeclContext;
    RNamespace* rootNamespace;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    ScriptElemVisitor(TakeRef<SmDeclContextPtr> rootDeclContext, RNamespace* rootNamespace, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : rootDeclContext{rootDeclContext.Take()}, rootNamespace{rootNamespace}, rFactory{rFactory.Take()}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* elem) { Visit(elem); }

    void Visit(SNamespaceDecl* elem);
    void Visit(SGlobalFuncDecl* elem);
    void Visit(SClassDecl* elem);
    void Visit(SStructDecl* elem);
    void Visit(SEnumDecl* elem);
    void Visit(STraitDecl* elem);
    void Visit(SImplTraitDecl* elem);
    void Visit(STypeAliasDecl* elem);
};

void VisitGlobalFunc(TakeRef<SmDeclContextPtr> outerDeclContext, SGlobalFuncDecl* sGFuncDecl, RNamespace* outer, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{   
    GlobalFuncTask::Register(move(outerDeclContext), outer, sGFuncDecl, move(rFactory), phaseManager);
}

void VisitStruct(TakeRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SStructDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{   
    RName name{RName::Normal(syntax->name)};
    auto* rStructDecl = (*rFactory)->MakeDecl<RStructDecl>(RDeclKey::Normal(name), outer, move(name), *rFactory);

    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rStructDecl, syntax->typeParams, *rFactory);
    rStructDecl->InitTypeParams(move(typeParams));
    outer.AddType(rStructDecl);

    // TODO: RDecl::MakeOpenTypeArgs는 전체적으로 typeArgs를 다시만드는데, 이 rDecl만큼만 만들게 할 수 있을 것이다
    auto* openTypeArgs = rStructDecl->MakeOpenTypeArgs(**rFactory);
    SmDeclContextPtr structDeclContext = MakePtr<SmDeclContext_Decl<RStructDecl>>(move(outerDeclContext), rStructDecl, openTypeArgs);

    StructTask::Register(structDeclContext, rStructDecl, syntax, phaseManager);

    // child     
    for (auto& memberDecl : syntax->memberDecls)
    {
        visit(StructElemVisitor{structDeclContext, rStructDecl, *rFactory, phaseManager}, memberDecl);
    }
}

// body를 만들지 않으므로 declContext를 만들지 않는다
void VisitEnum(TakeRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SEnumDecl* sEnum, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{   
    RName enumName{RName::Normal(sEnum->name)};

    auto* rEnum = (*rFactory)->MakeDecl<REnumDecl>(RDeclKey::Normal(enumName), outer, move(enumName), *rFactory);
    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rEnum, sEnum->typeParams, *rFactory);
    rEnum->InitTypeParams(move(typeParams));
    outer.AddType(rEnum);

    SmDeclContextPtr enumDeclContext = MakePtr<SmDeclContext_Decl<REnumDecl>>(move(outerDeclContext), rEnum, rEnum->MakeOpenTypeArgs(**rFactory));
    auto* emptyTypeArgs = (*rFactory)->MakeEmptyTypeArguments();

    // EnumElem
    for (auto* sEnumElem : sEnum->elements)
    {
        RName elemName{RName::Normal(sEnumElem->name)};
        auto* rEnumElem = (*rFactory)->MakeDecl<REnumElemDecl>(RDeclKey::Normal(elemName), rEnum, move(elemName), *rFactory);
        rEnum->AddElem(rEnumElem);

        SmDeclContextPtr enumElemDeclContext = MakePtr<SmDeclContext_Decl<REnumElemDecl>>(enumDeclContext, rEnumElem, emptyTypeArgs);

        // EnumElemVar
        for (auto* sEnumElemVar : sEnumElem->vars)
        {
            RName elemVarName{RName::Normal(sEnumElemVar->name)};
            auto* rEnumElemVar = (*rFactory)->MakeDecl<REnumElemVarDecl>(RDeclKey::Normal(elemVarName), rEnumElem, move(elemVarName));
            EnumElemVarTask::Register(enumElemDeclContext, rEnumElemVar, sEnumElemVar, *rFactory, phaseManager);
        }
    }
}

void VisitTrait(TakeRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, STraitDecl* sTrait, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName traitName{RName::Normal(sTrait->name)};
    auto* rTraitDecl = (*rFactory)->MakeDecl<RTraitDecl>(RDeclKey::Normal(traitName), outer, move(traitName), *rFactory);

    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rTraitDecl, sTrait->typeParams, *rFactory);
    rTraitDecl->InitTypeParams(move(typeParams));
    outer.AddType(rTraitDecl);

    SmDeclContextPtr traitDeclContext = MakePtr<SmDeclContext_Decl<RTraitDecl>>(move(outerDeclContext), rTraitDecl, rTraitDecl->MakeOpenTypeArgs(**rFactory));

    for (auto& sMemberDecl : sTrait->memberDecls)
    {
        visit([&traitDeclContext, rTraitDecl, &rFactory, &phaseManager](auto* sMemberDecl) {
            using T = remove_cvref_t<decltype(sMemberDecl)>;

            if constexpr (same_as<T, STraitTypeDecl*>)
            {
                SmTraitTypeTask::BuildTypeHierarchy(traitDeclContext, sMemberDecl, rTraitDecl, rFactory->get(), phaseManager);
            }
            else if constexpr (same_as<T, STraitFuncDecl*>)
            {
                TraitFuncTask::Register(traitDeclContext, rTraitDecl, sMemberDecl, *rFactory, phaseManager);
            }
            else static_assert(false);

        }, sMemberDecl);
    }
}

// Impl을 만드려고 하면은, trait, struct 등이 살아있어야 한다
void VisitImplTrait(SImplTraitDecl* sImplDecl, TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter outer, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    ImplTraitTask::Register(sImplDecl, move(outerDeclContext), outer, move(rFactory), phaseManager);
}

void StructElemVisitor::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void StructElemVisitor::Visit(SStructDecl* decl)
{
    auto accessor = MakeStructMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Struct outer{rStruct, accessor};
    VisitStruct(structDeclContext, outer, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SEnumDecl* decl)
{
    auto accessor = MakeStructMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Struct outer{rStruct, accessor};
    VisitEnum(structDeclContext, outer, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Struct outer{rStruct, MakeStructMemberAccessor(decl->accessModifier)};
    VisitTrait(structDeclContext, outer, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SImplTraitDecl* decl)
{   
    VisitImplTrait(decl, structDeclContext, RImplTraitDeclOuter{rStruct}, rFactory, phaseManager);
}

void StructElemVisitor::Visit(STypeAliasDecl* decl)
{
    RTypeDeclOuter_Struct outer{rStruct, MakeStructMemberAccessor(decl->accessModifier)};
    TypeAliasTask::Register(structDeclContext, outer, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructFuncDecl* decl)
{   
    StructFuncTask::Register(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructCtorDecl* decl)
{
    StructCtorTask::Register(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructDtorDecl* decl)
{
    StructDtorTask::Register(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructVarDecl* decl)
{   
    StructVarTask::Register(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

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

void ClassElemVisitor::Visit(STypeAliasDecl* decl)
{
    RTypeDeclOuter_Class outer{rClass, MakeClassMemberAccessor(decl->accessModifier)};
    TypeAliasTask::Register(classDeclContext, outer, decl, rFactory, phaseManager);
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
    VisitGlobalFunc(namespaceDeclContext, elem, curNS, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SNamespaceDecl* elem)
{
    RNamespace* curNamespace = curNS;
    auto curDeclContext = namespaceDeclContext;

    for (size_t i = 0, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        RNamespace* childNamespace = curNamespace->GetNamespace(RName::Normal(name));
        if (!childNamespace)
        {
            childNamespace = rFactory->MakeChildNamespaceDecl(curNamespace, name, rFactory);
            curNamespace->AddNamespace(childNamespace);
        }
        auto childDeclContext = MakePtr<SmDeclContext_Decl<RNamespace>>(curDeclContext, childNamespace, rFactory->MakeEmptyTypeArguments());

        curNamespace = childNamespace;
        curDeclContext = childDeclContext;
    }

    for (auto& nsElem : elem->elements)
    {
        visit(NamespaceElemVisitor{curDeclContext, curNamespace, rFactory, phaseManager}, nsElem);
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
    VisitStruct(namespaceDeclContext, outer, elem, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SEnumDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{curNS, accessor};
    VisitEnum(namespaceDeclContext, outer, elem, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Namespace outer{curNS, MakeNamespaceMemberAccessor(decl->accessModifier)};
    VisitTrait(namespaceDeclContext, outer, decl, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SImplTraitDecl* decl)
{
    VisitImplTrait(decl, namespaceDeclContext, RImplTraitDeclOuter{curNS}, rFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(STypeAliasDecl* decl)
{
    RTypeDeclOuter_Namespace outer{curNS, MakeNamespaceMemberAccessor(decl->accessModifier)};
    TypeAliasTask::Register(namespaceDeclContext, outer, decl, rFactory, phaseManager);
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

    auto* emptyTypeArgs = rFactory->MakeEmptyTypeArguments();
    SmDeclContextPtr curDeclContext = MakePtr<SmDeclContext_Decl<RNamespace>>(SmDeclContextPtr{nullptr}, curNamespace, emptyTypeArgs);

    for (size_t i = 1, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        auto childNamespace = curNamespace->GetNamespace(RName::Normal(name));
        if (!childNamespace)
        {
            childNamespace = rFactory->MakeChildNamespaceDecl(curNamespace, name, rFactory);
            curNamespace->AddNamespace(childNamespace);
        }
        auto childDeclContext = MakePtr<SmDeclContext_Decl<RNamespace>>(curDeclContext, childNamespace, emptyTypeArgs);

        curNamespace = childNamespace;
        curDeclContext = childDeclContext;
    }

    for (auto& nsElem : elem->elements)
    {
        visit(NamespaceElemVisitor{curDeclContext, curNamespace, rFactory, phaseManager}, nsElem);
    }
}

void ScriptElemVisitor::Visit(SGlobalFuncDecl* elem)
{
    VisitGlobalFunc(rootDeclContext, elem, rootNamespace, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SClassDecl* elem)
{
    throw NotImplementedException{};
}

void ScriptElemVisitor::Visit(SStructDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{rootNamespace, accessor};
    VisitStruct(rootDeclContext, outer, elem, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SEnumDecl* elem)
{
    auto accessor = MakeNamespaceMemberAccessor(elem->accessModifier);
    RTypeDeclOuter_Namespace outer{rootNamespace, accessor};
    VisitEnum(rootDeclContext, outer, elem, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Namespace outer{rootNamespace, MakeNamespaceMemberAccessor(decl->accessModifier)};
    VisitTrait(rootDeclContext, outer, decl, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SImplTraitDecl* decl)
{
    VisitImplTrait(decl, rootDeclContext, RImplTraitDeclOuter{rootNamespace}, rFactory, phaseManager);
}

void ScriptElemVisitor::Visit(STypeAliasDecl* decl)
{
    RTypeDeclOuter_Namespace outer{rootNamespace, MakeNamespaceMemberAccessor(decl->accessModifier)};
    TypeAliasTask::Register(rootDeclContext, outer, decl, rFactory, phaseManager);
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

    auto smFactory = MakePtr<SmFactory>();
    auto binOpQueryService = MakePtr<BinOpQueryService>(**rFactory);
    
    SmPhaseManager phaseManager{*logger, *rFactory, *mFactory, smFactory, binOpQueryService};
    auto* emptyTypeArgs = (*rFactory)->MakeEmptyTypeArguments();
    for (auto* script : scripts) // translation units
    {
        auto* rootNamespace = (*rFactory)->MakeRootNamespaceDecl(rModule, *rFactory);
        SmDeclContextPtr rootDeclContext = MakePtr<SmDeclContext_Decl<RNamespace>>(SmDeclContextPtr{nullptr}, rootNamespace, emptyTypeArgs);

        for (auto& elem : script->elements)
        {
            ScriptElemVisitor visitor{rootDeclContext, rootNamespace, *rFactory, phaseManager};
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
    //    var globalContext = new SmGlobalContext(factory, moduleDecls, logger);
    //    var buildingBodyPhaseContext = new BuildingBodyPhaseContext(globalContext);
    //    if (!buildingMemberDeclPhaseContext.BuildBody(buildingBodyPhaseContext))
    //        return null;
    //
    //    var body = globalContext.GetBodies();
    //    return new R.Script(moduleDecl, body);
    //}
}

} // namespace Citron
