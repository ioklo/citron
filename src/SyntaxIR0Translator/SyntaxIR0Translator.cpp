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
    case SAccessModifier::Private: throw NotImplementedException{};
    case SAccessModifier::Protected: throw NotImplementedException{};
    }

    unreachable();
}

class NamespaceElemVisitor : public SNamespaceDeclElementVisitor
{   
    NNamespaceDecl* curDecl;
    SkeletonPhaseContext& context;

public:
    NamespaceElemVisitor(NNamespaceDecl* curDecl, SkeletonPhaseContext& context)
        : curDecl{curDecl}, context{context}
    { }

    // Inherited via SNamespaceDeclElementVisitor
    void Visit(SGlobalFuncDecl* elem) override
    {
        // 이름을 만드려면 인자에 쓰인 타입이 확정되어야 해서, 다음 페이즈에서 해야 한다
        // TODO:
    }

    void Visit(SNamespaceDecl* elem) override
    {
        NNamespaceDecl* curNamespace = curDecl;
        for (size_t i = 0, size = elem->names.size(); i < size; i++)
        {
            auto& name = elem->names[i];

            NNamespaceDecl* childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = context.MakeChildNamespace(curNamespace, name);
                curNamespace->AddNamespace(childNamespace);
            }

            curNamespace = childNamespace;
        }

        for (auto* nsElem : elem->elements)
        {
            NamespaceElemVisitor visitor{curNamespace, context};
            nsElem->Accept(visitor);
        }
    }

    void Visit(SClassDecl* elem) override
    {
        throw NotImplementedException{};
    }

    void Visit(SStructDecl* elem) override
    {
        auto* nStructDecl = MakeStruct(curDecl, elem, MakeGlobalMemberAccessor, context);
        curDecl->AddType(nStructDecl);
    }

    void Visit(SEnumDecl* elem) override
    {
        auto* nEnum = MakeEnum(curDecl, elem, MakeGlobalMemberAccessor, context);
        curDecl->AddType(nEnum);
    }
};

class ScriptElemVisitor : public SScriptElementVisitor
{
    NNamespaceDecl* rootNamespace;
    SkeletonPhaseContext& context;

public:
    ScriptElemVisitor(NNamespaceDecl* rootNamespace, SkeletonPhaseContext& context)
        : rootNamespace{rootNamespace}, context{context}
    {
    }

    void Visit(SNamespaceDecl* elem) override
    {
        // A.B.C가 있을 경우, 하위 네임스페이스를 찾는다. 없으면 만들어 나간다

        // 첫번째는 모듈에서 찾는다
        assert(1 <= elem->names.size());

        auto curNamespace = rootNamespace->GetNamespace(elem->names[0]);
        if (!curNamespace)
        {
            curNamespace = context.MakeChildNamespace(rootNamespace, elem->names[0]);
            rootNamespace->AddNamespace(curNamespace);
        }

        for (size_t i = 1, size = elem->names.size(); i < size; i++)
        {
            auto& name = elem->names[i];

            auto childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = context.MakeChildNamespace(curNamespace, name);
                curNamespace->AddNamespace(childNamespace);
            }

            curNamespace = childNamespace;
        }

        for(auto* nsElem : elem->elements)
        {
            NamespaceElemVisitor visitor{curNamespace, context};
            nsElem->Accept(visitor);
        }
    }

    void Visit(SGlobalFuncDecl* elem) override
    {
        // moduleDecl->AddGlobalFunc();
    }

    void Visit(SClassDecl* elem) override
    {
        // moduleDecl->AddClass();
    }

    void Visit(SStructDecl* elem) override
    {
        // moduleDecl->AddStruct();
    }

    void Visit(SEnumDecl* elem) override
    {   
        auto* nEnum = MakeEnum(rootNamespace, elem, MakeGlobalMemberAccessor, context);
        rootNamespace->AddType(nEnum);
    }
};

} // unnamed namespace 

expected<NModule*, DiagPtr> Translate(
    std::string moduleName,
    const vector<SScript*>& scripts, // translation units
    const vector<MModule*>& referenceModules,
    RFactory& factory)
{
    // TODO: NewRootNamespaceDecl이 아니라 RootNamespaceGroupDecl이어야 할것 같고, 모듈은 rootNamespaceDeclGroup을 가져야 할 것 같다

    // NNamespaceDecl은 각 TranslationUnit별로 별개로 가지는데,
    auto* nModule = factory.MakeNModule(move(moduleName));

    SkeletonPhaseContext context{factory};
    for (auto* script : scripts) // translation units
    {
        auto* rootNamespace = factory.MakeRootNamespaceDecl();

        for (auto* elem : script->elements)
        {
            ScriptElemVisitor visitor{rootNamespace, context};
            elem->Accept(visitor);
        }
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
