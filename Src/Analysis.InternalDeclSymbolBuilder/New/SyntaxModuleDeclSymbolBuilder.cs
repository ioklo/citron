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
    public struct SyntaxModuleDeclSymbolBuilder
    {
        BuildingSkeletonPhaseContext context;

        public static ModuleDeclSymbol Build(M.Name moduleName, ImmutableArray<S.Script> scripts, ImmutableArray<ModuleDeclSymbol> refModuleDecls, SymbolFactory factory)
        {
            var moduleDecl = new ModuleDeclSymbol(moduleName);

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

            return moduleDecl;
        }
        
        // enum이 accessor를 적을 일이 있나
        M.Accessor MakeEnumMemberAccessor(S.AccessModifier? accessModifier)
        {
            return accessModifier switch
            {
                null => M.Accessor.Public,
                S.AccessModifier.Public => M.Accessor.Public,
                S.AccessModifier.Private => M.Accessor.Private,
                S.AccessModifier.Protected => throw new NotImplementedException(), // 에러 처리 해야
                _ => throw new UnreachableCodeException()
            };
        }
    }
}