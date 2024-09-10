#include "pch.h"
#include "SyntaxIR0Translator.h"

#include <stdexcept>
#include <memory>

#include <Infra/NotImplementedException.h>

#include <IR0/RModuleDecl.h>
#include <IR0/RNamespaceDecl.h>

#include "EnumTranslation.h"
#include "SkeletonPhaseContext.h"


using namespace std;

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
    shared_ptr<RNamespaceDecl> curDecl;
    SNamespaceDeclElementPtr sharedElem;
    SkeletonPhaseContext& context;

public:
    NamespaceElemVisitor(shared_ptr<RNamespaceDecl> curDecl, SNamespaceDeclElementPtr sharedElem, SkeletonPhaseContext& context)
        : curDecl { std::move(curDecl) }, context { context } {}

    // Inherited via SNamespaceDeclElementVisitor
    void Visit(SGlobalFuncDecl& elem) override
    {
        // 이름을 만드려면 인자에 쓰인 타입이 확정되어야 해서, 다음 페이즈에서 해야 한다
        // TODO:
    }

    void Visit(SNamespaceDecl& elem) override
    {
        shared_ptr<RNamespaceDecl> curNamespace = curDecl;

        for (size_t i = 0, size = elem.names.size(); i < size; i++)
        {
            auto& name = elem.names[i];

            shared_ptr<RNamespaceDecl> childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = make_shared<RNamespaceDecl>(curNamespace, name);
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
    }

    void Visit(SStructDecl& elem) override
    {
    }

    void Visit(SEnumDecl& elem) override
    {
        auto sharedEnumElem = dynamic_pointer_cast<SEnumDecl>(sharedElem);
        assert(sharedEnumElem);

        auto rEnum = MakeEnum(std::move(sharedEnumElem), curDecl, MakeGlobalMemberAccessor, context);
        // curDecl->AddEnum(rEnum);
    }
};

class ScriptElemVisitor : public SScriptElementVisitor
{
    shared_ptr<RModuleDecl> moduleDecl;
    SScriptElementPtr sharedElem;
    SkeletonPhaseContext& context;

public:
    ScriptElemVisitor(shared_ptr<RModuleDecl> moduleDecl, SScriptElementPtr sharedElem, SkeletonPhaseContext& context)
        : moduleDecl(std::move(moduleDecl)), sharedElem(std::move(sharedElem)), context(context)
    {
    }

    void Visit(SNamespaceDecl& elem) override
    {
        // A.B.C가 있을 경우, 하위 네임스페이를 찾는다. 없으면 만들어 나간다

        // 첫번째는 모듈에서 찾는다
        assert(1 <= elem.names.size());

        shared_ptr<RNamespaceDecl> curNamespace = moduleDecl->GetNamespace(elem.names[0]);
        if (!curNamespace)
        {
            curNamespace = make_shared<RNamespaceDecl>(moduleDecl, elem.names[0]);
            moduleDecl->AddNamespace(curNamespace);
        }

        for (size_t i = 1, size = elem.names.size(); i < size; i++)
        {
            auto& name = elem.names[i];

            shared_ptr<RNamespaceDecl> childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = make_shared<RNamespaceDecl>(moduleDecl, name);
                moduleDecl->AddNamespace(childNamespace);
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

        auto rEnum = MakeEnum(std::move(sharedEnumElem), moduleDecl, MakeGlobalMemberAccessor, context);
        moduleDecl->AddType(rEnum);
    }
};

} // unnamed namespace 

std::shared_ptr<RModuleDecl> Translate(
    std::string moduleName,
    vector<SScript> scripts,
    vector<shared_ptr<MModuleDecl>> referenceModules)
{
    auto rModuleDecl = make_shared<RModuleDecl>(moduleName);

    SkeletonPhaseContext context;
    for (auto& script : scripts)
        for (auto& elem : script.elements)
        {
            ScriptElemVisitor visitor(rModuleDecl, elem, context);
            elem->Accept(visitor);
        }

    return nullopt;

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
    //                            throw new UnreachableException();
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
