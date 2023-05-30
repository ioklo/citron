﻿using System;
using System.Diagnostics;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Infra.Misc;
using System.Collections.Generic;
using Citron.Log;

namespace Citron.Analysis
{
    // Syntax로부터 ModuleDeclSymbol을 만든다
    public struct SyntaxIR0Translator
    {
        public static R.Script? Build(Name moduleName, ImmutableArray<S.Script> scripts, ImmutableArray<ModuleDeclSymbol> refModuleDecls, SymbolFactory factory, ILogger logger)
        {   
            var moduleDecl = new ModuleDeclSymbol(moduleName, bReference: false);

            var skeletonPhaseContext = new BuildingSkeletonPhaseContext(); // refModules를 사용해야 한다
            var topLevelVisitor = new TopLevelVisitor_BuildingSkeletonPhase<ModuleDeclSymbol>(skeletonPhaseContext, moduleDecl);

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

                        default:
                            throw new UnreachableException();
                    }
                }
            }

            var modulesBuilder = ImmutableArray.CreateBuilder<ModuleDeclSymbol>(refModuleDecls.Length + 1);
            modulesBuilder.Add(moduleDecl);
            modulesBuilder.AddRange(refModuleDecls.AsEnumerable());
            var moduleDecls = modulesBuilder.MoveToImmutable();

            // 2. BuildingMemberDeclPhase
            var buildingMemberDeclPhaseContext = new BuildingMemberDeclPhaseContext(moduleDecls, factory);
            skeletonPhaseContext.BuildMemberDecl(buildingMemberDeclPhaseContext);

            // 3. BuildingTrivialConstructorPhase
            buildingMemberDeclPhaseContext.BuildTrivialConstructor();

            // 4. BuildingBodyPhase
            var globalContext = new GlobalContext(factory, moduleDecls, logger);
            var buildingBodyPhaseContext = new BuildingBodyPhaseContext(globalContext);
            if (!buildingMemberDeclPhaseContext.BuildBody(buildingBodyPhaseContext))
                return null;

            var body = globalContext.GetBodies();
            return new R.Script(moduleDecl, body);
        }
    }
}