#include "SyntaxIR0Translator.h"

#include <stdexcept>
#include <memory>
#include <cassert>
#include <ranges>

#include "Infra/Unreachable.h"
#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "NSymbol/NNamespaceDecl.h"
#include "NSymbol/NStructDecl.h"
#include "NSymbol/NEnumDecl.h"
#include "NSymbol/NModule.h"
#include "NSymbol/NClassDecl.h"
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
#include "PhaseManager.h"
#include "CommonTranslation.h"
#include "BinOpQueryService.h"

using namespace std;
using namespace Citron;

namespace Citron {

namespace {

class StructElemVisitor : public SStructMemberDeclVisitor
{
    NStructDecl* nStruct;
    NFactoryPtr nFactory;
    PhaseManager& phaseManager;

public:
    StructElemVisitor(NStructDecl* nStruct, const NFactoryPtr& nFactory, PhaseManager& phaseManager) 
        : nStruct{nStruct}, nFactory{nFactory}, phaseManager{phaseManager}
    {}

    void Visit(SClassDecl* decl) override;
    void Visit(SStructDecl* decl) override;
    void Visit(SEnumDecl* decl) override;
    void Visit(SStructFuncDecl* decl) override;
    void Visit(SStructCtorDecl* decl) override;
    void Visit(SStructDtorDecl* decl) override;
    void Visit(SStructVarDecl* decl) override;
};

class ClassElemVisitor : public SClassMemberDeclVisitor
{
    NClassDecl* outer;
    NFactoryPtr nFactory;
    PhaseManager& phaseManager;

public:
    ClassElemVisitor(NClassDecl* outer, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
        : outer{outer}, nFactory{nFactory}, phaseManager{phaseManager}
    {}

    void Visit(SClassDecl* decl) override;
    void Visit(SStructDecl* decl) override;
    void Visit(SEnumDecl* decl) override;
    void Visit(SClassFuncDecl* decl) override;
    void Visit(SClassCtorDecl* decl) override;
    void Visit(SClassVarDecl* decl) override;
};

// prepare task
class NamespaceElemVisitor : public SNamespaceDeclElementVisitor
{
    NNamespaceDecl* curDecl;
    NFactoryPtr nFactory;
    PhaseManager& phaseManager;

public:
    NamespaceElemVisitor(NNamespaceDecl* curDecl, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
        : curDecl{curDecl}, nFactory{nFactory}, phaseManager{phaseManager}
    {
    }

    // Inherited via SNamespaceDeclElementVisitor
    void Visit(SGlobalFuncDecl* elem) override;
    void Visit(SNamespaceDecl* elem) override;
    void Visit(SClassDecl* elem) override;
    void Visit(SStructDecl* elem) override;
    void Visit(SEnumDecl* elem) override;
};

class ScriptElemVisitor : public SScriptElementVisitor
{
    NNamespaceDecl* rootNamespace;
    NFactoryPtr nFactory;
    PhaseManager& phaseManager;

public:
    ScriptElemVisitor(NNamespaceDecl* rootNamespace, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
        : rootNamespace{rootNamespace}, nFactory{nFactory}, phaseManager{phaseManager}
    {
    }

    void Visit(SNamespaceDecl* elem) override;
    void Visit(SGlobalFuncDecl* elem) override;
    void Visit(SClassDecl* elem) override;
    void Visit(SStructDecl* elem) override;
    void Visit(SEnumDecl* elem) override;
};

void VisitGlobalFunc(SGlobalFuncDecl* sGFuncDecl, NNamespaceDecl* outer, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{   
    GlobalFuncTask::Register(outer, sGFuncDecl, nFactory, phaseManager);
}

template<typename TNOuter>
void VisitStruct(TNOuter* outer, SStructDecl* syntax, AccessorContext accessorContext, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{   
    auto accessor = MakeAccessor(syntax->accessModifier, accessorContext);
    auto typeParams = MakeTypeParams(syntax->typeParams);
    auto* nStructDecl = nFactory->MakeNDecl<NStructDecl>(outer, accessor, RName_Normal(syntax->name), move(typeParams));
    outer->AddType(nStructDecl);

    StructTask::Register(nStructDecl, syntax, accessorContext, phaseManager);

    // child     
    for (auto* memberDecl : syntax->memberDecls)
    {
        StructElemVisitor visitor{nStructDecl, nFactory, phaseManager};
        memberDecl->Accept(visitor);
    }
}

template<typename TNOuter>
void VisitEnum(TNOuter* outer, SEnumDecl* sEnum, AccessorContext accessorContext, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{   
    auto accessor = MakeAccessor(sEnum->accessModifier, accessorContext);
    auto typeParams = MakeTypeParams(sEnum->typeParams);
    auto* nEnum = nFactory->MakeNDecl<NEnumDecl>(outer, accessor, RName_Normal{sEnum->name}, move(typeParams));
    outer->AddType(nEnum);

    // EnumElem
    for (auto* sEnumElem : sEnum->elements)
    {
        auto* nEnumElem = nFactory->MakeNDecl<NEnumElemDecl>(nEnum, RName_Normal{sEnumElem->name});
        nEnum->AddElem(nEnumElem);

        // EnumElemVar
        for (auto* sEnumElemVar : sEnumElem->vars)
        {
            auto* nEnumElemVar = nFactory->MakeNDecl<NEnumElemVarDecl>(nEnumElem, RName_Normal{sEnumElemVar->name});
            EnumElemVarTask::Register(nEnumElemVar, sEnumElemVar, phaseManager);
        }
    }
}

void StructElemVisitor::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void StructElemVisitor::Visit(SStructDecl* decl)
{
    VisitStruct(nStruct, decl, AccessorContext::InsideStruct, nFactory, phaseManager);
}

void StructElemVisitor::Visit(SEnumDecl* decl)
{
    VisitEnum(nStruct, decl, AccessorContext::InsideStruct, nFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructFuncDecl* decl)
{   
    StructFuncTask::Register(nStruct, decl, nFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructCtorDecl* decl)
{
    StructCtorTask::Register(nStruct, decl, nFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructDtorDecl* decl)
{
    StructDtorTask::Register(nStruct, decl, nFactory, phaseManager);
}

void StructElemVisitor::Visit(SStructVarDecl* decl)
{   
    StructVarTask::Register(nStruct, decl, nFactory, phaseManager);
}

void ClassElemVisitor::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void ClassElemVisitor::Visit(SStructDecl* decl)
{
    VisitStruct(outer, decl, AccessorContext::InsideClass, nFactory, phaseManager);
}

void ClassElemVisitor::Visit(SEnumDecl* decl)
{
    VisitEnum(outer, decl, AccessorContext::InsideClass, nFactory, phaseManager);
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

// Inherited via SNamespaceDeclElementVisitor
void NamespaceElemVisitor::Visit(SGlobalFuncDecl* elem)
{
    VisitGlobalFunc(elem, curDecl, nFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SNamespaceDecl* elem)
{
    NNamespaceDecl* curNamespace = curDecl;
    for (size_t i = 0, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        NNamespaceDecl* childNamespace = curNamespace->GetNamespace(name);
        if (!childNamespace)
        {
            childNamespace = nFactory->MakeChildNamespaceDecl(curNamespace, name);
            curNamespace->AddNamespace(childNamespace);
        }

        curNamespace = childNamespace;
    }

    for (auto* nsElem : elem->elements)
    {
        NamespaceElemVisitor visitor{curNamespace, nFactory, phaseManager};
        nsElem->Accept(visitor);
    }
}

void NamespaceElemVisitor::Visit(SClassDecl* elem)
{
    throw NotImplementedException{};
}

void NamespaceElemVisitor::Visit(SStructDecl* elem)
{
    VisitStruct(curDecl, elem, AccessorContext::Global, nFactory, phaseManager);
}

void NamespaceElemVisitor::Visit(SEnumDecl* elem)
{
    VisitEnum(curDecl, elem, AccessorContext::Global, nFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SNamespaceDecl* elem)
{
    // A.B.C가 있을 경우, 하위 네임스페이스를 찾는다. 없으면 만들어 나간다

    // 첫번째는 모듈에서 찾는다
    assert(1 <= elem->names.size());

    auto curNamespace = rootNamespace->GetNamespace(elem->names[0]);
    if (!curNamespace)
    {
        curNamespace = nFactory->MakeChildNamespaceDecl(rootNamespace, elem->names[0]);
        rootNamespace->AddNamespace(curNamespace);
    }

    for (size_t i = 1, size = elem->names.size(); i < size; i++)
    {
        auto& name = elem->names[i];

        auto childNamespace = curNamespace->GetNamespace(name);
        if (!childNamespace)
        {
            childNamespace = nFactory->MakeChildNamespaceDecl(curNamespace, name);
            curNamespace->AddNamespace(childNamespace);
        }

        curNamespace = childNamespace;
    }

    for (auto* nsElem : elem->elements)
    {
        NamespaceElemVisitor visitor{curNamespace, nFactory, phaseManager};
        nsElem->Accept(visitor);
    }
}

void ScriptElemVisitor::Visit(SGlobalFuncDecl* elem)
{
    VisitGlobalFunc(elem, rootNamespace, nFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SClassDecl* elem)
{
    throw NotImplementedException{};
}

void ScriptElemVisitor::Visit(SStructDecl* elem)
{
    VisitStruct(rootNamespace, elem, AccessorContext::Global, nFactory, phaseManager);
}

void ScriptElemVisitor::Visit(SEnumDecl* elem)
{
    VisitEnum(rootNamespace, elem, AccessorContext::Global, nFactory, phaseManager);
}

} // unnamed namespace 

expected<NModuleMData, DiagPtr> TranslateSyntaxToNModuleMData(
    string moduleName,
    const vector<SScript*>& scripts, // translation units
    const vector<EModule*>& referenceModules,
    const LoggerPtr& logger,
    const RFactoryPtr& rFactory,
    const NFactoryPtr& nFactory,
    const MFactoryPtr& mFactory)
{
    // TODO: NewRootNamespaceDecl이 아니라 RootNamespaceGroupDecl이어야 할것 같고, 모듈은 rootNamespaceDeclGroup을 가져야 할 것 같다

    // NNamespaceDecl은 각 TranslationUnit별로 별개로 가지는데,
    auto* nModule = nFactory->MakeNModule(move(moduleName));

    auto srtFactory = MakePtr<SRTFactory>();
    auto binOpQueryService = MakePtr<BinOpQueryService>(*rFactory);
    
    PhaseManager phaseManager{logger, rFactory, nFactory, mFactory, srtFactory, binOpQueryService};
    for (auto* script : scripts) // translation units
    {
        auto* rootNamespace = nFactory->MakeRootNamespaceDecl();

        for (auto* elem : script->elements)
        {
            ScriptElemVisitor visitor{rootNamespace, nFactory, phaseManager};
            elem->Accept(visitor);
        }
    }

    auto eFuncBodies = phaseManager.Run();
    RETURN_ON_ERROR(eFuncBodies);

    auto* mData = mFactory->MakeMData(move(*eFuncBodies));
    return NModuleMData{nModule, mData};

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
