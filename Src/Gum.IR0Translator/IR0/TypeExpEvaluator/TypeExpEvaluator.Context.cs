﻿using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class TypeExpEvaluator
    {
        class Context
        {
            private ModuleInfoService moduleInfoService;
            private SyntaxNodeModuleItemService syntaxNodeModuleItemService;
            private ImmutableDictionary<ModuleItemId, TypeSkeleton> typeSkeletonsByTypeId;
            private IErrorCollector errorCollector;

            private Dictionary<S.TypeExp, TypeValue> typeValuesByTypeExp;
            private ImmutableDictionary<string, TypeValue.TypeVar> typeEnv;

            public Context(
                ModuleInfoService moduleInfoService,
                SyntaxNodeModuleItemService syntaxNodeModuleItemService,
                IEnumerable<TypeSkeleton> typeSkeletons,
                IErrorCollector errorCollector)
            {
                this.moduleInfoService = moduleInfoService;
                this.syntaxNodeModuleItemService = syntaxNodeModuleItemService;
                this.typeSkeletonsByTypeId = typeSkeletons.ToImmutableDictionary(skeleton => skeleton.TypeId);
                this.errorCollector = errorCollector;

                typeValuesByTypeExp = new Dictionary<S.TypeExp, TypeValue>(RefEqComparer<S.TypeExp>.Instance);
                typeEnv = ImmutableDictionary<string, TypeValue.TypeVar>.Empty;
            }            

            public IEnumerable<ITypeInfo> GetTypeInfos(ModuleItemId itemId)
            {
                return moduleInfoService.GetTypeInfos(itemId);
            }

            public ModuleItemId GetTypeId(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetTypeId(node);
            }

            public ModuleItemId GetFuncId(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetFuncId(node);
            }

            public bool GetSkeleton(ModuleItemId itemId, out TypeSkeleton outTypeSkeleton)
            {
                return typeSkeletonsByTypeId.TryGetValue(itemId, out outTypeSkeleton);
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
            {
                errorCollector.Add(new AnalyzeError(code, node, msg));
            }

            public void AddTypeValue(S.TypeExp exp, TypeValue typeValue)
            {
                typeValuesByTypeExp.Add(exp, typeValue);
            }

            public ImmutableDictionary<S.TypeExp, TypeValue> GetTypeValuesByTypeExp()
            {
                return typeValuesByTypeExp.ToImmutableWithComparer();
            }

            public bool GetTypeVar(string name, [NotNullWhen(true)] out TypeValue.TypeVar? typeValue)
            {
                return typeEnv.TryGetValue(name, out typeValue);
            }

            public void ExecInScope(ModuleItemId itemId, IEnumerable<string> typeParams, Action action)
            {
                var prevTypeEnv = typeEnv;

                foreach (var typeParam in typeParams)
                {
                    typeEnv = typeEnv.SetItem(typeParam, TypeValue.MakeTypeVar(itemId, typeParam));
                }

                try
                {
                    action();
                }
                finally
                {
                    typeEnv = prevTypeEnv;
                }
            }
        }

    }
}
