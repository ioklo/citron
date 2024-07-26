#include "pch.h"
#include "SyntaxIR0Translator.h"
#include <Symbol/MModuleDecl.h>
#include <Symbol/MNamespaceDecl.h>

using namespace std;

namespace Citron {

namespace {

class NamespaceElemVisitor : public SNamespaceDeclElementVisitor
{
public:
    std::shared_ptr<MNamespaceDecl> curDecl;

    NamespaceElemVisitor(std::shared_ptr<MNamespaceDecl> curDecl)
        : curDecl{std::move(curDecl)} {}

    // Inherited via SNamespaceDeclElementVisitor
    void Visit(SGlobalFuncDecl& elem) override
    {
        curDecl->AddFunc()
    }

    void Visit(SNamespaceDecl& elem) override
    {
        std::shared_ptr<MNamespaceDecl> curNamespace = curDecl;

        for (size_t i = 0, size = elem.names.size(); i < size; i++)
        {
            auto& name = elem.names[i];

            std::shared_ptr<MNamespaceDecl> childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = make_shared<MNamespaceDecl>(moduleDecl, name);
                moduleDecl->AddNamespace(childNamespace);
            }

            curNamespace = childNamespace;
        }

        for (auto& nsElem : elem.elements)
        {
            NamespaceElemVisitor visitor(curNamespace);
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
    }
};

class ScriptElemVisitor : public SScriptElementVisitor
{
    std::shared_ptr<MModuleDecl> moduleDecl;

public:
    void Visit(SNamespaceDecl& elem) override
    {
        // A.B.C가 있을 경우, 하위 네임스페이를 찾는다. 없으면 만들어 나간다

        // 첫번째는 모듈에서 찾는다
        assert(1 <= elem.names.size());

        std::shared_ptr<MNamespaceDecl> curNamespace = moduleDecl->GetNamespace(elem.names[0]);
        if (!curNamespace)
        {
            curNamespace = make_shared<MNamespaceDecl>(moduleDecl, elem.names[0]);
            moduleDecl->AddNamespace(curNamespace);
        }

        for (size_t i = 1, size = elem.names.size(); i < size; i++)
        {
            auto& name = elem.names[i];

            std::shared_ptr<MNamespaceDecl> childNamespace = curNamespace->GetNamespace(name);
            if (!childNamespace)
            {
                childNamespace = make_shared<MNamespaceDecl>(moduleDecl, name);
                moduleDecl->AddNamespace(childNamespace);
            }

            curNamespace = childNamespace;
        }

        for(auto& nsElem : elem.elements)
        {
            NamespaceElemVisitor visitor(curNamespace);
            nsElem->Accept(visitor);
        }
    }

    void Visit(SGlobalFuncDecl& elem) override
    {
        moduleDecl->AddGlobalFunc();
    }

    void Visit(SClassDecl& elem) override
    {
        moduleDecl->AddClass();
    }

    void Visit(SStructDecl& elem) override
    {
        moduleDecl->AddStruct();
    }

    void Visit(SEnumDecl& elem) override
    {
        moduleDecl->AddEnum();
    }
};

} // unnamed namespace 

SYNTAXIR0TRANSLATOR_API
optional<RProgram> Translate(
    MName moduleName,
    vector<SScript> scripts,
    vector<shared_ptr<MModuleDecl>> referenceModules)
{
    ScriptElemVisitor visitor;
    for (auto& script : scripts)
        for (auto& elem : script.elements)
            elem->Accept(visitor);

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
