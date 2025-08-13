#include "SyntaxIR0Translator.h"

#include <stdexcept>
#include <memory>
#include <cassert>

#include "Infra/Unreachable.h"
#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "IR0/NNamespaceDecl.h"
#include "IR0/NStructDecl.h"
#include "IR0/NEnumDecl.h"
#include "IR0/NModule.h"

#include "EnumTranslation.h"
#include "StructTranslation.h"
#include "SkeletonPhaseContext.h"

using namespace std;
using namespace Citron::SyntaxIR0Translator;

namespace Citron {

namespace {

RAccessor MakeGlobalMemberAccessor(std::optional<SAccessModifier> modifier)
{
    if (!modifier) return RAccessor::Private;

    switch(*modifier)
    {
    case SAccessModifier::Public: return RAccessor::Public;
    case SAccessModifier::Private: throw NotImplementedException();
    case SAccessModifier::Protected: throw NotImplementedException();
    }

    unreachable();
}

class NamespaceElemVisitor : public SNamespaceDeclElementVisitor
{   
    NNamespaceDecl* curDecl;
    SNamespaceDeclElementPtr sSharedElem;
    SkeletonPhaseContext& context;

public:
    NamespaceElemVisitor(NNamespaceDecl* curDecl, SNamespaceDeclElementPtr sharedElem, SkeletonPhaseContext& context)
        : curDecl { move(curDecl) }, context { context } {}

    // Inherited via SNamespaceDeclElementVisitor
    void Visit(SGlobalFuncDecl& elem) override
    {
        // 이름을 만드려면 인자에 쓰인 타입이 확정되어야 해서, 다음 페이즈에서 해야 한다
        // TODO:
    }

    void Visit(SNamespaceDecl& elem) override
    {
        NNamespaceDecl* curNamespace = curDecl;
        for (size_t i = 0, size = elem.names.size(); i < size; i++)
        {
            auto& name = elem.names[i];

            NNamespaceDecl* childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = context.MakeChildNamespace(curNamespace, name);
                curNamespace->AddNamespace(childNamespace);
            }

            curNamespace = childNamespace;
        }

        for (auto& nsElem : elem.elements)
        {
            NamespaceElemVisitor visitor(curNamespace, nsElem, context);
            nsElem->Accept(visitor);
        }
    }

    void Visit(SClassDecl& elem) override
    {
        throw NotImplementedException();
    }

    void Visit(SStructDecl& elem) override
    {
        auto sSharedStructDecl = dynamic_pointer_cast<SStructDecl>(sSharedElem);
        assert(sSharedStructDecl);

        auto nStructDecl = MakeStruct(curDecl, sSharedStructDecl, MakeGlobalMemberAccessor, context);
        curDecl->AddType(move(nStructDecl));
    }

    void Visit(SEnumDecl& elem) override
    {
        auto sSharedEnumDecl = dynamic_pointer_cast<SEnumDecl>(sSharedElem);
        assert(sSharedEnumDecl);

        auto nEnum = MakeEnum(curDecl, *sSharedEnumDecl, MakeGlobalMemberAccessor, context);
        curDecl->AddType(move(nEnum));
    }
};

class ScriptElemVisitor : public SScriptElementVisitor
{
    NNamespaceDecl* rootNamespace;
    SScriptElementPtr sharedElem;
    SkeletonPhaseContext& context;

public:
    ScriptElemVisitor(NNamespaceDecl* rootNamespace, const SScriptElementPtr& sharedElem, SkeletonPhaseContext& context)
        : rootNamespace(rootNamespace), sharedElem(sharedElem), context(context)
    {
    }

    void Visit(SNamespaceDecl& elem) override
    {
        // A.B.C가 있을 경우, 하위 네임스페이스를 찾는다. 없으면 만들어 나간다

        // 첫번째는 모듈에서 찾는다
        assert(1 <= elem.names.size());

        auto curNamespace = rootNamespace->GetNamespace(elem.names[0]);
        if (!curNamespace)
        {
            curNamespace = context.MakeChildNamespace(rootNamespace, elem.names[0]);
            rootNamespace->AddNamespace(curNamespace);
        }

        for (size_t i = 1, size = elem.names.size(); i < size; i++)
        {
            auto& name = elem.names[i];

            auto childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = context.MakeChildNamespace(curNamespace, name);
                curNamespace->AddNamespace(childNamespace);
            }

            curNamespace = childNamespace;
        }

        for(auto& nsElem : elem.elements)
        {
            NamespaceElemVisitor visitor(curNamespace, nsElem, context);
            nsElem->Accept(visitor);
        }
    }

    void Visit(SGlobalFuncDecl& elem) override
    {
        // moduleDecl->AddGlobalFunc();
    }

    void Visit(SClassDecl& elem) override
    {
        // moduleDecl->AddClass();
    }

    void Visit(SStructDecl& elem) override
    {
        // moduleDecl->AddStruct();
    }

    void Visit(SEnumDecl& elem) override
    {   
        auto sharedEnumElem = dynamic_pointer_cast<SEnumDecl>(sharedElem);
        assert(sharedEnumElem);

        auto nEnum = MakeEnum(rootNamespace, *sharedEnumElem, MakeGlobalMemberAccessor, context);
        rootNamespace->AddType(move(nEnum));
    }
};

} // unnamed namespace 

expected<shared_ptr<NModule>, DiagPtr> Translate(
    std::string moduleName,
    vector<SScript> scripts,
    vector<shared_ptr<MModule>> referenceModules,
    IR0Factory& factory)
{
    auto rootNamespace = factory.MakeRootNamespace();
    auto nModule = MakePtr<NModule>(move(moduleName), move(rootNamespace));

    SkeletonPhaseContext context(factory);
    for (auto& script : scripts)
        for (auto& elem : script.elements)
        {
            ScriptElemVisitor visitor(nModule->rootNamespace, elem, context);
            elem->Accept(visitor);
        }

    return nModule;

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
