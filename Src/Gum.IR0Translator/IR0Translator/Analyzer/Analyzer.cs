﻿using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using M = Gum.CompileTime;
using Gum.Infra;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using Gum.Log;
using Citron.Analysis;

namespace Gum.IR0Translator
{
    // Root Analyzer
    partial class Analyzer
    {
        static (R.ParamHash ParamHash, ImmutableArray<R.Param> Params) MakeParamHashAndParamInfos(GlobalContext globalContext, int typeParamCount, ImmutableArray<S.FuncParam> parameters)
        {
            var paramWithoutNames = ImmutableArray.CreateBuilder<R.ParamHashEntry>(parameters.Length);
            var parametersBuilder = ImmutableArray.CreateBuilder<R.Param>(parameters.Length);

            foreach (var param in parameters)
            {
                var typeValue = globalContext.GetSymbolByTypeExp(param.Type);

                var type = typeValue.MakeRPath();

                var kind = param.Kind switch
                {
                    S.FuncParamKind.Normal => R.ParamKind.Default,
                    S.FuncParamKind.Params => R.ParamKind.Params,
                    S.FuncParamKind.Ref => R.ParamKind.Ref,
                    _ => throw new UnreachableCodeException()
                };

                paramWithoutNames.Add(new R.ParamHashEntry(kind, type));
                var parameter = new R.Param(kind, type, new R.Name.Normal(param.Name));
                parametersBuilder.Add(parameter);
            }

            var rparamHash = new R.ParamHash(typeParamCount, paramWithoutNames.MoveToImmutable());
            var rparameters = parametersBuilder.MoveToImmutable();

            return (rparamHash, rparameters);
        }

        static ImmutableArray<R.Path> MakeRTypeArgs(int baseTypeParamCount, ImmutableArray<string> typeParams)
        {
            var typeArgsBuilder = ImmutableArray.CreateBuilder<R.Path>(typeParams.Length);
            
            for (int i = 0; i < typeParams.Length; i++)
                typeArgsBuilder.Add(new R.Path.TypeVarType(baseTypeParamCount + i));
            return typeArgsBuilder.MoveToImmutable();
        }

        static void HandleItemQueryResultError(GlobalContext globalContext, SymbolQueryResult.Error error, S.ISyntaxNode nodeForErrorReport)
        {
            switch (error)
            {
                case SymbolQueryResult.Error.MultipleCandidates:
                    globalContext.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport);
                    throw new UnreachableCodeException();

                case SymbolQueryResult.Error.VarWithTypeArg:
                    globalContext.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport);
                    throw new UnreachableCodeException();

                default:
                    throw new UnreachableCodeException();
            }
        }

        public static R.Script? Analyze(
            S.Script sscript,
            M.Name moduleName,
            ImmutableArray<ModuleDeclSymbol> referenceModules,
            ILogger logger)
        {
            try
            {
                var symbolFactory = new SymbolFactory();

                // S.TypeExp => TypeExpInfo
                var typeExpInfoService = TypeExpEvaluator.Evaluate(moduleName, sscript, referenceModules, logger);
                var declSymbolBuilderResult = InternalDeclSymbolBuilder.Build(moduleName, sscript, symbolFactory, typeExpInfoService, referenceModules);
                if (!declSymbolBuilderResult.HasValue) return null;
                var (symbolLoader, typeSymbolInfoService) = declSymbolBuilderResult.Value;

                // InternalModuleBuilder에서는 TypeSymbolInfoService(S.TypeExp => ITypeSymbol)를 리턴한다
                var rmoduleName = RItemFactory.MakeModuleName(moduleName);

                var globalContext = new GlobalContext(symbolLoader, globalItemValueFactory, logger);
                var rootContext = new RootContext(rmoduleName, itemValueFactory);

                return RootAnalyzer.Analyze(globalContext, rootContext, internalModuleInfo, sscript);
            }
            catch(AnalyzerFatalException)
            {
                return null;
            }
        }        
    }
}
