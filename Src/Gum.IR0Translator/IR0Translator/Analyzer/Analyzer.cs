using System;
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
                var typeValue = globalContext.GetTypeValueByTypeExp(param.Type);

                var type = typeValue.GetRPath();

                var kind = param.Kind switch
                {
                    S.FuncParamKind.Normal => R.ParamKind.Normal,
                    S.FuncParamKind.Params => R.ParamKind.Params,
                    S.FuncParamKind.Ref => R.ParamKind.Ref,
                    _ => throw new UnreachableCodeException()
                };

                paramWithoutNames.Add(new R.ParamHashEntry(kind, type));
                var parameter = new R.Param(kind, type, param.Name);
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

        public static R.Script? Analyze(
            S.Script script,
            R.ModuleName moduleName,
            ItemValueFactory itemValueFactory,
            GlobalItemValueFactory globalItemValueFactory,
            TypeExpInfoService typeExpInfoService,
            InternalModuleInfo internalModuleInfo,
            IErrorCollector errorCollector)
        {
            try
            {
                var globalContext = new GlobalContext(itemValueFactory, globalItemValueFactory, typeExpInfoService, errorCollector);
                var rootContext = new RootContext(moduleName, itemValueFactory);

                return RootAnalyzer.Analyze(globalContext, rootContext, internalModuleInfo, script);
            }
            catch(AnalyzerFatalException)
            {
                return null;
            }
        }        
    }
}
