using System;
using System.Diagnostics;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;
using M = Citron.Module;

using static Citron.Infra.Misc;
using System.Collections.Generic;

namespace Citron.Analysis
{
    // Syntax로부터 ModuleDeclSymbol을 만든다
    public struct SyntaxIR0Translator
    {
        public static ModuleDeclSymbol Build(M.Name moduleName, ImmutableArray<S.Script> scripts, ImmutableArray<ModuleDeclSymbol> refModuleDecls, SymbolFactory factory)
        {
            var moduleDecl = new ModuleDeclSymbol(moduleName, bReference: false);

            var skeletonPhaseContext = new BuildingSkeletonPhaseContext(); // refModules를 사용해야 한다
            var topLevelVisitor = new TopLevelVisitor_BuildingSkeletonPhase<ModuleDeclSymbol>(skeletonPhaseContext,  moduleDecl);

            foreach (var script in scripts)
            {
                foreach (var scriptElem in script.Elements)
                {
                    switch (scriptElem)
                    {
                        case S.TypeDeclScriptElement typeDeclElem:
                            topLevelVisitor.VisitTypeDecl(typeDeclElem.TypeDecl);
                            break;

                        case S.GlobalFuncDeclScriptElement globalFuncDeclElem:
                            topLevelVisitor.VisitGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
                            break;

                        case S.NamespaceDeclScriptElement namespaceDeclElem:
                            // Discovering Namespaces
                            topLevelVisitor.VisitNamespaceDecl(namespaceDeclElem.NamespaceDecl);
                            break;

                        case S.StmtScriptElement:
                            // 그냥 통과
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            }

            var modules = new List<ModuleDeclSymbol>(refModuleDecls.Length + 1);
            modules.Add(moduleDecl);
            modules.AddRange(refModuleDecls.AsEnumerable());

            var funcDeclPhaseContext = new BuildingFuncDeclPhaseContext(modules, factory);
            skeletonPhaseContext.DoRegisteredTasks(funcDeclPhaseContext);

            funcDeclPhaseContext.DoRegisteredTasks();

            

            return moduleDecl;
        }
    }
}