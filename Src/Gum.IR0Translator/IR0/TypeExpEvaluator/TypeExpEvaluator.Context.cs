using Gum.CompileTime;
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
            private ItemInfoRepository itemInfoRepo;
            private SyntaxNodeModuleItemService syntaxNodeModuleItemService;
            private ImmutableDictionary<ItemPath, TypeSkeleton> typeSkeletonsByPath;
            private IErrorCollector errorCollector;

            private Dictionary<S.TypeExp, TypeValue> typeValuesByTypeExp;
            private Dictionary<S.Exp, TypeValue> typeValuesByExp;
            private ImmutableDictionary<string, TypeValue.TypeVar> typeEnv;

            public Context(
                ItemInfoRepository itemInfoRepo,
                SyntaxNodeModuleItemService syntaxNodeModuleItemService,
                IEnumerable<TypeSkeleton> typeSkeletons,
                IErrorCollector errorCollector)
            {
                this.itemInfoRepo = itemInfoRepo;
                this.syntaxNodeModuleItemService = syntaxNodeModuleItemService;
                this.typeSkeletonsByPath = typeSkeletons.ToImmutableDictionary(skeleton => skeleton.Path, ModuleInfoEqualityComparer.Instance);
                this.errorCollector = errorCollector;

                typeValuesByTypeExp = new Dictionary<S.TypeExp, TypeValue>();
                typeValuesByExp = new Dictionary<S.Exp, TypeValue>();
                typeEnv = ImmutableDictionary<string, TypeValue.TypeVar>.Empty;
            }            

            public IEnumerable<Gum.CompileTime.TypeInfo> GetReferenceTypeInfos(ItemPath typePath)
            {
                return itemInfoRepo.GetTypes(typePath);
            }

            public ItemPath GetTypePath(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetTypePath(node);
            }

            public ItemPath GetFuncPath(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetFuncPath(node);
            }            

            public bool GetSkeleton(ItemPath path, out TypeSkeleton outTypeSkeleton)
            {
                return typeSkeletonsByPath.TryGetValue(path, out outTypeSkeleton);
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
                return typeValuesByTypeExp.ToImmutableDictionary();
            }

            public void AddTypeValue(S.Exp exp, TypeValue typeValue)
            {
                typeValuesByExp.Add(exp, typeValue);
            }

            public ImmutableDictionary<S.Exp, TypeValue> GetTypeValuesByExp()
            {
                return typeValuesByExp.ToImmutableDictionary();
            }

            public bool GetTypeVar(string name, [NotNullWhen(true)] out TypeValue.TypeVar? typeValue)
            {
                return typeEnv.TryGetValue(name, out typeValue);
            }

            public void ExecInScope(ItemPath itemPath, IEnumerable<string> typeParams, Action action)
            {
                var prevTypeEnv = typeEnv;

                foreach (var typeParam in typeParams)
                {
                    typeEnv = typeEnv.SetItem(typeParam, new TypeValue.TypeVar(new ItemId(ModuleName.Internal, itemPath), typeParam));
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
